using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BitArmory.ReCaptcha.Utils;


namespace BitArmory.ReCaptcha
{
   /// <summary>
   /// Service for validating reCAPTCHA.
   /// </summary>
   public class ReCaptchaService : IDisposable
   {
      /// <summary>
      /// The underlying HTTP client used to make the request.
      /// </summary>
      public HttpClient HttpClient { get; set; }

      /// <summary>
      /// The default HTTP client to use for new instances of the <see cref="ReCaptchaService"/>.
      /// </summary>
      public static HttpClient DefaultHttpClient { get; set; } = new HttpClient();

      private readonly string verifyUrl;

      /// <summary>
      /// Create a new service instance. This should be a singleton if you're using an IoC container.
      /// </summary>
      public ReCaptchaService() : this(Constants.VerifyUrl, DefaultHttpClient)
      {
      }

      /// <summary>
      /// Create a new service instance. This should be a singleton if you're using an IoC container.
      /// </summary>
      /// <param name="verifyUrl">Manually override the Google verify URL endpoint. If null, <seealso cref="Constants.VerifyUrl" /> is used.</param>
      /// <param name="client">The HttpClient to use for this instance. If null, the global static <seealso cref="ReCaptchaService.DefaultHttpClient"/> is used.</param>
      public ReCaptchaService(string verifyUrl = null, HttpClient client = null)
      {
         this.verifyUrl = verifyUrl ?? Constants.VerifyUrl;
         this.HttpClient = client ?? DefaultHttpClient;
      }

      /// <summary>
      /// Validate reCAPTCHA v2 <paramref name="clientToken"/> and return the json response (for internal use).
      /// </summary>
      /// <param name="clientToken">Required. The user response token provided by the reCAPTCHA client-side integration on your site. The <seealso cref="Constants.ClientResponseKey"/> value pulled from the client with the request headers or hidden form field.</param>
      /// <param name="remoteIp">Optional. The remote IP of the client.</param>
      /// <param name="siteSecret">Required. The server-side secret: v2 secret, invisible secret, or android secret. The shared key between your site and reCAPTCHA.</param>
      /// <param name="cancellationToken">Async cancellation token.</param>
      /// <returns>Task returning the parsed JSON response, or null if the request wasn't successful.</returns>
      async Task<JsonNode?> JsonResponse2Async(string clientToken, string remoteIp, string siteSecret, CancellationToken cancellationToken = default)
      {
         if (string.IsNullOrWhiteSpace(siteSecret)) throw new ArgumentException("The secret must not be null or empty", nameof(siteSecret));
         if (string.IsNullOrWhiteSpace(clientToken)) throw new ArgumentException("The client response must not be null or empty", nameof(clientToken));

         var form = PrepareRequestBody(clientToken, siteSecret, remoteIp);

         var response = await this.HttpClient.PostAsync(verifyUrl, form, cancellationToken)
            .ConfigureAwait(false);

         if( !response.IsSuccessStatusCode ) return null;

         var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

         return Json.Parse(json);
      }

      /// <summary>
      /// Validate reCAPTCHA v2 <paramref name="clientToken"/> using your secret.
      /// </summary>
      /// <param name="clientToken">Required. The user response token provided by the reCAPTCHA client-side integration on your site. The <seealso cref="Constants.ClientResponseKey"/> value pulled from the client with the request headers or hidden form field.</param>
      /// <param name="remoteIp">Optional. The remote IP of the client.</param>
      /// <param name="siteSecret">Required. The server-side secret: v2 secret, invisible secret, or android secret. The shared key between your site and reCAPTCHA.</param>
      /// <param name="cancellationToken">Async cancellation token.</param>
      /// <param name="hostname">Optional. The expected hostname. If not null, the verification will fail if this does not match.</param>
      /// <param name="action">Optional. The expected action (for Cloudflare Turnstile, this should be null if reCAPTCHA v2 is being used). If not null, the verification will fail if this does not match.</param>
      /// <returns>Task returning bool whether reCAPTHCA is valid or not.</returns>
      public virtual async Task<bool> Verify2Async(string clientToken, string remoteIp, string siteSecret, CancellationToken cancellationToken = default, string hostname = null, string action = null)
      {
         var model = await JsonResponse2Async(clientToken, remoteIp, siteSecret, cancellationToken).ConfigureAwait(false);

         // Check the request was successful
         if( model == null ) return false;

         // Verify the hostname if it's not null
         if (hostname != null && hostname != model["hostname"]) return false;

         // Verify the action if it's not null
         if (action != null && action != model["action"]) return false;

         // Now return the success value
         return model["success"].AsBool;
      }

      /// <summary>
      /// Validate reCAPTCHA v2 <paramref name="clientToken"/> using your secret and return the full response.
      /// </summary>
      /// <param name="clientToken">Required. The user response token provided by the reCAPTCHA client-side integration on your site. The <seealso cref="Constants.ClientResponseKey"/> value pulled from the client with the request headers or hidden form field.</param>
      /// <param name="remoteIp">Optional. The remote IP of the client</param>
      /// <param name="siteSecret">Required. The server-side secret: v2 secret, invisible secret, or android secret. The shared key between your site and reCAPTCHA.</param>
      /// <param name="cancellationToken">Async cancellation token.</param>
      /// <returns>Task returning the full response.</returns>
      public virtual async Task<ReCaptcha2Response> Response2Async(string clientToken, string remoteIp, string siteSecret, CancellationToken cancellationToken = default)
      {
         var model = await JsonResponse2Async(clientToken, remoteIp, siteSecret, cancellationToken).ConfigureAwait(false);

         if( model == null ) return new ReCaptcha2Response {IsSuccess = false};

         var result = new ReCaptcha2Response();

         foreach( var kv in model )
         {
            switch( kv.Key )
            {
               case "success":
                  result.IsSuccess = kv.Value;
                  break;
               case "action":
                  result.Action = kv.Value;
                  break;
               case "challenge_ts":
                  result.ChallengeTs = kv.Value;
                  break;
               case "hostname":
                  result.HostName = kv.Value;
                  break;
               case "apk_package_name":
                  result.ApkPackageName = kv.Value;
                  break;
               case "cdata":
                  result.CData = kv.Value;
                  break;
               case "error-codes" when kv.Value is JsonArray errors:
               {
                  result.ErrorCodes = errors.Children
                     .Select(n => (string)n)
                     .ToArray();

                  break;
               }
               default:
                  result.ExtraJson.Add(kv.Key, kv.Value);
                  break;
            }
         }

         return result;
      }

      /// <summary>
      /// Validate reCAPTCHA v3 <paramref name="clientToken"/> using your secret.
      /// </summary>
      /// <param name="clientToken">Required. The user response token provided by the reCAPTCHA client-side integration on your site. The <seealso cref="Constants.ClientResponseKey"/> value pulled from the client with the request headers or hidden form field.</param>
      /// <param name="remoteIp">Optional. The remote IP of the client</param>
      /// <param name="siteSecret">Required. The server-side secret: v2 secret, invisible secret, or android secret. The shared key between your site and reCAPTCHA.</param>
      /// <param name="cancellationToken">Async cancellation token.</param>
      public virtual async Task<ReCaptcha3Response> Verify3Async(string clientToken, string remoteIp, string siteSecret, CancellationToken cancellationToken = default)
      {
         if( string.IsNullOrWhiteSpace(siteSecret) ) throw new ArgumentException("The secret must not be null or empty", nameof(siteSecret));
         if( string.IsNullOrWhiteSpace(clientToken) ) throw new ArgumentException("The client response must not be null or empty", nameof(clientToken));

         var form = PrepareRequestBody(clientToken, siteSecret, remoteIp);

         var response = await this.HttpClient.PostAsync(verifyUrl, form, cancellationToken)
            .ConfigureAwait(false);

         if( !response.IsSuccessStatusCode ) return new ReCaptcha3Response {IsSuccess = false};

         var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

         var model = Json.Parse(json);

         var result = new ReCaptcha3Response();

         foreach( var kv in model )
         {
            switch( kv.Key )
            {
               case "success":
                  result.IsSuccess = kv.Value;
                  break;
               case "score":
                  result.Score = kv.Value;
                  break;
               case "action":
                  result.Action = kv.Value;
                  break;
               case "challenge_ts":
                  result.ChallengeTs = kv.Value;
                  break;
               case "hostname":
                  result.HostName = kv.Value;
                  break;
               case "apk_package_name":
                  result.ApkPackageName = kv.Value;
                  break;
               case "error-codes" when kv.Value is JsonArray errors:
               {
                  result.ErrorCodes = errors.Children
                     .Select(n => (string)n)
                     .ToArray();

                  break;
               }
               default:
                  result.ExtraJson.Add(kv.Key, kv.Value);
                  break;
            }
         }

         return result;
      }

      /// <summary>
      /// Method called when the request body needs to be prepared.
      /// </summary>
      /// <param name="clientResponse">The client response</param>
      /// <param name="secret"></param>
      /// <param name="remoteIp"></param>
      /// <returns></returns>
      protected FormUrlEncodedContent PrepareRequestBody(string clientResponse, string secret, string remoteIp)
      {
         var form = new List<KeyValuePair<string, string>>()
            {
               new KeyValuePair<string, string>("response", clientResponse),
               new KeyValuePair<string, string>("secret", secret),
            };

         if (!string.IsNullOrWhiteSpace(remoteIp))
         {
            form.Add(new KeyValuePair<string, string>("remoteip", remoteIp));
         }

         return new FormUrlEncodedContent(form);
      }

      /// <summary>
      /// Disposes the local <seealso cref="HttpClient"/>.
      /// Note: If the global static <seealso cref="ReCaptchaService.DefaultHttpClient"/> was used in this instance, this .Dispose() method will not dispose the global static HttpClient.
      /// </summary>
      public void Dispose()
      {
         if( this.HttpClient != DefaultHttpClient )
         {
            this.HttpClient.Dispose();
         }
      }

#if !STANDARD13
      /// <summary>
      /// Enable HTTP debugging via Fiddler. Ensure Tools > Fiddler Options... > Connections is enabled and has a port configured.
      /// Then, call this method with the following URL format: http://localhost.:PORT where PORT is the port number Fiddler proxy
      /// is listening on. (Be sure to include the period after the localhost).
      /// Note, calling this method will replace the object in <seealso cref="HttpClient"/> property.
      /// with a correctly configured HttpClient for proxy usage.
      /// </summary>
      /// <param name="proxyUrl">The full proxy URL Fiddler proxy is listening on. IE: http://localhost.:8888 - The period after localhost is important to include.</param>
      public void EnableFiddlerDebugProxy(string proxyUrl)
      {
         var webProxy = new WebProxy(proxyUrl, BypassOnLocal: false);
         var handler = new HttpClientHandler {Proxy = webProxy};
         this.HttpClient = new HttpClient(handler, true);
      }
#endif
   }
}
