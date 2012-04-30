using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Xml;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Configuration;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;
using GalleryServerPro.Web.Pages;
using SecurityManager = GalleryServerPro.Business.SecurityManager;

namespace GalleryServerPro.Web
{
	/// <summary>
	/// Contains general purpose routines useful for this website as well as a convenient
	/// gateway to functionality provided in other business layers.
	/// </summary>
	public static class Utils
	{
		#region Private Static Fields

		private static readonly object _sharedLock = new object();
		private static string _galleryRoot;
		private static string _galleryResourcesPath;
		private static string _webConfigFilePath;

		#endregion

		#region Public Static Properties

		/// <summary>
		/// Gets or sets the name of the current user. This property becomes available immediately after a user logs in, even within
		/// the current page's life cycle. This property is preferred over HttpContext.Current.User.Identity.Name, which does not
		/// contain the user's name until the next page load. This property should be set only when the user logs in. When the 
		/// property is not explicitly assigned, it automatically returns the value of HttpContext.Current.User.Identity.Name.
		/// </summary>
		/// <value>The name of the current user.</value>
		public static string UserName
		{
			get
			{
				object userName = HttpContext.Current.Items["UserName"];
				if (userName != null)
				{
					return userName.ToString();
				}
				else
				{
					return HttpContext.Current.User.Identity.Name;
				}
			}
			set { HttpContext.Current.Items["UserName"] = value; }
		}

		/// <summary>
		/// Gets a value indicating whether the current user is authenticated. This property becomes true available immediately after 
		/// a user logs in, even within the current page's life cycle. This property is preferred over 
		/// HttpContext.Current.User.Identity.IsAuthenticated, which does not become true until the next page load. 
		/// This property should be set only when the user logs in. When the property is not explicitly assigned, it automatically 
		/// returns the value of HttpContext.Current.User.Identity.IsAuthenticated.
		/// </summary>
		public static bool IsAuthenticated
		{
			get
			{
				bool isAuthenticated;
				object objIsAuthenticated = HttpContext.Current.Items["IsAuthenticated"];

				if ((objIsAuthenticated != null) && Boolean.TryParse(objIsAuthenticated.ToString(), out isAuthenticated))
				{
					return isAuthenticated;
				}
				else
				{
					return HttpContext.Current.User.Identity.IsAuthenticated;
				}
			}
			set { HttpContext.Current.Items["IsAuthenticated"] = value; }
		}

		/// <summary>
		/// Gets a value indicating whether the current request is from the local computer. Returns <c>false</c> if 
		/// <see cref="HttpContext.Current" /> is null.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the current request is from the local computer; otherwise, <c>false</c>.
		/// </value>
		public static bool IsLocalRequest
		{
			get
			{
				if (HttpContext.Current == null)
					return false;

				return HttpContext.Current.Request.IsLocal;
			}
		}

		/// <summary>
		/// Get the path, relative to the web site root, to the directory containing the Gallery Server Pro user controls and 
		/// other resources. Does not include the containing page or the trailing slash. Example: If GSP is installed at 
		/// C:\inetpub\wwwroot\dev\gallery, where C:\inetpub\wwwroot\ is the parent web site, and the gallery support files are in 
		/// the gsp directory, this property returns /dev/gallery/gsp. Guaranteed to not return null.
		/// </summary>
		/// <value>Returns the path, relative to the web site root, to the directory containing the Gallery Server Pro user 
		/// controls and other resources.</value>
		public static string GalleryRoot
		{
			get
			{
				if (_galleryRoot == null)
				{
					_galleryRoot = CalculateGalleryRoot();
				}

				return _galleryRoot;
			}
		}

		/// <summary>
		/// Gets the path, relative to the current application, to the directory containing the Gallery Server Pro
		/// resources such as images, user controls, scripts, etc. This value is pulled from the galleryResourcesPath setting
		/// in the galleryServerPro/core section of web.config. Examples: "gs", "GalleryServerPro\resources"
		/// </summary>
		/// <value>Returns the path, relative to the current application, to the directory containing the Gallery Server Pro
		/// resources such as images, user controls, scripts, etc.</value>
		public static string GalleryResourcesPath
		{
			get
			{
				if (_galleryResourcesPath == null)
				{
					_galleryResourcesPath = GetGalleryResourcesPath();
				}

				return _galleryResourcesPath;
			}
		}

		/// <summary>
		/// Gets the fully qualified file path to web.config. Guaranteed to not return null.
		/// Example: C:\inetpub\wwwroot\gallery\web.config
		/// </summary>
		/// <value>The fully qualified file path to web.config.</value>
		public static string WebConfigFilePath
		{
			get
			{
				if (_webConfigFilePath == null)
				{
					_webConfigFilePath = HttpContext.Current.Server.MapPath("~/web.config");
				}

				return _webConfigFilePath;
			}
		}

		/// <summary>
		/// Get the path, relative to the web site root, to the current web application. Does not include the containing page 
		/// or the trailing slash. Example: If GSP is installed at C:\inetpub\wwwroot\dev\gallery, and C:\inetpub\wwwroot\ is 
		/// the parent web site, this property returns /dev/gallery. Guaranteed to not return null.
		/// </summary>
		/// <value>Get the path, relative to the web site root, to the current web application.</value>
		public static string AppRoot
		{
			get
			{
				return HttpContext.Current.Request.ApplicationPath.TrimEnd(new char[] { '/' });
			}
		}

		/// <summary>
		/// Gets or sets the URI of the previous page the user was viewing. The value is stored in the user's session, and 
		/// can be used after a user has completed a task to return to the original page. If the Session object is not available,
		/// no value is saved in the setter and a null is returned in the getter.
		/// </summary>
		/// <value>The URI of the previous page the user was viewing.</value>
		public static Uri PreviousUri
		{
			get
			{
				if (HttpContext.Current.Session != null)
					return (Uri)HttpContext.Current.Session["ReferringUrl"];
				else
					return null;
			}
			set
			{
				if (HttpContext.Current.Session == null)
					return; // Session is disabled for this page.

				HttpContext.Current.Session["ReferringUrl"] = value;
			}
		}

		#endregion

		#region Public Static Methods

		/// <overloads>
		/// Determine if the current user has permission to perform the requested action.
		/// </overloads>
		/// <summary>
		/// Determine whether user has permission to perform at least one of the specified security actions. Un-authenticated users
		/// (anonymous users) are always considered NOT authorized (that is, this method returns false) except when the requested 
		/// security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or <see cref="SecurityActions.ViewOriginalImage" />, 
		/// since Gallery Server is configured by default to allow anonymous viewing access
		/// but it does not allow anonymous editing of any kind. This method will continue to work correctly if the webmaster configures
		/// Gallery Server to require users to log in in order to view objects, since at that point there will be no such thing as
		/// un-authenticated users, and the standard gallery server role functionality applies.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
		/// a bitwise OR between them (example: SecurityActions.AdministerSite | SecurityActions.AdministerGallery). If multiple actions are 
		/// specified, the method is successful if the user has permission for at least one of the actions. If you require that all actions 
		/// be satisfied to be successful, call one of the overloads that accept a SecurityActionsOption and 
		/// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
		/// <param name="albumId">The album ID to which the security action applies.</param>
		/// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist in 
		/// this gallery. This parameter is not required <paramref name="securityActions" /> is SecurityActions.AdministerSite (you can specify 
		/// <see cref="int.MinValue" />).</param>
		/// <param name="isPrivate">Indicates whether the specified album is private (hidden from anonymous users). The parameter
		/// 	is ignored for logged on users.</param>
		/// <returns>
		/// Returns true when the user is authorized to perform the specified security action against the specified album;
		/// otherwise returns false.
		/// </returns>
		public static bool IsUserAuthorized(SecurityActions securityActions, int albumId, int galleryId, bool isPrivate)
		{
			return IsUserAuthorized(securityActions, RoleController.GetGalleryServerRolesForUser(), albumId, galleryId, isPrivate);
		}

		/// <summary>
		/// Determine whether user has permission to perform the specified security actions. Un-authenticated users
		/// (anonymous users) are always considered NOT authorized (that is, this method returns false) except when the requested
		/// security action is <see cref="SecurityActions.ViewAlbumOrMediaObject"/> or <see cref="SecurityActions.ViewOriginalImage"/>,
		/// since Gallery Server is configured by default to allow anonymous viewing access
		/// but it does not allow anonymous editing of any kind. This method will continue to work correctly if the webmaster configures
		/// Gallery Server to require users to log in in order to view objects, since at that point there will be no such thing as
		/// un-authenticated users, and the standard gallery server role functionality applies.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
		/// a bitwise OR between them (example: SecurityActions.AdministerSite | SecurityActions.AdministerGallery).</param>
		/// <param name="albumId">The album ID to which the security action applies.</param>
		/// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId"/> must exist in
		/// this gallery. This parameter is not required <paramref name="securityActions"/> is SecurityActions.AdministerSite (you can specify
		/// <see cref="int.MinValue"/>).</param>
		/// <param name="isPrivate">Indicates whether the specified album is private (hidden from anonymous users). The parameter
		/// is ignored for logged on users.</param>
		/// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
		/// to be successful or just one.</param>
		/// <returns>
		/// Returns true when the user is authorized to perform the specified security action against the specified album;
		/// otherwise returns false.
		/// </returns>
		public static bool IsUserAuthorized(SecurityActions securityActions, int albumId, int galleryId, bool isPrivate, SecurityActionsOption secActionsOption)
		{
			return IsUserAuthorized(securityActions, RoleController.GetGalleryServerRolesForUser(), albumId, galleryId, isPrivate, secActionsOption);
		}

		/// <summary>
		/// Determine whether user has permission to perform at least one of the specified security actions. Un-authenticated users
		/// (anonymous users) are always considered NOT authorized (that is, this method returns false) except when the requested 
		/// security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or <see cref="SecurityActions.ViewOriginalImage" />, 
		/// since Gallery Server is configured by default to allow anonymous viewing access
		/// but it does not allow anonymous editing of any kind. This method will continue to work correctly if the webmaster configures
		/// Gallery Server to require users to log in in order to view objects, since at that point there will be no such thing as
		/// un-authenticated users, and the standard gallery server role functionality applies.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
		/// a bitwise OR between them (example: SecurityActions.AdministerSite | SecurityActions.AdministerGallery). If multiple actions are 
		/// specified, the method is successful if the user has permission for at least one of the actions. If you require that all actions 
		/// be satisfied to be successful, call one of the overloads that accept a SecurityActionsOption and 
		/// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
		/// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. This parameter is ignored
		/// 	for anonymous users. The parameter may be null.</param>
		/// <param name="albumId">The album ID to which the security action applies.</param>
		/// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist in 
		/// this gallery. This parameter is not required <paramref name="securityActions" /> is SecurityActions.AdministerSite (you can specify 
		/// <see cref="int.MinValue" />).</param>
		/// <param name="isPrivate">Indicates whether the specified album is private (hidden from anonymous users). The parameter
		/// 	is ignored for logged on users.</param>
		/// <returns>
		/// Returns true when the user is authorized to perform the specified security action against the specified album;
		/// otherwise returns false.
		/// </returns>
		public static bool IsUserAuthorized(SecurityActions securityActions, IGalleryServerRoleCollection roles, int albumId, int galleryId, bool isPrivate)
		{
			return IsUserAuthorized(securityActions, roles, albumId, galleryId, isPrivate, SecurityActionsOption.RequireOne);
		}

		/// <summary>
		/// Determine whether user has permission to perform the specified security actions. When multiple security actions are passed, use 
		/// <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied to be successful or only one item
		/// must be satisfied. Un-authenticated users (anonymous users) are always considered NOT authorized (that is, this method returns 
		/// false) except when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or 
		/// <see cref="SecurityActions.ViewOriginalImage" />, since Gallery Server is configured by default to allow anonymous viewing access
		/// but it does not allow anonymous editing of any kind. This method will continue to work correctly if the webmaster configures
		/// Gallery Server to require users to log in in order to view objects, since at that point there will be no such thing as
		/// un-authenticated users, and the standard gallery server role functionality applies.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
		/// a bitwise OR between them (example: SecurityActions.AdministerSite | SecurityActions.AdministerGallery). If multiple actions are
		/// specified, use <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied to be successful or 
		/// only one item must be satisfied.</param>
		/// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. This parameter is ignored
		/// 	for anonymous users. The parameter may be null.</param>
		/// <param name="albumId">The album ID to which the security action applies.</param>
		/// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist in 
		/// this gallery. This parameter is not required <paramref name="securityActions" /> is SecurityActions.AdministerSite (you can specify 
		/// <see cref="int.MinValue" />).</param>
		/// <param name="isPrivate">Indicates whether the specified album is private (hidden from anonymous users). The parameter
		/// 	is ignored for logged on users.</param>
		/// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
		/// to be successful or just one.</param>
		/// <returns>
		/// Returns true when the user is authorized to perform the specified security action against the specified album;
		/// otherwise returns false.
		/// </returns>
		public static bool IsUserAuthorized(SecurityActions securityActions, IGalleryServerRoleCollection roles, int albumId, int galleryId, bool isPrivate, SecurityActionsOption secActionsOption)
		{
			return SecurityManager.IsUserAuthorized(securityActions, roles, albumId, galleryId, IsAuthenticated, isPrivate, secActionsOption);
		}

		/// <summary>
		/// Determine whether the user belonging to the specified <paramref name="roles" /> is a site administrator. The user is considered a site
		/// administrator if at least one role has Allow Administer Site permission.
		/// </summary>
		/// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. The parameter may be null.</param>
		/// <returns>
		/// 	<c>true</c> if the user is a site administrator; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsUserSiteAdministrator(IGalleryServerRoleCollection roles)
		{
			return SecurityManager.IsUserSiteAdministrator(roles);
		}

		/// <summary>
		/// Determine whether the user belonging to the specified <paramref name="roles"/> is a gallery administrator for the specified
		/// <paramref name="galleryId"/>. The user is considered a gallery administrator if at least one role has Allow Administer Gallery permission.
		/// </summary>
		/// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. The parameter may be null.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>
		/// 	<c>true</c> if the user is a gallery administrator; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsUserGalleryAdministrator(IGalleryServerRoleCollection roles, int galleryId)
		{
			return SecurityManager.IsUserGalleryAdministrator(roles, galleryId);
		}

		/// <summary>
		/// Determine whether the currently logged-on user is a site administrator. The user is considered a site
		/// administrator if at least one role has Allow Administer Site permission.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if the user is a site administrator; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsCurrentUserSiteAdministrator()
		{
			return IsUserSiteAdministrator(RoleController.GetGalleryServerRolesForUser());
		}

		/// <summary>
		/// Determine whether the currently logged-on user is a gallery administrator for the specified <paramref name="galleryId"/>. 
		/// The user is considered a gallery administrator if at least one role has Allow Administer Gallery permission.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>
		/// 	<c>true</c> if the user is a gallery administrator; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsCurrentUserGalleryAdministrator(int galleryId)
		{
			return SecurityManager.IsUserGalleryAdministrator(RoleController.GetGalleryServerRolesForUser(), galleryId);
		}

		/// <summary>
		/// Determine the trust level of the currently running application.
		/// </summary>
		/// <returns>Returns the trust level of the currently running application.</returns>
		public static ApplicationTrustLevel GetCurrentTrustLevel()
		{
			AspNetHostingPermissionLevel aspnetTrustLevel = AspNetHostingPermissionLevel.None;

			foreach (AspNetHostingPermissionLevel aspnetTrustLevelIterator in
				new AspNetHostingPermissionLevel[] {
				                                   	AspNetHostingPermissionLevel.Unrestricted,
				                                   	AspNetHostingPermissionLevel.High,
				                                   	AspNetHostingPermissionLevel.Medium,
				                                   	AspNetHostingPermissionLevel.Low,
				                                   	AspNetHostingPermissionLevel.Minimal 
				                                   })
			{
				try
				{
					new AspNetHostingPermission(aspnetTrustLevelIterator).Demand();
					aspnetTrustLevel = aspnetTrustLevelIterator;
					break;
				}
				catch (SecurityException)
				{
					continue;
				}
			}

			ApplicationTrustLevel trustLevel = ApplicationTrustLevel.None;

			switch (aspnetTrustLevel)
			{
				case AspNetHostingPermissionLevel.Minimal: trustLevel = ApplicationTrustLevel.Minimal; break;
				case AspNetHostingPermissionLevel.Low: trustLevel = ApplicationTrustLevel.Low; break;
				case AspNetHostingPermissionLevel.Medium: trustLevel = ApplicationTrustLevel.Medium; break;
				case AspNetHostingPermissionLevel.High: trustLevel = ApplicationTrustLevel.High; break;
				case AspNetHostingPermissionLevel.Unrestricted: trustLevel = ApplicationTrustLevel.Full; break;
				default: trustLevel = ApplicationTrustLevel.Unknown; break;
			}

			return trustLevel;
		}

		/// <summary>
		/// Get the path, relative to the web site root, to the specified resource. Example: If the web application is at
		/// /dev/gsweb/, the directory containing the resources is /gs/, and the desired resource is /images/info.gif, this function
		/// will return /dev/gsweb/gs/images/info.gif.
		/// </summary>
		/// <param name="resource">A path relative to the directory containing the Gallery Server Pro resource files (ex: images/info.gif).
		/// The leading forward slash ('/') is optional.</param>
		/// <returns>Returns the path, relative to the web site root, to the specified resource.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="resource" /> is null.</exception>
		public static string GetUrl(string resource)
		{
			if (resource == null)
				throw new ArgumentNullException("resource");

			if (!resource.StartsWith("/", StringComparison.Ordinal))
				resource = resource.Insert(0, "/"); // Make sure it starts with a '/'

			resource = String.Concat(GalleryRoot, resource);

			//#if DEBUG
			//      if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath(resource)))
			//        throw new System.IO.FileNotFoundException(String.Format(CultureInfo.CurrentCulture, "No file exists at {0}.", resource), resource);
			//#endif

			return resource;
		}

		/// <overloads>Get an URL relative to the application root for the requested page.</overloads>
		/// <summary>
		/// Get an URL relative to the application root for the requested <paramref name="page"/>. Example: If 
		/// <paramref name="page"/> is PageId.album and the current page is /dev/gs/gallery.aspx, this function 
		/// returns /dev/gs/gallery.aspx?g=album. Returns null if <see cref="HttpContext.Current" /> is null.
		/// </summary>
		/// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="GalleryPage"/>.</param>
		/// <returns>Returns an URL relative to the application root for the requested <paramref name="page"/>, or null 
		/// if <see cref="HttpContext.Current" /> is null.</returns>
		public static string GetUrl(PageId page)
		{
			if (HttpContext.Current == null)
				return null;

			return AddQueryStringParameter(GetCurrentPageUrl(), String.Concat("g=", page));
		}

		/// <summary>
		/// Get an URL relative to the application root for the requested <paramref name="page"/> and with the specified 
		/// <paramref name="args"/> appended as query string parameters. Example: If <paramref name="page"/> is PageId.task_addobjects, 
		/// the current page is /dev/gs/gallery.aspx, <paramref name="format"/> is "aid={0}", and <paramref name="args"/>
		/// is "23", this function returns /dev/gs/gallery.aspx?g=task_addobjects&amp;aid=23. If the <paramref name="page"/> is
		/// <see cref="PageId.album"/> or <see cref="PageId.mediaobject"/>, don't include the "g" query string parameter, since 
		/// we can deduce it by looking for the aid or moid query string parms. Returns null if <see cref="HttpContext.Current" /> is null.
		/// </summary>
		/// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="GalleryPage"/>.</param>
		/// <param name="format">A format string whose placeholders are replaced by values in <paramref name="args"/>. Do not use a '?'
		/// or '&amp;' at the beginning of the format string. Example: "msg={0}".</param>
		/// <param name="args">The values to be inserted into the <paramref name="format"/> string.</param>
		/// <returns>Returns an URL relative to the application root for the requested <paramref name="page"/>, or 
		/// null if <see cref="HttpContext.Current" /> is null.</returns>
		public static string GetUrl(PageId page, string format, params object[] args)
		{
			if (HttpContext.Current == null)
				return null;

			string queryString = String.Format(CultureInfo.InvariantCulture, format, args);

			if ((page != PageId.album) && (page != PageId.mediaobject))
			{
				// Don't use the "g" parameter for album or mediaobject pages, since we can deduce it by looking for the 
				// aid or moid query string parms. This results in a shorter, cleaner URL.
				queryString = String.Concat("g=", page, "&", queryString);
			}

			return AddQueryStringParameter(GetCurrentPageUrl(), queryString);
		}

		/// <summary>
		/// Get the physical path to the <paramref name="resource"/>. Example: If the web application is at
		/// C:\inetpub\wwwroot\dev\gsweb\, the directory containing the resources is \gs\, and the desired resource is
		/// /templates/AdminNotificationAccountCreated.txt, this function will return 
		/// C:\inetpub\wwwroot\dev\gsweb\gs\templates\AdminNotificationAccountCreated.txt.
		/// </summary>
		/// <param name="resource">A path relative to the directory containing the Gallery Server Pro resource files (ex: images/info.gif).
		/// The slash may be forward (/) or backward (\), although there is a slight performance improvement if it is forward (/).
		/// The parameter does not require a leading slash, although there is a slight performance improvement if it is present.</param>
		/// <returns>Returns the physical path to the requested <paramref name="resource"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="resource" /> is null.</exception>
		public static string GetPath(string resource)
		{
			if (resource == null)
				throw new ArgumentNullException("resource");

			// Convert back slash (\) to forward slash, if present.
			resource = resource.Replace(Path.DirectorySeparatorChar, '/');

			return HttpContext.Current.Server.MapPath(GetUrl(resource));
		}

		/// <summary>
		/// Gets the URI of the current page request. Automatically handles port forwarding configurations by incorporating the port in the
		/// HTTP_HOST server variable in the URI. Ex: "http://75.135.92.12:8080/dev/gs/default.aspx?moid=770"
		/// Returns null if <see cref="HttpContext.Current" /> is null.
		/// </summary>
		/// <returns>Returns the URI of the current page request, or null if <see cref="HttpContext.Current" /> is null.</returns>
		public static Uri GetCurrentPageUri()
		{
			if (HttpContext.Current == null)
				return null;

			UriBuilder uriBuilder = new UriBuilder(HttpContext.Current.Request.Url);
			uriBuilder.Host = GetHostName();
			int? port = GetPort();
			if (port.HasValue)
			{
				uriBuilder.Port = port.Value;
			}

			return uriBuilder.Uri;
		}

		/// <overloads>
		/// Gets the URL, relative to the application root, to the current page.
		/// </overloads>
		/// <summary>
		/// Gets the URL, relative to the application root and without any query string parameters, to the current page.
		/// This method is a wrapper for a call to HttpContext.Current.Request.Url. Returns null if <see cref="HttpContext.Current" /> 
		/// is null. Example: "/dev/gs/gallery.aspx"
		/// </summary>
		/// <returns>Returns the URL, relative to the application root and without any query string parameters, to the current page,
		/// or null if <see cref="HttpContext.Current" /> is null.</returns>
		public static string GetCurrentPageUrl()
		{
			return GetCurrentPageUrl(false);
		}

		/// <summary>
		/// Gets the URL, relative to the application root and optionally including any query string parameters, to the current page.
		/// This method is a wrapper for a call to HttpContext.Current.Request.Url. Returns null if <see cref="HttpContext.Current" /> 
		/// is null. Examples: "/dev/gs/gallery.aspx", "/dev/gs/gallery.aspx?g=admin_email&amp;aid=2389" 
		/// </summary>
		/// <param name="includeQueryString">When <c>true</c> the query string is included.</param>
		/// <returns>Returns the URL, relative to the application root and including any query string parameters, to the current page,
		/// or null if <see cref="HttpContext.Current" /> is null.</returns>
		public static string GetCurrentPageUrl(bool includeQueryString)
		{
			if (HttpContext.Current == null)
				return null;

			if (includeQueryString)
				return String.Concat(HttpContext.Current.Request.Url.AbsolutePath, HttpContext.Current.Request.Url.Query);
			else
				return HttpContext.Current.Request.Url.AbsolutePath;
		}

		/// <summary>
		/// Get the full path to the current web page. Does not include any query string parms. Returns null if 
		/// <see cref="HttpContext.Current" /> is null. Example: "http://www.techinfosystems.com/gs/default.aspx"
		/// </summary>
		/// <returns>Returns the full path to the current web page, or null if <see cref="HttpContext.Current" /> is null.</returns>
		/// <remarks>This value is retrieved from the user's session. If not present in the session, such as when the user first arrives, it
		/// is calculated by passing the appropriate pieces from HttpContext.Current.Request.Url and HttpContext.Current.Request.FilePath
		/// to a UriBuilder object. The path is calculated on a per-user basis because the URL may be different for different users 
		/// (a local admin's URL may be http://localhost/gs/default.aspx, someone on the intranet may get the server's name
		/// (http://Server1/gs/default.aspx), and someone on the internet may get the full name (http://www.bob.com/gs/default.aspx).</remarks>
		public static string GetCurrentPageUrlFull()
		{
			if (HttpContext.Current == null)
				return null;

			string appRootUrl = null;

			if (HttpContext.Current.Session != null)
			{
				object appRootUrlSession = HttpContext.Current.Session["PageUrl"];
				if (appRootUrlSession != null)
					appRootUrl = appRootUrlSession.ToString();
			}

			if (String.IsNullOrEmpty(appRootUrl))
			{
				// Calculate the URL.
				appRootUrl = String.Concat(GetHostUrl(), GetCurrentPageUrl());

				if (HttpContext.Current.Session != null)
					HttpContext.Current.Session["PageUrl"] = appRootUrl;
			}

			return appRootUrl;
		}

		/// <summary>
		/// Get the URI scheme, DNS host name or IP address, and port number for the current application. 
		/// Examples: http://www.site.com, http://localhost, http://127.0.0.1, http://godzilla
		/// Returns null if <see cref="HttpContext.Current" /> is null.
		/// </summary>
		/// <returns>Returns the URI scheme, DNS host name or IP address, and port number for the current application, 
		/// or null if <see cref="HttpContext.Current" /> is null.</returns>
		/// <remarks>This value is retrieved from the user's session. If not present in the session, such as when the user first arrives, it
		/// is calculated by parsing the appropriate pieces from HttpContext.Current.Request.Url and the HTTP_HOST server variable. The path is 
		/// calculated on a per-user basis because the URL may be different for different users (a local admin's URL may be 
		/// http://localhost, someone on the intranet may get the server's name (http://Server1), and someone on the internet may get 
		/// the full name (http://www.site.com).</remarks>
		public static string GetHostUrl()
		{
			if (HttpContext.Current == null)
				return null;

			string hostUrl = null;

			if (HttpContext.Current.Session != null)
			{
				hostUrl = (String)HttpContext.Current.Session["HostUrl"];
			}

			if (String.IsNullOrEmpty(hostUrl))
			{
				hostUrl = String.Concat(HttpContext.Current.Request.Url.Scheme, "://", GetHostNameAndPort());

				if (HttpContext.Current.Session != null)
					HttpContext.Current.Session["HostUrl"] = hostUrl;
			}

			return hostUrl;
		}

		/// <summary>
		/// Gets the URL to the current web application. Does not include the containing page or the trailing slash. 
		/// Guaranteed to not return null. Example: If the gallery is installed in a virtual directory 'gallery'
		/// on domain 'www.site.com', this returns 'http://www.site.com/gallery'.
		/// </summary>
		/// <returns>Returns the URL to the current web application.</returns>
		public static string GetAppUrl()
		{
			return String.Concat(GetHostUrl(), AppRoot);
		}

		/// <summary>
		/// Gets the full URL to the directory containing the gallery resources. Does not include the containing page or 
		/// the trailing slash. Guaranteed to not return null. Example: If the gallery is installed in a virtual directory 'gallery'
		/// on domain 'www.site.com' and the resources are in directory 'gs', this returns 'http://www.site.com/gallery/gs'.
		/// </summary>
		/// <returns>Returns the full URL to the directory containing the gallery resources.</returns>
		public static string GetGalleryResourcesUrl()
		{
			return String.Concat(GetHostUrl(), GalleryRoot);
		}

		/// <summary>
		/// Gets the Domain Name System (DNS) host name or IP address and the port number for the current web application. Includes the
		/// port number if it differs from the default port. The value is generated from the HTTP_HOST server variable if present; 
		/// otherwise HttpContext.Current.Request.Url.Authority is used. Ex: "www.site.com", "www.site.com:8080", "192.168.0.50", "75.135.92.12:8080"
		/// </summary>
		/// <returns>A <see cref="String" /> containing the authority component of the URI for the current web application.</returns>
		/// <remarks>This function correctly handles configurations where the web application is port forwarded through a router. For 
		/// example, if the router is configured to map incoming requests at www.site.com:8080 to an internal IP 192.168.0.100:8056,
		/// this function returns "www.site.com:8080". This is accomplished by using the HTTP_HOST server variable rather than 
		/// HttpContext.Current.Request.Url.Authority (when HTTP_HOST is present).</remarks>
		public static string GetHostNameAndPort()
		{
			string httpHost = HttpContext.Current.Request.ServerVariables["HTTP_HOST"];

			return (!String.IsNullOrEmpty(httpHost) ? httpHost : HttpContext.Current.Request.Url.Authority);
		}

		/// <summary>
		/// Gets the host name for the current request. Does not include port number or scheme. The value is generated from the 
		/// HTTP_HOST server variable if present; otherwise HttpContext.Current.Request.Url.Authority is used. 
		/// Ex: "www.site.com", "75.135.92.12"
		/// </summary>
		/// <returns>Returns the host name for the current request.</returns>
		public static string GetHostName()
		{
			string host = GetHostNameAndPort();

			return (host.IndexOf(":", StringComparison.Ordinal) < 0 ? host : host.Substring(0, host.IndexOf(":", StringComparison.Ordinal)));
		}

		/// <summary>
		/// Gets the port for the current request if one is specified; otherwise returns null. The value is generated from the 
		/// HTTP_HOST server variable if present; otherwise HttpContext.Current.Request.Url.Authority is used. 
		/// </summary>
		/// <returns>Returns the port for the current request if one is specified; otherwise returns null.</returns>
		public static int? GetPort()
		{
			string host = GetHostNameAndPort();

			if (host.IndexOf(":", StringComparison.Ordinal) >= 0)
			{
				string portString = host.Substring(host.IndexOf(":", StringComparison.Ordinal) + 1);

				int port;
				if (Int32.TryParse(portString, out port))
				{
					return port;
				}
			}

			return null;
		}

		/// <overloads>Redirects the user to the specified <paramref name="page"/>.</overloads>
		/// <summary>
		/// Redirects the user to the specified <paramref name="page"/>. The redirect occurs immediately.
		/// </summary>
		/// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="GalleryPage"/>.</param>
		public static void Redirect(PageId page)
		{
			HttpContext.Current.Response.Redirect(GetUrl(page), true);
		}

		/// <summary>
		/// Redirects the user, using Response.Redirect, to the specified <paramref name="page"/>. If <paramref name="endResponse"/> is true, the redirect occurs 
		/// when the page has finished processing all events. When false, the redirect occurs immediately.
		/// </summary>
		/// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="GalleryPage"/>.</param>
		/// <param name="endResponse">When <c>true</c> the redirect occurs immediately. When false, the redirect is delayed until the
		/// page processing is complete.</param>
		public static void Redirect(PageId page, bool endResponse)
		{
			HttpContext.Current.Response.Redirect(GetUrl(page), endResponse);
		}

		/// <summary>
		/// Redirects the user, using Response.Redirect, to the specified <paramref name="page"/> and with the specified 
		/// <paramref name="args"/> appended as query string parameters. Example: If <paramref name="page"/> is PageId.album, 
		/// the current page is /dev/gs/gallery.aspx, <paramref name="format"/> is "aid={0}", and <paramref name="args"/>
		/// is "23", this function redirects to /dev/gs/gallery.aspx?g=album&amp;aid=23.
		/// </summary>
		/// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="GalleryPage"/>.</param>
		/// <param name="format">A format string whose placeholders are replaced by values in <paramref name="args"/>. Do not use a '?'
		/// or '&amp;' at the beginning of the format string. Example: "msg={0}".</param>
		/// <param name="args">The values to be inserted into the <paramref name="format"/> string.</param>
		public static void Redirect(PageId page, string format, params object[] args)
		{
			HttpContext.Current.Response.Redirect(GetUrl(page, format, args), true);
		}

		/// <summary>
		/// Redirects the user, using Response.Redirect, to the specified <paramref name="url"/>
		/// </summary>
		/// <param name="url">The URL to redirect the user to.</param>
		public static void Redirect(string url)
		{
			HttpContext.Current.Response.Redirect(url, true);
		}

		/// <summary>
		/// Transfers the user, using Server.Transfer, to the specified <paramref name="page"/>.
		/// </summary>
		/// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="GalleryPage"/>.</param>
		public static void Transfer(PageId page)
		{
			try
			{
				HttpContext.Current.Server.Transfer(GetUrl(page));
			}
			catch (ThreadAbortException) { }
		}

		/// <summary>
		/// Redirects the user to the specified <paramref name="page"/> and with the specified 
		/// <paramref name="args"/> appended as query string parameters. Example: If <paramref name="page"/> is PageId.album, 
		/// the current page is /dev/gs/gallery.aspx, <paramref name="format"/> is "aid={0}", and <paramref name="args"/>
		/// is "23", this function redirects to /dev/gs/gallery.aspx?g=album&amp;aid=23.
		/// </summary>
		/// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="GalleryPage"/>.</param>
		/// <param name="endResponse">When <c>true</c> the redirect occurs immediately. When false, the redirect is delayed until the
		/// page processing is complete.</param>
		/// <param name="format">A format string whose placeholders are replaced by values in <paramref name="args"/>. Do not use a '?'
		/// or '&amp;' at the beginning of the format string. Example: "msg={0}".</param>
		/// <param name="args">The values to be inserted into the <paramref name="format"/> string.</param>
		public static void Redirect(PageId page, bool endResponse, string format, params object[] args)
		{
			HttpContext.Current.Response.Redirect(GetUrl(page, format, args), endResponse);
			HttpContext.Current.ApplicationInstance.CompleteRequest();
		}

		/// <summary>
		/// Gets detailed information about the <paramref name="ex"/> that can be presented to an administrator. This is essentially 
		/// a string that combines the exception type with its message. It recursively checks for an InnerException and appends that 
		/// type and message if present. It does not include stack trace or other information. Callers to this method should ensure 
		/// that this information is shown to the user only if he or she is a system administrator and/or the ShowErrorDetails setting 
		/// of the configuration file to true.
		/// </summary>
		/// <param name="ex">The exception for which detailed information is to be returned.</param>
		/// <returns>Returns detailed information about the <paramref name="ex"/> that can be presented to an administrator.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ex" /> is null.</exception>
		public static string GetExceptionDetails(Exception ex)
		{
			if (ex == null)
				throw new ArgumentNullException("ex");

			string exMsg = String.Concat(ex.GetType(), ": ", ex.Message);
			Exception innerException = ex.InnerException;
			while (innerException != null)
			{
				exMsg += String.Concat(" ", innerException.GetType(), ": ", innerException.Message);
				innerException = innerException.InnerException;
			}

			return exMsg;
		}

		/// <summary>
		/// Retrieves the specified query string parameter value from the query string. Returns int.MinValue if
		/// the parameter is not found, it is not a valid integer, or it is &lt;= 0.
		/// </summary>
		/// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
		/// <returns>Returns the value of the specified query string parameter.</returns>
		public static int GetQueryStringParameterInt32(string parameterName)
		{
			string parm = HttpContext.Current.Request.QueryString[parameterName];

			int qsValue;
			if (Int32.TryParse(parm, out qsValue) && (qsValue >= 0))
			{
				return qsValue;
			}
			else
			{
				return Int32.MinValue;
			}
		}

		/// <summary>
		/// Retrieves the specified query string parameter value from the query string. If no URI is specified, the current 
		/// request URL is used. Returns int.MinValue if the parameter is not found, it is not a valid integer, or it is &lt;= 0.
		/// </summary>
		/// <param name="uri">The URI containing the query string parameter to retrieve.</param>
		/// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
		/// <returns>Returns the value of the specified query string parameter.</returns>
		public static int GetQueryStringParameterInt32(Uri uri, string parameterName)
		{
			string parm = null;
			if (uri == null)
			{
				parm = HttpContext.Current.Request.QueryString[parameterName];
			}
			else
			{
				string qs = uri.Query.TrimStart(new char[] { '?' });
				foreach (string nameValuePair in qs.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
				{
					string[] nameValue = nameValuePair.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
					if (nameValue.Length > 1)
					{
						if (String.Equals(nameValue[0], parameterName))
						{
							parm = nameValue[1];
							break;
						}
					}
				}
			}

			if ((String.IsNullOrEmpty(parm)) || (!HelperFunctions.IsInt32(parm) || (Convert.ToInt32(parm, CultureInfo.InvariantCulture) <= 0)))
			{
				return Int32.MinValue;
			}
			else
			{
				return Convert.ToInt32(parm, CultureInfo.InvariantCulture);
			}
		}

		/// <summary>
		/// Retrieves the specified query string parameter value from the query string. Returns string.Empty 
		/// if the parameter is not found.
		/// </summary>
		/// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
		/// <returns>Returns the value of the specified query string parameter.</returns>
		/// <remarks>Do not call UrlDecode on the string, as it appears that .NET already does this.</remarks>
		public static string GetQueryStringParameterString(string parameterName)
		{
			return HttpContext.Current.Request.QueryString[parameterName] ?? String.Empty;
		}

		/// <summary>
		/// Retrieves the specified query string parameter value from the specified <paramref name="uri"/>. Returns 
		/// string.Empty if the parameter is not found.
		/// </summary>
		/// <param name="uri">The URI to search.</param>
		/// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
		/// <returns>Returns the value of the specified query string parameter found in the <paramref name="uri"/>.</returns>
		public static string GetQueryStringParameterString(Uri uri, string parameterName)
		{
			string parm = null;
			if (uri == null)
			{
				parm = HttpContext.Current.Request.QueryString[parameterName];
			}
			else
			{
				string qs = uri.Query.TrimStart(new char[] { '?' });
				foreach (string nameValuePair in qs.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
				{
					string[] nameValue = nameValuePair.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
					if (nameValue.Length > 1)
					{
						if (String.Equals(nameValue[0], parameterName))
						{
							parm = nameValue[1];
							break;
						}
					}
				}
			}

			if (parm == null)
			{
				return String.Empty;
			}
			else
			{
				return parm;
			}
		}

		/// <summary>
		/// Retrieves the specified query string parameter value from the query string. The values "true" and "1"
		/// are returned as true; any other value is returned as false. It is not case sensitive. The bool is not
		/// set if the parameter is not present in the query string (i.e. the HasValue property is false).
		/// </summary>
		/// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
		/// <returns>Returns the value of the specified query string parameter.</returns>
		public static bool? GetQueryStringParameterBoolean(string parameterName)
		{
			bool? parmValue = null;

			object parm = HttpContext.Current.Request.QueryString[parameterName];

			if (parm != null)
			{
				if ((parm.ToString().Equals("1", StringComparison.Ordinal)) || (parm.ToString().Equals("TRUE", StringComparison.OrdinalIgnoreCase)))
				{
					parmValue = true;
				}
				else
				{
					parmValue = false;
				}
			}

			return parmValue;
		}

		/// <summary>
		/// Append the string to the url as a query string parameter. If the <paramref name="url" /> already contains the
		/// specified query string parameter, it is replaced with the new one.
		/// Example:
		/// Url = "www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3"
		/// QueryStringParameterNameValue = "moid=27"
		/// Return value: www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3&amp;moid=27
		/// </summary>
		/// <param name="url">The Url to which the query string parameter should be added
		/// (e.g. www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3).</param>
		/// <param name="queryStringParameterNameValue">The query string parameter and value to add to the Url
		/// (e.g. "moid=27").</param>
		/// <returns>Returns a new Url containing the specified query string parameter.</returns>
		public static string AddQueryStringParameter(string url, string queryStringParameterNameValue)
		{
			if (String.IsNullOrEmpty(queryStringParameterNameValue))
				return url;

			string parmName = queryStringParameterNameValue.Substring(0, queryStringParameterNameValue.IndexOf("=", StringComparison.Ordinal));

			url = RemoveQueryStringParameter(url, parmName);

			string rv = url;

			if (url.IndexOf("?", StringComparison.Ordinal) < 0)
			{
				rv += "?" + queryStringParameterNameValue;
			}
			else
			{
				rv += "&" + queryStringParameterNameValue;
			}
			return rv;
		}


		/// <overloads>
		/// Remove a query string parameter from an URL.
		/// </overloads>
		/// <summary>
		/// Remove all query string parameters from the url.
		/// Example:
		/// Url = "www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3&amp;moid=27"
		/// Return value: www.galleryserverpro.com/index.aspx
		/// </summary>
		/// <param name="url">The Url containing the query string parameters to remove
		/// (e.g. www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3&amp;moid=27).</param>
		/// <returns>Returns a new Url with all query string parameters removed.</returns>
		public static string RemoveQueryStringParameter(string url)
		{
			return RemoveQueryStringParameter(url, String.Empty);
		}

		/// <summary>
		/// Remove the specified query string parameter from the url. Specify <see cref="String.Empty" /> for the
		/// <paramref name="queryStringParameterName" /> parameter to remove the entire set of parameters.
		/// Example:
		/// Url = "www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3&amp;moid=27"
		/// QueryStringParameterName = "msg"
		/// Return value: www.galleryserverpro.com/index.aspx?aid=5&amp;moid=27
		/// </summary>
		/// <param name="url">The Url containing the query string parameter to remove
		/// (e.g. www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3&amp;moid=27).</param>
		/// <param name="queryStringParameterName">The query string parameter name to remove from the Url
		/// (e.g. "msg"). Specify <see cref="String.Empty" /> to remove the entire set of parameters.</param>
		/// <returns>Returns a new Url with the specified query string parameter removed.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="url" /> is null.</exception>
		public static string RemoveQueryStringParameter(string url, string queryStringParameterName)
		{
			if (url == null)
				throw new ArgumentNullException("url");

			string newUrl;

			// Get the location of the question mark so we can separate the base url from the query string
			int separator = url.IndexOf("?", StringComparison.Ordinal);

			if (separator < 0)
			{
				// No query string exists on the url. Simply return the original url.
				newUrl = url;
			}
			else
			{
				// We have a query string to remove. Separate the base url from the query string, and process the query string.

				// Get the base url (e.g. "www.galleryserverpro.com/index.aspx")
				newUrl = url.Substring(0, separator);

				if (String.IsNullOrEmpty(queryStringParameterName))
				{
					return newUrl;
				}

				newUrl += "?";

				string queryString = url.Substring(separator + 1);

				if (queryString.Length > 0)
				{
					// Url has a query string. Split each name/value pair into a string array, and rebuild the
					// query string, leaving out the parm passed to the function.
					string[] queryItems = queryString.Split(new char[] { '&' });

					for (int i = 0; i < queryItems.Length; i++)
					{
						if (!queryItems[i].StartsWith(queryStringParameterName, StringComparison.OrdinalIgnoreCase))
						{
							// Query parm doesn't match, so include it as we rebuilt the new query string
							newUrl += String.Concat(queryItems[i], "&");
						}
					}
				}
				// Trim any trailing '&' or '?'.
				newUrl = newUrl.TrimEnd(new char[] { '&', '?' });
			}

			return newUrl;
		}

		/// <summary>
		/// Returns a value indicating whether the specified query string parameter name is part of the query string. 
		/// </summary>
		/// <param name="parameterName">The name of the query string parameter to check for.</param>
		/// <returns>Returns true if the specified query string parameter value is part of the query string; otherwise 
		/// returns false. </returns>
		public static bool IsQueryStringParameterPresent(string parameterName)
		{
			return (HttpContext.Current.Request.QueryString[parameterName] != null);
		}

		/// <summary>
		/// Returns a value indicating whether the specified query string parameter name is part of the query string
		/// of the <paramref name="uri"/>. 
		/// </summary>
		/// <param name="uri">The URI to check for the present of the <paramref name="parameterName">query string parameter name</paramref>.</param>
		/// <param name="parameterName">Name of the query string parameter.</param>
		/// <returns>Returns true if the specified query string parameter value is part of the query string; otherwise 
		/// returns false. </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="uri" /> is null.</exception>
		public static bool IsQueryStringParameterPresent(Uri uri, string parameterName)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			if (String.IsNullOrEmpty(parameterName))
				return false;

			return (uri.Query.Contains("?" + parameterName + "=") || uri.Query.Contains("&" + parameterName + "="));
		}

		/// <overloads>Remove all HTML tags from the specified string.</overloads>
		/// <summary>
		/// Remove all HTML tags from the specified string.
		/// </summary>
		/// <param name="html">The string containing HTML tags to remove.</param>
		/// <returns>Returns a string with all HTML tags removed.</returns>
		public static string RemoveHtmlTags(string html)
		{
			return RemoveHtmlTags(html, false);
		}

		/// <summary>
		/// Remove all HTML tags from the specified string. If <paramref name="escapeQuotes"/> is true, then all 
		/// apostrophes and quotation marks are replaced with &quot; and &apos; so that the string can be specified in HTML 
		/// attributes such as title tags. If the escapeQuotes parameter is not specified, no replacement is performed.
		/// </summary>
		/// <param name="html">The string containing HTML tags to remove.</param>
		/// <param name="escapeQuotes">When true, all apostrophes and quotation marks are replaced with &quot; and &apos;.</param>
		/// <returns>Returns a string with all HTML tags removed.</returns>
		public static string RemoveHtmlTags(string html, bool escapeQuotes)
		{
			return HtmlValidator.RemoveHtml(html, escapeQuotes);
		}

		/// <summary>
		/// Removes potentially dangerous HTML and Javascript in <paramref name="html"/>. If the configuration
		/// setting <see cref="IGallerySettings.AllowUserEnteredHtml" /> is true, then the input is cleaned so that all 
		/// HTML tags that are not in a predefined list are HTML-encoded and invalid HTML attributes are deleted. If 
		/// <see cref="IGallerySettings.AllowUserEnteredHtml" /> is false, then all HTML tags are deleted. If the setting 
		/// <see cref="IGallerySettings.AllowUserEnteredJavascript" /> is true, then script tags and the text "javascript:"
		/// is allowed. Note that if script is not in the list of valid HTML tags defined in <see cref="IGallerySettings.AllowedHtmlTags" />,
		/// it will be deleted even when <see cref="IGallerySettings.AllowUserEnteredJavascript" /> is true. When the setting 
		/// is false, all script tags and instances of the text "javascript:" are deleted.
		/// </summary>
		/// <param name="html">The string containing the HTML tags.</param>
		/// <param name="galleryId">The gallery ID. This is used to look up the appropriate configuration values for the gallery.</param>
		/// <returns>
		/// Returns a string with potentially dangerous HTML tags deleted.
		/// </returns>
		public static string CleanHtmlTags(string html, int galleryId)
		{
			return HtmlValidator.Clean(html, galleryId);
		}

		/// <summary>
		/// Returns the current version of Gallery Server.
		/// </summary>
		/// <returns>Returns a string representing the version (e.g. "1.0.0").</returns>
		public static string GetGalleryServerVersion()
		{
			string appVersion;
			object version = HttpContext.Current.Application["GalleryServerVersion"];
			if (version != null)
			{
				// Version was found in Application cache. Return.
				appVersion = version.ToString();
			}
			else
			{
				// Version was not found in application cache.
				appVersion = HelperFunctions.GetGalleryServerVersion();

				HttpContext.Current.Application["GalleryServerVersion"] = appVersion;
			}

			return appVersion;
		}

		/// <summary>
		/// Truncate the specified string to the desired length. Any HTML tags that exist in the beginning portion
		/// of the string are preserved as long as no HTML tags exist in the part that is truncated.
		/// </summary>
		/// <param name="text">The string to be truncated. It may contain HTML tags.</param>
		/// <param name="maxLength">The maximum length of the string to be returned. If HTML tags are returned,
		/// their length is not counted - only the length of the "visible" text is counted.</param>
		/// <returns>Returns a string whose length - not counting HTML tags - does not exceed the specified length.</returns>
		public static string TruncateTextForWeb(string text, int maxLength)
		{
			// Example 1: Because no HTML tags are present in the truncated portion of the string, the HTML at the
			// beginning is preserved. (We know we won't be splitting up HTML tags, so we don't mind including the HTML.)
			// text = "Meet my <a href='http://www.cnn.com'>friend</a>. He works at the YMCA."
			// maxLength = 20
			// returns: "Meet my <a href='http://www.cnn.com'>friend</a>. He w"
			//
			// Example 2: The truncated portion has <b> tags, so all HTML is stripped. (This function isn't smart
			// enough to know whether it might be truncating in the middle of a tag, so it takes the safe route.)
			// text = "Meet my <a href='http://www.cnn.com'>friend</a>. He works at the <b>YMCA<b>."
			// maxLength = 20
			// returns: "Meet my friend. He w"
			if (text == null)
				return String.Empty;

			if (text.Length < maxLength)
				return text;

			// Remove all HTML tags from entire string.
			string cleanText = RemoveHtmlTags(text);

			// If the clean text length is less than our maximum, return the raw text.
			if (cleanText.Length <= maxLength)
				return text;

			// Get the text that will be removed.
			string cleanTruncatedPortion = cleanText.Substring(maxLength);

			// If the clean truncated text doesn't match the end of the raw text, the raw text must have HTML tags.
			bool truncatedPortionHasHtml = (!(text.EndsWith(cleanTruncatedPortion, StringComparison.OrdinalIgnoreCase)));

			string truncatedText;
			if (truncatedPortionHasHtml)
			{
				// Since the truncated portion has HTML tags, and we don't want to risk returning malformed HTML,
				// return text without ANY HTML.
				truncatedText = cleanText.Substring(0, maxLength);
			}
			else
			{
				// Since the truncated portion does not have HTML tags, we can safely return the first part of the
				// string, even if it has HTML tags.
				truncatedText = text.Substring(0, text.Length - cleanTruncatedPortion.Length);
			}
			return truncatedText;
		}

		/// <summary>
		/// Generates a pseudo-random 24 character string that can be as an encryption key.
		/// </summary>
		/// <returns>A pseudo-random 24 character string that can be as an encryption key.</returns>
		public static string GenerateNewEncryptionKey()
		{
			const int encryptionKeyLength = 24;
			const int numberOfNonAlphaNumericCharactersInEncryptionKey = 3;
			string encryptionKey = Membership.GeneratePassword(encryptionKeyLength, numberOfNonAlphaNumericCharactersInEncryptionKey);

			// An ampersand (&) is invalid, since it is used as an escape character in XML files. Replace any instances with an 'X'.
			return encryptionKey.Replace("&", "X");
		}

		/// <summary>
		/// HtmlEncodes a string using System.Web.HttpUtility.HtmlEncode().
		/// </summary>
		/// <param name="html">The text to HTML encode.</param>
		/// <returns>Returns <paramref name="html"/> as an HTML-encoded string.</returns>
		public static string HtmlEncode(string html)
		{
			return HttpUtility.HtmlEncode(html);
		}

		/// <summary>
		/// HtmlDecodes a string using System.Web.HttpUtility.HtmlDecode().
		/// </summary>
		/// <param name="html">The text to HTML decode.</param>
		/// <returns>Returns <paramref name="html"/> as an HTML-decoded string.</returns>
		public static string HtmlDecode(string html)
		{
			return HttpUtility.HtmlDecode(html);
		}

		/// <overloads>UrlEncodes a string using System.Uri.EscapeDataString().</overloads>
		/// <summary>
		/// UrlEncodes a string using System.Uri.EscapeDataString().
		/// </summary>
		/// <param name="text">The text to URL encode.</param>
		/// <returns>Returns <paramref name="text"/> as an URL-encoded string.</returns>
		public static string UrlEncode(string text)
		{
			if (String.IsNullOrEmpty(text))
			{
				return text;
			}

			return Uri.EscapeDataString(text);
		}

		/// <summary>
		/// UrlEncodes a string using System.Uri.EscapeDataString(), excluding the character specified in <paramref name="charNotToEncode"/>.
		/// This overload is useful for encoding URLs or file paths where the forward or backward slash is not to be encoded.
		/// </summary>
		/// <param name="text">The text to URL encode</param>
		/// <param name="charNotToEncode">The character that, if present in <paramref name="text"/>, is not encoded.</param>
		/// <returns>Returns <paramref name="text"/> as an URL-encoded string.</returns>
		public static string UrlEncode(string text, char charNotToEncode)
		{
			if (String.IsNullOrEmpty(text))
			{
				return text;
			}

			string[] tokens = text.Split(new char[] { charNotToEncode });
			for (int i = 0; i < tokens.Length; i++)
			{
				tokens[i] = UrlEncode(tokens[i]);
			}

			return String.Join(charNotToEncode.ToString(), tokens);
		}

		/// <summary>
		/// UrlDecodes a string using System.Uri.UnescapeDataString().
		/// </summary>
		/// <param name="text">The text to URL decode.</param>
		/// <returns>Returns text as an URL-decoded string.</returns>
		public static string UrlDecode(string text)
		{
			if (String.IsNullOrEmpty(text))
				return text;

			// Pre-process for + sign space formatting since System.Uri doesn't handle it
			// plus literals are encoded as %2b normally so this should be safe.
			text = text.Replace("+", " ");
			return Uri.UnescapeDataString(text);
		}

		/// <summary>
		/// Force the current application to recycle by updating the last modified timestamp on web.config.
		/// </summary>
		/// <exception cref="FileNotFoundException">Thrown when the application incorrectly calculates the current application's
		/// web.config file location.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown when the application does not have write permission to the
		/// current application's web.config file.</exception>
		/// <exception cref="NotSupportedException">Thrown when the path to the web.config file as calculated by the application is
		/// in an invalid format.</exception>
		public static void ForceAppRecycle()
		{
			File.SetLastWriteTime(WebConfigFilePath, DateTime.Now);
		}

		/// <summary>
		/// Excecute a maintenance routine to help ensure data integrity and eliminate unused data. For example, roles are synchronized between
		/// the membership system and the GSP roles. Also, albums with owners that no longer exist are reset to not have an owner. This 
		/// method is intended to be called periodically; for example, once each time the application starts. Code in the Render
		/// method of the base class <see cref="GalleryPage" /> is responsible for knowing when and how to invoke this method.
		/// </summary>
		/// <remarks>The first iteration of the maintenace routine was invoked on a background thread, but background threads cannot access
		/// HttpContext.Current, which is required for the DotNetNuke implementation (and potential future versions of GSP's implementation),
		/// so that approach was replaced with this one.</remarks>
		public static void PerformMaintenance()
		{
			bool mustRunMaintenance = false;

			lock (_sharedLock)
			{
				if (AppSetting.Instance.MaintenanceStatus == MaintenanceStatus.NotStarted)
				{
					mustRunMaintenance = true;
					AppSetting.Instance.MaintenanceStatus = MaintenanceStatus.InProgress;
				}
			}

			if (mustRunMaintenance)
			{
				try
				{
					HelperFunctions.BeginTransaction();

					// Make sure the list of ASP.NET roles is synchronized with the Gallery Server roles.
					RoleController.ValidateRoles();

					MediaConversionQueue.Instance.DeleteOldQueueItems();

					HelperFunctions.CommitTransaction();

					AppSetting.Instance.MaintenanceStatus = MaintenanceStatus.Complete;
				}
				catch
				{
					HelperFunctions.RollbackTransaction();
					throw;
				}
			}
		}

		/// <summary>
		/// Gets the browser IDs for current request. In many cases this will be equal to HttpContext.Current.Request.Browser.Browsers.
		/// However, Internet Explorer versions 1 through 8 include the ID "ie1to8", which is added by Gallery Server Pro. This allows
		/// the application to treat those versions differently than later versions.
		/// </summary>
		/// <returns>Returns the browser IDs for current request.</returns>
		public static Array GetBrowserIdsForCurrentRequest()
		{
			ArrayList browserIds = HttpContext.Current.Request.Browser.Browsers ?? new ArrayList(new string[] { "default" });

			browserIds = AddBrowserIdForInternetExplorer(browserIds);

			return browserIds.ToArray();
		}

		/// <summary>
		/// Determines whether the <paramref name="url" /> is an absolute URL rather than a relative one. An URL is considered absolute if
		/// it starts with "http" or "//".
		/// </summary>
		/// <param name="url">The URL to check.</param>
		/// <returns>
		/// 	<c>true</c> if the <paramref name="url" /> is absolute; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsAbsoluteUrl(string url)
		{
			if (String.IsNullOrEmpty(url))
				return false;

			return (url.StartsWith("http", StringComparison.OrdinalIgnoreCase) || url.StartsWith("//", StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Gets the database file path from the connection string. Applies only to data providers that specify a file path
		/// in the connection string (SQLite, SQL CE). Returns null if no file path is found.
		/// </summary>
		/// <param name="cnString">The cn string.</param>
		/// <returns>Returns the full file path to the database file, or null if no file path is found.</returns>
		public static string GetDbFilePathFromConnectionString(string cnString)
		{
			// Ex: "data source=|DataDirectory|\GalleryServerPro_Data.sdf;Password =a@3!7f$dQ;"
			const string dataSourceKeyword = "data source";
			int dataSourceStartPos = cnString.IndexOf(dataSourceKeyword, StringComparison.OrdinalIgnoreCase) + dataSourceKeyword.Length + 1;

			if (dataSourceStartPos < 0)
				return null;

			int dataSourceLength = cnString.IndexOf(";", dataSourceStartPos, StringComparison.Ordinal) - dataSourceKeyword.Length;

			if (dataSourceLength < 0)
				dataSourceLength = cnString.Length - dataSourceStartPos;

			string cnFilePath = cnString.Substring(dataSourceStartPos, dataSourceLength).Replace("|DataDirectory|", "App_Data");

			string filePath = HelperFunctions.IsRelativeFilePath(cnFilePath) ? HttpContext.Current.Request.MapPath(cnFilePath) : cnFilePath;

			if (File.Exists(filePath))
				return filePath;
			else
				return null;
		}

		/// <summary>
		/// Serializes the <paramref name="item" /> as JSON. Uses the <see cref="DataContractJsonSerializer" /> class.
		/// </summary>
		/// <param name="item">The object to serialize.</param>
		/// <returns>Returns a string that is a JSON-encoded representation of <paramref name="item" />.</returns>
		/// <remarks>If the results of this function are to be sent to the browser in javascript, the slash (\)
		/// and aposrophe (') characters should be escaped. Do this by adding the following code the return
		/// value: <code>.Replace(@"\", @"\\").Replace("'", @"\'")</code>.</remarks>
		public static string ToJson(this object item)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer(item.GetType());
			MemoryStream ms = new MemoryStream();

			ser.WriteObject(ms, item);
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		/// <summary>
		/// Converts the specified <paramref name="json">JSON string</paramref> to the requested object.
		/// </summary>
		/// <typeparam name="T">The type of object to convert the JSON to.</typeparam>
		/// <param name="json">The JSON string to convert.</param>
		/// <returns>An instance of T.</returns>
		/// <exception cref="InvalidCastException">Thrown when the JSON string cannot be cast to the 
		/// requested type.</exception>
		public static T FromJson<T>(this string json)
		{
			using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
			{
				DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
				return (T)ser.ReadObject(ms);
			}
		}

		/// <summary>
		/// Adds the <paramref name="results" /> to the current user's session. If an object already exists,
		/// the results are added to the existing collection. No action is taken if the session is unavailable. 
		/// The session object is given the name stored in <see cref="GlobalConstants.SkippedFilesDuringUploadSessionKey" />.
		/// </summary>
		/// <param name="results">The results to store in the user's session.</param>
		public static void AddResultToSession(IEnumerable<ActionResult> results)
		{
			if (HttpContext.Current.Session == null)
				return;

			List<ActionResult> uploadResults = HttpContext.Current.Session[GlobalConstants.SkippedFilesDuringUploadSessionKey] as List<ActionResult> ?? new List<ActionResult>();

			lock (uploadResults)
			{
				uploadResults.AddRange(results);
				HttpContext.Current.Session[GlobalConstants.SkippedFilesDuringUploadSessionKey] = uploadResults;
			}
		}

		#endregion

		#region Private Static Methods

		/// <summary>
		/// Calculates the path, relative to the web site root, to the directory containing the Gallery Server Pro user 
		/// controls and other resources. Does not include the default page or the trailing slash. Ex: /dev/gsweb/gsp
		/// </summary>
		/// <returns>Returns the path to the directory containing the Gallery Server Pro user controls and other resources.</returns>
		private static string CalculateGalleryRoot()
		{
			string appPath = AppRoot;
			string galleryPath = GetGalleryResourcesPath().TrimEnd(new char[] { Path.DirectorySeparatorChar, '/' });

			if (!String.IsNullOrEmpty(galleryPath))
			{
				galleryPath = galleryPath.Replace("\\", "/");

				if (!galleryPath.StartsWith("/", StringComparison.Ordinal))
					galleryPath = String.Concat("/", galleryPath); // Make sure it starts with a '/'

				appPath = String.Concat(appPath, galleryPath.TrimEnd('/'));
			}

			return appPath;
		}

		/// <summary>
		/// Gets the path, relative to the current application, to the directory containing the Gallery Server Pro
		/// resources such as images, user controls, scripts, etc. This value is pulled from the galleryResourcesPath setting
		/// in the galleryServerPro/core section of web.config if present; otherwise it defaults to "gs". Examples: "gs", "GalleryServerPro\resources"
		/// The path is returned exactly how it appears in the configuration file.
		/// </summary>
		/// <returns>Returns the path, relative to the current application, to the directory containing the Gallery Server Pro
		/// resources such as images, user controls, scripts, etc.</returns>
		private static String GetGalleryResourcesPath()
		{
			string galleryResourcesPath = ConfigManager.GetGalleryServerProConfigSection().Core.GalleryResourcesPath;

			if (String.IsNullOrEmpty(galleryResourcesPath))
			{
				// The first time the user views the gallery after upgrading the files (but before running the Upgrade Wizard), 
				// the web.config file will not have the <core galleryResourcesPath="gs" /> setting required to calculate the 
				// paths to the files. When this happens, try to calculate it from the 2.3-style
				// <galleryServerPro configSource="gs\config\galleryserverpro.config" />, which should still exist. If this fails, 
				// that means no <galleryServerPro> element exists in web.config, which is a fatal error.
				galleryResourcesPath = GetGalleryPathFromWebConfig();
			}

			return galleryResourcesPath;
		}

		/// <summary>
		/// When the current browser is Internet Explorer 1 to 8, add a "ie1to8" element to <paramref name="browserIds" />.
		/// The return value is the same instance as the <paramref name="browserIds" /> parameter.
		/// </summary>
		/// <param name="browserIds">The browser IDs.</param>
		/// <returns>Returns the <paramref name="browserIds" /> parameter with an additional element added when the 
		/// browser is Internet Explorer 1 to 8.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="browserIds" /> is null.</exception>
		private static ArrayList AddBrowserIdForInternetExplorer(ArrayList browserIds)
		{
			if (browserIds == null)
				throw new ArgumentNullException("browserIds");

			HttpBrowserCapabilities browserCaps = HttpContext.Current.Request.Browser;

			if ((browserCaps != null) && (browserCaps.Browser != null) && browserCaps.Browser.Equals("IE", StringComparison.OrdinalIgnoreCase))
			{
				const string browserIdForIE1to8 = "ie1to8";
				decimal version;
				if (Decimal.TryParse(browserCaps.Version, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out version) && (version < (decimal)9.0) && (!browserIds.Contains(browserIdForIE1to8)))
				{
					browserIds.Add(browserIdForIE1to8);
				}
			}

			return browserIds;
		}

		/// <summary>
		/// Gets the path, relative to the current application, to the directory containing the Gallery Server Pro
		/// resources such as images, user controls, scripts, etc. The value is calculated based on the path to the
		/// galleryserverpro.config file specified in web.config. For example, if the config file is at "gs\config\galleryserverpro.config",
		/// then the path to the resources is "gs".
		/// </summary>
		/// <returns>Returns the path, relative to the current application, to the directory containing the Gallery Server Pro
		/// resources such as images, user controls, scripts, etc.</returns>
		/// <remarks>This method assumes that galleryserverpro.config is in a directory named "config" and that it is at
		/// the same directory level as the other folders, such as controls, handler, images, pages, script, etc. This
		/// assumption will be valid as long as Gallery Server Pro is always deployed with the entire contents of the "gs"
		/// directory as a single block.</remarks>
		private static string GetGalleryPathFromWebConfig()
		{
			string galleryServerProConfigPath = String.Empty;

			// Search web.config for <galleryServerPro configSource="..." />
			using (FileStream fs = new FileStream(HttpContext.Current.Server.MapPath("~/web.config"), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (StreamReader sr = new StreamReader(fs))
				{
					XmlReader r = XmlReader.Create(sr);
					while (r.Read())
					{
						if ((r.Name == "galleryServerPro") && r.MoveToAttribute("configSource"))
						{
							galleryServerProConfigPath = r.Value; // "gs\config\galleryserverpro.config"
							break;
						}
					}
				}
			}

			if (String.IsNullOrEmpty(galleryServerProConfigPath))
				throw new WebException("The web.config file for this application does not contain a <galleryServerPro ...> configuration element. This is required for Gallery Server Pro.");

			const string gallerySubPath = @"config\galleryserverpro.config";
			if (!galleryServerProConfigPath.EndsWith(gallerySubPath, StringComparison.Ordinal))
				throw new WebException(String.Format(CultureInfo.CurrentCulture, "The configuration file galleryserverpro.config must reside in a directory named config. The path discovered in web.config was {0}.", galleryServerProConfigPath));

			// Remove the "\config\galleryserverpro.config" from the path, so we are left with, for example, "gs".
			return galleryServerProConfigPath.Remove(galleryServerProConfigPath.IndexOf(gallerySubPath, StringComparison.Ordinal)).TrimEnd(new char[] { Path.DirectorySeparatorChar });
		}

		#endregion
	}
}
