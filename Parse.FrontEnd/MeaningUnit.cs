using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd
{
    public class MeaningUnit
    {
        internal uint uniqueKey;

        public string Name { get; } = string.Empty;
        public Func<TreeNonTerminal, SymbolTable, int, MeaningErrInfoList, object> ActionLogic { get; set; }

        public MeaningUnit(string name)
        {
            this.Name = name;
        }

        public bool Equals(MeaningUnit other)
        {
            if (object.ReferenceEquals(other, null)) return false;

            return (this.GetHashCode() == other.GetHashCode());
        }

        public override int GetHashCode() => (int)this.uniqueKey;

        public override bool Equals(object obj)
        {
            bool result = false;

            if (obj is MeaningUnit)
            {
                MeaningUnit right = obj as MeaningUnit;

                result = (this.GetHashCode() == right.GetHashCode());
            }

            return result;
        }

        public static bool operator ==(MeaningUnit left, MeaningUnit right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(MeaningUnit left, MeaningUnit right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return !object.ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }
    }


    public class MeaningErrInfo
    {
        public int ErrorIndex { get; }
        public string ErrorMessage { get; }
    }

    public class MeaningErrInfoList : List<MeaningErrInfo>
    {
    }


    public class SementicAnalysisResult
    {
        public MeaningErrInfoList MeaningErrInfos { get; } = new MeaningErrInfoList();
        public SymbolTable SymbolTable { get; }

        public SementicAnalysisResult(MeaningErrInfoList meaningErrInfos, SymbolTable symbolTable)
        {
            MeaningErrInfos = meaningErrInfos;
            SymbolTable = symbolTable;
        }
    }
}
