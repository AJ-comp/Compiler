using Parse.FrontEnd.InterLanguages.Datas;
using Parse.FrontEnd.InterLanguages.LLVM.Models;
using System;
using System.Linq;

namespace Parse.FrontEnd.InterLanguages.LLVM
{
    public partial class LLVMAssemblyBuilder : IRBuilder
    {
        private Tuple<IRBlock, IRCondVar, LocalVar> CondHalfLogic(IROptions options, IRData target)
        {
            IRBlock result = new IRBlock();

            IRCondVar leftCondVar = null;
            SSNode toRightLabelNode = null;
            if ((target is IRLiteral) == false)            // IRLiteral type is ignored.
            {
                // if target is IRVar then it has to convert to CondVar.
                result.AddRange(ToCondVarLogic(target));

                leftCondVar = (result.Count == 0) ? target as IRCondVar :           // if target is CondVar
                                                                    new IRCondVar((bool)(result.Last() as Instruction).Result.LinkedValue);           // else 

                // create a label to go to next exp.
                toRightLabelNode = _ssVarTable.NewNode(false);

                return new Tuple<IRBlock, IRCondVar, LocalVar>(result, leftCondVar, toRightLabelNode.SSF);
            }

            return null;
        }

        private IRBlock ToCondVarLogic(IRData data)
        {
            var result = new IRBlock();
            var ssVar = _ssVarTable.Get(data as IRVar);

            // load
            // cmp
            if (data is IRVar)
            {
                var loadIns = Instruction.Load(ssVar, _ssVarTable);
                result.Add(loadIns);

                if (loadIns.Result.SSF.Type == DataType.Double)
                    result.Add(Instruction.Fcmp(IRCondition.NE, 
                                                                loadIns.Result.SSF, 
                                                                new IRDoubleLiteral(0), 
                                                                _ssVarTable));
                else
                    result.Add(Instruction.Icmp(IRCondition.NE, 
                                                                loadIns.Result.SSF, 
                                                                new IRIntegerLiteral(0), 
                                                                _ssVarTable));

                return result;
            }
            else if(data is IRLiteral)
            {

            }

            return null;
        }

        private IRBlock LoadAndExtLogic(ISSVar target, bool isItoFpCond)
        {
            IRBlock result = new IRBlock
            {
                // load
                Instruction.Load(target, _ssVarTable)
            };

            // sext (if a condition is met)
            result.Add(Instruction.SExt((result.Last() as Instruction).Result.SSF, _ssVarTable));
            if (isItoFpCond)
                result.Add(Instruction.IToFp((result.Last() as Instruction).Result.SSF, _ssVarTable)); // sitofp or uitofp

            return result;
        }
    }
}
