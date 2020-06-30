using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Models;
using System;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM
{
    public partial class LLVMAssemblyBuilder : IRBuilder
    {
        private Tuple<IRBlock, LocalVar> CondHalfLogic(IROptions options, IRData target)
        {
            if (target is IRValue)
            {
                // if value of the left is 0 it doesn't needs seeing all logic because And logic.
                var literal = target as IRValue;
                if (literal.IsZero) return null;
            }
            else if (target is IRCond)
            {
                //// if value of the left is false it doesn't needs see all logic because And logic.
                //var cond = target as IRCondVar;
                //if (!cond.Value) return null;
            }
            else
            {
                var leftResult = CondHalfLogic(options, target as IRVar);          // create load, cmp
            }
        }

        private Tuple<IRBlock, LocalVar> CondHalfLogic(IROptions options, IRVar target)
        {
            IRBlock result = new IRBlock();

            // if target is IRVar then it has to convert to CondVar.
            result.AddRange(ToCondVarLogic(target));

            var leftCondVar = new IRCond((bool)(result.Last() as Instruction).Result.LinkedValue);           // else 

            // create a label to go to next exp.
            var toRightLabelNode = _ssVarTable.NewNode(false);

            return new Tuple<IRBlock, LocalVar>(result, toRightLabelNode.SSF);
        }

        private IRBlock ToCondVarLogic(IRVar data)
        {
            var result = new IRBlock();
            var node = _ssVarTable.GetNode(data as IRVar);

            // load
            // cmp
            var loadIns = Instruction.Load(node.SSF, _ssVarTable);
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
