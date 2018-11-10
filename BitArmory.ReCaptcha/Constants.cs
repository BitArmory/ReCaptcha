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
      /// Default URL for reCAPTCHA.js
      /// </summary>
      public const string JavaScriptUrl = "https://www.google.com/recaptcha/api.js";

      /// <summary>  
      /// Default HTTP header key for reCAPTCHA response.
      /// </summary>
      public const string ClientResponseKey = "g-recaptcha-response";
   }
}