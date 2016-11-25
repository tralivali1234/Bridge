﻿using Bridge.Contract;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;

namespace Bridge.Translator
{
    public class SharpSixRewriter : CSharpSyntaxRewriter
    {
        public const string AutoInitFieldPrefix = "__Property__Initializer__";

        private readonly ILogger logger;
        private readonly ITranslator translator;
        private readonly CSharpCompilation compilation;
        private SemanticModel semanticModel;
        private List<MemberDeclarationSyntax> fields;
        private int tempKey = 1;

        public SharpSixRewriter(ITranslator translator)
        {
            this.translator = translator;
            this.logger = translator.Log;
            this.compilation = this.CreateCompilation();
        }

        public string Rewrite(int index)
        {
            var syntaxTree = this.compilation.SyntaxTrees[index];
            this.semanticModel = this.compilation.GetSemanticModel(syntaxTree, true);
            var result = this.Visit(syntaxTree.GetRoot());

            return result.ToFullString();
        }

        private CSharpCompilation CreateCompilation()
        {
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp6, Microsoft.CodeAnalysis.DocumentationMode.None, SourceCodeKind.Regular, translator.DefineConstants);
            var syntaxTrees = translator.SourceFiles.Select(s => ParseSourceFile(s, parseOptions)).Where(s => s != null).ToList();
            var references = new MetadataReference[this.translator.References.Count()];
            var i = 0;
            foreach (var r in this.translator.References)
            {
                references[i++] = MetadataReference.CreateFromFile(r.MainModule.FullyQualifiedName, new MetadataReferenceProperties(MetadataImageKind.Assembly, ImmutableArray.Create("global")));
            }

            return CSharpCompilation.Create(GetAssemblyName(), syntaxTrees, references, compilationOptions);
        }

        private string GetAssemblyName()
        {
            if (this.translator.AssemblyLocation != null)
            {
                return Path.GetFileNameWithoutExtension(this.translator.AssemblyLocation);
            }
            else if (this.translator.SourceFiles.Count > 0)
            {
                return Path.GetFileNameWithoutExtension(this.translator.SourceFiles[0]);
            }
            else
            {
                return null;
            }
        }

        private SyntaxTree ParseSourceFile(string path, CSharpParseOptions options)
        {
            if (!File.Exists(path))
            {
                logger.Error(string.Format("Source file `{0}' could not be found", path));
            }

            try
            {
                using (var rdr = new StreamReader(path))
                {
                    return SyntaxFactory.ParseSyntaxTree(rdr.ReadToEnd(), options, path);
                }
            }
            catch (IOException ex)
            {
                logger.Error(string.Format("Error reading source file `{0}': {1}", path, ex.Message));
                return null;
            }
        }

        private static bool IsExpandedForm(SemanticModel semanticModel, InvocationExpressionSyntax node, IMethodSymbol method)
        {
            var parameters = method.Parameters;
            var arguments = node.ArgumentList.Arguments;

            ExpressionSyntax target = null;
            if (method.ReducedFrom != null)
            {
                var mae = (MemberAccessExpressionSyntax)node.Expression;
                target = mae.Expression;
            }

            var isReducedExtensionMethod = target != null;

            if (parameters.Length == 0 || !parameters[parameters.Length - 1].IsParams)
                return false;   // Last parameter must be params

            int actualArgumentCount = arguments.Count + (isReducedExtensionMethod ? 1 : 0);

            if (actualArgumentCount < parameters.Length - 1)
                return false;   // No default arguments are allowed

            if (arguments.Any(a => a.NameColon != null))
                return false;   // No named arguments are allowed

            if (actualArgumentCount == parameters.Length - 1)
                return true;    // Empty param array

            var lastType = semanticModel.GetTypeInfo(arguments[arguments.Count - 1].Expression).ConvertedType;
            if (Equals(((IArrayTypeSymbol)parameters[parameters.Length - 1].Type).ElementType, lastType))
                return true;    // A param array needs to be created

            return false;
        }

        public override SyntaxNode VisitArgument(ArgumentSyntax node)
        {
            var ti = this.semanticModel.GetTypeInfo(node.Expression);

            ITypeSymbol type = null;
            IMethodSymbol method = null;
            IParameterSymbol parameter = null;

            if (ti.Type != null && ti.Type.TypeKind == TypeKind.Delegate)
            {
                type = ti.Type;
            }
            else if (ti.ConvertedType != null && ti.ConvertedType.TypeKind == TypeKind.Delegate)
            {
                type = ti.ConvertedType;
            }

            if (type != null)
            {
                var list = node.Parent as ArgumentListSyntax;
                var invocation = node.Parent.Parent as InvocationExpressionSyntax;

                if (list != null && invocation != null)
                {
                    method = this.semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

                    if (method != null)
                    {
                        if (node.NameColon != null)
                        {
                            if (node.NameColon.Name != null)
                            {
                                var nameText = node.NameColon.Name.Identifier.ValueText;
                                if (nameText != null)
                                {
                                    foreach (var p in method.Parameters)
                                    {
                                        if (string.Equals(p.Name, nameText, StringComparison.Ordinal))
                                        {
                                            parameter = p;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var index = list.Arguments.IndexOf(node);
                            if (index >= 0)
                            {
                                if (index < method.Parameters.Length)
                                {
                                    parameter = method.Parameters[index];
                                }
                                else if (index >= method.Parameters.Length && method.Parameters[method.Parameters.Length - 1].IsParams)
                                {
                                    parameter = method.Parameters[method.Parameters.Length - 1];
                                }
                            }
                        }
                    }
                }
            }
            var isParam = parameter != null && !SyntaxHelper.IsAnonymous(parameter.Type);
            var parent = isParam && parameter.IsParams ? (InvocationExpressionSyntax)node.Parent.Parent : null;
            node = (ArgumentSyntax)base.VisitArgument(node);

            if (isParam)
            {
                var pType = parameter.Type;
                if (parameter.IsParams && SharpSixRewriter.IsExpandedForm(this.semanticModel, parent, method))
                {
                    pType = ((IArrayTypeSymbol)parameter.Type).ElementType;
                }

                if (node.Expression is CastExpressionSyntax && type.Equals(pType) || parameter.RefKind != RefKind.None)
                {
                    return node;
                }

                if (pType.TypeKind == TypeKind.Delegate || parameter.IsParams && ((IArrayTypeSymbol)parameter.Type).ElementType.TypeKind == TypeKind.Delegate)
                {
                    var name = SyntaxFactory.IdentifierName(pType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).WithoutTrivia();
                    var expr = node.Expression;

                    if (expr is LambdaExpressionSyntax || expr is AnonymousMethodExpressionSyntax || expr is QueryExpressionSyntax)
                    {
                        expr = SyntaxFactory.ParenthesizedExpression(expr);
                    }

                    var cast = SyntaxFactory.CastExpression(name, expr);
                    node = node.WithExpression(cast);
                }
            }

            return node;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var method = this.semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

            node = (InvocationExpressionSyntax)base.VisitInvocationExpression(node);
            if (node.Expression is IdentifierNameSyntax &&
                ((IdentifierNameSyntax)node.Expression).Identifier.Text == "nameof")
            {
                string name = SyntaxHelper.GetSymbolName(node, semanticModel);
                return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(name));
            }
            else
            {
                if (method != null && method.IsGenericMethod && !method.TypeArguments.Any(SyntaxHelper.IsAnonymous))
                {
                    var expr = node.Expression;
                    var ma = expr as MemberAccessExpressionSyntax;

                    if (expr is IdentifierNameSyntax)
                    {
                        var name = (IdentifierNameSyntax)expr;

                        var genericName = SyntaxHelper.GenerateGenericName(name.Identifier, method.TypeArguments);
                        genericName = genericName.WithLeadingTrivia(name.GetLeadingTrivia()).WithTrailingTrivia(name.GetTrailingTrivia());
                        node = node.WithExpression(genericName);
                    }
                    else if (ma != null && ma.Name is IdentifierNameSyntax)
                    {
                        expr = ma.Name;
                        var name = (IdentifierNameSyntax)expr;
                        var genericName = SyntaxHelper.GenerateGenericName(name.Identifier, method.TypeArguments);
                        genericName = genericName.WithLeadingTrivia(name.GetLeadingTrivia()).WithTrailingTrivia(name.GetTrailingTrivia());

                        if (method.MethodKind == MethodKind.ReducedExtension && node.GetParent<ConditionalAccessExpressionSyntax>() == null)
                        {
                            var target = ma.Expression;
                            var clsName = "global::" + method.ContainingType.FullyQualifiedName();
                            ma = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(clsName), genericName);
                            node = node.WithArgumentList(node.ArgumentList.WithArguments(node.ArgumentList.Arguments.Insert(0, SyntaxFactory.Argument(target))));
                        }
                        else
                        {
                            ma = ma.WithName(genericName);
                        }

                        node = node.WithExpression(ma);
                    }
                }
            }

            return node;
        }

        public override SyntaxNode VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            var isInterpolatedString = semanticModel.GetConversion(node).IsInterpolatedString;
            node = (InterpolatedStringExpressionSyntax)base.VisitInterpolatedStringExpression(node);
            string methodNameToCall;
            string classNameToCall;

            if (isInterpolatedString)
            {
                classNameToCall = "System.Runtime.CompilerServices.FormattableStringFactory";
                methodNameToCall = "Create";
            }
            else
            {
                classNameToCall = "string";
                methodNameToCall = "Format";
            }

            string str = "";
            int placeholder = 0;
            var expressions = new List<ExpressionSyntax>();

            foreach (var content in node.Contents)
            {
                var interpolatedStringTextSyntax = content as InterpolatedStringTextSyntax;
                if (interpolatedStringTextSyntax != null)
                {
                    str += interpolatedStringTextSyntax.TextToken.ValueText;
                }
                else if (content is InterpolationSyntax)
                {
                    var interpolation = (InterpolationSyntax)content;

                    str += "{" + placeholder.ToString(CultureInfo.InvariantCulture);

                    if (interpolation.AlignmentClause != null)
                    {
                        var value = semanticModel.GetConstantValue(interpolation.AlignmentClause.Value).Value;

                        if (value == null)
                        {
                            logger.Error("Non-constant alignment");
                            return null;
                        }

                        str += "," + Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
                    }

                    if (interpolation.FormatClause != null)
                    {
                        str += ":" + interpolation.FormatClause.FormatStringToken.Text;
                    }

                    str += "}";
                    placeholder++;
                    expressions.Add(interpolation.Expression);
                }
                else
                {
                    logger.Error("Unknown content in interpolated string: " + content);
                    return null;
                }
            }

            expressions.Insert(0, SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(str)));

            var methodIdentifier = SyntaxFactory.IdentifierName(classNameToCall + "." + methodNameToCall);
            var invocation = SyntaxFactory.InvocationExpression(methodIdentifier, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(expressions.Select(SyntaxFactory.Argument))));

            return invocation;
        }

        public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.StaticKeyword.RawKind == (int)SyntaxKind.StaticKeyword)
            {
                return null;
            }
            return base.VisitUsingDirective(node);
        }

        public override SyntaxNode VisitGenericName(GenericNameSyntax node)
        {
            var symbol = semanticModel.GetSymbolInfo(node).Symbol;

            var parent = node.Parent;
            while (parent != null && !(parent is TypeDeclarationSyntax))
            {
                parent = parent.Parent;
            }

            ITypeSymbol thisType = null;
            if (parent is TypeDeclarationSyntax)
            {
                thisType = this.semanticModel.GetDeclaredSymbol(parent) as ITypeSymbol;
            }

            bool needHandle = !node.IsVar &&
                              symbol is ITypeSymbol &&
                              symbol.ContainingType != null &&
                              thisType != null &&
                              !thisType.InheritsFromOrEquals(symbol.ContainingType) &&
                              !thisType.Equals(symbol);

            var qns = node.Parent as QualifiedNameSyntax;
            if (qns != null && needHandle)
            {
                SyntaxNode n = node;
                do
                {
                    if (!qns.Left.Equals(n))
                    {
                        needHandle = false;
                    }

                    n = qns;
                    qns = qns.Parent as QualifiedNameSyntax;
                } while (qns != null && needHandle);
            }

            node = (GenericNameSyntax)base.VisitGenericName(node);

            if (needHandle && !(node.Parent is MemberAccessExpressionSyntax))
            {
                INamedTypeSymbol namedType = symbol as INamedTypeSymbol;
                if (namedType != null && namedType.IsGenericType && namedType.TypeArguments.Length > 0 && !namedType.TypeArguments.Any(SyntaxHelper.IsAnonymous))
                {
                    return SyntaxHelper.GenerateGenericName(SyntaxFactory.Identifier(node.GetLeadingTrivia(), symbol.FullyQualifiedName(false), node.GetTrailingTrivia()), namedType.TypeArguments);
                }

                return SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(node.GetLeadingTrivia(), symbol.FullyQualifiedName(), node.GetTrailingTrivia()));
            }

            IMethodSymbol methodSymbol = null;

            if (symbol != null && symbol.IsStatic && symbol.ContainingType != null
                && thisType != null && !thisType.InheritsFromOrEquals(symbol.ContainingType)
                && !(node.Parent is MemberAccessExpressionSyntax)
                && (
                    (methodSymbol = symbol as IMethodSymbol) != null
                    || symbol is IPropertySymbol
                    || symbol is IFieldSymbol
                    || symbol is IEventSymbol)
                )
            {
                if (methodSymbol != null && methodSymbol.IsGenericMethod && methodSymbol.TypeArguments.Length > 0 && !methodSymbol.TypeArguments.Any(SyntaxHelper.IsAnonymous))
                {
                    return SyntaxHelper.GenerateGenericName(SyntaxFactory.Identifier(node.GetLeadingTrivia(), symbol.FullyQualifiedName(false), node.GetTrailingTrivia()), methodSymbol.TypeArguments);
                }

                return SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(node.GetLeadingTrivia(), symbol.FullyQualifiedName(), node.GetTrailingTrivia()));
            }

            return node;
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbol = semanticModel.GetSymbolInfo(node).Symbol;

            var parent = node.Parent;
            while (parent != null && !(parent is TypeDeclarationSyntax))
            {
                parent = parent.Parent;
            }

            ITypeSymbol thisType = null;
            if (parent is TypeDeclarationSyntax)
            {
                thisType = this.semanticModel.GetDeclaredSymbol(parent) as ITypeSymbol;
            }

            bool needHandle = !node.IsVar &&
                              symbol is ITypeSymbol &&
                              symbol.ContainingType != null &&
                              thisType != null &&
                              !thisType.InheritsFromOrEquals(symbol.ContainingType) &&
                              !thisType.Equals(symbol);

            var qns = node.Parent as QualifiedNameSyntax;
            if (qns != null && needHandle)
            {
                SyntaxNode n = node;
                do
                {
                    if (!qns.Left.Equals(n))
                    {
                        needHandle = false;
                    }

                    n = qns;
                    qns = qns.Parent as QualifiedNameSyntax;
                } while (qns != null && needHandle);
            }

            node = (IdentifierNameSyntax)base.VisitIdentifierName(node);

            if (needHandle && !(node.Parent is MemberAccessExpressionSyntax))
            {
                INamedTypeSymbol namedType = symbol as INamedTypeSymbol;
                if (namedType != null && namedType.IsGenericType && namedType.TypeArguments.Length > 0 && !namedType.TypeArguments.Any(SyntaxHelper.IsAnonymous))
                {
                    var genericName = SyntaxHelper.GenerateGenericName(node.Identifier, namedType.TypeArguments);
                    return genericName.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
                }

                return SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(node.GetLeadingTrivia(), symbol.FullyQualifiedName(), node.GetTrailingTrivia()));
            }

            IMethodSymbol methodSymbol = null;

            if (symbol != null && symbol.IsStatic && symbol.ContainingType != null
                && thisType != null && !thisType.InheritsFromOrEquals(symbol.ContainingType)
                && !(node.Parent is MemberAccessExpressionSyntax)
                && (
                    (methodSymbol = symbol as IMethodSymbol) != null
                    || symbol is IPropertySymbol
                    || symbol is IFieldSymbol
                    || symbol is IEventSymbol)
                )
            {
                if (methodSymbol != null && methodSymbol.IsGenericMethod && methodSymbol.TypeArguments.Length > 0 && !methodSymbol.TypeArguments.Any(SyntaxHelper.IsAnonymous))
                {
                    var genericName = SyntaxHelper.GenerateGenericName(node.Identifier, methodSymbol.TypeArguments);
                    return genericName.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
                }

                return SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(node.GetLeadingTrivia(), symbol.FullyQualifiedName(), node.GetTrailingTrivia()));
            }

            return node;
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var symbol = semanticModel.GetSymbolInfo(node.Expression).Symbol;
            var symbolNode = semanticModel.GetSymbolInfo(node).Symbol;

            var parent = node.Parent;
            while (parent != null && !(parent is TypeDeclarationSyntax))
            {
                parent = parent.Parent;
            }

            ITypeSymbol thisType = null;
            if (parent is TypeDeclarationSyntax)
            {
                thisType = this.semanticModel.GetDeclaredSymbol(parent) as ITypeSymbol;
            }

            node = (MemberAccessExpressionSyntax)base.VisitMemberAccessExpression(node);

            if (node.Expression is IdentifierNameSyntax && symbol != null && symbol.IsStatic && symbol.ContainingType != null && thisType != null && !thisType.InheritsFromOrEquals(symbol.ContainingType) && (symbol is IMethodSymbol || symbol is IPropertySymbol || symbol is IFieldSymbol || symbol is IEventSymbol))
            {
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(node.GetLeadingTrivia(), symbol.FullyQualifiedName(), node.GetTrailingTrivia())), node.OperatorToken, node.Name);
            }

            var usingType = symbol as INamedTypeSymbol;
            if (node.Expression is IdentifierNameSyntax && symbol != null && symbolNode != null && usingType != null && symbolNode.IsStatic && symbol.ContainingType != null && thisType != null && !thisType.InheritsFromOrEquals(usingType) && !usingType.IsAccessibleIn(thisType) && (symbolNode is IMethodSymbol || symbolNode is IPropertySymbol || symbolNode is IFieldSymbol || symbolNode is IEventSymbol))
            {
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(node.GetLeadingTrivia(), symbol.FullyQualifiedName(), node.GetTrailingTrivia())), node.OperatorToken, node.Name);
            }

            return node;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            node = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node);
            var newNode = node;

            if (node.ExpressionBody != null)
            {
                newNode = SyntaxHelper.ToStatementBody(node);
            }

            if (node.IsAutoProperty() && node.AccessorList != null)
            {
                var setter = node.AccessorList.Accessors.SingleOrDefault(a => a.Keyword.Kind() == SyntaxKind.SetKeyword);

                if (setter == null)
                {
                    var getter = node.AccessorList.Accessors.Single(a => a.Keyword.Kind() == SyntaxKind.GetKeyword);
                    setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PrivateKeyword).WithTrailingTrivia(SyntaxFactory.Space)))
                            .WithBody(null)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            .WithLeadingTrivia(getter.GetLeadingTrivia())
                            .WithTrailingTrivia(getter.GetTrailingTrivia());

                    newNode = newNode.AddAccessorListAccessors(setter);
                }

                if (newNode.Initializer != null)
                {
                    var modifiers = SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PrivateKeyword).WithTrailingTrivia(SyntaxFactory.Space));

                    if (node.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword))
                    {
                        modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword).WithTrailingTrivia(SyntaxFactory.Space));
                    }

                    var field = SyntaxFactory.FieldDeclaration(SyntaxFactory.List<AttributeListSyntax>(),
                        modifiers,
                        SyntaxFactory.VariableDeclaration(
                            node.Type,
                            SyntaxFactory.SeparatedList(new[] {
                                SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier(AutoInitFieldPrefix + node.Identifier.Text),
                                    null,
                                    newNode.Initializer
                                )
                            })
                        ),
                        SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                    );
                    field = field.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
                    fields.Add(field);
                    newNode = newNode.ReplaceNode(newNode.Initializer, (SyntaxNode)null);
                    newNode = SyntaxHelper.RemoveSemicolon(newNode, newNode.SemicolonToken, t => newNode.WithSemicolonToken(t));
                }

                return newNode;
            }

            return newNode.Equals(node) ? node : newNode;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            this.fields = new List<MemberDeclarationSyntax>();

            var c = base.VisitClassDeclaration(node) as ClassDeclarationSyntax;

            if (c != null && fields.Count > 0)
            {
                c = c.AddMembers(this.fields.ToArray());
            }

            return c;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);
            if (node.ExpressionBody != null)
            {
                return SyntaxHelper.ToStatementBody(node);
            }

            return node;
        }

        public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            node = (OperatorDeclarationSyntax)base.VisitOperatorDeclaration(node);
            if (node.ExpressionBody != null)
            {
                return SyntaxHelper.ToStatementBody(node);
            }

            return node;
        }

        public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
        {
            node = (ConversionOperatorDeclarationSyntax)base.VisitConversionOperatorDeclaration(node);
            if (node.ExpressionBody != null)
            {
                return SyntaxHelper.ToStatementBody(node);
            }

            return node;
        }

        public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            node = (IndexerDeclarationSyntax)base.VisitIndexerDeclaration(node);
            if (node.ExpressionBody != null)
            {
                return SyntaxHelper.ToStatementBody(node);
            }

            return node;
        }

        public override SyntaxNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            var ti = this.semanticModel.GetTypeInfo(node);
            var oldValue = this.IsExpressionOfT;

            if (ti.Type != null && ti.Type.IsExpressionOfT() ||
                ti.ConvertedType != null && ti.ConvertedType.IsExpressionOfT())
            {
                this.IsExpressionOfT = true;
            }

            var newNode = base.VisitParenthesizedLambdaExpression(node);

            this.IsExpressionOfT = oldValue;

            return newNode;
        }

        public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            var ti = this.semanticModel.GetTypeInfo(node);
            var oldValue = this.IsExpressionOfT;

            if (ti.Type != null && ti.Type.IsExpressionOfT() ||
                ti.ConvertedType != null && ti.ConvertedType.IsExpressionOfT())
            {
                this.IsExpressionOfT = true;
            }

            var newNode = base.VisitSimpleLambdaExpression(node);

            this.IsExpressionOfT = oldValue;

            return newNode;
        }

        public bool IsExpressionOfT
        {
            get; set;
        }
        private int indexInstance;

        private class InitializerInfo
        {
            public IMethodSymbol method;
            public List<InitializerInfo> nested;
        }

        private bool NeedRewriteInitializer(InitializerExpressionSyntax initializer, List<InitializerInfo> infos, ref bool extensionMethodExists, ref bool isImplicitElementAccessSyntax)
        {
            bool need = false;
            foreach (var init in initializer.Expressions)
            {
                var info = new InitializerInfo();
                infos.Add(info);
                var ae = init as AssignmentExpressionSyntax;
                if (ae?.Right is InitializerExpressionSyntax)
                {
                    info.nested = new List<InitializerInfo>();
                    if (NeedRewriteInitializer((InitializerExpressionSyntax)ae.Right, info.nested, ref extensionMethodExists, ref isImplicitElementAccessSyntax))
                    {
                        need = true;
                    }
                }
                else
                {
                    var collectionInitializer = this.semanticModel.GetCollectionInitializerSymbolInfo(init).Symbol;
                    var mInfo = collectionInitializer != null ? collectionInitializer as IMethodSymbol : null;
                    if (mInfo != null)
                    {
                        info.method = mInfo;
                        if (mInfo.IsExtensionMethod)
                        {
                            extensionMethodExists = true;
                        }
                        need = true;
                    }

                    if (init.Kind() == SyntaxKind.SimpleAssignmentExpression)
                    {
                        var be = (AssignmentExpressionSyntax)init;
                        if (be.Left is ImplicitElementAccessSyntax)
                        {
                            isImplicitElementAccessSyntax = true;
                            need = true;
                        }
                    }
                }
            }

            return need;
        }

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            bool needRewrite = false;
            List<InitializerInfo> initializerInfos = null;
            bool extensionMethodExists = false;
            bool isImplicitElementAccessSyntax = false;

            if (node.Initializer != null)
            {
                initializerInfos = new List<InitializerInfo>();
                needRewrite = NeedRewriteInitializer(node.Initializer, initializerInfos, ref extensionMethodExists, ref isImplicitElementAccessSyntax);
            }

            node = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node);
            if (needRewrite)
            {
                if (this.IsExpressionOfT)
                {
                    if (isImplicitElementAccessSyntax)
                    {
                        var mapped = this.semanticModel.SyntaxTree.GetLineSpan(node.Span);
                        throw new Exception(string.Format(CultureInfo.InvariantCulture, "{2} - {3}({0},{1}): {4}", mapped.StartLinePosition.Line + 1, mapped.StartLinePosition.Character + 1, "Index collection initializer is not supported inside Expression<T>", this.semanticModel.SyntaxTree.FilePath, node.ToString()));
                    }

                    if (extensionMethodExists)
                    {
                        var mapped = this.semanticModel.SyntaxTree.GetLineSpan(node.Span);
                        throw new Exception(string.Format(CultureInfo.InvariantCulture, "{2} - {3}({0},{1}): {4}", mapped.StartLinePosition.Line + 1, mapped.StartLinePosition.Character + 1, "Extension method for collection initializer is not supported inside Expression<T>", this.semanticModel.SyntaxTree.FilePath, node.ToString()));
                    }

                    return node;
                }

                var initializers = node.Initializer.Expressions;
                ExpressionSyntax[] args = new ExpressionSyntax[2];
                var target = node.WithInitializer(null).WithoutTrivia();

                if (target.ArgumentList == null)
                {
                    target = target.WithArgumentList(SyntaxFactory.ArgumentList());
                }

                args[0] = target;

                List<StatementSyntax> statements = new List<StatementSyntax>();

                var parent = node.Parent;

                while (parent != null && !(parent is MethodDeclarationSyntax) && !(parent is ClassDeclarationSyntax))
                {
                    parent = parent.Parent;
                }

                string instance = "_o" + ++indexInstance;
                if (parent != null)
                {
                    var info = LocalUsageGatherer.GatherInfo(this.semanticModel, parent);
                    while (info.DirectlyOrIndirectlyUsedLocals.Any(s => s.Name == instance))
                    {
                        instance = "_o" + ++indexInstance;
                    }
                }

                SharpSixRewriter.ConvertInitializers(initializers, instance, statements, initializerInfos);

                statements.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(instance).WithLeadingTrivia(SyntaxFactory.Space)));

                var body = SyntaxFactory.Block(statements);
                var lambda = SyntaxFactory.ParenthesizedLambdaExpression(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Parameter(SyntaxFactory.Identifier(instance)) })), body);
                args[1] = lambda;

                var methodIdentifier = SyntaxFactory.IdentifierName("Bridge.Script.CallFor");
                var invocation = SyntaxFactory.InvocationExpression(methodIdentifier, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(args.Select(SyntaxFactory.Argument))));

                return invocation;
            }

            return node;
        }

        private static void ConvertInitializers(SeparatedSyntaxList<ExpressionSyntax> initializers, string instance, List<StatementSyntax> statements, List<InitializerInfo> infos)
        {
            var idx = 0;
            foreach (var init in initializers)
            {
                var info = infos[idx++];
                var mInfo = info != null && info.method != null ? info.method : null;
                if (mInfo != null)
                {
                    if (mInfo.IsStatic)
                    {
                        var ie = SyntaxHelper.GenerateStaticMethodCall(mInfo.Name,
                            mInfo.ContainingType.FullyQualifiedName(),
                            new[]
                            {
                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName(instance)),
                                SyntaxFactory.Argument(init.WithoutTrivia())
                            }, mInfo.TypeArguments.ToArray());
                        statements.Add(ie);
                    }
                    else
                    {
                        ArgumentSyntax[] arguments = null;
                        if (init.Kind() == SyntaxKind.ComplexElementInitializerExpression)
                        {
                            var complexInit = (InitializerExpressionSyntax)init;

                            arguments = new ArgumentSyntax[complexInit.Expressions.Count];
                            for (int i = 0; i < complexInit.Expressions.Count; i++)
                            {
                                arguments[i] = SyntaxFactory.Argument(complexInit.Expressions[i].WithoutTrivia());
                            }
                        }
                        else
                        {
                            arguments = new[]
                            {
                                SyntaxFactory.Argument(init.WithoutTrivia())
                            };
                        }

                        var ie = SyntaxHelper.GenerateMethodCall(mInfo.Name, instance, arguments, mInfo.TypeArguments.ToArray());
                        statements.Add(ie);
                    }
                }
                else
                {
                    var be = (AssignmentExpressionSyntax)init;

                    if (be.Right is InitializerExpressionSyntax)
                    {
                        string name = null;
                        if (be.Left is IdentifierNameSyntax)
                        {
                            var identifier = (IdentifierNameSyntax)be.Left;
                            name = instance + "." + identifier.Identifier.ValueText;
                        }
                        else if (be.Left is ImplicitElementAccessSyntax)
                        {
                            name = SyntaxFactory.ElementAccessExpression(SyntaxFactory.IdentifierName(instance),
                                    ((ImplicitElementAccessSyntax)be.Left).ArgumentList.WithoutTrivia()).ToString();
                        }
                        else
                        {
                            name = instance;
                        }

                        SharpSixRewriter.ConvertInitializers(((InitializerExpressionSyntax)be.Right).Expressions, name, statements, info.nested);
                    }
                    else
                    {
                        var indexerKeys = be.Left as ImplicitElementAccessSyntax;

                        if (indexerKeys != null)
                        {
                            be = be.WithLeft(SyntaxFactory.ElementAccessExpression(SyntaxFactory.IdentifierName(instance),
                                    indexerKeys.ArgumentList.WithoutTrivia()));
                        }
                        else
                        {
                            var identifier = (IdentifierNameSyntax)be.Left;
                            be =
                                be.WithLeft(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName(instance),
                                    SyntaxFactory.IdentifierName(identifier.Identifier.ValueText)));
                        }

                        be = be.WithRight(be.Right.WithoutTrivia());
                        be = be.WithoutTrivia();

                        statements.Add(SyntaxFactory.ExpressionStatement(be, SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                    }
                }
            }
        }

        public override SyntaxNode VisitTryStatement(TryStatementSyntax node)
        {
            node = (TryStatementSyntax)base.VisitTryStatement(node);

            List<CatchClauseSyntax> catches = new List<CatchClauseSyntax>();
            var replace = false;
            foreach (var catchItem in node.Catches)
            {
                var newCatch = catchItem;

                if (catchItem.Filter != null)
                {
                    var filter = catchItem.Filter;

                    var ifStatement = SyntaxFactory.IfStatement(filter.FilterExpression, catchItem.Block.WithoutTrivia(), SyntaxFactory.ElseClause(SyntaxFactory.ThrowStatement().WithLeadingTrivia(SyntaxFactory.Space)));
                    newCatch = catchItem.WithBlock(SyntaxFactory.Block(ifStatement)).WithFilter(null);
                    replace = true;
                }

                catches.Add(newCatch);
            }

            return replace ? node.WithCatches(SyntaxFactory.List(catches)) : node;
        }

        private class ConditionalAccessInfo
        {
            public ConditionalAccessInfo(SemanticModel semanticModel, ConditionalAccessExpressionSyntax node)
            {
                Node = node;

                var expressionType = semanticModel.GetTypeInfo(node.Expression).Type;
                ExpressionType = SyntaxFactory.ParseTypeName(expressionType.ToMinimalDisplayString(semanticModel, node.Expression.GetLocation().SourceSpan.Start));
                this.IsNullable = expressionType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

                if (this.IsNullable)
                {
                    UnderlyingNullableType = ((INamedTypeSymbol)expressionType).TypeArguments[0];
                    ExpressionType = SyntaxFactory.ParseTypeName(UnderlyingNullableType.ToMinimalDisplayString(semanticModel, node.Expression.GetLocation().SourceSpan.Start));
                }

                var resultType = semanticModel.GetTypeInfo(node).Type;
                ResultType = SyntaxFactory.ParseTypeName(resultType.ToMinimalDisplayString(semanticModel, node.GetLocation().SourceSpan.Start));

                IsResultVoid = resultType.SpecialType == SpecialType.System_Void;
                IsComplex = IsExpressionComplexEnoughToGetATemporaryVariable.IsComplex(semanticModel, node.Expression);
            }

            public ConditionalAccessExpressionSyntax Node;
            public TypeSyntax ResultType;
            public TypeSyntax ExpressionType;
            public bool IsResultVoid;
            public bool IsComplex;
            public bool IsNullable;
            public ITypeSymbol UnderlyingNullableType;
        }

        public override SyntaxNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            if (node.Parent is ConditionalAccessExpressionSyntax)
            {
                return base.VisitConditionalAccessExpression(node);
            }

            var infos = new List<ConditionalAccessInfo> { new ConditionalAccessInfo(semanticModel, node) };

            var conditionNode = node.WhenNotNull as ConditionalAccessExpressionSyntax;
            while (conditionNode != null)
            {
                infos.Add(new ConditionalAccessInfo(semanticModel, conditionNode));
                conditionNode = conditionNode.WhenNotNull as ConditionalAccessExpressionSyntax;
            }

            node = (ConditionalAccessExpressionSyntax)base.VisitConditionalAccessExpression(node);
            var idx = 0;
            infos[idx++].Node = node;
            conditionNode = node.WhenNotNull as ConditionalAccessExpressionSyntax;
            while (conditionNode != null)
            {
                infos[idx++].Node = conditionNode;
                conditionNode = conditionNode.WhenNotNull as ConditionalAccessExpressionSyntax;
            }

            ExpressionSyntax parentTarget = null;
            List<BinaryExpressionSyntax> conditions = new List<BinaryExpressionSyntax>();
            foreach (var info in infos)
            {
                ExpressionSyntax leftForCondition;
                if (info.IsComplex)
                {
                    var key = tempKey++;
                    var keyArg = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("key" + key));
                    var methodIdentifier = SyntaxFactory.IdentifierName("Bridge.Script.ToTemp");
                    var arg = parentTarget != null
                        ? SyntaxFactory.ParseExpression(parentTarget.ToString() + info.Node.Expression.WithoutTrivia().ToString())
                        : info.Node.Expression.WithoutTrivia();

                    leftForCondition = SyntaxFactory.InvocationExpression(methodIdentifier,
                        SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(keyArg), SyntaxFactory.Argument(arg) })));

                    var parentMethodIdentifier = SyntaxFactory.GenericName(SyntaxFactory.Identifier("Bridge.Script.FromTemp"),
                                                                 SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(new[] { info.ExpressionType })));
                    var invocation = SyntaxFactory.InvocationExpression(parentMethodIdentifier,
                        SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(keyArg) })));

                    parentTarget = SyntaxFactory.ParseExpression(invocation.ToString());
                }
                else
                {
                    leftForCondition = SyntaxFactory.ParseExpression(parentTarget != null ? (parentTarget.ToString() + info.Node.Expression.WithoutTrivia().ToString() + (info.IsNullable ? ".Value" : "")) : (info.Node.Expression.WithoutTrivia().ToString() + (info.IsNullable ? ".Value" : "")));
                    parentTarget = leftForCondition;
                }

                conditions.Add(SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, leftForCondition, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)));
            }

            BinaryExpressionSyntax condition = null;

            if (conditions.Count == 1)
            {
                condition = conditions[0];
            }
            else
            {
                condition = conditions[0];

                for (int i = 1; i < conditions.Count; i++)
                {
                    condition = SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, condition, conditions[i]);
                }
            }

            ConditionalAccessInfo lastInfo = infos.Last();
            ExpressionSyntax whenTrue = SyntaxFactory.ParseExpression(parentTarget.ToString() + lastInfo.Node.WhenNotNull.WithoutTrivia().ToString());

            if (lastInfo.IsResultVoid && lastInfo.Node.WhenNotNull is InvocationExpressionSyntax)
            {
                var methodIdentifier = SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("Bridge.Script.FromLambda"));
                var invocation = SyntaxFactory.InvocationExpression(methodIdentifier,
                    SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression(whenTrue)) })));
                whenTrue = invocation;
            }

            ExpressionSyntax whenFalse = lastInfo.IsResultVoid ? (ExpressionSyntax)SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression) : SyntaxFactory.CastExpression(lastInfo.ResultType, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));

            return SyntaxFactory.ConditionalExpression(condition, whenTrue, whenFalse);
        }
    }
}