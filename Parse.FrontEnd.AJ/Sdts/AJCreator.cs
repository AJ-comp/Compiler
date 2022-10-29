using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Single;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts
{
    public class AJCreator
    {
        public static VariableAJ CreateVarData(Access accessType,
                                                                AJType typeInfo,
                                                                TokenData nameToken,
                                                                IEnumerable<TokenData> levelTokens,
                                                                int blockLevel,
                                                                int offset,
                                                                ExprNode value)
        {
            VariableAJ result = null;

            if (typeInfo.DataType == AJDataType.Int)
            {
                result = new VariableAJ(accessType, typeInfo, nameToken, levelTokens, blockLevel, offset);
            }

            return result;
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

                else if (cRoot.SignPost.MeaningUnit == AJGrammar.FuncDef) result = new FuncDefNode(root, FuncType.Normal);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.Creator) result = new FuncDefNode(root, FuncType.Creator);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.FormalPara) result = new ParamListNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.CompoundSt) result = new CompoundStNode(root);

                //                else if (cRoot.SignPost.MeaningUnit == AJGrammar.StatList) result = new StatListNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.IfSt) result = new IfStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.IfElseSt) result = new IfElseStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.WhileSt) result = new WhileStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.BreakSt) result = new BreakStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ContinueSt) result = new ContinueStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ReturnSt) result = new ReturnStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DeclareVarSt) result = new DeclareVarStNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ExpSt) result = new ExprStatementNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.Call) result = new CallNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ActualParam) result = new ActualParamNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.Index) result = new IndexNode(root);

                // operator 1 priority 
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.PreIncM) result = new IncDecNode(root, AJGrammar.Inc.Value, Info.PreInc);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.PreDecM) result = new IncDecNode(root, AJGrammar.Dec.Value, Info.PreDec);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.PostIncM) result = new IncDecNode(root, AJGrammar.Inc.Value, Info.PostInc);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.PostDecM) result = new IncDecNode(root, AJGrammar.Dec.Value, Info.PostDec);

                // operator 2 priority 
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.LogicalNotM) result = new SLogicalNode(root, IRSingleOperation.Not);

                // operator 3 priority 
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.MulM) result = new ArithmeticNode(root, IRArithmeticOperation.Mul);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DivM) result = new ArithmeticNode(root, IRArithmeticOperation.Div);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ModM) result = new ArithmeticNode(root, IRArithmeticOperation.Mod);

                // operator 4 priority 
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.AddM) result = new ArithmeticNode(root, IRArithmeticOperation.Add);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.SubM) result = new ArithmeticNode(root, IRArithmeticOperation.Sub);

                // operator 5 priority 
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.RightShiftM) result = new BinBitwiseLogicalNode(root, IRBitwiseOperation.RightShift);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.LeftShiftM) result = new BinBitwiseLogicalNode(root, IRBitwiseOperation.LeftShift);

                // operator 6 priority 
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.GreaterThanM) result = new CompareNode(root, IRCompareOperation.GT);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.LessThanM) result = new CompareNode(root, IRCompareOperation.LT);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.GreaterEqualM) result = new CompareNode(root, IRCompareOperation.GE);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.LessEqualM) result = new CompareNode(root, IRCompareOperation.LE);

                // operator 7 priority 
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.EqualM) result = new CompareNode(root, IRCompareOperation.EQ);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.NotEqualM) result = new CompareNode(root, IRCompareOperation.NE);

                // operator 8 priority 
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.BitAndM) result = new BinBitwiseLogicalNode(root, IRBitwiseOperation.BitAnd);

                // operator 9 priority 
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.BitOrM) result = new BinBitwiseLogicalNode(root, IRBitwiseOperation.BitOr);

                // operator 10 priority 
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.LogicalAndM) result = new BinLogicalNode(root, IRLogicalOperation.And);

                // operator 11 priority 
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.LogicalOrM) result = new BinLogicalNode(root, IRLogicalOperation.Or);

                // operator 12 priority 
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.AssignM) result = new AssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.AddAssignM) result = new ArithmeticAssignNode(root, IRArithmeticOperation.Add);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.SubAssignM) result = new ArithmeticAssignNode(root, IRArithmeticOperation.Sub);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.MulAssignM) result = new ArithmeticAssignNode(root, IRArithmeticOperation.Mul);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DivAssignM) result = new ArithmeticAssignNode(root, IRArithmeticOperation.Div);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ModAssignM) result = new ArithmeticAssignNode(root, IRArithmeticOperation.Mod);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.BitOrAssignM) result = new BinBitwiseAssignNode(root, IRBitwiseOperation.BitOr);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.BitAndAssignM) result = new BinBitwiseAssignNode(root, IRBitwiseOperation.BitAnd);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.RightShiftAssignM) result = new BinBitwiseAssignNode(root, IRBitwiseOperation.RightShift);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.LeftShiftAssignM) result = new BinBitwiseAssignNode(root, IRBitwiseOperation.LeftShift);

                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ConstNode) result = new ConstNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.Dcl) result = new DeclareVarNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DclVar) result = new InitDeclaratorNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DeclareVar) result = new DeclareVarNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DeclareIdent) result = new DeclareIdentNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DeRef) result = new DeRefExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.UseSymbolChain) result = new UseSymbolChainNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.UseMember) result = new UseMemberNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.UseIdent) result = new UseIdentNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.IntLiteralNode) result = new IntegerLiteralNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DoubleLiteralNode) result = new DoubleLiteralNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.BoolLiteralNode) result = new BoolLiteralNode(root);

                else if (cRoot.SignPost.MeaningUnit == AJGrammar.VoidNode) result = new TypeDeclareNode(root, AJDataType.Void, false, AJGrammar.Void.Value);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.BoolNode) result = new TypeDeclareNode(root, AJDataType.Bool, false, AJGrammar.Bool.Value);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ByteNode) result = new TypeDeclareNode(root, AJDataType.Byte, false, AJGrammar.Byte.Value);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.SByteNode) result = new TypeDeclareNode(root, AJDataType.SByte, false, AJGrammar.SByte.Value);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.CharNode) result = new TypeDeclareNode(root, AJDataType.Byte, false, AJGrammar.Char.Value);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.ShortNode) result = new TypeDeclareNode(root, AJDataType.Short, false, AJGrammar.Short.Value);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.UShortNode) result = new TypeDeclareNode(root, AJDataType.UShort, false, AJGrammar.UShort.Value);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.SystemNode) result = new TypeDeclareNode(root, AJDataType.System, false, AJGrammar.System.Value);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.IntNode) result = new TypeDeclareNode(root, AJDataType.Int, true, AJGrammar.Int.Value);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.UIntNode) result = new TypeDeclareNode(root, AJDataType.UInt, false, AJGrammar.UInt.Value);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DoubleNode) result = new TypeDeclareNode(root, AJDataType.Double, true, AJGrammar.Double.Value);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.UserDefTypeNode) result = new TypeDeclareNode(root);
                else if (cRoot.SignPost.MeaningUnit == AJGrammar.DefNameNode) result = new DefNameNode(root);

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
