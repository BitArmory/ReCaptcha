<%@ Page Title="Home Page"
   Language="C#"
   MasterPageFile="~/Site.Master"
   AutoEventWireup="true"
   CodeBehind="Default.aspx.cs"
   Inherits="ReCaptchaV3.WebForms._Default"
   Async="true" %>
<%@ Import Namespace="ReCaptchaV3.WebForms" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
   <h3>BitArmory.ReCaptcha example for WebForms and .NET Full Framework 4.5+</h3>
   <p>
      This example demonstrates how to use <a href="https://www.google.com/recaptcha/intro/v3.html">reCAPTCHA</a> v3 for WebForms.
   </p>

   <h4>Implementation Notes:</h4>
   <ul>
      <li>To get this demo to work, replace the <b>CaptchaSettings</b> static class <b>SiteKey</b> and <b>SecretKey</b> values inside <b>Default.aspx.cs</b> with your own values you obtain in <a href="https://www.google.com/recaptcha/intro/v3.html">your admin console</a>. Be sure to add `localhost` to the list of domains that are allowed to use your keys.</li>
      <li>Ensure the page has <b>Async='true'</b> page directive. See <b>Default.aspx</b>.</li>
      <li>Take note of the <b>script</b> tag in <b>Site.Master</b> in the head tag of <b>Site.Master</b> page that pulls in reCAPTCHA using your site's key.</li>
      <li>Each page on your website should execute <b>grecaptcha.execute</b> even if they do nothing or do not process any HTTP backs or button clicks. See <a href="About.aspx">About.aspx</a> and <a href="Contact.aspx">Contact.aspx</a> page for examples.</li>
      <li>When you're ready to validate some action, you'll need to transfer a "client token" from the user's browser to the server to validate that action. This is done using a hidden field named `clientToken`. See <b>Default.aspx</b> for how this is done.</li>
      <li><b>grecaptcha.execute</b> actions may only contain alphanumeric characters and slashes, and must not be user-specific.</li>
   </ul>

   <h2>
      <asp:Label runat="server" Visible="false" ID="status"></asp:Label>
   </h2>
   <p>
      Please enter a message blow and click the "Submit and Validate Captcha" to perform validation on the server.
   </p>
   <p>
      <asp:TextBox runat="server" ID="message"></asp:TextBox>
      <asp:HiddenField runat="server" ID="clientToken" />
   </p>
   <p>
      <asp:Button runat="server" Text="Submit and Validate Captcha" OnClientClick="return SubmitButtonClicked()" ID="cmdUISubmit" />
      
      <asp:Button runat="server" CssClass="hide" ID="cmdValidateCaptcha" OnClick="cmdValidateCaptcha_OnClick"
                  Text="This button is hidden and will actually 'clicked' after reCAPTCHA sets the hidden field." />
   </p>


</asp:Content>

<asp:Content ID="FooterContent" ContentPlaceHolderID="FooterContent" runat="server">
   <script>
      function ExecuteReCaptcha() {
         grecaptcha.ready(function () {
            grecaptcha.execute('<%= CaptchaSettings.SiteKey %>', { action: '<%= PageAction %>' })
               .then(function (token) {
                  // Set `token` in a hidden form input.
                  $('#<%= this.clientToken.ClientID %>').val(token);

                  //Next, actually submit the form. This can be done two ways:
                  //1. Use a hidden "button" and simulate a click event.
                  //2. Call ASP.NET's JavaScript __doPostBack() to start the callback.
                  //Here we use the first approach.
                  $('#<%= this.cmdValidateCaptcha.ClientID%>').click();
               });
         });
      }

      //We execute grecaptcha when the user clicks the submit button because 
      //there may be a situation where the user visits the page but doesn't post anything
      //for over 5 minutes and the clientToken that would have been set on page load would
      //be expired.
      //
      //So, we run `grecaptcha.execute` at the moment when the user clicks 
      //the "Submit and Validate" button so we have a fresh client token to work with
      //when the server validates the request with the reCAPTCHA service at Google.
      function SubmitButtonClicked() {

         //Disable the UI button to prevent multiple clicks.
         $('#<%= this.cmdUISubmit.ClientID%>').attr('disabled', true);

         ExecuteReCaptcha();

         //We return false here to prevent the browser from actually submitting
         //the click event to the server because we have to asynchronously wait 
         //for the reCAPTCHA to resolve the JavaScript promise `grecaptcha.execute`
         //then and only then, when the promise is resolved, that we actually have
         //a clientToken to submit filled in the hidden field.
         return false;
      }
   </script>
</asp:Content>
