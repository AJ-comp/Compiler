﻿using Parse.FrontEnd.Ast;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Parse.FrontEnd
{
    public enum ErrorType { Error, Warning, Information };
    public enum MatchedAction { None, OffsetPlus, BlockPlus };

    public class MeaningUnit
    {
        public string Name { get; } = string.Empty;
        public MatchedAction Action { get; } = MatchedAction.None;

        public MeaningUnit(string name, MatchedAction action = MatchedAction.None)
        {
            Name = name;
            Action = action;
        }

        public override string ToString() => Name;

        public override bool Equals(object obj)
        {
            return obj is MeaningUnit unit &&
                   Name == unit.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public static bool operator ==(MeaningUnit left, MeaningUnit right)
        {
            return EqualityComparer<MeaningUnit>.Default.Equals(left, right);
        }

        public static bool operator !=(MeaningUnit left, MeaningUnit right)
        {
            return !(left == right);
        }
    }


    public class MeaningErrInfo : ParsingErrorInfo
    {
        public IReadOnlyList<TokenData> ErrTokens => _errTokens;
        public bool IsAllVirtualToken
        {
            get
            {
                if (ErrTokens.Count == 0) return false;

                foreach (var token in ErrTokens)
                {
                    if (!token.IsVirtual) return false;
                }

                return true;
            }
        }


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




    public class MeaningErrInfoList : IList<MeaningErrInfo>
    {
        private List<MeaningErrInfo> _list = new List<MeaningErrInfo>();
        public MeaningErrInfo this[int index] { get => _list[index]; set => throw new NotImplementedException(); }

        public int Count => _list.Count;
        public bool IsReadOnly => false;


        public void Add(MeaningErrInfo item)
        {
            if (item == null) return;
            if (item.IsAllVirtualToken) return;

            _list.Add(item);
        }

        public void Clear() => _list.Clear();
        public bool Contains(MeaningErrInfo item) => _list.Contains(item);
        public void CopyTo(MeaningErrInfo[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        public IEnumerator<MeaningErrInfo> GetEnumerator() => ((IList<MeaningErrInfo>)_list).GetEnumerator();
        public int IndexOf(MeaningErrInfo item) => _list.IndexOf(item);
        public void Insert(int index, MeaningErrInfo item) => _list.Insert(index, item);
        public bool Remove(MeaningErrInfo item) => _list.Remove(item);
        public void RemoveAt(int index) => _list.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => ((IList<MeaningErrInfo>)_list).GetEnumerator();
    }




    public class SemanticAnalysisResult
    {
        public SdtsNode SdtsRoot { get; }
        public IReadOnlyList<AstSymbol> AllNodes { get; }
        public Exception FiredException { get; }

        public SemanticAnalysisResult(SdtsNode sdtsRoot, IReadOnlyList<AstSymbol> allNodes, Exception exception = null)
        {
            SdtsRoot = sdtsRoot;
            AllNodes = allNodes;
            FiredException = exception;
        }
    }
}
