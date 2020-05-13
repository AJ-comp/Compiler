using ApplicationLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ApplicationLayer.Views
{
	public class CustomStyleSelector : StyleSelector
	{

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets or sets the <see cref="Style"/> to use for groups.
		/// </summary>
		/// <value>The <see cref="Style"/> to use for groups.</value>
		public Style GroupStyle { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="Style"/> to use for mail.
		/// </summary>
		/// <value>The <see cref="Style"/> to use for mail.</value>
		public Style UCodeStyle { get; set; }

		/// <summary>
		/// Returns a <see cref="Style"/> based on custom logic.
		/// </summary>
		/// <param name="item">The data object.</param>
		/// <param name="container">The data-bound element.</param>
		/// <returns>The <see cref="Style"/> to use.</returns>
#if WINRT
		protected override Style SelectStyleCore(object item, DependencyObject container) {
#else
		public override Style SelectStyle(object item, DependencyObject container)
		{
#endif
			if (item is UCodeTreeNodeModel)
				return this.UCodeStyle;
			else
				return this.GroupStyle;
		}

	}
}
