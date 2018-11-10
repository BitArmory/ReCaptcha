[![Build status](https://ci.appveyor.com/api/projects/status/pw7x5xdqcvqrsrml/branch/master?svg=true)](https://ci.appveyor.com/project/BitArmory/recaptcha/branch/master) [![Nuget](https://img.shields.io/nuget/v/BitArmory.ReCaptcha.svg)](https://www.nuget.org/packages/BitArmory.ReCaptcha/) [![Users](https://img.shields.io/nuget/dt/BitArmory.ReCaptcha.svg)](https://www.nuget.org/packages/BitArmory.ReCaptcha/) <img src="https://raw.githubusercontent.com/BitArmory/ReCaptcha/master/docs/recaptcha.png" align='right' />

BitArmory.ReCaptcha for .NET and C#
===================================

Project Description
-------------------
:recycle: A minimal, no-drama, friction-less **C#** **HTTP** verification client for **Google**'s [**reCAPTCHA** API](https://www.google.com/recaptcha).

The problem with current **ReCaptcha** libraries in **.NET** is that all of them take a hard dependency on the underlying web framework like **ASP.NET WebForms**, **ASP.NET MVC 5**, **ASP.NET Core**, or **ASP.NET Razor Pages**. 

Furthermore, current **reCAPTCHA** libraries for **.NET** are hard coded against the `HttpContext.Request` to retrieve the remote IP address of the visitor. Unfortunately, this method doesn't work if your website is behind a service like **CloudFlare** where the [`CF-Connecting-IP` header value](https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-Cloudflare-handle-HTTP-Request-headers) is the ***real*** IP address of the visitor on your site.

**BitArmory.ReCaptcha** is a minimal library that works across all **.NET** web frameworks without taking a hard dependency on any web framework. If you want to leverage platform specific features, like **MVC** ***Action Filters***, you'll need to implement your own `ActionFilter` that leverages the functionality in this library.

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
You'll need to create **reCAPTCHA** account. You can sign up [here](https://www.google.com/recaptcha)! After you sign up and setup your domain, you'll have two important pieces of information:
1. Your `site` key
2. Your `secret` key

This library both **reCAPTCHA v2** and **reCAPTCHA v3**.

## reCAPTCHA v3
### Client-side Setup

On every page of your website, add the following JavaScript:
```html
<html>
  <head>
    <script src='https://www.google.com/recaptcha/api.js?render=GOOGLE_SITE_KEY'></script>
  </head>
  <body>
    ...
    <script>
        grecaptcha.ready(function() {
          grecaptcha.execute('GOOGLE_SITE_KEY', {action: 'TAG'});
        });
    </script>
  </body>
</html>
```
Every page should call `grecaptcha.execute` with some unique `action:TAG`. When it's time to validate an **HTTP** `POST` you'll need to do the following:

```html
<html>
  <body>
    <form action='/do-post' method='POST'>
      <input id="captcha" type="hidden" name="captcha" value="" />
    </form>
    <script>
      grecaptcha.ready(function() {
        grecaptcha.execute('GOOGLE_SITE_KEY', {action: 'SOME_ACTION'})
          .then(function(token) {
             // Set `token` in a hidden form input.
             $("#captcha").val(token);
          });
      });
    </script>
  </body>
</html>
```
### Verifying the POST Server-side
When the `POST` is received on the server:
1. Get the client's IP address. If you're using **CloudFlare**, be sure to use the [`CF-Connecting-IP` header value](https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-Cloudflare-handle-HTTP-Request-headers).
2. Extract the `#captcha` value (client token) in the hidden **HTML** form field.
3. Use the `ReCaptchaService` to verify the client's **reCAPTCHA** is valid.

```csharp
//1. Get the client IP address in your chosen web framework
string clientIp = this.HttpContext.Connection.RemoteIpAddress.ToString();
string token = null;
string secret = "your_secret_key";

//2. Extract the `#captcha` field from the hidden HTML form in your chosen web framework
if( this.Request.Form.TryGetValue("captcha", out var formField) )
{
   token = formField;
}

//3. Validate the reCAPTCHA with Google
var captchaApi = new ReCaptchaService();
var result = await captchaApi.Verify3Async(token, clientIp, secret);

if( !result.IsSuccess || result.Action != "SOME_ACTION" || result.Score < 0.5 )
{
   // The POST is not valid
   return new BadRequestResult();
}
else{
   //continue processing, everything is okay!
}
```
You'll want to make sure the action name you choose for the reuqest is legitmate. The `result.score` is the probably of a human. So, you'll want to make sure you have a `result.Score > 0.5`; anything less is probably a bot.

## reCAPTCHA v2
### Client-side Setup
Add the following `<div class="g-recaptcha">` and `<script>` tags to your **HTML** form:
```html
<html>
  <body>
    <form method="POST">
        ...
        <div class="g-recaptcha" data-sitekey="your_site_key"></div>
        <input type="submit" value="Submit">
    </form>

    <script src="https://www.google.com/recaptcha/api.js" async defer></script>
  </body>
</html>
```


### Verifying the POST Server-side
When the `POST` is received on the server:
1. Get the client's IP address. If you're using **CloudFlare**, be sure to use the [`CF-Connecting-IP` header value](https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-Cloudflare-handle-HTTP-Request-headers).
2. Extract the `g-recaptcha-response` (Client Response) **HTML** form field.
3. Use the `ReCaptchaService` to verify the client's **reCAPTCHA** is valid.

The following example shows how to verify the captcha during an **HTTP** `POST` back in **ASP.NET Core: Razor Pages**.

```csharp
//1. Get the client IP address in your chosen web framework
string clientIp = this.HttpContext.Connection.RemoteIpAddress.ToString();
string captchaResponse = null;
string secret = "your_secret_key";

//2. Extract the `g-recaptcha-response` field from the HTML form in your chosen web framework
if( this.Request.Form.TryGetValue(Constants.ClientResponseKey, out var formField) )
{
   capthcaResponse = formField;
}

//3. Validate the reCAPTCHA with Google
var captchaApi = new ReCaptchaService();
var isValid = await captchaApi.Verify2Async(capthcaResponse, clientIp, secret);
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
