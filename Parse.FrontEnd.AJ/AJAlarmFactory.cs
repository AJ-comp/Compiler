using AJ.Common.Helpers;
using Parse.Extensions;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Single;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ
{
    public class AJAlarmFactory
    {
        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0000. <br/>
        /// MCL0000 is that <b><i>{0} is already defined.</i></b> <br/>
        /// </summary>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0000(TokenData token)
        {
            return new MeaningErrInfo(token,
                                        nameof(AlarmCodes.MCL0000),
                                        string.Format(AlarmCodes.MCL0000, token.Input));
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0001. <br/>
        /// MCL0001 is that <b><i>{0} is not defined.</i></b> <br/>
        /// </summary>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0001(TokenData token)
        {
            return new MeaningErrInfo(token,
                                        nameof(AlarmCodes.MCL0001),
                                        string.Format(AlarmCodes.MCL0001, token.Input));
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0002. <br/>
        /// MCL0002 is that <b><i>
        /// Value can not be changed because {0} is the constant type.</i></b>
        /// <br/>
        /// </summary>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0002(TokenData token)
        {
            return new MeaningErrInfo(token,
                            nameof(AlarmCodes.MCL0002),
                            string.Format(AlarmCodes.MCL0002, token.Input));
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0003. <br/>
        /// MCL0003 is that <b><i>
        /// It can not be allocated because the data type differs.</i></b> <br/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0003(TokenData left, TokenData right)
        {
            List<TokenData> tokens = new List<TokenData>();
            tokens.Add(left);
            tokens.Add(right);

            return new MeaningErrInfo(tokens, nameof(AlarmCodes.MCL0003), AlarmCodes.MCL0003);
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0004. <br/>
        /// MCL0004 is that <b><i>
        /// lvalue have to be modifiable format.</i></b> <br/>
        /// </summary>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0004()
        {
            return new MeaningErrInfo(nameof(AlarmCodes.MCL0004), AlarmCodes.MCL0004);
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0005. <br/>
        /// MCL0005 is that <b><i>
        /// {0} variable that not initialized is using. </i></b> <br/>
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0005(TokenData token)
        {
            return new MeaningErrInfo(token,
                                nameof(AlarmCodes.MCL0005),
                                string.Format(AlarmCodes.MCL0005, token.Input));
        }


        /*
        public static MeaningErrInfo CreateMCL0006()
        {

        }


        public static MeaningErrInfo CreateMCL0007()
        {

        }
        */


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0008. <br/>
        /// MCL0008 is that <b><i>
        /// The operand of Inc or Dec operator has to be variable, property or indexer. 
        /// </i></b> <br/>
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CretaeMCL0008(TokenData token)
        {
            return new MeaningErrInfo(token,
                                nameof(AlarmCodes.MCL0008),
                                string.Format(AlarmCodes.MCL0008));
        }



        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0009. <br/>
        /// MCL0009 is that <b><i>
        /// The {0} identifier was already defined in the current block. </i></b>
        /// <br/>
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0009(TokenData token)
        {
            return new MeaningErrInfo(token,
                                nameof(AlarmCodes.MCL0009),
                                string.Format(AlarmCodes.MCL0009, token.Input));
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0012. <br/>
        /// MCL0012 is that <b><i>
        /// The operand of the inc or dec has to be variable, property or indexer. 
        /// </i></b> <br/>
        /// </summary>
        /// <param name="incDecExpr"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0012(IncDecNode incDecNode)
        {
            return new MeaningErrInfo(incDecNode.AllTokens, nameof(AlarmCodes.MCL0012), AlarmCodes.MCL0012);
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0013. <br/>
        /// MCL0013 is that <b><i>
        /// The operator '{0}' is not applicated to {1} type operand. </i></b>
        /// <br/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0013(string type, string operation)
        {
            return new MeaningErrInfo(nameof(AlarmCodes.MCL0013),
                                                    string.Format(AlarmCodes.MCL0013, operation, type));
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0014. <br/>
        /// MCL0014 is that <b><i>
        /// The member '{0}' is not used to as method. </i></b>
        /// </summary>
        /// <param name="nameToken"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0014(TokenData nameToken)
        {
            return new MeaningErrInfo(nameToken,
                                                    nameof(AlarmCodes.MCL0014),
                                                    string.Format(AlarmCodes.MCL0014, nameToken.Input));
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0015. <br/>
        /// MCL0015 is that <b><i>
        /// There is not overload method '{1}' that use parameter {0} count. </i></b>
        /// </summary>
        /// <param name="paramCount"></param>
        /// <param name="nameToken"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0015(int paramCount, TokenData nameToken)
        {
            return new MeaningErrInfo(nameToken,
                                                    nameof(AlarmCodes.MCL0015),
                                                    string.Format(AlarmCodes.MCL0015, paramCount, nameToken.Input));
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0016. <br/>
        /// MCL0016 is that <b><i>
        /// There is not argument for parameter '{1}' that need in the {0} method. </i></b>
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0016(string methodName, string paramName)
        {
            return new MeaningErrInfo(nameof(AlarmCodes.MCL0016),
                                                    string.Format(AlarmCodes.MCL0016, methodName, paramName));
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0022. <br/>
        /// MCL0022 is that <b><i>
        /// '!' operator can be applied to only 'bool' type operand. </i></b> <br/>
        /// </summary>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0022()
        {
            return new MeaningErrInfo(nameof(AlarmCodes.MCL0022), AlarmCodes.MCL0022);
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0023. <br/>
        /// MCL0023 is that <b><i>
        /// '{0}' operator can't be applied to '{1}' and '{2}' type operand. 
        /// </i></b> <br/>
        /// </summary>
        /// <param name="binaryExprNode"></param>
        /// <param name="oper"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0023(BinaryExprNode binaryExprNode, string oper)
        {
            var leftType = binaryExprNode.LeftNode.Type;
            var rightType = binaryExprNode.RightNode.Type;
            var errMsg = string.Format(AlarmCodes.MCL0023, oper, leftType.FullName, rightType.FullName);

            return new MeaningErrInfo(binaryExprNode.AllTokens, nameof(AlarmCodes.MCL0023), errMsg);
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0024. <br/>
        /// MCL0024 is that <b><i>
        /// The type '{0}' and '{1}' can't be compared.</i></b>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0024(string left, string right)
        {
            var errMsg = string.Format(AlarmCodes.MCL0024, left, right);
            return new MeaningErrInfo(nameof(AlarmCodes.MCL0024), errMsg);
        }


        /*****************************************************/
        /// <summary>
        /// Creates the MeaningErrInfo for code MCL0025. <br/>
        /// MCL0025 is that <b><i>
        /// It can't convert from {from} type to {to} type implicitly.</i></b>
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0025(ExprNode from, ExprNode to)
        {
            // if predef type then display the name else display the full name included the namespace name.
            var fromTypeName = from.Type.DefineNode.IsPreDefType ? from.Type.Name : from.Type.FullName;
            var toTypeName = to.Type.DefineNode.IsPreDefType ? to.Type.Name : to.Type.FullName;

            var errMsg = string.Format(AlarmCodes.AJ0048, fromTypeName, toTypeName);

            return new MeaningErrInfo(from.Type.NameTokens, nameof(AlarmCodes.AJ0048), errMsg);
        }

        public static MeaningErrInfo CreateMCL0025(ExprNode from, string to)
        {
            // if predef type then display the name else display the full name included the namespace name.
            var fromTypeName = from.Type.DefineNode.IsPreDefType ? from.Type.Name : from.Type.FullName;

            var errMsg = string.Format(AlarmCodes.AJ0048, fromTypeName, to);

            return new MeaningErrInfo(from.AllTokens, nameof(AlarmCodes.MCL0025), errMsg);
        }


        /*****************************************************/
        /// <summary>
        /// Create the MeaningErrInfo for code AJ0026. <br/>
        /// AJ0027 is that <b><i>
        /// The '{token}' definition is included in the '{typeDefNode}' type.</i></b>
        /// </summary>
        /// <param name="typeDefNode"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateAJ0026(TypeDefNode typeDefNode, TokenData token)
        {
            var errMsg = string.Format(AlarmCodes.AJ0026, typeDefNode.Name, token.Input);
            return new MeaningErrInfo(token, nameof(AlarmCodes.AJ0026), errMsg);
        }


        /*****************************************************/
        /// <summary>
        /// Create the MeaningErrInfo for code AJ0027. <br/>
        /// AJ0027 is that <b><i>
        /// The member name can't same with the outer name.</i></b>
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateAJ0027(TokenData token)
        {
            return new MeaningErrInfo(nameof(AlarmCodes.AJ0027), AlarmCodes.AJ0027);
        }


        /*****************************************************/
        /// <summary>
        /// Create the MeaningErrInfo for code AJ0030. <br/>
        /// AJ0030 is that <b><i>
        /// {from} type can't convert to {to} type. please return type of function.</i></b>
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateAJ0030(ExprNode from, AJType to)
        {
            var errMsg = string.Format(AlarmCodes.AJ0030, 
                                                     from.Type.DataType.ToDescription(), 
                                                     to.DataType.ToDescription());

            return new MeaningErrInfo(from.AllTokens, nameof(AlarmCodes.AJ0030), errMsg);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static MeaningErrInfo CreateAJ0031(TokenData token)
        {
            return new MeaningErrInfo(token,
                    nameof(AlarmCodes.AJ0031),
                    string.Format(AlarmCodes.AJ0031, token.Input));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static MeaningErrInfo CreateAJ0031(IEnumerable<TokenData> tokens)
        {
            return new MeaningErrInfo(tokens,
                    nameof(AlarmCodes.AJ0031),
                    string.Format(AlarmCodes.AJ0031, tokens.ItemsString(".")));
        }


        public static MeaningErrInfo CreateAJ0039(TypeDeclareNode useType, TypeDefNode type1, TypeDefNode type2)
        {
            var errMsg = string.Format(AlarmCodes.AJ0039, type1.FullName, type2.FullName);

            return new MeaningErrInfo(useType.FullDataTypeToken, nameof(AlarmCodes.AJ0039), errMsg);
        }


        /*****************************************************/
        /// <summary>
        /// Creates the MeaningErrInfo for code AJ0048. <br/>
        /// AJ0048 is that <b><i>
        /// It can't convert from {0} type to {1} type implicitly. If it use the explicit convert this is can. It should check if there is explicit convert. </i></b>
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="toCorrect"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateAJ0048(ExprNode from, ExprNode to)
        {
            // if predef type then display the name else display the full name included the namespace name.
            var fromTypeName = from.Type.DefineNode.IsPreDefType ? from.Type.Name : from.Type.FullName;
            var toTypeName = to.Type.DefineNode.IsPreDefType ? to.Type.Name : to.Type.FullName;

            var errMsg = string.Format(AlarmCodes.AJ0048, fromTypeName, toTypeName);

            return new MeaningErrInfo(from.AllTokens, nameof(AlarmCodes.AJ0048), errMsg);
        }
    }
}
