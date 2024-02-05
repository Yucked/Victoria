using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Victoria.Converters;

internal sealed class LavaTrackListConverter : JsonConverter<IReadOnlyCollection<LavaTrack>>
{
    public override IReadOnlyCollection<LavaTrack> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        List<LavaTrack> trackList = new List<LavaTrack>();

        string hash = null,
               id = null,
               title = null,
               author = null,
               url = default,
               source = default,
               artworkUrl = default,
               isrc = default;

        long length = default,
             position = default;

        bool isSeekable = false,
             isLiveStream = false;

        while (reader.Read())
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                if (reader.TokenType == JsonTokenType.EndObject && hash != null)
                {
                    trackList.Add(new LavaTrack
                    {
                        Hash = hash,
                        Id = id,
                        Title = title,
                        Author = author,
                        Url = url,
                        Artwork = artworkUrl,
                        ISRC = isrc,
                        SourceName = source,
                        IsSeekable = isSeekable,
                        IsLiveStream = isLiveStream,
                        Position = TimeSpan.FromMilliseconds(position),
                        Duration = TimeSpan.FromMilliseconds(length)
                    });

                    hash = null;
                    id = null;
                    title = null;
                    author = null;
                    url = default;
                    source = default;
                    artworkUrl = default;
                    isrc = default;

                    length = default;
                    position = default;

                    isSeekable = false;
                    isLiveStream = false;
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
                url = reader.GetString();
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
        }

        return new ReadOnlyCollection<LavaTrack>(trackList);
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyCollection<LavaTrack> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    private bool IsPropertyAndRead(ref Utf8JsonReader reader, string text)
    {
        return reader.ValueTextEquals(text) && reader.Read();
    }
}