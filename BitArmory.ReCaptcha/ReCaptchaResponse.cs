using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitArmory.ReCaptcha
{
   public class Json
   {
      /// <summary>
      /// Extra data for/from the JSON serializer/deserializer to included with the object model.
      /// </summary>
      [JsonExtensionData]
      public IDictionary<string, JToken> ExtraJson { get; } = new Dictionary<string, JToken>();
   }

   /// <summary>
   /// Response from reCAPTCHA verify URL.
   /// </summary>
   public class ReCaptcha2Response : Json
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
   /// Response from reCAPTCHA verify URL.
   /// </summary>
   public class ReCaptcha3Response : Json
   {
      [JsonProperty("success")]
      public bool IsSuccess { get; set; }

      [JsonProperty("score")]
      public float Score { get; set; }

      [JsonProperty("action")]
      public string Action { get; set; }

      [JsonProperty("challenge_ts")]
      public string ChallengeTs { get; set; }

      [JsonProperty("error-codes")]
      public string[] ErrorCodes { get; set; }

      [JsonProperty("hostname")]
      public string HostName { get; set; }
   }

}