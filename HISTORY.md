## v5.0.1
* Improve XML docs.

## v5.0.0
* Uses SourceLink for F12 navigation in Visual Studio.
* The `BitArmory.ReCaptcha.dll` assembly is now signed.
* Issue 4: Fixed wrong parameter name in exception when clientToken is null or whitespace.

## v4.0.0
* Removed Newtonsoft.Json dependency.
* Removed Flurl.Http dependency.
* Refactored internals to work without dependencies. Underlying implementation uses `HttpClient` in the BCL. 

## v3.0.0
* Support for reCAPTCHA v3. 

## v1.0.0
* Initial release.