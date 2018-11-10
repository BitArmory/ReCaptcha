using System.Threading.Tasks;
using FluentAssertions;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace BitArmory.ReCaptcha.Tests
{
   [TestFixture]
   public class CaptchaTests
   {
      private ReCaptchaService captcha;
      private HttpTest server;

      [SetUp]
      public void BeforeEachTest()
      {
         server = new HttpTest();
         captcha = new ReCaptchaService();
      }
      [TearDown]
      public void AfterEachTest()
      {
         server.Dispose();
      }

      void SetupResponse(bool isSuccess)
      {
         var json =
            @"{
  ""success"": {SUCCESS},
  ""challenge_ts"": ""2018-05-15T23:05:22Z"",
  ""hostname"": ""localhost""
}".Replace("{SUCCESS}", isSuccess.ToString().ToLower());
         this.server.RespondWith(json);
      }

      [Test]
      public async Task can_verify_a_captcha()
      {
         SetupResponse(true);

         var response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc");

         response.Should().BeTrue();

         server.ShouldHaveCalled(Constants.VerifyUrl)
            .WithRequestBody("response=aaaaa&remoteip=bbbb&secret=cccc");
      }

      [Test]
      public async Task can_verify_failed_response()
      {
         SetupResponse(false);

         var response = await captcha.Verify2Async("aaaaa", "bbbb", "cccc");

         response.Should().BeFalse();

         server.ShouldHaveCalled(Constants.VerifyUrl)
            .WithRequestBody("response=aaaaa&remoteip=bbbb&secret=cccc");
      }
   }
}