using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.Security;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Web.Controller
{
	/// <summary>
	/// Contains functionality for managing roles.
	/// </summary>
	public static class RoleController
	{
		#region Private Fields

		private static RoleProvider _roleProvider;
		private static readonly object _sharedLock = new object();

		// RegEx pattern to match "_{PortalId}" portion of GSP role name. Not used in stand-alone version of GSP.
		//private static readonly System.Text.RegularExpressions.Regex _gspRoleNameSuffixRegEx = new System.Text.RegularExpressions.Regex(@"_\d+$", System.Text.RegularExpressions.RegexOptions.Compiled);

		// RegEx pattern to match the album owner role template name. The gallery ID is assigned the group name "galleryId".
		// Ex: Given "_Album Owner Template (Gallery ID 723: My gallery)", match will be a success and group name "galleryId" will contain "723"
		private static readonly string _gspAlbumOwnerTemplateRoleNameRegExPattern = String.Concat(GlobalConstants.AlbumOwnerRoleTemplateName, @" \(Gallery ID (?<galleryId>\d+): .*\)$");
		private static readonly System.Text.RegularExpressions.Regex _gspAlbumOwnerTemplateRoleNameRegEx = new System.Text.RegularExpressions.Regex(_gspAlbumOwnerTemplateRoleNameRegExPattern, System.Text.RegularExpressions.RegexOptions.Compiled);

		#endregion

		#region Properties

		/// <summary>
		/// Gets the role provider used by Gallery Server Pro.
		/// </summary>
		/// <value>The role provider used by Gallery Server Pro.</value>
		internal static RoleProvider RoleGsp
		{
			get
			{
				if (_roleProvider == null)
				{
					_roleProvider = GetRoleProvider();
				}

				return _roleProvider;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Persist the <paramref name="roleToSave" /> to the data store, associating any album IDs listed in <paramref name="topLevelCheckedAlbumIds" />
		/// with it. Prior to saving, validation is performed and a <see cref="GallerySecurityException" /> is thrown if a business rule
		/// would be violated.
		/// </summary>
		/// <param name="roleToSave">The role to save.</param>
		/// <param name="topLevelCheckedAlbumIds">The top level album IDs. May be null.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="roleToSave" /> is null.</exception>
		/// <exception cref="GallerySecurityException">Thrown when the role cannot be saved because doing so would violate a business rule.</exception>
		/// <exception cref="InvalidGalleryServerRoleException">Thrown when an existing role cannot be found in the database that matches the 
		/// role name of the <paramref name="roleToSave" /> parameter.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="roleToSave" /> is null.</exception>
		public static void Save(IGalleryServerRole roleToSave, IIntegerCollection topLevelCheckedAlbumIds)
		{
			if (roleToSave == null)
				throw new ArgumentNullException("roleToSave");

			ValidateSaveRole(roleToSave);

			UpdateRoleAlbumRelationships(roleToSave, topLevelCheckedAlbumIds);

			roleToSave.Save();
		}

		/// <summary>
		/// Add the specified user to the specified role.
		/// </summary>
		/// <param name="userName">The user name to add to the specified role.</param>
		/// <param name="roleName">The role to add the specified user name to.</param>
		public static void AddUserToRole(string userName, string roleName)
		{
			if (!String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(roleName))
			{
				AddUserToRoles(userName, new string[] { roleName.Trim() });
			}
		}

		/// <summary>
		/// Add the specified user to the specified roles.
		/// </summary>
		/// <param name="userName">The user name to add to the specified role.</param>
		/// <param name="roleNames">The roles to add the specified user name to.</param>
		public static void AddUserToRoles(string userName, string[] roleNames)
		{
			if (!String.IsNullOrEmpty(userName) && (roleNames != null) && (roleNames.Length > 0))
			{
				RoleGsp.AddUsersToRoles(new string[] { userName.Trim() }, roleNames);
			}
		}

		/// <summary>
		/// Removes the specified user from the specified role.
		/// </summary>
		/// <param name="userName">The user to remove from the specified role.</param>
		/// <param name="roleName">The role to remove the specified user from.</param>
		public static void RemoveUserFromRole(string userName, string roleName)
		{
			if (!String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(roleName))
			{
				RemoveUserFromRoles(userName, new string[] { roleName.Trim() });
			}
		}

		/// <summary>
		/// Removes the specified user from the specified roles.
		/// </summary>
		/// <param name="userName">The user to remove from the specified role.</param>
		/// <param name="roleNames">The roles to remove the specified user from.</param>
		public static void RemoveUserFromRoles(string userName, string[] roleNames)
		{
			if (!String.IsNullOrEmpty(userName) && (roleNames != null) && (roleNames.Length > 0))
			{
				RoleGsp.RemoveUsersFromRoles(new string[] { userName.Trim() }, roleNames);
			}

			ValidateRemoveUserFromRole(userName, roleNames);
		}

		/// <summary>
		/// Gets a list of all the ASP.NET roles for the current application.
		/// </summary>
		/// <returns>A list of all the ASP.NET roles for the current application.</returns>
		public static string[] GetAllRoles()
		{
			return RoleGsp.GetAllRoles();
		}

		/// <summary>
		/// Gets a list of the roles that a specified user is in for the current application.
		/// </summary>
		/// <param name="userName">The user name.</param>
		/// <returns>A list of the roles that a specified user is in for the current application.</returns>
		public static string[] GetRolesForUser(string userName)
		{
			if (String.IsNullOrEmpty(userName))
				return new string[] { };

			return RoleGsp.GetRolesForUser(userName.Trim());
		}

		/// <summary>
		/// Gets a list of users in the specified role for the current application.
		/// </summary>
		/// <param name="roleName">The name of the role.</param>
		/// <returns>A list of users in the specified role for the current application.</returns>
		public static string[] GetUsersInRole(string roleName)
		{
			if (String.IsNullOrEmpty(roleName))
				return new string[] { };

			return RoleGsp.GetUsersInRole(roleName.Trim());
		}

		/// <summary>
		/// Adds a role to the data source for the current application. If the role already exists, no action is taken.
		/// </summary>
		/// <param name="roleName">Name of the role. Any leading or trailing spaces are removed.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="roleName" /> is null.</exception>
		public static void CreateRole(string roleName)
		{
			if (String.IsNullOrEmpty(roleName))
				throw new ArgumentNullException("roleName");

			lock (_sharedLock)
			{
				if (!RoleExists(roleName))
				{
					RoleGsp.CreateRole(roleName.Trim());
				}
			}
		}

		/// <summary>
		/// Removes a role from the data source for the current application.
		/// </summary>
		/// <param name="roleName">Name of the role.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="roleName" /> is null.</exception>
		private static void DeleteRole(string roleName)
		{
			if (String.IsNullOrEmpty(roleName))
				throw new ArgumentNullException("roleName");

			RoleGsp.DeleteRole(roleName.Trim(), false);
		}

		/// <summary>
		/// Gets a value indicating whether the specified role name already exists in the data source for the current application.
		/// </summary>
		/// <param name="roleName">Name of the role.</param>
		/// <returns><c>true</c> if the role exists; otherwise <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="roleName" /> is null.</exception>
		public static bool RoleExists(string roleName)
		{
			if (String.IsNullOrEmpty(roleName))
				throw new ArgumentNullException("roleName");

			return RoleGsp.RoleExists(roleName.Trim());
		}

		/// <summary>
		/// Gets a value indicating whether the specified user is in the specified role for the current application.
		/// </summary>
		/// <param name="userName">The user name to search for.</param>
		/// <param name="roleName">The role to search in.</param>
		/// <returns>
		/// 	<c>true</c> if the specified user is in the specified role for the configured applicationName; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userName" /> or <paramref name="roleName" /> is null
		/// or an empty string.</exception>
		public static bool IsUserInRole(string userName, string roleName)
		{
			if (String.IsNullOrEmpty(userName))
				throw new ArgumentOutOfRangeException("userName", "The parameter 'userName' cannot be null or an empty string.");

			if (String.IsNullOrEmpty(roleName))
				throw new ArgumentOutOfRangeException("roleName", "The parameter 'roleName' cannot be null or an empty string.");

			return RoleGsp.IsUserInRole(userName.Trim(), roleName.Trim());
		}

		/// <overloads>Retrieve a collection of Gallery Server roles.  The roles may be returned from a cache.</overloads>
		/// <summary>
		/// Retrieve all roles. Album owner roles are included in the collection. This is the same as calling 
		/// GetGalleryServerRoles with the includeOwnerRoles parameter set to <c>true</c>.  The roles may be  returned from a cache.
		/// </summary>
		/// <returns>Returns all roles.</returns>
		[DataObjectMethod(DataObjectMethodType.Select)]
		public static IGalleryServerRoleCollection GetGalleryServerRoles()
		{
			return GetGalleryServerRoles(true);
		}

		/// <summary>
		/// Retrieve Gallery Server roles, optionally excluding roles that were programmatically
		/// created to assist with the album ownership and user album functions. Excluding the owner roles may be useful
		/// in reducing the clutter when an administrator is viewing the list of roles, as it hides those not specifically created
		/// by the administrator. The roles may be returned from a cache.
		/// </summary>
		/// <param name="includeOwnerRoles">If set to <c>true</c> include all roles that serve as an album owner role.
		/// When <c>false</c>, exclude owner roles from the result set.</param>
		/// <returns>
		/// Returns the Gallery Server roles, optionally excluding owner roles.
		/// </returns>
		[DataObjectMethod(DataObjectMethodType.Select)]
		public static IGalleryServerRoleCollection GetGalleryServerRoles(bool includeOwnerRoles)
		{
			if (includeOwnerRoles)
			{
				return Factory.LoadGalleryServerRoles();
			}
			else
			{
				IGalleryServerRoleCollection roles = new GalleryServerRoleCollection();

				foreach (IGalleryServerRole role in Factory.LoadGalleryServerRoles())
				{
					if (!IsRoleAnAlbumOwnerRole(role.RoleName))
					{
						roles.Add(role);
					}
				}

				return roles;
			}
		}

		/// <overloads>
		/// Gets a collection of Gallery Server roles.
		/// </overloads>
		/// <summary>
		/// Gets Gallery Server roles representing the roles for the currently logged-on user. Returns an empty collection if 
		/// no user is logged in or the user is logged in but not assigned to any roles (Count = 0).
		/// The roles may be returned from a cache. Guaranteed to not return null.
		/// </summary>
		/// <returns>
		/// Returns an <see cref="IGalleryServerRoleCollection" /> representing the roles for the currently logged-on user.
		/// </returns>
		public static IGalleryServerRoleCollection GetGalleryServerRolesForUser()
		{
			return GetGalleryServerRolesForUser(Utils.UserName);
		}

		/// <summary>
		/// Gets Gallery Server roles representing the roles for the specified <paramref name="userName"/>. Returns an empty collection if 
		/// the user is not assigned to any roles (Count = 0). The roles may be returned from a cache. Guaranteed to not return null.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns>
		/// Returns an <see cref="IGalleryServerRoleCollection"/> representing the roles for the specified <paramref name="userName" />.
		/// </returns>
		public static IGalleryServerRoleCollection GetGalleryServerRolesForUser(string userName)
		{
			if (!Utils.IsAuthenticated)
				return new GalleryServerRoleCollection();

			if (String.IsNullOrEmpty(userName))
				return new GalleryServerRoleCollection();

			// Get cached dictionary entry matching logged on user. If not found, retrieve from business layer and add to cache.
			Dictionary<string, IGalleryServerRoleCollection> rolesCache = (Dictionary<string, IGalleryServerRoleCollection>)HelperFunctions.GetCache(CacheItem.GalleryServerRoles);

			IGalleryServerRoleCollection roles = null;

			if (HttpContext.Current.Session != null)
			{
				if ((rolesCache != null) && (rolesCache.TryGetValue(GetCacheKeyNameForRoles(userName), out roles)))
				{
					return roles;
				}
			}

			// No roles in the cache, so get from business layer and add to cache.
			try
			{
				roles = Factory.LoadGalleryServerRoles(GetRolesForUser(userName));
			}
			catch (InvalidGalleryServerRoleException)
			{
				// We could not find one or more GSP roles for the ASP.NET roles we passed to Factory.LoadGalleryServerRoles(). Things probably
				// got out of synch. For example, this can happen if an admin adds an ASP.NET role outside of GSP (such as when using the 
				// DNN control panel). Purge the cache, then run the validation routine, and try again. If the same exception is thrown again,
				// let it bubble up - there isn't anything more we can do.
				HelperFunctions.PurgeCache();

				ValidateRoles();

				roles = Factory.LoadGalleryServerRoles(GetRolesForUser(userName));
			}

			if (rolesCache == null)
			{
				// The factory method should have created a cache item, so try again.
				rolesCache = (Dictionary<string, IGalleryServerRoleCollection>)HelperFunctions.GetCache(CacheItem.GalleryServerRoles);
				if (rolesCache == null)
				{
					if (AppSetting.Instance.EnableCache)
					{
						AppErrorController.LogError(new WebException("The method Factory.LoadGalleryServerRoles() should have created a cache entry, but none was found. This is not an issue if it occurs occasionally, but should be addressed if it is frequent."));
					}

					return roles;
				}
			}

			// Add to the cache, but only if we have access to the session ID.
			if (HttpContext.Current.Session != null)
			{
				lock (rolesCache)
				{
					if (!rolesCache.ContainsKey(GetCacheKeyNameForRoles(userName)))
					{
						rolesCache.Add(GetCacheKeyNameForRoles(userName), roles);
					}
				}
				HelperFunctions.SetCache(CacheItem.GalleryServerRoles, rolesCache);
			}

			return roles;
		}

		/// <summary>
		/// Gets all the gallery server roles that apply to the specified <paramref name="gallery" />.
		/// </summary>
		/// <param name="gallery">The gallery.</param>
		/// <returns>Returns an <see cref="IGalleryServerRoleCollection"/> representing the roles that apply to the specified 
		/// <paramref name="gallery" />.</returns>
		public static IGalleryServerRoleCollection GetGalleryServerRolesForGallery(IGallery gallery)
		{
			IGalleryServerRoleCollection roles = new GalleryServerRoleCollection();

			foreach (IGalleryServerRole role in GetGalleryServerRoles())
			{
				if (role.Galleries.Contains(gallery) && (!roles.Contains(role)))
				{
					roles.Add(role);
				}
			}

			return roles;
		}

		/// <summary>
		/// Gets the list of roles the user has permission to view. Users who have administer site permission can view all roles.
		/// Users with administer gallery permission can only view roles they have been associated with or roles that aren't 
		/// associated with *any* gallery, unless the application setting <see cref="IAppSetting.AllowGalleryAdminToViewAllUsersAndRoles" />
		/// is true, in which case they can see all roles.
		/// </summary>
		/// <param name="userIsSiteAdmin">If set to <c>true</c>, the currently logged on user is a site administrator.</param>
		/// <param name="userIsGalleryAdmin">If set to <c>true</c>, the currently logged on user is a gallery administrator for the current gallery.</param>
		/// <returns>Returns an <see cref="IGalleryServerRoleCollection" /> containing a list of roles the user has permission to view.</returns>
		public static IGalleryServerRoleCollection GetRolesCurrentUserCanView(bool userIsSiteAdmin, bool userIsGalleryAdmin)
		{
			if (userIsSiteAdmin || (userIsGalleryAdmin && AppSetting.Instance.AllowGalleryAdminToViewAllUsersAndRoles))
			{
				return RoleController.GetGalleryServerRoles();
			}
			else if (userIsGalleryAdmin)
			{
				IGalleryServerRoleCollection roles = RoleController.GetGalleryServerRoles();
				IGalleryServerRoleCollection filteredRoles = new GalleryServerRoleCollection();

				// Build up a list of roles where (1) the current user is a gallery admin for at least one gallery, 
				// (2) the role is an album owner template role and the current user is a gallery admin for its associated gallery, or
				// (3) the role isn't associated with any albums/galleries.
				foreach (IGalleryServerRole role in roles)
				{
					if (role.Galleries.Count > 0)
					{
						if (IsUserGalleryAdminForRole(role))
						{
							// Current user has gallery admin permissions for at least one galley associated with the role.
							filteredRoles.Add(role);
						}
					}
					else if (IsRoleAnAlbumOwnerTemplateRole(role.RoleName))
					{
						if (IsUserGalleryAdminForAlbumOwnerTemplateRole(role))
						{
							// The role is an album owner template role and the current user is a gallery admin for it's associated gallery.
							filteredRoles.Add(role);
						}
					}
					else
					{
						// Role isn't an album owner role and it isn't assigned to any albums. Add it.
						filteredRoles.Add(role);
					}
				}

				return filteredRoles;
			}
			else
			{
				return new GalleryServerRoleCollection();
			}
		}

		/// <summary>
		/// Create a Gallery Server Pro role corresponding to the specified parameters. Also creates the corresponding ASP.NET role.
		/// Throws an exception if a role with the specified name already exists in the data store. The role is persisted to the data store.
		/// </summary>
		/// <param name="roleName">A string that uniquely identifies the role.</param>
		/// <param name="allowViewAlbumOrMediaObject">A value indicating whether the user assigned to this role has permission to view albums
		/// and media objects.</param>
		/// <param name="allowViewOriginalImage">A value indicating whether the user assigned to this role has permission to view the original,
		/// high resolution version of an image. This setting applies only to images. It has no effect if there are no
		/// high resolution images in the album or albums to which this role applies.</param>
		/// <param name="allowAddMediaObject">A value indicating whether the user assigned to this role has permission to add media objects to an album.</param>
		/// <param name="allowAddChildAlbum">A value indicating whether the user assigned to this role has permission to create child albums.</param>
		/// <param name="allowEditMediaObject">A value indicating whether the user assigned to this role has permission to edit a media object.</param>
		/// <param name="allowEditAlbum">A value indicating whether the user assigned to this role has permission to edit an album.</param>
		/// <param name="allowDeleteMediaObject">A value indicating whether the user assigned to this role has permission to delete media objects within an album.</param>
		/// <param name="allowDeleteChildAlbum">A value indicating whether the user assigned to this role has permission to delete child albums.</param>
		/// <param name="allowSynchronize">A value indicating whether the user assigned to this role has permission to synchronize an album.</param>
		/// <param name="allowAdministerSite">A value indicating whether the user has administrative permission for all albums. This permission
		/// automatically applies to all albums across all galleries; it cannot be selectively applied.</param>
		/// <param name="allowAdministerGallery">A value indicating whether the user has administrative permission for all albums. This permission
		/// automatically applies to all albums in a particular gallery; it cannot be selectively applied.</param>
		/// <param name="hideWatermark">A value indicating whether the user assigned to this role has a watermark applied to images.
		/// This setting has no effect if watermarks are not used. A true value means the user does not see the watermark;
		/// a false value means the watermark is applied.</param>
		/// <param name="topLevelCheckedAlbumIds">The top level checked album ids. May be null.</param>
		/// <returns>
		/// Returns an <see cref="IGalleryServerRole"/> object corresponding to the specified parameters.
		/// </returns>
		/// <exception cref="InvalidGalleryServerRoleException">Thrown when a role with the specified role name already exists in the data store.</exception>
		public static IGalleryServerRole CreateRole(string roleName, bool allowViewAlbumOrMediaObject, bool allowViewOriginalImage, bool allowAddMediaObject, bool allowAddChildAlbum, bool allowEditMediaObject, bool allowEditAlbum, bool allowDeleteMediaObject, bool allowDeleteChildAlbum, bool allowSynchronize, bool allowAdministerSite, bool allowAdministerGallery, bool hideWatermark, IIntegerCollection topLevelCheckedAlbumIds)
		{
			lock (_sharedLock)
			{
				// Create the ASP.NET role.
				CreateRole(roleName);

				// Create the Gallery Server Pro role that extends the functionality of the ASP.NET role.
				IGalleryServerRole role = Factory.CreateGalleryServerRoleInstance(MakeRoleNameUnique(roleName), allowViewAlbumOrMediaObject, allowViewOriginalImage, allowAddMediaObject, allowAddChildAlbum, allowEditMediaObject, allowEditAlbum, allowDeleteMediaObject, allowDeleteChildAlbum, allowSynchronize, allowAdministerSite, allowAdministerGallery, hideWatermark);

				UpdateRoleAlbumRelationships(role, topLevelCheckedAlbumIds);

				role.Save();

				return role;
			}
		}

		/// <summary>
		/// Delete the specified role. Both components of the role are deleted: the IGalleryServerRole and ASP.NET role.
		/// </summary>
		/// <param name="roleName">Name of the role. Must match an existing <see cref="IGalleryServerRole.RoleName"/>. If no match
		/// if found, no action is taken.</param>
		/// <exception cref="GallerySecurityException">Thrown when the role cannot be deleted because doing so violates one of the business rules.</exception>
		public static void DeleteGalleryServerProRole(string roleName)
		{
			ValidateDeleteRole(roleName);

			try
			{
				DeleteGalleryServerRole(roleName);
			}
			finally
			{
				try
				{
					DeleteAspnetRole(roleName);
				}
				finally
				{
					HelperFunctions.PurgeCache();
				}
			}
		}

		/// <summary>
		/// Make sure the list of ASP.NET roles is synchronized with the Gallery Server roles. If any are missing from 
		/// either, add it.
		/// </summary>
		public static void ValidateRoles()
		{
			List<IGalleryServerRole> validatedRoles = new List<IGalleryServerRole>();
			IGalleryServerRoleCollection galleryRoles = Factory.LoadGalleryServerRoles();
			bool needToPurgeCache = false;

			foreach (string roleName in GetAllRoles())
			{
				IGalleryServerRole galleryRole = galleryRoles.GetRole(roleName);
				if (galleryRole == null)
				{
					// This is an ASP.NET role that doesn't exist in our list of gallery server roles. Add it with minimum permissions
					// applied to zero albums.
					IGalleryServerRole newRole = Factory.CreateGalleryServerRoleInstance(roleName, false, false, false, false, false, false, false, false, false, false, false, false);
					newRole.Save();
					needToPurgeCache = true;
				}
				validatedRoles.Add(galleryRole);
			}

			// Now check to see if there are gallery roles that are not ASP.NET roles. Add if necessary.
			foreach (IGalleryServerRole galleryRole in galleryRoles)
			{
				if (!validatedRoles.Contains(galleryRole))
				{
					// Need to create an ASP.NET role for this gallery role.
					CreateRole(galleryRole.RoleName);
					needToPurgeCache = true;
				}
			}

			if (needToPurgeCache)
			{
				HelperFunctions.PurgeCache();
			}
		}

		/// <summary>
		/// Verify that any role needed for album ownership exists and is properly configured. If an album owner
		/// is specified and the album is new (IsNew == true), the album is persisted to the data store. This is 
		/// required because the ID is not assigned until it is saved, and a valid ID is required to configure the
		/// role.
		/// </summary>
		/// <param name="album">The album to validate for album ownership. If a null value is passed, the function
		/// returns without error or taking any action.</param>
		public static void ValidateRoleExistsForAlbumOwner(IAlbum album)
		{
			// For albums, verify that any needed roles for album ownership are present. Create/update as needed.
			if (album == null)
				return;

			if (String.IsNullOrEmpty(album.OwnerUserName))
			{
				// If owner role is specified, delete it.
				if (!String.IsNullOrEmpty(album.OwnerRoleName))
				{
					DeleteGalleryServerProRole(album.OwnerRoleName);
					album.OwnerRoleName = String.Empty;
				}
			}
			else
			{
				// If this is a new album, save it before proceeding. We will need its album ID to configure the role, 
				// and it is not assigned until it is saved.
				if (album.IsNew)
					album.Save();

				// Verify that a role exists that corresponds to the owner.
				IGalleryServerRole role = Factory.LoadGalleryServerRoles().GetRole(album.OwnerRoleName);
				if (role == null)
				{
					// No role exists. Create it.
					album.OwnerRoleName = CreateAlbumOwnerRole(album);
				}
				else
				{
					// Role exists. Make sure album is assigned to role and owner is a member.
					if (!role.RootAlbumIds.Contains(album.Id))
					{
						// Current album is not a member. This should not typically occur, but just in case
						// it does let's add the current album to it and save it.
						role.RootAlbumIds.Add(album.Id);
						role.Save();
					}

					string[] rolesForUser = GetRolesForUser(album.OwnerUserName);
					if (Array.IndexOf(rolesForUser, role.RoleName) < 0)
					{
						// Owner is not a member. Add.
						AddUserToRole(album.OwnerUserName, role.RoleName);
					}
				}
			}
		}

		/// <summary>
		/// Determines whether the <paramref name="roleName"/> is a role that serves as an album owner role. Returns <c>true</c> if the
		/// <paramref name="roleName"/> starts with the same string as the global constant <see cref="GlobalConstants.AlbumOwnerRoleNamePrefix"/>.
		/// Album owner roles are roles that are programmatically created to provide the security context used for the album ownership
		/// and user album features.
		/// </summary>
		/// <param name="roleName">Name of the role.</param>
		/// <returns>
		/// 	<c>true</c> if <paramref name="roleName"/> is a role that serves as an album owner role; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsRoleAnAlbumOwnerRole(string roleName)
		{
			if (String.IsNullOrEmpty(roleName))
				return false;

			return roleName.Trim().StartsWith(GlobalConstants.AlbumOwnerRoleNamePrefix, StringComparison.Ordinal);
		}

		/// <summary>
		/// Determines whether the <paramref name="roleName"/> is a role that serves as an album owner template role. Returns <c>true</c> if the
		/// <paramref name="roleName"/> matches a regular expression that defines the pattern for the template role name.
		/// Album owner roles are created from the album owner template role.
		/// </summary>
		/// <param name="roleName">Name of the role.</param>
		/// <returns>
		/// 	<c>true</c> if <paramref name="roleName"/> is a role that serves as an album owner template role; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsRoleAnAlbumOwnerTemplateRole(string roleName)
		{
			return _gspAlbumOwnerTemplateRoleNameRegEx.Match(roleName).Success;
		}

		/// <summary>
		/// Removes the roles belonging to the current user from cache. This cache item has a unique name based on the session ID and logged-on 
		/// user's name. This function is not critical for security or correctness, but is useful in keeping the cache cleared of unused items. When
		/// a user logs on or off, their username changes - and therefore the name of the cache item changes, which causes the next call to 
		/// retrieve the user's roles to return nothing from the cache, which forces a retrieval from the database. Thus the correct roles will
		/// always be retrieved, even if this function is not invoked during a logon/logoff event.
		/// </summary>
		public static void RemoveRolesFromCache()
		{
			Dictionary<string, IGalleryServerRoleCollection> rolesCache = (Dictionary<string, IGalleryServerRoleCollection>)HelperFunctions.GetCache(CacheItem.GalleryServerRoles);

			if ((rolesCache != null) && (HttpContext.Current.Session != null))
			{
				rolesCache.Remove(GetCacheKeyNameForRoles(Utils.UserName));
			}
		}

		/// <overloads>
		/// Modify the name of the role to ensure it is unique to the portal (applies only to DotNetNuke versions).
		/// </overloads>
		/// <summary>
		/// This function returns the <paramref name="roleName" /> parameter without modification. It serves as a placeholder function
		/// for the DotNetNuke implementation.
		/// </summary>
		/// <param name="roleName">Name of the role.</param>
		/// <returns>Returns the <paramref name="roleName" /> with the portal ID appended to it.</returns>
		public static string MakeRoleNameUnique(string roleName)
		{
			return roleName;
		}

		/// <summary>
		/// This function returns the <paramref name="roleName" /> parameter without modification. It serves as a placeholder function
		/// for the DotNetNuke implementation.
		/// </summary>
		/// <param name="roleName">Name of the role.</param>
		/// <param name="portalId">The portal ID. Specify <see cref="int.MinValue" /> for non-DotNetNuke versions of this code.</param>
		/// <returns>Returns the <paramref name="roleName" /> without modification.</returns>
		public static string MakeRoleNameUnique(string roleName, int portalId)
		{
			return roleName;
		}

		/// <summary>
		/// Parses the name of the role from the <paramref name="roleNames" />. Example: If role name = "Administrators_0", return
		/// "Administrators". This function works by using a regular expression to remove all text that matches the "_{GalleryID}"
		/// pattern. If the role name does not have this suffix, the original role name is returned. This function is useful when
		/// GSP is used in an application where the role provider allows multiple roles with the same name, such as DotNetNuke.
		/// The contents of this function is commented out in the trunk (stand-alone) version of GSP and enabled in branched versions
		/// where required (such as DotNetNuke).
		/// </summary>
		/// <param name="roleNames">Name of the roles.</param>
		/// <returns>Returns a copy of the <paramref name="roleNames" /> parameter with the "_{GalleryID}" portion removed from each 
		/// role name.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="" /> is null.</exception>
		public static string[] ParseRoleNameFromGspRoleNames(string[] roleNames)
		{
			if (roleNames == null)
				throw new ArgumentNullException("roleNames");

			string[] roleNamesCopy = new string[roleNames.Length];

			for (int i = 0; i < roleNames.Length; i++)
			{
				roleNamesCopy[i] = ParseRoleNameFromGspRoleName(roleNames[i]);
			}

			return roleNamesCopy;
		}

		/// <summary>
		/// Parses the name of the role from the <paramref name="roleName" />. Example: If role name = "Administrators_0", return
		/// "Administrators". This function works by using a regular expression to remove all text that matches the "_{GalleryID}"
		/// pattern. If the role name does not have this suffix, the original role name is returned. This function is useful when
		/// GSP is used in an application where the role provider allows multiple roles with the same name, such as DotNetNuke.
		/// The contents of this function is commented out in the trunk (stand-alone) version of GSP and enabled in branched versions
		/// where required (such as DotNetNuke).
		/// </summary>
		/// <param name="roleName">Name of the role. Example: "Administrators_0"</param>
		/// <returns>Returns the role name with the "_{GalleryID}" portion removed.</returns>
		public static string ParseRoleNameFromGspRoleName(string roleName)
		{
			return roleName;
			//return _gspRoleNameSuffixRegEx.Replace(roleName, String.Empty); // DotNetNuke only
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Make sure the loggod-on person has authority to save the role and that h/she isn't doing anything stupid, like removing
		/// Administer site permission from the only role that has it.
		/// </summary>
		/// <param name="roleToSave">The role to be saved.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="roleToSave"/> is null.</exception>
		/// <exception cref="GallerySecurityException">Thrown when the role cannot be saved because doing so would violate a business rule.</exception>
		/// <exception cref="InvalidGalleryServerRoleException">Thrown when an existing role cannot be found in the database that matches the 
		/// role name of the <paramref name="roleToSave" /> parameter.</exception>
		private static void ValidateSaveRole(IGalleryServerRole roleToSave)
		{
			#region Parameter Validation

			if (roleToSave == null)
				throw new ArgumentNullException("roleToSave");

			if (String.IsNullOrEmpty(roleToSave.RoleName))
				return; // Role name will be empty when adding a new one, so the validation below doesn't apply.

			IGalleryServerRole existingRole = Factory.LoadGalleryServerRole(roleToSave.RoleName);

			if (existingRole == null)
				throw new InvalidGalleryServerRoleException();

			#endregion

			ValidateCanRemoveSiteAdminPermission(roleToSave, existingRole);

			ValidateGalleryAdminCanSaveRole(roleToSave, existingRole);

			ValidateUserDoesNotLoseAbilityToAdminCurrentGallery(roleToSave, existingRole);
		}

		/// <summary>
		/// If administer site permission is being removed from the <paramref name="roleToSave" />, verify that this action does not violate
		/// business rules. Specifically, ensure that at least one other role has the same permission to prevent the user from removing their
		/// ability to administer the site. Throws a <see cref="GallerySecurityException" /> if the role should not be saved.
		/// </summary>
		/// <param name="roleToSave">The role to save. It's role name must match the role name of <paramref name="existingRole" />.</param>
		/// <param name="existingRole">The existing role, as it is stored in the database. It's role name must match the role name of
		/// <paramref name="roleToSave" />.</param>
		/// <exception cref="GallerySecurityException">Thrown when the role cannot be saved because doing so would violate a business rule.</exception>
		private static void ValidateCanRemoveSiteAdminPermission(IGalleryServerRole roleToSave, IGalleryServerRole existingRole)
		{
			if (!roleToSave.RoleName.Equals(existingRole.RoleName, StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "The role name of the roleToSave and existingRole parameters must match, but they do not. roleToSave='{0}'; existingRole='{1}'", roleToSave, existingRole));
			}

			if (existingRole.AllowAdministerSite && !roleToSave.AllowAdministerSite)
			{
				// User is trying to remove administer site permission from this role. Make sure
				// at least one other role has this permission, and that the role has at least one member.
				bool atLeastOneOtherRoleHasAdminSitePermission = false;
				foreach (IGalleryServerRole role in GetGalleryServerRoles())
				{
					if ((!role.RoleName.Equals(existingRole.RoleName, StringComparison.OrdinalIgnoreCase) && role.AllowAdministerSite))
					{
						if (GetUsersInRole(role.RoleName).Length > 0)
						{
							atLeastOneOtherRoleHasAdminSitePermission = true;
							break;
						}
					}
				}

				if (!atLeastOneOtherRoleHasAdminSitePermission)
				{
					throw new GallerySecurityException(Resources.GalleryServerPro.Admin_Manage_Roles_Cannot_Remove_Admin_Perm_Msg);
				}
			}
		}

		/// <summary>
		/// When the current user is a gallery administrator but not a site administrator, verify the user can save the role.
		/// Specifically, the user is not allowed to add administer site permission or save any gallery she is not a gallery
		/// administrator for. It is up to the caller to verify that only site or gallery administrators call this function!
		/// </summary>
		/// <param name="roleToSave">The role to save. It's role name must match the role name of <paramref name="existingRole" />.</param>
		/// <param name="existingRole">The existing role, as it is stored in the database. It's role name must match the role name of
		/// <paramref name="roleToSave" />.</param>
		/// <exception cref="GallerySecurityException">Thrown when the role cannot be saved because doing so would violate a business rule.</exception>
		private static void ValidateGalleryAdminCanSaveRole(IGalleryServerRole roleToSave, IGalleryServerRole existingRole)
		{
			if (!roleToSave.RoleName.Equals(existingRole.RoleName, StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "The role name of the roleToSave and existingRole parameters must match, but they do not. roleToSave='{0}'; existingRole='{1}'", roleToSave, existingRole));
			}

			IGalleryServerRoleCollection roles = GetGalleryServerRolesForUser();

			if (!Utils.IsUserSiteAdministrator(roles))
			{
				// User is a gallery admin but not a site admin (we deduce this because ONLY site or gallery admins can call this function,
				// so if the user is not a site admin, she must be a gallery admin). The user CANNOT save this role if any of the following is true:
				// 1. The user is attempting to add AllowAdminSite permission.
				// 2. The role applies to a gallery the user is not an admin for.
				if (roleToSave.AllowAdministerSite)
				{
					throw new GallerySecurityException(Resources.GalleryServerPro.Admin_Manage_Roles_Cannot_Delete_Role_Insufficient_Permission_Msg);
				}

				foreach (IGallery gallery in existingRole.Galleries)
				{
					if (!Utils.IsUserGalleryAdministrator(roles, gallery.GalleryId))
					{
						throw new GallerySecurityException(Resources.GalleryServerPro.Admin_Manage_Roles_Cannot_Delete_Role_Insufficient_Permission_Msg2);
					}
				}
			}
		}

		/// <summary>
		/// Verify the the current user isn't jeopardizing their ability to administer the site or current gallery. Specifically, if
		/// the user is a member of the role being saved and admin site or gallery permissions are being removed from it, make sure
		/// the user is in at least one other role with similar permissions. Verifies only the current gallery: That is, it is possible
		/// for the user to remove their ability to administer another gallery.
		/// </summary>
		/// <param name="roleToSave">The role to save. It's role name must match the role name of <paramref name="existingRole" />.</param>
		/// <param name="existingRole">The existing role, as it is stored in the database. It's role name must match the role name of
		/// <paramref name="roleToSave" />.</param>
		/// <exception cref="GallerySecurityException">Thrown when the role cannot be saved because doing so would violate a business rule.</exception>
		private static void ValidateUserDoesNotLoseAbilityToAdminCurrentGallery(IGalleryServerRole roleToSave, IGalleryServerRole existingRole)
		{
			if (!roleToSave.RoleName.Equals(existingRole.RoleName, StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "The role name of the roleToSave and existingRole parameters must match, but they do not. roleToSave='{0}'; existingRole='{1}'", roleToSave, existingRole));
			}

			if (IsUserInRole(Utils.UserName, roleToSave.RoleName))
			{
				bool adminSitePermissionBeingRevoked = (!roleToSave.AllowAdministerSite && existingRole.AllowAdministerSite);
				bool adminGalleryPermissionBeingRevoked = (!roleToSave.AllowAdministerGallery && existingRole.AllowAdministerGallery);

				bool userHasAdminSitePermissionThroughAtLeastOneOtherRole = false;
				bool userHasAdminGalleryPermissionThroughAtLeastOneOtherRole = false;

				foreach (IGalleryServerRole roleForUser in GetGalleryServerRolesForUser())
				{
					if (!roleForUser.RoleName.Equals(roleToSave.RoleName))
					{
						if (roleForUser.AllowAdministerSite)
						{
							userHasAdminSitePermissionThroughAtLeastOneOtherRole = true;
						}
						if (roleForUser.AllowAdministerGallery)
						{
							userHasAdminGalleryPermissionThroughAtLeastOneOtherRole = true;
						}
					}
				}

				if (adminSitePermissionBeingRevoked && !userHasAdminSitePermissionThroughAtLeastOneOtherRole)
				{
					throw new GallerySecurityException(Resources.GalleryServerPro.Admin_Manage_Roles_Cannot_Save_Role_User_Would_Lose_Admin_Ability_Msg);
				}

				if (adminGalleryPermissionBeingRevoked && !userHasAdminGalleryPermissionThroughAtLeastOneOtherRole)
				{
					throw new GallerySecurityException(Resources.GalleryServerPro.Admin_Manage_Roles_Cannot_Save_Role_User_Would_Lose_Admin_Ability_Msg);
				}
			}
		}

		/// <summary>
		/// Gets the Role provider used by Gallery Server Pro.
		/// </summary>
		/// <returns>The Role provider used by Gallery Server Pro.</returns>
		private static RoleProvider GetRoleProvider()
		{
			if (String.IsNullOrEmpty(AppSetting.Instance.RoleProviderName))
			{
				return Roles.Provider;
			}
			else
			{
				return Roles.Providers[AppSetting.Instance.RoleProviderName];
			}
		}

		/// <summary>
		/// Throws an exception if the role cannot be deleted, such as when deleting the only role with Administer site permission
		/// or deleting a role that would lessen the logged-on users own level of administrative access.
		/// </summary>
		/// <param name="roleName">Name of the role to be deleted.</param>
		/// <exception cref="GallerySecurityException">Thrown when the role cannot be deleted because doing so violates one of the business rules.</exception>
		private static void ValidateDeleteRole(string roleName)
		{
			IGalleryServerRole roleToDelete = Factory.LoadGalleryServerRole(roleName);

			if (roleToDelete == null)
				return;

			// Test 1: Don't let user delete the only role with Administer site permission.
			ValidatePreventLastSysAdminRoleDeletion(roleToDelete);

			// Test 2: Don't let user delete a role with site admin or gallery admin permissions if that means the user will 
			// lose their own administrative access.
			ValidatePreventLoggedOnUserFromLosingAdminAccess(roleToDelete);

			// Test 3: User can delete role only if he is a site admin or a gallery admin in every gallery this role is associated with.
			ValidatePreventRoleDeletionAffectingOtherGalleries(roleToDelete);
		}

		/// <summary>
		/// Don't let user delete the only role with Administer site permission. This should be called before a role is deleted as a validation step.
		/// </summary>
		/// <param name="roleToDelete">The role to be deleted.</param>
		/// <exception cref="GallerySecurityException">Thrown when the role cannot be deleted because doing so violates one of the business rules.</exception>
		private static void ValidatePreventLastSysAdminRoleDeletion(IGalleryServerRole roleToDelete)
		{
			if (roleToDelete.AllowAdministerSite)
			{
				// User is trying to delete a role with administer site permission. Make sure
				// at least one other role has this permission, and that the role has at least one member.
				bool atLeastOneOtherRoleHasAdminSitePermission = false;
				foreach (IGalleryServerRole role in Factory.LoadGalleryServerRoles())
				{
					if ((!role.RoleName.Equals(roleToDelete.RoleName, StringComparison.OrdinalIgnoreCase) && role.AllowAdministerSite))
					{
						if (GetUsersInRole(role.RoleName).Length > 0)
						{
							atLeastOneOtherRoleHasAdminSitePermission = true;
							break;
						}
					}
				}

				if (!atLeastOneOtherRoleHasAdminSitePermission)
				{
					throw new GallerySecurityException(Resources.GalleryServerPro.Admin_Manage_Roles_Cannot_Delete_Role_Msg);
				}
			}
		}

		/// <summary>
		/// Don't let user delete a role with site admin or gallery admin permissions if that means the user will 
		/// lose their own administrative access. This should be called before a role is deleted as a validation step.
		/// </summary>
		/// <param name="roleToDelete">The role to be deleted.</param>
		/// <exception cref="GallerySecurityException">Thrown when the role cannot be deleted because doing so violates one of the business rules.</exception>
		private static void ValidatePreventLoggedOnUserFromLosingAdminAccess(IGalleryServerRole roleToDelete)
		{
			string roleName = roleToDelete.RoleName;

			if (roleToDelete.AllowAdministerSite || roleToDelete.AllowAdministerGallery)
			{
				bool needToVerify = false;
				IGalleryServerRoleCollection roles = GetGalleryServerRolesForUser(Utils.UserName);
				foreach (IGalleryServerRole role in roles)
				{
					if (role.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
					{
						needToVerify = true;
						break;
					}
				}

				if (needToVerify)
				{
					// User is deleting a role he is a member of. Make sure user is in at least one other role with the same type of access.
					bool userIsInAnotherRoleWithAdminAccess = false;
					if (roleToDelete.AllowAdministerSite)
					{
						foreach (IGalleryServerRole role in roles)
						{
							if (role.AllowAdministerSite && (!role.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
							{
								userIsInAnotherRoleWithAdminAccess = true;
								break;
							}
						}
					}
					else if (roleToDelete.AllowAdministerGallery)
					{
						foreach (IGalleryServerRole role in roles)
						{
							if (role.AllowAdministerGallery && (!role.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
							{
								userIsInAnotherRoleWithAdminAccess = true;
								break;
							}
						}
					}

					if (!userIsInAnotherRoleWithAdminAccess)
					{
						throw new GallerySecurityException(Resources.GalleryServerPro.Admin_Cannot_Delete_Role_Remove_Self_Admin_Msg);
					}
				}
			}
		}

		/// <summary>
		/// Don't let user delete a role that affects any gallery where the user is not a site admin or gallery admin. This should be called before 
		/// a role is deleted as a validation step. The only exception is that we allow a user to delete an album owner role, since that will typically
		/// be assigned to a single album, and we have logic elsewhere that verifies the user has permission to delete the album.
		/// </summary>
		/// <param name="roleToDelete">The role to be deleted.</param>
		/// <exception cref="GallerySecurityException">Thrown when the role cannot be deleted because doing so violates one of the business rules.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="roleToDelete" /> is null.</exception>
		private static void ValidatePreventRoleDeletionAffectingOtherGalleries(IGalleryServerRole roleToDelete)
		{
			if (roleToDelete == null)
				throw new ArgumentNullException("roleToDelete");

			if (IsRoleAnAlbumOwnerRole(roleToDelete.RoleName))
			{
				return;
			}

			IGalleryCollection adminGalleries = UserController.GetGalleriesCurrentUserCanAdminister();

			foreach (IGallery gallery in roleToDelete.Galleries)
			{
				if (!adminGalleries.Contains(gallery))
				{
					throw new GallerySecurityException(String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Admin_Cannot_Delete_Role_Insufficient_Permission_Msg, roleToDelete.RoleName, gallery.Description));
				}
			}
		}

		private static void DeleteAspnetRole(string roleName)
		{
			if (String.IsNullOrEmpty(roleName))
				return;

			if (RoleExists(roleName))
				DeleteRole(roleName); // This also deletes any user/role relationships
		}

		private static void DeleteGalleryServerRole(string roleName)
		{
			IGalleryServerRole role = Factory.LoadGalleryServerRole(roleName);

			if (role != null)
			{
				UpdateAlbumOwnerBeforeRoleDelete(role);
				role.Delete();
			}
		}

		/// <summary>
		/// For roles that provide album ownership functionality, remove users belonging to this role from the OwnedBy 
		/// property of any albums this role is assigned to. Since we are deleting the role that provides the ownership
		/// functionality, it is necessary to clear the owner field of all affected albums.
		/// </summary>
		/// <param name="role">Name of the role to be deleted.</param>
		private static void UpdateAlbumOwnerBeforeRoleDelete(IGalleryServerRole role)
		{
			// Proceed only when dealing with an album ownership role.
			if (!IsRoleAnAlbumOwnerRole(role.RoleName))
				return;

			// Loop through each album assigned to this role. If this role is assigned as the owner role,
			// clear the OwnerUserName property.
			foreach (int albumId in role.RootAlbumIds)
			{
				// Load the album and clear the owner role name. Warning: Do not load the album with
				// AlbumController.LoadAlbumInstance(), as it will create a recursive loop, leading to a stack overflow
				IAlbum album = Factory.LoadAlbumInstance(albumId, false, true, false);
				if (album.OwnerRoleName == role.RoleName)
				{
					album.OwnerUserName = String.Empty;
					GalleryObjectController.SaveGalleryObject(album);
				}
			}
		}

		/// <summary>
		/// Creates the album owner role template. This is the role that is used as the template for roles that define
		/// a user's permission level when the user is assigned as an album owner. Call this method when the role does
		/// not exist. It is set up with all permissions except Administer Site and Administer Gallery. The HideWatermark 
		/// permission is not applied, so this role allows its members to view watermarks if that functionality is enabled.
		/// </summary>
		/// <param name="galleryId">The ID of the gallery for which the album owner template role is to belong.</param>
		/// <returns>
		/// Returns an <see cref="IGalleryServerRole"/> that can be used as a template for all album owner roles.
		/// </returns>
		private static IGalleryServerRole CreateAlbumOwnerRoleTemplate(int galleryId)
		{
			return CreateRole(GetAlbumOwnerTemplateRoleName(galleryId), true, true, true, true, true, true, true, true, true, false, false, false, null);
		}

		/// <summary>
		/// Validates the album owner. If an album is being removed from the <paramref name="roleName"/> and that album is
		/// using this role for album ownership, remove the ownership setting from the album.
		/// </summary>
		/// <param name="roleName">Name of the role that is being modified.</param>
		/// <param name="rootAlbumIdsOld">The list of album ID's that were previously assigned to the role. If an album ID exists
		/// in this object and not in <paramref name="rootAlbumIdsNew"/>, that means the album is being removed from the role.</param>
		/// <param name="rootAlbumIdsNew">The list of album ID's that are now assigned to the role. If an album ID exists
		/// in this object and not in <paramref name="rootAlbumIdsOld"/>, that means it is a newly added album.</param>
		private static void ValidateAlbumOwnerRoles(string roleName, IEnumerable<int> rootAlbumIdsOld, ICollection<int> rootAlbumIdsNew)
		{
			foreach (int albumId in rootAlbumIdsOld)
			{
				if (!rootAlbumIdsNew.Contains(albumId))
				{
					// Album has been removed from role. Remove owner from the album if the album owner role matches the one we are dealing with.
					IAlbum album = AlbumController.LoadAlbumInstance(albumId, false, true);
					if (album.OwnerRoleName == roleName)
					{
						album.OwnerUserName = String.Empty;
						GalleryObjectController.SaveGalleryObject(album);
					}
				}
			}
		}

		/// <summary>
		/// Create a role to manage the ownership permissions for the <paramref name="album"/> and user specified in the OwnerUserName
		/// property of the album. The permissions of the new role are copied from the album owner role template. The new role
		/// is persisted to the data store and the user specified as the album owner is added as its sole member. The album is updated
		/// so that the OwnerRoleName property contains the role's name, but the album is not persisted to the data store.
		/// </summary>
		/// <param name="album">The album for which a role to represent owner permissions is to be created.</param>
		/// <returns>Returns the name of the role that is created.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="album" /> is new and has not yet been persisted to the data store.</exception>
		private static string CreateAlbumOwnerRole(IAlbum album)
		{
			// Create a role modeled after the template owner role, attach it to the album, then add the specified user as its member.
			// Role name: Album Owner - rdmartin - rdmartin's album (album 193)
			if (album == null)
				throw new ArgumentNullException("album");

			if (album.IsNew)
				throw new ArgumentException("Album must be persisted to data store before calling RoleController.CreateAlbumOwnerRole.");

			string roleName = GenerateAlbumOwnerRoleName(album);

			if (!RoleExists(roleName))
				CreateRole(roleName);

			if (!IsUserInRole(album.OwnerUserName, roleName))
				AddUserToRole(album.OwnerUserName, roleName);

			// Remove the roles from the cache. We do this because may may have just created a user album (that is, 
			// AlbumController.CreateUserAlbum() is in the call stack) and we want to make sure the AllAlbumIds property
			// of the album owner template role has the latest list of albums, including potentially the new album 
			// (which will be the case if the administrator has selected a parent album of the user album in the template
			// role).
			HelperFunctions.RemoveCache(CacheItem.GalleryServerRoles);

			IGalleryServerRole role = Factory.LoadGalleryServerRole(roleName);
			if (role == null)
			{
				IGalleryServerRole roleSource = Factory.LoadGalleryServerRole(GetAlbumOwnerTemplateRoleName(album.GalleryId));

				if (roleSource == null)
					roleSource = CreateAlbumOwnerRoleTemplate(album.GalleryId);

				role = roleSource.Copy();
				role.RoleName = roleName;
			}

			if (!role.AllAlbumIds.Contains(album.Id))
				role.RootAlbumIds.Add(album.Id);

			role.Save();

			return roleName;
		}

		/// <summary>
		/// Generates the name of the album owner role. Some gymnastics are performed to ensure the length of the role name is less than the 
		/// maximum allowed.
		/// </summary>
		/// <param name="album">The album for which an album owner role is to be created.</param>
		/// <returns>Returns a role name whose length is less than or equal to a value defined in the function.</returns>
		private static string GenerateAlbumOwnerRoleName(IAlbum album)
		{
			const int maxRoleNameLength = 256;
			const int minAlbumTitleLength = 10;
			const string ellipse = "...";

			string roleNameTemplate = MakeRoleNameUnique(String.Format(CultureInfo.InvariantCulture, "{0} - {{UserName}} - {{AlbumTitle}} (album {1})", GlobalConstants.AlbumOwnerRoleNamePrefix, album.Id));

			string roleName = roleNameTemplate.Replace("{UserName}", album.OwnerUserName).Replace("{AlbumTitle}", album.Title);

			if (roleName.Length > maxRoleNameLength)
			{
				// Role name is too long. Trim the album title and/or user name.
				string newAlbumTitle = album.Title;
				string newUserName = album.OwnerUserName;
				int numCharsToTrim = roleName.Length - maxRoleNameLength;
				int numCharsTrimmed = 0;

				if ((album.Title.Length - numCharsToTrim) >= minAlbumTitleLength)
				{
					// We can do all the trimming we need by shortening the album title.
					newAlbumTitle = String.Concat(album.Title.Substring(0, album.Title.Length - numCharsToTrim - ellipse.Length), ellipse);
					numCharsTrimmed = numCharsToTrim;
				}
				else
				{
					// Trim max chars from album title while leaving minAlbumTitleLength chars left. We'll have to trim the username to 
					// get as short as we need.
					try
					{
						newAlbumTitle = String.Concat(album.Title.Substring(0, minAlbumTitleLength - ellipse.Length), ellipse);
						numCharsTrimmed = album.Title.Length - newAlbumTitle.Length;
					}
					catch (ArgumentOutOfRangeException) {}
				}

				if (numCharsTrimmed < numCharsToTrim)
				{
					// We still need to shorten things up. Trim the user name.
					numCharsToTrim = numCharsToTrim - numCharsTrimmed;
					if (album.OwnerUserName.Length > numCharsToTrim)
					{
						newUserName = String.Concat(album.OwnerUserName.Substring(0, album.OwnerUserName.Length - numCharsToTrim - ellipse.Length), ellipse);
					}
					else
					{
						// It is not expected we ever get to this path.
						throw new WebException(String.Format(CultureInfo.CurrentCulture, "Invalid role name length. Unable to shorten the album owner role name enough to satisfy maximum length restriction. Proposed name='{0}' (length={1}); Max length={2}", roleName, roleName.Length, maxRoleNameLength));
					}
				}

				roleName = roleNameTemplate.Replace("{UserName}", newUserName).Replace("{AlbumTitle}", newAlbumTitle);

				// Perform one last final check to ensure we shortened things up correctly.
				if (roleName.Length > maxRoleNameLength)
				{
					throw new WebException(String.Format(CultureInfo.CurrentCulture, "Unable to shorten the album owner role name enough to satisfy maximum length restriction. Proposed name='{0}' (length={1}); Max length={2}", roleName, roleName.Length, maxRoleNameLength));
				}
			}

			return roleName;
		}

		/// <summary>
		/// Gets the name of the album owner template role. Example: "_Album Owner Template (Gallery ID 2: 'Engineering')"
		/// </summary>
		/// <param name="galleryId">The ID of the gallery to which the album owner template role is to belong.</param>
		/// <returns>Returns the name of the album owner template role.</returns>
		private static string GetAlbumOwnerTemplateRoleName(int galleryId)
		{
			string galleryDescription = Factory.LoadGallery(galleryId).Description;

			if (galleryDescription.Length > 100)
			{
				// Too long - shorten up... (role name can be only 256 chars)
				galleryDescription = String.Concat(galleryDescription.Substring(0, 100), "...");
			}

			// Note: If you change this, be sure to update _gspAlbumOwnerTemplateRoleNameRegExPattern to that it will match!
			return MakeRoleNameUnique(String.Format(CultureInfo.InvariantCulture, "{0} (Gallery ID {1}: '{2}')", GlobalConstants.AlbumOwnerRoleTemplateName, galleryId, galleryDescription));
		}

		private static string GetCacheKeyNameForRoles(string userName)
		{
			return String.Concat(HttpContext.Current.Session.SessionID, "_", userName, "_Roles");
		}

		/// <summary>
		/// Verify data integrity after removing a user from one or more roles. Specifically, if a role is an album owner role, 
		/// then check all albums in that role to see if current user is an owner for any. If he is, clear out the ownership field.
		/// </summary>
		/// <param name="userName">Name of the user who was removed from one or more roles.</param>
		/// <param name="roleNames">The names of the roles the user were removed from.</param>
		private static void ValidateRemoveUserFromRole(string userName, IEnumerable<string> roleNames)
		{
			if (String.IsNullOrEmpty(userName))
				return;

			if (roleNames == null)
				return;

			foreach (string roleName in roleNames)
			{
				if (IsRoleAnAlbumOwnerRole(roleName))
				{
					IGalleryServerRole role = Factory.LoadGalleryServerRole(roleName);

					if (role == null)
					{
						// Normally shouldn't be null, but might be if role has been deleted outside GSP.
						continue;
					}

					foreach (int albumId in role.RootAlbumIds)
					{
						IAlbum album = AlbumController.LoadAlbumInstance(albumId, false, true);
						if (album.OwnerUserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
						{
							album.OwnerUserName = String.Empty;
							GalleryObjectController.SaveGalleryObject(album);
						}
					}
				}
			}
		}

		/// <summary>
		/// Replace the list of root album IDs for the <paramref name="role"/> with the album ID's specified in
		/// <paramref name="topLevelCheckedAlbumIds"/>. Note that this function will cause the AllAlbumIds property 
		/// to be cleared out (Count = 0). The property can be repopulated by calling <see cref="IGalleryServerRole.Save"/>.
		/// </summary>
		/// <param name="role">The role whose root album/role relationships should be updated. When editing
		/// an existing role, specify this.GalleryRole. For new roles, pass the newly created role before
		/// saving it.</param>
		/// <param name="topLevelCheckedAlbumIds">The top level album IDs. May be null.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="role" /> is null.</exception>
		private static void UpdateRoleAlbumRelationships(IGalleryServerRole role, IIntegerCollection topLevelCheckedAlbumIds)
		{
			if (role == null)
				throw new ArgumentNullException("role");

			if (topLevelCheckedAlbumIds == null)
				topLevelCheckedAlbumIds = new IntegerCollection();

			int[] rootAlbumIdsOld = new int[role.RootAlbumIds.Count];
			role.RootAlbumIds.CopyTo(rootAlbumIdsOld, 0);

			role.RootAlbumIds.Clear();

			if (role.AllowAdministerSite)
			{
				// Administer site permission automatically applies to all albums, so all we need to do is get
				// a reference to the root album ID in each gallery.
				foreach (IGallery gallery in role.Galleries)
				{
					role.RootAlbumIds.Add(Factory.LoadRootAlbumInstance(gallery.GalleryId).Id);
				}
			}
			else if (role.AllowAdministerGallery)
			{
				// Administer gallery permission automatically applies to all albums in a gallery, so get a reference
				// to the root album for each checked album ID.
				foreach (int albumId in topLevelCheckedAlbumIds)
				{
					IAlbum album = AlbumController.LoadAlbumInstance(albumId, false);

					while (!(album.Parent is GalleryServerPro.Business.NullObjects.NullGalleryObject))
					{
						album = (IAlbum)album.Parent;
					}

					if (!role.RootAlbumIds.Contains(album.Id))
					{
						role.RootAlbumIds.Add(album.Id);
					}
				}
			}
			else
			{
				role.RootAlbumIds.AddRange(topLevelCheckedAlbumIds);
			}

			if (IsRoleAnAlbumOwnerRole(role.RoleName))
				ValidateAlbumOwnerRoles(role.RoleName, rootAlbumIdsOld, role.RootAlbumIds);
		}

		/// <summary>
		/// Determines whether the logged on user has gallery admin permissions for at least one galley associated with the
		/// <paramref name="role" />. This method should only be called when the logged on user is a gallery admin. DO NOT CALL
		/// if the user is a site admin or if <see cref="IAppSetting.AllowGalleryAdminToViewAllUsersAndRoles" /> is <c>true</c>
		/// (it will yield the correct result but is unnecessarily complex).
		/// </summary>
		/// <param name="role">The role to evaluate.</param>
		/// <returns>
		/// 	<c>true</c> if the logged on user has gallery admin permissions for at least one galley associated with the
		/// <paramref name="role" />; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsUserGalleryAdminForRole(IGalleryServerRole role)
		{
			foreach (IGallery gallery in role.Galleries)
			{
				IAlbum rootAlbum = Factory.LoadRootAlbumInstance(gallery.GalleryId);
				if (Utils.IsUserAuthorized(SecurityActions.AdministerGallery, rootAlbum.Id, gallery.GalleryId, rootAlbum.IsPrivate))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the logged on user is a gallery admin for the album owner template role specified in <paramref name="role" />.
		/// This is done by verifying that the gallery ID specified in the role's name is for a gallery the user can administer.
		/// Returns <c>true</c> when the role is an album owner template role and the current user is a gallery admin for it's
		/// associated gallery; otherwise returns <c>false</c>.
		/// </summary>
		/// <param name="role">The role to evaluate. It is expected that the role is an album owner template role, but this is
		/// not a requirement (function always returns false for non-template roles).</param>
		/// <returns>
		/// 	Returns <c>true</c> when the role is an album owner template role and the current user is a gallery admin for it's
		/// associated gallery; otherwise returns <c>false</c>.
		/// </returns>
		private static bool IsUserGalleryAdminForAlbumOwnerTemplateRole(IGalleryServerRole role)
		{
			System.Text.RegularExpressions.Match match = _gspAlbumOwnerTemplateRoleNameRegEx.Match(role.RoleName);
			if (match.Success)
			{
				// Parse out the gallery ID from the role name. Ex: "_Album Owner Template (Gallery ID 723: My gallery)" yields "723"
				int galleryId = Convert.ToInt32(match.Groups["galleryId"].Value, CultureInfo.InvariantCulture);

				IGallery gallery = null;
				try
				{
					gallery = Factory.LoadGallery(galleryId);
				}
				catch (InvalidGalleryException) { }

				if ((gallery != null) && GalleryController.GetGalleriesCurrentUserCanAdminister().Contains(gallery))
				{
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}
