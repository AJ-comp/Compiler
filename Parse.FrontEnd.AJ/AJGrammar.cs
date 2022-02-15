using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.AJ
{
    public class AJGrammar : Grammar
    {
        public Terminal Using { get; } = new Terminal(TokenType.Keyword.CategoryKeyword, "using", false);
        public Terminal Namespace { get; } = new Terminal(TokenType.Keyword.CategoryKeyword, "namespace", false);
        public Terminal Struct { get; } = new Terminal(TokenType.Keyword.CategoryKeyword, "struct", false);
        public Terminal Class { get; } = new Terminal(TokenType.Keyword.CategoryKeyword, "class", false);
        public static Terminal Private { get; } = new Terminal(TokenType.Keyword.Accessword, "private");
        public static Terminal Public { get; } = new Terminal(TokenType.Keyword.Accessword, "public");
        public static Terminal This { get; } = new Terminal(TokenType.Keyword, "this");
        public static Terminal New { get; } = new Terminal(TokenType.Keyword, "new");
        public Terminal If { get; } = new Terminal(TokenType.Keyword.Controlword, "if");
        public Terminal Else { get; } = new Terminal(TokenType.Keyword.Controlword, "else");
        public Terminal While { get; } = new Terminal(TokenType.Keyword.Repeateword, "while");
        public Terminal Return { get; } = new Terminal(TokenType.Keyword.Controlword, "return");
        public static Terminal Const { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "const");
        public static Terminal Bool { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "bool");
        public static Terminal Byte { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "byte");
        public static Terminal SByte { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "sbyte");
        public static Terminal Char { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "char");
        public static Terminal Short { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "short");
        public static Terminal UShort { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "ushort");
        public static Terminal System { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "system");
        public static Terminal Int { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "int");
        public static Terminal UInt { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "uint");
        public static Terminal Double { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "double");
        public static Terminal Void { get; } = new Terminal(TokenType.Keyword.DefinedDataType, "void");
        public static Terminal Ident { get; } = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", Resource.Ident, true, true);
        public static Terminal HexNumber { get; } = new Terminal(TokenType.Literal.Digit16, "0[xX][0-9a-fA-F]+", Resource.HexNumber, true, true);
        public static Terminal BinNumber { get; } = new Terminal(TokenType.Literal.Digit16, "0[bB][01]+", Resource.BinNumber, true, true);
        public static Terminal Number { get; } = new Terminal(TokenType.Literal.Digit10, "-?[0-9]+", Resource.DecimalNumber, true, true);
        public static Terminal RealNumber { get; } = new Terminal(TokenType.Literal.Digit10, @"-?[0-9]+\.[0-9]+", Resource.RealNumber, true, true);
        public static Terminal BoolTrue { get; } = new Terminal(TokenType.Literal.Bool, "true");
        public static Terminal BoolFalse { get; } = new Terminal(TokenType.Literal.Bool, "false");
        public Terminal LineComment { get; } = new Terminal(TokenType.SpecialToken.Comment, "//.*$", false, true);

        public Terminal OpenParenthesis { get; } = new Terminal(TokenType.Operator.PairOpen, "(", false);
        public Terminal CloseParenthesis { get; } = new Terminal(TokenType.Operator.PairClose, ")", false);
        public Terminal OpenCurlyBrace { get; } = new Terminal(TokenType.Operator.PairOpen, "{", false);
        public Terminal CloseCurlyBrace { get; } = new Terminal(TokenType.Operator.PairClose, "}", false);
        public Terminal OpenSquareBrace { get; } = new Terminal(TokenType.Operator.PairOpen, "[", false);
        public Terminal CloseSquareBrace { get; } = new Terminal(TokenType.Operator.PairOpen, "]", false);

        public Terminal ArraySymbol { get; } = new Terminal(TokenType.Operator.ArraySymbol, "[]", true);

        private Terminal scopeCommentStart = new Terminal(TokenType.SpecialToken.Comment.ScopeComment, "/*");
        private Terminal scopeCommentEnd = new Terminal(TokenType.SpecialToken.Comment.ScopeComment, "*/");

        public static Terminal Inc { get; } = new Terminal(TokenType.Operator.NormalOperator, "++", false);
        public static Terminal Dec { get; } = new Terminal(TokenType.Operator.NormalOperator, "--", false);
        public Terminal Add { get; } = new Terminal(TokenType.Operator.NormalOperator, "+", false);
        public Terminal Sub { get; } = new Terminal(TokenType.Operator.NormalOperator, "-", false);
        public static Terminal Mul { get; } = new Terminal(TokenType.Operator.NormalOperator, "*", false);
        public Terminal Div { get; } = new Terminal(TokenType.Operator.NormalOperator, "/", false);
        public Terminal Mod { get; } = new Terminal(TokenType.Operator.NormalOperator, "%", false);
        public Terminal Assign { get; } = new Terminal(TokenType.Operator.NormalOperator, "=", false);
        public Terminal AddAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "+=", false);
        public Terminal SubAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "-=", false);
        public Terminal MulAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "*=", false);
        public Terminal DivAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "/=", false);
        public Terminal ModAssign { get; } = new Terminal(TokenType.Operator.NormalOperator, "%=", false);
        public static Terminal Equal { get; } = new Terminal(TokenType.Operator.NormalOperator, "==", false);
        public static Terminal NotEqual { get; } = new Terminal(TokenType.Operator.NormalOperator, "!=", false);
        public static Terminal GreaterThan { get; } = new Terminal(TokenType.Operator.NormalOperator, ">", false);
        public static Terminal LessThan { get; } = new Terminal(TokenType.Operator.NormalOperator, "<", false);
        public static Terminal GreaterEqual { get; } = new Terminal(TokenType.Operator.NormalOperator, ">=", false);
        public static Terminal LessEqual { get; } = new Terminal(TokenType.Operator.NormalOperator, "<=", false);
        public Terminal SemiColon { get; } = new Terminal(TokenType.Operator.NormalOperator, ";", false);
        public Terminal Comma { get; } = new Terminal(TokenType.Operator.Comma, ",", false);
        public Terminal Dot { get; } = new Terminal(TokenType.Operator, ".", false);

        public Terminal LogicalOr { get; } = new Terminal(TokenType.Operator, "||", false);
        public Terminal LogicalAnd { get; } = new Terminal(TokenType.Operator, "&&", false);
        public Terminal LogicalNot { get; } = new Terminal(TokenType.Operator, "!", false);


        private NonTerminal ajProgram = new NonTerminal("mini_c", true);
        private NonTerminal usingDcl = new NonTerminal("using_dcl");
        private NonTerminal namespaceDcl = new NonTerminal(nameof(namespaceDcl));
        private NonTerminal namespaceMemberDcl = new NonTerminal(nameof(namespaceMemberDcl));
        private NonTerminal accesser = new NonTerminal(nameof(accesser));

        private NonTerminal structDef = new NonTerminal(nameof(structDef));
        private NonTerminal classDef = new NonTerminal(nameof(classDef));

        private NonTerminal classMemberDcl = new NonTerminal(nameof(classMemberDcl));
        private NonTerminal memberFieldDef = new NonTerminal(nameof(memberFieldDef));
        private NonTerminal memberPropDef = new NonTerminal(nameof(memberPropDef));
        private NonTerminal memberFuncDef = new NonTerminal(nameof(memberFuncDef));

        private NonTerminal translationUnit = new NonTerminal("translation_unit");
        private NonTerminal externalDcl = new NonTerminal("external_dcl");
        private NonTerminal functionDef = new NonTerminal("function_def");
        private NonTerminal typeSpecifier = new NonTerminal("type_specifier");
        private NonTerminal functionName = new NonTerminal("function_name");
        private NonTerminal formalParam = new NonTerminal("formal_param");
        private NonTerminal formalParamList = new NonTerminal("formal_param_list");
        private NonTerminal compoundSt = new NonTerminal(nameof(compoundSt));
        private NonTerminal declareVarSt = new NonTerminal(nameof(declareVarSt));
        private NonTerminal declarationList = new NonTerminal("declaration_list");
        private NonTerminal defName = new NonTerminal(nameof(defName));
        private NonTerminal memberCommon = new NonTerminal(nameof(memberCommon));
        private NonTerminal declaratorVar = new NonTerminal("declarator");
        private NonTerminal declaratorIdent = new NonTerminal(nameof(declaratorIdent));
        private NonTerminal literalInt = new NonTerminal("opt_number");
        private NonTerminal literalDouble = new NonTerminal(nameof(literalDouble));
        private NonTerminal literalBool = new NonTerminal("opt_bool");
        private NonTerminal statementList = new NonTerminal("statement_list");
        private NonTerminal statement = new NonTerminal("statement");
        private NonTerminal expressionSt = new NonTerminal("expression_st");
        private NonTerminal optExpression = new NonTerminal("opt_expression");
        private NonTerminal ifSt = new NonTerminal("if_st");
        private NonTerminal whileSt = new NonTerminal("while_st");
        private NonTerminal returnSt = new NonTerminal("return_st");
        private NonTerminal expression = new NonTerminal("expression");
        private NonTerminal assignmentExp = new NonTerminal("assignment_exp");
        private NonTerminal newAssignmentExp = new NonTerminal(nameof(newAssignmentExp));
        private NonTerminal logicalOrExp = new NonTerminal("logical_or_exp");
        private NonTerminal logicalAndExp = new NonTerminal("logical_and_exp");
        private NonTerminal equalityExp = new NonTerminal("equality_exp");
        private NonTerminal relationalExp = new NonTerminal("relational_exp");
        private NonTerminal additiveExp = new NonTerminal("additive_exp");
        private NonTerminal multiplicativeExp = new NonTerminal("multiplicative_exp");
        private NonTerminal unaryExp = new NonTerminal("unary_exp");
        private NonTerminal postfixExp = new NonTerminal("postfix_exp");
        private NonTerminal callExp = new NonTerminal(nameof(callExp));
        private NonTerminal optActualParam = new NonTerminal("opt_actual_param");
        private NonTerminal actualParam = new NonTerminal("actual_param");
        private NonTerminal actualParamList = new NonTerminal("actual_param_list");
        private NonTerminal primaryExp = new NonTerminal("primary_exp");
        private NonTerminal identChainExp = new NonTerminal(nameof(identChainExp));




        // These are meaning unit for semantic analysis.
        public static MeaningUnit Program { get; } = new MeaningUnit(nameof(Program));
        public static MeaningUnit AccesserNode { get; } = new MeaningUnit(nameof(AccesserNode));
        public static MeaningUnit UsingNode { get; } = new MeaningUnit(nameof(UsingNode));
        public static MeaningUnit NamespaceNode { get; } = new MeaningUnit(nameof(NamespaceNode));

        public static MeaningUnit StructDef { get; } = new MeaningUnit(nameof(StructDef));
        public static MeaningUnit ClassDef { get; } = new MeaningUnit(nameof(ClassDef));
        public static MeaningUnit MemberFunc { get; } = new MeaningUnit(nameof(MemberFunc));

        public static MeaningUnit FuncDef { get; } = new MeaningUnit(nameof(FuncDef));
        //        public static MeaningUnit FuncHead { get; } = new MeaningUnit(nameof(FuncHead), MatchedAction.BlockPlus);

        public static MeaningUnit DefNameNode { get; } = new MeaningUnit(nameof(DefNameNode));
        public static MeaningUnit ConstNode { get; } = new MeaningUnit(nameof(ConstNode));
        public static MeaningUnit BoolNode { get; } = new MeaningUnit(nameof(BoolNode));
        public static MeaningUnit ByteNode { get; } = new MeaningUnit(nameof(ByteNode));
        public static MeaningUnit SByteNode { get; } = new MeaningUnit(nameof(SByteNode));
        public static MeaningUnit CharNode { get; } = new MeaningUnit(nameof(CharNode));
        public static MeaningUnit ShortNode { get; } = new MeaningUnit(nameof(ShortNode));
        public static MeaningUnit UShortNode { get; } = new MeaningUnit(nameof(UShortNode));
        public static MeaningUnit SystemNode { get; } = new MeaningUnit(nameof(SystemNode));
        public static MeaningUnit IntNode { get; } = new MeaningUnit(nameof(IntNode));
        public static MeaningUnit UIntNode { get; } = new MeaningUnit(nameof(UIntNode));
        public static MeaningUnit DoubleNode { get; } = new MeaningUnit(nameof(DoubleNode));
        public static MeaningUnit VoidNode { get; } = new MeaningUnit(nameof(VoidNode));
        public static MeaningUnit UserDefTypeNode { get; } = new MeaningUnit(nameof(UserDefTypeNode));
        public static MeaningUnit FormalPara { get; } = new MeaningUnit(nameof(FormalPara));
        public static MeaningUnit CompoundSt { get; } = new MeaningUnit(nameof(CompoundSt));
        public static MeaningUnit Dcl { get; } = new MeaningUnit(nameof(Dcl));
        public static MeaningUnit DclVar { get; } = new MeaningUnit(nameof(DclVar), MatchedAction.OffsetPlus);
        public static MeaningUnit DeclareVar { get; } = new MeaningUnit(nameof(DeclareVar));
        public static MeaningUnit DeclareIdent { get; } = new MeaningUnit(nameof(DeclareIdent));
        public static MeaningUnit StatList { get; } = new MeaningUnit(nameof(StatList));
        public static MeaningUnit ExpSt { get; } = new MeaningUnit(nameof(ExpSt));
        public static MeaningUnit IfSt { get; } = new MeaningUnit(nameof(IfSt));
        public static MeaningUnit IfElseSt { get; } = new MeaningUnit(nameof(IfElseSt));
        public static MeaningUnit WhileSt { get; } = new MeaningUnit(nameof(WhileSt));
        public static MeaningUnit ReturnSt { get; } = new MeaningUnit(nameof(ReturnSt));
        public static MeaningUnit DeclareVarSt { get; } = new MeaningUnit(nameof(DeclareVarSt));
        public static MeaningUnit Index { get; } = new MeaningUnit(nameof(Index));
        public static MeaningUnit Call { get; } = new MeaningUnit(nameof(Call));
        public static MeaningUnit ActualParam { get; } = new MeaningUnit(nameof(ActualParam), MatchedAction.OffsetPlus);
        public static MeaningUnit DeRef { get; } = new MeaningUnit(nameof(DeRef));
        public static MeaningUnit UseVar { get; } = new MeaningUnit(nameof(UseVar));
        public static MeaningUnit IntLiteralNode { get; } = new MeaningUnit(nameof(IntLiteralNode));
        public static MeaningUnit DoubleLiteralNode { get; } = new MeaningUnit(nameof(DoubleLiteralNode));
        public static MeaningUnit BoolLiteralNode { get; } = new MeaningUnit(nameof(BoolLiteralNode));


        public static MeaningUnit AddM { get; } = new MeaningUnit(nameof(AddM));
        public static MeaningUnit SubM { get; } = new MeaningUnit(nameof(SubM));
        public static MeaningUnit MulM { get; } = new MeaningUnit(nameof(MulM));
        public static MeaningUnit DivM { get; } = new MeaningUnit(nameof(DivM));
        public static MeaningUnit ModM { get; } = new MeaningUnit(nameof(ModM));
        public static MeaningUnit NewAssignM { get; } = new MeaningUnit(nameof(NewAssignM));
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


        public override NonTerminal EbnfRoot => this.ajProgram;

        public AJGrammar()
        {
            this.ScopeInfos.Add(new ScopeInfo(this.scopeCommentStart, this.scopeCommentEnd));

            this.ajProgram.AddItem(usingDcl.ZeroOrMore() + namespaceDcl, Program);
            this.usingDcl.AddItem(Using + identChainExp + SemiColon, UsingNode);

            // namespace
            this.accesser.AddItem(Private | Public, AccesserNode);
            this.namespaceDcl.AddItem(Namespace + defName + (Dot + defName).ZeroOrMore() + SemiColon + namespaceMemberDcl.ZeroOrMore(), NamespaceNode);
            this.namespaceMemberDcl.AddItem(structDef | classDef);

            // struct and class def
            this.structDef.AddItem(accesser.Optional() + Struct + defName + OpenCurlyBrace + declareVarSt + CloseCurlyBrace, StructDef);
            this.classDef.AddItem(accesser.Optional() + Class + defName + OpenCurlyBrace + classMemberDcl.ZeroOrMore() + CloseCurlyBrace, ClassDef);
            this.classMemberDcl.AddItem(accesser.Optional() + (declareVarSt  | functionDef));
            this.functionDef.AddItem(Const.Optional() + typeSpecifier + defName + formalParam + compoundSt, FuncDef);
            this.formalParam.AddItem(OpenParenthesis + formalParamList.Optional() + CloseParenthesis, FormalPara);
            this.formalParamList.AddItem(declaratorVar | formalParamList + Comma + declaratorVar);

            this.identChainExp.AddItem(Ident + (Dot + identChainExp).ZeroOrMore());

            this.typeSpecifier.AddItem(Bool, BoolNode);
            this.typeSpecifier.AddItem(Byte, ByteNode);
            this.typeSpecifier.AddItem(Char, CharNode);
            this.typeSpecifier.AddItem(Short, ShortNode);
            this.typeSpecifier.AddItem(UShort, ShortNode);
            this.typeSpecifier.AddItem(System, SystemNode);
            this.typeSpecifier.AddItem(Int, IntNode);
            this.typeSpecifier.AddItem(UInt, UIntNode);
            this.typeSpecifier.AddItem(Double, DoubleNode);
            this.typeSpecifier.AddItem(Void, VoidNode);
            this.typeSpecifier.AddItem(Ident, UserDefTypeNode);

            this.defName.AddItem(Ident, DefNameNode);
            this.compoundSt.AddItem(this.OpenCurlyBrace + statementList.ZeroOrMore() + this.CloseCurlyBrace, CompoundSt);
//            this.declaration.AddItem(this.declaratorVar + this.SemiColon, Dcl);
//            this.initDclList.AddItem(this.initDeclarator | this.initDclList + this.Comma + this.initDeclarator);

            this.declaratorVar.AddItem(Const.Optional() + typeSpecifier + defName + (Assign + expression).Optional(), DeclareVar);

            this.literalInt.AddItem(Number | HexNumber, IntLiteralNode);
            this.literalDouble.AddItem(RealNumber, DoubleLiteralNode);
            this.literalBool.AddItem(BoolTrue | BoolFalse, BoolLiteralNode);
//            this.optStatList.AddItem(this.statementList | new Epsilon(), StatList);

            this.statementList.AddItem(this.statement | this.statementList + this.statement);
            this.statement.AddItem(compoundSt | declareVarSt | expressionSt | ifSt | whileSt | returnSt);
            this.expressionSt.AddItem(expression.Optional() + this.SemiColon, ExpSt);

            this.ifSt.AddItem(this.If + this.OpenParenthesis + this.expression + this.CloseParenthesis + this.statement, 1, IfSt);
            this.ifSt.AddItem(this.If + this.OpenParenthesis + this.expression + this.CloseParenthesis + this.statement + this.Else + this.statement, 0, IfElseSt);

            this.whileSt.AddItem(this.While + this.OpenParenthesis + this.expression + this.CloseParenthesis + this.statement, WhileSt);
            this.returnSt.AddItem(this.Return + expression.Optional() + SemiColon, ReturnSt);
            this.declareVarSt.AddItem(declaratorVar + SemiColon, DeclareVarSt);
//            this.expression.AddItem(this.newAssignmentExp);
            this.expression.AddItem(this.assignmentExp);

//            this.newAssignmentExp.AddItem(this.postfixExp + this.Assign + this.newAssignmentExp);
//            this.newAssignmentExp.AddItem(New + callExp, NewAssignM);

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
            this.equalityExp.AddItem(this.equalityExp + Equal + this.relationalExp, EqualM);
            this.equalityExp.AddItem(this.equalityExp + NotEqual + this.relationalExp, NotEqualM);

            this.relationalExp.AddItem(this.additiveExp);
            this.relationalExp.AddItem(this.relationalExp + GreaterThan + this.additiveExp, GreaterThanM);
            this.relationalExp.AddItem(this.relationalExp + LessThan + this.additiveExp, LessThanM);
            this.relationalExp.AddItem(this.relationalExp + GreaterEqual + this.additiveExp, GreaterEqualM);
            this.relationalExp.AddItem(this.relationalExp + LessEqual + this.additiveExp, LessEqualM);

            this.additiveExp.AddItem(this.multiplicativeExp);
            this.additiveExp.AddItem(this.additiveExp + this.Add + this.multiplicativeExp, AddM);
            this.additiveExp.AddItem(this.additiveExp + this.Sub + this.multiplicativeExp, SubM);

            this.multiplicativeExp.AddItem(this.unaryExp);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + Mul + this.unaryExp, MulM);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + Div + this.unaryExp, DivM);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + Mod + this.unaryExp, ModM);

            this.unaryExp.AddItem(this.callExp);
            this.unaryExp.AddItem(this.Sub + this.unaryExp, UnaryMinusM);
            this.unaryExp.AddItem(this.LogicalNot + this.unaryExp, LogicalNotM);
            this.unaryExp.AddItem(Inc + this.unaryExp, PreIncM);
            this.unaryExp.AddItem(Dec + this.unaryExp, PreDecM);

            this.callExp.AddItem(postfixExp);
            this.callExp.AddItem(postfixExp + OpenParenthesis + optActualParam.Optional() + CloseParenthesis, Call);

            this.postfixExp.AddItem(primaryExp);
            this.postfixExp.AddItem(postfixExp + OpenSquareBrace + expression + CloseSquareBrace, Index);
            this.postfixExp.AddItem(postfixExp + Inc, PostIncM);
            this.postfixExp.AddItem(postfixExp + Dec, PostDecM);
            this.postfixExp.AddItem(Mul + postfixExp, DeRef);

            this.optActualParam.AddItem(this.actualParam, ActualParam);

            this.actualParam.AddItem(this.actualParamList);
            this.actualParamList.AddItem(this.logicalOrExp);
            this.actualParamList.AddItem(this.actualParamList + this.Comma + this.logicalOrExp);

            this.primaryExp.AddItem(Ident + (Dot + Ident).ZeroOrMore(), UseVar);
            this.primaryExp.AddItem(this.literalInt);
            this.primaryExp.AddItem(this.literalDouble);
            this.primaryExp.AddItem(this.literalBool);

            // https://lucid.app/lucidchart/8a4c2427-e77c-4dc2-bfc3-a52a25eac791/edit?beaconFlowId=77C0A73F5DE9BDDD&invitationId=inv_d331b441-8997-489c-b184-7c28b3a411b7&page=0_0#
            this.primaryExp.AddItem(this.OpenParenthesis + this.expression + this.CloseParenthesis);


            this.Optimization();

        }
    }
}
