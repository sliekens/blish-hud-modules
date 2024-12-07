using GuildWars2;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SL.ChatLinks.Storage;

public class CoinConverter() : ValueConverter<Coin, int>(
    static coin => coin.Amount,
    static value => new Coin(value)
);
