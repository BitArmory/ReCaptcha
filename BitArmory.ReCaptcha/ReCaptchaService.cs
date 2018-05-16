using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("BitArmory.ReCaptcha.Tests")]

namespace BitArmory.ReCaptcha
{
   /// <summary>
   /// Default constants.
   /// </summary>
   public static class ReCaptchaConstants
   {
      /// <summary>
      /// Default URL for verifying reCAPTCHA.
      /// </summary>
      public const string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

      /// <summary>
      /// Default URL for reCAPTCHA.js
      /// </summary>
      public const string JavaScriptUrl = "https://www.google.com/recaptcha/api.js";

      /// <summary>
      /// Default HTTP header key for reCAPTCHA response.
      /// </summary>
      public const string ClientResponseKey = "g-recaptcha-response";
   }


   /// <summary>
   /// Response from reCAPTCHA verify URL.
   /// </summary>
   public class ReCaptchaResponse
   {
      [JsonProperty("success")]
      public bool IsSuccess { get; set; }

      [JsonProperty("challenge_ts")]
      public string ChallengeTs { get; set; }

      [JsonProperty("error-codes")]
      public string[] ErrorCodes { get; set; }

      [JsonProperty("hostname")]
      public string HostName { get; set; }

      [JsonProperty("apk_package_name")]
      public string ApkPackageName { get; set; }
   }

   /// <summary>
   /// Service for validating reCAPTCHA.
   /// </summary>
   public class ReCaptchaService
   {
      private readonly string verifyUrl;

      public ReCaptchaService() : this(ReCaptchaConstants.VerifyUrl)
      {
      }

      public ReCaptchaService(string verifyUrl)
      {
         this.verifyUrl = verifyUrl;
      }

      /// <summary>
      /// Validate reCAPTCHA <paramref name="clientResponse"/> using your secret.
      /// </summary>
      /// <param name="clientResponse">The <seealso cref="ReCaptchaConstants.ClientResponseKey"/> value pulled from the client with the request headers or hidden form field.</param>
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
