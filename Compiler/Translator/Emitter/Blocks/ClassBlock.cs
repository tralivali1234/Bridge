using Bridge.Contract;
using Bridge.Contract.Constants;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Object.Net.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bridge.Translator
{
    public class ClassBlock : AbstractEmitterBlock
    {
        public ClassBlock(IEmitter emitter, ITypeInfo typeInfo)
            : base(emitter, typeInfo.TypeDeclaration)
        {
            this.TypeInfo = typeInfo;
        }

        public ITypeInfo TypeInfo
        {
            get;
            set;
        }

        public bool IsGeneric
        {
            get;
            set;
        }

        public int StartPosition
        {
            get;
            set;
        }

        public bool HasEntryPoint
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            XmlToJsDoc.EmitComment(this, this.Emitter.Translator.EmitNode);
            string globalTarget = BridgeTypes.GetGlobalTarget(this.TypeInfo.Type.GetDefinition(), this.TypeInfo.TypeDeclaration);

            if (globalTarget != null)
            {
                this.CheckGlobalClass();
                this.Emitter.NamedFunctions = new Dictionary<string, string>();
                this.WriteTopInitMethods();

                this.Write(JS.Types.Bridge.APPLY);
                this.WriteOpenParentheses();
                this.Write(globalTarget);
                this.Write(", ");
                this.BeginBlock();

                new MethodBlock(this.Emitter, this.TypeInfo, true).Emit();

                this.WriteNewLine();
                this.EndBlock();
                this.WriteCloseParentheses();
                this.WriteSemiColon();

                this.EmitAnonymousTypes();
                this.EmitNamedFunctions();

                this.WriteAfterInitMethods();

                this.WriteNewLine();
            }
            else
            {
                this.EmitClassHeader();

                this.Emitter.NamedFunctions = new Dictionary<string, string>();

                if (this.TypeInfo.TypeDeclaration.ClassType != ClassType.Interface)
                {
                    this.EmitStaticBlock();
                    this.EmitInstantiableBlock();
                }

                this.EmitClassEnd();
            }
        }

        protected void CheckGlobalClass()
        {
            var type = this.TypeInfo.Type.GetDefinition();
            if (!type.IsStatic)
            {
                throw new EmitterException(this.TypeInfo.TypeDeclaration, string.Format("The type {0} must be static in order to be decorated with a [MixinAttribute] or [GlobalMethodsAttribute]", this.TypeInfo.Type.FullName));
            }

            if (type.TypeParameters.Count > 0)
            {
                throw new EmitterException(this.TypeInfo.TypeDeclaration, string.Format("[MixinAttribute] or [GlobalMethodsAttribute] cannot be applied to the generic type {0}.", this.TypeInfo.Type.FullName));
            }

            if (type.Members.Any(m => !m.IsStatic && m.SymbolKind != SymbolKind.Method))
            {
                throw new EmitterException(this.TypeInfo.TypeDeclaration, string.Format("The type {0} can contain only methods in order to be decorated with a [MixinAttribute] or [GlobalMethodsAttribute]", this.TypeInfo.Type.FullName));
            }
        }

        protected virtual void EmitClassHeader()
        {
            this.WriteTopInitMethods();

            var typeDef = this.Emitter.GetTypeDefinition();
            string name = this.Emitter.Validator.GetCustomTypeName(typeDef, this.Emitter);
            this.IsGeneric = typeDef.GenericParameters.Count > 0 && !Helpers.IsIgnoreGeneric(this.TypeInfo.Type, this.Emitter);

            if (name.IsEmpty())
            {
                name = BridgeTypes.DefinitionToJsName(this.TypeInfo.Type, this.Emitter);
            }

            if (typeDef.IsInterface && typeDef.HasGenericParameters)
            {
                this.Write(JS.Funcs.BRIDGE_DEFINEI);
            }
            else
            {
                this.Write(JS.Funcs.BRIDGE_DEFINE);
            }

            this.WriteOpenParentheses();

            this.WriteScript(name);
            this.StartPosition = this.Emitter.Output.Length;
            this.Write(", ");

            if (this.IsGeneric)
            {
                this.WriteFunction();
                this.WriteOpenParentheses();

                foreach (var p in typeDef.GenericParameters)
                {
                    this.EnsureComma(false);
                    this.Write(p.Name);
                    this.Emitter.Comma = true;
                }
                this.Emitter.Comma = false;
                this.WriteCloseParentheses();

                this.Write(" { return ");
            }

            this.BeginBlock();

            string extend = this.Emitter.GetTypeHierarchy();

            if (extend.IsNotEmpty() && !this.TypeInfo.IsEnum)
            {
                var bridgeType = this.Emitter.BridgeTypes.Get(this.Emitter.TypeInfo);

                if (this.TypeInfo.InstanceMethods.Any(m => m.Value.Any(subm => this.Emitter.GetEntityName(subm) == JS.Fields.INHERITS)) ||
                    this.TypeInfo.InstanceConfig.Fields.Any(m => m.GetName(this.Emitter) == JS.Fields.INHERITS))
                {
                    this.Write(JS.Vars.D);
                }

                this.Write(JS.Fields.INHERITS);
                this.WriteColon();
                if (Helpers.IsTypeArgInSubclass(bridgeType.TypeDefinition, bridgeType.TypeDefinition, this.Emitter, false))
                {
                    this.WriteFunction();
                    this.WriteOpenCloseParentheses(true);
                    this.WriteOpenBrace(true);
                    this.WriteReturn(true);
                    this.Write(extend);
                    this.WriteSemiColon();
                    this.WriteCloseBrace(true);
                }
                else
                {
                    this.Write(extend);
                }

                this.Emitter.Comma = true;
            }

            this.WriteKind();
            this.WriteObjectLiteral();

            if (this.TypeInfo.Module != null)
            {
                this.WriteScope();
            }

            this.WriteVariance();
        }

        private void WriteTopInitMethods()
        {
            var beforeDefineMethods = this.GetBeforeDefineMethods();

            if (beforeDefineMethods.Any())
            {
                foreach (var method in beforeDefineMethods)
                {
                    if (!this.Emitter.IsNewLine)
                    {
                        this.WriteNewLine();
                    }

                    this.Write(method);
                }

                this.WriteNewLine();
                this.WriteNewLine();
            }

            var topDefineMethods = this.GetTopDefineMethods();

            if (topDefineMethods.Any())
            {
                foreach (var method in topDefineMethods)
                {
                    this.Emitter.EmitterOutput.TopOutput.Append(method);
                }
            }
        }

        protected virtual void WriteVariance()
        {
            var itypeDef = this.TypeInfo.Type.GetDefinition();
            if (itypeDef.Kind == TypeKind.Interface && MetadataUtils.IsJsGeneric(itypeDef, this.Emitter) &&
                itypeDef.TypeParameters != null &&
                itypeDef.TypeParameters.Any(typeParameter => typeParameter.Variance != VarianceModifier.Invariant))
            {
                this.EnsureComma();
                this.Write(JS.Fields.VARIANCE);
                this.WriteColon();
                this.WriteScript(
                    itypeDef.TypeParameters.Select(typeParameter => ClassBlock.ConvertVarianceToInt(typeParameter.Variance))
                        .ToArray());
                this.Emitter.Comma = true;
            }
        }

        private static int ConvertVarianceToInt(VarianceModifier variance)
        {
            switch (variance)
            {
                case VarianceModifier.Covariant:
                    return 1;

                case VarianceModifier.Contravariant:
                    return 2;

                default:
                    return 0;
            }
        }

        protected virtual void WriteScope()
        {
            this.EnsureComma();
            this.Write(JS.Vars.SCOPE);
            this.WriteColon();
            this.Write(JS.Vars.EXPORTS);
            this.Emitter.Comma = true;
        }

        protected virtual void WriteKind()
        {
            if (this.TypeInfo.Type.Kind != TypeKind.Class)
            {
                this.EnsureComma();
                this.Write(JS.Fields.KIND);
                this.WriteColon();
                this.WriteScript(this.TypeInfo.Type.Kind.ToString().ToLowerInvariant());
                this.Emitter.Comma = true;
            }
        }

        protected virtual void WriteObjectLiteral()
        {
            if (this.TypeInfo.IsObjectLiteral)
            {
                this.EnsureComma();
                this.Write(JS.Fields.LITERAL);
                this.WriteColon();
                this.WriteScript(true);
                this.Emitter.Comma = true;
            }
        }

        protected virtual void EmitStaticBlock()
        {
            if (this.TypeInfo.HasRealStatic(this.Emitter))
            {
                this.Emitter.StaticBlock = true;
                this.EnsureComma();

                if (this.TypeInfo.InstanceMethods.Any(m => m.Value.Any(subm => this.Emitter.GetEntityName(subm) == JS.Fields.STATICS)) ||
                    this.TypeInfo.InstanceConfig.Fields.Any(m => m.GetName(this.Emitter) == JS.Fields.STATICS))
                {
                    this.Write(JS.Vars.D);
                }

                this.Write(JS.Fields.STATICS);
                this.WriteColon();
                this.BeginBlock();

                var ctorBlock = new ConstructorBlock(this.Emitter, this.TypeInfo, true);
                ctorBlock.Emit();
                this.HasEntryPoint = ctorBlock.HasEntryPoint;

                new MethodBlock(this.Emitter, this.TypeInfo, true).Emit();

                this.WriteNewLine();
                this.EndBlock();
                this.Emitter.Comma = true;
                this.Emitter.StaticBlock = false;
            }
        }

        protected virtual void EmitInstantiableBlock()
        {
            if (this.TypeInfo.IsEnum)
            {
                if (this.Emitter.GetTypeDefinition(this.TypeInfo.Type)
                        .CustomAttributes.Any(attr => attr.AttributeType.FullName == "System.FlagsAttribute"))
                {
                    this.EnsureComma();
                    this.Write(JS.Fields.FLAGS + ": true");
                    this.Emitter.Comma = true;
                }

                var etype = this.TypeInfo.Type.GetDefinition().EnumUnderlyingType;
                var enumMode = this.Emitter.Validator.EnumEmitMode(this.TypeInfo.Type);
                var isString = enumMode >= 3 && enumMode <= 6;
                if (!etype.IsKnownType(KnownTypeCode.Int32) || isString)
                {
                    this.EnsureComma();
                    this.Write(JS.Fields.UNDERLYINGTYPE + ": ");
                    this.Write(isString ? "System.String" : BridgeTypes.ToJsName(etype, this.Emitter));

                    this.Emitter.Comma = true;
                }
            }

            if (this.HasEntryPoint)
            {
                this.EnsureComma();
                this.Write(JS.Fields.ENTRY_POINT + ": true");
                this.Emitter.Comma = true;
            }

            var ctorBlock = new ConstructorBlock(this.Emitter, this.TypeInfo, false);

            if (this.TypeInfo.HasRealInstantiable(this.Emitter) || this.Emitter.Plugins.HasConstructorInjectors(ctorBlock) || this.TypeInfo.ClassType == ClassType.Struct)
            {
                ctorBlock.Emit();
                new MethodBlock(this.Emitter, this.TypeInfo, false).Emit();
            }
        }

        protected virtual void EmitClassEnd()
        {
            this.WriteNewLine();
            this.EndBlock();

            var classStr = this.Emitter.Output.ToString().Substring(this.StartPosition);

            if (Regex.IsMatch(classStr, "^\\s*,\\s*\\{\\s*\\}\\s*$", RegexOptions.Multiline))
            {
                this.Emitter.Output.Remove(this.StartPosition, this.Emitter.Output.Length - this.StartPosition);
            }

            if (this.IsGeneric)
            {
                this.Write("; }");
            }

            this.WriteCloseParentheses();
            this.WriteSemiColon();

            this.EmitAnonymousTypes();
            this.EmitNamedFunctions();

            this.WriteAfterInitMethods();

            this.WriteNewLine();
        }

        private void WriteAfterInitMethods()
        {
            var afterDefineMethods = this.GetAfterDefineMethods();

            if (afterDefineMethods.Any())
            {
                this.WriteNewLine();
            }

            foreach (var method in afterDefineMethods)
            {
                this.WriteNewLine();
                this.Write(method);
            }

            var bottomDefineMethods = this.GetBottomDefineMethods();

            if (bottomDefineMethods.Any())
            {
                foreach (var method in bottomDefineMethods)
                {
                    this.Emitter.EmitterOutput.BottomOutput.Append(method);
                }
            }
        }

        protected virtual void EmitAnonymousTypes()
        {
            var types = this.Emitter.AnonymousTypes.Values.Where(t => !t.Emitted).ToArray();
            if (types.Any())
            {
                this.Emitter.Comma = false;
                this.IntroducePrivateVar();

                foreach (IAnonymousTypeConfig type in types)
                {
                    this.WriteNewLine();
                    this.WriteNewLine();

                    type.Emitted = true;
                    this.Write(type.Code);
                }
            }
        }

        protected virtual void EmitNamedFunctions()
        {
            if (this.Emitter.NamedFunctions.Count > 0)
            {
                this.Emitter.Comma = false;

                this.IntroducePrivateVar();

                var name = BridgeTypes.ToJsName(this.Emitter.TypeInfo.Type, this.Emitter, true);

                this.WriteNewLine();
                this.WriteNewLine();
                this.Write(JS.Funcs.BRIDGE_NS);
                this.WriteOpenParentheses();
                this.WriteScript(name);
                this.Write(", " + JS.Vars.D_ + ")");
                this.WriteSemiColon();

                this.WriteNewLine();
                this.WriteNewLine();
                this.Write(JS.Types.Bridge.APPLY + "(" + JS.Vars.D_ + ".");
                this.Write(name);
                this.Write(", ");
                this.BeginBlock();

                foreach (KeyValuePair<string, string> namedFunction in this.Emitter.NamedFunctions)
                {
                    this.EnsureComma();
                    this.Write(namedFunction.Key + ": " + namedFunction.Value);
                    this.Emitter.Comma = true;
                }

                this.WriteNewLine();
                this.EndBlock();
                this.WriteCloseParentheses();
                this.WriteSemiColon();
            }
        }

        private void IntroducePrivateVar()
        {
            if (!this.Emitter.EmitterOutput.IsPrivateVarIntroduced)
            {
                this.WriteNewLine();
                this.WriteNewLine();
                this.Write("var " + JS.Vars.D_ + " = {};");
                this.Emitter.EmitterOutput.IsPrivateVarIntroduced = true;
            }
        }

        protected virtual IEnumerable<string> GetDefineMethods(string prefix, Func<MethodDeclaration, IMethod, string> fn)
        {
            var methods = this.TypeInfo.InstanceMethods;
            var attrName = "Bridge.InitAttribute";
            int value = 0;

            switch (prefix)
            {
                case "After":
                    value = 0;
                    break;

                case "Before":
                    value = 1;
                    break;

                case "Top":
                    value = 2;
                    break;

                case "Bottom":
                    value = 3;
                    break;
            }

            foreach (var methodGroup in methods)
            {
                foreach (var method in methodGroup.Value)
                {
                    foreach (var attrSection in method.Attributes)
                    {
                        foreach (var attr in attrSection.Attributes)
                        {
                            var rr = this.Emitter.Resolver.ResolveNode(attr.Type, this.Emitter);
                            if (rr.Type.FullName == attrName)
                            {
                                throw new EmitterException(attr, "Instance method cannot be Init method");
                            }
                        }
                    }
                }
            }

            methods = this.TypeInfo.StaticMethods;
            List<string> list = new List<string>();

            foreach (var methodGroup in methods)
            {
                foreach (var method in methodGroup.Value)
                {
                    MemberResolveResult rrMember = null;
                    IMethod rrMethod = null;
                    foreach (var attrSection in method.Attributes)
                    {
                        foreach (var attr in attrSection.Attributes)
                        {
                            var rr = this.Emitter.Resolver.ResolveNode(attr.Type, this.Emitter);
                            if (rr.Type.FullName == attrName)
                            {
                                int? initPosition = null;
                                if (attr.HasArgumentList)
                                {
                                    if (attr.Arguments.Count > 0)
                                    {
                                        var argExpr = attr.Arguments.First();
                                        var argrr = this.Emitter.Resolver.ResolveNode(argExpr, this.Emitter);
                                        if (argrr.ConstantValue is int)
                                        {
                                            initPosition = (int)argrr.ConstantValue;
                                        }
                                    }
                                    else
                                    {
                                        initPosition = 0; //Default InitPosition.After
                                    }
                                }
                                else
                                {
                                    initPosition = 0; //Default InitPosition.After
                                }

                                if (initPosition == value)
                                {
                                    if (rrMember == null)
                                    {
                                        rrMember = this.Emitter.Resolver.ResolveNode(method, this.Emitter) as MemberResolveResult;
                                        rrMethod = rrMember != null ? rrMember.Member as IMethod : null;
                                    }

                                    if (rrMethod != null)
                                    {
                                        if (rrMethod.TypeParameters.Count > 0)
                                        {
                                            throw new EmitterException(method, "Init method cannot be generic");
                                        }

                                        if (rrMethod.Parameters.Count > 0)
                                        {
                                            throw new EmitterException(method, "Init method should not have parameters");
                                        }

                                        if (rrMethod.ReturnType.Kind != TypeKind.Void
                                            && !rrMethod.ReturnType.IsKnownType(KnownTypeCode.Void))
                                        {
                                            throw new EmitterException(method, "Init method should not return anything");
                                        }

                                        list.Add(fn(method, rrMethod));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        protected virtual IEnumerable<string> GetBeforeDefineMethods()
        {
            return this.GetDefineMethods("Before",
                (method, rrMethod) =>
                {
                    this.PushWriter("(function(){0})();");
                    this.ResetLocals();
                    var prevMap = this.BuildLocalsMap();
                    var prevNamesMap = this.BuildLocalsNamesMap();

                    method.Body.AcceptVisitor(this.Emitter);

                    this.ClearLocalsMap(prevMap);
                    this.ClearLocalsNamesMap(prevNamesMap);
                    return this.PopWriter(true);
                });
        }

        protected virtual IEnumerable<string> GetTopDefineMethods()
        {
            return this.GetDefineMethods("Top",
                (method, rrMethod) =>
                {
                    this.PushWriter("{0}");
                    this.ResetLocals();
                    var prevMap = this.BuildLocalsMap();
                    var prevNamesMap = this.BuildLocalsNamesMap();
                    this.Emitter.NoBraceBlock = method.Body;
                    method.Body.AcceptVisitor(this.Emitter);

                    this.ClearLocalsMap(prevMap);
                    this.ClearLocalsNamesMap(prevNamesMap);
                    return this.PopWriter(true);
                });
        }

        protected virtual IEnumerable<string> GetBottomDefineMethods()
        {
            return this.GetDefineMethods("Bottom",
                (method, rrMethod) =>
                {
                    this.PushWriter("{0}");
                    this.ResetLocals();
                    var prevMap = this.BuildLocalsMap();
                    var prevNamesMap = this.BuildLocalsNamesMap();
                    this.Emitter.NoBraceBlock = method.Body;
                    method.Body.AcceptVisitor(this.Emitter);

                    this.ClearLocalsMap(prevMap);
                    this.ClearLocalsNamesMap(prevNamesMap);
                    return this.PopWriter(true);
                });
        }

        protected virtual IEnumerable<string> GetAfterDefineMethods()
        {
            return this.GetDefineMethods("After",
                (method, rrMethod) =>
                    BridgeTypes.ToJsName(rrMethod.DeclaringTypeDefinition, this.Emitter) + "." +
                    this.Emitter.GetEntityName(method) + "();");
        }
    }
}