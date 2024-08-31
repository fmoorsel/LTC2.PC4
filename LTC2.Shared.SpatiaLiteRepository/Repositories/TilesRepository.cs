using Microsoft.Data.Sqlite;
using System.IO;

namespace LTC2.Shared.SpatiaLiteRepository.Repositories
{
    public class TilesRepository : AbstractSqliteRepository
    {
        private readonly string _selectTile = @"
            SELECT 
                tilePbf
            FROM 
                Tiles
            WHERE
                tileX = @tileX
            AND
                tileY = @tileY
            AND
                tileZ = @tileZ            
            LIMIT 1;
        ";

        public TilesRepository() : base()
        {
            DbConnectionString = $"Data Source=.\\Resources\\tiles.sqlite";
        }

        public override void Open()
        {
        }

        public Stream GetTileStream(int x, int y, int z)
        {
            using (var connection = new SqliteConnection(DbConnectionString))
            {
                connection.Open();

                using (var sqlCommand = new SqliteCommand(_selectTile, connection))
                {
                    sqlCommand.Parameters.AddWithValue("@tileX", x);
                    sqlCommand.Parameters.AddWithValue("@tileY", y);
                    sqlCommand.Parameters.AddWithValue("@tileZ", z);

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetStream(0);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
    }
}
