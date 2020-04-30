using ApplicationLayer.Common;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
	public abstract class TreeNodeModel : INotifyPropertyChanged, IComparable
	{
		private bool isDraggable = true;
		private bool isEditable;
		private bool isEditing;
		private bool isExpanded;
		private bool isLoading;
		private bool isSelectable = true;
		private bool isSelected;
		private object tag;

		protected ObservableCollection<TreeNodeModel> _children = new ObservableCollection<TreeNodeModel>();

		protected TreeNodeModel()
		{
			this.tag = this;
		}

		[XmlIgnore] public ReadOnlyObservableCollection<TreeNodeModel> Children => new ReadOnlyObservableCollection<TreeNodeModel>(_children);

		[XmlIgnore] public TreeNodeModel Parent { get; internal set; }

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets or sets whether the node is draggable.
		/// </summary>
		/// <value>
		/// <c>true</c> if the node is draggable; otherwise, <c>false</c>.
		/// </value>
		[XmlIgnore] public bool IsDraggable
		{
			get => isDraggable;
			set
			{
				if (isDraggable == value) return;

				isDraggable = value;
				this.OnPropertyChanged("IsDraggable");
			}
		}

		/// <summary>
		/// Gets or sets whether the node is editable.
		/// </summary>
		/// <value>
		/// <c>true</c> if the node is editable; otherwise, <c>false</c>.
		/// </value>
		[XmlIgnore] public bool IsEditable
		{
			get => isEditable;
			set
			{
				if (isEditable == value) return;

				isEditable = value;
				this.OnPropertyChanged("IsEditable");
			}
		}

		/// <summary>
		/// Gets or sets whether the node is currently being edited.
		/// </summary>
		/// <value>
		/// <c>true</c> if the node is currently being edited; otherwise, <c>false</c>.
		/// </value>
		[XmlIgnore] public bool IsEditing
		{
			get => isEditing;
			set
			{
				if (isEditing == value) return;

				isEditing = value;
				this.OnPropertyChanged("IsEditing");
			}
		}

		/// <summary>
		/// Gets or sets whether the node is expanded.
		/// </summary>
		/// <value>
		/// <c>true</c> if the node is expanded; otherwise, <c>false</c>.
		/// </value>
		[XmlIgnore] public bool IsExpanded
		{
			get => isExpanded;
			set
			{
				if (isExpanded == value) return;

				isExpanded = value;
				this.OnPropertyChanged("IsExpanded");
			}
		}

		/// <summary>
		/// Gets or sets whether the node is currently loading children asynchronously.
		/// </summary>
		/// <value>
		/// <c>true</c> if the node is currently loading children asynchronously; otherwise, <c>false</c>.
		/// </value>
		[XmlIgnore] public bool IsLoading
		{
			get => isLoading;
			set
			{
				if (isLoading == value) return;

				isLoading = value;
				this.OnPropertyChanged("IsLoading");
			}
		}

		/// <summary>
		/// Gets or sets whether the node is capable of being selected.
		/// </summary>
		/// <value>
		/// <c>true</c> if the node is capable of being selected; otherwise, <c>false</c>.
		/// </value>
		[XmlIgnore] public bool IsSelectable
		{
			get => isSelectable;
			set
			{
				if (isSelectable == value) return;

				isSelectable = value;
				this.OnPropertyChanged("IsSelectable");
			}
		}

		/// <summary>
		/// Gets or sets whether the node is selected.
		/// </summary>
		/// <value>
		/// <c>true</c> if the node is selected; otherwise, <c>false</c>.
		/// </value>
		[XmlIgnore] public bool IsSelected
		{
			get => isSelected;
			set
			{
				if (isSelected == value) return;

				isSelected = value;
				this.OnPropertyChanged("IsSelected");
			}
		}

		/// <summary>
		/// Gets or sets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		[XmlIgnore] public abstract string DisplayName { get; set; }

		/// <summary>
		/// Gets or sets custom data for the node.
		/// </summary>
		/// <value>The custom data for the node.</value>
		[XmlIgnore] public object Tag
		{
			get => tag;
			set
			{
				if (tag == value) return;

				tag = value;
				this.OnPropertyChanged(nameof(Tag));
			}
		}

		[XmlIgnore] public abstract string FullOnlyPath { get; }

		public event PropertyChangedEventHandler PropertyChanged;
		public abstract event EventHandler<FileChangedEventArgs> Changed;



		/********************************************************************************************
         * public method section
         ********************************************************************************************/
		public void AddChildren(TreeNodeModel item)
		{
			item.Parent = this;
			this._children.Add(item);
			this.NotifyChildrenChanged();
		}

		public void Clear()
		{
			_children.Clear();
			this.NotifyChildrenChanged();
		}

		public void NotifyChildrenChanged() => this.OnPropertyChanged(nameof(Children));



		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="String"/> representation of this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format("{0}[Name={1}]", this.GetType().Name, this.DisplayName);
		}

		public int CompareTo(object obj)
		{
			var treeNodeModel = obj as TreeNodeModel;
			return DisplayName.CompareTo(treeNodeModel.DisplayName);
		}

		public static void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
		}

		public abstract void RemoveChild(TreeNodeModel nodeToRemove);
	}
}
