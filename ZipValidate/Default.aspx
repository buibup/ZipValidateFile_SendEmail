<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ZipValidate._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>ASP.NET</h1>
        <p class="lead">ASP.NET is a free web framework for building great Web sites and Web applications using HTML, CSS, and JavaScript.</p>
        <p><a href="http://www.asp.net" class="btn btn-primary btn-lg">Learn more &raquo;</a></p>
    </div>


    <div class="row">
        <asp:Button Text="Generate Tran Code" ID="btnGenTranCode" runat="server" OnClick="btnGenTranCode_Click" />
    </div>

    <div class="row">
        <asp:TextBox ID="txtTranCode" runat="server" Width ="300px" />
    </div>

    <div class="row">
        <asp:FileUpload ID="fileUpload1" runat="server" AllowMultiple="true" />
        <asp:Button ID="btnExtract" runat="server"
            OnClick="btnUpload_Click"
            Text="Upload Files" />

        <asp:Label ID="lblMessage" runat="server" />

        <asp:CheckBoxList ID="checkListFiles" runat="server">
        </asp:CheckBoxList>

    </div>

    <div class="row">
        <asp:Button ID="btnSendEmail" Text="Send Email" runat="server" OnClick="btnSendEmail_Click" />
        <asp:Label Text="" ID="lblSendEmailMsg" runat="server" />
    </div>

</asp:Content>
