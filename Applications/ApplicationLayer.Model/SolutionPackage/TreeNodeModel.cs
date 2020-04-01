using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class TreeNodeModel : INotifyPropertyChanged
    {
		private ObservableCollection<TreeNodeModel> children = new ObservableCollection<TreeNodeModel>();
//		private DelegateCommand<object> defaultActionCommand;
		private bool isDraggable = true;
		private bool isEditable;
		private bool isEditing;
		private bool isExpanded;
		private bool isLoading;
		private bool isSelectable = true;
		private bool isSelected;
		private string name;
		private object tag;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the collection of child nodes.
		/// </summary>
		/// <value>The collection of child nodes.</value>
		public ObservableCollection<TreeNodeModel> Children
		{
			get
			{
				if (children == null)
					children = new ObservableCollection<TreeNodeModel>();

				return children;
			}
		}

		/*
		/// <summary>
		/// Gets or sets the default action command.
		/// </summary>
		/// <value>The default action command.</value>
		public DelegateCommand<object> DefaultActionCommand
		{
			get
			{
				return defaultActionCommand;
			}
			set
			{
				defaultActionCommand = value;
				this.OnPropertyChanged("DefaultActionCommand");
			}
		}
		*/

		/// <summary>
		/// Gets or sets whether the node is draggable.
		/// </summary>
		/// <value>
		/// <c>true</c> if the node is draggable; otherwise, <c>false</c>.
		/// </value>
		public bool IsDraggable
		{
			get
			{
				return isDraggable;
			}
			set
			{
				if (isDraggable == value)
					return;

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
		public bool IsEditable
		{
			get
			{
				return isEditable;
			}
			set
			{
				if (isEditable == value)
					return;

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
		public bool IsEditing
		{
			get
			{
				return isEditing;
			}
			set
			{
				if (isEditing == value)
					return;

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
		public bool IsExpanded
		{
			get
			{
				return isExpanded;
			}
			set
			{
				if (isExpanded == value)
					return;

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
		public bool IsLoading
		{
			get
			{
				return isLoading;
			}
			set
			{
				if (isLoading == value)
					return;

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
		public bool IsSelectable
		{
			get
			{
				return isSelectable;
			}
			set
			{
				if (isSelectable == value)
					return;

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
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				if (isSelected == value)
					return;

				isSelected = value;
				this.OnPropertyChanged("IsSelected");
			}
		}

		/// <summary>
		/// Gets or sets the name of the node.
		/// </summary>
		/// <value>The name of the node.</value>
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (name == value)
					return;

				if (!string.IsNullOrEmpty(value))
					name = value;
				this.OnPropertyChanged("Name");
			}
		}

		/// <summary>
		/// Gets or sets custom data for the node.
		/// </summary>
		/// <value>The custom data for the node.</value>
		public object Tag
		{
			get
			{
				return tag;
			}
			set
			{
				if (tag == value)
					return;

				tag = value;
				this.OnPropertyChanged(nameof(Tag));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

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
			return string.Format("{0}[Name={1}]", this.GetType().Name, this.Name);
		}

	}
}
