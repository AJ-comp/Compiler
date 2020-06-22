using Parse.Extensions;
using Parse.FrontEnd.InterLanguages.Datas;
using Parse.FrontEnd.InterLanguages.LLVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.InterLanguages.LLVM
{
    public partial class LLVMAssemblyBuilder : IRBuilder
    {
        private SSVarTable _ssVarTable = new SSVarTable();

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

            var instruction2 = Instruction.BinOp(ssVar, new IRIntegerLiteralData(1), _ssVarTable, IROperation.Add);
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
        private IRFormat DclGlobalVar(IROptions options, IRVarData varData)
        {
            var instruction = Instruction.DeclareGlobalVar(varData.Name, varData.Type, options.Comment);

            return new IRFormat(instruction);
        }

        // sample format
        // case1
        // int a;
        // %1 = alloca i32, align 4
        private IRFormat DclLocalVar(IROptions options, IRVarData varData)
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
        private IRFormat AssignToLocalVar(IROptions option, IRVarData t, IRVarData s)
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

        public override IRFormat CreateDclFunction(IROptions options, IRFuncData funcData)
        {
            throw new System.NotImplementedException();
        }

        // sample format
        // int main(){ stmt }
        // define i32 @main(){ stmt }
        public override IRFormat CreateDefineFunction(IROptions options, IRFuncData funcData, IRBlock stmts)
        {
            return new IRFormat(new Function(funcData, stmts));
        }

        public override IRFormat CreateDclVar(IROptions options, IRVarData VarData, bool bGlobal)
            => (bGlobal == true) ? DclGlobalVar(options, VarData) : DclLocalVar(options, VarData);

        public override IRFormat CreateDclVarAndInit(IROptions options, IRVarData VarData, IRVarData initInfo, bool bGlobal)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateLoadVar(IROptions options, IRVarData varData, bool bGlobal)
        {
            var ssVar = _ssVarTable.Get(varData);
            var instruction = Instruction.Load(ssVar, _ssVarTable, options.Comment);

            return new IRFormat(instruction);
        }

        public override IRFormat CreateDclVarAndInit(IROptions options, IRVarData VarData, IRLiteralData initValue, bool bGlobal)
        {
            throw new System.NotImplementedException();
        }

        // sample format
        // c + 10; or 10 + c;
        // ---------------------------------------
        //  %1 = load i32, i32* @c, align 4
        //  %2 = add nsw i32 %1, 10
        // ---------------------------------------
        private IRFormat CreateBinOp(IROptions options, SSVarData left, IRLiteralData right, IROperation operation)
        {
            var instruction1 = Instruction.Load(left, _ssVarTable);
            var loadedVar = instruction1.Result;
            var instruction2 = Instruction.BinOp(loadedVar, right, _ssVarTable, operation);

            var block = new IRBlock()
            {
                instruction1, instruction2
            };

            return new IRFormat(block, _ssVarTable.LastInLocalList());
        }

        // sample format
        // a + b;
        // assume a = @a, b = @b
        // ---------------------------------------
        //  %1 = load i32, i32* @a, align 4
        //  %2 = load i32, i32* @b, align 4
        //  %3 = add nsw i32 %1, %2
        // ---------------------------------------
        private IRFormat CreateBinOp(IROptions options, SSVarData left, SSVarData right, IROperation operation)
        {
            var instruction1 = Instruction.Load(left, _ssVarTable);
            var leftNewSSVar = instruction1.Result;
            var instruction2 = Instruction.Load(right, _ssVarTable);
            var rightNewSSVar = instruction2.Result;

            var instruction3 = Instruction.BinOp(leftNewSSVar, rightNewSSVar, _ssVarTable, operation);

            var block = new IRBlock()
            {
                instruction1, instruction2, instruction3
            };

            return new IRFormat(block, _ssVarTable.LastInLocalList());
        }

        // sample format
        // c + 10;
        // ---------------------------------------
        //  %1 = load i32, i32* @c, align 4
        //  %2 = add nsw i32 %1, 10
        // ---------------------------------------
        public override IRFormat CreateBinOP(IROptions options, IRData left, IRData right, IROperation operation)
        {
            SSVarData leftSSVar = (left is IRVarData) ? _ssVarTable.Get(left as IRVarData) :  // ssVar for left (ex : @c);
                                            (left is SSVarData) ? left as SSVarData : null;

            SSVarData rightSSVar = (right is IRVarData) ? _ssVarTable.Get(right as IRVarData) :  // ssVar for right (ex : @c)
                                            (right is SSVarData) ? right as SSVarData : null;


            // if both left and right is LiteralData
            if (leftSSVar is null && rightSSVar is null)
            {
                var literalLeft = left as IRLiteralData;
                var literalRight = right as IRLiteralData;

                return new IRFormat(null, literalLeft.BinOp(literalRight, operation));
            }

            if (leftSSVar is null) return CreateBinOp(options, rightSSVar, left as IRLiteralData, operation);
            if (rightSSVar is null) return CreateBinOp(options, leftSSVar, right as IRLiteralData, operation);

            // if both left and right is SSVarData
            return CreateBinOp(options, leftSSVar, rightSSVar, operation);
        }

        public override IRFormat CreateCall(IROptions options, IRFuncData funcData, params IRVarData[] paramDatas)
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

        public override IRFormat CreatePreInc(IROptions options, IRVarData varData)
        {
            var ssVar = _ssVarTable.Get(varData); // ex : @a
            var result = CommonIncLogic(ssVar, options);

            return new IRFormat(result.Item2, result.Item1.Last());
        }

        public override IRFormat CreatePreDec(IROptions options, IRVarData varData)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreatePostInc(IROptions options, IRVarData varData)
        {
            var ssVar = _ssVarTable.Get(varData); // ex : @a
            var result = CommonIncLogic(ssVar, options);

            return new IRFormat(result.Item2, result.Item1.First());
        }

        public override IRFormat CreatePostDec(IROptions options, IRVarData varData)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateNot(IROptions options, IRData data)
        {
            throw new NotImplementedException();
        }

        public override IRFormat CreateAnd(IROptions options, IRData left, IRData right)
        {
            throw new NotImplementedException();
        }

        // sample format
        // <result> = load <ty>, <ty>* <left>, align <left.align>
        // [<result> = sext <ty> <op> to i32]
        // [<result> = sitofp i32 <op> to double] || [<result> = uitofp i32 <op> to double]
        // <result> = fcmp <cond> <ty> <left>, <right>
        private IRFormat CreateLogicalOp(IROptions options, SSVarData left, IRLiteralData right, IRCondition cond)
        {
            List<Instruction> insList = new List<Instruction>();
            var isItoFpCond = LLVMChecker.IsItoFpCondition(left.Type, right.Type);

            LoadAndExp(left, isItoFpCond);
        }

        // sample format
        // <result> = load i32, <ty>* <op>, align <align>
        // <result> = load i32, <ty>* <op>, align <align>
        // [<result> = sext <ty> <op> to i32]*
        // [<result> = sitofp i32 <op> to double] || [<result> = uitofp i32 <op> to double]
        // <result> = icmp eq i32 <op1>, <op2>
        private IRFormat CreateLogicalOp(IROptions options, SSVarData left, SSVarData right, IRCondition cond)
        {
            List<Instruction> insList = new List<Instruction>();
            var isItoFpCond = LLVMChecker.IsItoFpCondition(left.Type, right.Type);

            LoadAndExp(left, isItoFpCond);
            LoadAndExp(right, isItoFpCond);

            // icmp or fcmp
            if (LLVMChecker.IsDoubleType(left.Type, right.Type))
                insList.Add(Instruction.Fcmp(cond, insList.SecondLast().Result as LocalDoubleSSVar, insList.Last().Result as LocalDoubleSSVar, _ssVarTable));
            else
                insList.Add(Instruction.Icmp(cond, insList.SecondLast().Result as LocalIntSSVar, insList.Last().Result as LocalIntSSVar, _ssVarTable));

            var irBlock = new IRBlock();
            foreach (var instruction in insList) irBlock.Add(instruction);

            return new IRFormat(irBlock, insList.Last().Result);
        }

        public override IRFormat CreateLogicalOp(IROptions options, IRData left, IRData right, IRCondition cond)
        {
            SSVarData leftSSVar = (left is IRVarData) ? _ssVarTable.Get(left as IRVarData) :  // ssVar for left (ex : @c);
                                            (left is SSVarData) ? left as SSVarData : null;

            SSVarData rightSSVar = (right is IRVarData) ? _ssVarTable.Get(right as IRVarData) :  // ssVar for right (ex : @c)
                                            (right is SSVarData) ? right as SSVarData : null;

            if (leftSSVar is null && rightSSVar is null)
            {
                var literalLeft = left as IRLiteralData;
                var literalRight = right as IRLiteralData;

                return new IRFormat(null, literalLeft.LogicalOp(literalRight, cond));
            }

            if (leftSSVar is null) return CreateLogicalOp(options, rightSSVar, left as IRLiteralData, cond);
            if (rightSSVar is null) return CreateLogicalOp(options, leftSSVar, right as IRLiteralData, cond);

            // if both left and right is SSVarData
            return CreateLogicalOp(options, leftSSVar, rightSSVar, cond);
        }
    }
}
