using System.Collections.Generic;

namespace BitArmory.ReCaptcha
{
   /// <summary>
   /// Base class for all JSON responses.
   /// </summary>
   public class JsonResponse
   {
      /// <summary>
      /// Extra data for/from the JSON serializer/deserializer to included with the object model.
      /// </summary>
      public IDictionary<string, object> ExtraJson { get; } = new Dictionary<string, object>();
   }
   
   /// <summary>
   /// Response from reCAPTCHA verify URL.
   /// </summary>
   public class ReCaptcha3Response : JsonResponse
   {
      /// <summary>
      /// Whether this request was a valid reCAPTCHA token for your site.
      /// </summary>
      public bool IsSuccess { get; set; }

      /// <summary>
      /// The score for this request. Value ranges between 0.0 and 1.0; where 0.0 is very likely a bot and 1.0 is very likely a good human interaction.
      /// </summary>
      public float Score { get; set; }

      /// <summary>
      /// The action name for this request (important to verify)
      /// </summary>
      public string Action { get; set; }

      /// <summary>
      /// Timestamp of the challenge load (ISO format yyyy-MM-dd'T'HH:mm:ssZZ)
      /// </summary>
      public string ChallengeTs { get; set; }

      /// <summary>
      /// missing-input-secret: The secret parameter is missing.
      /// invalid-input-secret: The secret parameter is invalid or malformed.
      /// missing-input-response: The response parameter is missing.
      /// invalid-input-response: The response parameter is invalid or malformed.
      /// bad-request: The request is invalid or malformed.
      /// timeout-or-duplicate: The response is no longer valid: either is too old or has been used previously.
      /// </summary>
      public string[] ErrorCodes { get; set; }

      /// <summary>
      /// The hostname of the site where the reCAPTCHA was solved
      /// </summary>
      public string HostName { get; set; }
   }

}