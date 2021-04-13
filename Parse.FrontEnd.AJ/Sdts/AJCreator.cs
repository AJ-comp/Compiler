using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.AssignExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LogicalExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.AJ.Sdts.Datas.Variables;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts
{
    public class AJCreator
    {
        public static VariableMiniC CreateVarData(Access accessType,
                                                                        AJTypeInfo typeDatas,
                                                                        TokenData nameToken,
                                                                        TokenData levelToken,
                                                                        TokenData dimensionToken,
                                                                        int blockLevel,
                                                                        int offset,
                                                                        ExprNode value)
        {
            VariableMiniC result = null;

            if (typeDatas.DataType == MiniCDataType.String)
            {
                result = new StringVariableMiniC(accessType,
                                                                 typeDatas,
                                                                 nameToken,
                                                                 levelToken,
                                                                 dimensionToken,
                                                                 blockLevel,
                                                                 offset,
                                                                 value);
            }
            else if (typeDatas.DataType == MiniCDataType.Int)
            {
                result = new IntVariableMiniC(accessType,
                                                            typeDatas,
                                                            nameToken,
                                                            levelToken,
                                                            dimensionToken,
                                                            blockLevel,
                                                            offset,
                                                            value);
            }
            else if (typeDatas.DataType == MiniCDataType.Address)
            {
                result = new PointerVariableMiniC(accessType, 
                                                                    typeDatas, 
                                                                    nameToken, 
                                                                    blockLevel, 
                                                                    offset, 
                                                                    1, 
                                                                    value, 
                                                                    StdType.Int);
            }

            return result;
        }


        public static VariableMiniC CreateVarData(Access accessType,
                                                                        AJTypeInfo typeDatas,
                                                                        TokenData nameToken,
                                                                        TokenData levelToken,
                                                                        TokenData dimensionToken,
                                                                        int blockLevel,
                                                                        int offset)
        {
            return CreateVarData(accessType, 
                                            typeDatas, 
                                            nameToken, 
                                            levelToken, 
                                            dimensionToken, 
                                            blockLevel, 
                                            offset, 
                                            null);
        }



        public static SdtsNode CreateSdtsNode(AstSymbol root)
        {
            AJNode result = null;

            if (root is AstTerminal) result = new TerminalNode(root);
            else
            {
                AstNonTerminal cRoot = root as AstNonTerminal;

                if (cRoot.SignPost.MeaningUnit == AJGrammar.Program) result = new ProgramNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.UsingNode) result = new UsingStNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.NamespaceNode) result = new NamespaceNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.AccesserNode) result = new AccesserNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.StructDef) result = new StructDefNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ClassDef) result = new ClassDefNode(root);

                else if (cRoot.SignPost.MeaningUnit == AJGrammar.FuncDef) result = new FuncDefNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.FuncHead) result = new FuncHeadNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.FormalPara) result = new ParamListNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ParamDcl) result = new ParamNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.CompoundSt) result = new CompoundStNode(root);

                else if (cRoot.SignPost.MeaningUnit == AJGrammar.StatList) result = new StatListNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.IfSt) result = new IfStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.IfElseSt) result = new IfElseStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.WhileSt) result = new WhileStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ReturnSt) result = new ReturnStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ExpSt) result = new ExprStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.Call) result = new CallNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ActualParam) result = new ActualParamNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.Index) result = new IndexNode(root);

                else if (cRoot.SignPost.MeaningUnit == AJGrammar.AddM) result = new AddExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.SubM) result = new SubExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.MulM) result = new MulExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DivM) result = new DivExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ModM) result = new ModExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.AssignM) result = new AssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.AddAssignM) result = new AddAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.SubAssignM) result = new SubAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.MulAssignM) result = new MulAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DivAssignM) result = new DivAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ModAssignM) result = new ModAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.PreIncM) result = new PreIncExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.PreDecM) result = new PreDecExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.PostIncM) result = new PostIncExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.PostDecM) result = new PostDecExprNode(root);

                else if (cRoot.SignPost.MeaningUnit == AJGrammar.LogicalOrM) result = new OrExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.LogicalAndM) result = new AndExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.LogicalNotM) result = new NotExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.EqualM) result = new EqualExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.NotEqualM) result = new NotEqualExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.GreaterThanM) result = new GreaterThanNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.LessThanM) result = new LessThanNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.GreaterEqualM) result = new GreaterEqualNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.LessEqualM) result = new LessEqualNode(root);

                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DclSpec) result = new VariableTypeNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ConstNode) result = new ConstNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.VoidNode) result = new VoidNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.SystemNode) result = new SystemNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.IntNode) result = new IntNode(root, false);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.UIntNode) result = new IntNode(root, true);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.AddressNode) result = new AddressNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.Dcl) result = new VariableDclListNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DclVar) result = new InitDeclaratorNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DeclareVarIdent) result = new DeclareVarNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DeRef) result = new DeRefExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.UseVar) result = new UseIdentNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.IntLiteralNode) result = new IntLiteralNode(root);
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
