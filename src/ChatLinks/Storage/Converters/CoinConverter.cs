using GuildWars2;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SL.ChatLinks.Storage.Converters;

public class CoinConverter() : ValueConverter<Coin, int>(
    static coin => coin.Amount,
    static value => new Coin(value)
);
