using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Web.Security;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Entity;

namespace GalleryServerPro.Web.Controller
{
	/// <summary>
	/// Contains functionality related to user management.
	/// </summary>
	public static class UserController
	{
		#region Private Fields

		private static MembershipProvider _membershipProvider;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the Membership provider used by Gallery Server Pro.
		/// </summary>
		/// <value>The Membership provider used by Gallery Server Pro.</value>
		internal static MembershipProvider MembershipGsp
		{
			get
			{
				if (_membershipProvider == null)
				{
					_membershipProvider = GetMembershipProvider();
				}

				return _membershipProvider;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the membership provider is configured to require the user to answer a password 
		/// question for password reset and retrieval. 
		/// </summary>
		/// <value>
		/// 	<c>true</c> if a password answer is required for password reset and retrieval; otherwise, <c>false</c>. The default is true.
		/// </value>
		public static bool RequiresQuestionAndAnswer
		{
			get
			{
				return MembershipGsp.RequiresQuestionAndAnswer;
			}
		}

		/// <summary>
		/// Indicates whether the membership provider is configured to allow users to reset their passwords. 
		/// </summary>
		/// <value><c>true</c> if the membership provider supports password reset; otherwise, <c>false</c>. The default is true.</value>
		public static bool EnablePasswordReset
		{
			get
			{
				return MembershipGsp.EnablePasswordReset;
			}
		}

		/// <summary>
		/// Indicates whether the membership provider is configured to allow users to retrieve their passwords. 
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the membership provider is configured to support password retrieval; otherwise, <c>false</c>. The default is false.
		/// </value>
		public static bool EnablePasswordRetrieval
		{
			get
			{
				return MembershipGsp.EnablePasswordRetrieval;
			}
		}

		/// <summary>
		/// Gets the minimum length required for a password. 
		/// </summary>
		/// <value>The minimum length required for a password. </value>
		public static int MinRequiredPasswordLength
		{
			get
			{
				return MembershipGsp.MinRequiredPasswordLength;
			}
		}

		/// <summary>
		/// Gets the minimum number of non alphanumeric characters that must be present in a password. 
		/// </summary>
		/// <value>The minimum number of non alphanumeric characters that must be present in a password.</value>
		public static int MinRequiredNonAlphanumericCharacters
		{
			get
			{
				return MembershipGsp.MinRequiredNonAlphanumericCharacters;
			}
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Gets a collection of all the users in the database. The users may be returned from a cache.
		/// </summary>
		/// <returns>Returns a collection of all the users in the database.</returns>
		public static IUserAccountCollection GetAllUsers()
		{
			IUserAccountCollection usersCache = (IUserAccountCollection)HelperFunctions.GetCache(CacheItem.Users);

			if (usersCache == null)
			{
				usersCache = new UserAccountCollection();

				int totalRecords;
				foreach (MembershipUser user in MembershipGsp.GetAllUsers(0, 0x7fffffff, out totalRecords))
				{
					usersCache.Add(ToUserAccount(user));
				}

				HelperFunctions.SetCache(CacheItem.Users, usersCache);
			}

			return usersCache;
		}

		/// <summary>
		/// Populates the properties of <paramref name="userToLoad" /> with information about the user. Requires that the
		/// <see cref="IUserAccount.UserName" /> property of the <paramref name="userToLoad" /> parameter be assigned a value.
		/// If no user with the specified username exists, no action is taken.
		/// </summary>
		/// <param name="userToLoad">The user account whose properties should be populated.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="userToLoad" /> is null.</exception>
		public static void LoadUser(IUserAccount userToLoad)
		{
			if (userToLoad == null)
				throw new ArgumentNullException("userToLoad");

			if (String.IsNullOrEmpty(userToLoad.UserName))
			{
				throw new ArgumentException("The UserName property of the userToLoad parameter must have a valid value. Instead, it was null or empty.");
			}

			IUserAccount user = GetUser(userToLoad.UserName, false);

			if (user != null)
			{
				user.CopyTo(userToLoad);
			}
		}

		/// <overloads>
		/// Gets information from the data source for a user.
		/// </overloads>
		/// <summary>
		/// Gets information from the data source for the current logged-on membership user.
		/// </summary>
		/// <returns>A <see cref="IUserAccount"/> representing the current logged-on membership user.</returns>
		public static IUserAccount GetUser()
		{
			return ToUserAccount(MembershipGsp.GetUser(Utils.UserName, false));
		}

		/// <summary>
		/// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user. 
		/// </summary>
		/// <param name="userName">The name of the user to get information for.</param>
		/// <param name="userIsOnline"><c>true</c> to update the last-activity date/time stamp for the user; <c>false</c> to return user 
		/// information without updating the last-activity date/time stamp for the user.</param>
		/// <returns>A <see cref="IUserAccount"/> object populated with the specified user's information from the data source.</returns>
		public static IUserAccount GetUser(string userName, bool userIsOnline)
		{
			return ToUserAccount(MembershipGsp.GetUser(userName, userIsOnline));
		}

		/// <summary>
		/// Gets a collection of users the current user has permission to view. Users who have administer site permission can view all users,
		/// as can gallery administrators when the application setting <see cref="IAppSetting.AllowGalleryAdminToViewAllUsersAndRoles"/> is true. When
		/// the setting is false, gallery admins can only view users in galleries they have gallery admin permission in. Note that
		/// a user may be able to view a user but not update it. This can happen when the user belongs to roles that are associated with
		/// galleries the current user is not an admin for. The users may be returned from a cache. Guaranteed to not return null.
		/// This overload is slower than <see cref="GetUsersCurrentUserCanView(bool, bool)"/>, so use that one when possible.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>
		/// Returns an <see cref="IUserAccountCollection"/> containing a list of roles the user has permission to view.
		/// </returns>
		/// <overloads>
		/// Gets a collection of users the current user has permission to view.
		/// </overloads>
		public static IUserAccountCollection GetUsersCurrentUserCanView(int galleryId)
		{
			return GetUsersCurrentUserCanView(Utils.IsCurrentUserSiteAdministrator(), Utils.IsCurrentUserGalleryAdministrator(galleryId));
		}

		/// <summary>
		/// Gets a collection of users the current user has permission to view. Users who have administer site permission can view all users,
		/// as can gallery administrators when the application setting <see cref="IAppSetting.AllowGalleryAdminToViewAllUsersAndRoles" /> is true. When 
		/// the setting is false, gallery admins can only view users in galleries they have gallery admin permission in. Note that
		/// a user may be able to view a user but not update it. This can happen when the user belongs to roles that are associated with
		/// galleries the current user is not an admin for. The users may be returned from a cache. Guaranteed to not return null.
		/// This overload is faster than <see cref="GetUsersCurrentUserCanView(int)" />, so use this one when possible.
		/// </summary>
		/// <param name="userIsSiteAdmin">If set to <c>true</c>, the currently logged on user is a site administrator.</param>
		/// <param name="userIsGalleryAdmin">If set to <c>true</c>, the currently logged on user is a gallery administrator for the current gallery.</param>
		/// <returns>
		/// Returns an <see cref="IUserAccountCollection"/> containing a list of roles the user has permission to view.
		/// </returns>
		public static IUserAccountCollection GetUsersCurrentUserCanView(bool userIsSiteAdmin, bool userIsGalleryAdmin)
		{
			if (userIsSiteAdmin)
			{
				return UserController.GetAllUsers();
			}
			else if (userIsGalleryAdmin)
			{
				// See if we have a list in the cache. If not, generate it and add to cache.
				Dictionary<string, IUserAccountCollection> usersCache = (Dictionary<string, IUserAccountCollection>)HelperFunctions.GetCache(CacheItem.UsersCurrentUserCanView);

				IUserAccountCollection users;
				string cacheKeyName = String.Empty;

				if (System.Web.HttpContext.Current.Session != null)
				{
					cacheKeyName = GetCacheKeyNameForUsersCurrentUserCanView(Utils.UserName);

					if ((usersCache != null) && (usersCache.TryGetValue(cacheKeyName, out users)))
					{
						return users;
					}
				}

				// Nothing in the cache. Calculate it - this is processor intensive when there are many users and/or roles.
				users = DetermineUsersCurrentUserCanView(userIsSiteAdmin, userIsGalleryAdmin);

				// Add to the cache before returning.
				if (usersCache == null)
				{
					usersCache = new Dictionary<string, IUserAccountCollection>();
				}

				// Add to the cache, but only if we have access to the session ID.
				if (System.Web.HttpContext.Current.Session != null)
				{
					lock (usersCache)
					{
						if (!usersCache.ContainsKey(cacheKeyName))
						{
							usersCache.Add(cacheKeyName, users);
						}
					}
					HelperFunctions.SetCache(CacheItem.UsersCurrentUserCanView, usersCache);
				}

				return users;
			}

			return new UserAccountCollection();
		}

		private static string GetCacheKeyNameForUsersCurrentUserCanView(string userName)
		{
			return String.Concat(System.Web.HttpContext.Current.Session.SessionID, "_", userName, "_Users");
		}

		/// <summary>
		/// Determine the users the currently logged on user can view.
		/// </summary>
		/// <param name="userIsSiteAdmin">If set to <c>true</c>, the currently logged on user is a site administrator.</param>
		/// <param name="userIsGalleryAdmin">If set to <c>true</c>, the currently logged on user is a gallery administrator for the current gallery.</param>
		/// <returns>Returns an <see cref="IUserAccountCollection"/> containing a list of roles the user has permission to view.</returns>
		private static IUserAccountCollection DetermineUsersCurrentUserCanView(bool userIsSiteAdmin, bool userIsGalleryAdmin)
		{
			if (userIsSiteAdmin || (userIsGalleryAdmin && AppSetting.Instance.AllowGalleryAdminToViewAllUsersAndRoles))
			{
				return UserController.GetAllUsers();
			}

			// Filter the accounts so that only users in galleries where
			// the current user is a gallery admin are shown.
			IGalleryCollection adminGalleries = UserController.GetGalleriesCurrentUserCanAdminister();

			IUserAccountCollection users = new UserAccountCollection();

			foreach (IUserAccount user in UserController.GetAllUsers())
			{
				foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser(user.UserName))
				{
					bool userHasBeenAdded = false;
					foreach (IGallery gallery in role.Galleries)
					{
						if (adminGalleries.Contains(gallery))
						{
							// User belongs to a gallery that the current user is a gallery admin for. Include the account.
							users.Add(user);
							userHasBeenAdded = true;
							break;
						}
					}
					if (userHasBeenAdded) break;
				}
			}
			return users;
		}

		/// <summary>
		/// Gets the password for the specified user name from the data source. 
		/// </summary>
		/// <param name="userName">The user to retrieve the password for. </param>
		/// <returns>The password for the specified user name.</returns>
		public static String GetPassword(string userName)
		{
			return MembershipGsp.GetPassword(userName, null);
		}

		/// <summary>
		/// Resets a user's password to a new, automatically generated password.
		/// </summary>
		/// <param name="userName">The user to reset the password for. </param>
		/// <returns>The new password for the specified user.</returns>
		public static String ResetPassword(string userName)
		{
			return MembershipGsp.ResetPassword(userName, null);
		}

		/// <summary>
		/// Processes a request to update the password for a membership user.
		/// </summary>
		/// <param name="userName">The user to update the password for.</param>
		/// <param name="oldPassword">The current password for the specified user.</param>
		/// <param name="newPassword">The new password for the specified user.</param>
		/// <returns><c>true</c> if the password was updated successfully; otherwise, <c>false</c>.</returns>
		public static bool ChangePassword(string userName, string oldPassword, string newPassword)
		{
			return MembershipGsp.ChangePassword(userName, oldPassword, newPassword);
		}

		/// <summary>
		/// Clears a lock so that the membership user can be validated.
		/// </summary>
		/// <param name="userName">The membership user whose lock status you want to clear.</param>
		/// <returns><c>true</c> if the membership user was successfully unlocked; otherwise, <c>false</c>.</returns>
		public static bool UnlockUser(string userName)
		{
			return MembershipGsp.UnlockUser(userName);
		}

		/// <overloads>
		/// Persist the user to the data store.
		/// </overloads>
		/// <summary>
		/// Persist the <paramref name="userToSave" /> to the data store.
		/// </summary>
		/// <param name="userToSave">The user to save.</param>
		public static void SaveUser(IUserAccount userToSave)
		{
			UserController.UpdateUser(userToSave);
		}

		/// <summary>
		/// Persist the <paramref name="userToSave"/> to the data store, including adding or removing the specified roles.
		/// Prior to saving, validation is performed and a <see cref="GallerySecurityException"/> is thrown if a business rule
		/// would be violated.
		/// </summary>
		/// <param name="userToSave">The user to save.</param>
		/// <param name="rolesToAdd">The roles to associate with the user. The roles should not already be associated with the
		/// user, although no harm is done if they do.</param>
		/// <param name="rolesToRemove">The roles to remove from the user.</param>
		/// <param name="galleryId">The ID of the current gallery.</param>
		/// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="userToSave" /> is null.</exception>
		public static void SaveUser(IUserAccount userToSave, string[] rolesToAdd, string[] rolesToRemove, int galleryId)
		{
			if (userToSave == null)
				throw new ArgumentNullException("userToSave");

			ValidateSaveUser(userToSave, rolesToAdd, rolesToRemove, galleryId);

			UserController.UpdateUser(userToSave);
			RoleController.AddUserToRoles(userToSave.UserName, rolesToAdd);
			RoleController.RemoveUserFromRoles(userToSave.UserName, rolesToRemove);

			bool addingOrDeletingRoles = ((rolesToAdd != null) && (rolesToAdd.Length > 0) || (rolesToRemove != null) && (rolesToRemove.Length > 0));

			if (addingOrDeletingRoles)
			{
				HelperFunctions.RemoveCache(CacheItem.GalleryServerRoles);
			}
		}

		/// <summary>
		/// Removes a user from the membership data source.
		/// </summary>
		/// <param name="userName">The name of the user to delete.</param>
		/// <returns><c>true</c> if the user was successfully deleted; otherwise, <c>false</c>.</returns>
		public static bool DeleteUser(string userName)
		{
			return MembershipGsp.DeleteUser(userName, true);
		}

		/// <summary>
		/// Contains functionality that must execute after a user has logged on. Specifically, roles are cleared from the cache
		/// and, if user albums are enabled, the user's personal album is validated. Developers integrating Gallery Server into
		/// their applications should call this method after they have authenticated a user. User must be logged on by the
		/// time this method is called. For example, one can call this method in the LoggedIn event of the ASP.NET Login control.
		/// </summary>
		/// <param name="galleryId">The gallery ID for the gallery where the user album is to be validated. This value is required 
		/// when user albums are enabled; it is ignored when user albums are disabled.</param>
		/// <param name="userName">Name of the user that has logged on.</param>
		public static void UserLoggedOn(string userName, int galleryId)
		{
			// NOTE: If modifying this function to use galleryId in a place other than the ValidateUserAlbum function, be sure to 
			// update the XML comment for this parameter.
			RoleController.RemoveRolesFromCache();

			// Store the user name and the fact that user is authenticated. Ideally we would not do this and just use
			// User.Identity.Name and User.Identity.IsAuthenticated, but those won't be assigned by ASP.NET until the 
			// next page load.
			Utils.IsAuthenticated = true;
			Utils.UserName = userName;

			ValidateUserAlbum(userName, galleryId);
		}

		/// <summary>
		/// Contains functionality that must execute after a user has logged off. Specifically, roles are cleared from the cache.
		/// Developers integrating Gallery Server into their applications should call this method after a user has signed out. 
		/// User must be already be logged off by the time this method is called. For example, one can call this method in the 
		/// LoggedOut event of the ASP.NET LoginStatus control.
		/// </summary>
		public static void UserLoggedOff()
		{
			RoleController.RemoveRolesFromCache();

			// Clear the user name and the fact that user is not authenticated. Ideally we would not do this and just use
			// User.Identity.Name and User.Identity.IsAuthenticated, but those won't be assigned by ASP.NET until the 
			// next page load.
			Utils.IsAuthenticated = false;
			Utils.UserName = String.Empty;
		}

		/// <summary>
		/// Creates a new account in the membership system with the specified <paramref name="userName"/>, <paramref name="password"/>,
		/// <paramref name="email"/>, and belonging to the specified <paramref name="roles"/>. If required, it sends a verification
		/// e-mail to the user, sends an e-mail notification to admins, and creates a user album. The account will be disabled when
		/// <paramref name="isSelfRegistration"/> is <c>true</c> and either the system option RequireEmailValidationForSelfRegisteredUser
		/// or RequireApprovalForSelfRegisteredUser is enabled.
		/// </summary>
		/// <param name="userName">Account name of the user. Cannot be null or empty.</param>
		/// <param name="password">The password for the user. Cannot be null or empty.</param>
		/// <param name="email">The email associated with the user. Required when <paramref name="isSelfRegistration"/> is true
		/// and email verification is enabled.</param>
		/// <param name="roles">The names of the roles to assign to the user. The roles must already exist. If null or empty, no
		/// roles are assigned to the user.</param>
		/// <param name="isSelfRegistration">Indicates when the user is creating his or her own account. Set to false when an
		/// administrator creates an account.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Returns the newly created user.</returns>
		/// <exception cref="MembershipCreateUserException">Thrown when an error occurs during account creation. Check the StatusCode
		/// property for a MembershipCreateStatus value.</exception>
		/// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="userName" /> or <paramref name="password" /> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="userName" /> or <paramref name="password" /> is an empty string.</exception>
		public static IUserAccount CreateUser(string userName, string password, string email, string[] roles, bool isSelfRegistration, int galleryId)
		{
			#region Validation

			if (userName == null)
				throw new ArgumentNullException("userName");

			if (password == null)
				throw new ArgumentNullException("password");

			if (String.IsNullOrEmpty(userName))
				throw new ArgumentException("The parameter cannot be an empty string.", "userName");

			if (String.IsNullOrEmpty(password))
				throw new ArgumentException("The parameter cannot be an empty string.", "password");

			if ((String.IsNullOrEmpty(email)) && (HelperFunctions.IsValidEmail(userName)))
			{
				// No email address was specified, but the user name happens to be in the form of an email address,
				// so let's set the email property to the user name.
				email = userName;
			}

			#endregion

			IGallerySettings gallerySettings = Factory.LoadGallerySetting(galleryId);

			// Step 1: Create the user. Any number of exceptions may occur; we'll let the caller deal with them.
			IUserAccount user = CreateUser(userName, password, email);

			// Step 2: If this is a self-registered account and email verification is enabled or admin approval is required,
			// disable it. It will be approved when the user validates the email or the admin gives approval.
			if (isSelfRegistration)
			{
				if (gallerySettings.RequireEmailValidationForSelfRegisteredUser || gallerySettings.RequireApprovalForSelfRegisteredUser)
				{
					user.IsApproved = false;
					UpdateUser(user);
				}
			}

			// Step 3: Verify no business rules are being violated by the logged-on user creating an account. We skip this verification
			// for self registrations, because there isn't a logged-on user.
			if (!isSelfRegistration)
			{
				ValidateSaveUser(user, roles, new string[] { }, galleryId);
			}

			// Step 4: Add user to roles.
			if ((roles != null) && (roles.Length > 0))
			{
				foreach (string role in roles)
				{
					RoleController.AddUserToRole(userName, role);
				}
			}

			// Step 5: Notify admins that an account was created.
			NotifyAdminsOfNewlyCreatedAccount(user, isSelfRegistration, false, galleryId);

			// Step 6: Send user a welcome message or a verification link.
			if (HelperFunctions.IsValidEmail(user.Email))
			{
				NotifyUserOfNewlyCreatedAccount(user, galleryId);
			}
			else if (isSelfRegistration && gallerySettings.RequireEmailValidationForSelfRegisteredUser)
			{
				// Invalid email, but we need one to send the email verification. Throw error.
				throw new MembershipCreateUserException(MembershipCreateStatus.InvalidEmail);
			}

			HelperFunctions.PurgeCache();

			return user;
		}

		/// <summary>
		/// Delete the user from the membership system. In addition, remove the user from any roles. If a role is an ownership role,
		/// then delete it if the user is the only member. Remove the user from ownership of any albums, and delete the user's
		/// personal album, if user albums are enabled.
		/// </summary>
		/// <param name="userName">Name of the user to be deleted.</param>
		/// <param name="preventDeletingLoggedOnUser">If set to <c>true</c>, throw a <see cref="WebException"/> if attempting
		/// to delete the currently logged on user.</param>
		/// <exception cref="WebException">Thrown when the user cannot be deleted because doing so violates one of the business rules.</exception>
		/// <exception cref="GallerySecurityException">Thrown when the user cannot be deleted because doing so violates one of the business rules.</exception>
		public static void DeleteGalleryServerProUser(string userName, bool preventDeletingLoggedOnUser)
		{
			if (String.IsNullOrEmpty(userName))
				return;

			ValidateDeleteUser(userName, preventDeletingLoggedOnUser, true);

			foreach (IGallery gallery in Factory.LoadGalleries())
			{
				DeleteUserAlbum(userName, gallery.GalleryId);
			}

			UpdateRolesAndOwnershipBeforeDeletingUser(userName);

			ProfileController.DeleteProfileForUser(userName);

			DeleteUser(userName);

			HelperFunctions.PurgeCache();
		}

		///// <summary>
		///// Gets a <see cref="System.Data.DataTable"/> named Users with a single string column named UserName that contains the user 
		///// names of all the members as returned by GetAllUsers(). Data may be returned from cache.
		///// </summary>
		///// <returns>Returns a <see cref="System.Data.DataTable"/> containing the user names of all the current users.</returns>
		//public static DataTable GetUserNames()
		//{
		//  DataTable usersCache = (DataTable)HelperFunctions.GetCache(CacheItem.Users);

		//  if (usersCache == null)
		//  {
		//    usersCache = new DataTable("Users");
		//    usersCache.Columns.Add(new DataColumn("UserName", typeof(String)));
		//    foreach (IUserAccount user in GetAllUsers())
		//    {
		//      DataRow dr = usersCache.NewRow();
		//      dr[0] = user.UserName;
		//      usersCache.Rows.Add(dr);
		//    }

		//    HelperFunctions.SetCache(CacheItem.Users, usersCache);
		//  }

		//  return usersCache;
		//}

		/// <overloads>
		/// Gets the personal album for a user.
		/// </overloads>
		/// <summary>
		/// Gets the album for the current user's personal album and <paramref name="galleryId" /> (that is, get the 
		/// album that was created when the user's account was created). The album is created if it does not exist. 
		/// If user albums are disabled or the user has disabled their own album, this function returns null. It also 
		/// returns null if the UserAlbumId property is not found in the profile (this should not typically occur).
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Returns the album for the current user's personal album.</returns>
		public static IAlbum GetUserAlbum(int galleryId)
		{
			return GetUserAlbum(Utils.UserName, galleryId);
		}

		/// <summary>
		/// Gets the personal album for the specified <paramref name="userName"/> and <paramref name="galleryId" /> 
		/// (that is, get the album that was created when the user's account was created). The album is created if it 
		/// does not exist. If user albums are disabled or the user has disabled their own album, this function returns 
		/// null. It also returns null if the UserAlbumId property is not found in the profile (this should not typically occur).
		/// </summary>
		/// <param name="userName">The account name for the user.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>
		/// Returns the personal album for the specified <paramref name="userName"/>.
		/// </returns>
		public static IAlbum GetUserAlbum(string userName, int galleryId)
		{
			return ValidateUserAlbum(userName, galleryId);
		}

		/// <summary>
		/// Gets the ID of the album for the specified user's personal album (that is, this is the album that was created when the
		/// user's account was created). If user albums are disabled or the UserAlbumId property is not found in the profile,
		/// this function returns int.MinValue. This function executes faster than <see cref="GetUserAlbum(int)"/> and 
		/// <see cref="GetUserAlbum(string, int)"/> but it does not validate that the album exists.
		/// </summary>
		/// <param name="userName">The account name for the user.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>
		/// Returns the ID of the album for the current user's personal album.
		/// </returns>
		public static int GetUserAlbumId(string userName, int galleryId)
		{
			int albumId = Int32.MinValue;

			if (!Factory.LoadGallerySetting(galleryId).EnableUserAlbum)
				return albumId;

			int tmpAlbumId = ProfileController.GetProfileForGallery(userName, galleryId).UserAlbumId;
			albumId = (tmpAlbumId > 0 ? tmpAlbumId : albumId);

			return albumId;
		}

		/// <summary>
		/// Verifies the user album for the specified <paramref name="userName">user</paramref> exists if it is supposed to exist
		/// (creating it if necessary), or does not exist if not (that is, deleting it if necessary). Returns a reference to the user
		/// album if a user album exists or has just been created; otherwise returns null. Also returns null if user albums are
		/// disabled at the application level or <see cref="IGallerySettings.UserAlbumParentAlbumId" /> does not match an existing album.
		/// A user album is created if user albums are enabled but none for the user exists. If user albums are enabled at the
		/// application level but the user has disabled them in his profile, the album is deleted if it exists.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="galleryId">The gallery ID for the gallery where the user album is to be validated. This value is required 
		/// when user albums are enabled; it is ignored when user albums are disabled.</param>
		/// <returns>
		/// Returns a reference to the user album for the specified <paramref name="userName">user</paramref>, or null
		/// if user albums are disabled or <see cref="IGallerySettings.UserAlbumParentAlbumId" /> does not match an existing album.
		/// </returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="userName"/> is null or empty.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when user albums are enabled and <paramref name="galleryId"/> equals 
		/// <see cref="Int32.MinValue" />.</exception>
		public static IAlbum ValidateUserAlbum(string userName, int galleryId)
		{
			if (String.IsNullOrEmpty(userName))
				throw new ArgumentException("Parameter cannot be null or an empty string.", "userName");

			if (!Factory.LoadGallerySetting(galleryId).EnableUserAlbum)
				return null;

			if (galleryId == Int32.MinValue)
			{
				// If we get here then user albums are enabled but an invalid gallery ID has been passed. This function can't do 
				// its job without the ID, so throw an error.
				throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "A valid gallery ID must be passed to the UserController.ValidateUserAlbum function when user albums are enabled. Instead, the value {0} was passed for the gallery ID.", galleryId));
			}

			bool userAlbumExists = false;
			bool userAlbumShouldExist = ProfileController.GetProfileForGallery(userName, galleryId).EnableUserAlbum;

			IAlbum album = null;

			int albumId = GetUserAlbumId(userName, galleryId);

			if (albumId > Int32.MinValue)
			{
				try
				{
					// Try loading the album.
					album = AlbumController.LoadAlbumInstance(albumId, false, true);

					userAlbumExists = true;
				}
				catch (InvalidAlbumException) { }
			}

			// Delete or create if necessary. Deleting should only be needed if 
			if (userAlbumExists && !userAlbumShouldExist)
			{
				try
				{
					AlbumController.DeleteAlbum(album);
				}
				catch (Exception ex)
				{
					// Log any errors that happen but don't let them bubble up.
					AppErrorController.LogError(ex, galleryId);
				}
				finally
				{
					album = null;
				}
			}
			else if (!userAlbumExists && userAlbumShouldExist)
			{
				album = AlbumController.CreateUserAlbum(userName, galleryId);
			}

			return album;
		}

		/// <summary>
		/// Activates the account for the specified <paramref name="userName"/> and automatically logs on the user. If the
		/// admin approval system setting is enabled (RequireApprovalForSelfRegisteredUser=<c>true</c>), then record the
		/// validation in the user's comment field but do not activate the account. Instead, send the administrator(s) an
		/// e-mail notifying them of a pending account. This method is typically called after a user clicks the confirmation
		/// link in the verification e-mail after creating a new account.
		/// </summary>
		/// <param name="userName">Name of the user who has just validated his or her e-mail address.</param>
		/// <param name="galleryId">The gallery ID for the gallery where the user is being activated. This value is required 
		/// when user albums are enabled; it is ignored when user albums are disabled.</param>
		public static void UserEmailValidatedAfterCreation(string userName, int galleryId)
		{
			IUserAccount user = GetUser(userName, true);

			if (Factory.LoadGallerySetting(galleryId).RequireApprovalForSelfRegisteredUser)
			{
				NotifyAdminsOfNewlyCreatedAccount(user, true, true, galleryId);
			}
			else
			{
				user.IsApproved = true;

				LogOffUser();
				LogOnUser(userName, galleryId);
			}

			user.Comment = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.CreateAccount_Verification_Comment_Text, user.Email, DateTime.Now);

			UpdateUser(user);
		}

		/// <summary>
		/// Logs off the current user.
		/// </summary>
		public static void LogOffUser()
		{
			FormsAuthentication.SignOut();

			UserLoggedOff();
		}

		/// <overloads>
		/// Sets an authentication cookie for the specified user so that the user is considered logged on by the application. This
		/// function does not authenticate the user; the calling function must perform that function or otherwise guarantee that it
		/// is appropriate to log on the user.
		/// </overloads>
		/// <summary>
		/// Logs on the specified <paramref name="userName"/>.
		/// </summary>
		/// <param name="userName">The username for the user to log on.</param>
		public static void LogOnUser(string userName)
		{
			foreach (IGallery gallery in Factory.LoadGalleries())
			{
				LogOnUser(userName, gallery.GalleryId);
			}
		}

		/// <summary>
		/// Sets an authentication cookie for the specified <paramref name="userName"/> so that the user is considered logged on by
		/// the application. This function does not authenticate the user; the calling function must perform that function or 
		/// otherwise guarantee that it is appropriate to log on the user.
		/// </summary>
		/// <param name="userName">The username for the user to log on.</param>
		/// <param name="galleryId">The gallery ID for the gallery where the user album is to be validated. This value is required 
		/// when user albums are enabled; it is ignored when user albums are disabled.</param>
		public static void LogOnUser(string userName, int galleryId)
		{
			FormsAuthentication.SetAuthCookie(userName, false);

			UserLoggedOn(userName, galleryId);
		}

		/// <summary>
		/// Gets the error message associated with the <see cref="MembershipCreateUserException" /> exception that can occur when
		/// adding a user.
		/// </summary>
		/// <param name="status">A <see cref="MembershipCreateStatus" />. This can be populated from the 
		/// <see cref="MembershipCreateUserException.StatusCode" /> property of the exception.</param>
		/// <returns>Returns an error message.</returns>
		public static string GetAddUserErrorMessage(MembershipCreateStatus status)
		{
			switch (status)
			{
				case MembershipCreateStatus.DuplicateUserName:
					return Resources.GalleryServerPro.Admin_Manage_Users_Create_User_Error_DuplicateUserName;

				case MembershipCreateStatus.DuplicateEmail:
					return Resources.GalleryServerPro.Admin_Manage_Users_Create_User_Error_DuplicateEmail;

				case MembershipCreateStatus.InvalidPassword:
					return String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Admin_Manage_Users_Create_User_Error_InvalidPassword, MinRequiredPasswordLength, MinRequiredNonAlphanumericCharacters);

				case MembershipCreateStatus.InvalidEmail:
					return Resources.GalleryServerPro.Admin_Manage_Users_Create_User_Error_InvalidEmail;

				case MembershipCreateStatus.InvalidAnswer:
					return Resources.GalleryServerPro.Admin_Manage_Users_Create_User_Error_InvalidAnswer;

				case MembershipCreateStatus.InvalidQuestion:
					return Resources.GalleryServerPro.Admin_Manage_Users_Create_User_Error_InvalidQuestion;

				case MembershipCreateStatus.InvalidUserName:
					return Resources.GalleryServerPro.Admin_Manage_Users_Create_User_Error_InvalidUserName;

				case MembershipCreateStatus.ProviderError:
					return Resources.GalleryServerPro.Admin_Manage_Users_Create_User_Error_ProviderError;

				case MembershipCreateStatus.UserRejected:
					return Resources.GalleryServerPro.Admin_Manage_Users_Create_User_Error_UserRejected;

				default:
					return Resources.GalleryServerPro.Admin_Manage_Users_Create_User_Error_Generic;
			}
		}

		/// <summary>
		/// Get a list of galleries the current user can administer. Site administrators can view all galleries, while gallery
		/// administrators may have access to zero or more galleries.
		/// </summary>
		/// <returns>Returns an <see cref="IGalleryCollection" /> containing the galleries the current user can administer.</returns>
		public static IGalleryCollection GetGalleriesCurrentUserCanAdminister()
		{
			return GetGalleriesUserCanAdminister(Utils.UserName);
		}

		/// <summary>
		/// Get a list of galleries the specified <paramref name="userName"/> can administer. Site administrators can view all
		/// galleries, while gallery administrators may have access to zero or more galleries.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns>
		/// Returns an <see cref="IGalleryCollection"/> containing the galleries the current user can administer.
		/// </returns>
		public static IGalleryCollection GetGalleriesUserCanAdminister(string userName)
		{
			IGalleryCollection adminGalleries = new GalleryCollection();
			foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser(userName))
			{
				if (role.AllowAdministerSite)
				{
					return Factory.LoadGalleries();
				}
				else if (role.AllowAdministerGallery)
				{
					foreach (IGallery gallery in role.Galleries)
					{
						if (!adminGalleries.Contains(gallery))
						{
							adminGalleries.Add(gallery);
						}
					}
				}
			}

			return adminGalleries;
		}

		/// <summary>
		/// Gets a collection of all the galleries the specified <paramref name="userName" /> has access to.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns>Returns an <see cref="IGalleryCollection" /> of all the galleries the specified <paramref name="userName" /> has access to.</returns>
		public static IGalleryCollection GetGalleriesForUser(string userName)
		{
			IGalleryCollection galleries = new GalleryCollection();

			foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser(userName))
			{
				foreach (IGallery gallery in role.Galleries)
				{
					if (!galleries.Contains(gallery))
					{
						galleries.Add(gallery);
					}
				}
			}

			return galleries;
		}

		/// <summary>
		/// Validates the logged on user has permission to save the specified <paramref name="userToSave"/> and add/remove the user 
		/// to/from the specified <paramref name="rolesToAdd"/> and <paramref name="rolesToRemove" />. Throw a 
		/// <see cref="GallerySecurityException"/> if user is not authorized.
		/// This method assumes the logged on user is a site administrator or gallery administrator but does not verify it.
		/// </summary>
		/// <param name="userToSave">The user to save. The only property that must be specified is <see cref="IUserAccount.UserName" />.</param>
		/// <param name="rolesToAdd">The roles to be associated with the user. The roles should not already be assigned to the
		/// user, although no harm is done if they are.</param>
		/// <param name="rolesToRemove">The roles to remove from user.</param>
		/// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
		public static void ValidateLoggedOnUserHasPermissionToSaveUser(IUserAccount userToSave, string[] rolesToAdd, string[] rolesToRemove)
		{
			#region Parameter validation

			if (rolesToAdd == null)
				rolesToAdd = new string[] { };

			if (rolesToRemove == null)
				rolesToRemove = new string[] { };

			#endregion

			// Enforces the following rules:
			// 1. A user with site administration permission has no restrictions. Subsequent rules do not apply.
			// 2. Gallery admin is not allowed to add admin site permission to any user or update any user that has site admin permission.
			// 3. Gallery admin cannot add or remove a user to/from a role associated with other galleries, UNLESS he is also a gallery admin
			//    to those galleries.
			// 4. NOT ENFORCED: If user to be updated is a member of roles that apply to other galleries, Gallery admin must be a gallery admin 
			//    in every one of those galleries. Not enforced because this is considered acceptable behavior.

			if (Utils.IsCurrentUserSiteAdministrator())
				return;

			VerifyGalleryAdminIsNotUpdatingUserWithAdminSitePermission(userToSave, rolesToAdd);

			VerifyGalleryAdminCanAddOrRemoveRolesForUser(rolesToAdd, rolesToRemove);

			#region RULE 4 (Not enforced)
			// RULE 4: Gallery admin can update user only when he is a gallery admin in every gallery the user to be updated is a member of.

			//// Step 1: Get a list of galleries the user to be updated is associated with.
			//IGalleryCollection userGalleries = new GalleryCollection();
			//foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser(userToSave.UserName))
			//{
			//  foreach (IGallery gallery in role.Galleries)
			//  {
			//    if (!userGalleries.Contains(gallery))
			//    {
			//      userGalleries.Add(gallery);
			//    }
			//  }
			//}

			//// Step 2: Validate that the current user is a gallery admin for every gallery the user to be updated is a member of.
			//foreach (IGallery userGallery in userGalleries)
			//{
			//  if (!adminGalleries.Contains(userGallery))
			//  {
			//    throw new GallerySecurityException("You are attempting to save changes to a user that affects multiple galleries, including at least one gallery you do not have permission to administer. To edit this user, you must be a gallery administrator in every gallery this user is a member of.");
			//  }
			//}
			#endregion
		}

		/// <summary>
		/// In certain cases, the web-based installer creates a text file in the App Data directory that is meant as a signal to this
		/// code that additional setup steps are required. If this file is found, carry out the additional actions. This file is
		/// created in the SetFlagForMembershipConfiguration() method of pages\install.ascx.cs.
		/// </summary>
		internal static void ProcessInstallerFile()
		{
			string filePath = Path.Combine(AppSetting.Instance.PhysicalApplicationPath, Path.Combine(GlobalConstants.AppDataDirectory, GlobalConstants.InstallMembershipFileName));

			if (!File.Exists(filePath))
				return;

			string adminUserName;
			string adminPwd;
			string adminEmail;
			using (StreamReader sw = File.OpenText(filePath))
			{
				adminUserName = sw.ReadLine();
				adminPwd = sw.ReadLine();
				adminEmail = sw.ReadLine();
			}

			HelperFunctions.BeginTransaction();

			#region Create the Sys Admin role.

			// Create the Sys Admin role. If it already exists, make sure it has AllowAdministerSite permission.
			string sysAdminRoleName = Resources.GalleryServerPro.Installer_Sys_Admin_Role_Name;
			if (!RoleController.RoleExists(sysAdminRoleName))
				RoleController.CreateRole(sysAdminRoleName);

			IGalleryServerRole role = Factory.LoadGalleryServerRole(sysAdminRoleName);
			if (role == null)
			{
				role = Factory.CreateGalleryServerRoleInstance(sysAdminRoleName, true, true, true, true, true, true, true, true, true, true, true, false);

				// Ideally, at this point we should add the root album ID for the current gallery to the RootAlbumIds property. But we don't
				// know the gallery ID at this stage of the app life cycle, so we'll just avoid setting it. When we subsequently load this
				// role through Factory.LoadGalleryServerRoles(), there will be validation code that makes sure all sys admin roles are assigned
				// to the root album in each gallery, so we are covered.

				role.Save();
			}
			else
			{
				// Role already exists. Make sure it has Sys Admin permission.
				if (!role.AllowAdministerSite)
				{
					role.AllowAdministerSite = true;
					role.Save();
				}
			}

			#endregion

			#region Create the Sys Admin user account.

			// Create the Sys Admin user account. Will throw an exception if the name is already in use.
			try
			{
				CreateUser(adminUserName, adminPwd, adminEmail);
			}
			catch (MembershipCreateUserException ex)
			{
				if (ex.StatusCode == MembershipCreateStatus.DuplicateUserName)
				{
					// The user already exists. Update the password and email address to our values.
					IUserAccount user = GetUser(adminUserName, true);
					ChangePassword(user.UserName, GetPassword(user.UserName), adminPwd);
					user.Email = adminEmail;
					UpdateUser(user);
				}
			}

			// Add the Sys Admin user to the Sys Admin role.
			if (!RoleController.IsUserInRole(adminUserName, sysAdminRoleName))
				RoleController.AddUserToRole(adminUserName, sysAdminRoleName);

			#endregion

			// Set a flag so that later code will create a sample album and media object. The reason we don't do it here is because
			// we don't have access to the gallery ID at this stage of the page lifecycle.
			AppSetting.Instance.SampleObjectsNeeded = true;

			HelperFunctions.CommitTransaction();
			HelperFunctions.PurgeCache();

			File.Delete(filePath);
		}

		#endregion

		#region Private Static Methods

		/// <summary>
		/// Adds a new user with the specified e-mail address to the data store.
		/// </summary>
		/// <param name="userName">The user name for the new user.</param>
		/// <param name="password">The password for the new user.</param>
		/// <param name="email">The email for the new user.</param>
		/// <returns>Returns a new user with the specified e-mail address to the data store.</returns>
		private static IUserAccount CreateUser(string userName, string password, string email)
		{
			// This function is a re-implementation of the System.Web.Security.Membership.CreateUser method. We can't call it directly
			// because it uses the default provider, and we might be using a named provider.
			MembershipCreateStatus status;
			MembershipUser user = MembershipGsp.CreateUser(userName, password, email, null, null, true, null, out status);
			if (user == null)
			{
				throw new MembershipCreateUserException(status);
			}

			return ToUserAccount(user);
		}

		/// <summary>
		/// Gets the Membership provider used by Gallery Server Pro.
		/// </summary>
		/// <returns>The Membership provider used by Gallery Server Pro.</returns>
		private static MembershipProvider GetMembershipProvider()
		{
			if (String.IsNullOrEmpty(AppSetting.Instance.MembershipProviderName))
			{
				return Membership.Provider;
			}
			else
			{
				return Membership.Providers[AppSetting.Instance.MembershipProviderName];
			}
		}

		/// <summary>
		/// Send an e-mail to the users that are subscribed to new account notifications. These are specified in the
		/// <see cref="IGallerySettings.UsersToNotifyWhenAccountIsCreated" /> configuration setting. If 
		/// <see cref="IGallerySettings.RequireEmailValidationForSelfRegisteredUser" /> is enabled, do not send an e-mail at this time. 
		/// Instead, it is sent when the user clicks the confirmation link in the e-mail.
		/// </summary>
		/// <param name="user">An instance of <see cref="IUserAccount"/> that represents the newly created account.</param>
		/// <param name="isSelfRegistration">Indicates when the user is creating his or her own account. Set to false when an
		/// administrator creates an account.</param>
		/// <param name="isEmailVerified">If set to <c>true</c> the e-mail has been verified to be a valid, active e-mail address.</param>
		/// <param name="galleryId">The gallery ID storing the e-mail configuration information and the list of users to notify.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
		private static void NotifyAdminsOfNewlyCreatedAccount(IUserAccount user, bool isSelfRegistration, bool isEmailVerified, int galleryId)
		{
			if (user == null)
				throw new ArgumentNullException("user");

			IGallerySettings gallerySettings = Factory.LoadGallerySetting(galleryId);

			if (isSelfRegistration && !isEmailVerified && gallerySettings.RequireEmailValidationForSelfRegisteredUser)
			{
				return;
			}

			EmailTemplate emailTemplate;
			if (isSelfRegistration && gallerySettings.RequireApprovalForSelfRegisteredUser)
			{
				emailTemplate = EmailController.GetEmailTemplate(EmailTemplateForm.AdminNotificationAccountCreatedRequiresApproval, user);
			}
			else
			{
				emailTemplate = EmailController.GetEmailTemplate(EmailTemplateForm.AdminNotificationAccountCreated, user);
			}

			foreach (IUserAccount userToNotify in gallerySettings.UsersToNotifyWhenAccountIsCreated)
			{
				if (!String.IsNullOrEmpty(userToNotify.Email))
				{
					MailAddress admin = new MailAddress(userToNotify.Email, userToNotify.UserName);
					try
					{
						EmailController.SendEmail(admin, emailTemplate.Subject, emailTemplate.Body, galleryId);
					}
					catch (WebException ex)
					{
						AppErrorController.LogError(ex);
					}
					catch (SmtpException ex)
					{
						AppErrorController.LogError(ex);
					}
				}
			}
		}

		/// <summary>
		/// Send an e-mail to the user associated with the new account. This will be a verification e-mail if e-mail verification
		/// is enabled; otherwise it is a welcome message. The calling method should ensure that the <paramref name="user"/>
		/// has a valid e-mail configured before invoking this function.
		/// </summary>
		/// <param name="user">An instance of <see cref="IUserAccount"/> that represents the newly created account.</param>
		/// <param name="galleryId">The gallery ID. This specifies which gallery to use to look up configuration settings.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
		private static void NotifyUserOfNewlyCreatedAccount(IUserAccount user, int galleryId)
		{
			if (user == null)
				throw new ArgumentNullException("user");

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(galleryId);

			bool enableEmailVerification = gallerySetting.RequireEmailValidationForSelfRegisteredUser;
			bool requireAdminApproval = gallerySetting.RequireApprovalForSelfRegisteredUser;

			if (enableEmailVerification)
			{
				EmailController.SendNotificationEmail(user, EmailTemplateForm.UserNotificationAccountCreatedNeedsVerification, galleryId);
			}
			else if (requireAdminApproval)
			{
				EmailController.SendNotificationEmail(user, EmailTemplateForm.UserNotificationAccountCreatedNeedsApproval, galleryId);
			}
			else
			{
				EmailController.SendNotificationEmail(user, EmailTemplateForm.UserNotificationAccountCreated, galleryId);
			}
		}

		/// <summary>
		/// Throws an exception if the user cannot be deleted, such as when trying to delete his or her own account, or when deleting
		/// the only account with admin permission.
		/// </summary>
		/// <param name="userName">Name of the user to delete.</param>
		/// <param name="preventDeletingLoggedOnUser">If set to <c>true</c>, throw a <see cref="GallerySecurityException"/> if attempting
		/// to delete the currently logged on user.</param>
		/// <param name="preventDeletingLastAdminAccount">If set to <c>true</c> throw a <see cref="GallerySecurityException"/> if attempting
		/// to delete the last user with <see cref="SecurityActions.AdministerSite" /> permission. When false, do not perform this check. It does not matter
		/// whether the user to delete is actually an administrator.</param>
		/// <exception cref="GallerySecurityException">Thrown when the user cannot be deleted because doing so violates one of the business rules.</exception>
		private static void ValidateDeleteUser(string userName, bool preventDeletingLoggedOnUser, bool preventDeletingLastAdminAccount)
		{
			if (preventDeletingLoggedOnUser)
			{
				// Don't let user delete their own account.
				if (userName.Equals(Utils.UserName, StringComparison.OrdinalIgnoreCase))
				{
					throw new GallerySecurityException(Resources.GalleryServerPro.Admin_Manage_Users_Cannot_Delete_User_Msg);
				}
			}

			if (preventDeletingLastAdminAccount)
			{
				if (!DoesAtLeastOneOtherSiteAdminExist(userName))
				{
					if (!DoesAtLeastOneOtherGalleryAdminExist(userName))
					{
						throw new GallerySecurityException("You are attempting to delete the only user with permission to administer a gallery or site. If you want to delete this account, first assign another account to a role with administrative permission.");
					}
				}
			}

			// User can delete account only if he is a site admin or a gallery admin in every gallery this user can access.
			IGalleryCollection adminGalleries = GetGalleriesCurrentUserCanAdminister();

			if (adminGalleries.Count > 0) // Only continue when user is an admin for at least one gallery. This allows regular users to delete their own account.
			{
				foreach (IGallery gallery in GetGalleriesForUser(userName))
				{
					if (!adminGalleries.Contains(gallery))
					{
						throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "The user '{0}' has access to a gallery (Gallery ID = {1}) that you are not an administrator for. To delete a user, one of the following must be true: (1) you are a site administrator, or (2) you are a gallery administrator in every gallery the user has access to.", userName, gallery.GalleryId));
					}
				}
			}
		}

		/// <summary>
		/// If user is a gallery admin, verify at least one other user is a gallery admin for each gallery. If user is not a gallery 
		/// admin for any gallery, return <c>true</c> without actually verifying that each that each gallery has an admin, since it
		/// is reasonable to assume it does (and even if it didn't, that shouldn't prevent us from deleting this user).
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns><c>true</c> if at least one user besides <paramref name="userName" /> is a gallery admin for each gallery;
		/// otherwise <c>false</c>.</returns>
		private static bool DoesAtLeastOneOtherGalleryAdminExist(string userName)
		{
			bool atLeastOneOtherAdminExists = false;

			IGalleryCollection galleriesUserCanAdminister = UserController.GetGalleriesUserCanAdminister(userName);

			if (galleriesUserCanAdminister.Count == 0)
			{
				// User is not a gallery administrator, so we don't have to make sure there is another gallery administrator.
				// Besides, we can assume there is another one anyway.
				return true;
			}

			foreach (IGallery gallery in galleriesUserCanAdminister)
			{
				// Get all the roles that have gallery admin permission to this gallery
				foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForGallery(gallery).GetRolesWithGalleryAdminPermission())
				{
					// Make sure at least one user besides the user specified in userName is in these roles.
					foreach (string userNameInRole in RoleController.GetUsersInRole(role.RoleName))
					{
						if (!userNameInRole.Equals(userName, StringComparison.OrdinalIgnoreCase))
						{
							atLeastOneOtherAdminExists = true;
							break;
						}
					}

					if (atLeastOneOtherAdminExists)
						break;
				}

				if (atLeastOneOtherAdminExists)
					break;
			}

			return atLeastOneOtherAdminExists;
		}

		/// <summary>
		/// Determine if at least one other user beside <paramref name="userName" /> is a site administrator.
		/// </summary>
		/// <param name="userName">A user name.</param>
		/// <returns><c>true</c> if at least one other user beside <paramref name="userName" /> is a site administrator; otherwise <c>false</c>.</returns>
		private static bool DoesAtLeastOneOtherSiteAdminExist(string userName)
		{
			bool atLeastOneOtherAdminExists = false;

			foreach (IGalleryServerRole role in RoleController.GetGalleryServerRoles())
			{
				if (!role.AllowAdministerSite)
					continue;

				foreach (string userInAdminRole in RoleController.GetUsersInRole(role.RoleName))
				{
					if (userInAdminRole != userName)
					{
						atLeastOneOtherAdminExists = true;
						break;
					}
				}
			}
			return atLeastOneOtherAdminExists;
		}

		private static void DeleteUserAlbum(string userName, int galleryId)
		{
			IAlbum album = GetUserAlbum(userName, galleryId);

			if (album != null)
				AlbumController.DeleteAlbum(album);
		}

		/// <summary>
		/// Remove the user from any roles. If a role is an ownership role, then delete it if the user is the only member.
		/// Remove the user from ownership of any albums.
		/// </summary>
		/// <param name="userName">Name of the user to be deleted.</param>
		/// <remarks>The user will be specified as an owner only for those albums that belong in ownership roles, so
		/// to find all albums the user owns, we need only to loop through the user's roles and inspect the ones
		/// where the names begin with the album owner role name prefix variable.</remarks>
		private static void UpdateRolesAndOwnershipBeforeDeletingUser(string userName)
		{
			List<string> rolesToDelete = new List<string>();

			string[] userRoles = RoleController.GetRolesForUser(userName);
			foreach (string roleName in userRoles)
			{
				if (RoleController.IsRoleAnAlbumOwnerRole(roleName))
				{
					if (RoleController.GetUsersInRole(roleName).Length <= 1)
					{
						// The user we are deleting is the only user in the owner role. Mark for deletion.
						rolesToDelete.Add(roleName);
					}
				}
			}

			if (userRoles.Length > 0)
			{
				foreach (string role in userRoles)
				{
					RoleController.RemoveUserFromRole(userName, role);
				}
			}

			foreach (string roleName in rolesToDelete)
			{
				RoleController.DeleteGalleryServerProRole(roleName);
			}
		}

		private static IUserAccount ToUserAccount(MembershipUser u)
		{
			if (u == null)
				return null;

			if (MembershipGsp.GetType().ToString() == "System.Web.Security.ActiveDirectoryMembershipProvider")
			{
				// The AD provider does not support a few properties so substitute default values for them.
				return new UserAccount(u.Comment, u.CreationDate, u.Email, u.IsApproved, u.IsLockedOut, false,
															 DateTime.MinValue, u.LastLockoutDate, DateTime.MinValue, u.LastPasswordChangedDate,
															 u.PasswordQuestion, u.ProviderName, u.ProviderUserKey, u.UserName, false, String.Empty, String.Empty, String.Empty);
			}
			else
			{
				return new UserAccount(u.Comment, u.CreationDate, u.Email, u.IsApproved, u.IsLockedOut, u.IsOnline,
															 u.LastActivityDate, u.LastLockoutDate, u.LastLoginDate, u.LastPasswordChangedDate,
															 u.PasswordQuestion, u.ProviderName, u.ProviderUserKey, u.UserName, false, String.Empty, String.Empty, String.Empty);
			}
		}

		private static MembershipUser ToMembershipUser(IUserAccount u)
		{
			if (String.IsNullOrEmpty(u.UserName))
			{
				throw new ArgumentException("IUserAccount.UserName cannot be empty.");
			}

			MembershipUser user = MembershipGsp.GetUser(u.UserName, false);

			user.Comment = u.Comment;
			user.Email = u.Email;
			user.IsApproved = u.IsApproved;

			return user;
		}

		/// <summary>
		/// Updates information about a user in the data source.
		/// </summary>
		/// <param name="user">A <see cref="IUserAccount"/> object that represents the user to update and the updated information for the user.</param>
		private static void UpdateUser(IUserAccount user)
		{
			if (UserHasBeenModified(user))
			{
				MembershipGsp.UpdateUser(ToMembershipUser(user));
			}
		}

		/// <summary>
		/// Make sure the loggod-on person has authority to save the user info and that h/she isn't doing anything stupid,
		/// like removing admin permission from his or her own account. Throws a <see cref="GallerySecurityException"/> when
		/// the action is not allowed.
		/// </summary>
		/// <param name="userToSave">The user to save.</param>
		/// <param name="rolesToAdd">The roles to associate with the user. The roles should not already be associated with the
		/// user, although no harm is done if they do.</param>
		/// <param name="rolesToRemoveFromUser">The roles to remove from user.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
		private static void ValidateSaveUser(IUserAccount userToSave, string[] rolesToAdd, string[] rolesToRemoveFromUser, int galleryId)
		{
			if (!Utils.IsCurrentUserSiteAdministrator() && !Utils.IsCurrentUserGalleryAdministrator(galleryId))
			{
				throw new GallerySecurityException("You must be a gallery or site administrator to save changes to this user.");
			}

			if (userToSave.UserName.Equals(Utils.UserName, StringComparison.OrdinalIgnoreCase))
			{
				ValidateUserCanSaveOwnAccount(userToSave, rolesToRemoveFromUser);
			}

			ValidateLoggedOnUserHasPermissionToSaveUser(userToSave, rolesToAdd, rolesToRemoveFromUser);
		}

		/// <summary>
		/// Gets a value indicating whether the <paramref name="userToSave" /> is different than the one stored in the 
		/// membership provider.
		/// </summary>
		/// <param name="userToSave">The user to persist to the membership provider.</param>
		/// <returns>A bool indicating whether the <paramref name="userToSave" /> is different than the one stored in the 
		/// membership provider.</returns>
		private static bool UserHasBeenModified(IUserAccount userToSave)
		{
			MembershipUser user = MembershipGsp.GetUser(userToSave.UserName, false);

			if (user == null)
				return true;

			bool commentEqual = ((String.IsNullOrEmpty(userToSave.Comment) && String.IsNullOrEmpty(user.Comment)) || userToSave.Comment == user.Comment);
			bool emailEqual = ((String.IsNullOrEmpty(userToSave.Email) && String.IsNullOrEmpty(user.Email)) || userToSave.Email == user.Email);
			bool isApprovedEqual = (userToSave.IsApproved == user.IsApproved);

			return (!(commentEqual && emailEqual && isApprovedEqual));
		}

		/// <summary>
		/// Validates the user can save his own account. Throws a <see cref="GallerySecurityException" /> when the action is not allowed.
		/// </summary>
		/// <param name="userToSave">The user to save.</param>
		/// <param name="rolesToRemoveFromUser">The roles to remove from user.</param>
		/// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
		private static void ValidateUserCanSaveOwnAccount(IUserAccount userToSave, string[] rolesToRemoveFromUser)
		{
			// This function should be called only when the logged on person is updating their own account. They are not allowed to 
			// revoke approval and they must remain in at least one role that has Administer Site or Administer Gallery permission.
			if (!userToSave.IsApproved)
			{
				throw new GallerySecurityException(Resources.GalleryServerPro.Admin_Manage_Users_Cannot_Revoke_Approval_Msg);
			}

			bool rolesAreBeingRemovedFromAccount = ((rolesToRemoveFromUser != null) && (rolesToRemoveFromUser.Length > 0));

			if (rolesAreBeingRemovedFromAccount)
			{
				bool hasAdminPermission = false;
				foreach (IGalleryServerRole galleryRole in RoleController.GetGalleryServerRolesForUser(userToSave.UserName))
				{
					if (Array.IndexOf<string>(rolesToRemoveFromUser, galleryRole.RoleName) < 0)
					{
						// This is a role the user is in that is NOT being removed.
						if (galleryRole.AllowAdministerSite || galleryRole.AllowAdministerGallery)
						{
							hasAdminPermission = true;
							break;
						}
					}
				}

				if (!hasAdminPermission)
				{
					throw new GallerySecurityException(Resources.GalleryServerPro.Admin_Manage_Users_Cannot_Save_User_Msg);
				}
			}
		}

		/// <summary>
		/// Verifies that the specified <paramref name="userToSave" /> is not a site administrator or is being added to a site administrator
		/// role. Calling methods should invoke this function ONLY when the current user is a gallery administrator.
		/// </summary>
		/// <param name="userToSave">The user to save. The only property that must be specified is <see cref="IUserAccount.UserName" />.</param>
		/// <param name="rolesToAdd">The roles to be associated with the user. Must not be null. The roles should not already be assigned to the
		/// user, although no harm is done if they are.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="userToSave" /> or <paramref name="rolesToAdd" /> is null.</exception>
		private static void VerifyGalleryAdminIsNotUpdatingUserWithAdminSitePermission(IUserAccount userToSave, IEnumerable<string> rolesToAdd)
		{
			if (userToSave == null)
				throw new ArgumentNullException("userToSave");

			if (rolesToAdd == null)
				throw new ArgumentNullException("rolesToAdd");

			IGalleryServerRoleCollection rolesAssignedOrBeingAssignedToUser = RoleController.GetGalleryServerRolesForUser(userToSave.UserName).Copy();

			foreach (string roleToAdd in rolesToAdd)
			{
				if (rolesAssignedOrBeingAssignedToUser.GetRole(roleToAdd) == null)
				{
					IGalleryServerRole role = Factory.LoadGalleryServerRole(roleToAdd);

					if (role != null)
					{
						rolesAssignedOrBeingAssignedToUser.Add(role);
					}
				}
			}

			foreach (IGalleryServerRole role in rolesAssignedOrBeingAssignedToUser)
			{
				if (role.AllowAdministerSite)
				{
					throw new GallerySecurityException("You must be a site administrator to add a user to a role with Administer site permission or update an existing user who has Administer site permission. Sadly, you are just a gallery administrator.");
				}
			}
		}

		/// <summary>
		/// Verifies the current user can add or remove the specified roles to or from a user. Specifically, the user must be a gallery
		/// administrator in every gallery each role is associated with. Calling methods should invoke this function ONLY when the current 
		/// user is a gallery administrator.
		/// </summary>
		/// <param name="rolesToAdd">The roles to be associated with the user. Must not be null. The roles should not already be assigned to the
		/// user, although no harm is done if they are.</param>
		/// <param name="rolesToRemove">The roles to remove from user.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="rolesToAdd" /> or <paramref name="rolesToRemove" /> is null.</exception>
		private static void VerifyGalleryAdminCanAddOrRemoveRolesForUser(IEnumerable<string> rolesToAdd, IEnumerable<string> rolesToRemove)
		{
			if (rolesToAdd == null)
				throw new ArgumentNullException("rolesToAdd");

			if (rolesToRemove == null)
				throw new ArgumentNullException("rolesToRemove");

			IGalleryCollection adminGalleries = UserController.GetGalleriesCurrentUserCanAdminister();

			List<string> rolesBeingAddedOrRemoved = new List<string>(rolesToAdd);
			rolesBeingAddedOrRemoved.AddRange(rolesToRemove);

			foreach (string roleName in rolesBeingAddedOrRemoved)
			{
				// Gallery admin cannot add or remove a user to/from a role associated with other galleries, UNLESS he is also a gallery admin
				// to those galleries.
				IGalleryServerRole roleToAddOrRemove = Factory.LoadGalleryServerRole(roleName);

				if (roleToAddOrRemove != null)
				{
					foreach (IGallery gallery in roleToAddOrRemove.Galleries)
					{
						if (!adminGalleries.Contains(gallery))
						{
							throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "You are attempting to save changes to a user that will affect multiple galleries, including at least one gallery you do not have permission to administer. Specifically, the role '{0}' applies to gallery {1}, which you are not an administrator for.", roleToAddOrRemove.RoleName, gallery.GalleryId));
						}
					}
				}
			}
		}

		#endregion
	}
}
