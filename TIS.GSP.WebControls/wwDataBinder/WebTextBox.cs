using System;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace GalleryServerPro.WebControls
{

	public class WebTextBox : TextBox, IwwDataBinder
	{
		[Browsable(true)]
		[NotifyParentProperty(true),
		 DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		 PersistenceMode(PersistenceMode.Attribute)]
		[Category("Data")]
		public wwDataBindingItem BindingItem
		{
			get { return _BindingItem; }
		}
		private wwDataBindingItem _BindingItem = new wwDataBindingItem();


		protected override void OnInit(EventArgs e)
		{
			this.BindingItem.ControlId = this.ID;
			this.BindingItem.ControlInstance = this;
			this.BindingItem.Page = this.Page;

			base.OnInit(e);

		}


	}
}