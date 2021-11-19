using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Victoria.Player;

namespace Victoria.Converters {
    /// <summary>
    /// 
    /// </summary>
    internal sealed class LavaTracksPropertyConverter : JsonConverter<IReadOnlyCollection<LavaTrack>> {
        /// <inheritdoc />
        public override IReadOnlyCollection<LavaTrack> Read(ref Utf8JsonReader reader, Type typeToConvert,
                                                            JsonSerializerOptions options) {
            var lavaTracks = new HashSet<LavaTrack>();
            while (reader.Read()) {
                if (reader.TokenType == JsonTokenType.EndArray) {
                    break;
                }

                if (reader.TokenType != JsonTokenType.StartObject) {
                    continue;
                }

                string hash = null,
                       id = null,
                       title = null,
                       author = null,
                       url = default,
                       source = default;

                long duration = default;
                bool isSeekable = false,
                     isLiveStream = false;

                while (reader.Read()) {
                    if (reader.TokenType == JsonTokenType.EndObject) {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName
                        && reader.ValueTextEquals("track")
                        && reader.Read()) {
                        hash = reader.GetString();
                    }

                    if (reader.TokenType != JsonTokenType.StartObject) {
                        continue;
                    }

                    while (reader.Read()) {
                        if (reader.TokenType == JsonTokenType.EndObject) {
                            break;
                        }

                        if (reader.TokenType != JsonTokenType.PropertyName) {
                            continue;
                        }

                        if (reader.ValueTextEquals("identifier") && reader.Read()) {
                            id = reader.GetString();
                        }
                        else if (reader.ValueTextEquals("isSeekable") && reader.Read()) {
                            isSeekable = reader.GetBoolean();
                        }
                        else if (reader.ValueTextEquals("author") && reader.Read()) {
                            author = reader.GetString();
                        }
                        else if (reader.ValueTextEquals("length") && reader.Read()) {
                            duration = reader.GetInt64();
                        }
                        else if (reader.ValueTextEquals("isStream") && reader.Read()) {
                            isLiveStream = reader.GetBoolean();
                        }
                        else if (reader.ValueTextEquals("title") && reader.Read()) {
                            title = reader.GetString();
                        }
                        else if (reader.ValueTextEquals("uri") && reader.Read()) {
                            url = reader.GetString();
                        }
                        else if (reader.ValueTextEquals("sourceName") && reader.Read()) {
                            source = reader.GetString();
                        }
                    }
                }

                var lavaTrack = new LavaTrack(
                    hash,
                    id,
                    title,
                    author,
                    url,
                    default,
                    duration,
                    isSeekable,
                    isLiveStream,
                    source
                );

                lavaTracks.Add(lavaTrack);
            }

            return lavaTracks;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, IReadOnlyCollection<LavaTrack> value,
                                   JsonSerializerOptions options) {
            throw new NotImplementedException();
        }
    }
}