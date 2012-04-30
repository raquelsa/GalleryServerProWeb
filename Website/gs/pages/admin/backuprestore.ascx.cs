using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using ComponentArt.Web.UI;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages.Admin
{
	/// <summary>
	/// A page-like user control for using the backup and restore feature.
	/// </summary>
	public partial class backuprestore : Pages.AdminPage
	{
		#region Protected Methods

		/// <summary>
		/// Handles the Init event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Init(object sender, EventArgs e)
		{
			this.AdminHeaderPlaceHolder = phAdminHeader;
			this.AdminFooterPlaceHolder = phAdminFooter;
		}

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!UserCanAdministerSite && UserCanAdministerGallery)
			{
				Utils.Redirect(PageId.admin_gallerysettings, "aid={0}", this.GetAlbumId());
			}

			this.CheckUserSecurity(SecurityActions.AdministerSite);

			Page.MaintainScrollPositionOnPostBack = true;

			if (!IsPostBack)
			{
				ConfigureControlsFirstPageLoad();
			}

			ConfigureControlsEveryPageLoad();
		}

		/// <summary>
		/// Handles the Click event of the btnExportData control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnExportData_Click(object sender, EventArgs e)
		{
			string backupFilename = "GalleryServerBackup_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture);

			string galleryData = HelperFunctions.ExportGalleryData(chkExportMembership.Checked, chkExportGalleryData.Checked);

			IMimeType mimeType = MimeType.LoadMimeType("dummy.zip");

			int bufferSize = AppSetting.Instance.MediaObjectDownloadBufferSize;
			byte[] buffer = new byte[bufferSize];

			Stream stream = null;
			try
			{
				// Create an in-memory ZIP file.
				stream = ZipUtility.CreateZipStream(galleryData, backupFilename + ".xml");

				// Send to user.
				Response.AddHeader("Content-Disposition", "attachment; filename=" + backupFilename + ".zip");

				Response.Clear();
				Response.ContentType = (mimeType != null ? mimeType.FullType : "application/octet-stream");
				Response.Buffer = false;

				stream.Position = 0;
				int byteCount;
				while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					if (Response.IsClientConnected)
					{
						Response.OutputStream.Write(buffer, 0, byteCount);
						Response.Flush();
					}
					else
					{
						return;
					}
				}
			}
			finally
			{
				if (stream != null)
					stream.Close();

				Response.End();
			}
		}

		/// <summary>
		/// Handles the Click event of the lbRemoveRestoreFile control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void lbRemoveRestoreFile_Click(object sender, EventArgs e)
		{
			DeletePreviouslyUploadedFile();

			ConfigureBackupFileInfo(null);
		}

		/// <summary>
		/// Handles the Uploaded event of the Upload1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ComponentArt.Web.UI.UploadUploadedEventArgs"/> instance containing the event data.</param>
		protected void Upload1_Uploaded(object sender, UploadUploadedEventArgs e)
		{
			//string[] importOptions = Upload1.CallbackParameter.Split(new char[] { '|' });
			//bool importMembership = Convert.ToBoolean(importOptions[0]);
			//bool importGalleryData = Convert.ToBoolean(importOptions[1]);

			DeletePreviouslyUploadedFile();

			string filePath = SaveFileToTempDirectory(e.UploadedFiles[0]);

			IBackupFile backupFile = new BackupFile(filePath);
			ValidateRestoreFile(backupFile);

			ConfigureBackupFileInfo(backupFile);

			if (!backupFile.IsValid)
				File.Delete(filePath);
		}

		/// <summary>
		/// Handles the Click event of the btnRestore control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnRestore_Click(object sender, EventArgs e)
		{
			string filePath = ViewState["FilePath"].ToString();
			string msg;
			Page.MaintainScrollPositionOnPostBack = false;

			try
			{
				if (File.Exists(filePath))
				{
					HelperFunctions.ImportGalleryData(File.ReadAllText(filePath), chkImportMembership.Checked, chkImportGalleryData.Checked);

					this.wwMessage.CssClass = "wwErrorSuccess gsp_msgfriendly gsp_bold";
					msg = Resources.GalleryServerPro.Admin_Backup_Restore_Db_Successfully_Restored_Msg;
					wwMessage.ShowMessage(msg);
				}
				else
				{
					wwMessage.CssClass = "wwErrorFailure gsp_msgwarning";
					msg = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Admin_Backup_Restore_Cannot_Restore_File_File_Not_Found_Msg, filePath);
					wwMessage.ShowError(msg);
				}
			}
			catch (Exception ex)
			{
				LogError(ex);
				this.wwMessage.CssClass = "wwErrorFailure gsp_msgwarning";
				msg = String.Concat(Resources.GalleryServerPro.Admin_Backup_Restore_Cannot_Restore_File_Label, ex.Message);
				wwMessage.ShowError(msg);
			}
			finally
			{
				DeletePreviouslyUploadedFile();

				ConfigureBackupFileInfo(null);

				HelperFunctions.PurgeCache();

				bool adviseUserToManuallyRestartApp = false;
				try
				{
					// Recycle the app to force the providers to re-initialize. This will query the application ID from the database, which
					// may have changed during the restore. If any errors occur, advise the user to manually restart the app.
					Utils.ForceAppRecycle();
				}
				catch (IOException) { adviseUserToManuallyRestartApp = true; }
				catch (UnauthorizedAccessException) { adviseUserToManuallyRestartApp = true; }
				catch (PlatformNotSupportedException) { adviseUserToManuallyRestartApp = true; }

				if (adviseUserToManuallyRestartApp)
				{
					this.wwMessage.CssClass = "wwErrorSuccess gsp_msgfriendly";
					msg = Resources.GalleryServerPro.Admin_Backup_Restore_Db_Successfully_Restored_AppNotRecycled_Msg;
					wwMessage.ShowMessage(msg);
				}
			}
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsFirstPageLoad()
		{
			OkButtonIsVisible = false;
			CancelButtonIsVisible = false;
			AdminPageTitle = Resources.GalleryServerPro.Admin_Backup_Restore_Page_Header;
		}

		private void ConfigureControlsEveryPageLoad()
		{
			//mpBackupRestore.SelectPageById(PageView2.ID);
			this.PageTitle = Resources.GalleryServerPro.Admin_Backup_Restore_Page_Header;

			tsBackupRestore.ImagesBaseUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/tabstrip/");
			tsBackupRestore.TopGroupSeparatorImagesFolderUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/tabstrip/");

			if (ComponentArtUploadControlSupported())
			{
				// Use ComponentArt's Upload control.
				AddUploadControlForFullTrust();
			}
			else
			{
				// Use ASP.NET's FileUpload control.
				AddUploadControlForLessThanFullTrust();
			}

			if (AppSetting.Instance.License.IsInReducedFunctionalityMode)
			{
				wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Need_Product_Key_Msg2);
				wwMessage.CssClass = "wwErrorSuccess gsp_msgwarning";
				btnExportData.Enabled = false;
				btnRestore.Enabled = false;
			}

		}

		/// <summary>
		/// Returns a value indicating whether the ComponentArt Upload control can be used. The control requires that
		/// the application be running in Full Trust and that the browser be Internet Explorer or Firefox.
		/// </summary>
		/// <returns>Returns a value indicating whether the ComponentArt Upload control can be used.</returns>
		private bool ComponentArtUploadControlSupported()
		{
			return false;
			bool isFullTrust = (AppSetting.Instance.AppTrustLevel == ApplicationTrustLevel.Full);
			string browser = Request.Browser.Browser;
			bool isSupportedBrowser = (browser.Equals("IE", StringComparison.OrdinalIgnoreCase) ||
																 browser.Equals("FIREFOX", StringComparison.OrdinalIgnoreCase));

			return (isFullTrust & isSupportedBrowser);
		}

		private void AddUploadControlForFullTrust()
		{
			const string htmlBeforeUpload = @"<div class='sel gsp_addtopmargin5'>";
			const string htmlAfterUpload = @"</div>";

			Upload upload = new Upload();

			upload.ID = "Upload1";
			upload.MaximumFileCount = 1;
			upload.AutoPostBack = true;
			upload.TempFileFolder = AppSetting.Instance.TempUploadDirectory;
			upload.MaximumUploadSize = 2097151;
			upload.FileInputClientTemplateId = "FileInputTemplate";
			upload.FileInputImageUrl = Utils.GetUrl("/images/componentart/upload/transparent.gif");
			upload.FileInputHoverImageUrl = Utils.GetUrl("/images/componentart/upload/transparent.gif");
			upload.ProgressClientTemplateId = "ProgressTemplate";
			upload.ProgressDomElementId = "upload-progress";

			upload.Uploaded += Upload1_Uploaded;

			phUpload.Controls.Add(new LiteralControl(htmlBeforeUpload));
			phUpload.Controls.Add(upload);
			phUpload.Controls.Add(new LiteralControl(htmlAfterUpload));

			AddUploadClientTemplates(upload);

			AddUploadDialogClientTemplate();
		}

		private void AddUploadControlForLessThanFullTrust()
		{
			const string htmlBeforeUpload = @"<p>";
			const string htmlAfterUpload = @"</p>";

			FileUpload upload = new FileUpload();
			upload.ID = "fuUpload1";
			upload.Attributes.Add("title", Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_File_Browse_Button_Tooltip);
			upload.Attributes.Add("size", "45");

			Button uploadButton = new Button();
			uploadButton.ID = "btnUpload";
			uploadButton.Text = Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_File_Button_Text;
			uploadButton.ToolTip = Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_File_Button_Tooltip;
			uploadButton.Click += uploadButton_Click;

			phUpload.Controls.Add(new LiteralControl(htmlBeforeUpload));
			phUpload.Controls.Add(upload);
			phUpload.Controls.Add(new LiteralControl("&nbsp;"));
			phUpload.Controls.Add(uploadButton);
			phUpload.Controls.Add(new LiteralControl(htmlAfterUpload));
		}

		protected void uploadButton_Click(object sender, EventArgs e)
		{
			DeletePreviouslyUploadedFile();
			FileUpload uploadedFile = (FileUpload)phUpload.FindControl("fuUpload1");
			string filePath = SaveFileToTempDirectory(uploadedFile);

			IBackupFile backupFile = new BackupFile(filePath);
			ValidateRestoreFile(backupFile);

			ConfigureBackupFileInfo(backupFile);

			if (!backupFile.IsValid)
				File.Delete(filePath);
		}

		private static string SaveFileToTempDirectory(UploadedFileInfo fileToRestore)
		{
			// Save file to temp directory, ensuring that we are not overwriting an existing file. If the uploaded file is a ZIP archive,
			// extract the embedded XML file and save that.
			string filePath;
			if (Path.GetExtension(fileToRestore.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
			{
				using (ZipUtility zip = new ZipUtility(Utils.UserName, RoleController.GetGalleryServerRolesForUser()))
				{
					filePath = zip.ExtractNextFileFromZip(fileToRestore.GetStream(), AppSetting.Instance.TempUploadDirectory);
				}
			}
			else
			{
				string fileName = HelperFunctions.ValidateFileName(AppSetting.Instance.TempUploadDirectory, fileToRestore.FileName);
				filePath = Path.Combine(AppSetting.Instance.TempUploadDirectory, fileName);

				fileToRestore.SaveAs(filePath);
			}

			return filePath;
		}

		private string SaveFileToTempDirectory(FileUpload fileToRestore)
		{
			// Save file to temp directory, ensuring that we are not overwriting an existing file. If the uploaded file is a ZIP archive,
			// extract the embedded XML file and save that.
			string filePath;
			if (Path.GetExtension(fileToRestore.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
			{
				using (ZipUtility zip = new ZipUtility(Utils.UserName, GetGalleryServerRolesForUser()))
				{
					filePath = zip.ExtractNextFileFromZip(fileToRestore.FileContent, AppSetting.Instance.TempUploadDirectory);
				}
			}
			else
			{
				string fileName = HelperFunctions.ValidateFileName(AppSetting.Instance.TempUploadDirectory, fileToRestore.FileName);
				filePath = Path.Combine(AppSetting.Instance.TempUploadDirectory, fileName);

				fileToRestore.SaveAs(filePath);
			}

			return filePath;
		}

		private static void ValidateRestoreFile(IBackupFile backupFile)
		{
			if (Path.GetExtension(backupFile.FilePath).ToLowerInvariant() == ".xml")
				HelperFunctions.ValidateBackupFile(backupFile);
		}

		private void ConfigureBackupFileInfo(IBackupFile backupFile)
		{
			if (backupFile == null)
			{
				lblRestoreFilename.Text = Resources.GalleryServerPro.Admin_Backup_Restore_File_Not_Uploaded_Msg;
				lblRestoreFilename.CssClass = "gsp_msgwarning";
				lblNumApps.Text = String.Empty;
				lblNumProfiles.Text = String.Empty;
				lblNumRoles.Text = String.Empty;
				lblNumMembers.Text = String.Empty;
				lblNumUsers.Text = String.Empty;
				lblNumUsersInRoles.Text = String.Empty;
				lblNumGalleries.Text = String.Empty;
				lblNumAlbums.Text = String.Empty;
				lblNumMediaObjects.Text = String.Empty;
				lblNumMetadata.Text = String.Empty;
				lblNumRoleAlbums.Text = String.Empty;
				lblNumAppSettings.Text = String.Empty;
				lblNumGalleryControlSettings.Text = String.Empty;
				lblNumGallerySettings.Text = String.Empty;
				lblNumBrowserTemplates.Text = String.Empty;
				lblNumMimeTypes.Text = String.Empty;
				lblNumMimeTypeGalleries.Text = String.Empty;
				lblNumGalleryRoles.Text = String.Empty;
				lblNumAppErrors.Text = String.Empty;
				lblNumUserGalleryProfiles.Text = String.Empty;

				btnRestore.Enabled = false;
				imgValidationResult.Visible = false;
				lblValidationResult.Text = String.Empty;
				lblValidationResult.CssClass = String.Empty;
				lbRemoveRestoreFile.Visible = false;

				return;
			}

			lblRestoreFilename.Text = Path.GetFileName(backupFile.FilePath);

			string[] tableNames = new string[]
				{
					"aspnet_Applications", "aspnet_Profile", "aspnet_Roles", "aspnet_Membership", "aspnet_Users", "aspnet_UsersInRoles", "gs_Gallery", "gs_Album", 
					"gs_MediaObject", "gs_MediaObjectMetadata", "gs_Role_Album", "gs_Role", "gs_AppError", "gs_AppSetting", "gs_GalleryControlSetting", 
					"gs_GallerySetting", "gs_BrowserTemplate", "gs_MimeType", "gs_MimeTypeGallery", "gs_UserGalleryProfile"
				};

			Dictionary<string, int> dataRecords = backupFile.DataTableRecordCount;

			foreach (string tableName in tableNames)
			{
				switch (tableName)
				{
					case "aspnet_Applications":
						lblNumApps.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);

						chkImportMembership.Checked = (dataRecords.ContainsKey(tableName) && backupFile.DataTableRecordCount[tableName] > 0);
						break;
					case "aspnet_Profile":
						lblNumProfiles.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "aspnet_Roles":
						lblNumRoles.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "aspnet_Membership":
						lblNumMembers.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "aspnet_Users":
						lblNumUsers.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "aspnet_UsersInRoles":
						lblNumUsersInRoles.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_Gallery":
						lblNumGalleries.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);

						chkImportGalleryData.Checked = (dataRecords.ContainsKey(tableName) && backupFile.DataTableRecordCount[tableName] > 0);
						break;
					case "gs_Album":
						lblNumAlbums.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_MediaObject":
						lblNumMediaObjects.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_MediaObjectMetadata":
						lblNumMetadata.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_Role_Album":
						lblNumRoleAlbums.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_Role":
						lblNumGalleryRoles.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_AppSetting":
						lblNumAppSettings.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_GalleryControlSetting":
						lblNumGalleryControlSettings.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_GallerySetting":
						lblNumGallerySettings.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_BrowserTemplate":
						lblNumBrowserTemplates.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_MimeType":
						lblNumMimeTypes.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_MimeTypeGallery":
						lblNumMimeTypeGalleries.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_AppError":
						lblNumAppErrors.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
					case "gs_UserGalleryProfile":
						lblNumUserGalleryProfiles.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
						break;
				}
			}

			if (backupFile.IsValid)
			{
				btnRestore.Enabled = true && !AppSetting.Instance.License.IsInReducedFunctionalityMode;
				imgValidationResult.ImageUrl = Utils.GetUrl("/images/info.gif");
				imgValidationResult.Visible = true;
				lblValidationResult.Text = Resources.GalleryServerPro.Admin_Backup_Restore_File_Valid_Msg;
				lblValidationResult.CssClass = "gsp_msgsuccess";
				lblRestoreFilename.CssClass = "gsp_msgattention";
				lbRemoveRestoreFile.Visible = true;
				lblSchemaVersion.Text = backupFile.SchemaVersion;

				ViewState["FilePath"] = backupFile.FilePath;
			}
			else
			{
				btnRestore.Enabled = false;
				imgValidationResult.ImageUrl = Utils.GetUrl("/images/warning.gif");
				imgValidationResult.Visible = true;
				lblValidationResult.Text = Resources.GalleryServerPro.Admin_Backup_Restore_File_Not_Valid_Msg;
				lblValidationResult.CssClass = "gsp_msgfailure";
				lblRestoreFilename.CssClass = "gsp_msgattention";
				lbRemoveRestoreFile.Visible = false;
				lblSchemaVersion.Text = backupFile.SchemaVersion;
			}
		}

		private void DeletePreviouslyUploadedFile()
		{
			string filePath = ViewState["FilePath"] as string;

			if (!String.IsNullOrEmpty(filePath))
			{
				File.Delete(filePath);

				ViewState["FilePath"] = null;
			}
		}

		/// <summary>
		/// Adds client templates for the specified Upload control.
		/// </summary>
		private static void AddUploadClientTemplates(Upload upload)
		{
			#region File input client template

			// Add client template for the file input section (includes input tag and browse and upload buttons).
			ClientTemplate uploadFileInputClientTemplate = new ClientTemplate();

			uploadFileInputClientTemplate.ID = "FileInputTemplate";

			uploadFileInputClientTemplate.Text = String.Format(CultureInfo.InvariantCulture, @"
<div class=""file"">
	<div class='## DataItem.FileName ? ""filename"" : ""filename empty""; ##'>
		<input value='## DataItem.FileName ? DataItem.FileName : ""{0}""; ##'
			onfocus=""this.blur();"" /></div>
	<a href=""javascript:void(0);"" onclick=""this.blur();return false;"" class=""browse""
		title=""{1}"">{2}#$FileInputImage</a> <input type=""button"" onclick=""init_upload(Upload1);this.blur();return false;"" value=""{3}"" title=""{4}"" class=""upload"" />
</div>
",
			Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_File_Input_Text,
			Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_File_Browse_Button_Tooltip,
			Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_File_Browse_Text,
			Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_File_Button_Text,
			Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_File_Button_Tooltip
			);

			upload.ClientTemplates.Add(uploadFileInputClientTemplate);

			#endregion

			#region Upload progress client template

			// Add client template for the progress section (includes input tag and browse and upload buttons).
			ClientTemplate uploadProgressClientTemplate = new ClientTemplate();

			uploadProgressClientTemplate.ID = "ProgressTemplate";

			uploadProgressClientTemplate.Text = String.Format(CultureInfo.InvariantCulture, @"
<!-- Dialogue contents -->
<div class=""con"">
	<div class=""stat"">
		<p class=""gsp_h3"" rel=""total"">
			{0} <span class=""red"">## DataItem.CurrentFile; ##</span></p>
		<div class=""prog"">
			<div class=""con"">
				<div class=""bar"" style=""width: ## get_percentage(DataItem.Progress) ##%;"">
				</div>
			</div>
		</div>
		<div class=""lbl"">
			<strong>## format_file_size(DataItem.ReceivedBytes) ##</strong> (## get_percentage(DataItem.Progress)
			##%) {1}</div>
	</div>
</div>
<!-- /Dialogue contents -->
<!-- Dialogue footer -->
<div class=""ftr"">
	<div class=""ftr-l"">
	</div>
	<div class=""ftr-m"">
		<div class=""info"" id=""info1"">
			<span>{2} <strong>## format_time(DataItem.ElapsedTime); ##</strong></span>
			<span style=""padding-left: 8px;"">{3} <strong>## format_time(DataItem.ElapsedTime
				+ DataItem.RemainingTime); ##</strong></span> <span style=""padding-left: 8px;"">{4}
					<strong>## DataItem.Speed.toFixed(2) ## {5}</strong></span>
		</div>
		<div class=""btns"">
			<a onclick=""Upload1.abort();UploadDialog.close();this.blur();return false;"" href=""javascript:void(0);""
				rel=""cancel""><span class=""l""></span><span class=""m"" id=""btn1"">{6}</span>
				<span class=""r""></span></a>
		</div>
	</div>
	<div class=""ftr-r"">
	</div>
</div>
<!-- /Dialogue footer -->
",
					Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_Filename_Label, // 0
					Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_Bytes_Uploaded_Suffix, // 1
					Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_Elapsed_Time_Label, // 2
					Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_Estimated_Time_Label, // 3
					Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_Speed_Label, // 4
					Resources.GalleryServerPro.Site_KiloBytes_Per_Second_Abbreviation, // 5
					Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_Cancel_Upload_Text // 6
				);

			upload.ClientTemplates.Add(uploadProgressClientTemplate);

			#endregion
		}

		private void AddUploadDialogClientTemplate()
		{
			// Add loading panel client template
			ComponentArt.Web.UI.ClientTemplate uploadDialogClientTemplate = new ComponentArt.Web.UI.ClientTemplate();

			uploadDialogClientTemplate.ID = "UploadContent";

			uploadDialogClientTemplate.Text = String.Format(CultureInfo.InvariantCulture, @"
<div class=""ttl"" onmousedown=""UploadDialog.StartDrag(event);"">
	<div class=""ttlt"">
		<div class=""ttlt-l"">
		</div>
		<div class=""ttlt-m"">
			<a class=""close"" href=""javascript:void(0);"" onclick=""Upload1.abort();UploadDialog.close();this.blur();return false;"">
			</a><span>{0}</span>
		</div>
		<div class=""ttlt-r"">
		</div>
	</div>
	<div class=""ttlb"">
		<div class=""ttlb-l"">
		</div>
		<div class=""ttlb-m"">
		</div>
		<div class=""ttlb-r"">
		</div>
	</div>
</div>
<!-- for contents & footer, see upload progress client template -->
<div id=""upload-progress"">
</div>

",
				Resources.GalleryServerPro.Admin_Backup_Restore_Restore_Tab_Upload_Dialog_Uploading_Text);

			UploadDialog.ClientTemplates.Add(uploadDialogClientTemplate);

			UploadDialog.Visible = true;
		}

		#endregion
	}
}