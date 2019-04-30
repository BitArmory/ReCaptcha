using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace BitArmory.ReCaptcha.Tests
{
   [TestFixture]
   public class CaptchaVersion2Tests
   {
      string ResponseJson(bool isSuccess)
      {
         var json =
            @"{
  ""success"": {SUCCESS},
  ""challenge_ts"": ""2018-05-15T23:05:22Z"",
  ""hostname"": ""localhost""
}".Replace("{SUCCESS}", isSuccess.ToString().ToLower());

         return json;
      }

      [Test]
      public async Task can_verify_a_captcha()
      {
         var responseJson = ResponseJson(true);

         var mockHttp = new MockHttpMessageHandler();

         mockHttp.Expect(HttpMethod.Post, Constants.VerifyUrl)
            .Respond("application/json", responseJson)
            .WithExactFormData("response=aaaaa&remoteip=bbbb&secret=cccc");

         var captcha = new ReCaptchaService(client: mockHttp.ToHttpClient());

         var response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc");

         response.Should().BeTrue();

         mockHttp.VerifyNoOutstandingExpectation();
      }

      [Test]
      public async Task can_verify_failed_response()
      {
         var responseJson = ResponseJson(false);

         var mockHttp = new MockHttpMessageHandler();

         mockHttp.Expect(HttpMethod.Post, Constants.VerifyUrl)
            .Respond("application/json", responseJson)
            .WithExactFormData("response=aaaaa&remoteip=bbbb&secret=cccc");

         var captcha = new ReCaptchaService(client: mockHttp.ToHttpClient());

         var response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc");

         response.Should().BeFalse();

         mockHttp.VerifyNoOutstandingExpectation();
      }
   }
}