using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using Parse.Types;
using System;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM
{
    public partial class LLVMInterpreter
    {
        public static Dictionary<IRFunction, int> NextIndexByFunction { get; } = new Dictionary<IRFunction, int>();


        public static string ToBitCode(IRProgramRoot root)
        {
            string result = string.Empty;

            foreach (var structDef in root.StructDefs) result += ToBitCode(structDef);
            result += Environment.NewLine;

            foreach (var function in root.Functions) result += ToBitCode(function) + Environment.NewLine;
            result += Environment.NewLine;

            return result;
        }


        /**********************************************************/
        /// <summary>
        /// %struct.{Name} = type { members }    <br/>
        /// members ex: <br/>
        /// int: i32    <br/>
        /// int[5]: [5 x i32]   <br/> 
        /// struct*[2]: [2 x %struct.B*]    <br/>
        /// </summary>
        /// <param name="structDef"></param>
        /// <returns></returns>
        /**********************************************************/
        public static string ToBitCode(IRStructDef structDef)
        {
            string result = $"%struct.{structDef.IRName} = type ";

            result += "{";
            result += structDef.Members.ItemsString(PrintType.Property, "LLVMTypeName");
            result += "}";
            result += Environment.NewLine;

            return result;
        }


        /**********************************************************/
        /// <summary>
        /// This function returns a data format as below <br/>
        /// IsGlobal is true: 
        /// <b><i>@{name} = common global {type} {initValue}, align {size} </i></b>
        /// <br/>
        /// IsGlobal is false: 
        /// <b><i>%{index} = alloca {type}, allign {size}</i></b> <br/>
        /// </summary>
        /// <param name="irVar"></param>
        /// <returns></returns>
        /**********************************************************/
        public static string ToBitCode(IRFunction function)
        {
            var llvmFunction = new LLVMFunction(function);
            string result = $"define dso_local {llvmFunction.ReturnType.LLVMTypeName} @{llvmFunction.IRName} ";

            // argument code
            result += "(";
            for (int i = 0; i < llvmFunction.Arguments.Count; i++)
            {
                var arg = llvmFunction.Arguments[i];

                var param = new LLVMNamedVar(arg);
                llvmFunction.AddVar(param);

                result += $"{param.TypeName} noundef %{arg.Name}";
                if (i < llvmFunction.Arguments.Count - 1) result += ", ";
            }
            result += ") #0";
            llvmFunction.Code.AddEtcCommand(result);
            llvmFunction.Code.AddNewLine();

            // statement code
            llvmFunction.Code.AddOpenBlock();
            llvmFunction.Code.AddEtcCommand("entry:");

            // init argument
            for (int i = llvmFunction.Arguments.Count - 1; i >= 0; i--)
            {
                var arg = llvmFunction.Arguments[i];

                llvmFunction.Code.AddComment($"initialize for parameter {i}");
                llvmFunction.Code.AddInitialize(arg);
                llvmFunction.Code.AddNewLine();
            }

            ToBitCode(llvmFunction.Statement, llvmFunction);
            llvmFunction.Code.AddReturn();
            llvmFunction.Code.AddCloseBlock();
            llvmFunction.Code.AddNewLine();

            return llvmFunction.Code.GetAllCode();
        }


        public static void ToBitCode(IRStatement statement, LLVMFunction ownFunction)
        {
            if (statement is IRConditionStatement) ToBitCode(statement as IRConditionStatement, ownFunction);
            else if (statement is IRCompoundStatement) ToBitCode(statement as IRCompoundStatement, ownFunction);
            else if (statement is IRExprStatement) ToBitCode(statement as IRExprStatement, ownFunction);
            else if (statement is IRRepeatStatement) ToBitCode(statement as IRRepeatStatement, ownFunction);
            else if (statement is IRControlStatement) ToBitCode(statement as IRControlStatement, ownFunction);
            else throw new Exception("There is no correct statement.");
        }



        /// <summary>
        /// Create the br endVar code from elseOrEnVar through the below principle. <br/>
        /// 1. If the last code is br then do nothing.  (it doesn't need to create br endVar code because there is already other the br code. <br/>
        /// 2. If the type of elseOrEndVar is elseVar then it think of endVar was not created therefore create the endVar. <br/>
        /// </summary>
        /// <param name="elseOrEndVar"></param>
        /// <param name="ownFunction"></param>
        /// <returns></returns>
        private static LLVMVar CreateEndVarCodeConditional(LLVMVar elseOrEndVar, LLVMFunction ownFunction)
        {
            LLVMVar endVar = null;
            if (ownFunction.Code.LastCommandIsBranch()) return endVar;

            // if endVar is not created yet then create at here.
            if (elseOrEndVar.VarType == LLVMVarType.IfElseVar)
            {
                endVar = LLVMVar.CreateLabelVar(LLVMVarType.IfEndVar);
                ownFunction.AddVar(endVar);
            }
            else endVar = elseOrEndVar;

            ownFunction.Code.AddBranch(endVar);
            return endVar;
        }


        /// <summary>
        /// Algorithm: <br/>
        /// ex 1: in "if statement" <br/> 
        /// "if statement" create ifVar and endVar in the beginning.  (br if.then if.end)<br/>
        /// when the "if statement" is end then it try to create br endVar code using CreateEndVarCodeConditional function. <br/>
        /// if the last code is the br code don't create br code. <br/>
        /// else the endVar will be not created because it will be already created. And create br code using the endVar.<br/>
        /// ex 2: in "if else statement" <br/>
        /// "if else statement" create ifVar and elseVar in the beginning. (br if.then if.else) <br/>
        /// when the "if statement" is end in "if else statement" then it try to create br endVar code using CreateEndVarCodeConditional function. <br/>
        /// if the last code is not the br code then the endVar will be created.<br/>
        /// when the "else statement" is end then it try to create br endVar code using CreateEndVarCodeConditional function. <br/>
        /// <i>here!! if there is already endVar that created from "if statement" it has to pass that. Because it has to same the br label of "if statement" and "else statement".</i> <br/>
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="ownFunction"></param>
        public static void ToBitCode(IRConditionStatement statement, LLVMFunction ownFunction)
        {
            ToBitCode(statement.Condition, ownFunction, new LLVMBuildOption(false));

            var cmpVar = ownFunction.GetRecentVar(LLVMVarType.CmpVar);
            var ifVar = LLVMVar.CreateLabelVar(LLVMVarType.IfVar);
            var elseOrEndVar = (statement.FalseStatement != null) ? LLVMVar.CreateLabelVar(LLVMVarType.IfElseVar)
                                                                                            : LLVMVar.CreateLabelVar(LLVMVarType.IfEndVar);

            ownFunction.AddVars(ifVar, elseOrEndVar);

            ownFunction.Code.AddBranch(cmpVar, ifVar, elseOrEndVar);
            ownFunction.Code.AddNewLine();
            ownFunction.Code.AddLabel(ifVar, $"{statement.Condition} is true");
            ToBitCode(statement.TrueStatement, ownFunction);
            var endVar = CreateEndVarCodeConditional(elseOrEndVar, ownFunction);
            ownFunction.Code.AddNewLine();

            LLVMVar endVar2 = null;
            if (elseOrEndVar.VarType == LLVMVarType.IfElseVar)
            {
                ownFunction.Code.AddLabel(elseOrEndVar, $"{statement.Condition} is false");
                ToBitCode(statement.FalseStatement, ownFunction);
                var param = (endVar != null) ? endVar : elseOrEndVar;
                // endVar2 is null or same value with endVar
                endVar2 = CreateEndVarCodeConditional(param, ownFunction);
            }

            var finalEndVar = (endVar != null) ? endVar : endVar2;
            if (finalEndVar != null)
            {
                ownFunction.Code.AddNewLine();
                ownFunction.Code.AddLabel(finalEndVar, "cmp is end");
            }
        }


        public static void ToBitCode(IRRepeatStatement statement, LLVMFunction ownFunction)
        {
            var whileBodyVar = LLVMVar.CreateLabelVar(LLVMVarType.WhileBodyVar);
            ownFunction.AddVar(whileBodyVar);
            var whileCondVar = LLVMVar.CreateLabelVar(LLVMVarType.WhileCondVar);
            ownFunction.AddVar(whileCondVar);
            var whileEndVar = LLVMVar.CreateLabelVar(LLVMVarType.WhileEndVar);
            ownFunction.AddVar(whileEndVar);

            ownFunction.Code.AddBranch(whileCondVar);
            ownFunction.Code.AddNewLine();

            // while.cond
            ownFunction.Code.AddLabel(whileCondVar);
            ToBitCode(statement.Condition, ownFunction, new LLVMBuildOption(false));
            ownFunction.Code.AddBranch(ownFunction.GetRecentVar(), whileBodyVar, whileEndVar);
            ownFunction.Code.AddNewLine();

            // while.body
            ownFunction.Code.AddLabel(whileBodyVar);
            ToBitCode(statement.TrueStatement, ownFunction);
            if (!statement.IncludeBreak) ownFunction.Code.AddBranch(whileCondVar);
            ownFunction.Code.AddNewLine();

            // while.end
            ownFunction.Code.AddLabel(whileEndVar);
        }


        public static void ToBitCode(IRControlStatement statement, LLVMFunction ownFunction)
        {
            if (statement.ControlType == IRControlType.Break)
                ownFunction.Code.AddBranch(ownFunction.GetRecentVar(LLVMVarType.WhileEndVar));
            else if (statement.ControlType == IRControlType.Continue)
                ownFunction.Code.AddBranch(ownFunction.GetRecentVar(LLVMVarType.WhileCondVar));
        }


        private static void ToBitCode(IRVariable localVar, LLVMFunction ownFunction)
        {
            var namedVar = new LLVMNamedVar(localVar);
            ownFunction.AddVar(namedVar);

            ownFunction.Code.AddComment(localVar.ToString());
            ownFunction.Code.AddAlloca(namedVar);

            if (localVar.Type.Type == StdType.Struct)
            {

            }
            else
            {
                if (localVar.InitValue is IRLiteralExpr)
                {
                    var literalVar = localVar.InitValue as IRLiteralExpr;
                    ownFunction.Code.AddStore(namedVar, literalVar);
                }
                else
                {
                    ToBitCode(localVar.InitValue, ownFunction, new LLVMBuildOption(true));
                    ownFunction.Code.AddStore(namedVar, ownFunction.GetRecentVar());
                }
            }

            ownFunction.Code.AddNewLine();
        }


        public static void ToBitCode(IRCompoundStatement statement, LLVMFunction ownFunction)
        {
            foreach (var item in statement.Items)
            {
                if (item is IRVariable) ToBitCode(item as IRVariable, ownFunction);
                else ToBitCode(item as IRStatement, ownFunction);
            }
        }


        public static void ToBitCode(IRExprStatement statement, LLVMFunction ownFunction)
        {
            ownFunction.Code.AddComment($"{statement.Expr}");
            ToBitCode(statement.Expr, ownFunction, new LLVMBuildOption(true));
        }


        public static void ToBitCode(IRExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            if (expr is IRBinaryExpr) ToBitCode(expr as IRBinaryExpr, ownFunction, option);
            else if (expr is IRCallExpr) ToBitCode(expr as IRCallExpr, ownFunction, option);
            else if (expr is IRSingleExpr) ToBitCode(expr as IRSingleExpr, ownFunction, option);
            else if (expr is IRLiteralExpr) ToBitCode(expr as IRLiteralExpr, ownFunction, option);
            else if (expr is IRUseIdentExpr) ToBitCode(expr as IRUseIdentExpr, ownFunction, option);
            else throw new Exception("There is no correct expr.");
        }

        public static void ToBitCode(IRCallExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
        }

        public static void ToBitCode(IRSingleExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            if (!option.NoComment) ownFunction.Code.AddComment($"; {expr}");

            // postinc, postdec, preinc, predec has only IRUseIdentExpr
            // ex: a++ = ok, (a+b)++ = no
            var namedVar = ownFunction.GetNamedVar((expr.Items[0] as IRUseIdentExpr).Variable);
            var value = LLVMChecker.IsInc(expr.Operation) ? new IRLiteralExpr(1) : new IRLiteralExpr(-1);
            var type = LLVMChecker.IsInc(expr.Operation) ? LLVMVarType.IncVar : LLVMVarType.DecVar;

            if (expr.Operation == IRSingleOperation.PostInc || expr.Operation == IRSingleOperation.PostDec)
            {
                // The post has to be recent var is before calculate.
                var incDecVar = new LLVMVar(type, expr.Type);
                ownFunction.AddVar(incDecVar);
                ToBitCode(expr.Items[0] as IRExpr, ownFunction, option);
                var recentVar = ownFunction.GetRecentVar(LLVMVarType.NormalVar);

                ownFunction.Code.AddArithmetic(incDecVar, recentVar, value, IRBinaryOperation.Add);
                ownFunction.Code.AddStore(namedVar, incDecVar);
            }
            else if (expr.Operation == IRSingleOperation.PreInc || expr.Operation == IRSingleOperation.PreDec)
            {
                // The pre has to be recent var is after calculate.
                ToBitCode(expr.Items[0] as IRExpr, ownFunction, option);
                var recentVar = ownFunction.GetRecentVar(LLVMVarType.NormalVar);
                var incDecVar = new LLVMVar(type, expr.Type);
                ownFunction.AddVar(incDecVar);

                ownFunction.Code.AddArithmetic(incDecVar, recentVar, value, IRBinaryOperation.Add);
                ownFunction.Code.AddStore(namedVar, incDecVar);
            }
        }

        public static void ToBitCode(IRLiteralExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            LLVMVar var = new LLVMLiteralVar(expr);
            ownFunction.AddVar(var);
        }


        public static void ToBitCode(IRUseIdentExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            var namedVar = ownFunction.GetNamedVar(expr.Variable);
            var llvmVar = new LLVMVar(LLVMVarType.NormalVar, expr);
            ownFunction.AddVar(llvmVar);

            // namedVar.Typename == llvmVar.TypeName
            ownFunction.Code.AddLoad(llvmVar, namedVar);
        }
    }
}
