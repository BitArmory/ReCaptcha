using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using BitArmory.ReCaptcha;

namespace ReCaptchaV3.WebForms
{
   public static class CaptchaSettings{

      public const string SiteKey = "__YOU_MUST_CHANGE_THIS_VALUE__";
      public const string SecretKey = "__YOU_MUST_CHANGE_THIS_VALUE__";
   }

   public partial class _Default : Page
   {
      public const string PageAction = "DefaultHomePage";

      protected void cmdValidateCaptcha_OnClick(object sender, EventArgs e)
      {
         this.RegisterAsyncTask(new PageAsyncTask(VerifyCaptcha));
      }

      private async Task VerifyCaptcha(CancellationToken cancellationToken)
      {
         var captchaService = new ReCaptchaService();

         var clientToken = this.GetClientToken();
         var ipAddress = this.GetClientIPAddress();

         var result = await captchaService.Verify3Async(clientToken, ipAddress, CaptchaSettings.SecretKey, cancellationToken);

         if (result == null)
         {
            SetStatus("ERROR: Verification result is null.", Color.Red);
            return;
         }

         if (!result.IsSuccess ||
             result.Action != PageAction ||
             result.Score < 0.5)
         {
            SetStatus("ERROR: Something went wrong. I think you're a bot!", Color.Red);
            return;
         }

         var now = DateTime.Now;

         SetStatus( "Cool. Validation passed. Time is: "+now+". Your message is: " + this.message.Text, Color.Lime);
      }

      private void SetStatus(string message, Color color)
      {
         this.status.Visible = true;
         this.status.Text = message;
         this.status.ForeColor = color;
      }

      private string GetClientToken()
      {
         //If your site is behind CloudFlare, be sure you're suing the CF-Connecting-IP header value instead:
         //https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-Cloudflare-handle-HTTP-Request-headers

         return this.clientToken.Value;
      }

      private string GetClientIPAddress()
      {
         return this.Request.UserHostAddress;
      }

   }
}