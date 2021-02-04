<%@ Page Title="Login | SITConnect" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SITConnect.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadPlaceHolder" runat="server">
    <script src="https://www.google.com/recaptcha/api.js?render=6LcB7j8aAAAAAIpnGFum4SXTMqsTcRB0J3MtfHdh">

    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="errorOrSuccess" runat="server" BorderColor="Black" style="font-size: 20px;">
    </asp:Label>
    <div class="jumbotron">
        <h1> Login </h1>
        <p> Enter your SITConnect Account Details!</p>
        <br />
        <div class="row">
            <div class="form-group col-lg-6">
                    <asp:Label ID="eAddr" runat="server" Text="Email Address:"></asp:Label>
                    <asp:TextBox ID="emailAddr" runat="server" CssClass="form-control" TextMode="Email" placeholder="iamgreat123@hotmail.com"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="emailAddr"
                ForeColor="Red" ValidationExpression="^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"
                Display = "Dynamic" ErrorMessage = "Invalid email address"/>
                    <asp:RequiredFieldValidator ID="EmailAddressValidator" runat="server" ControlToValidate="emailAddr"
                    ForeColor="Red" Display = "Dynamic" ErrorMessage = "Required" />
             </div>
            <div class="form-group col-lg-6">
                    <asp:Label ID="pwd" runat="server" Text="Password:"></asp:Label>
                    <asp:TextBox ID="pwdInput" runat="server" CssClass="form-control" TextMode="Password" placeholder="Password"></asp:TextBox>
                    
                    <asp:RequiredFieldValidator ID="PasswordValidator" runat="server" ControlToValidate="pwdInput"
                    ForeColor="Red" Display = "Dynamic" ErrorMessage = "Required" />
             </div>
         </div>
         <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
        
        <asp:Button runat="server" CssClass="btn btn-primary btn-lg" Text="Login" OnClick="Login_Click"/>
    </div>
</asp:Content>
