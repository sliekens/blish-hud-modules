using GuildWars2;
using GuildWars2.Collections;
using GuildWars2.Items;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Tests;

public class JsonValueConverterTest
{
    [Fact]
    public void ConvertToProvider_converts_value_to_json()
    {
        IImmutableValueList<Extensible<GameType>> input = new ImmutableValueList<Extensible<GameType>>([GameType.Activity]);
        JsonValueConverter<IImmutableValueList<Extensible<GameType>>> sut = new();

        string? actual = (string?)sut.ConvertToProvider(input);

        Assert.NotNull(actual);
        Assert.Equal("[\"Activity\"]", actual);
    }

    [Fact]
    public void ConvertFromProvider_converts_json_to_value()
    {
        string json = "[\"Activity\"]";
        JsonValueConverter<IImmutableValueList<Extensible<GameType>>> sut = new();

        IImmutableValueList<Extensible<GameType>>? actual = (IImmutableValueList<Extensible<GameType>>?)sut.ConvertFromProvider(json);

        Assert.NotNull(actual);
        Extensible<GameType> only = Assert.Single(actual);
        Assert.Equal(GameType.Activity, only);
    }

    [Fact]
    public void Roundtrip_preserves_value()
    {
        IImmutableValueList<Extensible<GameType>> input = new ImmutableValueList<Extensible<GameType>>([GameType.Activity, GameType.Pvp]);
        JsonValueConverter<IImmutableValueList<Extensible<GameType>>> sut = new();

        string? json = (string?)sut.ConvertToProvider(input);
        IImmutableValueList<Extensible<GameType>>? actual = (IImmutableValueList<Extensible<GameType>>?)sut.ConvertFromProvider(json);

        Assert.NotNull(actual);
        Assert.Equal(input.Count, actual.Count);
        for (int i = 0; i < input.Count; i++)
        {
            Assert.Equal(input[i], actual[i]);
        }
    }
}
