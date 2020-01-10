using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WpfApp.Models
{
    public class ClassHierarchyData : IHierarchical<ClassHierarchyData>
    {
        public Type Data { get; set; }

        public ObservableCollection<ClassHierarchyData> Items { get; set; } = new ObservableCollection<ClassHierarchyData>();

        public bool Terminal { get => this.Items.Count == 0; }

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
