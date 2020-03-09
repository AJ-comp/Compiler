using Parse.Extensions;
using Parse.Tokenize;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class BlockParsingStack
    {
        private List<BlockStackItem> stacks = new List<BlockStackItem>();

        public IReadOnlyList<BlockStackItem> Stacks => this.stacks;
        public TokenCell Token { get; } = null;

        public BlockParsingStack(BlockStackItem blockItem, TokenCell token)
        {
            this.stacks.Add(blockItem);
            Token = token;
        }

        /// <summary>
        /// This function add a new parsing item on current parsing block.
        /// </summary>
        public void AddParsingItem()
        {
            this.stacks.Add(new BlockStackItem(this.Stacks.Last().AfterStack));
        }
    }

    public class BlockStackItem
    {
        public Stack<object> BeforeStack { get; } = new Stack<object>();
        public Stack<object> AfterStack { get; internal set; } = new Stack<object>();

        /// <summary>
        /// This constructor creates with initial stack state.
        /// </summary>
        public BlockStackItem()
        {
            BeforeStack.Push(0);
        }

        public BlockStackItem(Stack<object> beforeStack)
        {
            BeforeStack = beforeStack;
        }

        public BlockStackItem(Stack<object> beforeStack, Stack<object> afterStack)
        {
            BeforeStack = beforeStack;
            AfterStack = afterStack;
        }

        /// <summary>
        /// This function copy before stack data to the after stack.
        /// </summary>
        public void CopyBeforeStackToAfterStack()
        {
            this.AfterStack = this.BeforeStack.Clone();
        }

        public override string ToString() => string.Format("{0},{1}", BeforeStack.ToString(), AfterStack.ToString());
    }
}
