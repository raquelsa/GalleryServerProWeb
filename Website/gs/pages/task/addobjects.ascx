<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="addobjects.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Task.addobjects" %>
<%@ Register TagPrefix="ComponentArt" Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" %>
<%@ Register TagPrefix="tis" Namespace="GalleryServerPro.WebControls" Assembly="GalleryServerPro.WebControls" %>
<%@ Register TagPrefix="uc1" TagName="popup" Src="../../controls/popupinfo.ascx" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<div class="gsp_content">
	<asp:PlaceHolder ID="phTaskHeader" runat="server" />
	<tis:wwErrorDisplay ID="wwMessage" runat="server" CellPadding="2" UseFixedHeightWhenHiding="False"
		Center="False" Width="500px">
	</tis:wwErrorDisplay>
	<div class="gsp_addleftpadding1">
		<ComponentArt:TabStrip ID="tsAddObjects" MultiPageId="mpAddObjects" TopGroupCssClass="ts0TopGroup"
			DefaultSelectedItemLookId="SelectedTab" DefaultItemLookId="DefaultTab" DefaultChildSelectedItemLookId="SelectedTab"
			TopGroupShowSeparators="true" DefaultGroupSeparatorWidth="22" DefaultGroupSeparatorHeight="22"
			DefaultGroupFirstSeparatorWidth="15" DefaultGroupFirstSeparatorHeight="22" DefaultGroupLastSeparatorWidth="22"
			DefaultGroupLastSeparatorHeight="22" Width="100%" runat="server">
			<ClientEvents>
			</ClientEvents>
			<ItemLooks>
				<ComponentArt:ItemLook LookId="DefaultTab" CssClass="ts0DefaultTab" HoverCssClass="ts0DefaultTabHover"
					LabelPaddingLeft="10" LabelPaddingRight="17" LabelPaddingTop="2" LabelPaddingBottom="6" />
				<ComponentArt:ItemLook LookId="SelectedTab" CssClass="ts0SelectedTab" LabelPaddingLeft="10"
					LabelPaddingRight="17" LabelPaddingTop="2" LabelPaddingBottom="6" />
				<ComponentArt:ItemLook LookId="DisabledTab" CssClass="ts0DefaultTab" LabelPaddingLeft="10"
					LabelPaddingRight="17" LabelPaddingTop="2" LabelPaddingBottom="6" />
			</ItemLooks>
			<Tabs>
				<ComponentArt:TabStripTab ID="tabLocalMedia" runat="server" Visible="false" Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_Local_Media_Tab_Title %>" />
				<ComponentArt:TabStripTab ID="tabExternal" runat="server" Visible="false" Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Tab_Title %>" />
			</Tabs>
		</ComponentArt:TabStrip>
		<ComponentArt:MultiPage ID="mpAddObjects" CssClass="mp3MultiPage" runat="server">
			<ComponentArt:PageView ID="pvAddLocal" CssClass="mp3MultiPageContent" runat="server">
				<p class="gsp_h3">
					<asp:Literal ID="l1a" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_Local_Media_Tab_Hdr %>" /></p>
				<p class="gsp_addtopmargin5">
					<asp:Label ID="lblLocalMediaOverview" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_Local_Media_Tab_Dtl %>" /></p>
				<div id="uploader">
					<p style="width:100%;height:150px;text-align:center;padding-top:100px;">Loading...&nbsp;<img src="<%=Utils.GetUrl("/images/wait-squares.gif")%>" alt=""/></p>
				</div>
				<div id="pnlOptionsHdr">
					<div style="padding: 10px 5px 5px 0; cursor: pointer; vertical-align: middle;" title='<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Site_Options_Tooltip %>" />'>
						<img src="<%=Utils.GetUrl("/images/expand.jpg")%>" alt=""/>
						<span class="gsp_bold">
							<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:GalleryServerPro, Site_Options_Hdr %>" /></span>
						<asp:Label ID="lblExpCol" runat="server" CssClass="gsp_fs" />
					</div>
				</div>
				<div id="pnlOptions">
					<div class="gsp_fs">
						<p>
							<asp:CheckBox ID="chkDiscardOriginal" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_Discard_Original_File_Option_Text %>"
								ClientIDMode="Static" /></p>
						<p>
							<asp:CheckBox ID="chkDoNotExtractZipFile" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_Do_Not_Extract_Zip_File_Option_Text %>"
								ClientIDMode="Static" /></p>
					</div>
				</div>
			</ComponentArt:PageView>
			<ComponentArt:PageView ID="pvAddExternal" CssClass="mp3MultiPageContent" runat="server"
				Height="300">
				<p class="gsp_h3">
					<asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Tab_Hdr %>" />
				</p>
				<p>
					<asp:Label ID="lblExternalOverview" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Tab_Dtl %>" /></p>
				<table class="gsp_standardTable gsp_addtopmargin5" style="width: 100%;">
					<tr>
						<td style="width: 125px;">
							<asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Tab_Type_Label %>" />
						</td>
						<td>
							<asp:DropDownList ID="ddlMediaTypes" runat="server" Width="200">
								<asp:ListItem Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Tab_Type_Audio %>"
									Value="Audio" />
								<asp:ListItem Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Tab_Type_Image %>"
									Value="Image" />
								<asp:ListItem Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Tab_Type_Video %>"
									Value="Video" Selected="True" />
								<asp:ListItem Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Tab_Type_Other %>"
									Value="Other" />
							</asp:DropDownList>
						</td>
					</tr>
					<tr>
						<td style="width: 125px;">
							<asp:Label ID="lblTitle" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Tab_Title_Label %>" />
						</td>
						<td>
							<asp:TextBox ID="txtTitle" runat="server" Style="width: 92%;" />
						</td>
					</tr>
					<tr>
						<td class="gsp_aligntop">
							<asp:Label ID="lblExternalHtmlSource" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Tab_Html_Label %>" />
						</td>
						<td>
							<asp:TextBox ID="txtExternalHtmlSource" runat="server" Rows="5" TextMode="MultiLine"
								Style="width: 92%;" />
						</td>
					</tr>
				</table>
				<tis:PopupInfo ID="PopupInfo1" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
					DefaultDialogBodyCss="dg5ContentBodyCss">
					<PopupInfoItems>
						<tis:PopupInfoItem ID="poi1" runat="server" ControlId="chkDiscardOriginal" DialogTitle="<%$ Resources:GalleryServerPro, Task_Add_Objects_Discard_Original_File_Hdr %>"
							DialogBody="<%$ Resources:GalleryServerPro, Task_Add_Objects_Discard_Original_File_Bdy %>" />
						<tis:PopupInfoItem ID="poi2" runat="server" ControlId="chkDoNotExtractZipFile" DialogTitle="<%$ Resources:GalleryServerPro, Task_Add_Objects_ZipOption_Hdr %>"
							DialogBody="<%$ Resources:GalleryServerPro, Task_Add_Objects_ZipOption_Bdy %>" />
						<tis:PopupInfoItem ID="poi3" runat="server" ControlId="lblExternalOverview" DialogTitle="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Object_Overview_Hdr %>"
							DialogBody="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Object_Overview_Bdy %>" />
						<tis:PopupInfoItem ID="poi4" runat="server" ControlId="ddlMediaTypes" DialogTitle="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Object_Type_Hdr %>"
							DialogBody="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Object_Type_Bdy %>" />
						<tis:PopupInfoItem ID="poi5" runat="server" ControlId="lblTitle" DialogTitle="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Object_Title_Hdr %>"
							DialogBody="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Object_Title_Bdy %>" />
						<tis:PopupInfoItem ID="poi6" runat="server" ControlId="lblExternalHtmlSource" DialogTitle="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Object_Html_Hdr %>"
							DialogBody="<%$ Resources:GalleryServerPro, Task_Add_Objects_External_Object_Html_Bdy %>" />
					</PopupInfoItems>
				</tis:PopupInfo>
				<uc1:popup ID="ucPopupContainer" runat="server" />
			</ComponentArt:PageView>
		</ComponentArt:MultiPage>
	</div>
	<asp:PlaceHolder ID="phTaskFooter" runat="server" />
</div>
<div runat="server">
	<script type="text/javascript">

		var _fileProcessedCount = 0;
		var _isError = false; // Set to true if any of the uploads failed (or a file is skipped)
				
		$(function() {
			onPageLoad();
		});

		var onPageLoad = function() {
			configureEventHandlers();
			configureAddFile();
			configureAddHtmlSnippet();
		};
		
		var configureEventHandlers = function() {
			$(".gsp_btnOkTop, .gsp_btnOkBottom").click(function(e) {
				$(".gsp_spinner").show();
				$(".gsp_spinner_msg").text("<%= AddObjectsUploadingText %>").show();

				if (tsAddObjects.GetSelectedTab().get_id() == "tabLocalMedia") {
					uploadFiles(e);
				}
				else {
					uploadHtmlSnippet(e);	
				}
			});
		};

		var uploadFiles = function(e) {
			// Begin the upload of files to the server.
			var uploader = $('#uploader').pluploadQueue();
			var discardOriginal = $("#chkDiscardOriginal").prop("checked");
			
			if (discardOriginal) {
				uploader.settings.resize = {
					width : <%= GallerySettings.MaxOptimizedLength %>, 
					height : <%= GallerySettings.MaxOptimizedLength %>,
					quality : <%= GallerySettings.OptimizedImageJpegQuality %>
				};
			}
			
			// Files in queue upload them first
			if (uploader.files.length > 0) {
				uploader.start();
			} else {
				$(".gsp_spinner, .gsp_spinner_msg").hide();
				alert('<%= AddObjectsInstruction %>');
			}
			e.preventDefault();
		};

		var uploadHtmlSnippet = function(e) {
			// Validate that the user entered an HTML snippet, cancelling the postback if necessary.
			var htmlSnippet = $("#<%= txtExternalHtmlSource.ClientID %>").val();
			if ((htmlSnippet == null) || (htmlSnippet.length == 0)) {
				$(".gsp_spinner, .gsp_spinner_msg").hide();
				alert("Enter an HTML fragment / embed code.")
				e.preventDefault();
			}
		};
		
		var configureAddFile = function() {
				$('#pnlOptions').hide();
				$('#pnlOptionsHdr').click(function () {
					$('#pnlOptions').slideToggle('fast');
				});

			//				chunk_size: '2mb',
//				resize: { width: 640, height: 640, quality: 90 },

			$("#uploader").pluploadQueue({
					//runtimes: 'html4',
					runtimes: 'html5,silverlight,flash,browserplus,html4',
					url: '<%=Utils.GetUrl(String.Format(CultureInfo.InvariantCulture, "/handler/upload.ashx?aid={0}", GetAlbumId()))%>',
					flash_swf_url: '<%=Utils.GetUrl("/script/plupload/plupload.flash.swf")%>',
					silverlight_xap_url: '<%=Utils.GetUrl("/script/plupload/plupload.silverlight.xap")%>',
					unique_names: true,
					chunk_size: '2mb',
					init: {
						FileUploaded: onFileUpload,
						StateChanged: onStateChanged,
						Error: onUploadError
					}
				});
		};

		var configureAddHtmlSnippet = function() {
			// Nothing to do here...
		};

		var redirectToAlbum = function(msgId) {
			purgeCache();
			var albumUrl = '<%= Utils.GetUrl(PageId.album, "aid={0}", GetAlbumId()) %>';
			if (msgId && msgId > 0)
				window.location = albumUrl + '&msg=' + msgId;
			else 
				window.location = albumUrl;
		};
		
		var onStateChanged = function(up) {
			if (up.state == plupload.STOPPED) {
				// Fail-safe: Normally, we redirect after the web service finishes processing the file (onFileProcessed),
				// but just in case it doesn't return or the file counter gets messed up, set a timer to redirect to 
				// the album a few seconds after the last file upload is complete, thus guaranteeing the page will never
				// get stuck with a "Processing. Please wait..." message.
				window.setTimeout(redirectToAlbum, 20000);
			}
		};
		
		var onUploadError = function(up, args) {
			if (console) console.log("(" + args.file.name + ") " + args.message + " " + args.details);
			_isError = true;
			onFileComplete();
		};
		
		var onFileUpload = function(up, file, info) {
			// File has been transferred to the server; now call web service to copy file to destination album and create media object record.
			var getData = function() {
				var settings = { };
				
				settings.FileName = file.name;
				settings.FileNameOnServer = file.target_name;
				settings.AlbumId = <%=this.GetAlbumId()%>;
				settings.DiscardOriginalFile = $("#chkDiscardOriginal").prop("checked");
				settings.ExtractZipFile = !$("#chkDoNotExtractZipFile").prop("checked");

				return "{ 'settings': " + JSON.stringify(settings) + " }";
			};

			// Call we service to move uploaded file to destination and add to gallery.
			$.ajax(({
					type: "POST",
					url: '<%=Utils.GetUrl("/services/Gallery.asmx")%>/AddMediaObject',
					data: getData(),
					contentType: "application/json; charset=utf-8",
					dataType: "json",
					success: function(response) { onFileProcessed(file, response.d); },
					error: function(response) { _isError = true;onFileComplete(); }
				}));
		};

		var onFileProcessed = function(file, actionResults) {
			// Invoked after the web service has successfully processed an uploaded file. actionResults is an array 
			// of ActionResult objects.
			for (var i = 0; i < actionResults.length; i++) {
				if (actionResults[i].Status == 2) { //2=error
					_isError = true; break;
				}
			}
			
			_fileProcessedCount++;
			onFileComplete();
		};

		var onFileComplete = function() {
			// Invoked when a file has either failed to upload/be processed in some way or has successfully been uploaded and processed
			var uploader = $('#uploader').pluploadQueue();
			if (_fileProcessedCount + uploader.total.failed >= uploader.files.length) {
				if (_isError)
					redirectToAlbum(<%= (int)Message.ObjectsSkippedDuringUpload %>);
				else
					redirectToAlbum();
			}
		};

		var purgeCache = function() {
			$.ajax(({
				type: "POST",
				url: '<%=Utils.GetUrl("/services/Gallery.asmx")%>/PurgeCache',
				data: "{ 'albumId': " + JSON.stringify(<%=GetAlbumId()%>) + " }",
				contentType: "application/json; charset=utf-8",
				dataType: "json"
			}));
		};
		
	</script>
</div>
