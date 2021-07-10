# leetbot_night
[![Linux](https://github.com/thefullarcticfox/leetbot_night/actions/workflows/dotnet-linux.yml/badge.svg)](https://github.com/thefullarcticfox/leetbot_night/actions/workflows/dotnet-linux.yml)
[![macOS](https://github.com/thefullarcticfox/leetbot_night/actions/workflows/dotnet-macos.yml/badge.svg)](https://github.com/thefullarcticfox/leetbot_night/actions/workflows/dotnet-macos.yml)
[![Windows](https://github.com/thefullarcticfox/leetbot_night/actions/workflows/dotnet-windows.yml/badge.svg)](https://github.com/thefullarcticfox/leetbot_night/actions/workflows/dotnet-windows.yml)

this is just my little discord bot which served as a playground with async tasks, cryptography (AES/RSA) and other C#/.NET features back in 2019-2020 when i developed it

### uses
* .Net 5.0
* DSharpPlus - unofficial .NET wrapper for the Discord API
* TwitchLib - to monitor twitch channels live status
* Youtube API v3 - to monitor youtube playlist updates

### run requirements
- .Net 5.0 Runtime
- opus and sodium dlls + ffmpeg
> the second one is required only for VoiceNext functionality (that's not actually working though)

### build requirements
- Visual Studio 2019 with .Net Core cross-platform development module
- .Net 5.0 SDK
> also: some nerves to get through my bad code

> this project also builds on linux and macos
