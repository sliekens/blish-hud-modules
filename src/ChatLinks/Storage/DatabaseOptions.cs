using System.Globalization;

namespace SL.ChatLinks.Storage;

public class DatabaseOptions
{
    public string Directory { get; set; } = System.IO.Directory.GetCurrentDirectory();

    public string DatabaseFileName(CultureInfo culture) => culture switch
    {
        //{ TwoLetterISOLanguageName: "de" } => "data_de.db",
        //{ TwoLetterISOLanguageName: "es" } => "data_es.db",
        //{ TwoLetterISOLanguageName: "fr" } => "data_fr.db",
        _ => "data.db"
    };

    public string ConnectionString(CultureInfo culture) => $"Data Source={Path.Combine(Directory, DatabaseFileName(culture))}";
}