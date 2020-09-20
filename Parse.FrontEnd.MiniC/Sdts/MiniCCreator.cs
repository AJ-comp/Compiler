using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.FrontEnd.Grammars.Properties;
using System;
using static Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables.VariableMiniC;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public class MiniCCreator
    {
        public static VariableMiniC CreateVarData(MiniCTypeInfo typeDatas,
                                                                            TokenData nameToken,
                                                                            TokenData levelToken, 
                                                                            TokenData dimensionToken,
                                                                            int blockLevel, 
                                                                            int offset, 
                                                                            VarProperty varProperty, 
                                                                            ExprNode value)
        {
            VariableMiniC result = null;

            if (typeDatas.DataType == MiniCDataType.String)
            {
                result = new StringVariableMiniC(typeDatas,
                                                                nameToken,
                                                                levelToken,
                                                                dimensionToken,
                                                                blockLevel,
                                                                offset,
                                                                varProperty, value);
            }
            else if (typeDatas.DataType == MiniCDataType.Int)
            {
                result = new IntVariableMiniC(typeDatas,
                                                            nameToken,
                                                            levelToken,
                                                            dimensionToken,
                                                            blockLevel,
                                                            offset,
                                                            varProperty,
                                                            value);
            }

            return result;
        }


        public static VariableMiniC CreateVarData(MiniCTypeInfo typeDatas, 
                                                                            TokenData nameToken,
                                                                            TokenData levelToken, 
                                                                            TokenData dimensionToken,
                                                                            int blockLevel, 
                                                                            int offset,
                                                                            VarProperty varProperty)
        {
            return CreateVarData(typeDatas, nameToken, levelToken, dimensionToken, blockLevel, offset, varProperty, null);
        }



        public static SdtsNode CreateSdtsNode(AstSymbol root)
        {
            MiniCNode result = null;

            if (root is AstTerminal) result = new TerminalNode(root);
            else
            {
                AstNonTerminal cRoot = root as AstNonTerminal;

                if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Program) result = new ProgramNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.FuncDef) result = new FuncDefNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.FuncHead) result = new FuncHeadNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.FormalPara) result = new ParamListNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.ParamDcl) result = new ParamNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.CompoundSt) result = new CompoundStNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.StatList) result = new StatListNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.IfSt) result = new IfStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.IfElseSt) result = new IfElseStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.WhileSt) result = new WhileStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.ReturnSt) result = new ReturnStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.ExpSt) result = new ExprStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Call) result = new CallNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.ActualParam) result = new ActualParamNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Index) result = new IndexNode(root);

                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.AddM) result = new AddExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.SubM) result = new SubExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.MulM) result = new MulExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DivM) result = new DivExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.ModM) result = new ModExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.AssignM) result = new AssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.AddAssignM) result = new AddAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.SubAssignM) result = new SubAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.MulAssignM) result = new MulAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DivAssignM) result = new DivAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.ModAssignM) result = new ModAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.PreIncM) result = new PreIncExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.PreDecM) result = new PreDecExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.PostIncM) result = new PostIncExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.PostDecM) result = new PostDecExprNode(root);

                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.LogicalOrM) result = new OrExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.LogicalAndM) result = new AndExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.LogicalNotM) result = new NotExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.EqualM) result = new EqualExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.NotEqualM) result = new NotEqualExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.GreaterThanM) result = new GreaterThanNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.LessThanM) result = new LessThanNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.GreaterEqualM) result = new GreaterEqualNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.LessEqualM) result = new LessEqualNode(root);

                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DclSpec) result = new VariableTypeNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.ConstNode) result = new ConstNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.VoidNode) result = new VoidNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.IntNode) result = new IntNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DclList) result = new VariableDclsListNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Dcl) result = new VariableDclsNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DclVar) result = new InitDeclaratorNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DeclareVarIdent) result = new DeclareVarNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.UseVar) result = new UseIdentNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.IntLiteralNode) result = new IntLiteralNode(root);
                else throw new Exception(AlarmCodes.MCL0010);

                foreach (var item in cRoot.Items)
                {
                    var childNode = CreateSdtsNode(item);
                    if (childNode == null) return null;

                    childNode.Parent = result;
                    result.Items.Add(childNode);
                }
            }

            return result;
        }
    }
}
