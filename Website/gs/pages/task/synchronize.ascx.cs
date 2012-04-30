using System;
using System.Globalization;
using System.Web.UI;
using GalleryServerPro.Business;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Web.Pages.Task
{
	/// <summary>
	/// A page-like user control that handles the Synchronize task.
	/// </summary>
	public partial class synchronize : Pages.TaskPage
	{
		#region Event Handlers

		/// <summary>
		/// Handles the Init event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Init(object sender, EventArgs e)
		{
			this.TaskHeaderPlaceHolder = phTaskHeader;
			this.TaskFooterPlaceHolder = phTaskFooter;
		}

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			this.CheckUserSecurity(SecurityActions.Synchronize);

			ConfigureControls();

			RegisterJavascript();
		}

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			this.TaskHeaderText = Resources.GalleryServerPro.Task_Synch_Header_Text;
			this.TaskBodyText = String.Empty;
			this.OkButtonText = Resources.GalleryServerPro.Task_Synch_Ok_Button_Text;
			this.OkButtonToolTip = Resources.GalleryServerPro.Task_Synch_Ok_Button_Tooltip;
			
			this.OkButtonTop.OnClientClick = "startSynch();return false;";
			this.OkButtonBottom.OnClientClick = "startSynch();return false;";

			this.PageTitle = Resources.GalleryServerPro.Task_Synch_Page_Title;

			dgSynch.AnimationDirectionElement = this.OkButtonBottom.ClientID;

			lblAlbumTitle.Text = this.GetAlbum().Title;

			if (!GallerySettings.ExtractMetadata)
			{
				chkRegenerateMetadata.Enabled = false;
				chkRegenerateMetadata.CssClass = "gsp_disabledtext";
			}

			string scriptUrl = Utils.GetUrl("/script/progress.js");
			ScriptManager sm = ScriptManager.GetCurrent(this.Page);
			if (sm != null)
				sm.Scripts.Add(new ScriptReference(scriptUrl));
			else
				throw new WebException("Gallery Server Pro requires a ScriptManager on the page.");
		}

		private void RegisterJavascript()
		{
			string taskSynchProgressSkippedObjectsMaxExceeded_Msg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Task_Synch_Progress_Skipped_Objects_Max_Exceeded_Msg, GlobalConstants.MaxNumberOfSkippedObjectsToDisplayAfterSynch);

			string script = String.Format(CultureInfo.InvariantCulture, @"

var _progressManager = null;
var _galleryId = {26};
var _synchId = '{0}';
var _albumId = {1};
var _synchStartTime;
var _totalFiles = 0;
var _curFile = '';
var _curFileIndex;
var _headerMsg = '';
var _errorMsg = '';
var _synchFailed = false;
var _synchCancelled = false;

function startSynch()
{{
	document.body.style.cursor = 'wait';

	_synchStartTime = new Date();
	Gsp.Gallery.Synchronize(_albumId, _synchId, $get('chkIncludeChildAlbums').checked, $get('chkOverwriteThumbnails').checked, $get('chkOverwriteCompressed').checked, $get('{2}').checked, synchCompleted, synchFailed);

	setText($get('synchPopupHeader'), '{3}');
	setText($get('status'), '{4}');
	setText($get('synchEtl'), '');
	setText($get('synchRate'), '');
	$get('errorMessage').innerHTML = '';
	$get('progressbar').style.width = '1%';
	$get('btnCancel').disabled = false;
	$get('btnClose').disabled = true;
	dgSynch.Show();
	if ($get('synchAnimation').style.visibility == 'hidden') $get('synchAnimation').style.visibility= 'visible';
	_progressManager.startMonitor(_galleryId, _synchId, 2000, checkProgressStarted, checkProgressComplete, updateProgressCompleted);
}}

function checkProgressStarted()
{{
	document.body.style.cursor = 'wait';
}}

function checkProgressComplete(results)
{{
	document.body.style.cursor = 'default';
	if (results.Status == 'Complete')
		_progressManager.stopMonitor();

	if (results.SynchId != _synchId)
		return;
		
	if (_totalFiles == 0) _totalFiles = results.TotalFileCount;
	_curFileIndex = (results.Status == 'SynchronizingFiles' ? results.CurrentFileIndex : _totalFiles);	
	if (results.CurrentFile != null)
		_curFile = results.CurrentFile;
	
	$get('progressbar').style.width = results.PercentComplete + '%';
	setText($get('synchEtl'), calculateSynchEtl());
	setText($get('synchRate'), calculateSynchRate());
	setText($get('status'), results.StatusForUI);
	$get('errorMessage').innerHTML = '<p class=\'gsp_fs\'>{21} <span class=\'gsp_msgfriendly\'>' + _curFile + '</span></p>';
}}

function synchFailed(results, context, methodName)
{{
	document.body.style.cursor = 'default';
	if (results.get_exceptionType() == 'GalleryServerPro.ErrorHandler.CustomExceptions.SynchronizationInProgressException')
	{{
		_headerMsg = '{5}';
		_errorMsg = '<p class=\'gsp_msgwarning\'>{6}</p>';
	}}
	else
	{{
		_headerMsg = '{7}';
		_errorMsg = _errorMsg + '<p class=\'gsp_fs\'>{21} <span class=\'gsp_msgfriendly\'>' + _curFile + '</span></p><p class=\'gsp_msgwarning gsp_fs\'>{8}' + results.get_exceptionType() + ': ' + results.get_message() + '</p><p class=\'gsp_fs\'>{22} ' + results.get_statusCode() + '</p><p class=\'gsp_fs\'>{23}</p><p class=\'gsp_fs\'>' + results.get_stackTrace() + '</p>';
	}}
	_progressManager.stopMonitor();
	_synchFailed = true;
}}

function synchCompletedUpdateUI(results, context, methodName)
{{
	// Synch finished! Update the UI.
	if (_totalFiles == 0) _totalFiles = results.TotalFileCount;

	if (!_synchFailed && !_synchCancelled)
	{{
		_headerMsg = '{9}';
		_curFileIndex = _totalFiles;
		if (_curFileIndex)
		{{
			setText($get('synchEtl'), calculateSynchEtl());
			setText($get('synchRate'), calculateSynchRate());
		}}
		if (results.SkippedFiles.length > 0)
		{{
			var skippedFiles = results.SkippedFiles;
			var sb = new Sys.StringBuilder();
			
			if (results.SkippedFiles.length >= {24})
				sb.append('<p class=\'gsp_msgwarning gsp_fs\'>{25}</p>');
			else
				sb.append('<p class=\'gsp_msgwarning gsp_fs\'>{10}' + skippedFiles.length + '{11}</p>');

			sb.append('<ul class=\'gsp_fs\'>');
			for (var i = 0; i < skippedFiles.length; i++)
			{{
				var skippedFile = skippedFiles[i];
				sb.append('<li>' + skippedFile.Key + ': <span class=\'gsp_msgdark\'>' + skippedFile.Value + '</span></li>');
			}}
			sb.append('</ul>');
			sb.append('<p class=\'gsp_msgfriendly gsp_fs\'>{12}</p>');
			_errorMsg = sb.toString();
		}}
	}}

	resetSynchDialog();
}}

function resetSynchDialog()
{{
	document.body.style.cursor = 'default';
	setText($get('status'), _headerMsg);
	setText($get('synchPopupHeader'), _headerMsg);
	$get('errorMessage').innerHTML = _errorMsg;
		
	$get('progressbar').style.width = '100%';
	$get('synchAnimation').style.visibility = 'hidden';
	$get('btnCancel').disabled = true;
	$get('btnClose').disabled = false;
	$get('btnClose').focus();
	_curFile = '';
	_curFileIndex = null;
	_totalFiles = 0;
	_headerMsg = '';
	_errorMsg = '';
	_synchFailed = false;
	_synchCancelled = false;
}}

function calculateSynchRate()
{{
	if (_curFileIndex == 0) return '';
	
	var synchRate = _curFileIndex / getElapsedTimeSeconds();
	return (Math.round(synchRate * 10) / 10).toFixed(1) + '{13}';
}}

function calculateSynchEtl()
{{
	if (_curFileIndex == 0) return '';
	if (_totalFiles == 0) return '{14}';
	
	var elapsedTime = getElapsedTimeSeconds();
	var estimatedTotalTime = (elapsedTime * _totalFiles) / _curFileIndex;
	var timeLeft = new Date(0,0,0,0,0,0,(estimatedTotalTime - elapsedTime) * 1000);
	var etl = '';
	if (timeLeft.getHours() > 0) etl = timeLeft.getHours() + '{15}';

	etl = etl + timeLeft.getMinutes() + '{16}' + timeLeft.getSeconds() + '{17}' + _curFileIndex + '{18}' + _totalFiles + '{19}';
	return etl;
}}

function cancelSynch()
{{
	_progressManager.abortTask(_galleryId, _synchId);

	_headerMsg = '{5}';
	_errorMsg = '<p class=\'gsp_msgwarning\'>{20}</p>';
	_synchCancelled = true;
}}

function synchPageLoad(sender, args)
{{
	_progressManager = new Gsp.Progress();
}}

function synchCompleted(results, context, methodName)
{{
	document.body.style.cursor = 'default';
	_progressManager.stopMonitor();
}}

function updateProgressCompleted()
{{
	//Called from Gsp.Progress.stopMonitor, which is called when synch method completes (either successfully or with error)
	document.body.style.cursor = 'wait';
	Gsp.Gallery.GetCurrentStatus(_galleryId, synchCompletedUpdateUI, synchCompletedUpdateUIFailure);
}}

function synchCompletedUpdateUIFailure(results, context, methodName)
{{
	// An error during the final call to GetCurrentStatus after the synch method finished. If _headerMsg is empty 
	// that means the synch successfully completed. If not empty, then an error had occurred and the resetSynchDialog 
	// function will be displaying the messages related to the first error, so let's not confuse things by showing another error.
	if (_headerMsg.length == 0)
	{{
		_headerMsg = '{9}';
		alert(results.get_message());
	}}
	resetSynchDialog();
}}

function closeSynchWindow()
{{
	dgSynch.close();
}}

function getElapsedTimeSeconds()
{{
	return ((new Date().getTime() - _synchStartTime.getTime()) / 1000);
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
				Guid.NewGuid().ToString(), // 0
				GetAlbumId(), // 1
				chkRegenerateMetadata.ClientID, // 2
				Resources.GalleryServerPro.Task_Synch_Progress_SynchInProgress_Hdr, // 3
				Resources.GalleryServerPro.Task_Synch_Progress_Status_SynchInProgress_Msg, // 4
				Resources.GalleryServerPro.Task_Synch_Progress_SynchCancelled_Hdr, // 5
				Resources.GalleryServerPro.Task_Synch_Progress_Status_SynchInProgressException_Msg, // 6
				Resources.GalleryServerPro.Task_Synch_Progress_SynchError_Hdr, // 7
				Resources.GalleryServerPro.Task_Synch_Progress_SynchError_Msg_Prefix, // 8
				Resources.GalleryServerPro.Task_Synch_Progress_SynchComplete_Hdr, // 9
				Resources.GalleryServerPro.Task_Synch_Progress_Skipped_Objects_Msg_1_of_3, // 10
				Resources.GalleryServerPro.Task_Synch_Progress_Skipped_Objects_Msg_2_of_3, // 11
				Resources.GalleryServerPro.Task_Synch_Progress_Skipped_Objects_Msg_3_of_3, // 12
				Resources.GalleryServerPro.Task_Synch_Progress_SynchRate_Units, // 13
				Resources.GalleryServerPro.Task_Synch_Progress_ETL_Initial_Value, // 14
				Resources.GalleryServerPro.Task_Synch_Progress_ETL_1_of_5, // 15
				Resources.GalleryServerPro.Task_Synch_Progress_ETL_2_of_5, // 16
				Resources.GalleryServerPro.Task_Synch_Progress_ETL_3_of_5, // 17
				Resources.GalleryServerPro.Task_Synch_Progress_ETL_4_of_5, // 18
				Resources.GalleryServerPro.Task_Synch_Progress_ETL_5_of_5, // 19
				Resources.GalleryServerPro.Task_Synch_Progress_SynchCancelled_Bdy, // 20
				Resources.GalleryServerPro.Task_Synch_Progress_Current_File_Hdr, // 21
				Resources.GalleryServerPro.Task_Synch_Progress_SynchError_Status_Code_Label, // 22
				Resources.GalleryServerPro.Task_Synch_Progress_SynchError_Stack_Trace_Label, // 23
				GlobalConstants.MaxNumberOfSkippedObjectsToDisplayAfterSynch, // 24
				taskSynchProgressSkippedObjectsMaxExceeded_Msg, // 25
				GalleryId // 26
				);

			ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "synchScript", script, true);
		}
		
		#endregion

		#region Private Static Methods


		#endregion
	}
}