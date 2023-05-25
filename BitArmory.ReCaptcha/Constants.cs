namespace BitArmory.ReCaptcha
{
   /// <summary>
   /// Default constants.
   /// </summary>
   public static class Constants
   {
      /// <summary>
      /// Default URL for verifying reCAPTCHA.
      /// </summary>
      public const string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

      /// <summary>
      /// Default URL for verifying reCAPTCHA.
      /// </summary>
      public const string TurnstileVerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

      /// <summary>
      /// Default JavaScript URL for reCAPTCHA.js
      /// </summary>
      public const string JavaScriptUrl = "https://www.google.com/recaptcha/api.js";

      /// <summary>
      /// More available JavaScript URL for reCAPTCHA.js (available where google.com isn't)
      /// </summary>
      public const string NonGoogleJavaScriptUrl = "https://www.recaptcha.net/recaptcha/api.js";

      /// <summary>
      /// Default JavaScript URL for Cloudflare Turnstile
      /// </summary>
      public const string TurnstileJavaScriptUrl = "https://challenges.cloudflare.com/turnstile/v0/api.js";

      /// <summary>
      /// Default HTTP header key for reCAPTCHA response.
      /// </summary>
      public const string ClientResponseKey = "g-recaptcha-response";

      /// <summary>
      /// Default HTTP header key for reCAPTCHA response.
      /// </summary>
      public const string TurnstileClientResponseKey = "cf-turnstile-response";
   }
}
