using System;
using System.Globalization;
using System.Web;
using System.Web.Services;
using ComponentArt.Web.UI;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Handler
{
	/// <summary>
	/// Defines a handler that returns XML in a format that is consumable by the ComponentArt
	/// TreeView control. This can be called when a user clicks on a treeview node to dynamically
	/// load that node's contents.
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	public class gettreeviewxml : IHttpHandler
	{
		#region Private Fields

		private int _albumId;
		private SecurityActions _securityAction;
		private bool _showCheckbox;
		private string _navigateUrl;

		#endregion

		#region Public Methods

		/// <summary>
		/// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
		/// </summary>
		/// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
		public void ProcessRequest(HttpContext context)
		{
			if (!GalleryController.IsInitialized)
			{
				GalleryController.InitializeGspApplication();
			}

			if (InitializeVariables(context))
			{
				string tvXml = GenerateTreeviewXml();
				context.Response.ContentType = "text/xml";
				context.Response.Cache.SetCacheability(HttpCacheability.NoCache); // Needed for IE 7
				context.Response.Write(tvXml);
			}
			else
				context.Response.End();
		}

		#endregion

		#region Public Properties

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

		#region Private Methods

		private string GenerateTreeviewXml()
		{
			TreeView tv = GenerateTreeview();
			return tv.GetXml();
		}

		private TreeView GenerateTreeview()
		{
			// We'll use a TreeView instance to generate the appropriate XML structure 
			ComponentArt.Web.UI.TreeView tv = new ComponentArt.Web.UI.TreeView();

			string handlerPath = String.Concat(Utils.GalleryRoot, "/handler/gettreeviewxml.ashx");

			IAlbum parentAlbum = AlbumController.LoadAlbumInstance(this._albumId, true);

			string securityActionParm = String.Empty;
			if (SecurityActionEnumHelper.IsValidSecurityAction(this._securityAction))
			{
				securityActionParm = String.Format(CultureInfo.CurrentCulture, "&secaction={0}", (int)this._securityAction);
			}

			foreach (IAlbum childAlbum in parentAlbum.GetChildGalleryObjects(GalleryObjectType.Album, true, !Utils.IsAuthenticated))
			{
				TreeViewNode node = new TreeViewNode();
				node.Text = Utils.RemoveHtmlTags(childAlbum.Title);
				node.Value = childAlbum.Id.ToString(CultureInfo.InvariantCulture);
				node.ID = childAlbum.Id.ToString(CultureInfo.InvariantCulture);

				if (!String.IsNullOrEmpty(_navigateUrl))
				{
					node.NavigateUrl = Utils.AddQueryStringParameter(_navigateUrl, String.Concat("aid=", childAlbum.Id.ToString(CultureInfo.InvariantCulture)));
					node.HoverCssClass = "tv0HoverTreeNodeLink";
				}

				bool isUserAuthorized = true;
				if (SecurityActionEnumHelper.IsValidSecurityAction(this._securityAction))
				{
					isUserAuthorized = Utils.IsUserAuthorized(_securityAction, RoleController.GetGalleryServerRolesForUser(), childAlbum.Id, childAlbum.GalleryId, childAlbum.IsPrivate);
				}
				node.ShowCheckBox = isUserAuthorized && _showCheckbox;
				node.Selectable = isUserAuthorized;
				if (!isUserAuthorized) node.HoverCssClass = String.Empty;

				if (childAlbum.GetChildGalleryObjects(GalleryObjectType.Album).Count > 0)
				{
					string handlerPathWithAlbumId = Utils.AddQueryStringParameter(handlerPath, String.Concat("aid=", childAlbum.Id.ToString(CultureInfo.InvariantCulture)));
					node.ContentCallbackUrl = String.Format(CultureInfo.CurrentCulture, "{0}{1}&sc={2}&nurl={3}", handlerPathWithAlbumId, securityActionParm, node.ShowCheckBox, Utils.UrlEncode(_navigateUrl));
				}

				tv.Nodes.Add(node);
			}

			return tv;
		}

		/// <summary>
		/// Initialize the class level variables with information from the query string. Returns false if the variables cannot 
		/// be properly initialized.
		/// </summary>
		/// <param name="context">The HttpContext for the current request.</param>
		/// <returns>Returns true if all variables were initialized; returns false if there was a problem and one or more variables
		/// could not be set.</returns>
		private bool InitializeVariables(HttpContext context)
		{
			if (!ExtractQueryStringParms(context.Request.Url.Query))
				return false;

			if (_albumId > 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Extract information from the query string and assign to our class level variables. Return false if something goes wrong
		/// and the variables cannot be set. This will happen when the query string is in an unexpected format.
		/// </summary>
		/// <param name="queryString">The query string for the current request. Can be populated with HttpContext.Request.Url.Query.</param>
		/// <returns>Returns true if all relevant variables were assigned from the query string; returns false if there was a problem.</returns>
		private bool ExtractQueryStringParms(string queryString)
		{
			if (String.IsNullOrEmpty(queryString)) return false;

			if (queryString.StartsWith("?", StringComparison.Ordinal)) queryString = queryString.Remove(0, 1);

			//aid={0}&secaction={1}&sc={2}&nurl={3}
			foreach (string nameValuePair in queryString.Split(new char[] { '&' }))
			{
				string[] nameOrValue = nameValuePair.Split(new char[] { '=' });

				if (nameOrValue.Length < 2)
				{
					return false;
				}

				switch (nameOrValue[0])
				{
					case "aid":
						{
							int aid;
							if (Int32.TryParse(nameOrValue[1], out aid))
								_albumId = aid;
							else
								return false;
							break;
						}
					case "secaction":
						{
							int secActionInt;
							if (Int32.TryParse(nameOrValue[1], out secActionInt))
							{
								if (SecurityActionEnumHelper.IsValidSecurityAction((SecurityActions)secActionInt))
								{
									_securityAction = (SecurityActions)secActionInt; break;
								}
								else
									return false;
							}
							else
								return false;
						}
					case "sc":
						{
							bool showCheckbox;
							if (Boolean.TryParse(nameOrValue[1], out showCheckbox))
								_showCheckbox = showCheckbox;
							else
								return false;
							break;
						}
					case "nurl":
						{
							_navigateUrl = Utils.UrlDecode(nameOrValue[1]).Trim();
							break;
						}
					default: return false; // Unexpected query string parm. Return false so execution is aborted.
				}
			}

			return true;
		}

		#endregion
	}
}
