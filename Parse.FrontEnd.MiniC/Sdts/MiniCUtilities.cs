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
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public class MiniCUtilities
    {

        /// <summary>
        /// This function adds duplicated error to the node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="varTokenToCheck"></param>
        /// <returns></returns>
        public static bool AddDuplicatedError(MiniCNode node, TokenData varTokenToCheck = null)
        {
            if(varTokenToCheck == null)
            {
                node.ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(node.AllTokens,
                                                    nameof(AlarmCodes.MCL0009),
                                                    string.Format(AlarmCodes.MCL0009, node.AllTokens[0].Input))
                );
            }
            else
            {
                node.ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(varTokenToCheck,
                                                    nameof(AlarmCodes.MCL0009),
                                                    string.Format(AlarmCodes.MCL0009, varTokenToCheck.Input))
                );
            }

            return true;
        }

        public static bool AddErrorUseDefinedIdent(MiniCNode node, TokenData tokenData = null)
        {
            if (tokenData == null)
            {
                node.ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(node.AllTokens, nameof(AlarmCodes.MCL0017), AlarmCodes.MCL0017)
                );
            }
            else
            {
                node.ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(tokenData, nameof(AlarmCodes.MCL0017), AlarmCodes.MCL0017)
                );
            }

            return true;
        }


        public static bool AddErrorDefineCantOwn(MiniCNode node, TokenData tokenData)
        {
            node.ConnectedErrInfoList.Add
            (
                new MeaningErrInfo(tokenData, nameof(AlarmCodes.MCL0018), AlarmCodes.MCL0018)
            );

            return true;
        }


        /// <summary>
        /// This function returns all SymbolTable list of from fromNode to root node.
        /// first = leaf, last = root
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<MiniCSymbolTable> GetReferableSymbolTablelList(MiniCNode fromNode)
        {
            List<MiniCSymbolTable> result = new List<MiniCSymbolTable>();
            MiniCNode trasverNode = fromNode;

            while (trasverNode != null)
            {
                if(trasverNode.SymbolTable != null)
                    result.Add(trasverNode.SymbolTable);

                trasverNode = trasverNode.Parent as MiniCNode;
            }

            return result;
        }


        /// <summary>
        /// This function returns VarData that matched with 'varTokenToFind' from SymbolTable list referenceable.
        /// </summary>
        /// <param name="fromNode"></param>
        /// <returns></returns>
        public static MiniCReferenceRecord<VariableMiniC> GetVarRecordFromReferableST(MiniCNode fromNode, TokenData varTokenToFind)
        {
            if (varTokenToFind == null) return null;

            var tableList = GetReferableSymbolTablelList(fromNode);

            foreach (var table in tableList)
            {
                foreach (var var in table.VarTable)
                {
                    if (var.DefineField.Name != varTokenToFind.Input) continue;

                    return var;
                }
            }

            return null;
        }

        public static MiniCFuncData GetFuncDataFromReferableST(MiniCNode fromNode, TokenData varTokenToFind)
        {
            if (varTokenToFind == null) return null;

            MiniCFuncData result = null;
            var tableList = GetReferableSymbolTablelList(fromNode);

            foreach (var table in tableList)
            {
                var funcData = table.GetFuncByName(varTokenToFind.Input);
                if (funcData != null)
                {
                    result = funcData;
                    break;
                }
            }

            return result;
        }



        public SdtsNode CreateSdtsNode(AstSymbol root)
        {
            MiniCNode result = null;

            if (root is AstTerminal) result = new TerminalNode(root);
            else
            {
                AstNonTerminal cRoot = root as AstNonTerminal;

                if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Program) result = new ProgramNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DefinePrep) result = new DefinePrepNode(root);
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

                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Add) result = new AddExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Sub) result = new SubExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Mul) result = new MulExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Div) result = new DivExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Mod) result = new ModExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Assign) result = new AssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.AddAssign) result = new AddAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.SubAssign) result = new SubAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.MulAssign) result = new MulAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DivAssign) result = new DivAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.ModAssign) result = new ModAssignNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.PreInc) result = new PreIncExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.PreDec) result = new PreDecExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.PostInc) result = new PostIncExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.PostDec) result = new PostDecExprNode(root);

                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.LogicalOr) result = new OrExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.LogicalAnd) result = new AndExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.LogicalNot) result = new NotExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Equal) result = new EqualExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.NotEqual) result = new NotEqualExprNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.GreaterThan) result = new GreaterThanNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.LessThan) result = new LessThanNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.GreaterEqual) result = new GreaterEqualNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.LessEqual) result = new LessEqualNode(root);

                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DclSpec) result = new VariableTypeNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.ConstNode) result = new ConstNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.VoidNode) result = new VoidNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.IntNode) result = new IntNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DclList) result = new VariableDclsListNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.Dcl) result = new VariableDclsNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DclItem) result = new InitDeclaratorNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.DeclareVar) result = new DeclareVarNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.UseVar) result = new UseIdentNode(root);
                else if (cRoot.SignPost.MeaningUnit == MiniCGrammar.IntLiteralNode) result = new IntLiteralNode(root);
                else throw new Exception(AlarmCodes.MCL0010);

                foreach (var item in cRoot.Items)
                {
                    var childNode = GenerateSdtsAst(item);
                    if (childNode == null) return null;

                    childNode.Parent = result;
                    result.Items.Add(childNode);
                }
            }

            return result;
        }
    }
}
