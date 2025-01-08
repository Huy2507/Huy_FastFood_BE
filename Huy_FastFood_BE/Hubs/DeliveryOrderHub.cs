using Microsoft.AspNetCore.SignalR;

public class DeliveryOrderHub : Hub
{
    // Phương thức gọi từ server để cập nhật danh sách cho tất cả các client
    public async Task RefreshDeliveryOrderList()
    {
        await Clients.All.SendAsync("RefreshDeliveryOrderList");
    }
}
