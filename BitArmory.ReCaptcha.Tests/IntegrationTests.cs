using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace BitArmory.ReCaptcha.Tests
{
   [Explicit]
   [TestFixture]
   public class IntegrationTests
   {
      [Test]
      public async Task can_make_process_bad_request()
      {
         var service = new ReCaptchaService();
         service.EnableFiddlerDebugProxy("http://localhost.:8888");

         var response = await service.Verify3Async("ffff", "127.0.0.1", "bbbb");

         response.IsSuccess.Should().BeFalse();
      }


      [Test]
      public async Task can_make_good_request()
      {
         var service = new ReCaptchaService();
         service.EnableFiddlerDebugProxy("http://localhost.:8888");

         //response.IsSuccess.Should().BeTrue();
      }
   }
}