[![Build status](https://ci.appveyor.com/api/projects/status/pw7x5xdqcvqrsrml/branch/master?svg=true)](https://ci.appveyor.com/project/BitArmory/recaptcha/branch/master) [![Nuget](https://img.shields.io/nuget/v/BitArmory.ReCaptcha.svg)](https://www.nuget.org/packages/BitArmory.ReCaptcha/) [![Users](https://img.shields.io/nuget/dt/BitArmory.ReCaptcha.svg)](https://www.nuget.org/packages/BitArmory.ReCaptcha/) <img src="https://raw.githubusercontent.com/BitArmory/ReCaptcha/master/docs/recaptcha.png" align='right' />

BitArmory.ReCaptcha for .NET and C#
===================================

Project Description
-------------------
:recycle: A minimal, no-drama, friction-less **C#** **HTTP** verification client for **Google**'s [**reCAPTCHA** API](https://www.google.com/recaptcha), supporing [**Cloudflare Turnstile**](https://developers.cloudflare.com/turnstile/).

The problem with current **ReCaptcha** libraries in **.NET** is that all of them take a hard dependency on the underlying web framework like **ASP.NET WebForms**, **ASP.NET MVC 5**, **ASP.NET Core**, or **ASP.NET Razor Pages**. 

Furthermore, current **reCAPTCHA** libraries for **.NET** are hard coded against the `HttpContext.Request` to retrieve the remote IP address of the visitor. Unfortunately, this method doesn't work if your website is behind a service like **CloudFlare** where the [`CF-Connecting-IP` header value](https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-Cloudflare-handle-HTTP-Request-headers) is the ***real*** IP address of the visitor on your site.

**BitArmory.ReCaptcha** is a minimal library that works across all **.NET** web frameworks without taking a hard dependency on any web framework. If you want to leverage platform specific features, like **MVC** ***Action Filters***, you'll need to implement your own `ActionFilter` that leverages the functionality in this library.

#### Supported Platforms
* **.NET Standard 1.3** or later
* **.NET Framework 4.5** or later

#### Supported reCAPTCHA Versions
* [**reCAPTCHA v2 (I'm not a robot)**][2] 
* [**reCAPTCHA v3 (Invisible)**][3]
* [**Cloudflare Turnstile (Managed or Invisible)**][4]

#### Crypto Tip Jar
<a href="https://commerce.coinbase.com/checkout/f78fc08f-f34f-40c5-8262-8595c3492f3a"><img src="https://raw.githubusercontent.com/BitArmory/ReCaptcha/master/docs/tipjar.png" /></a>
* :dog2: **Dogecoin**: `DGVC2drEMt41sEzEHSsiE3VTrgsQxGn5qe`


### Download & Install
**NuGet Package [BitArmory.ReCaptcha](https://www.nuget.org/packages/BitArmory.ReCaptcha/)**

```powershell
Install-Package BitArmory.ReCaptcha
```

Full Examples
--------
Various full and complete examples can be found here:

#### reCAPTCHA v3 (Invisible)
* [ASP.NET WebForms for Full Framework](https://github.com/BitArmory/ReCaptcha/tree/master/Examples/ReCaptchaV3.WebForms)


General Usage
-------------
### Getting Started
You'll need to create **reCAPTCHA** account. You can sign up [here](https://www.google.com/recaptcha)! After you sign up and setup your domain, you'll have two important pieces of information:
1. Your `site` key
2. Your `secret` key

This library supports: 
* [**reCAPTCHA v2 (I'm not a robot)**][2] 
* [**reCAPTCHA v3 (Invisible)**][3].
* [**Cloudflare Turnstile (Managed or Invisible)**][4]


## reCAPTCHA v3 (Invisible)
### Client-side Setup

Be sure to checkout [this video that describes how reCAPTCHA v3 works](https://www.youtube.com/watch?v=tbvxFW4UJdU) before implementing.

Then, on every page of your website, add the following JavaScript:
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
Every page should call `grecaptcha.execute` with some unique **action** `TAG`. [Read more about actions in the official docs here](https://developers.google.com/recaptcha/docs/v3#actions).

When it is time to validate an **HTTP** `POST` you'll need transfer the captcha `token` in the browser to a hidden HTML form field as shown below:

```html
<html>
  <body>
    <form action='/do-post' method='POST'>
      <input id="captcha" type="hidden" name="captcha" value="" />
    </form>
    <script>
      function ExecuteReCaptcha_OnSome_ButtonAction(){
        grecaptcha.ready(function() {
          grecaptcha.execute('GOOGLE_SITE_KEY', {action: 'SomeAction'})
            .then(function(token) {
               // Set `token` in a hidden form input.
               $("#captcha").val(token);
               
               //And finally submit the form by firing
               //off the HTTP POST over the wire at this
               //exact moment in time here.
            });
        });
      }
    </script>
  </body>
</html>
```
You'll need to execute `ExecuteReCaptcha_OnSome_ButtonAction()` function the moment the user decides to submit your form. Otherwise, if you run `grecaptcha.*` code during page load, the token being copied to the hidden field can expire after a few minutes. This means, if the user takes a long time filling out a form, the token copied at page load can expire and your server will validate an expired token by the time the form is submitted resulting in a failed captcha verification.

Therefore, you should execute the `ExecuteReCaptcha_OnSome_ButtonAction()` function on some `onclick=` event to get a fresh token before the form is submitted.

Also, keep in mind, `grecaptcha.execute()` returns a **JavaScript Promise**. You won't have a valid token in your `<form>` until the line `$("#captcha").val(token);` above executes. So you'll need to postpone the form submission until `$("#captcha").val(token);` is actually executed. Then, *and only then,* you can continue submitting the HTML form to have it validated on your server with a valid token. 

### Verifying the POST Server-side
When the `POST` is received on the server:
1. Get the client's IP address. If you're using **CloudFlare**, be sure to use the [`CF-Connecting-IP` header value][0].
2. Extract the `#captcha` value (client token) in the hidden **HTML** form field.
3. Use the `ReCaptchaService` to verify the client's **reCAPTCHA** is valid.

```csharp
//1. Get the client IP address in your chosen web framework
string clientIp = GetClientIpAddress();
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

<details><summary>GetClientIpAddress() in ASP.NET Core</summary>
<p>

**Note:** If your site is behind CloudFlare, be sure you're suing the [`CF-Connecting-IP` header value][0] instead.

```csharp
public string GetClientIpAddress(){
   return this.HttpContext.Connection.RemoteIpAddress.ToString();
}
```

</p>
</details>

<details><summary>GetClientIpAddress() in ASP.NET WebForms</summary>
<p>

**Note:** If your site is behind CloudFlare, be sure you're suing the [`CF-Connecting-IP` header value][0] instead.

```csharp
public string GetClientIpAddress(){
   return this.Request.UserHostAddress;
}
```

</p>
</details>         

You'll want to make sure the action name you choose for the request is legitimate. The `result.Score` is the probably of a human. So, you'll want to make sure you have a `result.Score > 0.5`; anything less is probably a bot.

## reCAPTCHA v2 (I'm not a robot)
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
1. Get the client's IP address. If you're using **CloudFlare**, be sure to use the [`CF-Connecting-IP` header value][0].
2. Extract the `g-recaptcha-response` (Client Response) **HTML** form field.
3. Use the `ReCaptchaService` to verify the client's **reCAPTCHA** is valid.

The following example shows how to verify the captcha during an **HTTP** `POST` back in **ASP.NET Core: Razor Pages**.

```csharp
//1. Get the client IP address in your chosen web framework
string clientIp = GetClientIpAddress();
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

<details><summary>GetClientIpAddress() in ASP.NET Core</summary>
<p>

**Note:** If your site is behind CloudFlare, be sure you're suing the [`CF-Connecting-IP` header value][0] instead.

```csharp
public string GetClientIpAddress(){
   return this.HttpContext.Connection.RemoteIpAddress.ToString();
}
```

</p>
</details>

<details><summary>GetClientIpAddress() in ASP.NET WebForms</summary>
<p>

**Note:** If your site is behind CloudFlare, be sure you're suing the [`CF-Connecting-IP` header value][0] instead.

```csharp
public string GetClientIpAddress(){
   return this.Request.UserHostAddress;
}
```

</p>
</details>    

That's it! **Happy verifying!** :tada:

### Cloudflare Turnstile

[Cloudflare Turnstile](https://developers.cloudflare.com/turnstile/) is an alternative to reCAPTCHA, providing a very similar interface to reCAPTCHA v2, which doesn't require user intereaction unless the visitor is suspected of being a bot.

After following the [instructions](https://developers.cloudflare.com/turnstile/get-started/) to get the client ready, and to generate your secret key, verifying the resposne on the server side is easy.

#### Client Side

More detailed instructions, including theming, can be found at
[https://developers.cloudflare.com/turnstile/get-started/](https://developers.cloudflare.com/turnstile/get-started/).

In short, you want to include the JavaScript in the `<head>`:

```html
<script src="https://challenges.cloudflare.com/turnstile/v0/api.js" async defer></script>
```

And then insert the widget:

```html
<div class="cf-turnstile" data-sitekey="your-sitekey" data-action="example-action"></div>
```

The *data-action* attribute can be verified later, on the server side.

#### Server Side

```csharp
// 1. Get the client IP address in your chosen web framework
string clientIp = GetClientIpAddress();
string captchaResponse = null;
string secret = "your_secret_key";

// 2. Extract the `cf-turnstile-response` field from the HTML form in your chosen web framework
if( this.Request.Form.TryGetValue(Constants.TurnstileClientResponseKey, out var formField) )
{
   capthcaResponse = formField;
}

// 3. Validate the response
var captchaApi = new ReCaptchaService(Constants.TurnstileVerifyUrl);
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

The *hostname* and *action* can be (and **should** be) verified by passing the `hostname` and `action` arguments to `captchaApi.Verify2Async`, for example:

```csharp
var isValid = await captchaApi.Verify2Async(
    capthcaResponse, clientIp, secret,
    hostname: "expected.hostname",
    action: "example-action",
);
```

The full response, including `cdata`, can be fetched using `captchaApi.Response2Async(capthcaResponse, clientIp, secret)`.


Building
--------
* Download the source code.
* Run `build.cmd`.

Upon successful build, the results will be in the `\__compile` directory. If you want to build NuGet packages, run `build.cmd pack` and the NuGet packages will be in `__package`.



[0]:https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-Cloudflare-handle-HTTP-Request-headers
[2]:#recaptcha-v2-im-not-a-robot
[3]:#recaptcha-v3-invisible-1
[4]:#cloudflare-turnstile
