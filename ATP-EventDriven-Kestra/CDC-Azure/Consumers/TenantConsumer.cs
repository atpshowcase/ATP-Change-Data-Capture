using CDC_Azure.Models;

namespace CDC_Azure.Consumers
{
    public class TenantConsumer : BaseConsumer<mstTenant>
    {
        public TenantConsumer() : base("fullfillment.Test_DB.dbo.mstTenant") // Ganti ke topic tenant kamu
        {
        }

        protected override async Task HandleMessage(mstTenant data)  // Change from 'async void' to 'async Task'
        {
            System.Diagnostics.Debug.WriteLine($"Order Received: {data.id} - {data.tenant_name}");
        }
    }
}
