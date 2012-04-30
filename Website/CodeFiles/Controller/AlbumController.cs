using System;
using System.Collections.Generic;
using System.ComponentModel;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Entity;
using System.Globalization;

namespace GalleryServerPro.Web.Controller
{
	/// <summary>
	/// Contains functionality for interacting with albums. Typically web pages directly call the appropriate business layer objects,
	/// but when a task involves multiple steps or the functionality does not exist in the business layer, the methods here are
	/// used.
	/// </summary>
	public static class AlbumController
	{
		#region Public Static Methods

		/// <summary>
		/// Generate a read-only, inflated <see cref="IAlbum" /> instance with optionally inflated child media objects. Metadata 
		/// for media objects are automatically loaded. The album's <see cref="IAlbum.ThumbnailMediaObjectId" /> property is set 
		/// to its value from the data store, but the <see cref="IGalleryObject.Thumbnail" /> property is only inflated when 
		/// accessed. Guaranteed to not return null.
		/// </summary>
		/// <param name="albumId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies the album to retrieve.</param>
		/// <param name="inflateChildMediaObjects">When true, the child media objects of the album are added and inflated.
		/// Child albums are added but not inflated. When false, they are not added or inflated.</param>
		/// <returns>Returns an inflated album instance with all properties set to the values from the data store.</returns>
		/// <exception cref="InvalidAlbumException">Thrown when an album with the specified <paramref name = "albumId" /> 
		/// is not found in the data store.</exception>
		public static IAlbum LoadAlbumInstance(int albumId, bool inflateChildMediaObjects)
		{
			return LoadAlbumInstance(albumId, inflateChildMediaObjects, false, true);
		}

		/// <summary>
		/// Generate an inflated <see cref="IAlbum" /> instance with optionally inflated child media objects. Metadata 
		/// for media objects are automatically loaded. Use the <paramref name="isWritable" /> parameter to specify a writeable, 
		/// thread-safe instance that can be modified and persisted to the data store. The 
		/// album's <see cref="IAlbum.ThumbnailMediaObjectId" /> property is set to its value from the data store, but the 
		/// <see cref="IGalleryObject.Thumbnail" /> property is only inflated when accessed. Guaranteed to not return null.
		/// </summary>
		/// <param name="albumId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies the album to retrieve.</param>
		/// <param name="inflateChildMediaObjects">When true, the child media objects of the album are added and inflated.
		/// Child albums are added but not inflated. When false, they are not added or inflated.</param>
		/// <param name="isWritable">When set to <c>true</c> then return a unique instance that is not shared across threads.</param>
		/// <returns>Returns an inflated album instance with all properties set to the values from the data store.</returns>
		/// <exception cref="InvalidAlbumException">Thrown when an album with the specified <paramref name = "albumId" /> 
		/// is not found in the data store.</exception>
		public static IAlbum LoadAlbumInstance(int albumId, bool inflateChildMediaObjects, bool isWritable)
		{
			return LoadAlbumInstance(albumId, inflateChildMediaObjects, isWritable, true);
		}

		/// <summary>
		/// Generate an inflated <see cref="IAlbum" /> instance with optionally inflated child media objects, and optionally specifying
		/// whether to suppress the loading of media object metadata. Use the <paramref name="isWritable" />
		/// parameter to specify a writeable, thread-safe instance that can be modified and persisted to the data store. The 
		/// album's <see cref="IAlbum.ThumbnailMediaObjectId" /> property is set to its value from the data store, but the 
		/// <see cref="IGalleryObject.Thumbnail" /> property is only inflated when accessed. Guaranteed to not return null.
		/// </summary>
		/// <param name="albumId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies the album to retrieve.</param>
		/// <param name="inflateChildMediaObjects">When true, the child media objects of the album are added and inflated.
		/// Child albums are added but not inflated. When false, they are not added or inflated.</param>
		/// <param name="isWritable">When set to <c>true</c> then return a unique instance that is not shared across threads.</param>
		/// <param name="allowMetadataLoading">If set to <c>false</c>, the metadata for media objects are not loaded.</param>
		/// <returns>Returns an inflated album instance with all properties set to the values from the data store.</returns>
		/// <exception cref="InvalidAlbumException">Thrown when an album with the specified <paramref name = "albumId" /> 
		/// is not found in the data store.</exception>
		public static IAlbum LoadAlbumInstance(int albumId, bool inflateChildMediaObjects, bool isWritable, bool allowMetadataLoading)
		{
			IAlbum album = Factory.LoadAlbumInstance(albumId, inflateChildMediaObjects, isWritable, allowMetadataLoading);

			ValidateAlbumOwner(album);

			return album;
		}

		/// <summary>
		/// Creates an album, assigns the user name as the owner, saves it, and returns the newly created album.
		/// A profile entry is created containing the album ID. Returns null if the ID specified in the gallery settings
		/// for the parent album does not represent an existing album. That is, returns null if <see cref="IGallerySettings.UserAlbumParentAlbumId" />
		/// does not match an existing album.
		/// </summary>
		/// <param name="userName">The user name representing the user who is the owner of the album.</param>
		/// <param name="galleryId">The gallery ID for the gallery in which the album is to be created.</param>
		/// <returns>
		/// Returns the newly created user album. It has already been persisted to the database.
		/// Returns null if the ID specified in the gallery settings for the parent album does not represent an existing album.
		/// That is, returns null if <see cref="IGallerySettings.UserAlbumParentAlbumId" />
		/// does not match an existing album.
		/// </returns>
		public static IAlbum CreateUserAlbum(string userName, int galleryId)
		{
			IGallerySettings gallerySetting = Factory.LoadGallerySetting(galleryId);

			string albumNameTemplate = gallerySetting.UserAlbumNameTemplate;

			IAlbum parentAlbum;
			try
			{
				parentAlbum = AlbumController.LoadAlbumInstance(gallerySetting.UserAlbumParentAlbumId, false);
			}
			catch (InvalidAlbumException ex)
			{
				// The parent album does not exist. Record the error and return null.
				string galleryDescription = Utils.HtmlEncode(Factory.LoadGallery(gallerySetting.GalleryId).Description);
				string msg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Error_User_Album_Parent_Invalid_Ex_Msg, galleryDescription, gallerySetting.UserAlbumParentAlbumId);
				AppErrorController.LogError(new WebException(msg, ex), galleryId);
				return null;
			}

			IAlbum album = null;
			try
			{
				album = Factory.CreateEmptyAlbumInstance(parentAlbum.GalleryId);

				album.Title = albumNameTemplate.Replace("{UserName}", userName);
				album.Summary = gallerySetting.UserAlbumSummaryTemplate;
				album.OwnerUserName = userName;
				//newAlbum.ThumbnailMediaObjectId = 0; // not needed
				album.Parent = parentAlbum;
				album.IsPrivate = parentAlbum.IsPrivate;
				GalleryObjectController.SaveGalleryObject(album, userName);

				SaveAlbumIdToProfile(album.Id, userName, album.GalleryId);

				HelperFunctions.PurgeCache();
			}
			catch
			{
				if (album != null)
					album.Dispose();

				throw;
			}

			return album;
		}

		/// <summary>
		/// Get a reference to the highest level album in the specified <paramref name="galleryId" /> the current user has permission 
		/// to add albums to. Returns null if no album meets this criteria.
		/// </summary>
		/// <param name="galleryId">The ID of the gallery.</param>
		/// <returns>Returns a reference to the highest level album the user has permission to add albums to.</returns>
		public static IAlbum GetHighestLevelAlbumWithCreatePermission(int galleryId)
		{
			// Step 1: Loop through the roles and compile a list of album IDs where the role has create album permission.
			IGallery gallery = Factory.LoadGallery(galleryId);
			List<int> rootAlbumIdsWithCreatePermission = new List<int>();

			foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser())
			{
				if (role.Galleries.Contains(gallery))
				{
					if (role.AllowAddChildAlbum)
					{
						foreach (int albumId in role.RootAlbumIds)
						{
							if (!rootAlbumIdsWithCreatePermission.Contains(albumId))
								rootAlbumIdsWithCreatePermission.Add(albumId);
						}
					}
				}
			}

			// Step 2: Loop through our list of album IDs. If any album belongs to another gallery, remove it. If any album has an ancestor 
			// that is also in the list, then remove it. We only want a list of top level albums.
			List<int> albumIdsToRemove = new List<int>();
			foreach (int albumIdWithCreatePermission in rootAlbumIdsWithCreatePermission)
			{
				IGalleryObject album = AlbumController.LoadAlbumInstance(albumIdWithCreatePermission, false);

				if (album.GalleryId != galleryId)
				{
					// Album belongs to another gallery. Mark it for deletion.
					albumIdsToRemove.Add(albumIdWithCreatePermission);
				}
				else
				{
					while (true)
					{
						album = album.Parent as IAlbum;
						if (album == null)
							break;

						if (rootAlbumIdsWithCreatePermission.Contains(album.Id))
						{
							// Album has an ancestor that is also in the list. Mark it for deletion.
							albumIdsToRemove.Add(albumIdWithCreatePermission);
							break;
						}
					}
				}
			}

			foreach (int albumId in albumIdsToRemove)
			{
				rootAlbumIdsWithCreatePermission.Remove(albumId);
			}

			// Step 3: Starting with the root album, start iterating through the child albums. When we get to
			// one in our list, we can conclude that is the highest level album for which the user has create album permission.
			return FindFirstMatchingAlbumRecursive(Factory.LoadRootAlbumInstance(galleryId), rootAlbumIdsWithCreatePermission);
		}

		/// <summary>
		/// Get a reference to the highest level album in the specified <paramref name="galleryId" /> the current user has permission to 
		/// add albums and/or media objects to. Returns null if no album meets this criteria.
		/// </summary>
		/// <param name="verifyAddAlbumPermissionExists">Specifies whether the current user must have permission to add child albums
		/// to the album.</param>
		/// <param name="verifyAddMediaObjectPermissionExists">Specifies whether the current user must have permission to add media objects
		/// to the album.</param>
		/// <param name="galleryId">The ID of the gallery.</param>
		/// <returns>
		/// Returns a reference to the highest level album the user has permission to add albums and/or media objects to.
		/// </returns>
		public static IAlbum GetHighestLevelAlbumWithAddPermission(bool verifyAddAlbumPermissionExists, bool verifyAddMediaObjectPermissionExists, int galleryId)
		{
			// Step 1: Loop through the roles and compile a list of album IDs where the role has the required permission.
			// If the verifyAddAlbumPermissionExists parameter is true, then the user must have permission to add child albums.
			// If the verifyAddMediaObjectPermissionExists parameter is true, then the user must have permission to add media objects.
			// If either parameter is false, then the absense of that permission does not disqualify an album.
			IGallery gallery = Factory.LoadGallery(galleryId);

			List<int> rootAlbumIdsWithPermission = new List<int>();
			foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser())
			{
				if (role.Galleries.Contains(gallery))
				{
					bool albumPermGranted = (verifyAddAlbumPermissionExists ? role.AllowAddChildAlbum : true);
					bool mediaObjectPermGranted = (verifyAddMediaObjectPermissionExists ? role.AllowAddMediaObject : true);

					if (albumPermGranted && mediaObjectPermGranted)
					{
						// This role satisfies the requirements, so add each album to the list.
						foreach (int albumId in role.RootAlbumIds)
						{
							if (!rootAlbumIdsWithPermission.Contains(albumId))
								rootAlbumIdsWithPermission.Add(albumId);
						}
					}
				}
			}

			// Step 2: Loop through our list of album IDs. If any album belongs to another gallery, remove it. If any album has an ancestor 
			// that is also in the list, then remove it. We only want a list of top level albums.
			List<int> albumIdsToRemove = new List<int>();
			foreach (int albumIdWithPermission in rootAlbumIdsWithPermission)
			{
				IGalleryObject album = AlbumController.LoadAlbumInstance(albumIdWithPermission, false);

				if (album.GalleryId != galleryId)
				{
					// Album belongs to another gallery. Mark it for deletion.
					albumIdsToRemove.Add(albumIdWithPermission);
				}
				else
				{
					while (true)
					{
						album = album.Parent as IAlbum;
						if (album == null)
							break;

						if (rootAlbumIdsWithPermission.Contains(album.Id))
						{
							// Album has an ancestor that is also in the list. Mark it for deletion.
							albumIdsToRemove.Add(albumIdWithPermission);
							break;
						}
					}
				}
			}

			foreach (int albumId in albumIdsToRemove)
			{
				rootAlbumIdsWithPermission.Remove(albumId);
			}

			// Step 3: Starting with the root album, start iterating through the child albums. When we get to
			// one in our list, we can conclude that is the highest level album for which the user has create album permission.
			return FindFirstMatchingAlbumRecursive(Factory.LoadRootAlbumInstance(galleryId), rootAlbumIdsWithPermission);
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
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="albumEntity" /> is null.</exception>
		public static AlbumWebEntity UpdateAlbumInfo(AlbumWebEntity albumEntity)
		{
			if (albumEntity == null)
				throw new ArgumentNullException("albumEntity");

			if (albumEntity.Owner == Resources.GalleryServerPro.UC_Album_Header_Edit_Album_No_Owner_Text)
			{
				albumEntity.Owner = String.Empty;
			}

			IAlbum album = AlbumController.LoadAlbumInstance(albumEntity.Id, false, true);

			// Update remaining properties if user has edit album permission.
			if (Utils.IsUserAuthorized(SecurityActions.EditAlbum, album.Id, album.GalleryId, album.IsPrivate))
			{
				if (album.Title != albumEntity.Title)
				{
					IGallerySettings gallerySetting = Factory.LoadGallerySetting(album.GalleryId);

					album.Title = Utils.CleanHtmlTags(albumEntity.Title, album.GalleryId);
					if ((!album.IsRootAlbum) && (gallerySetting.SynchAlbumTitleAndDirectoryName))
					{
						// Root albums do not have a directory name that reflects the album's title, so only update this property for non-root albums.
						album.DirectoryName = HelperFunctions.ValidateDirectoryName(album.Parent.FullPhysicalPath, album.Title, gallerySetting.DefaultAlbumDirectoryNameLength);
					}
				}
				album.Summary = Utils.CleanHtmlTags(albumEntity.Summary, album.GalleryId);
				album.DateStart = albumEntity.DateStart.Date;
				album.DateEnd = albumEntity.DateEnd.Date;
				if (albumEntity.IsPrivate != album.IsPrivate)
				{
					if (!albumEntity.IsPrivate && album.Parent.IsPrivate)
					{
						throw new NotSupportedException("Cannot make album public: It is invalid to make an album public when it's parent album is private.");
					}
					album.IsPrivate = albumEntity.IsPrivate;
					SynchIsPrivatePropertyOnChildGalleryObjects(album);
				}

				// If the owner has changed, update it, but only if the user is administrator.
				if (albumEntity.Owner != album.OwnerUserName)
				{
					if (Utils.IsUserAuthorized(SecurityActions.AdministerSite | SecurityActions.AdministerGallery, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, album.IsPrivate))
					{
						if (!String.IsNullOrEmpty(album.OwnerUserName))
						{
							// Another user was previously assigned as owner. Delete role since this person will no longer be the owner.
							RoleController.DeleteGalleryServerProRole(album.OwnerRoleName);
						}

						if (UserController.GetUsersCurrentUserCanView(album.GalleryId).Contains(albumEntity.Owner) || String.IsNullOrEmpty(albumEntity.Owner))
						{
							
							// GalleryObjectController.SaveGalleryObject will make sure there is a role created for this user.
							album.OwnerUserName = albumEntity.Owner;
						}
					}
				}

				GalleryObjectController.SaveGalleryObject(album);
				HelperFunctions.PurgeCache();

				// Refresh the entity object with the data from the album object, in case something changed. For example,
				// some javascript or HTML may have been removed from the Title or Summary fields.
				albumEntity.Title = album.Title;
				albumEntity.Summary = album.Summary;
				albumEntity.DateStart = album.DateStart;
				albumEntity.DateEnd = album.DateEnd;
				albumEntity.IsPrivate = album.IsPrivate;
				albumEntity.Owner = album.OwnerUserName;
			}

			return albumEntity;
		}

		/// <overloads>
		/// Permanently delete this album from the data store and optionally the hard drive.
		/// </overloads>
		/// <summary>
		/// Permanently delete this album from the data store and the hard drive. Validation is performed prior to deletion to ensure
		/// album can be safely deleted. The validation is contained in the method <see cref="ValidateBeforeAlbumDelete"/>
		/// and may be invoked separately if desired. No security checks are performed; the caller must ensure the user
		/// has permission to delete an album prior to invoking this method.
		/// </summary>
		/// <param name="album">The album to delete. If null, the function returns without taking any action.</param>
		/// <param name="deleteFromFileSystem">if set to <c>true</c> the files and directories associated with the album
		/// are deleted from the hard disk. Set this to <c>false</c> to delete only the database records.</param>
		/// <exception cref="ErrorHandler.CustomExceptions.CannotDeleteAlbumException">Thrown when the album does not meet the
		/// requirements for safe deletion. At this time this exception is thrown only when the album is or contains the user album
		/// parent album and user albums are enabled.</exception>
		public static void DeleteAlbum(IAlbum album)
		{
			DeleteAlbum(album, true);
		}

		/// <summary>
		/// Permanently delete this album from the data store and optionally the hard drive. Validation is performed prior to deletion to ensure
		/// album can be safely deleted. The validation is contained in the method <see cref="ValidateBeforeAlbumDelete"/>
		/// and may be invoked separately if desired. No security checks are performed; the caller must ensure the user
		/// has permission to delete an album prior to invoking this method.
		/// </summary>
		/// <param name="album">The album to delete. If null, the function returns without taking any action.</param>
		/// <param name="deleteFromFileSystem">if set to <c>true</c> the files and directories associated with the album
		/// are deleted from the hard disk. Set this to <c>false</c> to delete only the database records.</param>
		/// <exception cref="ErrorHandler.CustomExceptions.CannotDeleteAlbumException">Thrown when the album does not meet the
		/// requirements for safe deletion. At this time this exception is thrown only when the album is or contains the user album
		/// parent album and user albums are enabled.</exception>
		public static void DeleteAlbum(IAlbum album, bool deleteFromFileSystem)
		{
			if (album == null)
				return;

			ValidateBeforeAlbumDelete(album);

			OnBeforeAlbumDelete(album);

			if (deleteFromFileSystem)
			{
				album.Delete();
			}
			else
			{
				album.DeleteFromGallery();
			}

			HelperFunctions.PurgeCache();
		}

		/// <summary>
		/// Verifies that the album meets the prerequisites to be safely deleted but does not actually delete the album. Throws a
		/// CannotDeleteAlbumException when it cannot be deleted.
		/// </summary>
		/// <param name="albumToDelete">The album to delete.</param>
		/// <remarks>This function is automatically called when using the <see cref="DeleteAlbum"/> method, so it is not necessary to 
		/// invoke when using that method. Typically you will call this method when there are several items to delete and you want to 
		/// check all of them before deleting any of them, such as we have on the Delete Objects page.</remarks>
		/// <exception cref="ErrorHandler.CustomExceptions.CannotDeleteAlbumException">Thrown when the album does not meet the 
		/// requirements for safe deletion.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="albumToDelete" /> is null.</exception>
		public static void ValidateBeforeAlbumDelete(IAlbum albumToDelete)
		{
			if (albumToDelete == null)
				throw new ArgumentNullException("albumToDelete");

			AlbumDeleteValidator validator = new AlbumDeleteValidator(albumToDelete);

			validator.Validate();

			if (!validator.CanBeDeleted)
			{
				switch (validator.ValidationFailureReason)
				{
					case GalleryObjectDeleteValidationFailureReason.AlbumSpecifiedAsUserAlbumContainer:
					case GalleryObjectDeleteValidationFailureReason.AlbumContainsUserAlbumContainer:
						{
							string albumTitle = String.Concat("'", albumToDelete.Title, "' (ID# ", albumToDelete.Id, ")");
							string msg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Task_Delete_Album_Cannot_Delete_Contains_User_Album_Parent_Ex_Msg, albumTitle);

							throw new CannotDeleteAlbumException(msg);
						}
					case GalleryObjectDeleteValidationFailureReason.AlbumSpecifiedAsDefaultGalleryObject:
					case GalleryObjectDeleteValidationFailureReason.AlbumContainsDefaultGalleryObjectAlbum:
					case GalleryObjectDeleteValidationFailureReason.AlbumContainsDefaultGalleryObjectMediaObject:
						{
							string albumTitle = String.Concat("'", albumToDelete.Title, "' (ID# ", albumToDelete.Id, ")");
							string msg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Task_Delete_Album_Cannot_Delete_Contains_Default_Gallery_Object_Ex_Msg, albumTitle);

							throw new CannotDeleteAlbumException(msg);
						}
					default:
						throw new InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The function ValidateBeforeAlbumDelete is not designed to handle the enumeration value {0}. The function must be updated.", validator.ValidationFailureReason));
				}
			}
		}

		#endregion

		#region Private Static Methods

		/// <summary>
		/// Performs any necessary actions that must occur before an album is deleted. Specifically, it deletes the owner role 
		/// if one exists for the album, but only when this album is the only one assigned to the role. It also clears out  
		/// <see cref="IGallerySettings.UserAlbumParentAlbumId" /> if the album's ID matches it. This function recursively calls
		/// itself to make sure all child albums are processed.
		/// </summary>
		/// <param name="album">The album to be deleted, or one of its child albums.</param>
		private static void OnBeforeAlbumDelete(IAlbum album)
		{
			// If there is an owner role associated with this album, and the role is not assigned to any other albums, delete it.
			if (!String.IsNullOrEmpty(album.OwnerRoleName))
			{
				IGalleryServerRole role = RoleController.GetGalleryServerRoles().GetRole(album.OwnerRoleName);

				if ((role != null) && (role.AllAlbumIds.Count == 1) && role.AllAlbumIds.Contains(album.Id))
				{
					RoleController.DeleteGalleryServerProRole(role.RoleName);
				}
			}

			// If the album is specified as the user album container, clear out the setting. The ValidateBeforeAlbumDelete()
			// function will throw an exception if user albums are enabled, so this should only happen when user albums
			// are disabled, so it is safe to clear it out.
			int userAlbumParentAlbumId = Factory.LoadGallerySetting(album.GalleryId).UserAlbumParentAlbumId;
			if (album.Id == userAlbumParentAlbumId)
			{
				IGallerySettings gallerySettingsWriteable = Factory.LoadGallerySetting(album.GalleryId, true);
				gallerySettingsWriteable.UserAlbumParentAlbumId = 0;
				gallerySettingsWriteable.Save();
			}

			// Recursively validate child albums.
			foreach (IGalleryObject childAlbum in album.GetChildGalleryObjects(GalleryObjectType.Album))
			{
				OnBeforeAlbumDelete((IAlbum)childAlbum);
			}
		}

		/// <summary>
		/// Finds the first album within the heirarchy of the specified <paramref name="album"/> whose ID is in 
		/// <paramref name="albumIds"/>. Acts recursively in an across-first, then-down search pattern, resulting 
		/// in the highest level matching album to be returned. Returns null if there are no matching albums.
		/// </summary>
		/// <param name="album">The album to be searched to see if it, or any of its children, matches one of the IDs
		/// in <paramref name="albumIds"/>.</param>
		/// <param name="albumIds">Contains the IDs of the albums to search for.</param>
		/// <returns>Returns the first album within the heirarchy of the specified <paramref name="album"/> whose ID is in 
		/// <paramref name="albumIds"/>.</returns>
		private static IAlbum FindFirstMatchingAlbumRecursive(IAlbum album, ICollection<int> albumIds)
		{
			// Is the current album in the list?
			if (albumIds.Contains(album.Id))
				return album;

			// Nope, so look at the child albums of this album.
			IAlbum albumToSelect = null;
			IGalleryObjectCollection childAlbums = album.GetChildGalleryObjects(GalleryObjectType.Album, true);

			foreach (IGalleryObject childAlbum in childAlbums)
			{
				if (albumIds.Contains(childAlbum.Id))
				{
					albumToSelect = (IAlbum)childAlbum;
					break;
				}
			}

			// Not the child albums either, so iterate through the children of the child albums. Act recursively.
			if (albumToSelect == null)
			{
				foreach (IGalleryObject childAlbum in childAlbums)
				{
					albumToSelect = FindFirstMatchingAlbumRecursive((IAlbum)childAlbum, albumIds);

					if (albumToSelect != null)
						break;
				}
			}

			return albumToSelect; // Returns null if no matching album is found
		}

		private static void SaveAlbumIdToProfile(int albumId, string userName, int galleryId)
		{
			IUserProfile profile = ProfileController.GetProfile(userName);

			IUserGalleryProfile pg = profile.GetGalleryProfile(galleryId);
			pg.UserAlbumId = albumId;

			ProfileController.SaveProfile(profile);
		}

		/// <summary>
		/// Set the IsPrivate property of all child albums and media objects of the specified album to have the same value
		/// as the specified album.
		/// </summary>
		/// <param name="album">The album whose child objects are to be updated to have the same IsPrivate value.</param>
		private static void SynchIsPrivatePropertyOnChildGalleryObjects(IAlbum album)
		{
			album.Inflate(true);
			foreach (IAlbum childAlbum in album.GetChildGalleryObjects(GalleryObjectType.Album))
			{
				childAlbum.Inflate(true); // The above Inflate() does not inflate child albums, so we need to explicitly inflate it.
				childAlbum.IsPrivate = album.IsPrivate;
				GalleryObjectController.SaveGalleryObject(childAlbum);
				SynchIsPrivatePropertyOnChildGalleryObjects(childAlbum);
			}

			foreach (IGalleryObject childGalleryObject in album.GetChildGalleryObjects(GalleryObjectType.MediaObject))
			{
				childGalleryObject.IsPrivate = album.IsPrivate;
				GalleryObjectController.SaveGalleryObject(childGalleryObject);
			}
		}

		#endregion

		/// <summary>
		/// Inspects the specified <paramref name="album" /> to see if the <see cref="IAlbum.OwnerUserName" /> is an existing user.
		/// If not, the property is cleared out (which also clears out the <see cref="IAlbum.OwnerRoleName" /> property).
		/// </summary>
		/// <param name="album">The album to inspect.</param>
		private static void ValidateAlbumOwner(IAlbum album)
		{
			if ((!String.IsNullOrEmpty(album.OwnerUserName)) && (!UserController.GetAllUsers().Contains(album.OwnerUserName)))
			{
				if (RoleController.GetUsersInRole(album.OwnerRoleName).Length == 0)
				{
					RoleController.DeleteGalleryServerProRole(album.OwnerRoleName);
				}

				if (album.IsWritable)
				{
					album.OwnerUserName = String.Empty; // This will also clear out the OwnerRoleName property.

					GalleryObjectController.SaveGalleryObject(album);
				}
				else
				{
					// Load a writeable version and update the database, then do the same update to our in-memory instance.
					IAlbum albumWritable = Factory.LoadAlbumInstance(album.Id, false, true);

					albumWritable.OwnerUserName = String.Empty; // This will also clear out the OwnerRoleName property.

					GalleryObjectController.SaveGalleryObject(albumWritable);

					// Update our local in-memory object to match the one we just saved.
					album.OwnerUserName = String.Empty;
					album.OwnerRoleName = String.Empty;
				}

				// Remove this item from cache so that we don't have any old copies floating around.
				Dictionary<int, IAlbum> albumCache = (Dictionary<int, IAlbum>)HelperFunctions.GetCache(CacheItem.Albums);

				if (albumCache != null)
				{
					albumCache.Remove(album.Id);
				}
			}
		}
	}
}
