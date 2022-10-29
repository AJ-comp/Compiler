using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM
{
    public abstract class CodeType
    {
        private string _originalValue;
        private readonly int _hashCode;
        private static ConcurrentDictionary<int, CodeType> _totalHashValue = new ConcurrentDictionary<int, CodeType>();

        protected static int GetHashCode(string value) => 2018552787 + EqualityComparer<string>.Default.GetHashCode(value);
        protected static CodeType GetCodeType(int hashValue) => (_totalHashValue.ContainsKey(hashValue)) ? _totalHashValue[hashValue] : null;

        protected CodeType(int hashCode, string value)
        {
            _hashCode = hashCode;
            _originalValue = value;

            var bExist = _totalHashValue.ContainsKey(hashCode);
            if (bExist == false) _totalHashValue.TryAdd(hashCode, this);
            else
            {
                // check a key duplication.
                var cacheType = _totalHashValue[hashCode];
                if (cacheType._originalValue != value)
                {
                    // if key was duplicated (same key, different TokenType) then assign a new value to the hashCode and add.
                    do
                    {
                        hashCode++;
                    } while (_totalHashValue.ContainsKey(hashCode));

                    _totalHashValue.TryAdd(hashCode, this);
                }
            }
        }

        /*
        public bool IsDirectedLine(TokenType tokenType)
        {
            if(this is tokenType)
        }
        */

        public static Command Command
        {
            get
            {
                var data = "Command";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Command(hashCode, data) : cacheType as Command;
            }
        }

        public static Comment Comment
        {
            get
            {
                var data = "Comment";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Comment(hashCode, data) : cacheType as Comment;
            }
        }

        public static Decorator Decorator
        {
            get
            {
                var data = "Decorator";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Decorator(hashCode, data) : cacheType as Decorator;
            }
        }

        public static bool operator ==(CodeType left, CodeType right)
        {
            return EqualityComparer<CodeType>.Default.Equals(left, right);
        }

        public static bool operator !=(CodeType left, CodeType right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is CodeType type &&
                   _hashCode == type._hashCode;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString() => _originalValue;
    }




    public class Command : CodeType
    {
        internal Command(int hashCode, string value) : base(hashCode, value) { }


        public Alloca Alloca
        {
            get
            {
                var data = "Alloca";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Alloca(hashCode, data) : cacheType as Alloca;
            }
        }

        public Load Load
        {
            get
            {
                var data = "Load";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Load(hashCode, data) : cacheType as Load;
            }
        }

        public Store Store
        {
            get
            {
                var data = "Store";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Store(hashCode, data) : cacheType as Store;
            }
        }

        public Cmp Cmp
        {
            get
            {
                var data = "Cmp";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Cmp(hashCode, data) : cacheType as Cmp;
            }
        }

        public Branch Branch
        {
            get
            {
                var data = "Branch";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Branch(hashCode, data) : cacheType as Branch;
            }
        }

        public Label Label
        {
            get
            {
                var data = "Label";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Label(hashCode, data) : cacheType as Label;
            }
        }

        public Block Block
        {
            get
            {
                var data = "Block";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Block(hashCode, data) : cacheType as Block;
            }
        }

        public Operator Operator
        {
            get
            {
                var data = "Operator";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Operator(hashCode, data) : cacheType as Operator;
            }
        }


        public Return Return
        {
            get
            {
                var data = "Return";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Return(hashCode, data) : cacheType as Return;
            }
        }

        public Etc Etc
        {
            get
            {
                var data = "Etc";
                var hashCode = GetHashCode(data);
                var cacheType = GetCodeType(hashCode);

                return (cacheType == null) ? new Etc(hashCode, data) : cacheType as Etc;
            }
        }
    }



    public class Alloca : Command
    {
        internal Alloca(int hashCode, string value) : base(hashCode, value) { }
    }
    public class Load : Command
    {
        internal Load(int hashCode, string value) : base(hashCode, value) { }
    }
    public class Store : Command
    {
        internal Store(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Cmp : Command
    {
        internal Cmp(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Branch : Command
    {
        internal Branch(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Label : Command
    {
        internal Label(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Block : Command
    {
        internal Block(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Operator : Command
    {
        internal Operator(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Return : Command
    {
        internal Return(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Etc : Command
    {
        internal Etc(int hashCode, string value) : base(hashCode, value) { }
    }


    public class Comment : CodeType
    {
        internal Comment(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Decorator : CodeType
    {
        internal Decorator(int hashCode, string value) : base(hashCode, value) { }
    }

}
