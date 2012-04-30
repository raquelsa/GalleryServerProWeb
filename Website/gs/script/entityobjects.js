Type.registerNamespace("Gsp");
Gsp.AlbumWebEntity = function Gsp$AlbumWebEntity()
{
	this._id = 0; this._title = ""; this._summary = ""; this._dateStart = ""; this._dateEnd = "";
}

Gsp.AlbumWebEntity.prototype = {
	get_Id: function() { return this._id;	},
	get_Title: function() {	return this._title;	},
	get_Summary: function() {	return this._summary; },
	get_DateStart: function() {	return this._dateStart; },
	get_DateEnd: function() {	return this._dateEnd;	}
}
Gsp.AlbumWebEntity.registerClass('Gsp.AlbumWebEntity');
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();