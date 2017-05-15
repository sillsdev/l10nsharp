## Overview

L10NSharp is a .NET localization library for Windows Forms applications. It collects strings which needs localization when your application first runs and saves them in a Translation Memory eXchange (TMX) file. It can also dynamically collect strings at runtime.

## L10NSharpExtender

To localize a Windows Forms form or control, simply add the L10NSharpExtender. It will automatically collect all the localizable strings on your form or control and its children.

## Localizing Within the Application

L10NSharp provides a dialog for translating terms while running the application. The dialog can be launched by Alt-Shift-clicking a Windows Forms element.

## Building

Just download the repository and build the solution (L10NSharp.sln).

For Linux, the build command would look something like this:

    /opt/mono4-sil/bin/xbuild build/L10NSharp.proj /t:Build /p:Configuration=Debug

Note that building L10NSharp requires at least version 4.6 of Mono, which mono4-sil provides.

## Running Unit Tests

We use NUnit to run our unit tests. NUnit is downloaded via NuGet.  There may be a few tests that do not run, but all tests that run should pass.

For Linux, the command to run the tests would look something like this:

    /opt/mono4-sil/bin/mono packages/NUnit.Runners.Net4.2.6.4/tools/nunit-console.exe output/Debug/L10NSharpTests.dll

It is also possible to run the tests from inside MonoDevelop, at least if mono4-sil is installed and made the default Mono runtime.
