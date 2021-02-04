<%@ Page Title="{username}" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="SITConnect.Profile" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadPlaceHolder" runat="server">
    <script type="text/javascript">
        function sessionExpireAlert(timeout) {
            var seconds = timeout / 1000;
            document.getElementById("secondsExpire").innerHTML = seconds;
            setInterval(function () {
                seconds--;
                document.getElementById("secondsExpire").innerHTML = seconds;
            }, 1000);
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
        <h1>Welcome, <asp:Label ID="username" runat="server"></asp:Label></h1>
        <p> Remaining Session Time: <span id="secondsExpire"></span> Seconds.</p>
        <div class="row">
        <asp:Button runat="server" CssClass="btn btn-primary btn-lg" Text="Logout" OnClick="Logout"/>
            <asp:Button runat="server" CssClass="btn btn-primary btn-lg" Text="Update Password" OnClick="update_pwd"/>
            </div>
</asp:Content>
