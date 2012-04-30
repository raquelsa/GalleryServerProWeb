using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages.Task
{
	/// <summary>
	/// A page-like user control that handles the Assign thumbnail task.
	/// </summary>
	public partial class assignthumbnail : Pages.TaskPage
	{
		#region Private Fields

		private int _thumbnailMediaObjectId;

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the Init event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Init(object sender, EventArgs e)
		{
			this.TaskHeaderPlaceHolder = phTaskHeader;
			this.TaskFooterPlaceHolder = phTaskFooter;
		}

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			this.CheckUserSecurity(SecurityActions.EditAlbum);

			if (!IsPostBack)
			{
				ConfigureControls();
			}

			if ((this.GetAlbum() != null) && (this.GetAlbum().GetChildGalleryObjects(GalleryObjectType.All).Count == 0))
			{
				this.RedirectToAlbumViewPage("msg={0}", ((int)Message.CannotAssignThumbnailNoObjectsExistInAlbum).ToString(CultureInfo.InvariantCulture));
			}

			// Import javascript to help the radio button work properly. See KB 316495 at microsoft.com
			// for more info on the bug. The javascript work-around was posted by a MS employee on an aspnet
			// newsgroup and can be seen here: http://groups.google.com/groups?hl=en&lr=&ie=UTF-8&selm=w5qH8dXEEHA.616%40cpmsftngxa06.phx.gbl&rnum=2
			// Add reference to entityobjects.js.
			string scriptUrl = Utils.GetUrl("/script/radiobuttonworkaround.js");
			ScriptManager sm = ScriptManager.GetCurrent(this.Page);
			if (sm != null)
				sm.Scripts.Add(new ScriptReference(scriptUrl));
			else
				throw new WebException("Gallery Server Pro requires a ScriptManager on the page.");
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Gets the media object title, truncating it if necessary.
		/// </summary>
		/// <param name="title">The title.</param>
		/// <returns>Returns the media object title.</returns>
		protected string GetTitle(string title)
		{
			if (String.IsNullOrEmpty(title))
				return String.Empty;

			// Truncate the Title if it is too long
			int maxLength = GallerySettings.MaxMediaObjectThumbnailTitleDisplayLength;
			string truncatedText = Utils.TruncateTextForWeb(title, maxLength);
			string titleText;
			if (truncatedText.Length != title.Length)
				titleText = String.Format(CultureInfo.CurrentCulture, "{0}...", truncatedText);
			else
				titleText = truncatedText;

			return titleText;
		}

		/// <summary>
		/// Determines whether the specified media object ID is the currently assigned thumbnail for this album.
		/// </summary>
		/// <param name="mediaObjectId">A media object ID for which to determine whether it matches the currently assigned thumbnail
		/// for this album.</param>
		/// <returns>Returns true if the specified media object ID is the currently assigned thumbnail for this album; 
		/// otherwise returns false.</returns>
		protected bool IsAlbumThumbnail(int mediaObjectId)
		{
			return (this._thumbnailMediaObjectId == mediaObjectId);
		}

		/// <summary>
		/// Gets a value that uniquely identifies the specified <paramref name="galleryObject" /> (ex: "a25", "m223").
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		/// <returns>Returns an ID.</returns>
		protected static string GetId(IGalleryObject galleryObject)
		{
			// Prepend an 'a' (for album) or 'm' (for media object) to the ID to indicate whether it is
			// an album ID or media object ID.
			if (galleryObject is Album)
				return "a" + galleryObject.Id.ToString(CultureInfo.InvariantCulture);
			else
				return "m" + galleryObject.Id.ToString(CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Determines whether the event for the server control is passed up the page's UI server control hierarchy.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="args">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		/// <returns>
		/// true if the event has been canceled; otherwise, false. The default is false.
		/// </returns>
		protected override bool OnBubbleEvent(object source, EventArgs args)
		{
			//An event from the control has bubbled up.  If it's the Ok button, then run the
			//code to save the data to the database; otherwise ignore.
			Button btn = source as Button;
			if ((btn != null) && (((btn.ID == "btnOkTop") || (btn.ID == "btnOkBottom"))))
			{
				btnOkClicked();

				this.RedirectToAlbumViewPage("msg={0}", ((int)Message.ThumbnailSuccessfullyAssigned).ToString(CultureInfo.InvariantCulture));
			}

			return true;
		}

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			this.TaskHeaderText = Resources.GalleryServerPro.Task_Assign_Thumbnail_Header_Text;
			this.TaskBodyText = Resources.GalleryServerPro.Task_Assign_Thumbnail_Body_Text;
			this.OkButtonText = Resources.GalleryServerPro.Task_AssignThumbnail_Ok_Button_Text;
			this.OkButtonToolTip = Resources.GalleryServerPro.Task_AssignThumbnail_Ok_Button_Tooltip;

			this.PageTitle = Resources.GalleryServerPro.Task_AssignThumbnail_Page_Title;

			this._thumbnailMediaObjectId = this.GetAlbum().Thumbnail.MediaObjectId;

			IGalleryObjectCollection albumChildren = this.GetAlbum().GetChildGalleryObjects(GalleryObjectType.All, true);

			SetThumbnailCssStyle(albumChildren);

			rptr.DataSource = albumChildren;
			rptr.DataBind();
		}

		private void btnOkClicked()
		{
			//User clicked 'Assign thumbnail'.  Assign the specified thumbnail to this album.
			int moid = GetSelectedMediaObjectId();

			if (moid > int.MinValue)
			{
				IAlbum album = this.GetAlbum(true);
				album.ThumbnailMediaObjectId = moid;
				GalleryObjectController.SaveGalleryObject(album);

				HelperFunctions.PurgeCache();
			}
		}

		private int GetSelectedMediaObjectId()
		{
			// Return the media object ID for the object the user selected.
			string idCode = GetSelectedIdCode();

			int id;
			if (ParseAndValidateIdCode(idCode, out id))
			{
				return id;
			}
			else
			{
				return int.MinValue;
			}
		}

		private bool ParseAndValidateIdCode(string idCode, out int id)
		{
			id = int.MinValue;

			if (String.IsNullOrEmpty(idCode) || (idCode.Length < 2))
			{
				return false;
			}

			// Step 1: Parse object type and ID from ID code
			int idToTest;
			char idType; // 'a' or 'm' for album or media object

			try
			{
				// Make sure value is a valid media object ID
				idToTest = Convert.ToInt32(idCode.Substring(1), CultureInfo.InvariantCulture);
				idType = Convert.ToChar(idCode.Substring(0, 1), CultureInfo.InvariantCulture);
			}
			catch (FormatException)
			{
				return false;
			}
			catch (OverflowException)
			{
				return false;
			}

			// Step 2: Validate the ID. If it is an album, first get the thumbnail media object ID for the album.
			if (idType.Equals('a'))
			{
				try
				{
					idToTest = AlbumController.LoadAlbumInstance(idToTest, false).ThumbnailMediaObjectId;

					if (idToTest == 0)
					{
						// User selected an album with a blank thumbnail. There is nothing to validate, so just return.
						id = idToTest;
						return true;
					}
				}
				catch (InvalidAlbumException)
				{
					return false;
				}
			}

			try
			{
				IGalleryObject mediaObject = Factory.LoadMediaObjectInstance(idToTest);

				if (IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, mediaObject.Parent.Id, GalleryId, mediaObject.IsPrivate))
				{
					// VALID! Assign to output parameter and return.
					id = idToTest;
					return true;
				}
			}
			catch (ArgumentException)
			{
				return false;
			}
			catch (InvalidMediaObjectException)
			{
				return false;
			}

			return false;
		}

		private string GetSelectedIdCode()
		{
			RadioButton rb;
			string goId = String.Empty;

			// Loop through each item in the repeater control. If an item is checked, extract the ID.
			foreach (RepeaterItem rptrItem in rptr.Items)
			{
				rb = (RadioButton)rptrItem.Controls[1]; // The <INPUT TYPE="RADIO"> tag
				if (rb.Checked)
				{
					// RadioButton is checked. Get the ID. Albums have an 'a' prefix; images have a 'm' prefix (e.g. "a322", "m999")
					HiddenField gc = (HiddenField)rptrItem.FindControl("hdn");

					goId = gc.Value;

					break;
				}
			}
			return goId;
		}

		#endregion
	}
}