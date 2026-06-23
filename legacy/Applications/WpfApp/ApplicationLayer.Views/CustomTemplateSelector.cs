using ApplicationLayer.Models;
using System.Windows;
using System.Windows.Controls;

namespace ApplicationLayer.Views
{
	public class CustomTemplateSelector : DataTemplateSelector
	{

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets or sets the <see cref="DataTemplate"/> to use for groups.
		/// </summary>
		/// <value>The <see cref="DataTemplate"/> to use for groups.</value>
		public DataTemplate GroupTemplate { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="DataTemplate"/> to use for mail.
		/// </summary>
		/// <value>The <see cref="DataTemplate"/> to use for mail.</value>
		public DataTemplate UCodeTemplate { get; set; }

		public DataTemplate ExceptionTemplate { get; set; }

		/// <summary>
		/// Returns a <see cref="DataTemplate"/> based on custom logic.
		/// </summary>
		/// <param name="item">The data object.</param>
		/// <param name="container">The data-bound element.</param>
		/// <returns>The <see cref="DataTemplate"/> to use.</returns>
#if WINRT
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) {
#else
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
#endif
			if (item is UCodeTreeNodeModel)
				return this.UCodeTemplate;
			else if (item is ExceptionTreeNodeModel)
				return this.ExceptionTemplate;
			else
				return this.GroupTemplate;
		}

	}
}
