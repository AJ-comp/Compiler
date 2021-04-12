using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Properties;
using Parse.FrontEnd.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using Parse.Types;
using System;

namespace Parse.FrontEnd.MiniC.Sdts
{
    public class MiniCCreator
    {
        public static VariableMiniC CreateVarData(Access accessType,
                                                                        MiniCTypeInfo typeDatas,
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
                                                                        MiniCTypeInfo typeDatas,
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
            MiniCNode result = null;

            if (root is AstTerminal) result = new TerminalNode(root);
            else
            {
                AstNonTerminal cRoot = root as AstNonTerminal;

                if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Program) result = new ProgramNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.UsingNode) result = new UsingStNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.NamespaceNode) result = new NamespaceNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.AccesserNode) result = new AccesserNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.StructDef) result = new StructDefNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.ClassDef) result = new ClassDefNode(root);

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
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.SystemNode) result = new SystemNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.IntNode) result = new IntNode(root, false);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.UIntNode) result = new IntNode(root, true);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.AddressNode) result = new AddressNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Dcl) result = new VariableDclListNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DclVar) result = new InitDeclaratorNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DeclareVarIdent) result = new DeclareVarNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DeRef) result = new DeRefExprNode(root);
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
