<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        Select File:
        <asp:FileUpload ID="FileUpload1" runat="server" />
        <asp:Button Text="Upload" runat="server" OnClick="Upload" />
        <hr />
        <asp:Label ID="lblText" runat="server" />
    </div>
    </form>
</body>
</html>
