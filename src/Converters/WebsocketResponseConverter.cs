using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.WebSocket;
using PlayerState = Victoria.Responses.WebSocket.PlayerState;

namespace Victoria.Converters {
	internal sealed class WebsocketResponseConverter : JsonConverter<BaseWsResponse> {
		/// <inheritdoc />
		public override BaseWsResponse Read(ref Utf8JsonReader reader, Type typeToConvert,
			JsonSerializerOptions options) {
			if (reader.TokenType != JsonTokenType.StartObject) {
				throw new JsonException();
			}

			var response = new BaseWsResponse();
			while (reader.Read()) {
				if (reader.TokenType == JsonTokenType.EndObject) {
					break;
				}

				if (reader.TokenType != JsonTokenType.PropertyName) {
					continue;
				}

				var index = reader.ValueSpan[0];
				if (index == 111) {
					reader.Read();

					if (reader.ValueTextEquals("playerUpdate")) {
						ProcessPlayerUpdate(ref reader, out var playerUpdateResponse);
						response = playerUpdateResponse;
					}
					else if (reader.ValueTextEquals("event")) {
						ProcessEvent(ref reader, out var eventResponse);
						response = eventResponse;
					}
					else {
						throw new JsonException("Unhandled OP.");
					}
				}
				else if (index == 112) {
					ProcessStats(ref reader, out var statsResponse);
					response = statsResponse;
				}
				else {
					throw new JsonException("Index was out of range. Only handled indexes are: 111, 112.");
				}
			}

			return response;
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, BaseWsResponse value, JsonSerializerOptions options) {
			throw new NotImplementedException("This method can't be used for writing.'");
		}

		private static void ProcessPlayerUpdate(ref Utf8JsonReader reader, out PlayerUpdateResponse playerUpdateResponse) {
			//    {"op":"playerUpdate","state":{"position":4720,"time":1566866929606},"guildId":"522440206494728203"}

			playerUpdateResponse = new PlayerUpdateResponse();
			while (reader.Read()) {
				switch (reader.TokenType) {
					case JsonTokenType.PropertyName when reader.ValueTextEquals("guildId"):
						reader.Read();
						playerUpdateResponse.GuildId = ulong.Parse(reader.GetString());
						break;

					case JsonTokenType.PropertyName when reader.ValueTextEquals("state"):
						var state = new PlayerState();

						while (reader.Read()) {
							if (reader.TokenType == JsonTokenType.EndObject) {
								playerUpdateResponse.State = state;
								break;
							}

							if (reader.TokenType != JsonTokenType.PropertyName) {
								continue;
							}

							if (reader.ValueTextEquals("time") && reader.Read()) {
								state.Time = DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64());
							}
							else if (reader.ValueTextEquals("position") && reader.Read()) {
								state.Position = TimeSpan.FromMilliseconds(reader.GetInt64());
							}
						}

						break;
				}
			}
		}

		private static void ProcessStats(ref Utf8JsonReader reader, out StatsResponse statsResponse) {
			//    {"playingPlayers":0,"op":"stats",
			//    "memory":{"reservable":1234567890,"used":1234567890,"free":1234567890,"allocated":1234567890},
			//    "players":0,"cpu":{"cores":4,"systemLoad":0,"lavalinkLoad":0.27354256456787324},"uptime":33731}

			statsResponse = new StatsResponse();

			if (reader.ValueTextEquals("playingPlayers") && reader.Read()) {
				statsResponse.Players = reader.GetInt32();
			}

			while (reader.Read()) {
				if (reader.TokenType == JsonTokenType.EndObject) {
					break;
				}

				if (reader.TokenType != JsonTokenType.PropertyName) {
					continue;
				}

				// MEMORY OBJECT
				if (reader.ValueTextEquals("memory") && reader.Read()) {
					var memory = new Memory();
					while (reader.Read()) {
						if (reader.TokenType == JsonTokenType.EndObject) {
							break;
						}

						if (reader.ValueTextEquals("free") && reader.Read()) {
							memory.Free = reader.GetInt64();
						}
						else if (reader.ValueTextEquals("used") && reader.Read()) {
							memory.Used = reader.GetInt64();
						}
						else if (reader.ValueTextEquals("allocated") && reader.Read()) {
							memory.Allocated = reader.GetInt64();
						}
						else if (reader.ValueTextEquals("reservable") && reader.Read()) {
							memory.Reservable = reader.GetInt64();
						}
					}

					statsResponse.Memory = memory;
				}
				else if (reader.ValueTextEquals("players") && reader.Read()) {
					statsResponse.Players = reader.GetInt32();
				}

				// CPU RESPOSNE
				else if (reader.ValueTextEquals("cpu") && reader.Read()) {
					var cpu = new Cpu();
					while (reader.Read()) {
						if (reader.TokenType == JsonTokenType.EndObject) {
							break;
						}

						if (reader.ValueTextEquals("cores") && reader.Read()) {
							cpu.Cores = reader.GetInt32();
						}
						else if (reader.ValueTextEquals("systemLoad") && reader.Read()) {
							cpu.SystemLoad = reader.GetDouble();
						}
						else if (reader.ValueTextEquals("lavalinkLoad") && reader.Read()) {
							cpu.LavalinkLoad = reader.GetDouble();
						}
					}

					statsResponse.Cpu = cpu;
				}

				else if (reader.ValueTextEquals("uptime") && reader.Read()) {
					statsResponse.Uptime = reader.GetInt64();
				}

				// FRAMES (MIGHT THROW EXCEPTION)
				else if (reader.ValueTextEquals("frames")) {
					var frames = new Frames();

					while (reader.Read()) {
						if (reader.TokenType == JsonTokenType.EndObject) {
							break;
						}

						if (reader.ValueTextEquals("sent") && reader.Read()) {
							frames.Sent = reader.GetInt32();
						}
						else if (reader.ValueTextEquals("nulled") && reader.Read()) {
							frames.Nulled = reader.GetInt32();
						}
						else if (reader.ValueTextEquals("deficit") && reader.Read()) {
							frames.Deficit = reader.GetInt32();
						}
					}

					statsResponse.Frames = frames;
				}
			}
		}

		private static void ProcessEvent(ref Utf8JsonReader reader, out BaseEventResponse eventResponse) {
			//{"op":"event","reason":"FINISHED","type":"TrackEndEvent","track":"QAAAcwIADUxhdGUgRm9y...","guildId":"522440206494728203"}

			var dictionary = new Dictionary<string, string>();

			while (reader.Read()) {
				if (reader.TokenType == JsonTokenType.EndObject) {
					break;
				}

				if (reader.TokenType != JsonTokenType.PropertyName) {
					continue;
				}

				var propName = reader.ValueSpan;
				reader.Read();
				var propValue = reader.ValueSpan;

				dictionary.Add(Encoding.UTF8.GetString(propName), Encoding.UTF8.GetString(propValue));
			}

			if (!dictionary.TryGetValue("type", out var type)) {
				eventResponse = default;
				return;
			}

			switch (type) {
				case "TrackStartEvent":
					eventResponse = new TrackStartEvent {
						GuildId = ulong.Parse(dictionary["guildId"]),
						Hash = dictionary["track"]
					};
					break;

				case "TrackEndEvent":
					eventResponse = new TrackEndEvent {
						GuildId = ulong.Parse(dictionary["guildId"]),
						Reason = (TrackEndReason) dictionary["reason"][0],
						Hash = dictionary["track"]
					};
					break;

				case "TrackExceptionEvent":
					eventResponse = new TrackExceptionEvent {
						GuildId = ulong.Parse(dictionary["guildId"]),
						Hash = dictionary["track"],
						Error = dictionary["error"]
					};
					break;

				case "TrackStuckEvent":
					eventResponse = new TrackStuckEvent {
						GuildId = ulong.Parse(dictionary["guildId"]),
						Hash = dictionary["track"],
						ThresholdMs = long.Parse(dictionary["thresholdMs"])
					};
					break;

				case "WebSocketClosedEvent":
					eventResponse = new WebSocketClosedEvent {
						GuildId = ulong.Parse(dictionary["guildId"]),
						Code = int.Parse(dictionary["code"]),
						Reason = dictionary["reason"],
						ByRemote = bool.Parse(dictionary["byRemote"])
					};
					break;

				default:
					eventResponse = default;
					break;
			}
		}
	}
}