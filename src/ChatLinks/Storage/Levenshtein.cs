using Microsoft.Data.Sqlite;

namespace SL.ChatLinks.Storage;

public class Levenshtein
{
    public static void RegisterLevenshteinFunction(SqliteConnection connection)
    {
        connection.CreateFunction(
            "LevenshteinDistance",
            (string s, string t) => LevenshteinDistance(s, t)
        );
    }

    private static int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a))
        {
            return string.IsNullOrEmpty(b) ? 0 : b.Length;
        }

        if (string.IsNullOrEmpty(b))
        {
            return a.Length;
        }

        int[,] costs = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++)
        {
            costs[i, 0] = i;
        }

        for (int j = 0; j <= b.Length; j++)
        {
            costs[0, j] = j;
        }

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                costs[i, j] = Math.Min(Math.Min(costs[i - 1, j] + 1, costs[i, j - 1] + 1), costs[i - 1, j - 1] + cost);
            }
        }

        return costs[a.Length, b.Length];
    }
}