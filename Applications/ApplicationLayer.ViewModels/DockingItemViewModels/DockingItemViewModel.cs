using GalaSoft.MvvmLight;

namespace ApplicationLayer.ViewModels.DockingItemViewModels
{
	/// <summary>
	/// Represents a base class for all docking item view-models.
	/// </summary>
	public abstract class DockingItemViewModel : ViewModelBase
	{

		private string description;
		//		private ImageSource imageSource;
		private bool isActive;
		private bool isFloating;
		private bool isOpen;
		private bool isSelected;
		private string serializationId;
		private string title;
		private string windowGroupName;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets or sets the description associated with the view-model.
		/// </summary>
		/// <value>The description associated with the view-model.</value>
		public string Description
		{
			get => this.description;
			set
			{
				if (this.description == value) return;

				this.description = value;
				this.RaisePropertyChanged(nameof(Description));
			}
		}

		/// <summary>
		/// Gets or sets whether the view is currently active.
		/// </summary>
		/// <value>
		/// <c>true</c> if the view is currently active; otherwise, <c>false</c>.
		/// </value>
		public bool IsActive
		{
			get => this.isActive;
			set
			{
				if (this.isActive == value) return;

				this.isActive = value;
				this.RaisePropertyChanged(nameof(IsActive));
			}
		}

		/// <summary>
		/// Gets or sets whether the view is floating.
		/// </summary>
		/// <value>
		/// <c>true</c> if the view is floating; otherwise, <c>false</c>.
		/// </value>
		public bool IsFloating
		{
			get => this.isFloating;
			set
			{
				if (this.IsFloating == value) return;

				this.isFloating = value;
				this.RaisePropertyChanged(nameof(IsFloating));
			}
		}

		/// <summary>
		/// Gets or sets whether the view is currently open.
		/// </summary>
		/// <value>
		/// <c>true</c> if the view is currently open; otherwise, <c>false</c>.
		/// </value>
		public bool IsOpen
		{
			get => this.isOpen;
			set
			{
				if (this.isOpen == value) return;

				this.isOpen = value;
				this.RaisePropertyChanged(nameof(IsOpen));
			}
		}

		/// <summary>
		/// Gets or sets whether the view is currently selected in its parent container.
		/// </summary>
		/// <value>
		/// <c>true</c> if the view is currently selected in its parent container; otherwise, <c>false</c>.
		/// </value>
		public bool IsSelected
		{
			get => this.isSelected;
			set
			{
				if (this.isSelected == value) return;

				this.isSelected = value;
				this.RaisePropertyChanged(nameof(IsSelected));
			}
		}

		/// <summary>
		/// Gets whether the container generated for this view model should be a tool window.
		/// </summary>
		/// <value>
		/// <c>true</c> if the container generated for this view model should be a tool window; otherwise, <c>false</c>.
		/// </value>
		public abstract bool IsTool { get; }

		/// <summary>
		/// Gets or sets the name that uniquely identifies the view-model for layout serialization.
		/// </summary>
		/// <value>The name that uniquely identifies the view-model for layout serialization.</value>
		public string SerializationId
		{
			get => this.serializationId;
			set
			{
				if (this.serializationId == value) return;

				this.serializationId = value;
				this.RaisePropertyChanged(nameof(SerializationId));
			}
		}

		/// <summary>
		/// Gets or sets the title associated with the view-model.
		/// </summary>
		/// <value>The title associated with the view-model.</value>
		public string Title
		{
			get => this.title;
			set
			{
				if (this.title == value) return;

				this.title = value;
				this.RaisePropertyChanged(nameof(Title));
			}
		}

		/// <summary>
		/// Gets or sets the window group name associated with the view-model.
		/// </summary>
		/// <value>The window group name associated with the view-model.</value>
		public string WindowGroupName
		{
			get => this.windowGroupName;
			set
			{
				if (this.windowGroupName == value) return;

				this.windowGroupName = value;
				this.RaisePropertyChanged(nameof(WindowGroupName));
			}
		}
	}
}
