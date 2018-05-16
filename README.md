[![Build status](https://ci.appveyor.com/api/projects/status/pw7x5xdqcvqrsrml/branch/master?svg=true)](https://ci.appveyor.com/project/BitArmory/recaptcha/branch/master) [![Nuget](https://img.shields.io/nuget/v/BitArmory.ReCaptcha.svg)](https://www.nuget.org/packages/Coinbase.Commerce/) [![Users](https://img.shields.io/nuget/dt/BitArmory.ReCaptcha.svg)](https://www.nuget.org/packages/BitArmory.ReCapthca/) <img src="https://raw.githubusercontent.com/BitArmory/ReCaptcha/master/docs/recaptcha.png" align='right' />

BitArmory.ReCaptcha for .NET and C#
===================================

Project Description
-------------------
:recycle: A no-drama and friction-less **C#** HTTP verification client Google's [**reCAPTCHA** API](https://www.google.com/recaptcha).

The problem with current **ReCaptcha** libraries in **.NET** is that they all take a hard dependency on the underlying web framework like **ASP.NET WebForms**, **ASP.NET MVC 5**, **ASP.NET Core**, or **ASP.NET Razor Pages**. 

Furthermore, these reCAPTCHA libraries for **.NET** are hard coded against `HttpContext` to get the remote IP of a client but that usually breaks down if you're behind a service like **CloudFlare** where the [`CF-Connecting-IP` header value](https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-Cloudflare-handle-HTTP-Request-headers) is the ***real*** remote IP address of the visitor on your site.

This is a minimal library that works across all **.NET** technology frameworks without taking a hard dependency on the underlying web framework. If you want to leverage platform specific features on your platform, like **MVC** ***Action Filters*** you'll need to implement your own `ActionFilter`.

#### Supported Platforms
* **.NET Standard 1.3** or later
* **.NET Framework 4.5** or later

#### Crypto Tip Jar
<a href="https://commerce.coinbase.com/checkout/f78fc08f-f34f-40c5-8262-8595c3492f3a"><img src="https://raw.githubusercontent.com/BitArmory/ReCaptcha/master/docs/tipjar.png" /></a>
* :dog2: **Dogecoin**: `DGVC2drEMt41sEzEHSsiE3VTrgsQxGn5qe`


### Download & Install
**Nuget Package [BitArmory.ReCaptcha](https://www.nuget.org/packages/BitArmory.ReCaptcha/)**

```powershell
Install-Package BitArmory.ReCaptcha
```

Usage
-----
### Getting Started
You'll need to create **reCAPTCHA** account. You can sign up [here](https://www.google.com/recaptcha)!

### Client-side Setup
Add the following to your **html** form `POST`:
```html
<html>
  <body>
    <form method="post">
        ...
        <div class="g-recaptcha text-center" data-sitekey="4MafbFZV5W..."></div>
        ...
    </form>
  </body>
</html>
```


### Verifying the POST Server-side
When the `POST` is received on the server:
* Get the client's IP address only. If you're using **CloudFlare**, be sure to use the [`CF-Connecting-IP` header value](https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-Cloudflare-handle-HTTP-Request-headers).
* Extract the `g-recaptcha-response` form value.
* Verify the **HTTP** `POST`.

The following is an example shows how to verify the captcha during an **HTTP** `POST` back in **ASP.NET Core: Razor Pages**.

```csharp
string clientIp = this.HttpContext.Connection.RemoteIpAddress.ToString();
string captchaResponse = null;
string secret = "wewasAZwh28...";

if( this.Request.Form.TryGetValue(Constants.ClientResponseKey, out var ghr) )
{
   capthcaResponse = ghr;
}

var captchaApi = new ReCaptchaService();

var isValid = await captchaApi.VerifyAsync(capthcaResponse, clientIp, secret);
if( !isValid )
{
   this.ModelState.AddModelError("captcha", "The reCAPTCHA is not valid.");
   return new BadRequestResult();
}
else{
   //continue processing, everything is okay!
}
```
That's it! **Happy verifying!** :tada:


Building
--------
* Download the source code.
* Run `build.cmd`.

Upon successful build, the results will be in the `\__compile` directory. If you want to build NuGet packages, run `build.cmd pack` and the NuGet packages will be in `__package`.

---
*Note: This application/third-party library is not directly supported by Coinbase Inc. Coinbase Inc. makes no claims about this application/third-party library.  This application/third-party library is not endorsed or certified by Coinbase Inc.*