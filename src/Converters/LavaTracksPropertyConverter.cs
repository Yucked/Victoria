using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Victoria.Interfaces;

namespace Victoria.Converters {
    /// <summary>
    /// 
    /// </summary>
    public class LavaTracksPropertyConverter : JsonConverter<IReadOnlyCollection<ILavaTrack>> {
        /// <inheritdoc />
        public override IReadOnlyCollection<ILavaTrack> Read(ref Utf8JsonReader reader, Type typeToConvert,
                                                             JsonSerializerOptions options) {
            var lavaTracks = new HashSet<ILavaTrack>();
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
                       url = default;

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
                    }
                }

                var lavaTrack = new AbstractLavaTrack(
                    hash,
                    id,
                    title,
                    author,
                    url,
                    default,
                    duration < TimeSpan.MaxValue.Ticks
                        ? TimeSpan.FromMilliseconds(duration)
                        : TimeSpan.MaxValue,
                    isSeekable,
                    isLiveStream
                );

                lavaTracks.Add(lavaTrack);
            }

            return lavaTracks;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, IReadOnlyCollection<ILavaTrack> value,
                                   JsonSerializerOptions options) {
            throw new NotImplementedException();
        }
    }
}