using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd
{
    public enum ErrorType { Error, Warning, Information };

    public class MeaningUnit
    {
        internal uint uniqueKey;

        public string Name { get; } = string.Empty;
        public Func<AstNonTerminal, object> ActionLogic { get; set; }
        public Func<AstNonTerminal, SymbolTable, int, int, AstBuildOption, AstBuildResult> BuildLogic { get; set; }

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

        public override string ToString() => Name;
    }


    public class MeaningErrInfo
    {
        private AstSymbol _errorSymbol;

        public ErrorType ErrorType { get; private set; }
        public IReadOnlyList<TokenData> ErrorTokens
        {
            get
            {
                List<TokenData> result = new List<TokenData>();

                if (_errorSymbol is AstTerminal)
                    result.Add((_errorSymbol as AstTerminal).Token);
                else
                {
                    var errNT = (_errorSymbol as AstNonTerminal).ConnectedParseTree;
                    result.AddRange(errNT.AllTokens);
                }

                return result;
            }
        }
        public string ErrorMessage { get; }

        public MeaningErrInfo(AstSymbol errorSymbol, string errorMessage, ErrorType errorType = ErrorType.Error)
        {
            _errorSymbol = errorSymbol;
            ErrorMessage = errorMessage;
            ErrorType = errorType;
        }
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
