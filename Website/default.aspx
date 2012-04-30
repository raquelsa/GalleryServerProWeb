<%@ Page Language="C#" %>
<%@ Register TagPrefix="gsp" Namespace="GalleryServerPro.Web" Assembly="GalleryServerPro.Web" %>
<!DOCTYPE html>
<html>
<head runat="server">
	<meta charset="utf-8" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" />
	<style type="text/css">
		html, body { margin: 0; padding: 2px; }
		body { background-color: #f5f5f5; }
	</style>
</head>
<body>
	<form id="form1" runat="server">
	<asp:ScriptManager ID="sm" runat="server" EnableScriptGlobalization="true" />
	<gsp:Gallery ID="g" runat="server" />
	</form>
</body>
</html>
