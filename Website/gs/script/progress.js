// Enables monitoring and cancelling of long-running server tasks
Type.registerNamespace('Gsp');

Gsp.Progress = function Gsp$Progress() 
{
    Gsp.Progress.initializeBase(this);
    this._galleryId = null;
    this._timerId = null;
    this._taskId = null;
    this._onProgressStartCallback = null;
    this._onProgressCompleteCallback = null;
    this._msInterval = null;
    this._callback = null;
}

// Start the timer to periodically check the status of the ongoing task
function Gsp$Progress$startMonitor(galleryId, taskId, msInterval, onProgressStartCallback, onProgressCompleteCallback, taskCompletedCallback) { 
    if (arguments.length !== 6) throw Error.parameterCount();
    // Update internal members
    _galleryId = galleryId;
    _taskId = taskId;
    _msInterval = msInterval;
    _onProgressStartCallback = onProgressStartCallback;
    _onProgressCompleteCallback = onProgressCompleteCallback;
    _taskCompletedCallback = taskCompletedCallback;
   
    this._startTimer();
}

// Stop the timer  
function Gsp$Progress$stopMonitor() { 
    window.clearTimeout(_timerId);
    if (_taskCompletedCallback !== null)
        _taskCompletedCallback();    
}

// Cancel the operation
function Gsp$Progress$abortTask() { 
    Gsp.Gallery.TerminateTask(_galleryId, _taskId, this._onFeedbackFailed, this._onFeedbackFailed, this);
}

// Start the timer to control progress
function Gsp$Progress$_startTimer() {
    this._callback = Function.createDelegate(this, this._checkProgress);
    _timerId = window.setTimeout(this._callback, _msInterval);
}

// Modify the request to add the task ID to a hidden field (for UpdatePanel pages)
function Gsp$Progress$modifyRequestForTaskId(request, taskId, hiddenField) {
    var body = request.get_body();
    var token = "&" + hiddenField + "=";
    body = body.replace(token, token + taskId);
    request.set_body(body);
    
    return request;
}

// Get the current status of the task
function Gsp$Progress$_checkProgress() {
	if (_onProgressStartCallback != null)
		_onProgressStartCallback();

	Gsp.Gallery.GetCurrentStatus(_galleryId, this._onFeedbackReceived, this._onFeedbackFailed, this);
}

// Notify callback function that we received a status update
function Gsp$Progress$_onFeedbackReceived(results, context) {
    context._startTimer();
    
    if (_onProgressCompleteCallback !== null)
        _onProgressCompleteCallback(results);
}

function Gsp$Progress$_onFeedbackFailed(results) {
    window.clearTimeout(_timerId);
}

Gsp.Progress.prototype = 
{
    startMonitor:           Gsp$Progress$startMonitor,
    stopMonitor:            Gsp$Progress$stopMonitor,
    abortTask:              Gsp$Progress$abortTask,
    modifyRequestForTaskId: Gsp$Progress$modifyRequestForTaskId,
    _startTimer:            Gsp$Progress$_startTimer,
    _checkProgress:         Gsp$Progress$_checkProgress,
    _onFeedbackReceived:    Gsp$Progress$_onFeedbackReceived,
    _onFeedbackFailed:      Gsp$Progress$_onFeedbackFailed    
}

Gsp.Progress.registerClass('Gsp.Progress');
Sys.Application.notifyScriptLoaded();




