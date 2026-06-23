using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ApplicationLayer.Common.Utilities
{
    public class ClassHierarchyData : IHierarchical<ClassHierarchyData>
    {
        public Type Data { get; set; }

        public ObservableCollection<ClassHierarchyData> Items { get; set; } = new ObservableCollection<ClassHierarchyData>();

        public bool Terminal { get => this.Items.Count == 0; }

        public int MaxDepthLevel
        {
            get
            {
                List<int> sum = new List<int>();

                foreach (var item in this.Items) sum.Add(item.MaxDepthLevel);

                sum.Sort();
                int result = (sum.Count > 0) ? sum.Last() : 0;

                // child depth level + own
                return result + 1;
            }
        }

        public Collection<DetailType> GetTerminalCollection(string accumClassification = "")
        {
            accumClassification += this.Data.Name + ".";
            Collection<DetailType> result = new Collection<DetailType>();

            foreach(var child in Items)
            {
                if (child.Terminal) result.Add(new DetailType(child.Data, accumClassification));
                else
                {
                    var terminalChildren = child.GetTerminalCollection(accumClassification);
                    foreach (var terminalChild in terminalChildren)
                        result.Add(terminalChild);
                }
            }

            return result;
        }

        public ClassHierarchyData GetFirstClassHierarchyDataByDepth(int depthIndexToGet)
        {
            ClassHierarchyData result = this;

            for (int i = 0; i < depthIndexToGet; i++)
            {
                if (result == null) break;

                result = (result.Terminal) ? null : result.Items[0];
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is ClassHierarchyData data &&
                   EqualityComparer<Type>.Default.Equals(Data, data.Data);
        }

        public override int GetHashCode()
        {
            return -301143667 + EqualityComparer<Type>.Default.GetHashCode(Data);
        }
    }


    public class DetailType
    {
        public Type Type { get; }
        public string Classification { get; }
        public List<uint> MatchedIndexes { get; set; } = new List<uint>();

        public DetailType(Type type, string classification)
        {
            Type = type;
            Classification = classification;
        }
    }
}
