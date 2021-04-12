using Parse.Extensions;
using Parse.MiddleEnd.IR.LLVM;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Datas
{
    public sealed class IRStructDefInfo
    {
        public string Name { get; }
        public IEnumerable<IRDeclareVar> MemberVarList => _memberVarList;
        public IEnumerable<string> MemberTypeList
        {
            get
            {
                List<string> result = new List<string>();

                foreach (var memberVar in _memberVarList)
                    result.Add(LLVMConverter.ToInstructionName(memberVar));

                return result;
            }
        }

        public IEnumerable<IRFuncDefInfo> FuncDefList => _funcDefList;


        public int MaxTypeSize
        {
            get
            {
                int maxSize = 1;
                foreach (var memberVar in _memberVarList)
                {
                    int size = LLVMConverter.ToAlignSize(memberVar);
                    if (maxSize < size) maxSize = size;
                }

                return maxSize;
            }
        }


        public IRStructDefInfo(string name, IEnumerable<IRDeclareVar> memberVarList, IEnumerable<IRFuncDefInfo> funcDefList)
        {
            Name = name;
            _memberVarList.AddRangeExceptNull(memberVarList);
            _funcDefList.AddRangeExceptNull(funcDefList);
        }


        private List<IRDeclareVar> _memberVarList = new List<IRDeclareVar>();
        private List<IRFuncDefInfo> _funcDefList = new List<IRFuncDefInfo>();
    }
}
