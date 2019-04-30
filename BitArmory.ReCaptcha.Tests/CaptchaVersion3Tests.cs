using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace BitArmory.ReCaptcha.Tests
{
   [TestFixture]
   public class CaptchaVersion3Tests
   {
      public const string ErrorJson = @"{
  ""success"": false,
  ""error-codes"": [
    ""invalid-input-response"",
    ""invalid-input-secret""
  ]
}";

      public const string GoodJson = @"{
  ""success"": true,
  ""challenge_ts"": ""2019-04-30T02:37:21Z"",
  ""hostname"": ""www.bitarmory.com"",
  ""score"": 0.9,
  ""action"": ""purchase""
}";

      [Test]
      public async Task can_parse_errors()
      {
         var mockHttp = new MockHttpMessageHandler();
         
         mockHttp.Expect(HttpMethod.Post, Constants.VerifyUrl)
            .Respond("application/json", ErrorJson)
            .WithExactFormData("response=aaa&remoteip=bbb&secret=ccc");

         var captcha = new ReCaptchaService(client: mockHttp.ToHttpClient());

         var response = await captcha.Verify3Async("aaa", "bbb", "ccc");

         response.IsSuccess.Should().BeFalse();
         response.ErrorCodes.Should().BeEquivalentTo("invalid-input-response", "invalid-input-secret");
      }

      [Test]
      public async Task can_parse_good_response()
      {
         var mockHttp = new MockHttpMessageHandler();

         mockHttp.Expect(HttpMethod.Post, Constants.VerifyUrl)
            .Respond("application/json", GoodJson)
            .WithExactFormData("response=aaa&remoteip=bbb&secret=ccc");

         var captcha = new ReCaptchaService(client: mockHttp.ToHttpClient());

         var response = await captcha.Verify3Async("aaa", "bbb", "ccc");

         response.IsSuccess.Should().BeTrue();
         response.ErrorCodes.Should().BeNull();
         response.ChallengeTs.Should().Be("2019-04-30T02:37:21Z");
         response.HostName.Should().Be("www.bitarmory.com");
         response.Score.Should().Be(0.9f);
         response.Action.Should().Be("purchase");

      }
   }
}