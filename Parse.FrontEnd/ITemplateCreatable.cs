namespace Parse.FrontEnd
{
    public interface ITemplateCreatable
    {
        object Template();
    }

    public interface ITemplateCreatable<T> where T : class
    {
        T Template();
    }
}
