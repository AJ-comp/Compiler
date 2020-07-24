﻿using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using Parse.MiddleEnd.IR.LLVM.Models;
using System;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM
{
    public partial class LLVMAssemblyBuilder : IRBuilder
    {
        private Tuple<IRBlock, LocalVar<Bit>> CondHalfLogic(IROptions options, IRData<Bit> target)
        {
            if (target is IRVar<Bit>) return CondHalfLogic(options, target as IRVar<Bit>);
            else if (target is IRValue<Bit>)
                ;

            return null;
        }

        private Tuple<IRBlock, LocalVar<Bit>> CondHalfLogic(IROptions options, IRVar<Bit> target)
        {
            IRBlock result = new IRBlock();

            // if target is IRVar then it has to convert to bit value.
            result.AddRange(ToCondVarLogic(target));
//            var leftCondVar = new SSValue<Bit>((bool)(result.Last() as Instruction).Result.LinkedValue);           // else 

            // create a label to go to next exp.
            var toRightLabelNode = _ssVarTable.NewNode(false);
            return new Tuple<IRBlock, LocalVar<Bit>>(result, toRightLabelNode.SSF as LocalVar<Bit>);
        }

        private IRBlock ToCondVarLogic(IRVar<Bit> data)
        {
            var result = new IRBlock();
            var node = _ssVarTable.GetNode(data);

            // load
            // cmp
            var loadIns = Instruction.Load(node.SSF, _ssVarTable);
            result.Add(loadIns);

            if (loadIns.Result.SSF.TypeName == DType.Double)
                result.Add(Instruction.Fcmp(IRCondition.NE, 
                                                            loadIns.Result.SSF as LocalVar<DoubleType>, 
                                                            new SSValue<DoubleType>(0), 
                                                            _ssVarTable));
            else
                result.Add(Instruction.Icmp(IRCondition.NE, 
                                                            loadIns.Result.SSF as LocalVar<Int>, 
                                                            new SSValue<Int>(0), 
                                                            _ssVarTable));

            return result;
        }

        private IRBlock LoadAndExtLogic(IRVar target, bool isItoFpCond)
        {
            IRBlock result = new IRBlock
            {
                // load
                Instruction.Load(target, _ssVarTable)
            };

            var ssf = (result.Last() as Instruction).Result.SSF;
            // sext (if a condition is met)
            result.Add(Instruction.SExt(ssf as LocalVar<Integer>, _ssVarTable));
            if (isItoFpCond)
                result.Add(Instruction.IToFp(ssf as LocalVar<Integer>, _ssVarTable)); // sitofp or uitofp

            return result;
        }
    }
}