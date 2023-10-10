# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2023-07-02
### Changes
- Added Authorization Code flow with PKCE (Proof Key for Code Exchange) `AuthorizationCodeFlowWithPkce`.

## [1.1.3] - 2023-02-26
### Changes
- Json serializable classes marked with `[Preserve]` attribute to prevent them from being stripped out (i.e. UWP platform)

## [1.1.2] - 2023-02-1
### Changes
- Redirect URL can be used without forward slash with `StandaloneBrowser`
- Added `DeepLinkBrowser` through a custom scheme (aka protocol) for Android, iOS, or UWP

## [1.1.1] - 2022-12-16
### Changes
- Added `csc.rsp` to fix `error CS0103: The name 'HttpUtility' does not exist in the current context` error

## [1.1.0] - 2022-10-04
### Changes
- Added `MockServer` to be able to test app against a mock oauth provider
- Fixed access token expiration comparison

## [1.0.2] - 2022-09-28
### Changes
- Bug fixes

## [1.0.1] - 2022-09-27
### Changes
- Added Facebook auth

## [1.0.0] - 2022-09-27
### Changes
- Initial release
