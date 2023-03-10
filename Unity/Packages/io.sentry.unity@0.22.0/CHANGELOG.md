# Changelog

## 0.22.0

### Fixes

- Correctly byte-swap ELF build-ids ([#872](https://github.com/getsentry/sentry-unity/pull/872))
- Wizard autoselects with only one option available ([#891](https://github.com/getsentry/sentry-unity/pull/891))
- Reenabled IL2CPP line number support for mobile ([#902](https://github.com/getsentry/sentry-unity/pull/902))

### Features

- Added parameters to the options to control screenshot quality ([#912](https://github.com/getsentry/sentry-unity/pull/912))
- Added highlights and info messages to editor configuration window ([#910](https://github.com/getsentry/sentry-unity/pull/910))
- Added programmatic access to enable the experimental IL2CPP line numbers support ([#905](https://github.com/getsentry/sentry-unity/pull/905))
- Bump Java SDK to v6.3.0 ([#887](https://github.com/getsentry/sentry-unity/pull/887), [#894](https://github.com/getsentry/sentry-unity/pull/894))
  - [changelog](https://github.com/getsentry/sentry-java/blob/main/CHANGELOG.md#630)
  - [diff](https://github.com/getsentry/sentry-java/compare/6.1.4...6.3.0)
- Bump CLI to v2.5.0 ([#889](https://github.com/getsentry/sentry-unity/pull/889), [#898](https://github.com/getsentry/sentry-unity/pull/898))
  - [changelog](https://github.com/getsentry/sentry-cli/blob/master/CHANGELOG.md#250)
  - [diff](https://github.com/getsentry/sentry-cli/compare/2.3.1...2.5.0)
- Bump Cocoa SDK to v7.22.0 ([#892](https://github.com/getsentry/sentry-unity/pull/892), [#909](https://github.com/getsentry/sentry-unity/pull/909))
  - [changelog](https://github.com/getsentry/sentry-cocoa/blob/master/CHANGELOG.md#7220)
  - [diff](https://github.com/getsentry/sentry-cocoa/compare/7.20.0...7.22.0)
- Bump .NET SDK to v3.20.1 ([#907](https://github.com/getsentry/sentry-unity/pull/907), [#911](https://github.com/getsentry/sentry-unity/pull/911))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/main/CHANGELOG.md#3201)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.19.0...3.20.1)

## 0.21.0

### Fixes

- Fix ProGuard setup if build.gradle uses CRLF (Windows) line breaks ([#885](https://github.com/getsentry/sentry-unity/pull/885))

### Features

- Attach screenshots to native errors on iOS ([#878](https://github.com/getsentry/sentry-unity/pull/878))
- Bump CLI to v2.3.1 ([#875](https://github.com/getsentry/sentry-unity/pull/875))
  - [changelog](https://github.com/getsentry/sentry-cli/blob/master/CHANGELOG.md#231)
  - [diff](https://github.com/getsentry/sentry-cli/compare/2.3.0...2.3.1)
- Bump Cocoa SDK to v7.20.0 ([#877](https://github.com/getsentry/sentry-unity/pull/877))
  - [changelog](https://github.com/getsentry/sentry-cocoa/blob/master/CHANGELOG.md#7200)
  - [diff](https://github.com/getsentry/sentry-cocoa/compare/7.19.0...7.20.0)

## 0.20.1

### Fixes

- Explicitly disable Windows x86 native-error tracking and IL2CPP processing integration ([#871](https://github.com/getsentry/sentry-unity/pull/871))

## 0.20.0

### Features

- Generate and upload IL2CPP line mappings alongside debug files ([#790](https://github.com/getsentry/sentry-unity/pull/790))
- Launch a setup wizard after installation ([#780](https://github.com/getsentry/sentry-unity/pull/780))
- Reduced automated screenshot attachment controls to a simple toggle ([#784](https://github.com/getsentry/sentry-unity/pull/784))
- Disable AutoSessionTracking on unknown platforms ([#840](https://github.com/getsentry/sentry-unity/pull/840))
- Support Android apps minified with Proguard ([#844](https://github.com/getsentry/sentry-unity/pull/844))
- Bump Cocoa SDK to v7.19.0 ([#802](https://github.com/getsentry/sentry-unity/pull/802), [#821](https://github.com/getsentry/sentry-unity/pull/821), [#835](https://github.com/getsentry/sentry-unity/pull/835), [#854](https://github.com/getsentry/sentry-unity/pull/854), [#868](https://github.com/getsentry/sentry-unity/pull/868))
  - [changelog](https://github.com/getsentry/sentry-cocoa/blob/master/CHANGELOG.md#7190)
  - [diff](https://github.com/getsentry/sentry-cocoa/compare/7.16.0...7.19.0)
- Bump .NET SDK to v3.19.0 ([#807](https://github.com/getsentry/sentry-unity/pull/807), [#860](https://github.com/getsentry/sentry-unity/pull/860))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/main/CHANGELOG.md#3190)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.17.0...3.19.0)
- Bump Java SDK to v6.1.4 ([#811](https://github.com/getsentry/sentry-unity/pull/811), [#820](https://github.com/getsentry/sentry-unity/pull/820), [#828](https://github.com/getsentry/sentry-unity/pull/828), [#847](https://github.com/getsentry/sentry-unity/pull/847), [#857](https://github.com/getsentry/sentry-unity/pull/857))
  - [changelog](https://github.com/getsentry/sentry-java/blob/main/CHANGELOG.md#614)
  - [diff](https://github.com/getsentry/sentry-java/compare/6.0.0-1-gc62a9f7a...6.1.4)
- Bump Native SDK to v0.4.18 ([#810](https://github.com/getsentry/sentry-unity/pull/810), [#824](https://github.com/getsentry/sentry-unity/pull/824))
  - [changelog](https://github.com/getsentry/sentry-native/blob/master/CHANGELOG.md#0418)
  - [diff](https://github.com/getsentry/sentry-native/compare/0.4.15-7-g9eecb1b...0.4.18)
- Bump CLI to v2.3.0 ([#826](https://github.com/getsentry/sentry-unity/pull/826), [#867](https://github.com/getsentry/sentry-unity/pull/867))
  - [changelog](https://github.com/getsentry/sentry-cli/blob/master/CHANGELOG.md#230)
  - [diff](https://github.com/getsentry/sentry-cli/compare/1.71.0...2.3.0)

### Fixes

- Only use application-not-responding detection in C#, disable in native plugins ([#852](https://github.com/getsentry/sentry-unity/pull/852))
- The SDK no longer accesses the disk on unknown platforms (e.g. Switch) ([#865](https://github.com/getsentry/sentry-unity/pull/865))

## 0.19.0

### Features

- Add rich context info to native crashes ([#747](https://github.com/getsentry/sentry-unity/pull/747))
- Include build ID in an event release info ([#795](https://github.com/getsentry/sentry-unity/pull/795))

### Fixes

- Don't report Application-Not-Responding while the app is in the background ([#796](https://github.com/getsentry/sentry-unity/pull/796))

## 0.18.0

### Features

- Capture Native Instruction Addrs for Exceptions ([#683](https://github.com/getsentry/sentry-unity/pull/683))
- Enable native crash support with Mono scripting backend on Android, Windows and Linux ([#751](https://github.com/getsentry/sentry-unity/pull/751))
- Application-Not-Responding detection ([#771](https://github.com/getsentry/sentry-unity/pull/771))
- Allow uploading sources for debug files ([#773](https://github.com/getsentry/sentry-unity/pull/773))
- Bump Sentry Java SDK to v6.0.0 ([#787](https://github.com/getsentry/sentry-unity/pull/787))
  - [changelog](https://github.com/getsentry/sentry-java/blob/6.0.0/CHANGELOG.md?plain=1#L3..L73)
  - [diff](https://github.com/getsentry/sentry-java/compare/6.0.0-rc.1...6.0.0)

## 0.17.0

### Features

- The SDK now uses the Unity Log Handler to capture errors, instead of manually parsing events received on Application.logMessageReceived ([#731](https://github.com/getsentry/sentry-unity/pull/731))
- Linux native crash support ([#734](https://github.com/getsentry/sentry-unity/pull/734))
- Collect context information synchronously during init to capture it for very early events ([#744](https://github.com/getsentry/sentry-unity/pull/744))
- Automatic user IDs on native crashes & .NET events ([#728](https://github.com/getsentry/sentry-unity/pull/728))
- Use single-threaded HTTP transport on unknown platforms ([#756](https://github.com/getsentry/sentry-unity/pull/756))
- Disable offline caching on unknown platforms ([#770](https://github.com/getsentry/sentry-unity/pull/770))
- Bump Sentry Cocoa SDK to v7.16.0 ([#725](https://github.com/getsentry/sentry-unity/pull/725))
  - [changelog](https://github.com/getsentry/sentry-cocoa/blob/7.16.0/CHANGELOG.md?plain=1#L3..L38)
  - [diff](https://github.com/getsentry/sentry-cocoa/compare/7.13.0...7.16.0)
- Bump Sentry Java SDK to v6.0.0-rc.1 ([#725](https://github.com/getsentry/sentry-unity/pull/725))
  - [changelog](https://github.com/getsentry/sentry-java/blob/6.0.0-rc.1/CHANGELOG.md?plain=1#L3..L79)
  - [diff](https://github.com/getsentry/sentry-java/compare/6.0.0-alpha.4...6.0.0-rc.1)

## 0.16.0

### Features

- macOS native crash support ([#710](https://github.com/getsentry/sentry-unity/pull/710))
- The SentryUnityOptions now provide a method to disable the UnityLoggingIntegration ([#724](https://github.com/getsentry/sentry-unity/pull/724))

### Fixes

- The automated debug symbol upload now works with Unity 2021.2 and newer ([#730](https://github.com/getsentry/sentry-unity/pull/730))
- Dropped support for Sentry options as Json ([#709](https://github.com/getsentry/sentry-unity/pull/709))
  - If you're migrating from version 0.3.0 or older, make sure to upgrade to 0.15.0 first, as it is the last version supporting the automated conversion of the options as Json file to a Scriptable Object.
- Bump Sentry .NET SDK 3.17.0 ([#726](https://github.com/getsentry/sentry-unity/pull/726))
  - [changelog 3.17.0](https://github.com/getsentry/sentry-dotnet/blob/3.17.0/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.16.0...3.17.0)

## 0.15.0

### Features

- WebGL - .NET support ([#657](https://github.com/getsentry/sentry-unity/pull/657))

### Fixes

- The SDK no longer creates a custom link.xml ([#707](https://github.com/getsentry/sentry-unity/pull/707))
- Fixed `DebugOnlyInEditor` only applying to the Unity logger ([#706](https://github.com/getsentry/sentry-unity/pull/706))
- Sentry no longer fails to send events in Unity 2019.4 IL2CPP builds for macOS ([#701](https://github.com/getsentry/sentry-unity/pull/701))
- Bump Sentry Cocoa SDK 7.13.0 ([#697](https://github.com/getsentry/sentry-unity/pull/697))
  - [changelog](https://github.com/getsentry/sentry-cocoa/blob/7.13.0/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-cocoa/compare/7.7.0...7.13.0)

## 0.14.0

### Features

- Capture `Debug.LogError()` and `Debug.LogException()` also on background threads ([#673](https://github.com/getsentry/sentry-unity/pull/673))
- Adding override for Sentry CLI URL ([#666](https://github.com/getsentry/sentry-unity/pull/666))
- Option to automatically attach screenshots to all events ([#670](https://github.com/getsentry/sentry-unity/pull/670))

### Fixes

- Refactor InApp logic from Stack Traces ([#661](https://github.com/getsentry/sentry-unity/pull/661))
- Whitespaces no longer cause issues when uploading symbols for Windows native ([#655](https://github.com/getsentry/sentry-unity/pull/655))
- AndroidManifest update removes previous `io.sentry` entries ([#652](https://github.com/getsentry/sentry-unity/pull/652))
- Bump Sentry .NET SDK 3.16.0 ([#678](https://github.com/getsentry/sentry-unity/pull/678))
  - [changelog 3.16.0](https://github.com/getsentry/sentry-dotnet/blob/3.16.0/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.14.1...3.16.0)
- Bump Sentry Java SDK to v6.0.0-alpha.4 ([#653](https://github.com/getsentry/sentry-unity/pull/653))
  - [changelog](https://github.com/getsentry/sentry-java/blob/6.0.0-alpha.4/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-java/compare/5.5.0...6.0.0-alpha.4)

## 0.13.0

### Features

- Windows - include sentry.dll & .pdb in debug symbol upload ([#641](https://github.com/getsentry/sentry-unity/pull/641))

### Fixes

- Resolved issue of the SDK accessing the same cache directory with multiple game instances running ([#643](https://github.com/getsentry/sentry-unity/pull/643))
- Bump Sentry .NET SDK 3.15.0 ([#639](https://github.com/getsentry/sentry-unity/pull/639))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/3.15.0/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.14.1...3.15.0)

## 0.12.0

### Features

- The SDK no longer depends on Unity's iOS module for non iOS builds ([#601](https://github.com/getsentry/sentry-unity/pull/601))
- Update native SDK to v0.4.16 (pre-release) ([#615](https://github.com/getsentry/sentry-unity/pull/615))
- Windows native support (64-bit)
  - native crash handler ([#380](https://github.com/getsentry/sentry-unity/pull/380))
  - configuration & log forwarding ([#577](https://github.com/getsentry/sentry-unity/pull/577))
  - scope synchronization ([#546](https://github.com/getsentry/sentry-unity/pull/546))
  - symbol upload while building through Unity ([#607](https://github.com/getsentry/sentry-unity/pull/607))

### Fixes

- iOS builds no longer break when native support disabled or not available ([#592](https://github.com/getsentry/sentry-unity/pull/592))
- Close sentry instance when quitting the app ([#608](https://github.com/getsentry/sentry-unity/pull/608))
- iOS options.CrashedLastRun() reported an incorrect value ([#615](https://github.com/getsentry/sentry-unity/pull/615))

## 0.11.0

### Features

- Config window support for programmatic options configuration ([#569](https://github.com/getsentry/sentry-unity/pull/569))

## 0.10.1

### Features

- Samples include programmatic options configuration snippet ([#568](https://github.com/getsentry/sentry-unity/pull/568))
- Support for programmatic options configuration ([#564](https://github.com/getsentry/sentry-unity/pull/564))

### Fixes

- Bump Sentry .NET SDK 3.14.1 ([#573](https://github.com/getsentry/sentry-unity/pull/573))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/3.14.1/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.14.0...3.14.1)

## 0.10.0

### Features

- Updated native samples to only display buttons for the currently active platform ([#551](https://github.com/getsentry/sentry-unity/pull/551))
- Unity logged exceptions are marked as unhandled by default ([#542](https://github.com/getsentry/sentry-unity/pull/542))

### Fixes

- Sentry.Unity.Editor.iOS.dll no longer breaks builds on Windows when the iOS module has not been installed ([#559](https://github.com/getsentry/sentry-unity/pull/559))
- Importing the link.xml when opening the config window no longer causes an infinite loop ([#539](https://github.com/getsentry/sentry-unity/pull/539))
- Bump Sentry .NET SDK 3.14.0 ([#561](https://github.com/getsentry/sentry-unity/pull/561))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/3.14.0/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.13.0...3.14.0)

## 0.9.4

### Fixes

- Android Native Support no longer crashes when built with Mono while trying to capture a crash ([#524](https://github.com/getsentry/sentry-unity/pull/524))
- Bump Sentry CLI 1.72.0 ([#526](https://github.com/getsentry/sentry-unity/pull/526))
  - [changelog](https://github.com/getsentry/sentry-cli/releases/tag/1.72.0)
  - [diff](https://github.com/getsentry/sentry-cli/compare/1.71.0...1.72.0)
- Bump Sentry .NET SDK 3.13.0 ([#503](https://github.com/getsentry/sentry-unity/pull/503))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/3.13.0/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.12.3...3.13.0)

## 0.9.3

### Fixes

- Automated Android symbols upload now uses valid paths on Windows ([#509](https://github.com/getsentry/sentry-unity/pull/509))

## 0.9.2

### Features

- Tag whether event was created in the UI thread ([#495](https://github.com/getsentry/sentry-unity/pull/495))

### Fixes

- Fix missing Sentry/Sentry.h ([#504](https://github.com/getsentry/sentry-unity/pull/504))
- Automated Android symbols upload now correctly escapes sentry-cli executable path ([#507](https://github.com/getsentry/sentry-unity/pull/507))

## 0.9.1

### Fixes

- Suppress errors when adding attachments ([#485](https://github.com/getsentry/sentry-unity/pull/485))

## 0.9.0

### Features

- Sample scene with custom context and screenshot ([#472](https://github.com/getsentry/sentry-unity/pull/472))

### Fixes

- Initializing the SDK with an options object won't bypass default option values ([#469](https://github.com/getsentry/sentry-unity/pull/469))
- Fixed overwriting Xcode build properties ([#466](https://github.com/getsentry/sentry-unity/pull/466))
- Xcode exports no longer break with sentry-cli already added ([#457](https://github.com/getsentry/sentry-unity/pull/457))
- Explicitly set <SignAssembly>false</SignAssembly> ([#470](https://github.com/getsentry/sentry-unity/pull/470)). So that Sentry.dll is not strong named when consumed inside Unity.
- Bump Sentry .NET SDK 3.12.3 ([#484](https://github.com/getsentry/sentry-unity/pull/484))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/3.12.3/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.11.1...3.12.3)
- Bump Sentry Cocoa SDK 7.7.0 ([#481](https://github.com/getsentry/sentry-unity/pull/481))
  - [changelog](https://github.com/getsentry/sentry-cocoa/blob/7.7.0/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-cocoa/compare/7.5.2...7.7.0)
- Bump Sentry Android SDK 5.5.0 ([#474](https://github.com/getsentry/sentry-unity/pull/474))
  - [changelog](https://github.com/getsentry/sentry-java/blob/5.5.0/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-java/compare/5.4.0...5.5.0)

## 0.8.0

### Features

- ScriptableSentryUnityOptions is now public ([#419](https://github.com/getsentry/sentry-unity/pull/419))
- Automated symbols upload for iOS builds when bitcode is enabled ([#444](https://github.com/getsentry/sentry-unity/pull/444))
- Automated symbols upload for iOS builds when bitcode is disabled ([#443](https://github.com/getsentry/sentry-unity/pull/443))

### Fixes

- Android: Automated symbol upload no longer breaks non IL2CPP builds ([#450](https://github.com/getsentry/sentry-unity/pull/450))
- Config window now saves changes to sentry-cli options ([#454](https://github.com/getsentry/sentry-unity/pull/454))
- Sentry no longer requires Xcode projects to be exported on macOS ([#442](https://github.com/getsentry/sentry-unity/pull/442))

## 0.7.0

### Features

- Added automated symbols upload support for Android ([#408](https://github.com/getsentry/sentry-unity/pull/408))
- Avoid DLL conflict with other Unity packages ([#425](https://github.com/getsentry/sentry-unity/issues/425))

## 0.6.2

- fix release packaging ([#417](https://github.com/getsentry/sentry-unity/pull/417))

## 0.6.1

### Features

- Added sentry-cli to Unity package ([#414](https://github.com/getsentry/sentry-unity/pull/414))

### Fixes

- Added missing release string validation ([#389](https://github.com/getsentry/sentry-unity/pull/389))
- Sentry internal logs no longer show up as breadcrumbs ([#377](https://github.com/getsentry/sentry-unity/pull/377))
- Fixed missing context data when initializing SDK programmatically ([#376](https://github.com/getsentry/sentry-unity/pull/376))
- Fixed CaptureInEditor flag when initializing SDK programmatically ([#370](https://github.com/getsentry/sentry-unity/pull/370))
- Preventing numeric options to be set negative in the editor window ([#364](https://github.com/getsentry/sentry-unity/pull/364))
- Bump Sentry .NET SDK 3.11.1 ([#407](https://github.com/getsentry/sentry-unity/pull/407))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/3.11.1/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.9.3...3.11.1)
- Bump Sentry Cocoa SDK 7.5.2 ([#407](https://github.com/getsentry/sentry-unity/pull/407))
  - [changelog](https://github.com/getsentry/sentry-cocoa/blob/7.5.2/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-cocoa/compare/7.3.0...7.5.2)
- Bump Sentry Android SDK 5.4.0 ([#411](https://github.com/getsentry/sentry-unity/pull/411))
  - [changelog](https://github.com/getsentry/sentry-java/blob/5.4.0/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-java/compare/5.2.1...5.4.0)

## 0.6.0

### Features

- Added iOS simulator support ([#358](https://github.com/getsentry/sentry-unity/pull/358))
- Android Native Support ([#307](https://github.com/getsentry/sentry-unity/pull/307))
- Android mark sessions as crashed ([#347](https://github.com/getsentry/sentry-unity/pull/347))
- Android native bridge for scope sync ([#308](https://github.com/getsentry/sentry-unity/pull/308))
- iOS native bridge for scope sync ([#296](https://github.com/getsentry/sentry-unity/pull/296))
- Sample: Throw exceptions in C++ and Objective-C. C++ segfault ([#342](https://github.com/getsentry/sentry-unity/pull/342))
- Update Unity from 2019.4.21f to 2019.4.30f ([#350](https://github.com/getsentry/sentry-unity/pull/350))

### Fixes

- Fixed Xcode generation with invalid or disabled SentryOptions ([#330](https://github.com/getsentry/sentry-unity/pull/330))
- Fixed iOS support related reference resolution issue for Windows ([#325](https://github.com/getsentry/sentry-unity/pull/325))
- Import link.xml caused an infinite loop ([#315](https://github.com/getsentry/sentry-unity/pull/315))
- Removed unused .asmdefs which clears a warning from console ([#316](https://github.com/getsentry/sentry-unity/pull/316))
- Don't send negative line number ([#317](https://github.com/getsentry/sentry-unity/pull/317))
- Android SDK: re-installation of native backend through C# ([#339](https://github.com/getsentry/sentry-unity/pull/339))
- Bump Sentry .NET SDK 3.9.3 ([#328](https://github.com/getsentry/sentry-unity/pull/328))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/3.9.3/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.9.0...3.9.3)
- Bump Sentry Cocoa SDK 7.3.0 ([#328](https://github.com/getsentry/sentry-unity/pull/328))
  - [changelog](https://github.com/getsentry/sentry-cocoa/blob/7.3.0/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-cocoa/compare/7.2.2...7.3.0)
- Bump Sentry Android SDK 5.2.1 ([#359](https://github.com/getsentry/sentry-unity/pull/359))
  - [changelog](https://github.com/getsentry/sentry-java/blob/5.2.1/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-java/compare/5.2.0-beta.3...5.2.1)

## 0.5.2

### Features

- Operating System reported as raw_description and parsed by Sentry ([#305](https://github.com/getsentry/sentry-unity/pull/305))
- Release & Environment now sync with native options ([#298](https://github.com/getsentry/sentry-unity/pull/298))
- Bump Sentry .NET SDK 3.9.0 ([#299](https://github.com/getsentry/sentry-unity/pull/299))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/3.9.0/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.8.3...3.9.0)

## 0.5.1

### Fixes

- Removed editor flag checks from options validation during build process ([#295](https://github.com/getsentry/sentry-unity/pull/295))
- By default, don't block Sentry.Init up to 2 seconds to flush events ([#291](https://github.com/getsentry/sentry-unity/pull/291))

## 0.5.0

### Features

- iOS native support ([#254](https://github.com/getsentry/sentry-unity/pull/254))
- Compile Initialization with the game ([#272](https://github.com/getsentry/sentry-unity/pull/272))
- Native crash in sample ([#270](https://github.com/getsentry/sentry-unity/pull/270))
- Cache, background threads and data for UnityEventProcessor ([#268](https://github.com/getsentry/sentry-unity/pull/268))

### Fixes

- Bump Sentry Cocoa SDK 7.2.2 ([#289](https://github.com/getsentry/sentry-unity/pull/289))
  - [changelog](https://github.com/getsentry/sentry-cocoa/blob/7.2.2/CHANGELOG.md)
  - [diff](https://github.com/getsentry/sentry-cocoa/compare/7.2.0-beta.7...7.2.2)
- Included NativeExample.c in sample project ([#288](https://github.com/getsentry/sentry-unity/pull/288))
- Added attribute to SentryInitialization to prevent codestripping of Init ([#285](https://github.com/getsentry/sentry-unity/pull/285))
- Fixed passing Sentry diagnostic level to iOS native layer ([#281](https://github.com/getsentry/sentry-unity/pull/281))
- Fixed stuck traces sample rate slider ([#276](https://github.com/getsentry/sentry-unity/pull/276))
- Fixed selected input field tab glitches ([#276](https://github.com/getsentry/sentry-unity/pull/276))
- Bump Sentry .NET SDK 3.8.3 ([#269](https://github.com/getsentry/sentry-unity/pull/269))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/main/CHANGELOG.md#383)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.8.2...3.8.3)

## 0.4.3

### Features

- Log in single line ([#262](https://github.com/getsentry/sentry-unity/pull/262))

### Fixes

- Bump Sentry .NET SDK 3.8.2 ([#263](https://github.com/getsentry/sentry-unity/pull/263))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/main/CHANGELOG.md#382)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.8.1...3.8.2)

## 0.4.2

### Features

- Added IsGlobalModeEnabled to SetDefaults ([#260](https://github.com/getsentry/sentry-unity/pull/260))

## 0.4.1

### Fixes

- Bump Sentry .NET SDK 3.8.1 ([#258](https://github.com/getsentry/sentry-unity/pull/258))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/main/CHANGELOG.md#381)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.8.0...3.8.1)

## 0.4.0

### Features

- Main thread data ([#245](https://github.com/getsentry/sentry-unity/pull/245))
- New config window ([#243](https://github.com/getsentry/sentry-unity/pull/243))
- Bump Sentry .NET SDK 3.8.0 ([#255](https://github.com/getsentry/sentry-unity/pull/255))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/main/CHANGELOG.md#380)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.5.0...3.8.0)

## 0.3.3

### Fixes

- Release process cleanup

## 0.3.2

### Fixes

- Link.xml file exist check ([#248](https://github.com/getsentry/sentry-unity/pull/248))

## 0.3.1

### Fixes

- Logger now attaching again ([#244](https://github.com/getsentry/sentry-unity/pull/244))

## 0.3.0

### Features

- Sentry config is now a scriptable object ([#220](https://github.com/getsentry/sentry-unity/pull/220))
- Unity protocol ([#234](https://github.com/getsentry/sentry-unity/pull/234))
- Release health integration & Event-listener ([#225](https://github.com/getsentry/sentry-unity/pull/225))

### Fixes

- Default options values ([#241](https://github.com/getsentry/sentry-unity/pull/241))
- Un-embedding the link.xml to fix code stripping ([#237](https://github.com/getsentry/sentry-unity/pull/237))
- Setting IsEnvironmentUser to false by default ([#230](https://github.com/getsentry/sentry-unity/pull/230))

## 0.2.0

### Features

- Offline caching ([#208](https://github.com/getsentry/sentry-unity/pull/208))
- Breadcrumb categories added ([#206](https://github.com/getsentry/sentry-unity/pull/206))
- Bump Sentry .NET SDK 3.5.0 ([#218](https://github.com/getsentry/sentry-unity/pull/218))
  - [changelog](https://github.com/getsentry/sentry-dotnet/blob/main/CHANGELOG.md#350)
  - [diff](https://github.com/getsentry/sentry-dotnet/compare/3.3.4...3.5.0)

### Fixes

- Force 'Auto' from sentry window ([#219](https://github.com/getsentry/sentry-unity/pull/219))
- Hub.IsEnabled check in logging integration ([#210](https://github.com/getsentry/sentry-unity/pull/210))

## 0.1.1

### Fixes

- Fallback for missing/empty product name ([#205](https://github.com/getsentry/sentry-unity/pull/205))
- Add product name to release as default ([#202](https://github.com/getsentry/sentry-unity/pull/202))
- normalize line endings ([#204](https://github.com/getsentry/sentry-unity/pull/204))

## 0.1.0

### Features

- Simplified scene breadcrumbs messages ([#197](https://github.com/getsentry/sentry-unity/pull/197))
- Embedded link.xml in assembly ([#194](https://github.com/getsentry/sentry-unity/pull/194))
- Transition scene to test scene loading events (breadcrumbs) ([#185](https://github.com/getsentry/sentry-unity/pull/185))

### Fixes

- Check/create directory before saving ([#196](https://github.com/getsentry/sentry-unity/pull/196))
- Exclude SentryOptions.json from release package ([#195](https://github.com/getsentry/sentry-unity/pull/195))
- default env and version ([#199](https://github.com/getsentry/sentry-unity/pull/199))
- SentryEvent.ServerName forced to 'null' ([#201](https://github.com/getsentry/sentry-unity/pull/201))

## 0.0.14

### Features

- Simulator is set only when Application.isEditor is true ([#190](https://github.com/getsentry/sentry-unity/pull/190))
- Sentry UnityLogger aligned to Unity Debug API ([#163](https://github.com/getsentry/sentry-unity/pull/163))
- Scene manager integration for breadcrumbs ([#170](https://github.com/getsentry/sentry-unity/pull/170))

### Fixes

- Flag simulator based on Application.isEditor ([#184](https://github.com/getsentry/sentry-unity/pull/184))
- il2cpp remove zeroes from path ([#179](https://github.com/getsentry/sentry-unity/pull/179))
- SDK version format correction ([#120](https://github.com/getsentry/sentry-unity/pull/120))
- Auto compression option is part of drop down (no extra checkbox) ([#160](https://github.com/getsentry/sentry-unity/pull/160))
- Rename DiagnosticsLogger to DiagnosticLogger ([#168](https://github.com/getsentry/sentry-unity/pull/168))
- SentryOptions config proper check ([#176](https://github.com/getsentry/sentry-unity/pull/176))
- Diagnostic logger writes to console that it was disabled ([#183](https://github.com/getsentry/sentry-unity/pull/183))

## 0.0.13

### Fixes

- Missing meta files warnings ([#146](https://github.com/getsentry/sentry-unity/pull/146))

## 0.0.12

### Fixes

- Release process improvements

## 0.0.11

### Feature

- Craft Release

## 0.0.10

### Fixes

- Add missing meta files (cd9c7fd)

## 0.0.9

### Features

- Unity Sentry SDK programmatic setup ([#130](https://github.com/getsentry/sentry-unity/pull/130))
  - SentryWindow updated

### Fixes

- UPM meta updated ([#124](https://github.com/getsentry/sentry-unity/pull/124))
- Bump dotnet 3.3.4 ([#132](https://github.com/getsentry/sentry-unity/pull/132))
  - https://github.com/getsentry/sentry-dotnet/blob/main/CHANGELOG.md#334
  - Bug fixes for performance monitoring
  - Ability to keep failed envelopes for troubleshooting when they are too large

## 0.0.8

### Fixes

- Bump sentry-dotnet to 8ca0906 - fix IL2CPP crash ([#122](https://github.com/getsentry/sentry-unity/pull/122))
- IL2CPP players crash due to startup time detection ([#123](https://github.com/getsentry/sentry-unity/pull/123))

## 0.0.7

### Features

- Strip zeroes for ill2cpp builds ([#108](https://github.com/getsentry/sentry-unity/pull/108))
- Proper sdk name reporting for sentry event ([#111](https://github.com/getsentry/sentry-unity/pull/111))
- Bump .NET SDK to 3.3.1 ([#115](https://github.com/getsentry/sentry-unity/pull/115))
- Release package samples ([#113](https://github.com/getsentry/sentry-unity/pull/113))

## 0.0.6

### First release through UPM

- .NET SDK setup for Unity with Unity editor configuration.
