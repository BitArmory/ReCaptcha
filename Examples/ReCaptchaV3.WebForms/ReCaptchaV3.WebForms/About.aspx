<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="ReCaptchaV3.WebForms.About" %>
<%@ Import Namespace="ReCaptchaV3.WebForms" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>
    <h3>Your application description page.</h3>
    <p>Use this area to provide additional information.</p>
</asp:Content>


<asp:Content ID="FooterContent" ContentPlaceHolderID="FooterContent" runat="server">
   <script>
      grecaptcha.ready(function() {
         grecaptcha.execute('<%= CaptchaSettings.SiteKey%>', {action: 'AboutPage'});
      });
   </script>
</asp:Content>