using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CDC_Azure.Config;
using CDC_Azure.Helpers;
using CDC_Azure.Models;

namespace CDC_Azure.Services
{
    public class OrderService
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public OrderService(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task CreateOrder(string execId, CancellationToken token)
        {
            var url = $"{KestraConfig.Host}{KestraConfig.UrlAPI}{execId}";

            while (!token.IsCancellationRequested)
            {
                var response = await KestraHttpClient.Client.GetAsync(url, token);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to monitor: {response.StatusCode} - {json}");
                    break;
                }

                var doc = JsonDocument.Parse(json);
                var state = doc.RootElement.GetProperty("state").GetProperty("current").GetString();
                Console.WriteLine($"Execution State: {state}");

                if (state == "SUCCESS" || state == "FAILED")
                {
                    if (state == "SUCCESS")
                    {
                        try
                        {
                            var inputs = doc.RootElement
                                .GetProperty("inputs")
                                .GetProperty("after")
                                .GetRawText();

                            var data = JsonSerializer.Deserialize<mstOrder>(inputs);

                            if (data != null)
                            {
                                //await UpdateOrderAsync(data, token);
                                Console.WriteLine("Order updated successfully in database");
                            }
                            else
                            {
                                Console.WriteLine("Data order kosong, update dibatalkan");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Gagal memproses data: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Execution failed");
                    }

                    break;
                }

                await Task.Delay(3000, token);
            }
        }

        private async Task UpdateOrderAsync(mstOrder order, CancellationToken token)
        {
            var query = @"
                UPDATE mstOrder
                SET
                    PMSitac = 'ANANDe',
                    TenantID = @TenantID,
                    CompanyID = @CompanyID,
                    CreatedBy = @CreatedBy,
                    ProductID = @ProductID,
                    UpdatedBy = @UpdatedBy,
                    ColoTypeID = @ColoTypeID,
                    CreatedDate = @CreatedDate,
                    PLNPowerKVA = @PLNPowerKVA,
                    SitacTypeID = @SitacTypeID,
                    UpdatedDate = @UpdatedDate,
                    ShelterTypeID = @ShelterTypeID,
                    SitacOfficerID = @SitacOfficerID,
                    AccountManagerID = @AccountManagerID,
                    FieldControllerID = @FieldControllerID,
                    SitacSpecialistID = @SitacSpecialistID,
                    STIPApprovalStatus = @STIPApprovalStatus,
                    LeadProjectManagerID = @LeadProjectManagerID,
                    ProjectDirectorPDIID = @ProjectDirectorPDIID,
                    SOWCustomerProductID = @SOWCustomerProductID
                WHERE PMCME = @PMCME AND SONumber = @SONumber";

            using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(token);

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@PMCME", order.PMCME ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@SONumber", order.SONumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@TenantID", order.TenantID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CompanyID", order.CompanyID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", order.CreatedBy ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ProductID", order.ProductID);
            cmd.Parameters.AddWithValue("@UpdatedBy", order.UpdatedBy ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ColoTypeID", order.ColoTypeID);
            cmd.Parameters.AddWithValue("@CreatedDate", order.CreatedDate);
            cmd.Parameters.AddWithValue("@PLNPowerKVA", order.PLNPowerKVA);
            cmd.Parameters.AddWithValue("@SitacTypeID", order.SitacTypeID);
            cmd.Parameters.AddWithValue("@UpdatedDate", order.UpdatedDate);
            cmd.Parameters.AddWithValue("@ShelterTypeID", order.ShelterTypeID);
            cmd.Parameters.AddWithValue("@SitacOfficerID", order.SitacOfficerID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountManagerID", order.AccountManagerID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@FieldControllerID", order.FieldControllerID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@SitacSpecialistID", order.SitacSpecialistID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@STIPApprovalStatus", order.STIPApprovalStatus);
            cmd.Parameters.AddWithValue("@LeadProjectManagerID", order.LeadProjectManagerID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ProjectDirectorPDIID", order.ProjectDirectorPDIID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@SOWCustomerProductID", order.SOWCustomerProductID);

            await cmd.ExecuteNonQueryAsync(token);
        }
    }
}
