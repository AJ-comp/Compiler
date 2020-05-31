using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Parse.FrontEnd.Grammars
{
    public abstract class Sdts
    {
        public MeaningUnit Add { get; } = new MeaningUnit("Add");
        public MeaningUnit Sub { get; } = new MeaningUnit("Sub");
        public MeaningUnit Mul { get; } = new MeaningUnit("Mul");
        public MeaningUnit Div { get; } = new MeaningUnit("Div");
        public MeaningUnit Mod { get; } = new MeaningUnit("Mod");
        public MeaningUnit Assign { get; } = new MeaningUnit("Assign");
        public MeaningUnit AddAssign { get; } = new MeaningUnit("AddAssign");
        public MeaningUnit SubAssign { get; } = new MeaningUnit("SubAssign");
        public MeaningUnit MulAssign { get; } = new MeaningUnit("MulAssign");
        public MeaningUnit DivAssign { get; } = new MeaningUnit("DivAssign");
        public MeaningUnit ModAssign { get; } = new MeaningUnit("ModAssign");
        public MeaningUnit LogicalOr { get; } = new MeaningUnit("LogicalOr");
        public MeaningUnit LogicalAnd { get; } = new MeaningUnit("LogicalAnd");
        public MeaningUnit LogicalNot { get; } = new MeaningUnit("LogicalNot");
        public MeaningUnit Equal { get; } = new MeaningUnit("Equal");
        public MeaningUnit NotEqual { get; } = new MeaningUnit("NotEqual");
        public MeaningUnit GreaterThan { get; } = new MeaningUnit("GreaterThan");
        public MeaningUnit LessThan { get; } = new MeaningUnit("LessThan");
        public MeaningUnit GreaterEqual { get; } = new MeaningUnit("GreatherEqual");
        public MeaningUnit LessEqual { get; } = new MeaningUnit("LessEqual");
        public MeaningUnit UnaryMinus { get; } = new MeaningUnit("UnaryMinus");
        public MeaningUnit PreInc { get; } = new MeaningUnit("PreInc");
        public MeaningUnit PreDec { get; } = new MeaningUnit("PreDec");
        public MeaningUnit PostInc { get; } = new MeaningUnit("PostInc");
        public MeaningUnit PostDec { get; } = new MeaningUnit("PostDec");

        protected Sdts(KeyManager keyManager)
        {
            Type type = this.GetType();

            BindingFlags Flags = BindingFlags.Instance
                                           | BindingFlags.Public
                                           | BindingFlags.NonPublic;

            foreach (var member in type.GetProperties(Flags))
            {
                if (member.PropertyType.FullName == typeof(MeaningUnit).ToString())
                {
                    MeaningUnit param = member.GetValue(this) as MeaningUnit;

                    keyManager.AllocateUniqueKey(param);
                }
            }
        }

        public abstract event EventHandler<SemanticErrorArgs> SemanticErrorEventHandler;

        public abstract SemanticAnalysisResult Process(AstSymbol symbol);
    }
}
