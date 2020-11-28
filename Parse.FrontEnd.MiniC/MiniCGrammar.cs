using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.MiniC.Properties;
using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.MiniC
{
    public class MiniCGrammar : Grammar
    {
        public Terminal Using { get; } = new Terminal(TokenType.Keyword.CategoryKeyword, "using");
        public Terminal Namespace { get; } = new Terminal(TokenType.Keyword.CategoryKeyword, "namespace");
        public Terminal Struct { get; } = new Terminal(TokenType.Keyword.CategoryKeyword, "struct");
        public Terminal Class { get; } = new Terminal(TokenType.Keyword.CategoryKeyword, "class");
        public Terminal Private { get; } = new Terminal(TokenType.Keyword.Accessword, "private");
        public Terminal Public { get; } = new Terminal(TokenType.Keyword.Accessword, "public");
        public Terminal If { get; } = new Terminal(TokenType.Keyword.Controlword, "if");
        public Terminal Else { get; } = new Terminal(TokenType.Keyword.Controlword, "else");
        public Terminal While { get; } = new Terminal(TokenType.Keyword.Repeateword, "while");
        public Terminal Return { get; } = new Terminal(TokenType.Keyword.Controlword, "return");
        public Terminal Const { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "const");
        public static Terminal Char { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "char");
        public static Terminal Short { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "short");
        public static Terminal System { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "system");
        public static Terminal Int { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "int");
        public static Terminal Double { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "double");
        public static Terminal Address { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "address");
        public static Terminal Void { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "void");
        public static Terminal Ident { get; } = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", Resource.Ident, true, true);
        public static Terminal HexNumber { get; } = new Terminal(TokenType.Digit.Digit16, "0[xX][0-9a-fA-F]+", Resource.HexNumber, true, true);
        public static Terminal BinNumber { get; } = new Terminal(TokenType.Digit.Digit16, "0[bB][01]+", Resource.BinNumber, true, true);
        public static Terminal Number { get; } = new Terminal(TokenType.Digit.Digit10, "[0-9]+", Resource.DecimalNumber, true, true);
        //        public Terminal RealNumber { get; } = new Terminal(TokenType.Digit.Digit10, "[0-9]+.[0-9]+", "real number", true, true);
        public Terminal LineComment { get; } = new Terminal(TokenType.SpecialToken.Comment, "//.*$", false, true);

        public Terminal OpenParenthesis { get; } = new Terminal(TokenType.Operator.PairOpen, "(", false);
        public Terminal CloseParenthesis { get; } = new Terminal(TokenType.Operator.PairClose, ")", false);
        public Terminal OpenCurlyBrace { get; } = new Terminal(TokenType.Operator.PairOpen, "{", false);
        public Terminal CloseCurlyBrace { get; } = new Terminal(TokenType.Operator.PairClose, "}", false);
        public Terminal OpenSquareBrace { get; } = new Terminal(TokenType.Operator.PairOpen, "[", false);
        public Terminal CloseSquareBrace { get; } = new Terminal(TokenType.Operator.PairOpen, "]", false);

        private Terminal scopeCommentStart = new Terminal(TokenType.SpecialToken.Comment.ScopeComment, "/*");
        private Terminal scopeCommentEnd = new Terminal(TokenType.SpecialToken.Comment.ScopeComment, "*/");

        public Terminal Inc { get; } = new Terminal(TokenType.Operator.NormalOperator, "++", false);
        public Terminal Dec { get; } = new Terminal(TokenType.Operator.NormalOperator, "--", false);
        public Terminal Add { get; } = new Terminal(TokenType.Operator.NormalOperator, "+", false);
        public Terminal Sub { get; } = new Terminal(TokenType.Operator.NormalOperator, "-", false);
        public Terminal Mul { get; } = new Terminal(TokenType.Operator.NormalOperator, "*", false);
        public Terminal Div { get; } = new Terminal(TokenType.Operator.NormalOperator, "/", false);
        public Terminal Mod { get; } = new Terminal(TokenType.Operator.NormalOperator, "%", false);
        public Terminal Assign { get; } = new Terminal(TokenType.Operator.NormalOperator, "=", false);
        public Terminal AddAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "+=", false);
        public Terminal SubAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "-=", false);
        public Terminal MulAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "*=", false);
        public Terminal DivAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "/=", false);
        public Terminal ModAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "%=", false);
        public Terminal Equal { get; } = new Terminal(TokenType.Operator.NormalOperator, "==", false);
        public Terminal NotEqual { get; } = new Terminal(TokenType.Operator.NormalOperator, "!=", false);
        public Terminal GreaterThan { get; } = new Terminal(TokenType.Operator.NormalOperator, ">", false);
        public Terminal LessThan { get; } = new Terminal(TokenType.Operator.NormalOperator, "<", false);
        public Terminal GreaterEqual { get; } = new Terminal(TokenType.Operator.NormalOperator, ">=", false);
        public Terminal LessEqual { get; } = new Terminal(TokenType.Operator.NormalOperator, "<=", false);
        public Terminal SemiColon { get; } = new Terminal(TokenType.Operator.NormalOperator, ";", false);
        public Terminal Comma { get; } = new Terminal(TokenType.Operator.Comma, ",", false);

        public Terminal LogicalOr { get; } = new Terminal(TokenType.Operator, "||", false);
        public Terminal LogicalAnd { get; } = new Terminal(TokenType.Operator, "&&", false);
        public Terminal LogicalNot { get; } = new Terminal(TokenType.Operator, "!", false);


        private NonTerminal miniC = new NonTerminal("mini_c", true);
        private NonTerminal usingDcl = new NonTerminal("using_dcl");
        private NonTerminal namespaceDcl = new NonTerminal("namespace_dcl");
        private NonTerminal translationUnit = new NonTerminal("translation_unit");
        private NonTerminal externalDcl = new NonTerminal("external_dcl");
        private NonTerminal structDef = new NonTerminal("struct_def");
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
        private NonTerminal literalInt = new NonTerminal("opt_number");
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




        // These are meaning unit for semantic analysis.
        public static MeaningUnit Program { get; } = new MeaningUnit(nameof(Program));
        public static MeaningUnit UsingNode { get; } = new MeaningUnit(nameof(UsingNode));
        public static MeaningUnit NamespaceNode { get; } = new MeaningUnit(nameof(NamespaceNode));
        public static MeaningUnit StructDef { get; } = new MeaningUnit(nameof(StructDef));
        public static MeaningUnit FuncDef { get; } = new MeaningUnit(nameof(FuncDef));
        public static MeaningUnit FuncHead { get; } = new MeaningUnit(nameof(FuncHead), MatchedAction.BlockPlus);
        public static MeaningUnit DclSpec { get; } = new MeaningUnit(nameof(DclSpec));
        public static MeaningUnit ConstNode { get; } = new MeaningUnit(nameof(ConstNode));
        public static MeaningUnit AddressNode { get; } = new MeaningUnit(nameof(AddressNode));
        public static MeaningUnit CharNode { get; } = new MeaningUnit(nameof(CharNode));
        public static MeaningUnit ShortNode { get; } = new MeaningUnit(nameof(ShortNode));
        public static MeaningUnit SystemNode { get; } = new MeaningUnit(nameof(SystemNode));
        public static MeaningUnit IntNode { get; } = new MeaningUnit(nameof(IntNode));
        public static MeaningUnit DoubleNode { get; } = new MeaningUnit(nameof(DoubleNode));
        public static MeaningUnit VoidNode { get; } = new MeaningUnit(nameof(VoidNode));
        public static MeaningUnit FormalPara { get; } = new MeaningUnit(nameof(FormalPara));
        public static MeaningUnit ParamDcl { get; } = new MeaningUnit(nameof(ParamDcl));
        public static MeaningUnit CompoundSt { get; } = new MeaningUnit(nameof(CompoundSt));
        public static MeaningUnit DclList { get; } = new MeaningUnit(nameof(DclList));
        public static MeaningUnit Dcl { get; } = new MeaningUnit(nameof(Dcl));
        public static MeaningUnit DclVar { get; } = new MeaningUnit(nameof(DclVar), MatchedAction.OffsetPlus);
        public static MeaningUnit DeclareVarIdent { get; } = new MeaningUnit(nameof(DeclareVarIdent));
        public static MeaningUnit StatList { get; } = new MeaningUnit(nameof(StatList));
        public static MeaningUnit ExpSt { get; } = new MeaningUnit(nameof(ExpSt));
        public static MeaningUnit IfSt { get; } = new MeaningUnit(nameof(IfSt));
        public static MeaningUnit IfElseSt { get; } = new MeaningUnit(nameof(IfElseSt));
        public static MeaningUnit WhileSt { get; } = new MeaningUnit(nameof(WhileSt));
        public static MeaningUnit ReturnSt { get; } = new MeaningUnit(nameof(ReturnSt));
        public static MeaningUnit Index { get; } = new MeaningUnit(nameof(Index));
        public static MeaningUnit Call { get; } = new MeaningUnit(nameof(Call));
        public static MeaningUnit ActualParam { get; } = new MeaningUnit(nameof(ActualParam), MatchedAction.OffsetPlus);
        public static MeaningUnit DeRef { get; } = new MeaningUnit(nameof(DeRef));
        public static MeaningUnit UseVar { get; } = new MeaningUnit(nameof(UseVar));
        public static MeaningUnit IntLiteralNode { get; } = new MeaningUnit(nameof(IntLiteralNode));


        public static MeaningUnit AddM { get; } = new MeaningUnit(nameof(AddM));
        public static MeaningUnit SubM { get; } = new MeaningUnit(nameof(SubM));
        public static MeaningUnit MulM { get; } = new MeaningUnit(nameof(MulM));
        public static MeaningUnit DivM { get; } = new MeaningUnit(nameof(DivM));
        public static MeaningUnit ModM { get; } = new MeaningUnit(nameof(ModM));
        public static MeaningUnit AssignM { get; } = new MeaningUnit(nameof(AssignM));
        public static MeaningUnit AddAssignM { get; } = new MeaningUnit(nameof(AddAssignM));
        public static MeaningUnit SubAssignM { get; } = new MeaningUnit(nameof(SubAssignM));
        public static MeaningUnit MulAssignM { get; } = new MeaningUnit(nameof(MulAssignM));
        public static MeaningUnit DivAssignM { get; } = new MeaningUnit(nameof(DivAssignM));
        public static MeaningUnit ModAssignM { get; } = new MeaningUnit(nameof(ModAssignM));
        public static MeaningUnit LogicalOrM { get; } = new MeaningUnit(nameof(LogicalOrM));
        public static MeaningUnit LogicalAndM { get; } = new MeaningUnit(nameof(LogicalAndM));
        public static MeaningUnit LogicalNotM { get; } = new MeaningUnit(nameof(LogicalNotM));
        public static MeaningUnit EqualM { get; } = new MeaningUnit(nameof(EqualM));
        public static MeaningUnit NotEqualM { get; } = new MeaningUnit(nameof(NotEqualM));
        public static MeaningUnit GreaterThanM { get; } = new MeaningUnit(nameof(GreaterThanM));
        public static MeaningUnit LessThanM { get; } = new MeaningUnit(nameof(LessThanM));
        public static MeaningUnit GreaterEqualM { get; } = new MeaningUnit(nameof(GreaterEqualM));
        public static MeaningUnit LessEqualM { get; } = new MeaningUnit(nameof(LessEqualM));
        public static MeaningUnit UnaryMinusM { get; } = new MeaningUnit(nameof(UnaryMinusM));
        public static MeaningUnit PreIncM { get; } = new MeaningUnit(nameof(PreIncM));
        public static MeaningUnit PreDecM { get; } = new MeaningUnit(nameof(PreDecM));
        public static MeaningUnit PostIncM { get; } = new MeaningUnit(nameof(PostIncM));
        public static MeaningUnit PostDecM { get; } = new MeaningUnit(nameof(PostDecM));


        public override NonTerminal EbnfRoot => this.miniC;

        public MiniCGrammar()
        {
            this.ScopeInfos.Add(new ScopeInfo(this.scopeCommentStart, this.scopeCommentEnd));

            this.miniC.AddItem(usingDcl.ZeroOrMore() | namespaceDcl, Program);
            this.usingDcl.AddItem(Using + Ident + SemiColon, UsingNode);
            this.namespaceDcl.AddItem(Namespace + Ident + OpenCurlyBrace + translationUnit + CloseCurlyBrace, NamespaceNode);
            this.translationUnit.AddItem(this.externalDcl | this.translationUnit + this.externalDcl);
            this.externalDcl.AddItem(structDef | functionDef | declaration);
            this.structDef.AddItem(Struct + Ident + OpenCurlyBrace + declaration + CloseCurlyBrace, StructDef);
            this.functionDef.AddItem(this.functionHeader + this.compoundSt, FuncDef);
            this.functionHeader.AddItem(this.dclSpec + this.functionName + this.formalParam, FuncHead);
            this.dclSpec.AddItem(this.dclSpecifiers, DclSpec);
            this.dclSpecifiers.AddItem(this.typeQualifier.Optional() + this.typeSpecifier);
            this.typeQualifier.AddItem(this.Const, ConstNode);

            this.typeSpecifier.AddItem(Address, AddressNode);
            this.typeSpecifier.AddItem(Char, CharNode);
            this.typeSpecifier.AddItem(Short, ShortNode);
            this.typeSpecifier.AddItem(System, SystemNode);
            this.typeSpecifier.AddItem(Int, IntNode);
            this.typeSpecifier.AddItem(Double, DoubleNode);
            this.typeSpecifier.AddItem(Void, VoidNode);

            this.functionName.AddItem(Ident);
            this.formalParam.AddItem(this.OpenParenthesis + this.optFormalParam + this.CloseParenthesis, FormalPara);
            this.optFormalParam.AddItem(this.formalParamList | new Epsilon());
            this.formalParamList.AddItem(this.paramDcl | this.formalParamList + this.Comma + this.paramDcl);
            this.paramDcl.AddItem(this.dclSpec + this.declarator, ParamDcl);
            this.compoundSt.AddItem(this.OpenCurlyBrace + this.optDclList + this.optStatList + this.CloseCurlyBrace, CompoundSt);
            this.optDclList.AddItem(this.declarationList | new Epsilon(), DclList);
            this.declarationList.AddItem(this.declaration | this.declarationList + this.declaration);
            this.declaration.AddItem(this.dclSpec + this.initDclList + this.SemiColon, Dcl);
            this.initDclList.AddItem(this.initDeclarator | this.initDclList + this.Comma + this.initDeclarator);

            this.initDeclarator.AddItem(this.declarator + (this.Assign + this.expression).Optional(), DclVar);

            this.declarator.AddItem(Ident, DeclareVarIdent);
            this.declarator.AddItem(Ident + this.OpenSquareBrace + this.literalInt + this.CloseSquareBrace, DeclareVarIdent);

            this.literalInt.AddItem(Number | HexNumber, IntLiteralNode);
            this.optStatList.AddItem(this.statementList | new Epsilon(), StatList);

            this.statementList.AddItem(this.statement | this.statementList + this.statement);
            this.statement.AddItem(this.compoundSt | this.expressionSt | this.ifSt | this.whileSt | this.returnSt);
            this.expressionSt.AddItem(this.optExpression + this.SemiColon, ExpSt);
            this.optExpression.AddItem(this.expression | new Epsilon());

            this.ifSt.AddItem(this.If + this.OpenParenthesis + this.expression + this.CloseParenthesis + this.statement, 1, IfSt);
            this.ifSt.AddItem(this.If + this.OpenParenthesis + this.expression + this.CloseParenthesis + this.statement + this.Else + this.statement, 0, IfElseSt);

            this.whileSt.AddItem(this.While + this.OpenParenthesis + this.expression + this.CloseParenthesis + this.statement, WhileSt);
            this.returnSt.AddItem(this.Return + this.expressionSt, ReturnSt);
            this.expression.AddItem(this.assignmentExp);

            this.assignmentExp.AddItem(this.logicalOrExp);
            this.assignmentExp.AddItem(this.assignmentExp + this.Assign + this.assignmentExp, AssignM);
            this.assignmentExp.AddItem(this.assignmentExp + this.AddAssign + this.assignmentExp, AddAssignM);
            this.assignmentExp.AddItem(this.assignmentExp + this.SubAssign + this.assignmentExp, SubAssignM);
            this.assignmentExp.AddItem(this.assignmentExp + this.MulAssign + this.assignmentExp, MulAssignM);
            this.assignmentExp.AddItem(this.assignmentExp + this.DivAssign + this.assignmentExp, DivAssignM);
            this.assignmentExp.AddItem(this.assignmentExp + this.ModAssign + this.assignmentExp, ModAssignM);

            this.logicalOrExp.AddItem(this.logicalAndExp);
            this.logicalOrExp.AddItem(this.logicalOrExp + this.LogicalOr + this.logicalAndExp, LogicalOrM);

            this.logicalAndExp.AddItem(this.equalityExp);
            this.logicalAndExp.AddItem(this.logicalAndExp + this.LogicalAnd + this.equalityExp, LogicalAndM);

            this.equalityExp.AddItem(this.relationalExp);
            this.equalityExp.AddItem(this.equalityExp + this.Equal + this.relationalExp, EqualM);
            this.equalityExp.AddItem(this.equalityExp + this.NotEqual + this.relationalExp, NotEqualM);

            this.relationalExp.AddItem(this.additiveExp);
            this.relationalExp.AddItem(this.relationalExp + this.GreaterThan + this.additiveExp, GreaterThanM);
            this.relationalExp.AddItem(this.relationalExp + this.LessThan + this.additiveExp, LessThanM);
            this.relationalExp.AddItem(this.relationalExp + this.GreaterEqual + this.additiveExp, GreaterEqualM);
            this.relationalExp.AddItem(this.relationalExp + this.LessEqual + this.additiveExp, LessEqualM);

            this.additiveExp.AddItem(this.multiplicativeExp);
            this.additiveExp.AddItem(this.additiveExp + this.Add + this.multiplicativeExp, AddM);
            this.additiveExp.AddItem(this.additiveExp + this.Sub + this.multiplicativeExp, SubM);

            this.multiplicativeExp.AddItem(this.unaryExp);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + this.Mul + this.unaryExp, MulM);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + this.Div + this.unaryExp, DivM);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + this.Mod + this.unaryExp, ModM);

            this.unaryExp.AddItem(this.postfixExp);
            this.unaryExp.AddItem(this.Sub + this.unaryExp, UnaryMinusM);
            this.unaryExp.AddItem(this.LogicalNot + this.unaryExp, LogicalNotM);
            this.unaryExp.AddItem(this.Inc + this.unaryExp, PreIncM);
            this.unaryExp.AddItem(this.Dec + this.unaryExp, PreDecM);

            this.postfixExp.AddItem(this.primaryExp);
            this.postfixExp.AddItem(this.postfixExp + this.OpenSquareBrace + this.expression + this.CloseSquareBrace, Index);
            this.postfixExp.AddItem(this.postfixExp + this.OpenParenthesis + this.optActualParam.Optional() + this.CloseParenthesis, Call);
            this.postfixExp.AddItem(this.postfixExp + this.Inc, PostIncM);
            this.postfixExp.AddItem(this.postfixExp + this.Dec, PostDecM);
            this.postfixExp.AddItem(Mul + this.postfixExp, DeRef);

            this.optActualParam.AddItem(this.actualParam, ActualParam);

            this.actualParam.AddItem(this.actualParamList);
            this.actualParamList.AddItem(this.logicalOrExp);
            this.actualParamList.AddItem(this.actualParamList + this.Comma + this.logicalOrExp);

            this.primaryExp.AddItem(Ident, UseVar);
            this.primaryExp.AddItem(this.literalInt);
            this.primaryExp.AddItem(this.OpenParenthesis + this.expression + this.CloseParenthesis);


            this.Optimization();

        }
    }
}
