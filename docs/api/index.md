# Changelog

## `v6.x`

---

## `v5.x`
Version 5.x is a mix of v4 and v3. v4 was mostly flawed mainly because it separated Lavalink's WebSocket and Rest into 2 different classes known as `LavaSocketClient`/`LavaShardClient` and `LavaRestClient`. Overall this update focuses on performance and easier code readability.

### `Additions`
- Added `LavaNode` which represents a single Lavalink connection. From v4 -> v5: `LavaNode` is `LavaBaseClient` + `LavaRestClient`.
- Added exceptions. Instead of keeping everything silent like v4. v5 throws exception is something isn't set to make it easier to debug and etc.
- Added `PlayerState` in `LavaPlayer` which represents player current state e.g, `Playing`, `Paused`, `Stopped`, etc.
- Added event args for events. You will get a proper event object when an event is triggered instead of `Func<int, string, Task>`.
- Added `LavaNode#HasPlayer`.
- Added JsonConverter for `SearchResponse` and WebSocket events.

### `Removals`
- Removed `LavaBaseClient`, `LavaSocketClient`, `LavaShardClient` and `LavaRestClient`.
- Removed Newtonsoft.Json.
- Removed `SocketHelper`.
- Removed `HttpHelper`.
- Removed `LyricsHelper`.
- Removed extensions related to fetching lyrics and thumbnails.

### `Modifications`

---

## `v4.x`

### `Additions`
- Added `LavaBaseClient` that acts as a base class for `LavaSocketClient` and `LavaShardClient`. `LavaBaseClient` implements all the methods and features that are needed for socket and shard client.
- Added `LavaSocketClient` which supports `DiscordSocketClient`.
- Added `LavaShardClient` which supports `DiscordShardedClient`.
- Added `VictoriaExtensions`, exposes extension methods for `LavaTrack`.
- Added `netstandard2.0` as a TargetFramework along with `netcoreapp3.0`
- Added `IQueueObject` for `LavaQueue`. You can now use `Queue` in `LavaPlayer` for more than tracks.
- Added `event`keyword with `Func<T>`.
- Added `LavaRestClient` that handles all Lavalink REST features.
- VictoriaExtensions now contains a method for figuring out if next track should be played
- Added `LavaBaseClient#GetPlayer` method.
- Created an extensions method of `Func<LogMessage, Task>` and replaced all logging statements with the extension method.
- Added locks for `LavaQueue` methods.
- Re-added `LavaBaseClient#DisconnectAsync`.
~~- Added existing argument for ConnectAsync which tries to restart websocket connection if closed and updates player.~~
~~- Added `ShouldPreservePlayers` incase Lavalink dies.~~
- Added `PlayerState` struct to make JSON deserialization faster.
- Added documentation for Configuration properties.
- Added `LavaBaseClient#MoveChannelsAsync(IVoiceChannel)` to move voice channels.
- Added `UpdateTextChannel(ulong, ITextChannel)` that changes player's text channel.
- Added more configuration options such as: `AutoDisconnect`, `PreservePlayers`, `InactivityTimeout`.
- Added `Provider` property for `LavaTrack` object.

### `Removals`
- Removed `LavaNode`, `LavaBaseClient` now acts as a `LavaNode`.
- Removed `Lavalink`, there is no purpose of `Lavalink`.
- Removed `LavaState`, no purpose.
- Removed `TrackInfo`.
- Removed redundant cast for `BufferSize`.
- Removed bloat from `StartAsync`.
- Removed `ShouldPreservePlayers` property from Configuration.
- Removed residue of `ShouldPresevePlayers` and all logic related to it.

### `Modifications`
- Fixed `HTTPHelper#WithCustomHeaders` existing header exception.
- Skip method now replaces tracks and returns the old track.
- `LavaPlayer#Dispose` properly disposes and destroys player.
- `LavaQueue#TryDequeue` first checks if there are any elements.
- Modified `LavaBaseClient#GetTracksAsync` method so it properly builds `SearchResult`.
- Changed namespace for Entities.XXX to Entities.
- Fixed AccessViolation/StackOverFlowExceptions.
- Fixed track sting being null.
- Fixed `guildId` being `ulong`.
- Changed CTOR of `LavaRestClient`. All args are now required.
- Fixed configuration related bug
- Set `CurrentVolume` to 100 in `LavaPlayer` CTOR.
- Moved `SocketVoiceState` from `LavaBaseClient` to `LavaPlayer`.
- FinishedEvent now sets `IsPlaying` to false.
- Fixed reconnect attempts bug in `SocketHelper`.
- Moved classes to Responses folder.
- Specified default values for Configuration properties.
- Changed `LavaRestClient#TracksRequestAsync` to `LavaRestClient#SearchTracksAsync`.
- Changed setter modifier of VoiceChannel and TextChannel to internal.
- `LavaShardClient` and `LavaSocketClient` now check `PreservePlayers` before clearing up players in disconnect event.
- Made configuration field protected.
- `OnUserVoiceStateUpdated` now handles auto disconnecting if there are no users in voice channel.
- Made it sweeter (using `??=` to fix configuration issue, see #35).