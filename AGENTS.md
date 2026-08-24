# Agent notes for L10nSharp

## Versioning

This repo versions with GitVersion, following [SemVer](https://semver.org/). Patch is the
default increment and needs no marker. A commit that adds a backward-compatible feature
must include `+semver:minor` in its commit message, and a breaking change `+semver:major`
(no space after the colon; anywhere in the message). When squashing a branch, make sure
the surviving commit keeps the marker.
