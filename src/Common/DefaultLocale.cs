using System.Globalization;

using GuildWars2;

namespace SL.Common;

public sealed class DefaultLocale : ILocale
{
    public Language Current => CultureInfo.CurrentUICulture switch
    {
        { TwoLetterISOLanguageName: "en" } => Language.English,
        { TwoLetterISOLanguageName: "de" } => Language.German,
        { TwoLetterISOLanguageName: "es" } => Language.Spanish,
        { TwoLetterISOLanguageName: "fr" } => Language.French,
        _ => Language.English
    };
}
