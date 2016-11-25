using Bridge.Contract;
using Bridge.Contract.Constants;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge.Translator
{
    public abstract partial class ConversionBlock : AbstractEmitterBlock
    {
        public ConversionBlock(IEmitter emitter, AstNode node)
            : base(emitter, node)
        {
        }

        protected sealed override void DoEmit()
        {
            this.AfterOutput = "";
            this.AfterExpressionOutput = "";
            var expression = this.GetExpression();

            if (expressionInWork.Contains(expression))
            {
                if (!ConversionBlock.expressionIgnoreUserDefine.Contains(expression))
                {
                    if (ConversionBlock.expressionMap.ContainsKey(expression))
                    {
                        this.Write(ConversionBlock.expressionMap[expression]);
                    }
                    else
                    {
                        this.EmitConversionExpression();
                    }
                    
                    return;
                }
            }
            else
            {
                expressionInWork.Add(expression);
            }

            int parenthesesLevel = 0;
            bool check = expression != null && !expression.IsNull && expression.Parent != null;

            if (check)
            {
                parenthesesLevel = this.CheckConversion(expression);
            }

            if (this.DisableEmitConversionExpression)
            {
                if (expressionInWork.Contains(expression) && !ConversionBlock.expressionIgnoreUserDefine.Contains(expression))
                {
                    expressionInWork.Remove(expression);
                }
                return;
            }

            if (expression != null && ConversionBlock.expressionMap.ContainsKey(expression))
            {
                this.Write(ConversionBlock.expressionMap[expression]);
            }
            else
            {
                this.EmitConversionExpression();
            }

            if (expressionInWork.Contains(expression) && !ConversionBlock.expressionIgnoreUserDefine.Contains(expression))
            {
                expressionInWork.Remove(expression);
            }

            if (parenthesesLevel > 0)
            {
                for (int i = 0; i < parenthesesLevel; i++)
                {
                    this.WriteCloseParentheses();
                }
            }

            if (this.AfterOutput.Length > 0)
            {
                this.Write(this.AfterOutput);
            }
        }

        protected virtual string AfterOutput
        {
            get;
            set;
        }

        protected virtual string AfterExpressionOutput
        {
            get;
            set;
        }

        internal static List<Expression> expressionInWork = new List<Expression>();
        internal static List<Expression> expressionIgnoreUserDefine = new List<Expression>();
        internal static Dictionary<Expression, string> expressionMap = new Dictionary<Expression, string>(); 

        protected virtual bool DisableEmitConversionExpression
        {
            get;
            set;
        }

        protected virtual int CheckConversion(Expression expression)
        {
            return ConversionBlock.CheckConversion(this, expression);
        }

        public static bool IsUserDefinedConversion(AbstractEmitterBlock block, Expression expression)
        {
            Conversion conversion = null;

            try
            {
                // The resolveNode call required to get GetConversion not fail
                block.Emitter.Resolver.ResolveNode(expression, null);
                conversion = block.Emitter.Resolver.Resolver.GetConversion(expression);

                if (conversion == null)
                {
                    return false;
                }

                return conversion.IsUserDefined;
            }
            catch
            {
            }

            return false;
        }

        public static int CheckConversion(ConversionBlock block, Expression expression)
        {
            try
            {
                var rr = block.Emitter.Resolver.ResolveNode(expression, block.Emitter);
                var conversion = block.Emitter.Resolver.Resolver.GetConversion(expression);
                var expectedType = block.Emitter.Resolver.Resolver.GetExpectedType(expression);
                int level = 0;

                return ConversionBlock.DoConversion(block, expression, conversion, expectedType, level, rr);
            }
            catch
            {
            }

            return 0;
        }

        private static int DoConversion(ConversionBlock block, Expression expression, Conversion conversion, IType expectedType,
            int level, ResolveResult rr, bool ignoreConversionResolveResult = false)
        {
            if (ConversionBlock.expressionIgnoreUserDefine.Contains(expression) && conversion.IsUserDefined)
            {
                expectedType = conversion.Method.Parameters.First().Type;
            }

            if (block.Emitter.IsAssignment)
            {
                return level;
            }

            if (expression is ParenthesizedExpression && ((ParenthesizedExpression) expression).Expression is CastExpression)
            {
                return level;
            }

            if (conversion.IsUserDefined && expression.Parent is CastExpression &&
                ((CastExpression) expression.Parent).Expression == expression)
            {
                var parentConversion = block.Emitter.Resolver.Resolver.GetConversion((CastExpression) expression.Parent);

                if (!parentConversion.IsUserDefined || parentConversion.Method.Equals(conversion.Method))
                {
                    return level;
                }
            }

            if (rr is ConstantResolveResult && expression is CastExpression && !conversion.IsUserDefined)
            {
                return level;
            }

            var convrr = rr as ConversionResolveResult;

            if (convrr != null && convrr.Input is ConstantResolveResult && !convrr.Conversion.IsUserDefined)
            {
                return level;
            }

            string afterUserDefined = "";
            if (convrr != null && !conversion.IsUserDefined && !ignoreConversionResolveResult)
            {
                if (expectedType != convrr.Type)
                {
                    if (expression.Parent is CastExpression &&
                        ((CastExpression) expression.Parent).Expression == expression)
                    {
                        var parentExpectedType = block.Emitter.Resolver.Resolver.GetExpectedType((CastExpression) expression.Parent);
                        var parentrr = block.Emitter.Resolver.ResolveNode(expression.Parent, block.Emitter) as ConversionResolveResult;

                        if (parentrr != null && parentrr.Type != expectedType || parentrr == null && expectedType != parentExpectedType)
                        {
                            level = ConversionBlock.DoConversion(block, expression, conversion, expectedType, level, rr, true);
                            afterUserDefined = block.AfterOutput;
                            block.AfterOutput = "";
                        }
                    }
                    else
                    {
                        level = ConversionBlock.DoConversion(block, expression, conversion, expectedType, level, rr, true);
                        afterUserDefined = block.AfterOutput;
                        block.AfterOutput = "";
                    }
                }

                conversion = convrr.Conversion;
                rr = convrr.Input;
                expectedType = convrr.Type;
            }

            var isNumLifted = conversion != null && conversion.IsImplicit && conversion.IsLifted &&
                              conversion.IsNumericConversion && !(expression is BinaryOperatorExpression);
            bool skipUserConversion = conversion == null || (!conversion.IsUserDefined &&
                                                             (Helpers.IsDecimalType(expectedType, block.Emitter.Resolver) ||
                                                              Helpers.Is64Type(expectedType, block.Emitter.Resolver) ||
                                                              conversion.IsIdentityConversion ||
                                                              isNumLifted));

            if (!skipUserConversion && conversion.IsUserDefined &&
                (expression is BinaryOperatorExpression || expression is UnaryOperatorExpression ||
                 expression.Parent is AssignmentExpression))
            {
                level = ConversionBlock.CheckUserDefinedConversion(block, expression, conversion, level, rr, expectedType);

                if (conversion.IsUserDefined && block.DisableEmitConversionExpression)
                {
                    return level;
                }

                afterUserDefined = block.AfterOutput + afterUserDefined;
                block.AfterOutput = "";
            }

            if (!(conversion.IsExplicit && conversion.IsNumericConversion))
            {
                if (ConversionBlock.CheckDecimalConversion(block, expression, rr, expectedType, conversion, ignoreConversionResolveResult))
                {
                    block.AfterOutput += ")";
                }
            }

            if (Helpers.IsDecimalType(expectedType, block.Emitter.Resolver) && !conversion.IsUserDefined)
            {
                var s = block.AfterOutput;
                block.AfterOutput = "";
                if (!((expression.Parent is CastExpression) && !(expression is CastExpression)))
                {
                    ConversionBlock.CheckNumericConversion(block, expression, rr, expectedType, conversion);
                }

                block.AfterOutput =block.AfterOutput + s + afterUserDefined;
                return level;
            }

            if (!(conversion.IsExplicit && conversion.IsNumericConversion))
            {
                if (ConversionBlock.CheckLongConversion(block, expression, rr, expectedType, conversion, ignoreConversionResolveResult))
                {
                    block.AfterOutput += ")";
                }
            }

            if (Helpers.Is64Type(expectedType, block.Emitter.Resolver) && !conversion.IsUserDefined)
            {
                var s = block.AfterOutput;
                block.AfterOutput = "";
                if (!((expression.Parent is CastExpression) && !(expression is CastExpression)))
                {
                    ConversionBlock.CheckNumericConversion(block, expression, rr, expectedType, conversion);
                }

                block.AfterOutput = block.AfterOutput + s + afterUserDefined;
                return level;
            }

            if (!((expression.Parent is CastExpression) && !(expression is CastExpression)))
            {
                ConversionBlock.CheckNumericConversion(block, expression, rr, expectedType, conversion);
            }

            if (conversion.IsIdentityConversion)
            {
                block.AfterOutput = block.AfterOutput + afterUserDefined;
                return level;
            }

            if (isNumLifted && !conversion.IsUserDefined)
            {
                block.AfterOutput = block.AfterOutput + afterUserDefined;
                return level;
            }
            bool isLifted = conversion.IsLifted && !isNumLifted && !(block is CastBlock) &&
                            !Helpers.IsDecimalType(expectedType, block.Emitter.Resolver) &&
                            !Helpers.Is64Type(expectedType, block.Emitter.Resolver) && !NullableType.IsNullable(expectedType);
            if (isLifted)
            {
                block.Write(JS.Types.SYSTEM_NULLABLE + ".getValue(");
                block.AfterOutput += ")";
            }

            if (conversion.IsUserDefined &&
                !(expression is BinaryOperatorExpression || expression is UnaryOperatorExpression ||
                  expression.Parent is AssignmentExpression))
            {
                level = ConversionBlock.CheckUserDefinedConversion(block, expression, conversion, level, rr, expectedType);
            }

            block.AfterOutput = block.AfterOutput + afterUserDefined;
            return level;
        }

        private static int CheckUserDefinedConversion(ConversionBlock block, Expression expression, Conversion conversion, int level, ResolveResult rr, IType expectedType)
        {
            if (conversion.IsUserDefined && !ConversionBlock.expressionIgnoreUserDefine.Contains(expression))
            {
                var method = conversion.Method;

                string inline = block.Emitter.GetInline(method);

                if (conversion.IsExplicit && !string.IsNullOrWhiteSpace(inline))
                {
                    // Still returns true if Nullable.lift( was written.
                    return level;
                }

                if (!string.IsNullOrWhiteSpace(inline))
                {
                    ConversionBlock.expressionIgnoreUserDefine.Add(expression);

                    if (expression is InvocationExpression)
                    {
                        new InlineArgumentsBlock(block.Emitter, new ArgumentsInfo(block.Emitter, (InvocationExpression)expression), inline).Emit();
                    }
                    else if (expression is ObjectCreateExpression)
                    {
                        new InlineArgumentsBlock(block.Emitter, new ArgumentsInfo(block.Emitter, (ObjectCreateExpression)expression), inline).Emit();
                    }
                    else if (expression is UnaryOperatorExpression)
                    {
                        var unaryExpression = (UnaryOperatorExpression)expression;
                        var resolveOperator = block.Emitter.Resolver.ResolveNode(unaryExpression, block.Emitter);
                        OperatorResolveResult orr = resolveOperator as OperatorResolveResult;
                        new InlineArgumentsBlock(block.Emitter, new ArgumentsInfo(block.Emitter, unaryExpression, orr, method), inline).Emit();
                    }
                    else
                    {
                        new InlineArgumentsBlock(block.Emitter, new ArgumentsInfo(block.Emitter, expression, method), inline, method).Emit();
                    }

                    block.DisableEmitConversionExpression = true;
                    ConversionBlock.expressionIgnoreUserDefine.Remove(expression);

                    // Still returns true if Nullable.lift( was written.
                    return level;
                }
                else
                {
                    if (method.DeclaringTypeDefinition != null && (block.Emitter.Validator.IsIgnoreType(method.DeclaringTypeDefinition) || Helpers.IsIgnoreCast(method.DeclaringTypeDefinition, block.Emitter)))
                    {
                        // Still returns true if Nullable.lift( was written.
                        return level;
                    }

                    block.Write(BridgeTypes.ToJsName(method.DeclaringType, block.Emitter));
                    block.WriteDot();

                    block.Write(OverloadsCollection.Create(block.Emitter, method).GetOverloadName());
                }

                block.WriteOpenParentheses();
                block.AfterOutput += ")";

                var arg = method.Parameters[0];

                if (Helpers.IsDecimalType(arg.Type, block.Emitter.Resolver, arg.IsParams) && !Helpers.IsDecimalType(rr.Type, block.Emitter.Resolver) && !expression.IsNull)
                {
                    block.Write(JS.Types.SYSTEM_DECIMAL);
                    if (NullableType.IsNullable(arg.Type) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write("." + JS.Funcs.Math.LIFT);
                    }
                    if (expression is CastExpression &&
                    ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return level;
                    }
                    block.WriteOpenParentheses();
                    block.AfterOutput += ")";
                }

                if (Helpers.Is64Type(arg.Type, block.Emitter.Resolver, arg.IsParams) && !Helpers.Is64Type(rr.Type, block.Emitter.Resolver) && !expression.IsNull)
                {
                    var isUint = Helpers.IsULongType(arg.Type, block.Emitter.Resolver, arg.IsParams);
                    block.Write(isUint ? JS.Types.SYSTEM_UInt64 : JS.Types.System.Int64.NAME);
                    if (NullableType.IsNullable(arg.Type) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write("." + JS.Funcs.Math.LIFT);
                    }
                    if (expression is CastExpression &&
                    ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return level;
                    }
                    block.WriteOpenParentheses();
                    block.AfterOutput += ")";
                }
            }

            return level;
        }

        private delegate bool IsType(IType type, IMemberResolver resolver, bool allowArray = false);

        private static bool CheckTypeConversion(ConversionBlock block, Expression expression, ResolveResult rr, IType expectedType, Conversion conversion, string typeName, IsType isType, bool ignoreConversionResolveResult)
        {
            if (conversion.IsUserDefined)
            {
                var m = conversion.Method;
                if (isType(m.ReturnType, block.Emitter.Resolver))
                {
                    return false;
                }
            }

            if (expression is CastExpression && !ignoreConversionResolveResult)
            {
                var nestedExpr = ((CastExpression)expression).Expression;
                var nested_rr = block.Emitter.Resolver.ResolveNode(nestedExpr, block.Emitter);

                if (!(nested_rr is ConversionResolveResult))
                {
                    return false;
                }
            }

            var invocationExpression = expression.Parent as InvocationExpression;
            if (invocationExpression != null && invocationExpression.Arguments.Any(a => a == expression))
            {
                var index = invocationExpression.Arguments.ToList().IndexOf(expression);
                var methodResolveResult = block.Emitter.Resolver.ResolveNode(invocationExpression, block.Emitter) as MemberResolveResult;

                if (methodResolveResult != null)
                {
                    var m = methodResolveResult.Member as IMethod;
                    var arg = m.Parameters[index < m.Parameters.Count ? index : (m.Parameters.Count - 1)];

                    if (isType(arg.Type, block.Emitter.Resolver, arg.IsParams) && !isType(rr.Type, block.Emitter.Resolver))
                    {
                        if (expression.IsNull)
                        {
                            return false;
                        }

                        block.Write(typeName);
                        if (NullableType.IsNullable(arg.Type) && ConversionBlock.ShouldBeLifted(expression))
                        {
                            block.Write("." + JS.Funcs.Math.LIFT);
                        }
                        if (!ignoreConversionResolveResult && expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                        {
                            return false;
                        }
                        block.WriteOpenParentheses();
                        return true;
                    }
                }
            }

            var objectCreateExpression = expression.Parent as ObjectCreateExpression;
            if (objectCreateExpression != null && objectCreateExpression.Arguments.Any(a => a == expression))
            {
                var index = objectCreateExpression.Arguments.ToList().IndexOf(expression);
                var methodResolveResult = block.Emitter.Resolver.ResolveNode(objectCreateExpression, block.Emitter) as MemberResolveResult;

                if (methodResolveResult != null)
                {
                    var m = methodResolveResult.Member as IMethod;
                    var arg = m.Parameters[index < m.Parameters.Count ? index : (m.Parameters.Count - 1)];

                    if (isType(arg.Type, block.Emitter.Resolver, arg.IsParams) && !isType(rr.Type, block.Emitter.Resolver))
                    {
                        if (expression.IsNull)
                        {
                            return false;
                        }

                        block.Write(typeName);
                        if (NullableType.IsNullable(arg.Type) && ConversionBlock.ShouldBeLifted(expression))
                        {
                            block.Write("." + JS.Funcs.Math.LIFT);
                        }
                        if (!ignoreConversionResolveResult && expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                        {
                            return false;
                        }
                        block.WriteOpenParentheses();
                        return true;
                    }
                }
            }

            var namedArgExpression = expression.Parent as NamedArgumentExpression;
            if (namedArgExpression != null)
            {
                var namedArgResolveResult = block.Emitter.Resolver.ResolveNode(namedArgExpression, block.Emitter) as NamedArgumentResolveResult;

                if (isType(namedArgResolveResult.Type, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write(typeName);
                    if (NullableType.IsNullable(namedArgResolveResult.Type) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write("." + JS.Funcs.Math.LIFT);
                    }
                    if (!ignoreConversionResolveResult && expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            var namedExpression = expression.Parent as NamedExpression;
            if (namedExpression != null)
            {
                var namedResolveResult = block.Emitter.Resolver.ResolveNode(namedExpression, block.Emitter);

                if (isType(namedResolveResult.Type, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write(typeName);
                    if (NullableType.IsNullable(namedResolveResult.Type) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write("." + JS.Funcs.Math.LIFT);
                    }
                    if (!ignoreConversionResolveResult && expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            var binaryOpExpr = expression.Parent as BinaryOperatorExpression;
            if (binaryOpExpr != null)
            {
                var idx = binaryOpExpr.Left == expression ? 0 : 1;
                var binaryOpRr = block.Emitter.Resolver.ResolveNode(binaryOpExpr, block.Emitter) as OperatorResolveResult;

                if (binaryOpRr != null && isType(binaryOpRr.Operands[idx].Type, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                {
                    var isNullable = NullableType.IsNullable(binaryOpRr.Operands[idx].Type);
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write(typeName);
                    if (isNullable && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write("." + JS.Funcs.Math.LIFT);
                    }
                    if (!ignoreConversionResolveResult && expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            var conditionalExpr = expression.Parent as ConditionalExpression;
            if (conditionalExpr != null && conditionalExpr.Condition != expression)
            {
                var idx = conditionalExpr.TrueExpression == expression ? 1 : 2;
                var conditionalrr = block.Emitter.Resolver.ResolveNode(conditionalExpr, block.Emitter) as OperatorResolveResult;

                if (conditionalrr != null && isType(conditionalrr.Operands[idx].Type, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write(typeName);
                    if (NullableType.IsNullable(conditionalrr.Operands[idx].Type) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write("." + JS.Funcs.Math.LIFT);
                    }
                    if (!ignoreConversionResolveResult && expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            var assignmentExpr = expression.Parent as AssignmentExpression;
            if (assignmentExpr != null)
            {
                var assigmentRr = block.Emitter.Resolver.ResolveNode(assignmentExpr, block.Emitter) as OperatorResolveResult;

                if (isType(assigmentRr.Operands[1].Type, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write(typeName);
                    if (NullableType.IsNullable(assigmentRr.Operands[1].Type) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write("." + JS.Funcs.Math.LIFT);
                    }
                    if (!ignoreConversionResolveResult && expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            var indexerExpr = expression.Parent as IndexerExpression;
            if (indexerExpr != null)
            {
                var index = indexerExpr.Arguments.ToList().IndexOf(expression);

                if (index >= 0)
                {
                    var invocationrr = block.Emitter.Resolver.ResolveNode(indexerExpr, block.Emitter) as InvocationResolveResult;
                    if (invocationrr != null)
                    {
                        var parameters = invocationrr.Member.Parameters;
                        if (parameters.Count <= index)
                        {
                            index = parameters.Count - 1;
                        }

                        if (isType(invocationrr.Member.Parameters.ElementAt(index).Type, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                        {
                            if (expression.IsNull)
                            {
                                return false;
                            }

                            block.Write(typeName);
                            if (NullableType.IsNullable(invocationrr.Member.Parameters.ElementAt(index).Type) && ConversionBlock.ShouldBeLifted(expression))
                            {
                                block.Write("." + JS.Funcs.Math.LIFT);
                            }
                            if (!ignoreConversionResolveResult && expression is CastExpression &&
                                ((CastExpression)expression).Expression is ParenthesizedExpression)
                            {
                                return false;
                            }
                            block.WriteOpenParentheses();
                            return true;
                        }
                    }
                }
            }

            var arrayInit = expression.Parent as ArrayInitializerExpression;
            if (arrayInit != null)
            {
                while (arrayInit.Parent is ArrayInitializerExpression)
                {
                    arrayInit = (ArrayInitializerExpression)arrayInit.Parent;
                }

                IType elementType = null;
                var arrayCreate = arrayInit.Parent as ArrayCreateExpression;
                if (arrayCreate != null)
                {
                    var rrArrayType = block.Emitter.Resolver.ResolveNode(arrayCreate, block.Emitter);
                    if (rrArrayType.Type is TypeWithElementType)
                    {
                        elementType = ((TypeWithElementType)rrArrayType.Type).ElementType;
                    }
                    else
                    {
                        elementType = rrArrayType.Type;
                    }
                }
                else
                {
                    var rrElemenet = block.Emitter.Resolver.ResolveNode(arrayInit.Parent, block.Emitter);
                    var pt = rrElemenet.Type as ParameterizedType;
                    if (pt != null && pt.TypeArguments.Count > 0)
                    {
                        if (pt.TypeArguments.Count == 1)
                        {
                            elementType = pt.TypeArguments.First();
                        }
                        else
                        {
                            var index = 0;
                            arrayInit = expression.Parent as ArrayInitializerExpression;

                            for (int i = 0; i < arrayInit.Elements.Count; i++)
                            {
                                if (expression == arrayInit.Elements.ElementAt(i))
                                {
                                    index = i;
                                    break;
                                }
                            }

                            elementType = index < pt.TypeArguments.Count ? pt.TypeArguments.ElementAt(index) : pt.TypeArguments.ElementAt(0);
                        }
                    }
                    else
                    {
                        var arrayType = rrElemenet.Type as TypeWithElementType;
                        if (arrayType != null)
                        {
                            elementType = arrayType.ElementType;
                        }
                        else
                        {
                            elementType = rrElemenet.Type;
                        }
                    }
                }

                if (elementType != null && isType(elementType, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write(typeName);
                    if (NullableType.IsNullable(elementType) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write("." + JS.Funcs.Math.LIFT);
                    }
                    if (!ignoreConversionResolveResult && expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
                else if (Helpers.Is64Type(rr.Type, block.Emitter.Resolver)
                         && Helpers.IsFloatType(elementType, block.Emitter.Resolver)
                         && !Helpers.IsDecimalType(elementType, block.Emitter.Resolver)
                         && isType(rr.Type, block.Emitter.Resolver))
                {
                    block.Write(JS.Types.System.Int64.TONUMBER);
                    if (!ignoreConversionResolveResult && expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            if (isType(expectedType, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver) && !(conversion.IsExplicit && conversion.IsNumericConversion))
            {
                var castExpr = expression.Parent as CastExpression;
                ResolveResult castTypeRr = null;
                if (castExpr != null)
                {
                    castTypeRr = block.Emitter.Resolver.ResolveNode(castExpr.Type, block.Emitter);
                }

                /*if (castTypeRr == null || !isType(castTypeRr.Type, block.Emitter.Resolver))*/
                if (castTypeRr == null || !conversion.IsExplicit)
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write(typeName);
                    if (NullableType.IsNullable(expectedType) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write("." + JS.Funcs.Math.LIFT);
                    }

                    if (!ignoreConversionResolveResult && expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            return false;
        }

        private static bool IsTypeConversion(ConversionBlock block, Expression expression, ResolveResult rr, IType expectedType, Conversion conversion, string typeName, IsType isType)
        {
            if (conversion.IsUserDefined)
            {
                var m = conversion.Method;
                if (isType(m.ReturnType, block.Emitter.Resolver))
                {
                    return false;
                }
            }

            var invocationExpression = expression.Parent as InvocationExpression;
            if (invocationExpression != null && invocationExpression.Arguments.Any(a => a == expression))
            {
                var index = invocationExpression.Arguments.ToList().IndexOf(expression);
                var methodResolveResult = block.Emitter.Resolver.ResolveNode(invocationExpression, block.Emitter) as MemberResolveResult;

                if (methodResolveResult != null)
                {
                    var m = methodResolveResult.Member as IMethod;
                    var arg = m.Parameters[index < m.Parameters.Count ? index : (m.Parameters.Count - 1)];

                    if (isType(arg.Type, block.Emitter.Resolver, arg.IsParams) && !isType(rr.Type, block.Emitter.Resolver))
                    {
                        if (expression.IsNull)
                        {
                            return false;
                        }

                        if (expression is CastExpression && ((CastExpression)expression).Expression is ParenthesizedExpression)
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }

            var objectCreateExpression = expression.Parent as ObjectCreateExpression;
            if (objectCreateExpression != null && objectCreateExpression.Arguments.Any(a => a == expression))
            {
                var index = objectCreateExpression.Arguments.ToList().IndexOf(expression);
                var methodResolveResult = block.Emitter.Resolver.ResolveNode(objectCreateExpression, block.Emitter) as MemberResolveResult;

                if (methodResolveResult != null)
                {
                    var m = methodResolveResult.Member as IMethod;
                    var arg = m.Parameters[index < m.Parameters.Count ? index : (m.Parameters.Count - 1)];

                    if (isType(arg.Type, block.Emitter.Resolver, arg.IsParams) && !isType(rr.Type, block.Emitter.Resolver))
                    {
                        if (expression.IsNull)
                        {
                            return false;
                        }

                        if (expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }

            var namedArgExpression = expression.Parent as NamedArgumentExpression;
            if (namedArgExpression != null)
            {
                var namedArgResolveResult = block.Emitter.Resolver.ResolveNode(namedArgExpression, block.Emitter) as NamedArgumentResolveResult;

                if (isType(namedArgResolveResult.Type, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    if (expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }

                    return true;
                }
            }

            var namedExpression = expression.Parent as NamedExpression;
            if (namedExpression != null)
            {
                var namedResolveResult = block.Emitter.Resolver.ResolveNode(namedExpression, block.Emitter);

                if (isType(namedResolveResult.Type, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    if (expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }
                    return true;
                }
            }

            var binaryOpExpr = expression.Parent as BinaryOperatorExpression;
            if (binaryOpExpr != null)
            {
                var idx = binaryOpExpr.Left == expression ? 0 : 1;
                var binaryOpRr = block.Emitter.Resolver.ResolveNode(binaryOpExpr, block.Emitter) as OperatorResolveResult;

                if (binaryOpRr != null && isType(binaryOpRr.Operands[idx].Type, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    if (expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }

                    return true;
                }
            }

            var conditionalExpr = expression.Parent as ConditionalExpression;
            if (conditionalExpr != null && conditionalExpr.Condition != expression)
            {
                var idx = conditionalExpr.TrueExpression == expression ? 0 : 1;
                var conditionalrr = block.Emitter.Resolver.ResolveNode(conditionalExpr, block.Emitter) as OperatorResolveResult;

                if (conditionalrr != null && isType(conditionalrr.Operands[idx].Type, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    if (expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }

                    return true;
                }
            }

            var assignmentExpr = expression.Parent as AssignmentExpression;
            if (assignmentExpr != null)
            {
                var assigmentRr = block.Emitter.Resolver.ResolveNode(assignmentExpr, block.Emitter) as OperatorResolveResult;

                if (isType(assigmentRr.Operands[1].Type, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    if (expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }

                    return true;
                }
            }

            var arrayInit = expression.Parent as ArrayInitializerExpression;
            if (arrayInit != null)
            {
                while (arrayInit.Parent is ArrayInitializerExpression)
                {
                    arrayInit = (ArrayInitializerExpression)arrayInit.Parent;
                }

                IType elementType = null;
                var arrayCreate = arrayInit.Parent as ArrayCreateExpression;
                if (arrayCreate != null)
                {
                    var rrArrayType = block.Emitter.Resolver.ResolveNode(arrayCreate, block.Emitter);
                    if (rrArrayType.Type is TypeWithElementType)
                    {
                        elementType = ((TypeWithElementType)rrArrayType.Type).ElementType;
                    }
                    else
                    {
                        elementType = rrArrayType.Type;
                    }
                }
                else
                {
                    var rrElemenet = block.Emitter.Resolver.ResolveNode(arrayInit.Parent, block.Emitter);
                    var pt = rrElemenet.Type as ParameterizedType;
                    if (pt != null)
                    {
                        elementType = pt.TypeArguments.Count > 0 ? pt.TypeArguments.First() : null;
                    }
                    else
                    {
                        var arrayType = rrElemenet.Type as TypeWithElementType;
                        if (arrayType != null)
                        {
                            elementType = arrayType.ElementType;
                        }
                        else
                        {
                            elementType = rrElemenet.Type;
                        }
                    }
                }

                if (elementType != null && isType(elementType, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    if (expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }

                    return true;
                }
                else if (Helpers.Is64Type(rr.Type, block.Emitter.Resolver)
                         && Helpers.IsFloatType(elementType, block.Emitter.Resolver)
                         && !Helpers.IsDecimalType(elementType, block.Emitter.Resolver)
                         && isType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }
                    return true;
                }
            }

            if (isType(expectedType, block.Emitter.Resolver) && !isType(rr.Type, block.Emitter.Resolver) && !(conversion.IsExplicit && conversion.IsNumericConversion))
            {
                var castExpr = expression.Parent as CastExpression;
                ResolveResult castTypeRr = null;
                if (castExpr != null)
                {
                    castTypeRr = block.Emitter.Resolver.ResolveNode(castExpr.Type, block.Emitter);
                }

                /*if (castTypeRr == null || !isType(castTypeRr.Type, block.Emitter.Resolver))*/
                if (castTypeRr == null || !conversion.IsExplicit)
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    if (expression is CastExpression &&
                        ((CastExpression)expression).Expression is ParenthesizedExpression)
                    {
                        return false;
                    }

                    return true;
                }
            }

            return false;
        }

        private static bool CheckDecimalConversion(ConversionBlock block, Expression expression, ResolveResult rr, IType expectedType, Conversion conversion, bool ignoreConversionResolveResult)
        {
            return CheckTypeConversion(block, expression, rr, expectedType, conversion, JS.Types.SYSTEM_DECIMAL, Helpers.IsDecimalType, ignoreConversionResolveResult);
        }

        private static bool CheckLongConversion(ConversionBlock block, Expression expression, ResolveResult rr, IType expectedType, Conversion conversion, bool ignoreConversionResolveResult)
        {
            return CheckTypeConversion(block, expression, rr, expectedType, conversion, JS.Types.System.Int64.NAME, Helpers.IsLongType, ignoreConversionResolveResult) ||
                   CheckTypeConversion(block, expression, rr, expectedType, conversion, JS.Types.SYSTEM_UInt64, Helpers.IsULongType, ignoreConversionResolveResult);
        }

        private static bool IsLongConversion(ConversionBlock block, Expression expression, ResolveResult rr, IType expectedType, Conversion conversion)
        {
            return IsTypeConversion(block, expression, rr, expectedType, conversion, JS.Types.System.Int64.NAME, Helpers.IsLongType) ||
                   IsTypeConversion(block, expression, rr, expectedType, conversion, JS.Types.SYSTEM_UInt64, Helpers.IsULongType);
        }

        public static bool ShouldBeLifted(Expression expr)
        {
            return !(expr is PrimitiveExpression || expr.IsNull);
        }

        protected abstract void EmitConversionExpression();

        protected abstract Expression GetExpression();
    }
}