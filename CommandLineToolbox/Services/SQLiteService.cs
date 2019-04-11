using CommandLineTools.Contracts;
using System.Data.SQLite;

namespace CommandLineToolbox.Services
{
    public class SQLiteService : ISQLiteService
    {
        public void Get()
        {
            string dataSource = "small_sorters_result.sqlite";

            var connection = new SQLiteConnection($"Data Source={dataSource}");
            connection.Open();

            var comm = new SQLiteCommand(connection);
        }
    }
}