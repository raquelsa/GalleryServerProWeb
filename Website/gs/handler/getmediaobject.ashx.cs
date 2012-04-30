using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.SessionState;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;
using Image = System.Drawing.Image;

namespace GalleryServerPro.Web.Handler
{
	/// <summary>
	/// Defines a handler that sends the specified media object to the output stream.
	/// </summary>
	[System.Web.Services.WebService(Namespace = "http://tempuri.org/")]
	[System.Web.Services.WebServiceBinding(ConformsTo = System.Web.Services.WsiProfiles.BasicProfile1_1)]
	public class getmediaobject : IHttpHandler, IReadOnlySessionState
	{
		#region Private Fields

		private static int _bufferSize;

		private HttpContext _context;
		private int _galleryIdInQueryString = int.MinValue;
		private int _galleryId = int.MinValue;
		private int _mediaObjectId;
		private DisplayObjectType _displayType;
		private bool _sendAsAttachment;

		private IGalleryObject _mediaObject;
		private string _mediaObjectFilePath;
		private IGallerySettings _gallerySetting;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the media object being requested. Guaranteed to not return null; returns <see cref="Business.NullObjects.NullGalleryObject" />
		/// when no media object is being requested or it is invalid. This property does not verify the user has permission to view the
		/// media object.
		/// </summary>
		/// <value>The media object being requested.</value>
		private IGalleryObject MediaObject
		{
			get
			{
				if (_mediaObject == null)
				{
					if (_mediaObjectId > int.MinValue)
					{
						try
						{
							_mediaObject = Factory.LoadMediaObjectInstance(_mediaObjectId);
						}
						catch (InvalidMediaObjectException)
						{
							_mediaObject = new Business.NullObjects.NullGalleryObject();
						}
					}
					else
					{
						_mediaObject = new Business.NullObjects.NullGalleryObject();
					}
				}

				return _mediaObject;
			}
		}

		/// <summary>
		/// Gets the file path to the requested media object. It will be the thumbnail, optimized, or original file depending
		/// on which version is being requested.
		/// </summary>
		/// <value>The file path to the requested media object.</value>
		private string MediaObjectFilePath
		{
			get
			{
				if (_mediaObjectFilePath == null)
				{
					switch (_displayType)
					{
						case DisplayObjectType.Thumbnail:
							_mediaObjectFilePath = MediaObject.Thumbnail.FileNamePhysicalPath;
							break;
						case DisplayObjectType.Optimized:
							_mediaObjectFilePath = MediaObject.Optimized.FileNamePhysicalPath;
							break;
						case DisplayObjectType.Original:
							_mediaObjectFilePath = MediaObject.Original.FileNamePhysicalPath;
							break;
					}
				}

				return _mediaObjectFilePath;
			}
		}

		/// <summary>
		/// Gets the MIME type for the requested media object. It will be for the thumbnail, optimized, or original file depending
		/// on which version is being requested.
		/// </summary>
		/// <value>The MIME type for the requested media object.</value>
		private IMimeType MimeType
		{
			get
			{
				switch (_displayType)
				{
					case DisplayObjectType.Thumbnail:
						return MediaObject.Thumbnail.MimeType;
					case DisplayObjectType.Optimized:
						return MediaObject.Optimized.MimeType;
					case DisplayObjectType.Original:
						return MediaObject.Original.MimeType;
					default:
						return MediaObject.Original.MimeType;
				}
			}
		}

		/// <summary>
		/// Gets the gallery ID associated with the media object being requested. If no media object is available (perhaps an empty
		/// album thumbnail is being requested), then use the gallery ID specified in the query string.
		/// </summary>
		/// <value>The gallery ID.</value>
		private int GalleryId
		{
			get
			{
				if (_galleryId == int.MinValue)
				{
					if (!(MediaObject is Business.NullObjects.NullGalleryObject))
					{
						_galleryId = MediaObject.GalleryId;
					}
					else
					{
						_galleryId = _galleryIdInQueryString;
					}
				}

				return _galleryId;
			}
		}

		/// <summary>
		/// Gets the gallery settings for the gallery the requested media object is in.
		/// </summary>
		/// <value>The gallery settings.</value>
		private IGallerySettings GallerySettings
		{
			get
			{
				if (_gallerySetting == null)
				{
					_gallerySetting = Factory.LoadGallerySetting(GalleryId);
				}

				return _gallerySetting;
			}
		}

		/// <summary>
		/// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
		/// </returns>
		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
		/// </summary>
		/// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
		public void ProcessRequest(HttpContext context)
		{
			// Send the specified media object to the output stream.
			// Expected format:
			// /dev/gs/handler/getmediaobject.ashx?moid=34&amp;dt=1&amp;g=1
			// moid: The media object ID. Is int.MinValue if an empty image is to be returned.
			// dt: The display type. It is an integer that maps to the enum GalleryServerPro.Business.DisplayObjectType.
			// (0=Unknown,1=Thumbnail,2=Optimized,3=Original) At present all values other than 3 (Original) are ignored. If 3,
			// a security check is done to make sure user is authorized to view original images.
			// g: The gallery ID.
			// If URL encryption is enabled, then the entire query string portion is encrypted.
			try
			{
				if (!GalleryController.IsInitialized)
				{
					GalleryController.InitializeGspApplication();
				}

				if (InitializeVariables(context))
				{
					if (!IsUserAuthorized())
					{
						this._context.Response.StatusCode = 403;
						this._context.Response.End();
					}

					if (IsMediaObjectRequest() && !DoesMediaObjectExist())
					{
						this._context.Response.StatusCode = 404;
						this._context.Response.End();
					}

					ShowMediaObject();
				}
			}
			catch (System.Threading.ThreadAbortException)
			{
				throw; // We don't want these to fall into the generic catch because we don't want them logged.
			}
			catch (Exception ex)
			{
				AppErrorController.LogError(ex);
			}
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Initialize the class level variables with information from the query string. Returns false if the variables cannot 
		/// be properly initialized.
		/// </summary>
		/// <param name="context">The HttpContext for the current request.</param>
		/// <returns>Returns true if all variables were initialized; returns false if there was a problem and one or more variables
		/// could not be set.</returns>
		private bool InitializeVariables(HttpContext context)
		{
			this._context = context;

			if (!ExtractQueryStringParms(context.Request.Url.Query))
				return false;

			if (_bufferSize == 0)
			{
				_bufferSize = AppSetting.Instance.MediaObjectDownloadBufferSize;
			}

			if (DisplayObjectTypeEnumHelper.IsValidDisplayObjectType(this._displayType))
			{
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Extract information from the query string and assign to our class level variables. Return false if something goes wrong
		/// and the variables cannot be set. This will happen when the query string is in an unexpected format.
		/// </summary>
		/// <param name="queryString">The query string for the current request. Can be populated with HttpContext.Request.Url.Query.
		/// Must start with a question mark (?).</param>
		/// <returns>Returns true if all relevant variables were assigned from the query string; returns false if there was a problem.</returns>
		private bool ExtractQueryStringParms(string queryString)
		{
			if (String.IsNullOrEmpty(queryString)) return false;

			queryString = queryString.Remove(0, 1); // Strip off the ?

			bool filepathIsEncrypted = AppSetting.Instance.EncryptMediaObjectUrlOnClient;
			if (filepathIsEncrypted)
			{
				// Decode, then decrypt the query string. Note that we must replace spaces with a '+'. This is required when the the URL is
				// used in javascript to create the Silverlight media player. Apparently, Silverlight or the media player javascript decodes
				// the query string when it requests the URL, so that means any instances of '%2b' are decoded into '+' before it gets here.
				// Ideally, we wouldn't even call UrlDecode in this case, but we don't have a way of knowing that it has already been decoded.
				// So we decode anyway, which doesn't cause any harm *except* it converts '+' to a space, so we need to convert them back.
				queryString = HelperFunctions.Decrypt(HttpUtility.UrlDecode(queryString).Replace(" ", "+"));
			}

			//moid={0}&dt={1}g={2}
			foreach (string nameValuePair in queryString.Split(new char[] { '&' }))
			{
				string[] nameOrValue = nameValuePair.Split(new char[] { '=' });
				switch (nameOrValue[0])
				{
					case "g":
						{
							int gid;
							if (Int32.TryParse(nameOrValue[1], out gid))
								_galleryIdInQueryString = gid;
							else
								return false;
							break;
						}
					case "moid":
						{
							int moid;
							if (Int32.TryParse(nameOrValue[1], out moid))
								_mediaObjectId = moid;
							else
								return false;
							break;
						}
					case "dt":
						{
							int dtInt;
							if (Int32.TryParse(nameOrValue[1], out dtInt))
							{
								if (DisplayObjectTypeEnumHelper.IsValidDisplayObjectType((DisplayObjectType)dtInt))
								{
									_displayType = (DisplayObjectType)dtInt; break;
								}
								else
									return false;
							}
							else
								return false;
						}
					case "sa":
						{
							_sendAsAttachment = ((nameOrValue[1].Equals("1", StringComparison.Ordinal)) || (nameOrValue[1].Equals("TRUE", StringComparison.OrdinalIgnoreCase)));
							break;
						}
					default: return false; // Unexpected query string parm. Return false so execution is aborted.
				}
			}

			ValidateDisplayType();

			return true;
		}

		/// <summary>
		/// If an optimized version is being requested, make sure a file name is specified for it. If not, switch to the original version.
		/// This switch will be necessary for most non-image media objects, since the client usually requests optimized versions for everything.
		/// </summary>
		/// <remarks>This function became necessary when switching to the ID-based request in 2.4 (rather than the file-based request). It was 
		/// considered to change the requesting logic to ensure the correct display type is specified, and while that seems preferable from an
		/// architectural perspective, it was more complex to implement and potentially more fragile than this simple function.</remarks>
		private void ValidateDisplayType()
		{
			if ((_displayType == DisplayObjectType.Optimized) && (String.IsNullOrEmpty(MediaObjectFilePath)))
			{
				_displayType = DisplayObjectType.Original;
				_mediaObjectFilePath = null;

				// Comment out the exception, as it generates unnecessary errors when bots request deleted items
				//if (String.IsNullOrEmpty(MediaObjectFilePath))
				//{
				//  throw new InvalidMediaObjectException(String.Format(CultureInfo.CurrentCulture, "A request was made to the Gallery Server Pro HTTP handler to serve the optimized image for media object ID {0}, but either the media object does not exist or neither the optimized nor the original has a filename stored in the database, and therefore cannot be served.", _mediaObjectId));
				//}
			}
		}

		private bool IsUserAuthorized()
		{
			// If no media object is specified, then return true (this happens for empty album thumbnails).
			if (MediaObject.Id == int.MinValue)
			{
				return true;
			}

			SecurityActions requestedPermission = SecurityActions.ViewAlbumOrMediaObject;

			if ((this._displayType == DisplayObjectType.Original) && (MediaObject.MimeType.TypeCategory == MimeTypeCategory.Image))
			{
				requestedPermission = SecurityActions.ViewOriginalImage;
			}

			return Utils.IsUserAuthorized(requestedPermission, RoleController.GetGalleryServerRolesForUser(), MediaObject.Parent.Id, GalleryId, MediaObject.IsPrivate);
		}

		/// <summary>
		/// Determines whether the current request is for a media object. Returns <c>true</c> when the moid query string parameter
		/// has a value greater than <see cref="Int32.MinValue" />. Empty albums using this handler to generate a default image
		/// pass Int32.MinValue for the media object ID, so in these cases this function returns <c>false</c>.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if the current request is for a media object; otherwise, <c>false</c>.
		/// </returns>
		private bool IsMediaObjectRequest()
		{
			return (_mediaObjectId > int.MinValue);
		}

		/// <summary>
		/// Returns a value indicating whether the requested media object currently exists in the gallery.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if the requested media object exists; otherwise, <c>false</c>.
		/// </returns>
		private bool DoesMediaObjectExist()
		{
			return (MediaObject.Id > int.MinValue);
		}

		private void ShowMediaObject()
		{
			if (MediaObject.Id == int.MinValue)
			{
				// A filename matching the DefaultFilename constant is our signal to generate the
				// default album thumbnail and send to client.
				ProcessDefaultThumbnail();
			}
			else
			{
				if (!MimeTypeEnumHelper.IsValidMimeTypeCategory(MediaObject.MimeType.TypeCategory))
				{
					throw new UnexpectedQueryStringException();
				}

				if ((MediaObject.MimeType.TypeCategory != MimeTypeCategory.Image) && (this._mediaObjectId > int.MinValue))
				{
					// We never apply the watermark to non-image media objects.
					ProcessMediaObject();
				}
				else
				{
					// Apply watermark to thumbnails only when the config setting applyWatermarkToThumbnails = true.
					// Apply watermark to optimized and original images only when applyWatermark = true.
					bool applyWatermark = GallerySettings.ApplyWatermark;
					bool applyWatermarkToThumbnails = GallerySettings.ApplyWatermarkToThumbnails;
					bool isThumbnail = (_displayType == DisplayObjectType.Thumbnail);

					if (AppSetting.Instance.License.IsInReducedFunctionalityMode && !isThumbnail)
					{
						ProcessMediaObjectWithWatermark();
					}
					else if ((applyWatermark && !isThumbnail) || (applyWatermark && applyWatermarkToThumbnails && isThumbnail))
					{
						// If the user belongs to a role with watermarks set to visible, then show it; otherwise don't show the watermark.
						if (Utils.IsUserAuthorized(SecurityActions.HideWatermark, RoleController.GetGalleryServerRolesForUser(), MediaObject.Parent.Id, GalleryId, MediaObject.IsPrivate))
						{
							// Show the image without the watermark.
							ProcessMediaObject();
						}
						else
						{
							// Overlay watermark on image before sending it to client.
							ProcessMediaObjectWithWatermark();
						}
					}
					else
					{
						ProcessMediaObject();
					}
				}
			}
		}

		private void ProcessDefaultThumbnail()
		{
			// Generate the default album thumbnail and send to client.
			Bitmap bmp = null;
			try
			{
				this._context.Response.ContentType = "image/jpeg";

				HttpCachePolicy cachePolicy = this._context.Response.Cache;
				cachePolicy.SetExpires(DateTime.Now.AddSeconds(2592000)); // 30 days
				cachePolicy.SetCacheability(HttpCacheability.Public);
				cachePolicy.SetValidUntilExpires(true);

				bmp = GetDefaultThumbnailBitmap();
				bmp.Save(_context.Response.OutputStream, ImageFormat.Jpeg);
			}
			finally
			{
				if (bmp != null)
					bmp.Dispose();
			}
		}

		private Bitmap GetDefaultThumbnailBitmap()
		{
			//Return a bitmap of a default album image.  This will be used when no actual
			//image is available to serve as the pictorial view of the album.

			float ratio = GallerySettings.EmptyAlbumThumbnailWidthToHeightRatio;
			int maxLength = GallerySettings.MaxThumbnailLength;
			string imageText = GallerySettings.EmptyAlbumThumbnailText;
			string fontName = GallerySettings.EmptyAlbumThumbnailFontName;
			int fontSize = GallerySettings.EmptyAlbumThumbnailFontSize;
			Color bgColor = HelperFunctions.GetColor(GallerySettings.EmptyAlbumThumbnailBackgroundColor);
			Color fontColor = HelperFunctions.GetColor(GallerySettings.EmptyAlbumThumbnailFontColor);

			int rctWidth, rctHeight; //Image width and height
			int x; //Starting point from left for the text
			int y; //Start point from top for the text

			if (ratio > 1)
			{
				rctWidth = maxLength;
				rctHeight = Convert.ToInt32((float)maxLength / ratio);
			}
			else
			{
				rctHeight = maxLength;
				rctWidth = Convert.ToInt32((float)maxLength * ratio);
			}

			Bitmap bmp = null;
			Graphics g = null;
			try
			{
				// If the font name does not match an installed font, .NET will substitute Microsoft Sans Serif.
				Font fnt = new Font(fontName, fontSize);
				Rectangle rct = new Rectangle(0, 0, rctWidth, rctHeight);
				bmp = new Bitmap(rct.Width, rct.Height);
				g = Graphics.FromImage(bmp);

				// Calculate x and y offset for text
				Size textSize = g.MeasureString(imageText, fnt).ToSize();

				x = (rctWidth - textSize.Width) / 2;
				y = (rctHeight - textSize.Height) / 2;

				if (x < 0) x = 0;
				if (y < 0) y = 0;

				// Generate image
				g.FillRectangle(new SolidBrush(bgColor), rct);
				g.DrawString(imageText, fnt, new SolidBrush(fontColor), x, y);
			}
			catch
			{
				if (bmp != null)
					bmp.Dispose();

				throw;
			}
			finally
			{
				if (g != null)
					g.Dispose();
			}

			return bmp;
		}

		private void ProcessMediaObject()
		{
			// Send the specified file to the client.
			try
			{
				this._context.Response.Clear();
				this._context.Response.AddHeader("Content-Disposition", string.Format("inline;filename=\"{0}\"", MakeFileNameDownloadFriendly(Path.GetFileName(MediaObjectFilePath))));
				this._context.Response.ContentType = MimeType.FullType;
				this._context.Response.Buffer = false;

				HttpCachePolicy cachePolicy = this._context.Response.Cache;
				cachePolicy.SetExpires(DateTime.Now.AddSeconds(2592000)); // 30 days
				cachePolicy.SetCacheability(HttpCacheability.Public);
				cachePolicy.SetValidUntilExpires(true);

				FileStream fileStream = null;
				try
				{
					byte[] buffer = new byte[_bufferSize];

					try
					{
						fileStream = File.OpenRead(MediaObjectFilePath);
					}
					catch (ArgumentException) { return; }			 // If the file or directory isn't found, just return. This helps avoid clogging the error log 
					catch (FileNotFoundException) { return; }	 // with entries caused by search engine retrieving media objects that have been moved or deleted.		 
					catch (DirectoryNotFoundException) { return; }

					// Required for Silverlight to properly work
					this._context.Response.AddHeader("Content-Length", fileStream.Length.ToString(CultureInfo.InvariantCulture));

					int byteCount;
					while ((byteCount = fileStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						if (this._context.Response.IsClientConnected)
						{
							this._context.Response.OutputStream.Write(buffer, 0, byteCount);
							this._context.Response.Flush();
						}
						else
						{
							return;
						}
					}
				}
				finally
				{
					if (fileStream != null)
						fileStream.Close();
				}
			}
			catch (Exception ex)
			{
				AppErrorController.LogError(ex);
			}
		}

		private void ProcessMediaObjectWithWatermark()
		{
			// Send the specified file to the client with the watermark overlayed on top.
			this._context.Response.Clear();
			this._context.Response.ContentType = MediaObject.MimeType.FullType;

			Image watermarkedImage = null;
			try
			{
				try
				{
					watermarkedImage = ImageHelper.AddWatermark(MediaObjectFilePath, MediaObject.GalleryId);
				}
				catch (Exception ex)
				{
					// Can't apply watermark to image. Substitute an error image and send that to the user.
					if (!(ex is FileNotFoundException))
					{
						// Don't log FileNotFoundException exceptions. This helps avoid clogging the error log 
						// with entries caused by search engine retrieving media objects that have been moved or deleted.
						AppErrorController.LogError(ex);
					}
					watermarkedImage = Image.FromFile(this._context.Request.MapPath(String.Concat(Utils.GalleryRoot, "/images/error_48x48.png")));
				}

				watermarkedImage.Save(this._context.Response.OutputStream, ImageFormat.Jpeg);
			}
			finally
			{
				if (watermarkedImage != null)
					watermarkedImage.Dispose();
			}
		}

		/// <summary>
		/// Makes the file name download friendly. Since IE automatically replaces spaces with underscores, we can prevent this by encoding them.
		/// The fileName is unmodified for all other browsers. Example: If fileName="My cat.jpg" and the current browser is IE, this function 
		/// returns "My%20cat.jpg".
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>Returns the <paramref name="fileName" /> parameter, modified if necessary.</returns>
		private string MakeFileNameDownloadFriendly(string fileName)
		{
			if (this._context.Request.Browser.Browser.Equals("IE", StringComparison.OrdinalIgnoreCase))
			{
				fileName = fileName.Replace(" ", "%20");
			}

			return fileName;
		}

		#endregion
	}
}
