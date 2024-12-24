using GuildWars2;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SL.ChatLinks.Storage.Converters;

public class ExtensibleEnumConverter<TEnum>() : ValueConverter<Extensible<TEnum>, string>(
    static value => value.ToString(),
    static value => new Extensible<TEnum>(value)
) where TEnum : struct, Enum;
