using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Victoria.Converters;

internal sealed class LavaTrackConverter : JsonConverter<LavaTrack> {
    public override LavaTrack Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        string hash = null,
               id = null,
               title = null,
               author = null,
               uri = default,
               source = default,
               artworkUrl = default,
               isrc = default;

        long length = default,
             position = default;

        bool isSeekable = false,
             isLiveStream = false;

        try
        {
            var isNearingEndOfPayload = false;

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    if (isNearingEndOfPayload)
                    {
                        if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == 1)
                        {
                            break;
                        }
                    }

                    continue;
                }

                if (IsPropertyAndRead(ref reader, "encoded"))
                {
                    hash = reader.GetString();
                }

                else if (IsPropertyAndRead(ref reader, "identifier"))
                {
                    id = reader.GetString();
                }

                else if (IsPropertyAndRead(ref reader, "title"))
                {
                    title = reader.GetString();
                }

                else if (IsPropertyAndRead(ref reader, "author"))
                {
                    author = reader.GetString();
                }

                else if (IsPropertyAndRead(ref reader, "uri"))
                {
                    uri = reader.GetString();
                }

                else if (IsPropertyAndRead(ref reader, "sourceName"))
                {
                    source = reader.GetString();
                }

                else if (IsPropertyAndRead(ref reader, "artworkUrl"))
                {
                    artworkUrl = reader.GetString();
                }

                else if (IsPropertyAndRead(ref reader, "isrc"))
                {
                    isrc = reader.GetString();
                }

                else if (IsPropertyAndRead(ref reader, "isSeekable"))
                {
                    isSeekable = reader.GetBoolean();
                }

                else if (IsPropertyAndRead(ref reader, "isStream"))
                {
                    isLiveStream = reader.GetBoolean();
                }

                else if (IsPropertyAndRead(ref reader, "length"))
                {
                    length = reader.GetInt64();
                }

                else if (IsPropertyAndRead(ref reader, "position"))
                {
                    position = reader.GetInt64();
                }

                else if (IsPropertyAndRead(ref reader, "filters"))
                {
                    isNearingEndOfPayload = true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        var lavaTrack = new LavaTrack {
            Hash = hash,
            Id = id,
            Title = title,
            Author = author,
            Uri = uri,
            Artwork = artworkUrl,
            ISRC = isrc,
            SourceName = source,
            IsSeekable = isSeekable,
            IsLiveStream = isLiveStream,
            Position = TimeSpan.FromMilliseconds(position),
            Duration = TimeSpan.FromMilliseconds(length)
        };

        return lavaTrack;
    }

    public override void Write(Utf8JsonWriter writer, LavaTrack value, JsonSerializerOptions options) {
        throw new NotImplementedException();
    }

    private bool IsPropertyAndRead(ref Utf8JsonReader reader, string text) {
        return reader.ValueTextEquals(text) && reader.Read();
    }
}