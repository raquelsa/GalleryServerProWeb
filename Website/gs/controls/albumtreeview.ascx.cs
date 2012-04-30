using System;
using System.Collections.Generic;
using System.Globalization;
using ComponentArt.Web.UI;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Controls
{
	/// <summary>
	/// A user control that renders albums in a treeview.
	/// </summary>
	public partial class albumtreeview : GalleryUserControl
	{
		#region Private Fields

		private IAlbum _albumToSelect;
		private IIntegerCollection _albumIdsToCheck = new IntegerCollection();
		private SecurityActions _requiredSecurityPermissions;
		private string _securityPermissionParm;
		private IGalleryCollection _galleries;

		#endregion

		#region Protected Events

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			RegisterJavascript();

			ConfigureControlsEveryTime();
		}

		/// <summary>
		/// Handles the OnServerValidate event of the CustomValidator control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="args" /> is null.</exception>
		protected void cv_OnServerValidate(object sender, System.Web.UI.WebControls.ServerValidateEventArgs args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			int albumId;
			args.IsValid = ((tv.SelectedNode != null) && Int32.TryParse(tv.SelectedNode.Value, out albumId) && (albumId > int.MinValue));
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets a reference to the ComponentArt.Web.UI.TreeView control within this user control.
		/// </summary>
		public TreeView TreeView
		{
			get
			{
				return tv;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether more than one checkbox can be selected at a time in the treeview.
		/// The default value is false. This property should be set before calling DataBind().
		/// </summary>
		public bool AllowMultiCheck
		{
			get
			{
				object viewStateValue = ViewState["AllowMultiSelect"];
				bool allowMultiSelect;
				if ((viewStateValue != null) && (Boolean.TryParse(viewStateValue.ToString(), out allowMultiSelect)))
					return allowMultiSelect;
				else
					return false;
			}
			set
			{
				ViewState["AllowMultiSelect"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether a checkbox is to be rendered for each album in the treeview. The default value
		/// is true. This property should be set before calling DataBind().
		/// </summary>
		public bool ShowCheckbox
		{
			get
			{
				object viewStateValue = ViewState["ShowCheckbox"];
				bool showCheckbox;
				if ((viewStateValue != null) && (Boolean.TryParse(viewStateValue.ToString(), out showCheckbox)))
					return showCheckbox;
				else
					return true;
			}
			set
			{
				ViewState["ShowCheckbox"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the base URL to invoke when a tree node is clicked. This property should be set before calling DataBind().
		/// The album ID of the selected album is passed to the URL as the query string parameter "aid".
		/// Example: "Gallery.aspx, http://site.com/gallery.aspx"
		/// </summary>
		public string NavigateUrl
		{
			get
			{
				object viewStateValue = ViewState["NavigateUrl"];

				return (viewStateValue != null ? viewStateValue.ToString() : String.Empty);
			}
			set
			{
				ViewState["NavigateUrl"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value to be prepended to the root album title in the treeview. The default value is <see cref="String.Empty" />.
		/// May contain the placeholder values {GalleryId} and/or {GalleryDescription}. If present, the placeholders are replaced by the 
		/// action values during databinding. This property should be set before calling DataBind(). Example: "Gallery {GalleryDescription}: "
		/// </summary>
		public string RootAlbumPrefix
		{
			get
			{
				object viewStateValue = ViewState["RootAlbumPrefix"];

				return (viewStateValue != null ? viewStateValue.ToString() : String.Empty);
			}
			set
			{
				ViewState["RootAlbumPrefix"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the galleries to be rendered in the treeview. If not explicitly set, this defaults to the current gallery.
		/// If the <see cref="RootAlbumId" /> property is assigned, this property is ignored.
		/// </summary>
		/// <value>The galleries to be rendered in the treeview.</value>
		public IGalleryCollection Galleries
		{
			get
			{
				if (_galleries == null)
				{
					_galleries = new GalleryCollection();

					_galleries.Add(Factory.LoadGallery(this.GalleryPage.GalleryId));
				}

				return _galleries;
			}
			set
			{
				_galleries = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating the top level album to render. When not specified, the <see cref="Galleries" /> property determines
		/// the root albums to be rendered. This property should be set before calling DataBind().
		/// </summary>
		/// <value>The top level album to render.</value>
		public int RootAlbumId
		{
			get
			{
				object viewStateValue = ViewState["RootAlbumId"];
				int rootAlbumId;
				if ((viewStateValue != null) && (Int32.TryParse(viewStateValue.ToString(), out rootAlbumId)))
					return rootAlbumId;
				else
					return int.MinValue;
			}
			set
			{
				ViewState["RootAlbumId"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the user is required to select an album from the treeview.
		/// Default is false.
		/// </summary>
		public bool RequireAlbumSelection
		{
			get
			{
				return cv.Enabled;
			}
			set
			{
				cv.Enabled = value;
			}
		}

		/// <summary>
		/// Gets or sets the security permission the logged on user must have in order for an album to be displayed in the
		/// treeview. It may be a single value or some combination of valid enumeration values.
		/// </summary>
		public SecurityActions RequiredSecurityPermissions
		{
			get
			{
				return this._requiredSecurityPermissions;
			}
			set
			{
				if (!SecurityActionEnumHelper.IsValidSecurityAction(value))
					throw new ArgumentException("Invalid SecurityActions enumeration value.");

				this._requiredSecurityPermissions = value;
			}
		}

		/// <summary>
		/// Gets a string representing the RequiredSecurityPermission property that can be used as a querystring parameter.
		/// Ex: "&amp;secaction=3"
		/// </summary>
		private string SecurityPermissionQueryStringParm
		{
			get
			{
				if (String.IsNullOrEmpty(this._securityPermissionParm))
				{
					if (SecurityActionEnumHelper.IsValidSecurityAction(this.RequiredSecurityPermissions))
					{
						this._securityPermissionParm = String.Format(CultureInfo.CurrentCulture, "&secaction={0}", (int)this.RequiredSecurityPermissions);
					}
				}

				return this._securityPermissionParm;
			}
		}

		/// <summary>
		/// Gets a list of the checked treeview nodes in the treeview.
		/// </summary>
		public TreeViewNode[] CheckedNodes
		{
			get
			{
				return tv.CheckedNodes;
			}
		}

		/// <summary>
		/// Gets a collection of the "highest" checked nodes.
		/// </summary>
		public IIntegerCollection TopLevelCheckedAlbumIds
		{
			get
			{
				TreeViewNode[] checkedNodes = tv.CheckedNodes;
				IIntegerCollection checkedNodeIds = new IntegerCollection();

				foreach (TreeViewNode node in checkedNodes)
				{
					if (IsTopLevelCheckedNode(node))
					{
						checkedNodeIds.Add(Convert.ToInt32(node.Value, CultureInfo.InvariantCulture));
					}
				}

				return checkedNodeIds;
			}
		}

		/// <summary>
		/// Gets a reference to the collection of album IDs whose associated checkboxes should be checked.
		/// Add the desired album IDs to this collection and then call DataBind(). This user control 
		/// guarantees that ALL albums in this collection are rendered and made visible during the DataBind() method.
		/// The collection is cleared after the databind is finished. Use the property TopLevelCheckedAlbumIds
		/// to retrieve the list of top level checked albums after the user has interacted with the control.
		/// </summary>
		public IIntegerCollection AlbumIdsToCheck
		{
			get
			{
				if (this._albumIdsToCheck == null)
				{
					this._albumIdsToCheck = new IntegerCollection();
				}

				return this._albumIdsToCheck;
			}
		}

		/// <summary>
		/// Gets or sets the selected node in the treeview. Only one node can be selected at a time.
		/// </summary>
		public TreeViewNode SelectedNode
		{
			get
			{
				return tv.SelectedNode;
			}
			set
			{
				tv.SelectedNode = value;
			}
		}

		/// <summary>
		/// Gets or sets the with of the treeview control.
		/// </summary>
		public System.Web.UI.WebControls.Unit Width
		{
			get
			{
				return tv.Width;
			}
			set
			{
				tv.Width = value;
			}
		}

		/// <summary>
		/// Gets or sets the height of the treeview control.
		/// </summary>
		public System.Web.UI.WebControls.Unit Height
		{
			get
			{
				return tv.Height;
			}
			set
			{
				tv.Height = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of a javascript function to invoke when a treenode is selected.
		/// Returns String.Empty if it has not been assigned.
		/// </summary>
		public string ClientOnTreeNodeSelectJavascriptFunctionName
		{
			get
			{
				object viewStateValue = ViewState["OnNodeSelect"];

				return (viewStateValue != null ? viewStateValue.ToString() : String.Empty);
			}
			set
			{
				ViewState["OnNodeSelect"] = value;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Render the treeview to two levels - the root album and its direct children. If the AlbumIdsToCheck property
		/// has items in its collection, make sure every album in the collection is rendered, no matter how deep in the album heirarchy 
		/// they may be. If the albumToSelect parameter is specified, then make sure this album is rendered and 
		/// selected/checked, no matter how deep in the hierarchy it may be.
		/// </summary>
		public void BindTreeView()
		{
			BindTreeView(null);
		}

		/// <summary>
		/// Render the treeview to two levels - the root album and its direct children. If the <see cref="AlbumIdsToCheck" /> property
		/// has items in its collection, make sure every album in the collection is rendered, no matter how deep in the album heirarchy 
		/// they may be. If the <paramref name="albumToSelect" /> parameter is specified, then make sure this album is rendered and 
		/// selected/checked, no matter how deep in the hierarchy it may be.
		/// </summary>
		/// <param name="albumToSelect">An album to be selected, checked, and made visible. The treeview is automatically expanded as
		/// needed to ensure this album is visible.</param>
		public void BindTreeView(IAlbum albumToSelect)
		{
			this._albumToSelect = albumToSelect;

			DataBindTreeView();

			this._albumToSelect = null;

			this.AlbumIdsToCheck.Clear();
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsEveryTime()
		{
			tv.LineImagesFolderUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/treeview/lines");

			cv.Text = String.Format(CultureInfo.CurrentCulture, "<span class='gsp_bold'>{0} </span>{1}", Resources.GalleryServerPro.Task_Transfer_Objects_Cannot_Transfer_No_Destination_Album_Selected_Hdr, Resources.GalleryServerPro.Task_Transfer_Objects_Cannot_Transfer_No_Destination_Album_Selected_Dtl);

			if (this.AllowMultiCheck)
			{
				tv.ClientEvents.NodeExpand = new ClientEvent("tv_onNodeExpand");
				//tv.ClientEvents.NodeCheckChange = new ClientEvent("tv_onNodeCheckChange");
			}
		}

		/// <summary>
		/// Render the treeview with the first two levels of albums that are viewable to the logged on user.
		/// </summary>
		private void DataBindTreeView()
		{
			#region Validation

			//if (!this.AllowMultiSelect && this.AlbumIdsToCheck.Count > 1)
			//{
			//  throw new InvalidOperationException("The property AllowMultiSelect must be false when multiple album IDs have been assigned to the property AlbumIdsToCheck.");
			//}

			if (!this.AllowMultiCheck && this.AlbumIdsToCheck.Count > 1)
			{
				throw new InvalidOperationException("The property AllowMultiCheck must be false when multiple album IDs have been assigned to the property AlbumIdsToCheck.");
			}

			if (!SecurityActionEnumHelper.IsValidSecurityAction(this.RequiredSecurityPermissions))
			{
				throw new InvalidOperationException("The property GalleryServerPro.Web.Controls.albumtreeview.RequiredSecurityPermissions must be assigned before the TreeView can be rendered.");
			}

			#endregion

			tv.Nodes.Clear();

			foreach (IAlbum rootAlbum in GetRootAlbums())
			{
				// Add root node.
				TreeViewNode rootNode = new TreeViewNode();

				string albumTitle = GetRootAlbumTitle(rootAlbum);
				rootNode.Text = albumTitle;
				rootNode.ToolTip = albumTitle;
				rootNode.Value = rootAlbum.Id.ToString(CultureInfo.InvariantCulture);
				rootNode.ID = rootAlbum.Id.ToString(CultureInfo.InvariantCulture);
				rootNode.Expanded = true;

				if (!String.IsNullOrEmpty(NavigateUrl))
				{
					rootNode.NavigateUrl = Utils.AddQueryStringParameter(NavigateUrl, String.Concat("aid=", rootAlbum.Id.ToString(CultureInfo.InvariantCulture)));
					rootNode.HoverCssClass = "tv0HoverTreeNodeLink";
				}

				bool isAlbumSelectable = !rootAlbum.IsVirtualAlbum;
				rootNode.ShowCheckBox = isAlbumSelectable && ShowCheckbox;
				rootNode.Selectable = isAlbumSelectable;
				if (!isAlbumSelectable) rootNode.HoverCssClass = String.Empty;

				// Select and check this node if needed.
				if (isAlbumSelectable && (this._albumToSelect != null) && (rootAlbum.Id == _albumToSelect.Id))
				{
					tv.SelectedNode = rootNode;
					rootNode.Checked = true;
				}

				// Check this node if needed.
				if (this._albumIdsToCheck.Contains(rootAlbum.Id))
				{
					rootNode.Checked = true;
				}

				tv.Nodes.Add(rootNode);

				// Add the first level of albums below the root album.
				BindAlbumToTreeview(rootAlbum.GetChildGalleryObjects(GalleryObjectType.Album, true, this.GalleryPage.IsAnonymousUser), rootNode, false);

				// Only display the root node if it is selectable or we added any children to it; otherwise, remove it.
				if (!rootNode.Selectable && rootNode.Nodes.Count == 0)
				{
					tv.Nodes.Remove(rootNode);
				}
			}

			if ((this._albumToSelect != null) && (tv.SelectedNode == null))
			{
				// We have an album we are supposed to select, but we haven't encountered it in the first two levels,
				// so expand the treeview as needed to include this album.
				BindSpecificAlbumToTreeview(this._albumToSelect);
			}

			// Make sure all specified albums are visible and checked.
			foreach (int albumId in this._albumIdsToCheck)
			{
				IAlbum album = AlbumController.LoadAlbumInstance(albumId, false);
				if (this.GalleryPage.IsUserAuthorized(RequiredSecurityPermissions, album))
				{
					BindSpecificAlbumToTreeview(album);
				}
			}
		}

		private string GetRootAlbumTitle(IAlbum rootAlbum)
		{
			IGallery gallery = Factory.LoadGallery(rootAlbum.GalleryId);
			string rootAlbumPrefix = RootAlbumPrefix.Replace("{GalleryId}", gallery.GalleryId.ToString(CultureInfo.InvariantCulture)).Replace("{GalleryDescription}", gallery.Description);
			return Utils.RemoveHtmlTags(String.Concat(rootAlbumPrefix, rootAlbum.Title));
		}

		/// <summary>
		/// Gets a list of top-level albums to display in the treeview. There will be a maximum of one for each gallery.
		/// If the <see cref="RootAlbumId" /> property is assigned, that album is returned and the <see cref="Galleries" /> property is 
		/// ignored.
		/// </summary>
		/// <returns>Returns a list of top-level albums to display in the treeview.</returns>
		private IEnumerable<IAlbum> GetRootAlbums()
		{
			List<IAlbum> rootAlbums = new List<IAlbum>(1);

			if (RootAlbumId > int.MinValue)
			{
				rootAlbums.Add(AlbumController.LoadAlbumInstance(RootAlbumId, true));
			}
			else
			{
				foreach (IGallery gallery in Galleries)
				{
					rootAlbums.Add(Factory.LoadRootAlbum(gallery.GalleryId, this.RequiredSecurityPermissions, this.GalleryPage.GetGalleryServerRolesForUser(), Utils.IsAuthenticated));
				}
			}

			return rootAlbums;
		}

		/// <summary>
		/// Bind the specified album to the treeview. This method assumes the treeview has at least the root node already
		/// built. The specified album can be at any level in the hierarchy. Nodes between the album and the existing top node
		/// are automatically created so that the full node path to the album is shown.
		/// </summary>
		/// <param name="album">An album to be added to the treeview.</param>
		private void BindSpecificAlbumToTreeview(IAlbum album)
		{
			if (tv.FindNodeById(album.Id.ToString(CultureInfo.InvariantCulture)) == null)
			{
				// Get a stack of albums that go from the current album to the top level album.
				// Once the stack is built we'll then add these albums to the treeview so that the full heirarchy
				// to the current album is shown.
				TreeViewNode existingParentNode;
				Stack<IAlbum> albumParents = GetAlbumsBetweenTopLevelNodeAndAlbum(tv, album, out existingParentNode);

				if (existingParentNode == null)
					return;

				BindSpecificAlbumToTreeview(existingParentNode, albumParents);
			}
		}

		/// <summary>
		/// Retrieve a list of albums that are in the heirarchical path between the specified album and a node in the treeview.
		/// The node that is discovered as the ancestor of the album is assigned to the existingParentNode parameter.
		/// </summary>
		/// <param name="treeview">The treeview with at least one node added to it. At least one node must be an ancestor of the 
		/// specified album.</param>
		/// <param name="album">An album. This method navigates the ancestors of this album until it finds a matching node in the treeview.</param>
		/// <param name="existingParentNode">The existing node in the treeview that is an ancestor of the specified album is assigned to
		/// this parameter.</param>
		/// <returns>Returns a list of albums where the first album (the one returned by calling Pop) is a child of the album 
		/// represented by the existingParentNode treeview node, and each subsequent album is a child of the previous album.
		/// The final album is the same album specified in the album parameter.</returns>
		private static Stack<IAlbum> GetAlbumsBetweenTopLevelNodeAndAlbum(ComponentArt.Web.UI.TreeView treeview, IAlbum album, out TreeViewNode existingParentNode)
		{
			if (treeview.Nodes.Count == 0)
				throw new ArgumentException("The treeview must have at least one top-level node before calling the function GetAlbumsBetweenTopLevelNodeAndAlbum().");

			Stack<IAlbum> albumParents = new Stack<IAlbum>();
			albumParents.Push(album);

			IAlbum parentAlbum = (IAlbum)album.Parent;

			albumParents.Push(parentAlbum);

			// Navigate up from the specified album until we find an album that exists in the treeview. Remember,
			// the treeview has been built with the root node and the first level of albums, so eventually we
			// should find an album. If not, just return without showing the current album.
			while ((existingParentNode = treeview.FindNodeById(parentAlbum.Id.ToString(CultureInfo.InvariantCulture))) == null)
			{
				parentAlbum = parentAlbum.Parent as IAlbum;

				if (parentAlbum == null)
					break;

				albumParents.Push(parentAlbum);
			}

			// Since we found a node in the treeview we don't need to add the most recent item in the stack. Pop it off.
			albumParents.Pop();

			return albumParents;
		}

		/// <summary>
		/// Bind the heirarchical list of albums to the specified treeview node.
		/// </summary>
		/// <param name="existingParentNode">The treeview node to add the first album in the stack to.</param>
		/// <param name="albumParents">A list of albums where the first album should be a child of the specified treeview
		/// node, and each subsequent album is a child of the previous album.</param>
		private void BindSpecificAlbumToTreeview(TreeViewNode existingParentNode, Stack<IAlbum> albumParents)
		{
			// Assumption: The first album in the stack is a child of the existingParentNode node.
			existingParentNode.Expanded = true;

			// For each album in the heirarchy of albums to the current album, add the album and all its siblings to the 
			// treeview.
			foreach (IAlbum album in albumParents)
			{
				if (existingParentNode.Nodes.Count == 0)
				{
					// Add all the album's siblings to the treeview.
					IGalleryObjectCollection childAlbums = AlbumController.LoadAlbumInstance(Convert.ToInt32(existingParentNode.ID, CultureInfo.InvariantCulture), true).GetChildGalleryObjects(GalleryObjectType.Album, true, this.GalleryPage.IsAnonymousUser);
					BindAlbumToTreeview(childAlbums, existingParentNode, false);
				}

				// Now find the album in the siblings we just added that matches the current album in the stack.
				// Set that album as the new parent and expand it.
				TreeViewNode nodeInAlbumHeirarchy = null;
				foreach (TreeViewNode node in existingParentNode.Nodes)
				{
					if (node.ID == album.Id.ToString(CultureInfo.InvariantCulture))
					{
						nodeInAlbumHeirarchy = node;
						nodeInAlbumHeirarchy.Expanded = true;
						break;
					}
				}

				if (nodeInAlbumHeirarchy == null)
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "Album ID {0} is not a child of the treeview node representing album ID {1}.", album.Id, Convert.ToInt32(existingParentNode.Value, CultureInfo.InvariantCulture)));

				existingParentNode = nodeInAlbumHeirarchy;
			}
			existingParentNode.Expanded = false;
		}

		/// <summary>
		/// Add the collection of albums to the specified treeview node.
		/// </summary>
		/// <param name="albums">The collection of albums to add the the treeview node.</param>
		/// <param name="parentNode">The treeview node that will receive child nodes representing the specified albums.</param>
		/// <param name="expandNode">Specifies whether the nodes should be expanded.</param>
		private void BindAlbumToTreeview(IGalleryObjectCollection albums, TreeViewNode parentNode, bool expandNode)
		{
			string handlerPath = String.Concat(Utils.GalleryRoot, "/handler/gettreeviewxml.ashx");

			foreach (IAlbum album in albums)
			{
				TreeViewNode node = new TreeViewNode();
				string albumTitle = Utils.RemoveHtmlTags(album.Title);
				node.Text = albumTitle;
				node.ToolTip = albumTitle;
				node.Value = album.Id.ToString(CultureInfo.InvariantCulture);
				node.ID = album.Id.ToString(CultureInfo.InvariantCulture);
				node.Expanded = expandNode;

				if (!String.IsNullOrEmpty(NavigateUrl))
				{
					node.NavigateUrl = Utils.AddQueryStringParameter(NavigateUrl, String.Concat("aid=", album.Id.ToString(CultureInfo.InvariantCulture)));
					node.HoverCssClass = "tv0HoverTreeNodeLink";
				}

				node.ShowCheckBox = parentNode.ShowCheckBox;
				node.Selectable = true;

				if (album.GetChildGalleryObjects(GalleryObjectType.Album, false, this.GalleryPage.IsAnonymousUser).Count > 0)
				{
					string handlerPathWithAlbumId = Utils.AddQueryStringParameter(handlerPath, String.Concat("aid=", album.Id.ToString(CultureInfo.InvariantCulture)));
					node.ContentCallbackUrl = String.Format(CultureInfo.CurrentCulture, "{0}{1}&sc={2}&nurl={3}", handlerPathWithAlbumId, this.SecurityPermissionQueryStringParm, node.ShowCheckBox, Utils.UrlEncode(NavigateUrl));
				}

				// Select and check this node if needed.
				if ((this._albumToSelect != null) && (album.Id == this._albumToSelect.Id))
				{
					tv.SelectedNode = node;
					node.Checked = true;
					node.Expanded = true;
					// Expand the child of the selected album.
					BindAlbumToTreeview(album.GetChildGalleryObjects(GalleryObjectType.Album, true, this.GalleryPage.IsAnonymousUser), node, false);
				}

				// Check this node if needed.
				if (this._albumIdsToCheck.Contains(album.Id))
				{
					node.Checked = true;
				}

				parentNode.Nodes.Add(node);
			}
		}

		/// <summary>
		/// Determines whether the specified node is the "highest" checked node, or whether it has any ancestor nodes that are checked.
		/// </summary>
		/// <param name="albumNode">A treeview node for which to determine if any of its parents are checked.</param>
		/// <returns>Returns true if none of this node's ancestors is checked; otherwise returns false.</returns>
		private static bool IsTopLevelCheckedNode(TreeViewNode albumNode)
		{
			if (!albumNode.Checked)
				throw new WebException("Only checked treeview nodes should be passed to the IsTopLevelCheckedNode() method. Instead, the specified node was not checked.");

			TreeViewNode node = albumNode;
			while (node.ParentNode != null)
			{
				node = node.ParentNode;
				if (node.Checked)
					return false;
			}

			return true;
		}

		private void RegisterJavascript()
		{
			string script;
			if (this.AllowMultiCheck)
			{
				script = String.Format(CultureInfo.InvariantCulture, @"
var addedIds = new Array();
var removedIds = new Array();

function tv_onNodeSelect(sender, eventArgs)
{{
	// Manage the checking and unchecking when the node text is clicked.
	// When a node is checked, all its children should be checked. When a node
	// is unchecked, all its parents should be unchecked.
	var selectedNode = eventArgs.get_node(); 
	if (selectedNode == null) return;

	if (selectedNode.get_checked() == true) // User is unchecking node
	{{
		uncheckAll(selectedNode, true);
	}}
	else
	{{
		checkAll(selectedNode);
	}}
	{0}
	sender.render();
}}

function checkAll(parentNode)
{{
	parentNode.set_checked(true);
	var nodes = parentNode.get_nodes();
	for (var i = 0; i < nodes.get_length(); i++)
	{{
		checkAll(nodes.getNode(i));
	}}
}}

function uncheckAll(node, navigateUp)
{{
	if (node == null)
		return;

	node.set_checked(false);
	if (navigateUp)
	{{
		uncheckAll(node.get_parentNode(), navigateUp);
	}}
	else
	{{
		// Drill down, unchecking along the way
		var nodes = node.get_nodes();
		for (var i = 0; i < nodes.get_length(); i++)
		{{
			uncheckAll(nodes.getNode(i), navigateUp);
		}}
	}}
}}
		
function tv_onNodeBeforeCheckChange(sender, eventArgs)
{{
	// Enforce rules: When a node is checked, all its children should be checked. When a node
	// is unchecked, all its parents should be unchecked.
	var checkedNode = eventArgs.get_node(); 
	if (checkedNode == null) return;

	if (checkedNode.get_checked()) // Checked property gives 'before' state
	{{
		// Since the 'before' state is checked, user is trying to uncheck the node.
		uncheckAll(checkedNode, true);
	}}
	else
		checkAll(checkedNode); // User is checking node - always allow.
		
	//The sender parameter is the treeview - tell it to render so that the updates are displayed.
	sender.render();
}}

function tv_onNodeExpand(sender, eventArgs)
{{
	var node = eventArgs.get_node();
	if (node.get_checked())
	{{
		checkAll(node);
		sender.render();
	}}
}}

",
				                       GetTreeNodeOnSelectJavascriptFunction()
					);
			}
			else
			{
				script = String.Format(CultureInfo.InvariantCulture, @"
function tv_onNodeSelect(sender, e)
{{
	// Manage the checking and unchecking when the node text is clicked.
	var selectedNode = e.get_node(); 
	if (selectedNode == null) return;

	sender.unCheckAll();
	selectedNode.set_checked(true); 
	{0}
	sender.render();
}}
		
function tv_onNodeBeforeCheckChange(sender, e)
{{
	var node = e.get_node(); 
	if (node == null) return;

	sender.unCheckAll(); // user is checking this node
	if (!node.get_checked()) // Checked property gives 'before' state
	{{
		node.set_checked(true);
		sender.selectNodeById(node.get_id());
	}}
		
	sender.render();
}}",
				                       GetTreeNodeOnSelectJavascriptFunction()
					);
			}

			System.Web.UI.ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "tvFunctions", script, true);

		}

		private string GetTreeNodeOnSelectJavascriptFunction()
		{
			if (String.IsNullOrEmpty(this.ClientOnTreeNodeSelectJavascriptFunctionName))
				return string.Empty;

			return String.Concat(this.ClientOnTreeNodeSelectJavascriptFunctionName, "(sender, e);");
		}

		#endregion
	}
}