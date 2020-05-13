using Parse.FrontEnd.Grammars.Properties;
using Parse.FrontEnd.RegularGrammar;
using Parse.FrontEnd.Grammars.MiniC.Sdts;

namespace Parse.FrontEnd.Grammars.MiniC
{
    public class MiniCGrammar : Grammar
    {
        public Terminal If { get; } = new Terminal(TokenType.Keyword.Controlword, "if");
        public Terminal Else { get; } = new Terminal(TokenType.Keyword.Controlword, "else");
        public Terminal While { get; } = new Terminal(TokenType.Keyword.Repeateword, "while");
        public Terminal Return { get; } = new Terminal(TokenType.Keyword.Controlword, "return");
        public Terminal Const { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "const");
        public Terminal Int { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "int");
        public Terminal Void { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "void");
        public Terminal Ident { get; } = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", Resource.Ident, true, true);
        public Terminal Number { get; } = new Terminal(TokenType.Digit.Digit10, "[0-9]+", Resource.Number, true, true);
        public Terminal LineComment { get; } = new Terminal(TokenType.SpecialToken.Comment, "//.*$", false, true);

        public Terminal OpenParenthesis { get; } = new Terminal(TokenType.Operator.Parenthesis, "(");
        public Terminal CloseParenthesis { get; } = new Terminal(TokenType.Operator.Parenthesis, ")");
        public Terminal OpenCurlyBrace { get; } = new Terminal(TokenType.Operator.CurlyBrace, "{");
        public Terminal CloseCurlyBrace { get; } = new Terminal(TokenType.Operator.CurlyBrace, "}");
        public Terminal OpenSquareBrace { get; } = new Terminal(TokenType.Operator.Square, "[");
        public Terminal CloseSquareBrace { get; } = new Terminal(TokenType.Operator.Square, "]");

        private Terminal scopeCommentStart = new Terminal(TokenType.SpecialToken.Comment.ScopeComment, "/*");
        private Terminal scopeCommentEnd = new Terminal(TokenType.SpecialToken.Comment.ScopeComment, "*/");

        public Terminal Inc { get; } = new Terminal(TokenType.Operator.NormalOperator, "++");
        public Terminal Dec { get; } = new Terminal(TokenType.Operator.NormalOperator, "--");
        public Terminal Add { get; } = new Terminal(TokenType.Operator.NormalOperator, "+");
        public Terminal Sub { get; } = new Terminal(TokenType.Operator.NormalOperator, "-");
        public Terminal Mul { get; } = new Terminal(TokenType.Operator.NormalOperator, "*");
        public Terminal Div { get; } = new Terminal(TokenType.Operator.NormalOperator, "/");
        public Terminal Mod { get; } = new Terminal(TokenType.Operator.NormalOperator, "%");
        public Terminal Assign { get; } = new Terminal(TokenType.Operator.NormalOperator, "=");
        public Terminal AddAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "+=");
        public Terminal SubAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "-=");
        public Terminal MulAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "*=");
        public Terminal DivAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "/=");
        public Terminal ModAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "%=");
        public Terminal Equal { get; } = new Terminal(TokenType.Operator.NormalOperator, "==");
        public Terminal NotEqual { get; } = new Terminal(TokenType.Operator.NormalOperator, "!=");
        public Terminal GreaterThan { get; } = new Terminal(TokenType.Operator.NormalOperator, ">");
        public Terminal LessThan { get; } = new Terminal(TokenType.Operator.NormalOperator, "<");
        public Terminal GreaterEqual { get; } = new Terminal(TokenType.Operator.NormalOperator, ">=");
        public Terminal LessEqual { get; } = new Terminal(TokenType.Operator.NormalOperator, "<=");
        public Terminal SemiColon { get; } = new Terminal(TokenType.Operator.NormalOperator, ";");
        public Terminal Comma { get; } = new Terminal(TokenType.Operator.Comma, ",");

        public Terminal LogicalOr { get; } = new Terminal(TokenType.Operator, "||");
        public Terminal LogicalAnd { get; } = new Terminal(TokenType.Operator, "&&");
        public Terminal LogicalNot { get; } = new Terminal(TokenType.Operator, "!");


        private NonTerminal miniC = new NonTerminal("mini_c", true);
        private NonTerminal translationUnit = new NonTerminal("translation_unit");
        private NonTerminal externalDcl = new NonTerminal("external_dcl");
        private NonTerminal functionDef = new NonTerminal("function_def");
        private NonTerminal functionHeader = new NonTerminal("function_header");
        private NonTerminal dclSpec = new NonTerminal("dcl_spec");
        private NonTerminal dclSpecifiers = new NonTerminal("dcl_specifiers");
        private NonTerminal dclSpecifier = new NonTerminal("dcl_specifier");
        private NonTerminal typeQualifier = new NonTerminal("type_qualifier");
        private NonTerminal typeSpecifier = new NonTerminal("type_specifier");
        private NonTerminal functionName = new NonTerminal("function_name");
        private NonTerminal formalParam = new NonTerminal("formal_param");
        private NonTerminal optFormalParam = new NonTerminal("opt_formal_param");
        private NonTerminal formalParamList = new NonTerminal("formal_param_list");
        private NonTerminal paramDcl = new NonTerminal("param_dcl");
        private NonTerminal compoundSt = new NonTerminal("compound_st");
        private NonTerminal optDclList = new NonTerminal("opt_dcl_list");
        private NonTerminal declarationList = new NonTerminal("declaration_list");
        private NonTerminal declaration = new NonTerminal("declaration");
        private NonTerminal initDclList = new NonTerminal("init_dcl_list");
        private NonTerminal initDeclarator = new NonTerminal("init_declarator");
        private NonTerminal declarator = new NonTerminal("declarator");
        private NonTerminal optNumber = new NonTerminal("opt_number");
        private NonTerminal optStatList = new NonTerminal("opt_stat_list");
        private NonTerminal statementList = new NonTerminal("statement_list");
        private NonTerminal statement = new NonTerminal("statement");
        private NonTerminal expressionSt = new NonTerminal("expression_st");
        private NonTerminal optExpression = new NonTerminal("opt_expression");
        private NonTerminal ifSt = new NonTerminal("if_st");
        private NonTerminal whileSt = new NonTerminal("while_st");
        private NonTerminal returnSt = new NonTerminal("return_st");
        private NonTerminal expression = new NonTerminal("expression");
        private NonTerminal assignmentExp = new NonTerminal("assignment_exp");
        private NonTerminal logicalOrExp = new NonTerminal("logical_or_exp");
        private NonTerminal logicalAndExp = new NonTerminal("logical_and_exp");
        private NonTerminal equalityExp = new NonTerminal("equality_exp");
        private NonTerminal relationalExp = new NonTerminal("relational_exp");
        private NonTerminal additiveExp = new NonTerminal("additive_exp");
        private NonTerminal multiplicativeExp = new NonTerminal("multiplicative_exp");
        private NonTerminal unaryExp = new NonTerminal("unary_exp");
        private NonTerminal postfixExp = new NonTerminal("postfix_exp");
        private NonTerminal optActualParam = new NonTerminal("opt_actual_param");
        private NonTerminal actualParam = new NonTerminal("actual_param");
        private NonTerminal actualParamList = new NonTerminal("actual_param_list");
        private NonTerminal primaryExp = new NonTerminal("primary_exp");

        public override Grammars.Sdts SDTS { get; }

        public MiniCGrammar()
        {
            this.ScopeInfos.Add(new ScopeInfo(this.scopeCommentStart, this.scopeCommentEnd));

            this.SDTS = new MiniCSdts(this.keyManager, this);
            var sdts = this.SDTS as MiniCSdts;

            this.miniC.AddItem(this.translationUnit, sdts.Program);
            this.translationUnit.AddItem(this.externalDcl | this.translationUnit + this.externalDcl);
            this.externalDcl.AddItem(this.functionDef | this.declaration);
            this.functionDef.AddItem(this.functionHeader + this.compoundSt, sdts.FuncDef);
            this.functionHeader.AddItem(this.dclSpec + this.functionName + this.formalParam, sdts.FuncHead);
            this.dclSpec.AddItem(this.dclSpecifiers, sdts.DclSpec);
            this.dclSpecifiers.AddItem(this.dclSpecifier | this.dclSpecifiers + this.dclSpecifier);
            this.dclSpecifier.AddItem(this.typeQualifier | this.typeSpecifier);
            this.typeQualifier.AddItem(this.Const, sdts.ConstNode);
            this.typeSpecifier.AddItem(this.Int, sdts.IntNode);
            this.typeSpecifier.AddItem(this.Void, sdts.VoidNode);
            this.functionName.AddItem(this.Ident);
            this.formalParam.AddItem(this.OpenParenthesis + this.optFormalParam + this.CloseParenthesis, sdts.FormalPara);
            this.optFormalParam.AddItem(this.formalParamList | new Epsilon());
            this.formalParamList.AddItem(this.paramDcl | this.formalParamList + this.Comma + this.paramDcl);
            this.paramDcl.AddItem(this.dclSpec + this.declarator, sdts.ParamDcl);
            this.compoundSt.AddItem(this.OpenCurlyBrace + this.optDclList + this.optStatList + this.CloseCurlyBrace, sdts.CompoundSt);
            this.optDclList.AddItem(this.declarationList | new Epsilon(), sdts.DclList);
            this.declarationList.AddItem(this.declaration | this.declarationList + this.declaration);
            this.declaration.AddItem(this.dclSpec + this.initDclList + this.SemiColon, sdts.Dcl);
            this.initDclList.AddItem(this.initDeclarator | this.initDclList + this.Comma + this.initDeclarator);

            this.initDeclarator.AddItem(this.declarator, sdts.DclItem);
            this.initDeclarator.AddItem(this.declarator + this.Assign + this.Number, sdts.DclItem);

            this.declarator.AddItem(this.Ident, sdts.SimpleVar);
            this.declarator.AddItem(this.Ident + this.OpenSquareBrace + this.optNumber + this.CloseSquareBrace, sdts.ArrayVar);

            this.optNumber.AddItem(this.Number | new Epsilon());
            this.optStatList.AddItem(this.statementList, sdts.StatList);
            this.optStatList.AddItem(new Epsilon());

            this.statementList.AddItem(this.statement | this.statementList + this.statement);
            this.statement.AddItem(this.compoundSt | this.expressionSt | this.ifSt | this.whileSt | this.returnSt);
            this.expressionSt.AddItem(this.optExpression + this.SemiColon, sdts.ExpSt);
            this.optExpression.AddItem(this.expression | new Epsilon());

            this.ifSt.AddItem(this.If + this.OpenParenthesis + this.expression + this.CloseParenthesis + this.statement, 1, sdts.IfSt);
            this.ifSt.AddItem(this.If + this.OpenParenthesis + this.expression + this.CloseParenthesis + this.statement + this.Else + this.statement, 0, sdts.IfElseSt);

            this.whileSt.AddItem(this.While + this.OpenParenthesis + this.expression + this.CloseParenthesis + this.statement, sdts.WhileSt);
            this.returnSt.AddItem(this.Return + this.optExpression + this.SemiColon, sdts.ReturnSt);
            this.expression.AddItem(this.assignmentExp);

            this.assignmentExp.AddItem(this.logicalOrExp);
            this.assignmentExp.AddItem(this.unaryExp + this.Assign + this.assignmentExp, sdts.Assign);
            this.assignmentExp.AddItem(this.unaryExp + this.AddAssign + this.assignmentExp, sdts.AddAssign);
            this.assignmentExp.AddItem(this.unaryExp + this.SubAssign + this.assignmentExp, sdts.SubAssign);
            this.assignmentExp.AddItem(this.unaryExp + this.MulAssign + this.assignmentExp, sdts.MulAssign);
            this.assignmentExp.AddItem(this.unaryExp + this.DivAssign + this.assignmentExp, sdts.DivAssign);
            this.assignmentExp.AddItem(this.unaryExp + this.ModAssign + this.assignmentExp, sdts.ModAssign);

            this.logicalOrExp.AddItem(this.logicalAndExp);
            this.logicalOrExp.AddItem(this.logicalOrExp + this.LogicalOr + this.logicalAndExp, sdts.LogicalOr);

            this.logicalAndExp.AddItem(this.equalityExp);
            this.logicalAndExp.AddItem(this.logicalAndExp + this.LogicalAnd + this.equalityExp, sdts.LogicalAnd);

            this.equalityExp.AddItem(this.relationalExp);
            this.equalityExp.AddItem(this.equalityExp + this.Equal + this.relationalExp, sdts.Equal);
            this.equalityExp.AddItem(this.equalityExp + this.NotEqual + this.relationalExp, sdts.NotEqual);

            this.relationalExp.AddItem(this.additiveExp);
            this.relationalExp.AddItem(this.relationalExp + this.GreaterThan + this.additiveExp, sdts.GreaterThan);
            this.relationalExp.AddItem(this.relationalExp + this.LessThan + this.additiveExp, sdts.LessThan);
            this.relationalExp.AddItem(this.relationalExp + this.GreaterEqual + this.additiveExp, sdts.GreatherEqual);

            this.additiveExp.AddItem(this.multiplicativeExp);
            this.additiveExp.AddItem(this.additiveExp + this.Add + this.multiplicativeExp, sdts.Add);
            this.additiveExp.AddItem(this.additiveExp + this.Sub + this.multiplicativeExp, sdts.Sub);

            this.multiplicativeExp.AddItem(this.unaryExp);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + this.Mul + this.unaryExp, sdts.Mul);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + this.Div + this.unaryExp, sdts.Div);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + this.Mod + this.unaryExp, sdts.Mod);

            this.unaryExp.AddItem(this.postfixExp);
            this.unaryExp.AddItem(this.Sub + this.unaryExp, sdts.UnaryMinus);
            this.unaryExp.AddItem(this.LogicalNot + this.unaryExp, sdts.LogicalNot);
            this.unaryExp.AddItem(this.Inc + this.unaryExp, sdts.PreInc);
            this.unaryExp.AddItem(this.Dec + this.unaryExp, sdts.PreDec);

            this.postfixExp.AddItem(this.primaryExp);
            this.postfixExp.AddItem(this.postfixExp + this.OpenSquareBrace + this.expression + this.CloseSquareBrace, sdts.Index);
            this.postfixExp.AddItem(this.postfixExp + this.OpenParenthesis + this.optActualParam + this.CloseParenthesis, sdts.Cell);
            this.postfixExp.AddItem(this.postfixExp + this.Inc, sdts.PostInc);
            this.postfixExp.AddItem(this.postfixExp + this.Dec, sdts.PostDec);

            this.optActualParam.AddItem(this.actualParam, sdts.ActualParam);
            this.optActualParam.AddItem(new Epsilon());

            this.actualParam.AddItem(this.actualParamList);
            this.actualParamList.AddItem(this.assignmentExp);
            this.actualParamList.AddItem(this.actualParamList + this.Comma + this.assignmentExp);

            this.primaryExp.AddItem(this.Ident | this.Number | this.OpenParenthesis + this.expression + this.CloseParenthesis);


            this.Optimization();

        }
    }
}
