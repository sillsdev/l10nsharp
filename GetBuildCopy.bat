hg pull -u
REM %comspec% /k ""C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"" x86
call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"
msbuild "Localization.sln"
call "copyto bloom".bat
call "copyto saymore".bat
