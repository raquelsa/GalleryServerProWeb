/* ------------ ATTENTION! -----------------
This is the original, un-minified version of mediaobjectview.js. This script is rendered to the browser
when the project is compiled in Debug mode; the minified version is sent when the project has any other
compilation configuration.
TO MODIFY: Make changes to this file and test locally under the Debug compilation configuration. When 
finished, run this text through a javascript minifier and copy the output to mediaobjectview.js. 
There is an online minifier at http://www.refresh-sf.com/yui/.
------------ ATTENTION! ----------------- */
var _timer = null; // Used in slideshow
var _ssInProgress = false; // Slideshow is running
var _moInfo; // Client version of server class MediaObjectWebEntity
var _moCacheImg = new Image(); // Prefetches next MO so it is already downloaded when user clicks next
var _inPrefetch = true; // Set to true during prefetch callback to server
var _nextMORequested = false; // Set to true when user requests next MO but callback hasn't been returned from prefetching the data
var _prevMORequested = false; // Set to true when user requests previous MO but callback hasn't been returned from prefetching the data
var _showCurrentMO = false; // Set to true when user requests the current MO to be re-rendered, possibly with new settings (like hi-res)
var _mediaObjectsToDispose = new Array(); // Contains AJAX components used to display a media object and should be disposed when moving to next/previous
var _widthBorderBuffer = 34; // Space to allow for margins, padding, and border on media object. Also defined in controls/mediaobjectview.ascx.cs
var _galleryId;
var _pageLoadHasFired = false; // Set to true in moViewPageLoad. Used by some templates, such as Silverlight.
var _mediaObjectTransitionType;
var _mediaObjectTransitionDuration;

function tbMediaObjectActions_onItemCheckChange(sender, eventArgs)
{
	var toolbarItem = eventArgs.get_item();
	switch (toolbarItem.get_id())
	{
		case "tbiInfo": toggleInfo(toolbarItem); break;
		case "tbiViewHiRes": toggleHiRes(toolbarItem); break;
		case "tbiPermalink": togglePermalink(toolbarItem); break;
	}
}

function tbMediaObjectActions_onItemSelect(sender, eventArgs)
{
	var toolbarItem = eventArgs.get_item();
	switch (toolbarItem.get_id())
	{
		case "tbiSlideshow": toggleSlideshow(toolbarItem); break;
		case "tbiDelete": deleteObject(toolbarItem); break;
	}
}

function toggleInfo(toolbarItem)
{
	if (dgMediaObjectInfo.get_isShowing())
	{
		dgMediaObjectInfo.Close();
	}
	else
	{
		setMediaObjectInfoDialogSize();
		refreshMetadata($get('moid').value);
		dgMediaObjectInfo.Show();

		updateProfile(toolbarItem.get_checked());
	}
}

function startSlideShow()
{
	// Start slide show if it is not already running
	if (_tbIsVisible)
	{
		var toolbarItem = tbMediaObjectActions.get_items().getItemById('tbiSlideshow');
		if (toolbarItem)
		{
			if (toolbarItem.get_imageUrl() == 'play.png')
				toggleSlideshow(toolbarItem);
		}
		else
		{
			// No play/pause toolbar item is present. Start slide show without it.
			startSlideShowTimer();
		}
	}
	else
	{
		// No toolbar is present. Start slide show without it.
		startSlideShowTimer();
	}
}

function toggleSlideshow(toolbarItem)
{
	// Start or stop the slideshow.
	if (toolbarItem.get_imageUrl() == 'play.png')
	{
		toolbarItem.set_imageUrl('pause.png');
		startSlideShowTimer();
	}
	else
	{
		if (_timer)
		{
			_timer.stop();
			_ssInProgress = false;
		}
		toolbarItem.set_imageUrl('play.png');
	}
}

function startSlideShowTimer()
{
	_timer = new Gsp.Timer(showNextMediaObject);
	_timer.set_interval(_ssDelay);
	_ssInProgress = true;
	_timer.start();
}

function updateProfile(showMetadata)
{
	Gsp.Gallery.SetMetaDataVisibility(showMetadata, _galleryId);
}

function setMediaObjectInfoDialogSize()
{
	if (_inPrefetch)
	{
		window.setTimeout("setMediaObjectInfoDialogSize()", 100);
		return;
	}
	var minWidth = 200;
	var defaultWidth = 400;
	var maxWidth = 600;
	var minHeight = 400;
	var maxHeight = document.body.clientHeight ? document.body.clientHeight : 600;
	var xBuffer = 52; // Horizontal buffer space
	var dgWidth = document.body.clientWidth - _moInfo.Width - xBuffer;
	var dgHeight = _moInfo.Height;

	if ($get('hr').value.toLowerCase() == "true")
	{
		dgWidth = defaultWidth; /* Viewing hi-res, so use default */
	}
	else if (dgWidth < minWidth) dgWidth = minWidth;
	else if (dgWidth > maxWidth) dgWidth = maxWidth;

	if (dgHeight < minHeight) dgHeight = minHeight;
	else if (dgHeight > maxHeight) dgHeight = maxHeight;

	dgMediaObjectInfo.set_width(dgWidth);
	dgMediaObjectInfo.set_height(dgHeight);
}

function getDisplayType()
{
	// (0=Unknown,1=Thumbnail,2=Optimized,3=Original)
	if (_viewHiRes)
		return 3;
	else
		return 2;
}

function showMediaObject(id)
{
	if (typeof (dgEditCaption) !== 'undefined')
	{
		if (dgEditCaption.get_isShowing()) dgEditCaption.close();
	}

	_showCurrentMO = true;
	_inPrefetch = true;
	Gsp.Gallery.GetMediaObjectHtml(id, getDisplayType(), getMediaObjectHtmlCompleted, getMediaObjectHtmlFailure);
}

function getMediaObjectHtmlCompleted(results, context, methodName)
{
	_moInfo = results;
	if (results.NextSSUrl && results.NextSSUrl.length > 0)
		_moCacheImg.src = results.NextSSUrl;
	_inPrefetch = false;

	if (_nextMORequested)
	{
		_nextMORequested = false;

		if (_ssInProgress && !_timer.isRunning)
		{
			// We may have stopped the timer in showNextMediaObject() because the timer is ticking faster than our callback
			// can complete. Now that the callback is done, we can restart the timer to resume the slideshow.
			_timer.start();
		}

		showNextMediaObject();
	}

	if (_prevMORequested)
	{
		_prevMORequested = false;
		showPrevMediaObject();
	}

	if (_showCurrentMO)
	{
		_showCurrentMO = false;
		showCurrentMediaObject();
	}
}

function disposeAjaxObject(element)
{
	var o = $find(element);
	if (o) o.dispose();
}

function showCurrentMediaObject()
{
	Array.forEach(_mediaObjectsToDispose, disposeAjaxObject);
	Array.clear(_mediaObjectsToDispose);

	$get(_mo).innerHTML = _moInfo.HtmlOutput;
	if (_moInfo.ScriptOutput.length > 0)
		runScript(_moInfo.ScriptOutput, _moInfo.Id);

	var ctl = $get(_moTitle);
	if (ctl)
	{
		ctl.innerHTML = _moInfo.Title;
	}

	ctl = $get('lblMoPosition');
	if (ctl) ctl.innerHTML = _moInfo.Index + 1;

	ctl = $get('lblMoCount');
	if (ctl) ctl.innerHTML = _moInfo.NumObjectsInAlbum;

	if (_tbIsVisible)
	{
		var pLink = tbMediaObjectActions.get_items().getItemById('tbiPermalink');
		if (pLink) pLink.set_checked(false);
		Sys.UI.DomElement.removeCssClass($get('divPermalink'), "gsp_visible");
		Sys.UI.DomElement.addCssClass($get('divPermalink'), "gsp_invisible");

		var tbiHiRes = tbMediaObjectActions.get_items().getItemById('tbiViewHiRes');
		if (tbiHiRes)
		{
			tbiHiRes.set_visible(_moInfo.HiResAvailable);

			var newWidth = _moInfo.Width + _widthBorderBuffer;
			var moHeaderWidth = parseInt($get('toolbarContainer').style.width);

			$get('divMoView').style.width = (newWidth > moHeaderWidth ? newWidth + "px" : moHeaderWidth + "px");
		}

		var tbiDownload = tbMediaObjectActions.get_items().getItemById('tbiDownload');
		if (tbiDownload)
			tbiDownload.set_visible(_moInfo.IsDownloadable);
	}

	document.body.style.cursor = "default";
}

function showNextMediaObject()
{
	if (_inPrefetch)
	{
		// A Gsp.Gallery.GetMediaObjectHtml() is in progress. Set flag so the callback method can re-execute
		// this method when it is complete.
		document.body.style.cursor = "wait";
		_nextMORequested = true;

		if (_ssInProgress) // Timer ticking faster than our prefetch can complete. Temporarily stop timer. We'll resume in callback complete method.
			_timer.stop();

		return;
	}

	Array.forEach(_mediaObjectsToDispose, disposeAjaxObject);
	Array.clear(_mediaObjectsToDispose);

	if ((_moInfo.NextId == 0) || (_ssInProgress && (_moInfo.NextSSId == 0))) { redirectToHomePage(); return; }

	document.body.style.cursor = "wait";

	if (typeof (dgEditCaption) !== 'undefined')
	{
		if (dgEditCaption.get_isShowing()) dgEditCaption.close();
	}

	$get('moid').value = (_ssInProgress ? _moInfo.NextSSId : _moInfo.NextId);
	$get(_mo).innerHTML = (_ssInProgress ? _moInfo.NextSSHtmlOutput : _moInfo.NextHtmlOutput);

	if ((_ssInProgress) && (_moInfo.NextSSScriptOutput.length > 0))
		runScript(_moInfo.NextSSScriptOutput, _moInfo.NextSSId);
	else if ((!_ssInProgress) && (_moInfo.NextScriptOutput.length > 0))
		runScript(_moInfo.NextScriptOutput, _moInfo.NextId);
	
	var moImg = $('#mo_img');
	if (moImg) {
		moImg.prop("src", _moCacheImg.src);
		animate(moImg);
	}

	var moTitle = $get(_moTitle);
	if (moTitle)
	{
		moTitle.innerHTML = (_ssInProgress ? _moInfo.NextSSTitle : _moInfo.NextTitle);
	}

	ctl = $get('lblMoPosition');
	if (ctl) ctl.innerHTML = (_ssInProgress ? _moInfo.NextSSIndex + 1 : _moInfo.Index + 2);

	ctl = $get('lblMoCount');
	if (ctl) ctl.innerHTML = _moInfo.NumObjectsInAlbum;

	if (_tbIsVisible)
	{
		var pLink = tbMediaObjectActions.get_items().getItemById('tbiPermalink');
		if (pLink) pLink.set_checked(false);
		Sys.UI.DomElement.removeCssClass($get('divPermalink'), "gsp_visible");
		Sys.UI.DomElement.addCssClass($get('divPermalink'), "gsp_invisible");

		var tbiHiRes = tbMediaObjectActions.get_items().getItemById('tbiViewHiRes');
		if (tbiHiRes)
		{
			if (_ssInProgress)
				tbiHiRes.set_visible(_moInfo.NextSSHiResAvailable);
			else
				tbiHiRes.set_visible(_moInfo.NextHiResAvailable);

			var moWidth = (_ssInProgress ? _moInfo.NextSSWidth : _moInfo.NextWidth);
			var newWidth = moWidth + _widthBorderBuffer;
			var moHeaderWidth = parseInt($get('toolbarContainer').style.width);
			
			$get('divMoView').style.width = (newWidth > moHeaderWidth ? newWidth + "px" : moHeaderWidth + "px");
		}

		var tbiDownload = tbMediaObjectActions.get_items().getItemById('tbiDownload');
		if (tbiDownload)
		{
			if (_ssInProgress)
				tbiDownload.set_visible(_moInfo.NextSSIsDownloadable);
			else
				tbiDownload.set_visible(_moInfo.NextIsDownloadable);
		}
	}

	if ((typeof (dgMediaObjectInfo) != 'undefined') && dgMediaObjectInfo.get_isShowing())
	{
		if (_ssInProgress)
			refreshMetadata(_moInfo.NextSSId);
		else
			refreshMetadata(_moInfo.NextId);
	}

	document.body.style.cursor = "default";

	// Pre-fetch data in anticipation of the next request
	_inPrefetch = true;
	var moidToPrefetch = (_ssInProgress ? _moInfo.NextSSId : _moInfo.NextId);
	Gsp.Gallery.GetMediaObjectHtml(moidToPrefetch, getDisplayType(), getMediaObjectHtmlCompleted, getMediaObjectHtmlFailureOnNavigate);
}

function animate(obj) {
	if (!obj)
		return;

	if (_mediaObjectTransitionType == 'Fade') {
		obj.hide().fadeIn(_mediaObjectTransitionDuration);
	}
}

function getMediaObjectHtmlFailureOnNavigate(error, context, methodName)
{
	if (error.get_exceptionType() == "GalleryServerPro.ErrorHandler.CustomExceptions.InvalidMediaObjectException")
	{
		// The next/previous media object may have been deleted. Try again with the current ID.
		Gsp.Gallery.GetMediaObjectHtml(_moInfo.Id, getDisplayType(), getMediaObjectHtmlCompleted, getMediaObjectHtmlFailure);
	}
	else
	{
		_inPrefetch = false;
	}
}

function getMediaObjectHtmlFailure(error, context, methodName)
{
	if (error.get_exceptionType() == "GalleryServerPro.ErrorHandler.CustomExceptions.InvalidMediaObjectException")
		alert(error.get_message());
	else
		alert(error.get_exceptionType() + ": " + error.get_message());

	_inPrefetch = false;
}

function showPrevMediaObject()
{
	if (_inPrefetch)
	{
		// A Gsp.Gallery.GetMediaObjectHtml() is in progress. Set flag so the callback method can re-execute
		// this method when it is complete.
		document.body.style.cursor = "wait";
		_prevMORequested = true;
		return;
	}

	Array.forEach(_mediaObjectsToDispose, disposeAjaxObject);
	Array.clear(_mediaObjectsToDispose);

	if (_moInfo.PrevId == 0) { redirectToHomePage(); return; }

	document.body.style.cursor = "wait";

	if (typeof (dgEditCaption) !== 'undefined')
	{
		if (dgEditCaption.get_isShowing()) dgEditCaption.close();
	}

	$get('moid').value = _moInfo.PrevId;
	$get(_mo).innerHTML = _moInfo.PrevHtmlOutput;
	if (_moInfo.PrevScriptOutput.length > 0)
		runScript(_moInfo.PrevScriptOutput, _moInfo.PrevId);

	animate($('#mo_img'));

	var moTitle = $get(_moTitle);
	if (moTitle)
	{
		moTitle.innerHTML = _moInfo.PrevTitle;
	}

	ctl = $get('lblMoPosition');
	if (ctl) ctl.innerHTML = _moInfo.Index;

	ctl = $get('lblMoCount');
	if (ctl) ctl.innerHTML = _moInfo.NumObjectsInAlbum;

	if (_tbIsVisible)
	{
		var pLink = tbMediaObjectActions.get_items().getItemById('tbiPermalink');
		if (pLink) pLink.set_checked(false);
		Sys.UI.DomElement.removeCssClass($get('divPermalink'), "gsp_visible");
		Sys.UI.DomElement.addCssClass($get('divPermalink'), "gsp_invisible");

		var tbiHiRes = tbMediaObjectActions.get_items().getItemById('tbiViewHiRes');
		if (tbiHiRes)
		{
			tbiHiRes.set_visible(_moInfo.PrevHiResAvailable);

			var newWidth = _moInfo.PrevWidth + _widthBorderBuffer;
			var moHeaderWidth = parseInt($get('toolbarContainer').style.width);

			$get('divMoView').style.width = (newWidth > moHeaderWidth ? newWidth + "px" : moHeaderWidth + "px");
		}

		var tbiDownload = tbMediaObjectActions.get_items().getItemById('tbiDownload');
		if (tbiDownload)
			tbiDownload.set_visible(_moInfo.PrevIsDownloadable);
	}

	if ((typeof (dgMediaObjectInfo) != 'undefined') && dgMediaObjectInfo.get_isShowing())
		refreshMetadata(_moInfo.PrevId);

	document.body.style.cursor = "default";

	// Pre-fetch data in anticipation of the next request
	_inPrefetch = true;
	Gsp.Gallery.GetMediaObjectHtml(_moInfo.PrevId, getDisplayType(), getMediaObjectHtmlCompleted, getMediaObjectHtmlFailureOnNavigate);
}

function dgMediaObjectInfo_OnClose(sender, eventArgs)
{
	if (_tbIsVisible)
	{
		var info = tbMediaObjectActions.get_items().getItemById('tbiInfo');
		if (info) info.set_checked(false);
	}
	updateProfile(false);
}

function refreshMetadata(moid)
{
	Gsp.Gallery.GetMetadataItems(moid, refreshMetadataCompleted);
}

function refreshMetadataCompleted(result)
{
	gdmeta.load(result);
	gdmeta.render();
}

function gdmeta_onLoad(sender, eventArgs)
{
	if (_tbIsVisible)
	{
		var info = tbMediaObjectActions.get_items().getItemById('tbiInfo');
		if ((info) && info.get_checked())
		{
			{
				setMediaObjectInfoDialogSize();
				gdmeta.render();
				dgMediaObjectInfo.Show();
			}
		}
	}
}

function setDialogSize(dialog, width, height)
{
	// Set width & height of CA dialog control.
	var dialogDiv = document.getElementById(dialog.get_id());
	dialog.beginUpdate();
	dialog.Width = width; // Doesn't work in Firefox, so hack below is needed (for height, too)
	dialog.Height = height;
	if (Sys.Browser.agent != Sys.Browser.InternetExplorer)
	{
		// Only needed for Firefox (maybe others, too?)
		dialogDiv.style.width = width + "px";
		dialogDiv.style.height = height + "px";
	}
	dialog.endUpdate();
}

function runScript(script, moid)
{
	// Execute the script. The script may depend on javascript libraries that haven't been loaded by the browser.
	// For example, this will happen if another user adds a Silverlight object to the album after the current
	// user loads this page (since the Silverlight javascript files are only sent if there is a Silverlight
	// object in the album.) So, if an exception is thrown, reload the page, which should cause the necessary
	// files to be sent.
	try
	{
		eval(script);
	}
	catch (e)
	{
		window.location = getPermalink(moid);
	}
}

function getEmbedCode(moid)
{
	return "<iframe allowtransparency='true' frameborder='0' sandbox='allow-same-origin allow-forms allow-scripts' scrolling='auto' src='"
		+ _galleryResourcesUrl + "/embed.aspx?moid=" + moid + "' style='width:100%;height:100%'></iframe>";
}

function getPermalink(moid)
{
	var url = removeQSParm(document.location.href, 'msg');
	url = replaceQSParm(url, 'moid', moid);
	if (_tbIsVisible)
	{
		var tbiHiRes = tbMediaObjectActions.get_items().getItemById('tbiViewHiRes');
		if (tbiHiRes && tbiHiRes.get_checked())
			return replaceQSParm(url, 'hr', 1);
		else
			return removeQSParm(url, 'hr');
	}
}

function replaceQSParm(url, param, value)
{
	var re = new RegExp('([?|&])' + param + '=.*?(&|$)', 'i');
	if (url.match(re))
		return url.replace(re, '$1' + param + '=' + value + '$2');
	else if (url.indexOf('?') == -1)
		return url + '?' + param + '=' + value;
	else
		return url + '&' + param + '=' + value;
}

function removeQSParm(url, param)
{
	// Note: Requires param to be after a '&'
	var re = new RegExp('(&)' + param + '=.*?(&|$)', 'i');
	if (url.match(re))
		return url.replace(re, '$2');
	else
		return url;
}

// Client timer. Used for slideshow.
Type.registerNamespace('Gsp');
Gsp.Timer = function (userCallback)
{
	this._interval = 10000;
	this._raiseTickDelegate = Function.createDelegate(this, this._tick);
	this._userCallback = userCallback;
	this._timer = null;
}

function Gsp$Timer$get_interval() { return this._interval; }

function Gsp$Timer$set_interval(value) { this._interval = value; }

function Gsp$Timer$stop() { this._stopTimer(); }

function Gsp$Timer$start() { this._startTimer(); }

function Gsp$Timer$_tick()
{
	if (this._userCallback !== null)
		this._userCallback();
	this._startTimer();
}

function Gsp$Timer$_startTimer()
{
	this._timer = window.setTimeout(
		this._raiseTickDelegate, this.get_interval());
}

function Gsp$Timer$_stopTimer()
{
	if (this._timer !== null)
	{
		window.clearTimeout(this._timer);
		this._timer = null;
	}
}

function Gsp$Timer$_isRunning() { return (this._timer !== null) }

Gsp.Timer.prototype =
{
	get_interval: Gsp$Timer$get_interval,
	set_interval: Gsp$Timer$set_interval,
	stop: Gsp$Timer$stop,
	start: Gsp$Timer$start,
	_tick: Gsp$Timer$_tick,
	_startTimer: Gsp$Timer$_startTimer,
	_stopTimer: Gsp$Timer$_stopTimer,
	_isRunning: Gsp$Timer$_isRunning
}

Gsp.Timer.registerClass('Gsp.Timer');
Sys.Application.notifyScriptLoaded();