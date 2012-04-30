using System;

namespace GalleryServerPro.WebControls
{
	/// <summary>
	/// Extender style interface that allows adding a PopupInfo 
	/// object to a control and interact with a DataBinder object
	/// on a Page. 
	/// 
	/// Any control marked with this interface can be automatically
	/// pulled into the a PopupInfo instance with 
	/// PopupInfo.LoadFromControls().
	/// </summary>
	public interface IPopupInfo
	{
		PopupInfoItem PopupInfoItem
		{
			get;
		}
	}
}
