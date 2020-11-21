﻿using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMFuncDefExpression : LLVMFirstLayerExpression
    {
        public IRFuncData FuncData { get; private set; }
        public LLVMParamListAndReturnExpression ParamListAndReturnExpression { get; }
        public LLVMBlockExpression BlockExpression { get; private set; }
        public int FuncIndex { get; private set; }

        public LLVMFuncDefExpression(IRFuncData funcData,
                                                        LLVMParamListAndReturnExpression paramAndReturnExpression,
                                                        LLVMBlockExpression blockExpression, 
                                                        LLVMSSATable ssaTable,
                                                        int funcIndex) : base(ssaTable)
        {
            FuncData = funcData;
            ParamListAndReturnExpression = paramAndReturnExpression;
            BlockExpression = blockExpression;
            FuncIndex = funcIndex;
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            // generate param code
            string argumentString = "(";
            foreach (var argument in FuncData.Arguments)
                argumentString += LLVMConverter.ToInstructionName(argument.TypeName) + ",";

            if (FuncData.Arguments.Count() > 0) argumentString = argumentString[0..^1];
            argumentString += ")";


            result.Add(new Instruction(string.Format("define {0} @{1}{2} #{3}",
                                                                        LLVMConverter.ToInstructionName(FuncData.ReturnType),
                                                                        FuncData.Name,
                                                                        argumentString,
                                                                        FuncIndex)));

            // generate block code
            result.Add(new Instruction("{"));
            result.AddRange(ParamListAndReturnExpression.Build());
            result.AddRange(BlockExpression.Build());
            if (FuncData.ReturnType == ReturnType.Void) result.Add(new Instruction("ret void"));
            result.Add(new Instruction("}"));
            result.Add(Instruction.EmptyLine());

            if(FuncData.Name == "main")
            {
                LLVMAttribute attribute = new LLVMAttribute(FuncIndex)
                {
                    NounwindReadnone = true,
                    StackProtectorBufferSize = 8
                };

                result.Add(new Instruction(attribute.ToString()));
            }

            return result;
        }
    }
}
