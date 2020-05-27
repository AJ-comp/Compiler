namespace Parse.FrontEnd.Ast
{
    public enum AstBuildOption { None=0, NotAssign=1, Reference=2, }

    public class AstBuildOptionChecker
    {
        public static bool HasOption(AstBuildOption srcOption, AstBuildOption optionToCheck)
            => ((srcOption & optionToCheck) == optionToCheck) ? true : false;
    }
}
