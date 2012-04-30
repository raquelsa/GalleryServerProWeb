using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages.Task
{
	/// <summary>
	/// A page-like user control that handles the Rotate image task.
	/// </summary>
	public partial class rotateimage : Pages.TaskPage
	{
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
			if (GallerySettings.MediaObjectPathIsReadOnly)
				RedirectToAlbumViewPage("msg={0}", ((int)Message.CannotEditGalleryIsReadOnly).ToString(CultureInfo.InvariantCulture));

			this.CheckUserSecurity(SecurityActions.EditMediaObject);

			if (this.GetMediaObject() == null)
			{
				Utils.Redirect(Utils.AddQueryStringParameter(Utils.GetCurrentPageUrl(), "msg=" + (int) Message.MediaObjectDoesNotExist));
			}

			if (!IsPostBack)
			{
				ConfigureControls();
			}
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
			//An event from a control has bubbled up.  If it's the Ok button, then run the
			//code to synchronize; otherwise ignore.
			Button btn = source as Button;
			if ((btn != null) && (((btn.ID == "btnOkTop") || (btn.ID == "btnOkBottom"))))
			{
				int msg = btnOkClicked();

				if (msg > int.MinValue)
					Utils.Redirect(PageId.mediaobject, "moid={0}&msg={1}", this.GetMediaObjectId(), msg.ToString(CultureInfo.InvariantCulture));
				else
					Utils.Redirect(PageId.mediaobject, "moid={0}&msg={1}", this.GetMediaObjectId(), ((int)Message.ObjectsSuccessfullyRotated).ToString(CultureInfo.InvariantCulture));}

			return true;
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Gets the URL to the specified <paramref name="galleryObject" />.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		/// <returns>Returns a string.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
		protected string GetImageUrl(IGalleryObject galleryObject)
		{
			if (galleryObject == null)
				throw new ArgumentNullException("galleryObject");

			if ((galleryObject.Optimized.Width > int.MinValue) || (galleryObject.Optimized.Height > int.MinValue))
				return GetOptimizedUrl(galleryObject);
			else
				return GetThumbnailUrl(galleryObject);
		}

		/// <summary>
		/// Gets the width of the specified <paramref name="galleryObject" />.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		/// <returns>The width.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
		protected static int GetImageWidth(IGalleryObject galleryObject)
		{
			if (galleryObject == null)
				throw new ArgumentNullException("galleryObject");

			if (galleryObject.Optimized.Width > int.MinValue)
				return galleryObject.Optimized.Width;
			else
				return galleryObject.Thumbnail.Width;
		}

		/// <summary>
		/// Gets the height of the specified <paramref name="galleryObject" />.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		/// <returns>The height.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
		protected static int GetImageHeight(IGalleryObject galleryObject)
		{
			if (galleryObject == null)
				throw new ArgumentNullException("galleryObject");

			if (galleryObject.Optimized.Height > int.MinValue)
				return galleryObject.Optimized.Height;
			else
				return galleryObject.Thumbnail.Height;
		}

		#endregion

		#region Public Properties

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			this.TaskHeaderText = Resources.GalleryServerPro.Task_Rotate_Image_Header_Text;
			this.TaskBodyText = Resources.GalleryServerPro.Task_Rotate_Image_Body_Text;
			this.OkButtonText = Resources.GalleryServerPro.Task_Rotate_Image_Ok_Button_Text;
			this.OkButtonToolTip = Resources.GalleryServerPro.Task_Rotate_Image_Ok_Button_Tooltip;

			this.PageTitle = Resources.GalleryServerPro.Task_Rotate_Image_Page_Title;

			IGalleryObjectCollection images = new GalleryObjectCollection();
			IGalleryObject image = this.GetMediaObject();
			
			if (image is GalleryServerPro.Business.Image)
			{
				images.Add(image);
				rptr.DataSource = images;
				rptr.DataBind();
			}
			else
			{
				Utils.Redirect(Web.PageId.mediaobject, "moid={0}&msg={1}", image.Id, ((int)Message.CannotRotateObjectNotRotatable).ToString(CultureInfo.InvariantCulture));
			}
		}

		private int btnOkClicked()
		{
			return rotateImage();
		}

		private int rotateImage()
		{
			// Rotate any images on the hard drive according to the user's wish.
			int returnValue = int.MinValue;

			Dictionary<int, RotateFlipType> imagesToRotate = retrieveUserSelections();

			foreach (KeyValuePair<int, RotateFlipType> kvp in imagesToRotate)
			{
				IGalleryObject image = Factory.LoadMediaObjectInstance(kvp.Key, GalleryObjectType.Image, true);
				image.Rotation = kvp.Value;
				try
				{
					GalleryObjectController.SaveGalleryObject(image);
				}
				catch (UnsupportedImageTypeException)
				{
					returnValue = (int)Message.CannotRotateInvalidImage;
				}
			}

			HelperFunctions.PurgeCache();

			return returnValue;
		}

		private Dictionary<int, RotateFlipType> retrieveUserSelections()
		{
			// Iterate through all the objects, retrieving the orientation of each image. If the
			// orientation has changed (it is no longer set to 't' for top), then add it to an array.
			// The media object IDs are stored in a hidden input tag.
			HtmlInputHidden moidTag;

			Dictionary<int, RotateFlipType> imagesToRotate = new Dictionary<int, RotateFlipType>();
			HtmlInputHidden rotateTag = (HtmlInputHidden)rptr.Items[0].FindControl("txtSelectedSide");

			if (rotateTag.Value.Trim().Length < 1)
				return imagesToRotate;

			char newOrientation = Convert.ToChar(rotateTag.Value.Trim().Substring(0, 1), CultureInfo.InvariantCulture);
			// If the orientation value isn't valid, throw an exception.
			if ((newOrientation != 't') && (newOrientation != 'r') && (newOrientation != 'b') && (newOrientation != 'l'))
				throw new GalleryServerPro.ErrorHandler.CustomExceptions.UnexpectedFormValueException();

			RotateFlipType rft;
			if (newOrientation != 't')
			{
				// User selected an orientation other than t(top). Add to array.
				switch (newOrientation)
				{
					case 'r': rft = RotateFlipType.Rotate270FlipNone; break;
					case 'b': rft = RotateFlipType.Rotate180FlipNone; break;
					case 'l': rft = RotateFlipType.Rotate90FlipNone; break;
					default: rft = RotateFlipType.RotateNoneFlipNone; break; // Should never get here because of our if condition above, but let's be safe
				}

				// User selected an orientation other than t(top). Add to dictionary.
				moidTag = (HtmlInputHidden)rptr.Items[0].FindControl("moid"); // The hidden <input> tag with the media object ID
				int moid;
				if (Int32.TryParse(moidTag.Value, out moid))
				{
					imagesToRotate.Add(Convert.ToInt32(moidTag.Value, CultureInfo.InvariantCulture), rft);
				}
				else
					throw new GalleryServerPro.ErrorHandler.CustomExceptions.UnexpectedFormValueException();
			}
			return imagesToRotate;
		}

		#endregion
	}
}