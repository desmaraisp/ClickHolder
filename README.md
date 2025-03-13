# ClickHolder
 
My laptop's touchpad buttons no longer register clicks, meaning that I'm no longer able to click and drag. This AvaloniaUI program hooks into the win32 global shortcuts to register mouse down/up event on ATL+K, allowing me to click and drag with the keyboard. Plus I used advanced windows gestures to call that shortcut with 4-finger clicks to make the whole thing a little more seamless.

Why not autoHotkey, you ask? Because I don't like the extra dependency on my system. Using a quick app made more sense to me ¯\\\_(ツ)_/¯

## Installing
No dependencies or installation required, this is NativeAoT-compiled. Just download from the release.

## Building
Publishing requires the VS c++ workload. Publish using the following CLI command:

`dotnet publish .\src\ClickHolder\ClickHolder.csproj -r win-x64 /p:VersionPrefix={VersionNumber}`
