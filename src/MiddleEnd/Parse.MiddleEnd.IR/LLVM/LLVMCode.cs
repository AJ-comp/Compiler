using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class Code : List<CodeSnippet>
    {
        public CodeSnippet GetLastCommand()
        {
            CodeSnippet result = null;

            for (int i = Count - 1; i > 0; i--)
            {
                var item = this[i];

                if (item.CodeType is Command)
                {
                    result = item;
                    break;
                }
            }

            return result;
        }

        public string GetAllCode()
        {
            string result = string.Empty;

            foreach (var snippet in this)
            {
                result += snippet.Data;
                if (snippet.CodeType != CodeType.Decorator) result += Environment.NewLine;
            }

            return result;
        }

        public bool LastCommandIsBranch()
        {
            var item = GetLastCommand();
            if (item == null) return false;

            return item.CodeType == CodeType.Command.Branch;
        }


        public void AddComment(string comment)
        {
            if (string.IsNullOrEmpty(comment)) return;

            Add(new CodeSnippet(CodeType.Comment, $"; {comment}"));
        }
        public void AddNewLine() => Add(new CodeSnippet(CodeType.Decorator, Environment.NewLine));
        public void AddOpenBlock() => Add(new CodeSnippet(CodeType.Command.Block, "{"));
        public void AddCloseBlock() => Add(new CodeSnippet(CodeType.Command.Block, "}"));
        public void AddEtcCommand(string command) => Add(new CodeSnippet(CodeType.Command.Etc, command));
        public void AddReturn() => Add(new CodeSnippet(CodeType.Command.Return, "ret void"));

        public void AddArithmetic(LLVMVar toVar, LLVMVar fromVar1, LLVMVar fromVar2, IRBinaryOperation operation)
            => Add(new CodeSnippet(CodeType.Command.Operator, GetArithmeticCommand(toVar, fromVar1, fromVar2, operation)));
        public void AddArithmetic(LLVMVar toVar, LLVMVar fromVar1, IRLiteralExpr fromValue, IRBinaryOperation operation)
            => Add(new CodeSnippet(CodeType.Command.Operator, GetArithmeticCommand(toVar, fromVar1, fromValue, operation)));


        /// <summary>
        /// Create the alloca code for local var.
        /// </summary>
        /// <param name="namedVar"></param>
        public void AddAlloca(LLVMNamedVar namedVar)
        {
            Add(new CodeSnippet(CodeType.Command.Alloca, $"{namedVar.NameInFunction} = alloca {namedVar.TypeName}, align {namedVar.Size}"));
        }

        /// <summary>
        /// Create the initialize code for parameter.
        /// </summary>
        /// <param name="arg"></param>
        public void AddInitialize(LLVMNamedVar arg)
        {
            Add(new CodeSnippet(CodeType.Command.Alloca, $"{arg.NameInFunction}.addr = alloca {arg.Type.LLVMTypeName}, align {arg.Type.Size}"));
            Add(new CodeSnippet(CodeType.Command.Store, $"store {arg.Type.LLVMTypeName} {arg.NameInFunction}, " +
                                                                                      $"{arg.Type.LLVMTypeName}* {arg.NameInFunction}.addr, align {arg.Type.Size}"));
        }


        private string CheckAddr(LLVMVar toCheckVar)
        {
            string result = string.Empty;

            if (toCheckVar is LLVMNamedVar)
            {
                if (toCheckVar.Type.PointerLevel > 0) result = ".addr";
            }

            return result;
        }

        public void AddStore(LLVMVar toVar, LLVMVar fVar)
        {
            string addr = CheckAddr(toVar);

            Add(new CodeSnippet(CodeType.Command.Store,
                                            $"store {toVar.TypeName} {fVar.NameInFunction}, {toVar.TypeName}* {toVar.NameInFunction}{addr}, align {toVar.Size}"));
        }

        public void AddStore(LLVMVar toVar, IRLiteralExpr expr)
        {
            string value = expr.Value.ToString();
            string addr = CheckAddr(toVar);

            if (toVar.Type.PointerLevel > 0) value = $"inttoptr(i64 {expr.Value} to {toVar.TypeName})";

            Add(new CodeSnippet(CodeType.Command.Store,
                                            $"store {toVar.TypeName} {value}, {toVar.TypeName}* {toVar.NameInFunction}{addr}, align {toVar.Size}"));
        }

        public void AddLoad(LLVMVar toVar, LLVMNamedVar fromVar)
        {
            string addr = CheckAddr(fromVar);

            Add(new CodeSnippet(CodeType.Command.Load,
                                            $"{toVar.NameInFunction} = load {toVar.TypeName}, {toVar.TypeName}* {fromVar.NameInFunction}{addr}, align {fromVar.Size}"));
        }


        public void AddCmp(IRBinaryExpr expr, LLVMVar cmpVar, LLVMVar to1, LLVMVar to2)
        {
            string cmpType = (expr.Left.Type.IsIntegerType && expr.Right.Type.IsIntegerType) ? "icmp" : "fcmp";
            string unsign = (expr.Left.Type.IsUnsigned || expr.Right.Type.IsUnsigned) ? "u" : "s";
            var maximumType = LLVMChecker.MaximumType(expr.Left.Type, expr.Right.Type);
            string operation = LLVMConverter.GetInstructionName(expr.Operation);

            Add(new CodeSnippet(CodeType.Command.Cmp,
                                            $"{cmpVar.NameInFunction} = {cmpType} {unsign}{operation} {maximumType.LLVMTypeName} {to1.NameInFunction}, {to2.NameInFunction}"));
        }

        /// <summary>
        /// Create conditional branch code.
        /// </summary>
        /// <param name="cmpVar"></param>
        /// <param name="label1"></param>
        /// <param name="label2"></param>
        public void AddBranch(LLVMVar cmpVar, LLVMVar label1, LLVMVar label2)
        {
            string command = $"br {cmpVar.TypeName} {cmpVar.NameInFunction}, label {label1.NameInFunction}";
            if (label2 != null) command += $", label {label2.NameInFunction}";

            Add(new CodeSnippet(CodeType.Command.Branch, command));
        }

        /// <summary>
        /// Create unconditional branch code.
        /// </summary>
        /// <param name="label"></param>
        public void AddBranch(LLVMVar label) => Add(new CodeSnippet(CodeType.Command.Branch, $"br label {label.NameInFunction}"));

        public void AddLabel(LLVMVar label, string comment = "")
        {
            string command = $"{label.NameInFunction.Replace("%", "")}: ";
            if (!string.IsNullOrEmpty(comment)) command += $"\t\t\t ; {comment}";

            Add(new CodeSnippet(CodeType.Command.Label, command));
        }


        public void AddCall(LLVMVar returnVar, string funcName, IEnumerable<LLVMVar> @params)
        {
            string data = $"{returnVar.NameInFunction} = call {returnVar.Type.LLVMTypeName} {funcName}";
            data += GetCallParamCommand(@params);

            Add(new CodeSnippet(CodeType.Command.Call, data));
        }

        public void AddCall(string funcName, IEnumerable<LLVMVar> @params)
        {
            string data = $"call void {funcName}";
            data += GetCallParamCommand(@params);

            Add(new CodeSnippet(CodeType.Command.Call, data));
        }

        public bool IsExistLabel()
        {
            bool result = false;

            foreach(var code in this)
            {
                if(code.CodeType == CodeType.Command.Label)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }



        private string GetCallParamCommand(IEnumerable<LLVMVar> @params)
        {
            string data = "(";
            for (int i = 0; i < @params.Count(); i++)
            {
                var param = @params.ElementAt(i);

                data += $"{param.Type.LLVMTypeName} noundef ";
                if (param is LLVMLiteralVar) data += $"{(param as LLVMLiteralVar).Value}";
                else data += $"{param.NameInFunction}";

                if (i < @params.Count() - 1) data += ", ";
            }
            data += ")";

            return data;
        }


        private string GetArithmeticCommand(LLVMVar toVar, LLVMVar fromVar1, IRLiteralExpr fromValue, IRBinaryOperation operation)
        {
            return $"{GetArithmeticCommon(toVar, fromVar1, fromValue.Type, operation)}, {fromValue}";
        }

        private string GetArithmeticCommand(LLVMVar toVar, LLVMVar fromVar1, LLVMVar fromVar2, IRBinaryOperation operation)
        {
            return $"{GetArithmeticCommon(toVar, fromVar1, fromVar2.Type, operation)}, {fromVar2.NameInFunction}";
        }

        private string GetArithmeticCommon(LLVMVar toVar, LLVMVar fromVar1, IRType fromVar2Type, IRBinaryOperation operation)
        {
            string oper = (operation == IRBinaryOperation.Add) ? "add"
                 : (operation == IRBinaryOperation.Sub) ? "sub"
                 : (operation == IRBinaryOperation.Mul) ? "mul"
                 : (operation == IRBinaryOperation.Div) ? "div"
                 : (operation == IRBinaryOperation.Mod) ? "rem"
                 : throw new Exception();


            string unsign = string.Empty;
            if (operation == IRBinaryOperation.Div || operation == IRBinaryOperation.Mod)
                unsign = (fromVar1.Type.IsUnsigned || fromVar2Type.IsUnsigned) ? "u" : "s";

            return $"{toVar.NameInFunction} = {unsign}{oper} {toVar.TypeName} {fromVar1.NameInFunction}";
        }
    }

    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class CodeSnippet
    {
        public CodeSnippet(CodeType codeType, string data)
        {
            CodeType = codeType;
            Data = data;
        }

        public CodeType CodeType { get; }
        public string Data { get; }

        private string GetDebuggerDisplay() => Data;
    }
}
