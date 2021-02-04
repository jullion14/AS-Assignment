<%@ Page Title="Register | SITConnect" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="SITConnect._Default" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadPlaceHolder" runat="server">
    <link rel="stylesheet" href="Content/Register.css" />
    <script src="https://www.google.com/recaptcha/api.js?render=6LcB7j8aAAAAAIpnGFum4SXTMqsTcRB0J3MtfHdh">

    </script>
    <script type="text/javascript">
        function validate() {
            var str = document.getElementById('<%=pwdInput.ClientID%>').value;
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

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="errorOrSuccess" runat="server" BorderColor="Black" style="font-size: 20px;"></asp:Label>
    <div class="jumbotron">
        <h1> Registration</h1>
        <p> Enter your details below to create a SITConnect Account!</p>
        <br />
        <div class="row">
            <div class="form-group col-lg-4">
                <asp:Label ID="fNameLbl" runat="server" Text="First Name:"></asp:Label>
                <asp:TextBox ID="firstName" runat="server" CssClass="form-control" placeholder="Bob"></asp:TextBox>

                <asp:RequiredFieldValidator ID="fNameValidator" runat="server" ControlToValidate="firstName"
                    ForeColor="Red" Display = "Dynamic" ErrorMessage = "Required" />
            </div>
            <div class="form-group col-lg-4">
                <asp:Label ID="lNameLbl" runat="server" Text="Last Name:"></asp:Label>
                <asp:TextBox ID="lastName" runat="server" CssClass="form-control" placeholder="Toh"></asp:TextBox>
                
                <asp:RequiredFieldValidator ID="lNameValidator" runat="server" ControlToValidate="lastName"
                    ForeColor="Red" Display = "Dynamic" ErrorMessage = "Required" />
            </div>
            <div class="form-group col-lg-4">
                <asp:Label ID="ccInfo" runat="server" Text="Credit Card Number:"></asp:Label>
                <asp:TextBox ID="ccardInfo" runat="server" CssClass="form-control" MaxLength="16" TextMode="SingleLine" placeholder="9999 9999 0000 0000"></asp:TextBox>
                
                <asp:RequiredFieldValidator ID="cCardValidator" runat="server" ControlToValidate="ccardInfo"
                    ForeColor="Red" Display = "Dynamic" ErrorMessage = "Required" />
            </div>
        </div>
        <hr />
        <div class="row">
            <div class="form-group col-lg-4">
                <asp:Label ID="eAddr" runat="server" Text="Email Address:"></asp:Label>
                <asp:TextBox ID="emailAddr" runat="server" CssClass="form-control" TextMode="Email" placeholder="iamgreat123@hotmail.com"></asp:TextBox>
                
                <asp:RegularExpressionValidator ID="EmailValidator" runat="server" ControlToValidate="emailAddr"
                ForeColor="Red" ValidationExpression="^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"
                Display = "Dynamic" ErrorMessage = "Invalid email address"/>
                <asp:RequiredFieldValidator ID="DateFieldValidator" runat="server" ControlToValidate="emailAddr"
                    ForeColor="Red" Display = "Dynamic" ErrorMessage = "Required" />            
            </div>
            
            
            <div class="form-group col-lg-4">
                <asp:Label ID="pwd" runat="server" Text="Password:"></asp:Label>
                <asp:TextBox ID="pwdInput" runat="server" CssClass="form-control" TextMode="Password" placeholder="Password" onkeyup="javascript:validate()"></asp:TextBox>
                
                <p id="pwdFeedback" style="font-size: 14px;"></p>
                <asp:RequiredFieldValidator ID="PasswordValidator" runat="server" ControlToValidate="pwdInput"
                    ForeColor="Red" Display = "Dynamic" ErrorMessage = "Required" />
            </div>
            
            <div class="form-group col-lg-4">
                <asp:Label ID="dob" runat="server" Text="Date of Birth:"></asp:Label>
                <asp:TextBox ID="tb_date" runat="server" CssClass="form-control" TextMode="Date" placeholder="12/02/2001"></asp:TextBox>
            </div>
        </div>        
        
        <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>
        <asp:Button runat="server" CssClass="btn btn-primary btn-lg" Text="Register" OnClick="Submit_register_Click"/>
    </div>
   
</asp:Content>
