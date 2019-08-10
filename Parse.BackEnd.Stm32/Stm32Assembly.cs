using Parse.Ast;
using Parse.RegularGrammar;

namespace Parse.BackEnd.Stm32
{
    public class Stm32Assembly : TargetAssembly
    {
        /*
        Logic logic = new Logic();

        public Stm32Assembly()
        {
            this.logic.AddAction =
                ((left, right) => 
                {
                    if(left is AstNonTerminal)

                    string.Format("ldr r0, {0}", left);
                    string.Format("ldr r1, {0}", right);
                    string.Format("add r0, r0, r1");
                });

            this.logic.SubAction =
                ((left, right) =>
                {
                    string.Format("ldr r0, {0}", left);
                    string.Format("ldr r1, {0}", right);
                    string.Format("sub r0, r0, r1");
                });
        }
        */

        public override void GenerateCode(AstNonTerminal asTree)
        {
            
        }
    }
}
