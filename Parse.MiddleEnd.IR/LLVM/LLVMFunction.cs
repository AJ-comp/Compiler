using AJ.Common.Helpers;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMFunction
    {
        public string Name => _irFunction.Name;
        public IRType ReturnType => _irFunction.ReturnType;
        public List<IRVariable> Arguments => _irFunction.Arguments;
        public IRCompoundStatement Statement => _irFunction.Statement;
        public string IRName => Name.Replace(".", "_").Replace("~", "_");

        public Code Code { get; } = new Code();

        public void AddVar(LLVMVar var)
        {
            if (var is LLVMNamedVar)
            {
                var namedVar = var as LLVMNamedVar;

                // get the count of same name in current function
                var count = GetCountOfSameName(namedVar);
                namedVar.NameInFunction = $"{var.VarType.ToDescription()}{namedVar.Variable.Name}{count}";
            }
            else if (var is LLVMLiteralVar)
            {
                var literalVar = var as LLVMLiteralVar;
                var.NameInFunction = literalVar.Value.ToString();
            }
            else
            {
                var.NameInFunction = $"{var.VarType.ToDescription()}{_localVars[var.VarType].Count}";
            }

            _orderingVars.Add(var);
            _localVars[var.VarType].Add(var);
        }

        public void AddVars(params LLVMVar[] vars)
        {
            foreach (var llvmVar in vars)
            {
                if (llvmVar == null) continue;

                AddVar(llvmVar);
            }
        }

        public LLVMNamedVar GetNamedVar(IRVariable var)
        {
            LLVMNamedVar result = null;

            foreach (var llvmVar in _orderingVars)
            {
                if (!(llvmVar is LLVMNamedVar)) continue;
                var namedVar = llvmVar as LLVMNamedVar;

                if (namedVar.Variable == var)
                {
                    result = namedVar;
                    break;
                }
            }

            return result;
        }

        public LLVMVar GetRecentVar() => _orderingVars.Last();
        public LLVMVar GetRecentVar(LLVMVarType varType) => _localVars[varType].Last();
        public IEnumerable<LLVMVar> GetRecentVars(int count) => _orderingVars.TakeLast(count);


        private Dictionary<LLVMVarType, List<LLVMVar>> _localVars = new Dictionary<LLVMVarType, List<LLVMVar>>();
        private List<LLVMVar> _orderingVars = new List<LLVMVar>();


        public LLVMFunction(IRFunction irFunction)
        {
            _irFunction = irFunction;
            var list = Enum.GetValues(typeof(LLVMVarType)).Cast<LLVMVarType>().ToList();

            foreach (var varType in list)
                _localVars.Add(varType, new List<LLVMVar>());
        }

        private IRFunction _irFunction;

        private int GetCountOfSameName(LLVMNamedVar selVar)
        {
            int result = 0;
            foreach (var item in _orderingVars)
            {
                if (!(item is LLVMNamedVar)) continue;

                var targetVar = item as LLVMNamedVar;
                if (targetVar.Variable.Name == selVar.Variable.Name) result++;
            }

            return result;
        }
    }
}
