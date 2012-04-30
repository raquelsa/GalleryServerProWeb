using System;
using System.Collections.Generic;
using System.IO;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Web.Controller
{
	/// <summary>
	/// Contains functionality for interacting with gallery objects (that is, media objects and albums). Typically web pages 
	/// directly call the appropriate business layer objects, but when a task involves multiple steps or the functionality 
	/// does not exist in the business layer, the methods here are used.
	/// </summary>
	public static class GalleryObjectController
	{
		#region Public Static Methods

		/// <summary>
		/// Persist the gallery object to the data store. This method updates the audit fields before saving. The currently logged
		/// on user is recorded as responsible for the changes. All gallery objects should be
		/// saved through this method rather than directly invoking the gallery object's Save method, unless you want to 
		/// manually update the audit fields yourself.
		/// </summary>
		/// <param name="galleryObject">The gallery object to persist to the data store.</param>
		/// <remarks>When no user name is available through <see cref="Utils.UserName" />, the string &lt;unknown&gt; is
		/// substituted. Since GSP requires users to be logged on to edit objects, there will typically always be a user name 
		/// available. However, in some cases one won't be available, such as when an error occurs during self registration and
		/// the exception handling code needs to delete the just-created user album.</remarks>
		public static void SaveGalleryObject(IGalleryObject galleryObject)
		{
			string userName = (String.IsNullOrEmpty(Utils.UserName) ? Resources.GalleryServerPro.Site_Missing_Data_Text : Utils.UserName);
			SaveGalleryObject(galleryObject, userName);
		}

		/// <summary>
		/// Persist the gallery object to the data store. This method updates the audit fields before saving. All gallery objects should be
		/// saved through this method rather than directly invoking the gallery object's Save method, unless you want to
		/// manually update the audit fields yourself.
		/// </summary>
		/// <param name="galleryObject">The gallery object to persist to the data store.</param>
		/// <param name="userName">The user name to be associated with the modifications. This name is stored in the internal
		/// audit fields associated with this gallery object.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
		public static void SaveGalleryObject(IGalleryObject galleryObject, string userName)
		{
			if (galleryObject == null)
				throw new ArgumentNullException("galleryObject");

			DateTime currentTimestamp = DateTime.Now;

			if (galleryObject.IsNew)
			{
				galleryObject.CreatedByUserName = userName;
				galleryObject.DateAdded = currentTimestamp;
			}

			if (galleryObject.HasChanges)
			{
				galleryObject.LastModifiedByUserName = userName;
				galleryObject.DateLastModified = currentTimestamp;
			}

			// Verify that any role needed for album ownership exists and is properly configured.
			RoleController.ValidateRoleExistsForAlbumOwner(galleryObject as IAlbum);

			// Persist to data store.
			galleryObject.Save();
		}

		/// <summary>
		/// Move the specified object to the specified destination album. This method moves the physical files associated with this
		/// object to the destination album's physical directory. The object's Save() method is invoked to persist the changes to the
		/// data store. When moving albums, all the album's children, grandchildren, etc are also moved. 
		/// The audit fields are automatically updated before saving.
		/// </summary>
		/// <param name="galleryObjectToMove">The gallery object to move.</param>
		/// <param name="destinationAlbum">The album to which the current object should be moved.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObjectToMove" /> is null.</exception>
		public static void MoveGalleryObject(IGalleryObject galleryObjectToMove, IAlbum destinationAlbum)
		{
			if (galleryObjectToMove == null)
				throw new ArgumentNullException("galleryObjectToMove");

			string currentUser = Utils.UserName;
			DateTime currentTimestamp = DateTime.Now;

			galleryObjectToMove.LastModifiedByUserName = currentUser;
			galleryObjectToMove.DateLastModified = currentTimestamp;

			galleryObjectToMove.MoveTo(destinationAlbum);
		}

		/// <summary>
		/// Copy the specified object and place it in the specified destination album. This method creates a completely separate copy
		/// of the original, including copying the physical files associated with this object. The copy is persisted to the data
		/// store and then returned to the caller. When copying albums, all the album's children, grandchildren, etc are also copied.
		/// The audit fields of the copied objects are automatically updated before saving.
		/// </summary>
		/// <param name="galleryObjectToCopy">The gallery object to copy.</param>
		/// <param name="destinationAlbum">The album to which the current object should be copied.</param>
		/// <returns>
		/// Returns a new gallery object that is an exact copy of the original, except that it resides in the specified
		/// destination album, and of course has a new ID. Child objects are recursively copied.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObjectToCopy" /> is null.</exception>
		public static IGalleryObject CopyGalleryObject(IGalleryObject galleryObjectToCopy, IAlbum destinationAlbum)
		{
			if (galleryObjectToCopy == null)
				throw new ArgumentNullException("galleryObjectToCopy");

			string currentUser = Utils.UserName;

			return galleryObjectToCopy.CopyTo(destinationAlbum, currentUser);
		}

		/// <summary>
		/// Adds a media file to an album. Prior to calling this method, the file should exist in the 
		/// temporary upload directory (<see cref="GlobalConstants.TempUploadDirectory" />) in the 
		/// App_Data directory with the name <see cref="AddMediaObjectSettings.FileNameOnServer" />. The
		/// file is copied to the destination album and given the name of 
		/// <see cref="AddMediaObjectSettings.FileName" /> (instead of whatever name it currently has, which 
		/// may contain a GUID).
		/// </summary>
		/// <param name="settings">The settings that contain data and configuration options for the media file.</param>
		public static List<ActionResult> AddMediaObject(AddMediaObjectSettings settings)
		{
			List<ActionResult> results = CreateMediaObjectFromFile(settings);

			Utils.AddResultToSession(results);

			return results;
		}

		/// <summary>
		/// Return the requested display object from the specified media object. If Unknown is passed in the 
		/// displayType parameter, and the object is an image, return the optimized object. If an optimized 
		/// version does not exist, return the original object. If Unknown is passed in the displayType parameter, 
		/// and the object is NOT an image, return the original object. If a thumbnail is requested, always 
		/// return a thumbnail object.
		/// </summary>
		/// <param name="mediaObject">The media object containing the display object to return.</param>
		/// <param name="displayType">One of the DisplayObjectType enumeration values indicating which object to return.</param>
		/// <returns>Returns the requested display object from the specified media object.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaObject" /> is null.</exception>
		public static IDisplayObject GetDisplayObject(IGalleryObject mediaObject, DisplayObjectType displayType)
		{
			if (mediaObject == null)
				throw new ArgumentNullException("mediaObject");

			IDisplayObject displayObject;
			switch (displayType)
			{
				case DisplayObjectType.Thumbnail:
					{
						displayObject = mediaObject.Thumbnail;
						break;
					}
				case DisplayObjectType.Unknown:
				case DisplayObjectType.Optimized:
					{
						// Switch to original if it's not in the queue, there isn't an optimized file, or the opt file
						// is the same as the original file. 
						bool isOptMissing = String.IsNullOrEmpty(mediaObject.Optimized.FileName);
						bool notInQueue = !MediaConversionQueue.Instance.IsWaitingInQueueOrProcessing(mediaObject.Id);
						bool isOptSameAsOriginal = (mediaObject.Optimized.FileName == mediaObject.Original.FileName);
						if (notInQueue && (isOptMissing || isOptSameAsOriginal))
							displayObject = mediaObject.Original;
						else
							displayObject = mediaObject.Optimized;

						//displayObject = (mediaObject.Optimized.FileName == mediaObject.Original.FileName) ? mediaObject.Original : mediaObject.Optimized;
						break;
					}
				case DisplayObjectType.Original:
					{
						displayObject = mediaObject.Original;
						break;
					}
				default:
					{
						displayObject = (mediaObject.Optimized.FileName == mediaObject.Original.FileName) ? mediaObject.Original : mediaObject.Optimized;
						break;
					}
			}

			return displayObject;
		}

		#endregion

		#region Private Static Methods

		private static List<ActionResult> CreateMediaObjectFromFile(AddMediaObjectSettings options)
		{
			string sourceFilePath = Path.Combine(AppSetting.Instance.PhysicalApplicationPath, GlobalConstants.TempUploadDirectory, options.FileNameOnServer);

			try
			{
				IAlbum album = AlbumController.LoadAlbumInstance(options.AlbumId, true);

				if ((Path.GetExtension(options.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase)) && (options.ExtractZipFile))
				{
					// Extract the files from the zipped file.
					using (ZipUtility zip = new ZipUtility(Utils.UserName, RoleController.GetGalleryServerRolesForUser()))
					{
						using (FileStream fs = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
						{
							return zip.ExtractZipFile(fs, album, options.DiscardOriginalFile);
						}
					}
				}
				else
				{
					string albumPhysicalPath = album.FullPhysicalPathOnDisk;
					string filename = HelperFunctions.ValidateFileName(albumPhysicalPath, options.FileName);
					string filepath = Path.Combine(albumPhysicalPath, filename);

					MoveFile(filepath, sourceFilePath);

					ActionResult result = CreateMediaObject(filepath, album, options);
					return new List<ActionResult>() { result };
				}
			}
			catch (Exception ex)
			{
				AppErrorController.LogError(ex);
				return new List<ActionResult>()
				       	{
				       		new ActionResult()
				       			{
				       				Title = options.FileName,
				       				Status = ActionResultStatus.Error,
				       				Message = "The event log may have additional details."
				       			}
				       	};
			}
			finally
			{
				try
				{
					// If the file still exists in the temp directory, delete it. Typically this happens when we've
					// extracted the contents of a zip file (since other files will have already been moved to the dest album.)
					if (File.Exists(sourceFilePath))
					{
						File.Delete(sourceFilePath);
					}
				}
				catch (IOException) { } // Ignore an error; not a big deal if it continues to exist in the temp directory
				catch (UnauthorizedAccessException) { } // Ignore an error; not a big deal if it continues to exist in the temp directory
			}
		}

		private static void MoveFile(string filepath, string sourceFilePath)
		{
			// Move file to album. If IOException happens, wait 1 second and try again, up to 10 times.
			while (true)
			{
				int counter = 0;
				const int maxTries = 10;
				try
				{
					File.Move(sourceFilePath, filepath);
					break;
				}
				catch (IOException ex)
				{
					counter++;
					ex.Data.Add("CannotMoveFile", string.Format("This error occurred while trying to move file '{0}' to '{1}'. This error has occurred {2} times. The system will try again up to a maximum of {3} attempts.", sourceFilePath, filepath, counter, maxTries));
					AppErrorController.LogError(ex);

					if (counter >= maxTries)
						break;

					System.Threading.Thread.Sleep(1000);
				}
			}
		}

		private static ActionResult CreateMediaObject(string filePath, IAlbum album, AddMediaObjectSettings options)
		{
			var result = new ActionResult()
			{
				Title = Path.GetFileName(filePath)
			};

			try
			{
				using (IGalleryObject go = Factory.CreateMediaObjectInstance(filePath, album))
				{
					SaveGalleryObject(go);

					if (options.DiscardOriginalFile)
					{
						go.DeleteOriginalFile();
						GalleryObjectController.SaveGalleryObject(go);
					}

					result.Status = ActionResultStatus.Success;
				}
			}
			catch (UnsupportedMediaObjectTypeException ex)
			{
				try
				{
					File.Delete(filePath);
				}
				catch (UnauthorizedAccessException) { } // Ignore an error; the file will continue to exist in the destination album directory

				result.Status = ActionResultStatus.Error;
				result.Message = ex.Message;
			}

			return result;
		}

		#endregion
	}
}
