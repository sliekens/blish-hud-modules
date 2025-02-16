using System.Globalization;

using Blish_HUD;

using GuildWars2;

using Gw2Sharp.WebApi;

using SL.Common;

namespace SL.Adapters;

public sealed class OverlayLocale : ILocale, IDisposable
{
    private readonly IEventAggregator _eventAggregator;

    public OverlayLocale(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        Current = GameService.Overlay.UserLocale.Value switch
        {
            Locale.German => Language.German,
            Locale.Spanish => Language.Spanish,
            Locale.French => Language.French,
            _ => Language.English
        };

        GameService.Overlay.UserLocaleChanged += OnUserLocaleChanged;
    }

    private void OnUserLocaleChanged(object sender, ValueEventArgs<CultureInfo> args)
    {
        Language language = args.Value switch
        {
            { TwoLetterISOLanguageName: "de" } => Language.German,
            { TwoLetterISOLanguageName: "es" } => Language.Spanish,
            { TwoLetterISOLanguageName: "fr" } => Language.French,
            _ => Language.English
        };

        Current = language;
        _eventAggregator.Publish(new LocaleChanged(language));
    }

    public Language Current { get; private set; }

    public void Dispose()
    {
        GameService.Overlay.UserLocaleChanged -= OnUserLocaleChanged;
    }
}
