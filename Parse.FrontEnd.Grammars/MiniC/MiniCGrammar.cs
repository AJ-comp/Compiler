﻿using Parse.RegularGrammar;

namespace Parse.FrontEnd.Grammars.MiniC
{
    public class MiniCGrammar : Grammar
    {
        private Terminal @if = new Terminal(TokenType.Keyword, "if");
        private Terminal @else = new Terminal(TokenType.Keyword, "else");
        private Terminal @while = new Terminal(TokenType.Keyword, "while");
        private Terminal @return = new Terminal(TokenType.Keyword, "return");
        private Terminal @const = new Terminal(TokenType.Keyword, "const");
        private Terminal @int = new Terminal(TokenType.Keyword, "int");
        private Terminal @void = new Terminal(TokenType.Keyword, "void");
        private Terminal ident = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", "ident", true, true);
        private Terminal number = new Terminal(TokenType.Digit10, "[0-9]*", "number", true, true);
        private Terminal lineComment = new Terminal(TokenType.LineComment, "//.*$", false, true);

        private Terminal openParenthesis = new Terminal(TokenType.Operator, "(");
        private Terminal closeParenthesis = new Terminal(TokenType.Operator, ")");
        private Terminal openCurlyBrace = new Terminal(TokenType.Operator, "{");
        private Terminal closeCurlyBrace = new Terminal(TokenType.Operator, "}");
        private Terminal openSquareBrace = new Terminal(TokenType.Operator, "[");
        private Terminal closeSquareBrace = new Terminal(TokenType.Operator, "]");

        private Terminal inc = new Terminal(TokenType.Operator, "++");
        private Terminal dec = new Terminal(TokenType.Operator, "--");
        private Terminal add = new Terminal(TokenType.Operator, "+");
        private Terminal sub = new Terminal(TokenType.Operator, "-");
        private Terminal mul = new Terminal(TokenType.Operator, "*");
        private Terminal div = new Terminal(TokenType.Operator, "/");
        private Terminal mod = new Terminal(TokenType.Operator, "%");
        private Terminal assign = new Terminal(TokenType.Operator, "=");
        private Terminal addAssign = new Terminal(TokenType.Operator, "+=");
        private Terminal subAssign = new Terminal(TokenType.Operator, "-=");
        private Terminal mulAssign = new Terminal(TokenType.Operator, "*=");
        private Terminal divAssign = new Terminal(TokenType.Operator, "/=");
        private Terminal modAssign = new Terminal(TokenType.Operator, "%=");
        private Terminal equal = new Terminal(TokenType.Operator, "==");
        private Terminal notEqual = new Terminal(TokenType.Operator, "!=");
        private Terminal greaterThan = new Terminal(TokenType.Operator, ">");
        private Terminal lessThan = new Terminal(TokenType.Operator, "<");
        private Terminal greaterEqual = new Terminal(TokenType.Operator, ">=");
        private Terminal lessEqual = new Terminal(TokenType.Operator, "<=");
        private Terminal semiColon = new Terminal(TokenType.Operator, ";");
        private Terminal comma = new Terminal(TokenType.Operator, ",");

        private Terminal logicalOr = new Terminal(TokenType.Operator, "||");
        private Terminal logicalAnd = new Terminal(TokenType.Operator, "&&");
        private Terminal logicalNot = new Terminal(TokenType.Operator, "!");


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

        public MiniCGrammar()
        {
            MiniCSdts sdts = new MiniCSdts(this.keyManager);

            this.miniC.AddItem(this.translationUnit, sdts.Program);
            this.translationUnit.AddItem(this.externalDcl | this.translationUnit + this.externalDcl);
            this.externalDcl.AddItem(this.functionDef | this.declaration);
            this.functionDef.AddItem(this.functionHeader + this.compoundSt, sdts.FuncDef);
            this.functionHeader.AddItem(this.dclSpec + this.functionName + this.formalParam, sdts.FuncHead);
            this.dclSpec.AddItem(this.dclSpecifiers,sdts.DclSpec);
            this.dclSpecifiers.AddItem(this.dclSpecifier | this.dclSpecifiers + this.dclSpecifier);
            this.dclSpecifier.AddItem(this.typeQualifier | this.typeSpecifier);
            this.typeQualifier.AddItem(this.@const, sdts.ConstNode);
            this.typeSpecifier.AddItem(this.@int, sdts.IntNode);
            this.typeSpecifier.AddItem(this.@void, sdts.VoidNode);
            this.functionName.AddItem(this.ident);
            this.formalParam.AddItem(this.openParenthesis + this.optFormalParam + this.closeParenthesis, sdts.FormalPara);
            this.optFormalParam.AddItem(this.formalParamList | new Epsilon());
            this.formalParamList.AddItem(this.paramDcl | this.formalParamList + this.comma + this.paramDcl);
            this.paramDcl.AddItem(this.dclSpec + this.declarator, sdts.ParamDcl);
            this.compoundSt.AddItem(this.openCurlyBrace + this.optDclList + this.optStatList + this.closeCurlyBrace, sdts.CompoundSt);
            this.optDclList.AddItem(this.declarationList | new Epsilon(), sdts.DclList);
            this.declarationList.AddItem(this.declaration | this.declarationList + this.declaration);
            this.declaration.AddItem(this.dclSpec + this.initDclList + this.semiColon, sdts.Dcl);
            this.initDclList.AddItem(this.initDeclarator | this.initDclList + this.comma + this.initDeclarator);

            this.initDeclarator.AddItem(this.declarator, sdts.DclItem);
            this.initDeclarator.AddItem(this.declarator + this.assign + this.number, sdts.DclItem);

            this.declarator.AddItem(this.ident, sdts.SimpleVar);
            this.declarator.AddItem(this.ident + this.openSquareBrace + this.optNumber + this.closeSquareBrace, sdts.ArrayVar);

            this.optNumber.AddItem(this.number | new Epsilon());
            this.optStatList.AddItem(this.statementList, sdts.StatList);
            this.optStatList.AddItem(new Epsilon());

            this.statementList.AddItem(this.statement | this.statementList + this.statement);
            this.statement.AddItem(this.compoundSt | this.expressionSt | this.ifSt | this.whileSt | this.returnSt);
            this.expressionSt.AddItem(this.optExpression + this.semiColon, sdts.ExpSt);
            this.optExpression.AddItem(this.expression | new Epsilon());

            this.ifSt.AddItem(this.@if + this.openParenthesis + this.expression + this.closeParenthesis + this.statement, 1, sdts.IfSt);
            this.ifSt.AddItem(this.@if + this.openParenthesis + this.expression + this.closeParenthesis + this.statement + this.@else + this.statement, 0, sdts.IfElseSt);

            this.whileSt.AddItem(this.@while + this.openParenthesis + this.expression + this.closeParenthesis + this.statement, sdts.WhileSt);
            this.returnSt.AddItem(this.@return + this.optExpression + this.semiColon, sdts.ReturnSt);
            this.expression.AddItem(this.assignmentExp);

            this.assignmentExp.AddItem(this.logicalOrExp);
            this.assignmentExp.AddItem(this.unaryExp + this.assign + this.assignmentExp, sdts.Assign);
            this.assignmentExp.AddItem(this.unaryExp + this.addAssign + this.assignmentExp, sdts.AddAssign);
            this.assignmentExp.AddItem(this.unaryExp + this.subAssign + this.assignmentExp, sdts.SubAssign);
            this.assignmentExp.AddItem(this.unaryExp + this.mulAssign + this.assignmentExp, sdts.MulAssign);
            this.assignmentExp.AddItem(this.unaryExp + this.divAssign + this.assignmentExp, sdts.DivAssign);
            this.assignmentExp.AddItem(this.unaryExp + this.modAssign + this.assignmentExp, sdts.ModAssign);

            this.logicalOrExp.AddItem(this.logicalAndExp);
            this.logicalOrExp.AddItem(this.logicalOrExp + this.logicalOr + this.logicalAndExp, sdts.LogicalOr);

            this.logicalAndExp.AddItem(this.equalityExp);
            this.logicalAndExp.AddItem(this.logicalAndExp + this.logicalAnd + this.equalityExp, sdts.LogicalAnd);

            this.equalityExp.AddItem(this.relationalExp);
            this.equalityExp.AddItem(this.equalityExp + this.equal + this.relationalExp, sdts.Equal);
            this.equalityExp.AddItem(this.equalityExp + this.notEqual + this.relationalExp, sdts.NotEqual);

            this.relationalExp.AddItem(this.additiveExp);
            this.relationalExp.AddItem(this.relationalExp + this.greaterThan + this.additiveExp, sdts.GreaterThan);
            this.relationalExp.AddItem(this.relationalExp + this.lessThan + this.additiveExp, sdts.LessThan);
            this.relationalExp.AddItem(this.relationalExp + this.greaterEqual + this.additiveExp, sdts.GreatherEqual);

            this.additiveExp.AddItem(this.multiplicativeExp);
            this.additiveExp.AddItem(this.additiveExp + this.add + this.multiplicativeExp, sdts.Add);
            this.additiveExp.AddItem(this.additiveExp + this.sub + this.multiplicativeExp, sdts.Sub);

            this.multiplicativeExp.AddItem(this.unaryExp);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + this.mul + this.unaryExp, sdts.Mul);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + this.div + this.unaryExp, sdts.Div);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + this.mod + this.unaryExp, sdts.Mod);

            this.unaryExp.AddItem(this.postfixExp);
            this.unaryExp.AddItem(this.sub + this.unaryExp, sdts.UnaryMinus);
            this.unaryExp.AddItem(this.logicalNot + this.unaryExp, sdts.LogicalNot);
            this.unaryExp.AddItem(this.inc + this.unaryExp, sdts.PreInc);
            this.unaryExp.AddItem(this.dec + this.unaryExp, sdts.PreDec);

            this.postfixExp.AddItem(this.primaryExp);
            this.postfixExp.AddItem(this.postfixExp + this.openSquareBrace + this.expression + this.closeSquareBrace, sdts.Index);
            this.postfixExp.AddItem(this.postfixExp + this.openParenthesis + this.optActualParam + this.closeParenthesis, sdts.Cell);
            this.postfixExp.AddItem(this.postfixExp + this.inc, sdts.PostInc);
            this.postfixExp.AddItem(this.postfixExp + this.dec, sdts.PostDec);

            this.optActualParam.AddItem(this.actualParam, sdts.ActualParam);
            this.optActualParam.AddItem(new Epsilon());

            this.actualParam.AddItem(this.actualParamList);
            this.actualParamList.AddItem(this.assignmentExp);
            this.actualParamList.AddItem(this.actualParamList + this.comma + this.assignmentExp);

            this.primaryExp.AddItem(this.ident | this.number | this.openParenthesis + this.expression + this.closeParenthesis);


            this.Optimization();

        }
    }
}
