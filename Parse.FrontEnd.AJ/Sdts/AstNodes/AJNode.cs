using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public abstract partial class AJNode : SdtsNode
    {
        public ProgramNode RootNode { get; private set; }
        public int BlockLevel { get; private set; } = 0;
        public int Offset { get; private set; } = 0;
        public bool StubCode { get; protected set; } = false;

        public string FileFullPath => RootNode.FullPath;

        public IEnumerable<PreDefTypeData> PreDefTypeList
        {
            get
            {
                List<PreDefTypeData> result = new List<PreDefTypeData>();

                result.Add(new PreDefTypeData("bool", "System.Boolean", 1));
                result.Add(new PreDefTypeData("byte", "System.Byte", 1));
                result.Add(new PreDefTypeData("sbyte", "System.SByte", 1));
                result.Add(new PreDefTypeData("short", "System.Int16", 2));
                result.Add(new PreDefTypeData("ushort", "System.UInt16", 2));
                result.Add(new PreDefTypeData("int", "System.Int32", 4));
                result.Add(new PreDefTypeData("uint", "System.UInt32", 4));
                result.Add(new PreDefTypeData("double", "System.Double", 8));

                return result;
            }
        }

        public int ParentBlockLevel
        {
            get
            {
                int result = -1;
                var travNode = this.Parent as AJNode;

                while (travNode != null)
                {
                    if (travNode.BlockLevel != -1)
                    {
                        result = travNode.BlockLevel;
                        break;
                    }

                    travNode = travNode.Parent as AJNode;
                }

                return result;
            }
        }

        public bool IsNotUsed
        {
            get => _isNotUsed;
            set
            {
                _isNotUsed = value;

                foreach (var token in AllTokens)
                    token.IsNotUsed = value;
            }
        }

        protected AJNode(AstSymbol node)
        {
            Ast = node;
        }


        public override SdtsNode Compile(CompileParameter param)
        {
            try
            {
                return CompileLogic(param);
            }
            catch (Exception ex)
            {
                RootNode.FiredExceptoins.Add(ex);
                AddUnExpectedError(ex.Message);

                return this;
            }
            finally
            {
                if (param.Build) DBContext.Instance.Insert(this);
            }
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            if (param != null)
            {
                RootNode = param.RootNode as ProgramNode;
                BlockLevel = param.BlockLevel;
                Offset = param.Offset;
            }

            /*
            Alarms.Clear();
            RootNode.UnLinkedSymbols.Remove(this);
            RootNode.LinkedSymbols.Remove(this);
            RootNode.AmbiguityLinkedSymbols.Remove(this);
            RootNode.CompletedSymbols.Remove(this);
            */

            return this;
        }

        public override string ToString() => this.GetType().Name;

        public override bool Equals(object obj)
        {
            return obj is AJNode node &&
                   Id == node.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        private bool _isNotUsed = false;

        public static bool operator ==(AJNode left, AJNode right)
        {
            return EqualityComparer<AJNode>.Default.Equals(left, right);
        }

        public static bool operator !=(AJNode left, AJNode right)
        {
            return !(left == right);
        }
    }
}
