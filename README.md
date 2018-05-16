[![Build status](https://ci.appveyor.com/api/projects/status/3nq1hvf67yp0nswg/branch/master?svg=true)](https://ci.appveyor.com/project/bchavez/coinbase-commerce/branch/master) [![Nuget](https://img.shields.io/nuget/v/BitArmory.ReCaptcha.svg)](https://www.nuget.org/packages/Coinbase.Commerce/) [![Users](https://img.shields.io/nuget/dt/BitArmory.ReCaptcha.svg)](https://www.nuget.org/packages/Coinbase.Commerce/) <img src="https://raw.githubusercontent.com/bchavez/BitArmory.ReCaptcha/master/docs/recaptcha.png" align='right' />

BitArmory.ReCaptcha for .NET and C#
===================================

Project Description
-------------------
:recycle: A no-drama and friction-less **C#** HTTP verification client Google's [**reCAPTCHA** API](https://www.google.com/recaptcha).

The problem with current **ReCaptcha** libraries in **.NET** is that they are all take a hard dependency on the underlying web framework like **ASP.NET WebForms**, **ASP.NET MVC 5**, **ASP.NET Core**, or **ASP.NET Razor Pages**. 

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



### Server-side Setup


### Verifying


Building
--------
* Download the source code.
* Run `build.cmd`.

Upon successful build, the results will be in the `\__compile` directory. If you want to build NuGet packages, run `build.cmd pack` and the NuGet packages will be in `__package`.

---
*Note: This application/third-party library is not directly supported by Coinbase Inc. Coinbase Inc. makes no claims about this application/third-party library.  This application/third-party library is not endorsed or certified by Coinbase Inc.*