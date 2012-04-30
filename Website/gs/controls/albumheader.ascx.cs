using System;
using System.Globalization;
using System.Web.UI;
using ComponentArt.Web.UI;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Web.Controls
{
	/// <summary>
	/// A user control that contains album information.
	/// </summary>
	public partial class albumheader : GalleryUserControl
	{
		#region Private Fields

		private bool _enableInlineEditing;
		private bool _enableAlbumDownload = true;

		#endregion

		#region Protected Events

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!this.GalleryPage.IsCallback)
			{
				ConfigureControls();

				ConfigureSecurityRelatedControls();
			}

			RegisterJavascript();
		}

		/// <summary>
		/// Gets the CSS class to apply to the private album icon.
		/// </summary>
		/// <returns></returns>
		protected string GetAlbumPrivateCssClass()
		{
			if (this.GalleryPage.GetAlbum().IsPrivate)
				return String.Empty;
			else
				return "gsp_invisible";
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets a value indicating whether the album information can be edited.
		/// </summary>
		/// <value><c>true</c> if the album information can be edited; otherwise, <c>false</c>.</value>
		public bool EnableInlineEditing
		{
			get
			{
				return this._enableInlineEditing;
			}
			set
			{
				this._enableInlineEditing = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether [enable album download].
		/// </summary>
		/// <value><c>true</c> if [enable album download]; otherwise, <c>false</c>.</value>
		public bool EnableAlbumDownload
		{
			get
			{
				return this._enableAlbumDownload;
			}
			set
			{
				this._enableAlbumDownload = value;
			}
		}

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			IAlbum album = GalleryPage.GetAlbum();
			lblTitle.Text = album.Title;
			lblSummary.Text = album.Summary;
			litStats.Text = getAlbumStats(album);
			lblDate.Text = getAlbumDates(album);

			if (String.IsNullOrEmpty(lblDate.Text))
			{
				dateContainer.Attributes["class"] = "gsp_invisible gsp_minimargin";
			}
			else
			{
				dateContainer.Attributes["class"] = "gsp_minimargin";
			}

			pADC.Visible = (this.GalleryPage.GallerySettings.EnableGalleryObjectZipDownload && EnableAlbumDownload);
		}

		private void ConfigureSecurityRelatedControls()
		{
			// If the user has permission to edit the media object, configure the caption so that when it is double-clicked,
			// a dialog window appears that lets the user edit and save the caption. Note that this code is dependent on the
			// saveCaption javascript function, which is added in the RegisterJavascript method.
			if (this.EnableInlineEditing && this.GalleryPage.UserCanEditAlbum)
			{
				pnlAlbumHeader.ToolTip = Resources.GalleryServerPro.Site_Editable_Content_Tooltip;

				if (GalleryPage.AlbumTreeViewIsVisible)
				{
					pnlAlbumHeader.CssClass = "albumHeader albumHeaderEditableContentOff gsp_treeviewBufferFullWidth";
					pnlAlbumHeader.Attributes.Add("onmouseover", "this.className='albumHeader albumHeaderEditableContentOn gsp_treeviewBufferFullWidth';");
					pnlAlbumHeader.Attributes.Add("onmouseout", "this.className='albumHeader albumHeaderEditableContentOff gsp_treeviewBufferFullWidth';");
					pnlAlbumHeader.Attributes.Add("ondblclick", "editAlbumInfo();");
				}
				else
				{
					pnlAlbumHeader.CssClass = "albumHeader albumHeaderEditableContentOff";
					pnlAlbumHeader.Attributes.Add("onmouseover", "this.className='albumHeader albumHeaderEditableContentOn';");
					pnlAlbumHeader.Attributes.Add("onmouseout", "this.className='albumHeader albumHeaderEditableContentOff';");
					pnlAlbumHeader.Attributes.Add("ondblclick", "editAlbumInfo();");
				}

				AddEditAlbumInfoDialog();
			}
			else
			{
				if (GalleryPage.AlbumTreeViewIsVisible)
				{
					pnlAlbumHeader.CssClass = "albumHeaderReadOnly gsp_treeviewBufferFullWidth";
				}
				else
				{
					pnlAlbumHeader.CssClass = "albumHeaderReadOnly";					
				}
			}
		}

		private void AddEditAlbumInfoDialog()
		{
			Dialog dgEditAlbum = new Dialog();

			dgEditAlbum.ContentTemplate = Page.LoadTemplate(Utils.GetUrl("/controls/albumedittemplate.ascx"));

			#region Set Dialog Properties

			dgEditAlbum.ID = "dgEditAlbum";
			dgEditAlbum.AnimationDirectionElement = pnlAlbumHeader.ClientID;
			dgEditAlbum.CloseTransition = TransitionType.Fade;
			dgEditAlbum.ShowTransition = TransitionType.Fade;
			dgEditAlbum.AnimationSlide = SlideType.Linear;
			dgEditAlbum.AnimationType = DialogAnimationType.Outline;
			dgEditAlbum.AnimationPath = SlidePath.Direct;
			dgEditAlbum.AnimationDuration = 400;
			dgEditAlbum.TransitionDuration = 400;
			dgEditAlbum.Icon = "pencil.gif";
			dgEditAlbum.Alignment = DialogAlignType.MiddleCentre;
			dgEditAlbum.AllowResize = true;
			dgEditAlbum.ContentCssClass = "dg0ContentCss";
			dgEditAlbum.HeaderCssClass = "dg0HeaderCss";
			dgEditAlbum.CssClass = "gsp_dg0DialogCss gsp_ns";
			dgEditAlbum.FooterCssClass = "dg0FooterCss";
			dgEditAlbum.ZIndex = 900;

			dgEditAlbum.HeaderClientTemplateId = "dgEditAlbumHeaderTemplate";

			#endregion

			#region Header Template

			ClientTemplate ctHeader = new ClientTemplate();
			ctHeader.ID = "dgEditAlbumHeaderTemplate";

			ctHeader.Text = String.Format(CultureInfo.InvariantCulture, @"
		<div onmousedown='dgEditAlbum.StartDrag(event);'>
			<img id='dg0DialogCloseImage' onclick=""closeEditDialog();"" src='{0}/images/componentart/dialog/close.gif' /><img
				id='dg0DialogIconImage' src='{0}/images/componentart/dialog/pencil.gif' style='width:27px;height:30px;' />
				## Parent.Title ##
		</div>", Utils.GalleryRoot);

			dgEditAlbum.ClientTemplates.Add(ctHeader);

			#endregion

			phEditAlbumDialog.Controls.Add(dgEditAlbum);
		}

		private static string getAlbumDates(IAlbum album)
		{
			//If there are two different valid dates, then display both.  Otherwise, display
			//the one that is valid, or return blank if neither are valid.
			string dateText = string.Empty;
			if ((album.DateStart > DateTime.MinValue) && (album.DateEnd > DateTime.MinValue))
				//Both dates are valid.  Combine them.
				dateText = String.Format(CultureInfo.CurrentCulture, "{0:d} {1} {2:d}", album.DateStart, Resources.GalleryServerPro.UC_Album_Header_Album_Date_Range_Separator_Text, album.DateEnd);
			else if (album.DateStart > DateTime.MinValue)
				//The start date is valid.  Since the end date is invalid (we know this because if 
				//it was valid we would have gone through the previous branch rather than this one)
				//only display the start date.
				dateText = album.DateStart.ToShortDateString();
			else if (album.DateEnd > DateTime.MinValue)
				//The end date is valid.  Since the start date is invalid (we know this because if 
				//it was valid we would have gone through the first branch rather than this one)
				//only display the end date.
				dateText = album.DateEnd.ToShortDateString();

			return dateText;
		}

		private string getAlbumStats(IAlbum album)
		{
			//Create a string like: (12 objects, created 3/24/04)
			int numObjects = album.GetChildGalleryObjects(false, GalleryPage.IsAnonymousUser).Count;

			if (album.IsVirtualAlbum)
				return String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.UC_Album_Header_Stats_Without_Date_Text, numObjects);
			else
				return String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.UC_Album_Header_Stats_Text, numObjects, album.DateAdded);
		}

		private void RegisterJavascript()
		{
			// Add dynamically built javascript.
			IAlbum album = this.GalleryPage.GetAlbum();

			string beginDateVarDeclaration = (album.DateStart == DateTime.MinValue ? "null" : String.Format(CultureInfo.CurrentCulture, "new Date({0}, {1}, {2})", album.DateStart.Year, album.DateStart.Month - 1, album.DateStart.Day));
			string endDateVarDeclaration = (album.DateEnd == DateTime.MinValue ? "null" : String.Format(CultureInfo.CurrentCulture, "new Date({0}, {1}, {2})", album.DateEnd.Year, album.DateEnd.Month - 1, album.DateEnd.Day));

			string script = String.Format(CultureInfo.InvariantCulture, @"

	var _albumId = {0};
	var _beginDate = {1};
	var _endDate = {2};
	var _dateFormatString = 'd'; // Short date format
			",
			album.Id, // 0
			beginDateVarDeclaration, // 1
			endDateVarDeclaration // 2
			);

			// Add a few more functions for users with edit album permission.
			if (this.EnableInlineEditing && this.GalleryPage.UserCanEditAlbum)
			{
				// Add reference to entityobjects.js.
				string scriptUrl = Utils.GetUrl("/script/entityobjects.js");
				ScriptManager sm = ScriptManager.GetCurrent(GalleryPage.Page);
				if (sm != null)
					sm.Scripts.Add(new ScriptReference(scriptUrl));
				else
					throw new WebException("Gallery Server Pro requires a ScriptManager on the page.");

				script += String.Format(CultureInfo.InvariantCulture, @"

	function editAlbumInfo()
	{{
		dgEditAlbum.show();
		$get(_txtTitleId).focus();
		var title = $get('{0}').innerHTML.replace(/&amp;/g, '&');
		dgEditAlbum.set_title('{1}: ' + title);
		$get(_txtTitleId).value = title;

		$get('albumSummary').value = $get('{2}').innerHTML.replace(/&amp;/g, '&');
		var isPrivate = !Sys.UI.DomElement.containsCssClass($get('albumPrivateIcon'), 'gsp_invisible');
		$get('private').checked = isPrivate;
		$get('private').disabled = {3};

		if (_beginDate != null)
		{{
			$get('beginDate').value = _beginDate.localeFormat(_dateFormatString);
			var cdrDateStart = $find(_cdrBeginDateId);
			cdrDateStart.setSelectedDate(_beginDate);		
		}}

		if (_endDate != null)
		{{
			$get('endDate').value = _endDate.localeFormat(_dateFormatString);
			var cdrDateEnd = $find(_cdrEndDateId);
			cdrDateEnd.setSelectedDate(_endDate);		
		}}
	}}
	
	function saveAlbumInfo()
	{{
		document.body.style.cursor = 'wait';
		var albumEntity = new Gsp.AlbumWebEntity();
		albumEntity.Id = _albumId;
		albumEntity.Title = $get(_txtTitleId).value;
		albumEntity.Summary = $get('albumSummary').value;
		albumEntity.IsPrivate = $get('private').checked;
		
		if (typeof (cboOwner) != 'undefined')
			albumEntity.Owner = cboOwner.get_text(); 

		var beginDate = Date.parseLocale($get('beginDate').value, _dateFormatString);
		if (beginDate != null)
		{{
			var tzo = (beginDate.getTimezoneOffset()/60)*(-1);
			if (tzo > 0)
			{{
				beginDate.setHours(beginDate.getHours() + tzo);
			}}
			_beginDate = beginDate;
			albumEntity.DateStart = beginDate;
		}}
		else
		{{
			_beginDate = null;
		}}

		var endDate = Date.parseLocale($get('endDate').value, _dateFormatString);
		if (endDate != null)
		{{
			var tzo = (endDate.getTimezoneOffset()/60)*(-1);
			if (tzo > 0)
			{{
				endDate.setHours(endDate.getHours() + tzo);
			}}
			_endDate = endDate;
			albumEntity.DateEnd = endDate;
		}}
		else
		{{
			_endDate = null;
		}}

		Gsp.Gallery.UpdateAlbumInfo(albumEntity, updateAlbumInfoCompleted);
		dgEditAlbum.close();
	}}

	function updateAlbumInfoCompleted(results, context, methodName)
	{{
		$get('{4}').innerHTML = results.Title;
		$get('{5}').innerHTML = results.Summary;
		
		UpdateDateLabel(_beginDate, _endDate);

		setText($get('currentAlbumLink'), results.Title);

		if (results.IsPrivate)
			Sys.UI.DomElement.removeCssClass($get('albumPrivateIcon'), 'gsp_invisible');
		else
			Sys.UI.DomElement.addCssClass($get('albumPrivateIcon'), 'gsp_invisible');

		document.body.style.cursor = 'default';
	}}

	function UpdateDateLabel(beginDate, endDate)
	{{
		var dateLabelText = '';
		var showDate = true;
		if ((_beginDate != null) && (_endDate != null))
			//Both dates are valid.  Combine them.
			dateLabelText = _beginDate.localeFormat(_dateFormatString) + ' {6} ' + _endDate.localeFormat(_dateFormatString);
		else if (_beginDate != null)
			dateLabelText = _beginDate.localeFormat(_dateFormatString);
		else if (_endDate != null)
			dateLabelText = _endDate.localeFormat(_dateFormatString);
		else
			showDate = false;

		$get('{7}').innerHTML = dateLabelText;

		var dateElement = $get('{8}');
		if (showDate)
		{{
			Sys.UI.DomElement.removeCssClass(dateElement, 'gsp_invisible');
			Sys.UI.DomElement.addCssClass(dateElement, 'visible');
		}}
		else
		{{
			Sys.UI.DomElement.removeCssClass(dateElement, 'gsp_visible');
			Sys.UI.DomElement.addCssClass(dateElement, 'gsp_invisible');
		}}
	}}

	function setText(node, newText)
	{{
		var childNodes = node.childNodes;
		for (var i=0; i < childNodes.length; i++)
		{{
			node.removeChild(childNodes[i]);
		}}
		if ((newText != null) && (newText.length > 0))
			node.appendChild(document.createTextNode(newText));
	}}

				",
				lblTitle.ClientID, // 0
				Resources.GalleryServerPro.UC_Album_Header_Dialog_Title_Edit_Album, // 1
				lblSummary.ClientID, // 2
				album.Parent.IsPrivate.ToString().ToLowerInvariant(), // 3
				lblTitle.ClientID, // 4
				lblSummary.ClientID, // 5
				Resources.GalleryServerPro.UC_Album_Header_Album_Date_Range_Separator_Text, // 6
				lblDate.ClientID, // 7
				dateContainer.ClientID // 8
				);
			}

			ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "albumHeaderScript", script, true);
		}

		#endregion
	}
}