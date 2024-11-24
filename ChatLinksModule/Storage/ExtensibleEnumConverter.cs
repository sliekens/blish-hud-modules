using System;

using GuildWars2;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChatLinksModule.Storage;

public class ExtensibleEnumConverter<TEnum>() : ValueConverter<Extensible<TEnum>, string>(
	static value => value.ToString(),
	static value => new Extensible<TEnum>(value)
) where TEnum : struct, Enum;