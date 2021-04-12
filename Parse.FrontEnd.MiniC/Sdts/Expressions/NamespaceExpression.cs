using Parse.FrontEnd.MiniC.Sdts.Datas;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.Sdts.Expressions
{
    /// ****************************************/
    /// <summary>
    /// namespace 문을 의미하는 표현식입니다.
    /// ex : 
    /// namespace Name
    /// {
    ///     IEnumerable<ClassExpression>
    /// }
    /// </summary>
    /// ****************************************/
    public class NamespaceExpression : AJExpression
    {
        public IEnumerable<ClassExpression> Classes => _classes;

        public NamespaceExpression(INamespaceExpression namespaceExpression)
        {
            _datas = namespaceExpression;

            foreach (var symbol in namespaceExpression.ClassDatas)
            {
                _classes.Add(new ClassExpression(symbol));
            }
        }

        public override string ToString()
        {
            var result = string.Format("namespace {0}", _datas.Name);
            result += "{" + Environment.NewLine;

            foreach (var classExpression in _classes)
                result += "\t" + classExpression.ToString() + Environment.NewLine;

            return result;
        }


        private INamespaceExpression _datas;
        private List<ClassExpression> _classes = new List<ClassExpression>();
    }
}
