using Parse.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Grammars.PracticeGrammars
{
    public class Mini_C : Grammar
    {
        private Terminal @if = new Terminal(TokenType.Keyword, "if");
        private Terminal @else = new Terminal(TokenType.Keyword, "else");
        private Terminal @while = new Terminal(TokenType.Keyword, "while");
        private Terminal @return = new Terminal(TokenType.Keyword, "return");
        private Terminal @const = new Terminal(TokenType.Keyword, "const");
        private Terminal @int = new Terminal(TokenType.Keyword, "int");
        private Terminal @void = new Terminal(TokenType.Keyword, "void");
        private Terminal ident = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", "ident", true, true);
        private Terminal number = new Terminal(TokenType.Identifier, "[0-9]*", "number", true, true);

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

        public Mini_C()
        {
            this.miniC.AddItem(this.translationUnit);
            this.translationUnit.AddItem(this.externalDcl | this.translationUnit + this.externalDcl);
            this.externalDcl.AddItem(this.functionDef | this.declaration);
            this.functionDef.AddItem(this.functionHeader + this.compoundSt);
            this.functionHeader.AddItem(this.dclSpec + this.functionName + this.formalParam);
            this.dclSpec.AddItem(this.dclSpecifiers);
            this.dclSpecifiers.AddItem(this.dclSpecifier | this.dclSpecifiers + this.dclSpecifier);
            this.dclSpecifier.AddItem(this.typeQualifier | this.typeSpecifier);
            this.typeQualifier.AddItem(this.@const);
            this.typeSpecifier.AddItem(this.@int | this.@void);
            this.functionName.AddItem(this.ident);
            this.formalParam.AddItem(this.openParenthesis + this.optFormalParam + this.closeParenthesis);
            this.optFormalParam.AddItem(this.formalParamList | new Epsilon());
            this.formalParamList.AddItem(this.paramDcl | this.formalParamList + this.comma + this.paramDcl);
            this.paramDcl.AddItem(this.dclSpec + this.declarator);
            this.compoundSt.AddItem(this.openCurlyBrace + this.optDclList + this.optStatList + this.closeCurlyBrace);
            this.optDclList.AddItem(this.declarationList | new Epsilon());
            this.declarationList.AddItem(this.declaration | this.declarationList + this.declaration);
            this.declaration.AddItem(this.dclSpec + this.initDclList + this.semiColon);
            this.initDclList.AddItem(this.initDeclarator | this.initDclList + this.comma + this.initDeclarator);

            this.initDeclarator.AddItem(this.declarator);
            this.initDeclarator.AddItem(this.declarator + this.assign + this.number);

            this.declarator.AddItem(this.ident);
            this.declarator.AddItem(this.ident + this.openSquareBrace + this.optNumber + this.closeSquareBrace);

            this.optNumber.AddItem(this.number | new Epsilon());
            this.optStatList.AddItem(this.statementList | new Epsilon());

            this.statementList.AddItem(this.statement | this.statementList + this.statement);
            this.statement.AddItem(this.compoundSt | this.expressionSt | this.ifSt | this.whileSt | this.returnSt);
            this.expressionSt.AddItem(this.optExpression + this.semiColon);
            this.optExpression.AddItem(this.expression | new Epsilon());

            this.ifSt.AddItem(this.@if + this.openParenthesis + this.expression + this.closeParenthesis + this.statement, 1);
            this.ifSt.AddItem(this.@if + this.openParenthesis + this.expression + this.closeParenthesis + this.statement + this.@else + this.statement, 0);

            this.whileSt.AddItem(this.@while + this.openParenthesis + this.expression + this.closeParenthesis + this.statement);
            this.returnSt.AddItem(this.@return + this.optExpression + this.semiColon);
            this.expression.AddItem(this.assignmentExp);

            this.assignmentExp.AddItem(this.logicalOrExp);
            this.assignmentExp.AddItem(this.unaryExp + this.assign + this.assignmentExp);
            this.assignmentExp.AddItem(this.unaryExp + this.addAssign + this.assignmentExp);
            this.assignmentExp.AddItem(this.unaryExp + this.subAssign + this.assignmentExp);
            this.assignmentExp.AddItem(this.unaryExp + this.mulAssign + this.assignmentExp);
            this.assignmentExp.AddItem(this.unaryExp + this.divAssign + this.assignmentExp);
            this.assignmentExp.AddItem(this.unaryExp + this.modAssign + this.assignmentExp);

            this.logicalOrExp.AddItem(this.logicalAndExp);
            this.logicalOrExp.AddItem(this.logicalOrExp + this.logicalOr + this.logicalAndExp);

            this.logicalAndExp.AddItem(this.equalityExp);
            this.logicalAndExp.AddItem(this.logicalAndExp + this.logicalAnd + this.equalityExp);

            this.equalityExp.AddItem(this.relationalExp);
            this.equalityExp.AddItem(this.equalityExp + this.equal + this.relationalExp);
            this.equalityExp.AddItem(this.equalityExp + this.notEqual + this.relationalExp);

            this.relationalExp.AddItem(this.additiveExp);
            this.relationalExp.AddItem(this.relationalExp + this.greaterThan + this.additiveExp);
            this.relationalExp.AddItem(this.relationalExp + this.lessThan + this.additiveExp);
            this.relationalExp.AddItem(this.relationalExp + this.greaterEqual + this.additiveExp);

            this.additiveExp.AddItem(this.multiplicativeExp);
            this.additiveExp.AddItem(this.additiveExp + this.add + this.multiplicativeExp);
            this.additiveExp.AddItem(this.additiveExp + this.sub + this.multiplicativeExp);

            this.multiplicativeExp.AddItem(this.unaryExp);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + this.mul + this.unaryExp);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + this.div + this.unaryExp);
            this.multiplicativeExp.AddItem(this.multiplicativeExp + this.mod + this.unaryExp);

            this.unaryExp.AddItem(this.postfixExp);
            this.unaryExp.AddItem(this.sub + this.unaryExp);
            this.unaryExp.AddItem(this.logicalNot + this.unaryExp);
            this.unaryExp.AddItem(this.inc + this.unaryExp);
            this.unaryExp.AddItem(this.dec + this.unaryExp);

            this.postfixExp.AddItem(this.primaryExp);
            this.postfixExp.AddItem(this.postfixExp + this.openSquareBrace + this.expression + this.closeSquareBrace);
            this.postfixExp.AddItem(this.postfixExp + this.openParenthesis + this.optActualParam + this.closeParenthesis);
            this.postfixExp.AddItem(this.postfixExp + this.inc);
            this.postfixExp.AddItem(this.postfixExp + this.dec);

            this.optActualParam.AddItem(this.actualParam);
            this.optActualParam.AddItem(new Epsilon());

            this.actualParam.AddItem(this.actualParamList);
            this.actualParamList.AddItem(this.assignmentExp);
            this.actualParamList.AddItem(this.actualParamList + this.comma + this.assignmentExp);

            this.primaryExp.AddItem(this.ident | this.number | this.openParenthesis + this.expression + this.closeParenthesis);


            this.Optimization();

        }
    }
}
