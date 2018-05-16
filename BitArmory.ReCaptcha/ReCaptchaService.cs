using System;
using System.Threading.Tasks;
using Flurl.Http;

namespace BitArmory.ReCaptcha
{
   /// <summary>
   /// Service for validating reCAPTCHA.
   /// </summary>
   public class ReCaptchaService
   {
      private readonly string verifyUrl;

      /// <summary>
      /// Create a new service instance. This should be a singleton if you're using an IoC container.
      /// </summary>
      public ReCaptchaService() : this(Constants.VerifyUrl)
      {
      }

      /// <summary>
      /// Create a new service instance. This should be a singleton if you're using an IoC container.
      /// </summary>
      /// <param name="verifyUrl">Manually override the Google verify URL endpoint</param>
      public ReCaptchaService(string verifyUrl)
      {
         this.verifyUrl = verifyUrl;
      }

      /// <summary>
      /// Validate reCAPTCHA <paramref name="clientResponse"/> using your secret.
      /// </summary>
      /// <param name="clientResponse">The <seealso cref="Constants.ClientResponseKey"/> value pulled from the client with the request headers or hidden form field.</param>
      /// <param name="remoteIp">The remote IP of the client</param>
      /// <param name="secret">The server-side secret: v2 secret, invisible secret, or android secret.</param>
      /// <returns>Task returning bool whether reCAPTHCA is valid or not.</returns>
      public virtual async Task<bool> VerifyAsync(string clientResponse, string remoteIp, string secret)
      {
         if (string.IsNullOrWhiteSpace(secret)) throw new ArgumentException("The secret must not be null or empty", nameof(secret));
         if (string.IsNullOrWhiteSpace(clientResponse)) throw new ArgumentException("The client response must not be null or empty", nameof(secret));

         var postTask = this.verifyUrl.PostUrlEncodedAsync(new
         {
            response = clientResponse,
            remoteip = remoteIp,
            secret = secret
         });

         var postResponse = await postTask.ConfigureAwait(false);

         if (!postResponse.IsSuccessStatusCode) return false;

         var response = await postTask.ReceiveJson<ReCaptchaResponse>().ConfigureAwait(false);

         return response.IsSuccess;
      }
   }
}
