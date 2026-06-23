using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ApplicationLayer.Views
{

    //public class MenusForTargetCollection : IList<MenusForTarget>
    //{
    //    private List<MenusForTarget> datas = new List<MenusForTarget>();
    //    public IReadOnlyList<MenusForTarget> Items => datas;

    //    public MenusForTarget this[int index] { get => ((IList<MenusForTarget>)datas)[index]; set => ((IList<MenusForTarget>)datas)[index] = value; }

    //    public int Count => this.datas.Count;

    //    public bool IsReadOnly => false;

    //    public void Add(MenusForTarget item) => this.datas.Add(item);

    //    public void Clear() => this.datas.Clear();

    //    public bool Contains(MenusForTarget item) => this.datas.Contains(item);

    //    public void CopyTo(MenusForTarget[] array, int arrayIndex) => this.datas.CopyTo(array, arrayIndex);

    //    public IEnumerator<MenusForTarget> GetEnumerator() => ((IList<MenusForTarget>)datas).GetEnumerator();

    //    public int IndexOf(MenusForTarget item) => this.datas.IndexOf(item);

    //    public void Insert(int index, MenusForTarget item) => this.datas.Insert(index, item);

    //    public bool Remove(MenusForTarget item) => this.datas.Remove(item);

    //    public void RemoveAt(int index) => this.datas.RemoveAt(index);

    //    IEnumerator IEnumerable.GetEnumerator() => ((IList<MenusForTarget>)datas).GetEnumerator();
    //}



    //public class MenusForTargetCollection : List<MenusForTarget>
    //{
    //    public MenusForTargetCollection()
    //    {
    //    }
    //}


    public class MenusForTargetCollection : FrameworkElement { }
}
