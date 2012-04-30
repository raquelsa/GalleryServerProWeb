using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Script.Services;
using System.Web.Services;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Data;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web;
using GalleryServerPro.Web.Controller;
using GalleryServerPro.Web.Entity;
using GalleryServerPro.Web.Pages;

namespace Gsp
{
	/// <summary>
	/// Contains web services that can be used to interact with the gallery.
	/// </summary>
	[ScriptService]
	[WebService(Namespace = "http://www.galleryserverpro.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	public class Gallery : WebService
	{
		#region Constructors

		/// <summary>
		/// Initializes the <see cref="Gallery"/> class.
		/// </summary>
		static Gallery()
		{
			if (!GalleryController.IsInitialized)
			{
				GalleryController.InitializeGspApplication();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Gallery"/> class.
		/// </summary>
		public Gallery()
		{
			// Ensure the app is initialized. This should have been done in the static constructor, but if anything went wrong
			// there, it may not be initialized, so we check again.
			if (!GalleryController.IsInitialized)
			{
				GalleryController.InitializeGspApplication();
			}
		}

		#endregion

		#region Public Web Methods

		/// <summary>
		/// Get information for the specified media object, including its previous and next media object.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the media object.</param>
		/// <param name="displayType">The type of display object to receive (thumbnail, optimized, original).</param>
		/// <returns>Returns an instance of MediaObjectWebEntity containing information for the specified media object,
		/// including its previous and next media object.</returns>
		[WebMethod(EnableSession = true)]
		public MediaObjectWebEntity GetMediaObjectHtml(int mediaObjectId, DisplayObjectType displayType)
		{
			try
			{
				return GalleryPage.GetMediaObjectHtml(Factory.LoadMediaObjectInstance(mediaObjectId), displayType, true);
			}
			catch (Exception ex)
			{
				AppErrorController.LogError(ex);
				throw;
			}
		}

		[WebMethod(EnableSession = true)]
		public List<ActionResult> AddMediaObject(AddMediaObjectSettings settings)
		{
			try
			{
				return GalleryObjectController.AddMediaObject(settings);
			}
			catch (Exception ex)
			{
				AppErrorController.LogError(ex);
				throw;
			}
		}

		/// <summary>
		/// Permanently deletes the specified media object from the file system and data store. No action is taken if the
		/// user does not have delete permission.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the media object to be deleted.</param>
		[WebMethod(EnableSession = true)]
		public void DeleteMediaObject(int mediaObjectId)
		{
			IGalleryObject mo = null;

			try
			{
				mo = Factory.LoadMediaObjectInstance(mediaObjectId);
				if (Utils.IsUserAuthorized(SecurityActions.DeleteMediaObject, mo.Parent.Id, mo.GalleryId, mo.IsPrivate))
				{
					mo.Delete();
					HelperFunctions.PurgeCache();
				}
			}
			catch (Exception ex)
			{
				if (mo != null)
				{
					AppErrorController.LogError(ex, mo.GalleryId);
				}
				else
				{
					AppErrorController.LogError(ex);
				}
				throw;
			}
		}

		/// <summary>
		/// Permanently deletes the specified albums from the file system and data store. No action is taken if the
		/// user does not have delete permission.
		/// </summary>
		/// <param name="albumIds">The IDs that uniquely identifies the albums to be deleted.</param>
		[WebMethod(EnableSession = true)]
		public void DeleteAlbums(int[] albumIds)
		{
			if (albumIds == null)
				return;

			IGalleryObject album = null;

			try
			{
				foreach (int albumId in albumIds)
				{
					try
					{
						album = AlbumController.LoadAlbumInstance(albumId, false);

						if (Utils.IsUserAuthorized(SecurityActions.DeleteAlbum, album.Id, album.GalleryId, album.IsPrivate))
						{
							album.Delete();
						}
					}
					catch (GalleryServerPro.ErrorHandler.CustomExceptions.InvalidAlbumException) { }
				}
				HelperFunctions.PurgeCache();
			}
			catch (Exception ex)
			{
				if (album != null)
				{
					AppErrorController.LogError(ex, album.GalleryId);
				}
				else
				{
					AppErrorController.LogError(ex);
				}
				throw;
			}
		}

		/// <summary>
		/// Update the media object with the specified title and persist to the data store. The title is validated before
		/// saving, and may be altered to conform to business rules, such as removing HTML tags. The validated title is returned.
		/// If the user is not authorized to edit the title, no action is taken and the original title as stored in the data
		/// store is returned.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the media object whose title is to be updated.</param>
		/// <param name="title">The title to be saved to the media object.</param>
		/// <returns>Returns the validated title.</returns>
		[WebMethod(EnableSession = true)]
		public string UpdateMediaObjectTitle(int mediaObjectId, string title)
		{
			IGalleryObject mo = null;

			try
			{
				mo = Factory.LoadMediaObjectInstance(mediaObjectId, true);
				if (Utils.IsUserAuthorized(SecurityActions.EditMediaObject, mo.Parent.Id, mo.GalleryId, mo.IsPrivate))
				{
					string previousTitle = mo.Title;
					mo.Title = Utils.CleanHtmlTags(title, mo.GalleryId);

					if (mo.Title != previousTitle)
						GalleryObjectController.SaveGalleryObject(mo);

					HelperFunctions.PurgeCache();
				}

				return mo.Title;
			}
			catch (Exception ex)
			{
				if (mo != null)
				{
					AppErrorController.LogError(ex, mo.GalleryId);
				}
				else
				{
					AppErrorController.LogError(ex);
				}
				throw;
			}
		}

		/// <summary>
		/// Update the album with the specified properties in the albumEntity parameter. The title is validated before
		/// saving, and may be altered to conform to business rules, such as removing HTML tags. After the object is persisted
		/// to the data store, the albumEntity parameter is updated with the latest properties from the album object and returned.
		/// If the user is not authorized to edit the album, no action is taken.
		/// </summary>
		/// <param name="albumEntity">An AlbumWebEntity instance containing data to be persisted to the data store.</param>
		/// <returns>Returns an AlbumWebEntity instance containing the data as persisted to the data store. Some properties,
		/// such as the Title property, may be slightly altered to conform to validation rules.</returns>
		[WebMethod(EnableSession = true)]
		public AlbumWebEntity UpdateAlbumInfo(AlbumWebEntity albumEntity)
		{
			if (albumEntity == null)
				return null;

			try
			{
				albumEntity.Owner = Utils.HtmlDecode(albumEntity.Owner);

				return AlbumController.UpdateAlbumInfo(albumEntity);
			}
			catch (Exception ex)
			{
				AppErrorController.LogError(ex);
				throw;
			}
		}

		/// <summary>
		/// Retrieve album information for the specified album ID. Returns an object with empty properties if the user
		/// does not have permission to view the specified album.
		/// </summary>
		/// <param name="albumId">The album ID for which to retrieve information.</param>
		/// <returns>Returns AlbumWebEntity object containing information about the requested album.</returns>
		[WebMethod(EnableSession = true)]
		public AlbumWebEntity GetAlbumInfo(int albumId)
		{
			IAlbum album = null;

			try
			{
				AlbumWebEntity albumEntity = new AlbumWebEntity();

				album = AlbumController.LoadAlbumInstance(albumId, false);

				if (Utils.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, albumId, album.GalleryId, album.IsPrivate))
				{
					albumEntity.Title = album.Title;
					albumEntity.Summary = album.Summary;
					albumEntity.DateStart = album.DateStart;
					albumEntity.DateEnd = album.DateEnd;
					albumEntity.IsPrivate = album.IsPrivate;
				}

				return albumEntity;
			}
			catch (Exception ex)
			{
				if (album != null)
				{
					AppErrorController.LogError(ex, album.GalleryId);
				}
				else
				{
					AppErrorController.LogError(ex);
				}
				throw;
			}
		}

		/// <summary>
		/// Get a list of metadata items corresponding to the specified mediaObjectId. Guaranteed to not return null.
		/// </summary>
		/// <param name="mediaObjectId">The ID of the media object for which to return its metadata items.</param>
		/// <returns>Returns a generic list of MetadataItemWebEntity objects that contain the metadata for the 
		/// specified media object.</returns>
		[WebMethod(EnableSession = true)]
		public List<MetadataItemWebEntity> GetMetadataItems(int mediaObjectId)
		{
			IGalleryObject mo = null;

			try
			{
				List<MetadataItemWebEntity> metadataItems = new List<MetadataItemWebEntity>();

				mo = Factory.LoadMediaObjectInstance(mediaObjectId);
				if (Utils.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), mo.Parent.Id, mo.GalleryId, mo.Parent.IsPrivate))
				{
					foreach (IGalleryObjectMetadataItem metadata in mo.MetadataItems.GetVisibleItems())
					{
						metadataItems.Add(new MetadataItemWebEntity(metadata.Description, metadata.Value));
					}
				}

				return metadataItems;
			}
			catch (Exception ex)
			{
				if (mo != null)
				{
					AppErrorController.LogError(ex, mo.GalleryId);
				}
				else
				{
					AppErrorController.LogError(ex);
				}
				throw;
			}
		}

		/// <summary>
		/// Permanently deletes the specified application error item from the error log. No action is taken if the
		/// user does not have delete permission.
		/// </summary>
		/// <param name="appErrorId">The ID that uniquely identifies the media object to be deleted.</param>
		[WebMethod(EnableSession = true)]
		public void DeleteAppError(int appErrorId)
		{
			IAppError appError = null;

			try
			{
				appError = Factory.GetAppErrors().FindById(appErrorId);

				bool isAuthorized = true;

				// If the error has a gallery ID (not all do), then check the user's permission. For those errors without a gallery ID,
				// just assume the user has permission, because there is no way to verify the user can delete this error. We could do something
				// that mostly works like verifying the user is a gallery admin for at least one gallery, but the function we are trying to
				// protect is deleting an error message, which is not that important to worry about.
				if (appError.GalleryId > int.MinValue)
				{
					isAuthorized = Utils.IsUserAuthorized(SecurityActions.AdministerSite | SecurityActions.AdministerGallery, RoleController.GetGalleryServerRolesForUser(), int.MinValue, appError.GalleryId, false);
				}

				if (isAuthorized)
				{
					GalleryServerPro.ErrorHandler.Error.Delete(appErrorId);
					HelperFunctions.PurgeCache();
				}
			}
			catch (Exception ex)
			{
				if (appError != null)
				{
					AppErrorController.LogError(ex, appError.GalleryId);
				}
				else
				{
					AppErrorController.LogError(ex);
				}
				throw;
			}
		}

		/// <summary>
		/// Stores the user's preference for showing or hiding the metadata popup window. The information is saved
		/// to the user's profile. Applies only to the gallery specified by <paramref name="galleryId" />.
		/// </summary>
		/// <param name="isVisible">if set to <c>true</c> the metadata popup window is visible; otherwise <c>false</c>.</param>
		/// <param name="galleryId">The gallery ID.</param>
		[WebMethod(EnableSession = true)]
		public void SetMetaDataVisibility(bool isVisible, int galleryId)
		{
			try
			{
				IUserProfile profile = ProfileController.GetProfile();
				profile.GetGalleryProfile(galleryId).ShowMediaObjectMetadata = isVisible;
				ProfileController.SaveProfile(profile);
			}
			catch (Exception ex)
			{
				AppErrorController.LogError(ex);

#if DEBUG
				throw;
#endif
			}
		}

		/// <summary>
		/// Excecute a maintenance routine to help ensure data integrity and eliminate unused data. For example, roles are synchronized between
		/// the membership system and the GSP roles. Also, albums with owners that no longer exist are reset to not have an owner. This web
		/// service method is intended to be called periodically; for example, once each time the application starts. Code in the Render
		/// method of the base class <see cref="GalleryPage" /> is responsible for knowing when and how to invoke this method.
		/// </summary>
		/// <remarks>The first iteration of the maintenace routine was invoked on a background thread, but background threads cannot access
		/// HttpContext.Current, which is required for the DotNetNuke implementation (and potential future versions of GSP's implementation),
		/// so that approach was replaced with this one.</remarks>
		[WebMethod(EnableSession = false)]
		public void PerformMaintenance()
		{
			try
			{
				Utils.PerformMaintenance();
			}
			catch (Exception ex)
			{
				AppErrorController.LogError(ex);
			}
		}

		/// <summary>
		/// Permanently deletes the specified queue items from the data store. Current user must be
		/// a gallery administrator for the gallery the media item belongs to; otherwise no action 
		/// is taken.
		/// </summary>
		/// <param name="mediaQueueIds">The media queue ids.</param>
		[WebMethod(EnableSession = false)]
		public void DeleteQueueItem(int[] mediaQueueIds)
		{
			try
			{
				foreach (int mediaQueueId in mediaQueueIds)
				{
					MediaQueueDto item = MediaConversionQueue.Instance.Get(mediaQueueId);

					if (item == null)
						continue;

					IGalleryObject mo;
					try
					{
						mo = Factory.LoadMediaObjectInstance(item.FKMediaObjectId);
					}
					catch (InvalidMediaObjectException)
					{
						continue;
					}

					if (Utils.IsCurrentUserGalleryAdministrator(mo.GalleryId))
					{
						MediaConversionQueue.Instance.RemoveMediaQueueItem(mediaQueueId);
					}
				}
			}
			catch (Exception ex)
			{
				AppErrorController.LogError(ex);
			}
		}

		/// <summary>
		/// Clears all caches relevant to the specified <paramref name="albumId" />. The logged on user 
		/// must have edit permission to this album.
		/// </summary>
		/// <param name="albumId">The album ID. All caches containing data relevant to this album are 
		/// cleared.</param>
		[WebMethod(EnableSession = true)]
		public void PurgeCache(int albumId)
		{
			IAlbum album = null;

			try
			{
				album = AlbumController.LoadAlbumInstance(albumId, false);

				// Get a list of all actions that require purging the cache. User must have one of these
				// permissions in order to purge the cache.
				const SecurityActions securityActions = SecurityActions.AddChildAlbum | SecurityActions.EditAlbum | SecurityActions.DeleteChildAlbum | SecurityActions.AddMediaObject | SecurityActions.EditMediaObject | SecurityActions.DeleteMediaObject;
				if (Utils.IsUserAuthorized(securityActions, albumId, album.GalleryId, album.IsPrivate))
				{
					HelperFunctions.PurgeCache();
				}
			}
			catch (Exception ex)
			{
				if (album != null)
				{
					AppErrorController.LogError(ex, album.GalleryId);
				}
				else
				{
					AppErrorController.LogError(ex);
				}
			}
		}

		///// <summary>
		///// Recycles the current web application.
		///// </summary>
		//[WebMethod(EnableSession = true)]
		//public void RecycleApp()
		//{
		//  Utils.ForceAppRecycle();
		//}

		#region Synchronize Web Services

		/// <summary>
		/// Synchronize the specified album with the physical directory on the hard drive.
		/// </summary>
		/// <param name="albumId">The album id for the album to synchronize.</param>
		/// <param name="synchId">A GUID that uniquely indentifies the synchronization. If another synchronization is in 
		/// progress, a <see cref="GalleryServerPro.ErrorHandler.CustomExceptions.SynchronizationInProgressException" /> exception is thrown.</param>
		/// <param name="isRecursive">If set to <c>true</c> the synchronization continues drilling down into directories
		/// below the current one.</param>
		/// <param name="rebuildThumbnails">if set to <c>true</c> the thumbnail image for each media 
		/// object is deleted and overwritten with a new one based on the original file. Applies to 
		/// all media objects.</param>
		/// <param name="rebuildOptimized">if set to <c>true</c> the optimized version of each media 
		/// object is deleted and overwritten with a new one based on the original file. Only relevant 
		/// for images and for video/audio files when FFmpeg is installed and an applicable encoder
		/// setting exists.</param>
		/// <param name="regenerateMetadata">if set to <c>true</c> the existing metadata for each media object is replaced with
		/// the metadata stored within the original media object file.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.SynchronizationInProgressException">
		/// Thrown if another synchronization is in progress.</exception>
		[WebMethod(EnableSession = true)]
		public void Synchronize(int albumId, string synchId, bool isRecursive, bool rebuildThumbnails, bool rebuildOptimized, bool regenerateMetadata)
		{
			IAlbum album = null;

			try
			{
				#region Check user authorization

				bool isUserAuthenticated = Utils.IsAuthenticated;
				if (!isUserAuthenticated)
					return;

				album = AlbumController.LoadAlbumInstance(albumId, true, true, false);

				if (!Utils.IsUserAuthorized(SecurityActions.Synchronize, RoleController.GetGalleryServerRolesForUser(), albumId, album.GalleryId, false))
					return;

				#endregion

				SynchronizationManager synchMgr = new SynchronizationManager(album.GalleryId);

				synchMgr.IsRecursive = isRecursive;
				synchMgr.RebuildThumbnail = rebuildThumbnails;
				synchMgr.RebuildOptimized = rebuildOptimized;
				synchMgr.RegenerateMetadata = regenerateMetadata;

				synchMgr.Synchronize(synchId, album, Utils.UserName);
			}
			catch (Exception ex)
			{
				if (album != null)
				{
					AppErrorController.LogError(ex, album.GalleryId);
				}
				else
				{
					AppErrorController.LogError(ex);
				}
				throw;
			}
		}

		/// <summary>
		/// Synchronize galleries in the data store with its physical directory on the hard drive. The <paramref name="password" />
		/// must match <see cref="IGallerySettings.RemoteAccessPassword" />. This method is designed to be invoked by anonymous users
		/// who have the correct password. The sync is invoked on a separate thread so this method returns immediately.
		/// </summary>
		/// <param name="isRecursive">If set to <c>true</c> the synchronization continues drilling down into directories
		/// below the current one.</param>
		/// <param name="rebuildThumbnails">if set to <c>true</c> the thumbnail image for each media 
		/// object is deleted and overwritten with a new one based on the original file. Applies to 
		/// all media objects.</param>
		/// <param name="rebuildOptimized">if set to <c>true</c> the optimized version of each media 
		/// object is deleted and overwritten with a new one based on the original file. Only relevant 
		/// for images and for video/audio files when FFmpeg is installed and an applicable encoder
		/// setting exists.</param>
		/// <param name="regenerateMetadata">if set to <c>true</c> the existing metadata for each media object is replaced with
		/// the metadata stored within the original media object file.</param>
		/// <param name="password">The password that authorizes the caller to invoke a synchronization.</param>
		/// <remarks>NOTE TO DEVELOPER: If you change the name of this method, update the property 
		/// <see cref="GalleryServerPro.Web.Pages.Admin.albums.SyncAllGalleriesUrl" />.</remarks>
		[WebMethod(EnableSession = false)]
		public void SyncAllGalleries(bool isRecursive, bool rebuildThumbnails, bool rebuildOptimized, bool regenerateMetadata, string password)
		{
			foreach (IGallery gallery in Factory.LoadGalleries())
			{
				SyncAlbum(Factory.LoadRootAlbumInstance(gallery.GalleryId, false).Id, isRecursive, rebuildThumbnails, rebuildOptimized, regenerateMetadata, password);
			}
		}

		/// <summary>
		/// Synchronize the specified album with the physical directory on the hard drive. The 
		/// <paramref name="password" /> must match <see cref="IGallerySettings.RemoteAccessPassword" />.
		/// This method is designed to be invoked by anonymous users who have the correct password. 
		/// The sync is invoked on a separate thread so this method returns immediately.
		/// The sync is recursive, with the rebuild thumbnail, rebuild optimized, and regenerate 
		/// metadata options disabled.
		/// </summary>
		/// <param name="isRecursive">If set to <c>true</c> the synchronization continues drilling 
		/// down into directories below the current one.</param>
		/// <param name="rebuildThumbnails">if set to <c>true</c> the thumbnail image for each media 
		/// object is deleted and overwritten with a new one based on the original file. Applies to 
		/// all media objects.</param>
		/// <param name="rebuildOptimized">if set to <c>true</c> the optimized version of each media 
		/// object is deleted and overwritten with a new one based on the original file. Only relevant 
		/// for images and for video/audio files when FFmpeg is installed and an applicable encoder
		/// setting exists.</param>
		/// <param name="regenerateMetadata">if set to <c>true</c> the existing metadata for each 
		/// media object is replaced with the metadata stored within the original media object file.</param>
		/// <param name="albumId">The album ID for the album to synchronize.</param>
		/// <param name="password">The password that authorizes the caller to invoke a 
		/// synchronization.</param>
		/// <remarks>NOTE TO DEVELOPER: If you change the name of this method, update the property 
		/// <see cref="GalleryServerPro.Web.Pages.Admin.albums.SyncAlbumUrl" />.</remarks>
		[WebMethod(EnableSession = false)]
		public void SyncAlbum(int albumId, bool isRecursive, bool rebuildThumbnails, bool rebuildOptimized, bool regenerateMetadata, string password)
		{
			IAlbum album = null;

			try
			{
				album = AlbumController.LoadAlbumInstance(albumId, true, true, false);

				if (!ValidateRemoteSync(album, password))
				{
					return;
				}

				SynchronizeSettingsEntity syncSettings = new SynchronizeSettingsEntity();
				syncSettings.SyncInitiator = SyncInitiator.RemoteApp;
				syncSettings.AlbumToSynchronize = album;
				syncSettings.IsRecursive = isRecursive;
				syncSettings.RebuildThumbnails = rebuildThumbnails;
				syncSettings.RebuildOptimized = rebuildOptimized;
				syncSettings.RegenerateMetadata = regenerateMetadata;

				// Start sync on new thread
				Thread notifyAdminThread = new Thread(GalleryController.Synchronize);
				notifyAdminThread.Start(syncSettings);
			}
			catch (Exception ex)
			{
				if (album != null)
				{
					AppErrorController.LogError(ex, album.GalleryId);
				}
				else
				{
					AppErrorController.LogError(ex);
				}
				throw;
			}
		}

		private static bool ValidateRemoteSync(IAlbum album, string password)
		{
			IGallerySettings gallerySettings = Factory.LoadGallerySetting(album.GalleryId);

			if (!gallerySettings.EnableRemoteSync)
			{
				AppErrorController.LogEvent(String.Format(CultureInfo.CurrentCulture, "Cannot start synchronization: A web service request to start synchronizing album '{0}' (ID {1}) was received, but the gallery is currently configured to disallow remote synchronizations. This feature can be enabled on the Albums page in the Site admin area.", album.Title, album.Id), album.GalleryId);
				return false;
			}

			if (!gallerySettings.RemoteAccessPassword.Equals(password))
			{
				AppErrorController.LogEvent(String.Format(CultureInfo.CurrentCulture, "Cannot start synchronization: A web service request to start synchronizing album '{0}' (ID {1}) was received, but the specified password is incorrect.", album.Title, album.Id), album.GalleryId);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Gets information about the current synchronization. If no synchronization is in progress, returns information about
		/// the most recent synchronization.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>
		/// Returns information about the current synchronization.
		/// </returns>
		[WebMethod]
		public SynchStatusWebEntity GetCurrentStatus(int galleryId)
		{
			ISynchronizationStatus synchStatus = SynchronizationStatus.GetInstance(galleryId);

			try
			{
				SynchStatusWebEntity synchStatusWeb = new SynchStatusWebEntity();

				synchStatusWeb.SynchId = synchStatus.SynchId;
				synchStatusWeb.TotalFileCount = synchStatus.TotalFileCount;
				synchStatusWeb.CurrentFileIndex = synchStatus.CurrentFileIndex + 1;

				if ((synchStatus.CurrentFilePath != null) && (synchStatus.CurrentFileName != null))
					synchStatusWeb.CurrentFile = System.IO.Path.Combine(synchStatus.CurrentFilePath, synchStatus.CurrentFileName);

				synchStatusWeb.Status = synchStatus.Status.ToString();
				synchStatusWeb.StatusForUI = GetFriendlyStatusText(synchStatus.Status);
				synchStatusWeb.PercentComplete = CalculatePercentComplete(synchStatus);

				// Update the Skipped Files, but only when the synch is complete.
				lock (synchStatus)
				{
					if (synchStatus.Status == SynchronizationState.Complete)
					{
						if (synchStatus.SkippedMediaObjects.Count > GlobalConstants.MaxNumberOfSkippedObjectsToDisplayAfterSynch)
						{
							// We have a large number of skipped media objects. We don't want to send it all to the browsers, because it might take
							// too long or cause an error if it serializes to a string longer than int.MaxValue, so let's trim it down.
							synchStatus.SkippedMediaObjects.RemoveRange(GlobalConstants.MaxNumberOfSkippedObjectsToDisplayAfterSynch,
																													synchStatus.SkippedMediaObjects.Count -
																													GlobalConstants.MaxNumberOfSkippedObjectsToDisplayAfterSynch);
						}
						synchStatusWeb.SkippedFiles = synchStatus.SkippedMediaObjects;
					}
				}
				return synchStatusWeb;
			}
			catch (Exception ex)
			{
				AppErrorController.LogError(ex, synchStatus.GalleryId);
				throw;
			}
		}

		/// <summary>
		/// Terminates the synchronization with the specified <paramref name="taskId"/>.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="taskId">The task id (also knows as synch ID) representing the synchronization to cancel.</param>
		[WebMethod]
		public void TerminateTask(int galleryId, string taskId)
		{
			ISynchronizationStatus synchStatus = SynchronizationStatus.GetInstance(galleryId);

			try
			{
				synchStatus.CancelSynchronization(taskId);
			}
			catch (Exception ex)
			{
				AppErrorController.LogError(ex);
				throw;
			}
		}

		#endregion

		#endregion

		#region Private Methods

		private static string GetFriendlyStatusText(SynchronizationState status)
		{
			switch (status)
			{
				case SynchronizationState.AnotherSynchronizationInProgress: return Resources.GalleryServerPro.Task_Synch_Progress_Status_SynchInProgressException_Hdr;
				case SynchronizationState.Complete: return status.ToString();
				case SynchronizationState.Error: return status.ToString();
				case SynchronizationState.PersistingToDataStore: return Resources.GalleryServerPro.Task_Synch_Progress_Status_PersistingToDataStore_Hdr;
				case SynchronizationState.SynchronizingFiles: return Resources.GalleryServerPro.Task_Synch_Progress_Status_SynchInProgress_Hdr;
				default: throw new System.ComponentModel.InvalidEnumArgumentException("The GetFriendlyStatusText() method in synchronize.aspx encountered a SynchronizationState enum value it was not designed for. This method must be updated.");
			}
		}

		private static int CalculatePercentComplete(ISynchronizationStatus synchStatus)
		{
			if (synchStatus.Status == SynchronizationState.SynchronizingFiles)
				return (int)(((double)synchStatus.CurrentFileIndex / (double)synchStatus.TotalFileCount) * 100);
			else
				return 100;
		}

		#endregion

	}
}
