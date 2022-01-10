using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd
{
    public interface IData
    {
        int Id { get; set; }
    }


    public interface IHasParent : IData
    {
        int ParentId { get; set; }
        string ParentType { get; set; }
        int ChildIndex { get; set; }
    }
}
