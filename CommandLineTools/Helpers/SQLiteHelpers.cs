using System.Data.SQLite;

namespace CommandLineTools.Helpers
{
    public static class SQLiteHelpers
    {
        public static SQLiteConnection CreateConnection(string filename)
        {
            return new SQLiteConnection($"Data Source={filename}");
        }
    }
}