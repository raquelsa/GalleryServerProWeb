using System;
using System.Collections;

namespace GalleryServerPro.WebControls
{
	/// <summary>
	/// Collection of individual PopupInfoItems. Implemented explicitly as
	/// a CollectionBase class rather than using List#PopupInfoItem#
	/// so that Add can be overridden
	/// </summary>
	public class PopupInfoItemCollection : CollectionBase
	{
		/// <summary>
		/// Internal reference to the PopupInfo object
		/// that is passed to the individual items if available
		/// </summary>
		PopupInfo _ParentPopupInfo = null;

		/// <summary>
		/// Preferred Constructor - Add a reference to the PopupInfo object here
		/// so a reference can be passed to the children.
		/// </summary>
		/// <param name="Parent"></param>
		public PopupInfoItemCollection(PopupInfo Parent)
		{
			this._ParentPopupInfo = Parent;
		}

		/// <summary>
		/// Not the preferred constructor - If possible pass a reference to the
		/// Binder object in the overloaded version.
		/// </summary>
		public PopupInfoItemCollection()
		{
		}

		/// <summary>
		/// Public indexer for the Items
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public PopupInfoItem this[int index]
		{
			get
			{
				return this.InnerList[index] as PopupInfoItem;
			}
			set
			{
				this.InnerList[index] = value;
			}
		}


		/// <summary>
		/// Add a PopupInfoItem to the collection
		/// </summary>
		/// <param name="Item"></param>
		public void Add(PopupInfoItem Item)
		{
			if (_ParentPopupInfo != null)
			{
				Item.Page = _ParentPopupInfo.Page;
				Item.PopupInfoContainer = _ParentPopupInfo;

				// *** VS Designer adds new items as soon as they are accessed
				// *** but items may not be valid so we have to clean up
				if (this._ParentPopupInfo.DesignMode)
				{
					// *** Remove any blank items
					UpdateListInDesignMode();
				}
			}

			this.InnerList.Add(Item);
		}


		/// <summary>
		/// Add a PopupInfoItem to the collection
		/// </summary>
		/// <param name="index"></param>
		/// <param name="Item"></param>
		public void AddAt(int index, PopupInfoItem Item)
		{
			if (_ParentPopupInfo != null)
			{
				Item.Page = _ParentPopupInfo.Page;
				Item.PopupInfoContainer = _ParentPopupInfo;

				// *** VS Designer adds new items as soon as their accessed
				// *** but items may not be valid so we have to clean up
				if (this._ParentPopupInfo.DesignMode)
				{
					UpdateListInDesignMode();
				}
			}

			InnerList.Insert(index, Item);
		}

		/// <summary>
		/// We have to delete 'empty' items because the designer requires items to be 
		/// added to the collection just for editing. This way we may have one 'extra'
		/// item, but not a whole long list of items.
		/// </summary>
		private void UpdateListInDesignMode()
		{
			if (this._ParentPopupInfo == null)
				return;

			bool Update = false;

			// *** Remove empty items - so the designer doesn't create excessive empties
			for (int x = 0; x < this.Count; x++)
			{
				if (string.IsNullOrEmpty(this[x].DialogTitle) && string.IsNullOrEmpty(this[x].DialogBody))
				{
					this.RemoveAt(x);
					Update = true;
				}
			}

			if (Update)
				this._ParentPopupInfo.NotifyDesigner();
		}
	}
}
