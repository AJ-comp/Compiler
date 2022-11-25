using System;
using System.Collections.Generic;
using System.Text;

namespace AJ.Common
{
    public interface ITree<T> where T: ITree<T>
    {
        T Parent { get; set; }
    }

    public static class TreeHelper
    {
        public static T GetParent<T>(T start, Type toFindParent) where T : ITree<T>
        {
            var travNode = start;

            while (travNode != null)
            {
                if (travNode.GetType() == toFindParent) break;

                travNode = travNode.Parent;
            }

            return travNode;
        }


        public static T GetParentAs<T>(T start, params Type[] toFindParents) where T : ITree<T>
        {
            var travNode = start;

            bool bFind = false;
            while (travNode != null)
            {
                foreach(var item in toFindParents)
                {
                    if (travNode.GetType() == item)
                    {
                        bFind = true;
                        break;
                    }

                    if (travNode.GetType().IsSubclassOf(item))
                    {
                        bFind = true;
                        break;
                    }
                }

                if (bFind) break;
                travNode = travNode.Parent;
            }

            return travNode;
        }
    }
}
