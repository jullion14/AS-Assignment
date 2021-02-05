<%@ Page Title="Passowrd Change | SITConnect" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="SITConnect.ChangePassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadPlaceHolder" runat="server">
    <script src="https://www.google.com/recaptcha/api.js?render=6LcB7j8aAAAAAIpnGFum4SXTMqsTcRB0J3MtfHdh">


    </script>
    <script type="text/javascript">
        function validate() {
            var str = document.getElementById('<%=newpass.ClientID%>').value;
            if (str.length < 8) {
                document.getElementById("pwdFeedback").innerHTML = "Password Length Must be at least 8 characters.";
                document.getElementById("pwdFeedback").style.color = "Red";
                return ("too_short");
            }
            else if (str.search(/[0-9]/) == -1) {
                document.getElementById("pwdFeedback").innerHTML = "Password require at least 1 number";
                document.getElementById("pwdFeedback").style.color = "Red";
                return ("no_number");
            }
            else if (str.search(/[A-Z]/) == -1) {
                document.getElementById("pwdFeedback").innerHTML = "Password require at least 1 uppercase character";
                document.getElementById("pwdFeedback").style.color = "Red";
                return ("no_uppercase");
            }
            else if (str.search(/[a-z]/) == -1) {
                document.getElementById("pwdFeedback").innerHTML = "Password require at least 1 lowercase character";
                document.getElementById("pwdFeedback").style.color = "Red";
                return ("no_lowercase");
            }

            else if (str.search(/[!@#$%^&*]/) == -1) {
                document.getElementById("pwdFeedback").innerHTML = "Password require at least 1 symbol";
                document.getElementById("pwdFeedback").style.color = "Red";
                return ("no_symbol");
            }
            else if (str.length == 0) {
                document.getElementById("pwdFeedback").innerHTML = "";
                return ("");
            }
            document.getElementById("pwdFeedback").innerHTML = "Excellent!";
            document.getElementById("pwdFeedback").style.color = "Blue";
        }
        
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="errorOrSuccess" runat="server" BorderColor="Black" style="font-size: 20px;"></asp:Label>
    <div class="jumbotron">
        <h1> Change Password </h1>
        <p> Enter in the email address you used to register with us. </p>
        <div class="row">
            <div class="form-group col-lg-4">
                    <asp:Label ID="eAddr" runat="server" Text="Email Address:"></asp:Label>
                    <asp:TextBox ID="emailAddr" runat="server" CssClass="form-control" TextMode="Email" placeholder="iamgreat123@hotmail.com"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="emailAddr"
                ForeColor="Red" ValidationExpression="^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"
                Display = "Dynamic" ErrorMessage = "Invalid email address"/>
                    <asp:RequiredFieldValidator ID="EmailAddressValidator" runat="server" ControlToValidate="emailAddr"
                    ForeColor="Red" Display = "Dynamic" ErrorMessage = "Required" /><br />
                 
             </div>
            <div class="form-group col-lg-4">
                <asp:Label ID="oldpass" runat="server" Text="Old Password:"></asp:Label>
                <asp:TextBox ID="tb_oldpass" runat="server" CssClass="form-control" TextMode="Password" AutoCompleteType="Disabled"></asp:TextBox><br /> 
                <asp:RequiredFieldValidator ID="oldPasswordValidator" runat="server" ControlToValidate="tb_oldpass"
                    ForeColor="Red" Display = "Dynamic" ErrorMessage = "Required" />
            </div>
            <div class="form-group col-lg-4">
                <asp:Label ID="newpass" runat="server" Text="New Password:"></asp:Label>
                <asp:TextBox ID="tb_newpass" runat="server" CssClass="form-control" TextMode="Password" AutoCompleteType="Disabled" onkeyup="javascript:validate()"></asp:TextBox><br />
                <p id="pwdFeedback" style="font-size: 14px;"></p>
                <asp:RequiredFieldValidator ID="PasswordValidator" runat="server" ControlToValidate="tb_newpass"
                    ForeColor="Red" Display = "Dynamic" ErrorMessage = "Required" />
            </div>
            <asp:Button runat="server" CssClass="btn btn-primary btn-lg" Text="Change Password" OnClick="change_pwd_Click"/>
        </div>       
        <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
    </div>
</asp:Content>