using Parse.FrontEnd.InterLanguages.Datas;
using Parse.FrontEnd.InterLanguages.LLVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.InterLanguages.LLVM
{
    public class LLVMAssemblyBuilder : IRBuilder
    {
        private SSVarTable _ssVarTable = new SSVarTable();
        private Stack<LocalSSVar> _reservedData = new Stack<LocalSSVar>();

        public override bool IsSSA => true;

        // sample format
        // a++
        // %1 = load i32, i32* @a, align 4 ; <-- use this.
        // %2 = add nsw i32 %1, 1
        // store i32 %2, i32* @a, align 4
        private Tuple<IReadOnlyList<LocalSSVar>,IRBlock> CommonIncLogic(SSVarData ssVar, IROptions options)
        {
            var instruction1 = Instruction.Load(ssVar, _ssVarTable);
            var firstSSVar = _ssVarTable.LastInLocalList();

            var instruction2 = Instruction.Add(ssVar, _ssVarTable, 1);
            var secSSVar = _ssVarTable.LastInLocalList();

            var instruction3 = Instruction.Store(secSSVar, ssVar);

            var ssVarList = new List<LocalSSVar>() { firstSSVar, secSSVar };
            var block = new IRBlock(options.Label, options.Comment)
            {
                instruction1,
                instruction2,
                instruction3
            };

            return new Tuple<IReadOnlyList<LocalSSVar>, IRBlock>(ssVarList, block);
        }

        // sample format
        // int a;
        // @a = [common] global i32 0, align 4
        private IRFormat DclGlobalVar(IROptions options, VarData varData)
        {
            var instruction = Instruction.DeclareGlobalVar(varData.Name, varData.Type, options.Comment);

            return new IRFormat(instruction);
        }

        // sample format
        // case1
        // int a;
        // %1 = alloca i32, align 4
        private IRFormat DclLocalVar(IROptions options, VarData varData)
            => new IRFormat(Instruction.DeclareLocalVar(varData, _ssVarTable, options.Comment));

        // sample format
        // a(t) = b(s);
        // if both a and b is local var --------
        // assume [%1 = a, %2 = b]--------
        // %3 = load i32, i32* %2, align 4
        // store i32 %3, i32* %1, align 4
        // ---------------------------------------
        // if a is global var --------------------
        // %3 = load i32, i32* %2, align 4
        // store i32 %3, i32* @a, align 4
        // ---------------------------------------
        private IRFormat AssignToLocalVar(IROptions option, VarData t, VarData s)
        {
            var tSSVar = _ssVarTable.Get(t);
            var sSSVar = _ssVarTable.Get(s);

            var block = new IRBlock(option.Label, option.Comment)
            {
                // temperory var is not added to the localOffsetDic.
                Instruction.Load(tSSVar, _ssVarTable),
                Instruction.Store(sSSVar, tSSVar)
            };

            return new IRFormat(block);
        }

        public override IRFormat CreateDclFunction(IROptions options, FuncData funcData)
        {
            throw new System.NotImplementedException();
        }

        // sample format
        // int main(){ stmt }
        // define i32 @main(){ stmt }
        public override IRFormat CreateDefineFunction(IROptions options, FuncData funcData, IRBlock stmts)
        {
            return new IRFormat(new Function(funcData, stmts));
        }

        public override IRFormat CreateDclVar(IROptions options, VarData VarData, bool bGlobal)
            => (bGlobal == true) ? DclGlobalVar(options, VarData) : DclLocalVar(options, VarData);

        public override IRFormat CreateDclVarAndInit(IROptions options, VarData VarData, VarData initInfo, bool bGlobal)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateLoadVar(IROptions options, VarData varData, bool bGlobal)
        {
            var ssVar = _ssVarTable.Get(varData);
            var instruction = Instruction.Load(ssVar, _ssVarTable, options.Comment);

            return new IRFormat(instruction);
        }

        public override IRFormat CreateDclVarAndInit(IROptions options, VarData VarData, LiteralData initValue, bool bGlobal)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateBinOP(IROptions options, VarData left, VarData right, IROperation operation)
        {
            throw new System.NotImplementedException();
        }

        // sample format
        // c + 10;
        // ---------------------------------------
        //  %1 = load i32, i32* @c, align 4
        //  %2 = add nsw i32 %1, 10
        // ---------------------------------------
        public override IRFormat CreateBinOP(IROptions options, VarData left, LiteralData right, IROperation operation)
        {
            if (_reservedData.Count > 0)
            {
                var newSSVar = _reservedData.Pop();
                var instruction = (operation == IROperation.Add) ? Instruction.Add(newSSVar, _ssVarTable, right.Value) :
                                                                            null;

                var block = new IRBlock(options.Label, options.Comment) { instruction };
                return new IRFormat(block);
            }
            else
            {
                var ssVar = _ssVarTable.Get(left); // ssVar for left (ex : @c)

                var instruction1 = Instruction.Load(ssVar, _ssVarTable);
                var newSSVar = _ssVarTable.LastInLocalList(); // ex : %1

                var instruction2 = (operation == IROperation.Add) ? Instruction.Add(newSSVar, _ssVarTable, right.Value) :
                                                                            null;

                var block = new IRBlock(options.Label, options.Comment)
                {
                    instruction1,
                    instruction2
                };

                return new IRFormat(block);
            }
        }

        public override IRFormat CreateBinOP(IROptions options, LiteralData left, VarData right, IROperation operation)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateBinOP(IROptions options, LiteralData left, LiteralData right, IROperation operation)
        {
            _reservedData.Push()

            return null;
        }

        public override IRFormat CreateCall(IROptions options, FuncData funcData, params VarData[] paramDatas)
        {
            var result = new IRBlock();
            string callIns = (funcData.ReturnType == ReturnType.Void) ?
                string.Format("call {0} @{1}", funcData.ReturnType, funcData.Name) :
                string.Format("%{0} = call {1} @{2}",  funcData.ReturnType, funcData.Name);

            callIns += "(";

            foreach(var arg in funcData.Arguments)
                string.Format("{0} {1}, ", arg.Type, (arg.IsGlobal) ? "@" + arg.Name : "%" + arg.Offset);

            if (funcData.Arguments.Count > 0) callIns = callIns.Substring(0, callIns.Length - 2);
            callIns += ")";

            result.Add(new Instruction(callIns, string.Format("call {0} func", funcData.Name)));
            return result;
        }

        public override IRFormat CreatePreInc(IROptions options, VarData varData)
        {
            var ssVar = _ssVarTable.Get(varData); // ex : @a

            var result = CommonIncLogic(ssVar, options);
            _reservedData.Push(result.Item1.Last());

            return new IRFormat(result.Item2);
        }

        public override IRFormat CreatePreDec(IROptions options, VarData varData)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreatePostInc(IROptions options, VarData varData)
        {
            var ssVar = _ssVarTable.Get(varData); // ex : @a

            var result = CommonIncLogic(ssVar, options);
            _reservedData.Push(result.Item1.First());

            return new IRFormat(result.Item2);
        }

        public override IRFormat CreatePostDec(IROptions options, VarData varData)
        {
            throw new System.NotImplementedException();
        }
    }
}
