## Overview

L10NSharp is a .NET localization library for Windows Forms applications. It collects strings which needs localization when your application first runs and saves them in a Translation Memory eXchange (TMX) file. It can also dynamically collect strings at runtime.

## L10NSharpExtender

To localize a Windows Forms form or control, simply add the L10NSharpExtender. It will automatically collect all the localizable strings on your form or control and its children.

## Localizing Within the Application

L10NSharp provides a dialog for translating terms while running the application. The dialog can be launched by Alt-Shift-clicking a Windows Forms element.

## Building

Just download the repository and build the solution (L10NSharp.sln).

## Running Unit Tests

We use NUnit to run our unit tests. NUnit is downloaded via NuGet.
