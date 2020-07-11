using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using Parse.MiddleEnd.IR.LLVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM
{
    public partial class LLVMAssemblyBuilder : IRBuilder
    {
        private SSTable _ssVarTable = new SSTable();

        public override bool IsSSA => true;

        // sample format
        // a++
        // %1 = load i32, i32* @a, align 4 ; <-- use this.
        // %2 = add nsw i32 %1, 1
        // store i32 %2, i32* @a, align 4
        private Tuple<IReadOnlyList<SSNode>,IRBlock> CommonIncLogic(IRVar ssVar, IROptions options)
        {
            var instruction1 = Instruction.Load(ssVar, _ssVarTable);
            var firstNode = instruction1.Result;

            var instruction2 = Instruction.BinOp(firstNode.SSF, new SSValue<Int>(1), _ssVarTable, IROperation.Add);
            var secNode = _ssVarTable.LastInLocalList();

            var instruction3 = Instruction.Store(secNode.SSF, ssVar);

            var ssVarList = new List<SSNode>() { firstNode, secNode };
            var block = new IRBlock(options.Label, options.Comment)
            {
                instruction1,
                instruction2,
                instruction3
            };

            return new Tuple<IReadOnlyList<SSNode>, IRBlock>(ssVarList, block);
        }

        // sample format
        // int a;
        // @a = [common] global i32 0, align 4
        private IRFormat DclGlobalVar(IROptions options, IRVar varData)
        {
            var instruction = Instruction.DeclareGlobalVar(varData, options.Comment);

            return new IRFormat(instruction);
        }

        // sample format
        // case1
        // int a;
        // %1 = alloca i32, align 4
        private IRFormat DclLocalVar(IROptions options, IRVar varData)
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
        private IRFormat AssignToLocalVar(IROptions option, IRVar t, IRVar s)
        {
            var tNode = _ssVarTable.GetNode(t);
            var sNode = _ssVarTable.GetNode(s);

            var block = new IRBlock(option.Label, option.Comment)
            {
                // temperory var is not added to the localOffsetDic.
                Instruction.Load(tNode.SSF, _ssVarTable),
                Instruction.Store(sNode.SSF, tNode.SSF)
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

        public override IRFormat CreateDclVar(IROptions options, IRVar VarData, bool bGlobal)
            => (bGlobal == true) ? DclGlobalVar(options, VarData) : DclLocalVar(options, VarData);

        public override IRFormat CreateDclVarAndInit(IROptions options, IRVar VarData, IRVar initInfo, bool bGlobal)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateLoadVar(IROptions options, IRVar varData, bool bGlobal)
        {
            var node = _ssVarTable.GetNode(varData);
            var instruction = Instruction.Load(node.SSF, _ssVarTable, options.Comment);

            return new IRFormat(instruction);
        }

        public override IRFormat CreateDclVarAndInit(IROptions options, IRVar VarData, IRValue initValue, bool bGlobal)
        {
            throw new System.NotImplementedException();
        }

        // sample format
        // c + 10; or 10 + c;
        // ---------------------------------------
        //  %1 = load i32, i32* @c, align 4
        //  %2 = add nsw i32 %1, 10
        // ---------------------------------------
        private IRFormat CreateBinOp(IROptions options, IRVar left, IRValue right, IROperation operation)
        {
            var instruction1 = Instruction.Load(left, _ssVarTable);
            var loadedVar = instruction1.Result;
            var ssValue = LLVMConverter.ToSSValue(right);
            var instruction2 = Instruction.BinOp(loadedVar.SSF, ssValue, _ssVarTable, operation);

            var block = new IRBlock()
            {
                instruction1, instruction2
            };

            return new IRFormat(block, _ssVarTable.LastInLocalList().SSF);
        }

        // sample format
        // a + b;
        // assume a = @a, b = @b
        // ---------------------------------------
        //  %1 = load i32, i32* @a, align 4
        //  %2 = load i32, i32* @b, align 4
        //  %3 = add nsw i32 %1, %2
        // ---------------------------------------
        private IRFormat CreateBinOp(IROptions options, IRVar left, IRVar right, IROperation operation)
        {
            var instruction1 = Instruction.Load(left, _ssVarTable);
            var leftNewSSVar = instruction1.Result.SSF;
            var instruction2 = Instruction.Load(right, _ssVarTable);
            var rightNewSSVar = instruction2.Result.SSF;

            var instruction3 = Instruction.BinOp(leftNewSSVar, rightNewSSVar, _ssVarTable, operation);

            var block = new IRBlock()
            {
                instruction1, instruction2, instruction3
            };

            return new IRFormat(block, _ssVarTable.LastInLocalList().SSF);
        }

        // sample format
        // c + 10;
        // ---------------------------------------
        //  %1 = load i32, i32* @c, align 4
        //  %2 = add nsw i32 %1, 10
        // ---------------------------------------
        public override IRFormat CreateBinOP(IROptions options, IRData left, IRData right, IROperation operation)
        {
            IRVar leftSSVar = (left is IRVar) ? _ssVarTable.GetNode(left as IRVar).SSF : // ssVar for left (ex : @c);
                                                                null;

            IRVar rightSSVar = (right is IRVar) ? _ssVarTable.GetNode(right as IRVar).SSF :  // ssVar for right (ex : @c)
                                                                null;


            // if both left and right is LiteralData
            if (leftSSVar is null && rightSSVar is null)
            {
                var literalLeft = left as IRValue;
                var literalRight = right as IRValue;

                return new IRFormat(null, literalLeft.BinOp(literalRight, operation));
            }

            if (leftSSVar is null) return CreateBinOp(options, rightSSVar, left as IRValue, operation);
            if (rightSSVar is null) return CreateBinOp(options, leftSSVar, right as IRValue, operation);

            // if both left and right is SSVarData
            return CreateBinOp(options, leftSSVar, rightSSVar, operation);
        }

        public override IRFormat CreateCall(IROptions options, IRFuncData funcData, params IRVar[] paramDatas)
        {
            /*
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
            */

            return null;
        }

        public override IRFormat CreatePreInc(IROptions options, IRVar varData)
        {
            var node = _ssVarTable.GetNode(varData); // ex : @a
            var result = CommonIncLogic(node.SSF, options);

            return new IRFormat(result.Item2, result.Item1.Last().SSF);
        }

        public override IRFormat CreatePreDec(IROptions options, IRVar varData)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreatePostInc(IROptions options, IRVar varData)
        {
            var node = _ssVarTable.GetNode(varData); // ex : @a
            var result = CommonIncLogic(node.SSF, options);

            return new IRFormat(result.Item2, result.Item1.First().SSF);
        }

        public override IRFormat CreatePostDec(IROptions options, IRVar varData)
        {
            throw new System.NotImplementedException();
        }

        public override IRFormat CreateNot(IROptions options, IRData data)
        {
            throw new NotImplementedException();
        }

        /// ============================================================================
        /// <summary>
        /// This function creates IR for And operation.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="left">it can be IRVar(variable) or IRValue(literal) or IRCondVar(bool (result of operation))</param>
        /// <param name="right">it can be IRVar(variable) or IRValue(literal) or IRCondVar(bool (result of operation))</param>
        /// <returns></returns>
        /// ============================================================================
        public override IRFormat CreateAnd(IROptions options, IRData<Bit> left, IRData<Bit> right)
        {
            var leftResult = CondHalfLogic(options, left);          // create load, cmp
            if (leftResult == null) return new IRFormat(null, new SSValue<Bit>(false));

            var rightResult = CondHalfLogic(options, right);        // create load, cmp
            if (rightResult == null) return new IRFormat(null, new SSValue<Bit>(false));

            // create br for right block to the leftBlock.
            var leftCmpNode = (leftResult.Item1.Last() as Instruction).Result;
            leftResult.Item1.Add(Instruction.CBranch(leftCmpNode.SSF as LocalVar<Bit>, leftResult.Item2, rightResult.Item2));

            // create br for right block to the leftBlock.
            var rightCmpNode = (leftResult.Item1.Last() as Instruction).Result;
            rightResult.Item1.Add(Instruction.CBranch(rightCmpNode.SSF as LocalVar<Bit>, leftResult.Item2, rightResult.Item2)); // ??

            return null;
//            return CreateLogicalOp(options, left, right, IRCondition.NE);
        }

        // sample format
        // <result> = load <ty>, <ty>* <left>, align <left.align>
        // [<result> = sext <ty> <op> to i32]
        // [<result> = sitofp i32 <op> to double] || [<result> = uitofp i32 <op> to double]
        // <result> = fcmp <cond> <ty> <left>, <right>
        private IRFormat CreateLogicalOp(IROptions options, IRVar left, IRValue right, IRCondition condition)
        {
            IRBlock irBlock = new IRBlock();
            var isItoFpCond = LLVMChecker.IsItoFpCondition(left.TypeName, right.TypeName);

            irBlock.AddRange(LoadAndExtLogic(left, isItoFpCond));

            var ssf = (irBlock.SecondLast() as Instruction).Result.SSF;
            // icmp or fcmp
            if (left.TypeName == DType.Double || right.TypeName == DType.Double)
                irBlock.Add(Instruction.Fcmp(condition, 
                                                            ssf as LocalVar<DoubleType>, 
                                                            right as SSValue<DoubleType>, 
                                                            _ssVarTable));
            else
                irBlock.Add(Instruction.Icmp(condition, 
                                                            ssf as LocalVar<Int>, 
                                                            right as SSValue<Int>, 
                                                            _ssVarTable));

            return new IRFormat(irBlock, (irBlock.Last() as Instruction).Result.SSF);
        }

        // sample format
        // <result> = load i32, <ty>* <op>, align <align>
        // <result> = load i32, <ty>* <op>, align <align>
        // [<result> = sext <ty> <op> to i32]*
        // [<result> = sitofp i32 <op> to double] || [<result> = uitofp i32 <op> to double]
        // <result> = icmp eq i32 <op1>, <op2>
        private IRFormat CreateLogicalOp(IROptions options, IRVar left, IRVar right, IRCondition condition)
        {
            var irBlock = new IRBlock();
            var isItoFpCond = LLVMChecker.IsItoFpCondition(left.TypeName, right.TypeName);

            irBlock.AddRange(LoadAndExtLogic(left, isItoFpCond));
            irBlock.AddRange(LoadAndExtLogic(right, isItoFpCond));

            // icmp or fcmp
            if (left.TypeName == DType.Double || right.TypeName == DType.Double)
                irBlock.Add(Instruction.Fcmp(condition, 
                                                            (irBlock.SecondLast() as Instruction).Result.SSF as LocalVar<DoubleType>, 
                                                            (irBlock.Last() as Instruction).Result.SSF as LocalVar<DoubleType>, 
                                                            _ssVarTable));
            else
                irBlock.Add(Instruction.Icmp(condition, 
                                                            (irBlock.SecondLast() as Instruction).Result.SSF as LocalVar<Int>, 
                                                            (irBlock.Last() as Instruction).Result.SSF as LocalVar<Int>, 
                                                            _ssVarTable));

            return new IRFormat(irBlock, (irBlock.Last() as Instruction).Result.SSF);
        }

        public override IRFormat CreateLogicalOp(IROptions options, IRData left, IRData right, IRCondition condition)
        {
            IRVar leftSSVar = (left is IRVar) ? _ssVarTable.GetNode(left as IRVar).SSF :  // ssVar for left (ex : @c);
                                                                null;

            IRVar rightSSVar = (right is IRVar) ? _ssVarTable.GetNode(right as IRVar).SSF :  // ssVar for right (ex : @c)
                                                                null;

            if (leftSSVar is null && rightSSVar is null)
            {
                var literalLeft = left as IRValue;
                var literalRight = right as IRValue;

                var result = LLVMChecker.LogicalOp(literalLeft, literalRight, condition);
                return new IRFormat(null, result);
            }

            if (leftSSVar is null) return CreateLogicalOp(options, rightSSVar, left as IRValue, condition);
            if (rightSSVar is null) return CreateLogicalOp(options, leftSSVar, right as IRValue, condition);

            // if both left and right is SSVarData
            return CreateLogicalOp(options, leftSSVar, rightSSVar, condition);
        }

        public override IRFormat CreateOr(IROptions options, IRData<Bit> left, IRData<Bit> right)
        {
            throw new NotImplementedException();
        }

        public override IRFormat CreateAssign(IROptions options, IRVar left, IRData right)
        {
            throw new NotImplementedException();
        }

        public override IRFormat CretaeConditionalJump(IROptions options, IRVar<Bit> cond, IRVar<Bit> trueLabel, IRVar<Bit> falseLabel)
        {
            throw new NotImplementedException();
        }

        public override IRFormat CretaeUnConditionalJump(IROptions options, IRVar<Bit> jumpLabel)
        {
            throw new System.NotImplementedException();
        }
    }
}
