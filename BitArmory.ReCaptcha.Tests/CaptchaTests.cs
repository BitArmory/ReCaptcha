using System.Threading.Tasks;
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

      void SetupSuccessResponse()
      {
         var json =
            @"{
  ""success"": true,
  ""challenge_ts"": ""2018-05-15T23:05:22Z"",
  ""hostname"": ""localhost""
}";
         this.server.RespondWith(json);
      }

      [Test]
      public async Task can_verify_a_captcha()
      {
         await captcha.VerifyAsync("aaaaa", "bbbb", "cccc");

         server.ShouldHaveCalled(ReCaptchaConstants.VerifyUrl)
            .WithRequestBody("response=aaaaa&remoteip=bbbb&secret=cccc");
      }
   }
}