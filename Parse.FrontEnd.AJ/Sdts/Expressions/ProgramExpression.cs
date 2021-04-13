using Parse.FrontEnd.AJ.Sdts.Datas;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.Expressions
{
    public class ProgramExpression : AJExpression
    {
        public IEnumerable<ClassDefData> Classes { get; }

        public List<UsingExpression> Usings { get; } = new List<UsingExpression>();
        public List<NamespaceExpression> Namespaces { get; } = new List<NamespaceExpression>();

        public ProgramExpression(IEnumerable<ClassDefData> classes)
        {
            Classes = classes;
        }

        public ProgramExpression()
        {
        }


        public override string ToString()
        {
            string result = string.Empty;

            foreach (var use in Usings) result += use.ToString();
            result += Environment.NewLine;
            foreach (var names in Namespaces) result += names.ToString();

            return result;
        }
    }
}
