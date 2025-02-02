using GuildWars2;

namespace SL.ChatLinks.Storage;

public class DatabaseOptions
{
    public string Directory { get; set; } = System.IO.Directory.GetCurrentDirectory();

    public string? RefData { get; set; }

    public string DatabaseFileName(Language language) => language switch
    {
        { Alpha2Code: "de" } => "data_de.db",
        { Alpha2Code: "es" } => "data_es.db",
        { Alpha2Code: "fr" } => "data_fr.db",
        _ => "data.db"
    };

    public string ConnectionString(Language language) => $"Data Source={Path.Combine(Directory, DatabaseFileName(language))}";
}