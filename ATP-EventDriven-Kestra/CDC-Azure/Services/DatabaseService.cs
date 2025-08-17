using System.Data.SqlClient;
using CDC_Azure.Models;
using CDC_Azure.Config;

namespace CDC_Azure.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = SqlConfig.ConnectionString
                                 ?? throw new InvalidOperationException("SQL Connection string is not set in environment variables.");
        }

        public async Task InsertOrUpdateMstOrder(mstOrder order)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                MERGE INTO tempMstOrder AS Target
                USING (SELECT @Id AS Id) AS Source
                ON Target.Id = Source.Id
                WHEN MATCHED THEN 
                    UPDATE SET OrderNumber = @OrderNumber, LastName = @LastName, Email = @Email
                WHEN NOT MATCHED THEN
                    INSERT (Id, OrderNumber, LastName, Email) VALUES (@Id, @OrderNumber, @LastName, @Email);";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", order.ID);
            command.Parameters.AddWithValue("@OrderNumber", order.SONumber);

            await command.ExecuteNonQueryAsync();
        }
    }
}
