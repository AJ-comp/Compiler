﻿using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd
{
    public enum ErrorType { Error, Warning, Information };

    public class MeaningUnit
    {
        internal uint uniqueKey;

        public string Name { get; } = string.Empty;
        public Func<AstNonTerminal, AstBuildParams, List<AstSymbol>, AstBuildResult> BuildLogic { get; set; }

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


    public class MeaningErrInfo : ParsingErrorInfo
    {
        public IReadOnlyList<TokenData> ErrTokens => _errTokens;

        public MeaningErrInfo(string code, string errorMessage, ErrorType errorType = ErrorType.Error)
            : base(errorType, code, errorMessage)
        {
        }

        public MeaningErrInfo(TokenData token, string code, string errorMessage, ErrorType errorType = ErrorType.Error)
            : this(code, errorMessage, errorType)
        {
            _errTokens.Add(token);
        }

        public MeaningErrInfo(IReadOnlyList<TokenData> tokens, string code, string errorMessage, ErrorType errorType = ErrorType.Error)
            : this(code, errorMessage, errorType)
        {
            _errTokens.AddRange(tokens);
        }

        private List<TokenData> _errTokens = new List<TokenData>();
    }

    public class MeaningErrInfoList : List<MeaningErrInfo>
    {
    }


    public class SemanticAnalysisResult
    {
        public AstSymbol RootAst { get; }
        public IReadOnlyList<AstSymbol> AllNodes { get; }
        public Exception FiredException { get; }

        public SemanticAnalysisResult(AstSymbol rootAst, IReadOnlyList<AstSymbol> allNodes, Exception exception = null)
        {
            RootAst = rootAst;
            AllNodes = allNodes;
            FiredException = exception;
        }
    }
}
