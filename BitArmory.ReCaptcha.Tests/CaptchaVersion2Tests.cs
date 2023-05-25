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

      string ResponseJsonWithHostAndAction(bool isSuccess, string hostname, string action)
      {
         var json = @"{
  ""success"": {SUCCESS},
  ""challenge_ts"": ""2018-05-15T23:05:22Z"",
  ""hostname"": ""{HOSTNAME}"",
  ""action"": ""{ACTION}""
}"
         .Replace("{SUCCESS}", isSuccess.ToString().ToLower())
         .Replace("{HOSTNAME}", hostname)
         .Replace("{ACTION}", action);

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

      [Test]
      public async Task can_verify_a_captcha_from_another_url()
      {
         var responseJson = ResponseJson(true);

         var mockHttp = new MockHttpMessageHandler();

         mockHttp.Expect(HttpMethod.Post, Constants.TurnstileVerifyUrl)
            .Respond("application/json", responseJson)
            .WithExactFormData("response=aaaaa&remoteip=bbbb&secret=cccc");

         var captcha = new ReCaptchaService(Constants.TurnstileVerifyUrl, client: mockHttp.ToHttpClient());

         var response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc");

         response.Should().BeTrue();

         mockHttp.VerifyNoOutstandingExpectation();
      }

      [Test]
      public async Task can_verify_a_captcha_with_hostname_and_action()
      {
         var responseJson = ResponseJson(true);
         var mockHttp = new MockHttpMessageHandler();
         mockHttp.When(HttpMethod.Post, Constants.TurnstileVerifyUrl)
            .Respond("application/json", responseJson)
            .WithExactFormData("response=aaaaa&remoteip=bbbb&secret=cccc");
         var captcha = new ReCaptchaService(Constants.TurnstileVerifyUrl, client: mockHttp.ToHttpClient());

         var response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc", hostname: "localhost");
         response.Should().BeTrue();

         response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc", hostname: "not-localhost");
         response.Should().BeFalse();

         response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc", hostname: "not-localhost", action: "test");
         response.Should().BeFalse();

         response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc", hostname: "localhost", action: "test");
         response.Should().BeFalse();

         mockHttp.VerifyNoOutstandingExpectation();

         // Now generate responses with the action field filled out (and a different hostname)
         responseJson = ResponseJsonWithHostAndAction(true, "example.com", "test-action");
         mockHttp = new MockHttpMessageHandler();
         mockHttp.When(HttpMethod.Post, Constants.TurnstileVerifyUrl)
            .Respond("application/json", responseJson)
            .WithExactFormData("response=aaaaa&remoteip=bbbb&secret=cccc");
         captcha = new ReCaptchaService(Constants.TurnstileVerifyUrl, client: mockHttp.ToHttpClient());

         response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc", hostname: "localhost", action: "test-action");
         response.Should().BeFalse();

         response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc", hostname: "example.com", action: "test-action");
         response.Should().BeTrue();

         response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc", hostname: "example.com", action: "action");
         response.Should().BeFalse();

         response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc", hostname: "example.com");
         response.Should().BeTrue();

         response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc", action: "example.com");
         response.Should().BeFalse();

         response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc", action: "test-action");
         response.Should().BeTrue();

         response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc");
         response.Should().BeTrue();

         mockHttp.VerifyNoOutstandingExpectation();
      }
   }
}
