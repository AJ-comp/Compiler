using AJ.Common.Helpers;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
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
        public static MeaningErrInfo CretaeMCL0005(TokenData token)
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
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0012()
        {
            return new MeaningErrInfo(nameof(AlarmCodes.MCL0012), AlarmCodes.MCL0012);
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
        /// <param name="name"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0014(string name)
        {
            return new MeaningErrInfo(nameof(AlarmCodes.MCL0014),
                                                    string.Format(AlarmCodes.MCL0014, name));
        }


        /*****************************************************/
        /// <summary>
        /// This function creates the MeaningErrInfo for code MCL0015. <br/>
        /// MCL0015 is that <b><i>
        /// There is not overload method '{1}' that use parameter {0} count. </i></b>
        /// </summary>
        /// <param name="paramCount"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0015(int paramCount, string methodName)
        {
            return new MeaningErrInfo(nameof(AlarmCodes.MCL0015),
                                                    string.Format(AlarmCodes.MCL0015, paramCount, methodName));
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
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="oper"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0023(string left, string right, string oper)
        {
            var errMsg = string.Format(AlarmCodes.MCL0023, oper, left, right);
            return new MeaningErrInfo(nameof(AlarmCodes.MCL0023), errMsg);
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
        /// This function creates the MeaningErrInfo for code MCL0025. <br/>
        /// MCL0025 is that <b><i>
        /// It can't convert from {from} type to {to} type implicitly.</i></b>
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <returns></returns>
        /*****************************************************/
        public static MeaningErrInfo CreateMCL0025(string from, string to)
        {
            var errMsg = string.Format(AlarmCodes.MCL0025, from, to);
            return new MeaningErrInfo(nameof(AlarmCodes.MCL0025), errMsg);
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
        public static MeaningErrInfo CreateAJ0030(AJTypeInfo from, AJTypeInfo to)
        {
            var errMsg = string.Format(AlarmCodes.AJ0030, 
                                                     from.DataType.ToDescription(), 
                                                     to.DataType.ToDescription());

            List<TokenData> tokens = new List<TokenData>();

            tokens.Add(from.Token);
            tokens.Add(to.Token);

            return new MeaningErrInfo(tokens, nameof(AlarmCodes.AJ0030), errMsg);
        }
    }
}
