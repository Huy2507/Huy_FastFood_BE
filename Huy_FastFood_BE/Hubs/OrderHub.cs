using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Huy_FastFood_BE.Hubs
{
    public class OrderHub : Hub
    {
        // Phương thức để gửi thông tin cập nhật đơn hàng đến client
        public async Task SendOrderUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveOrderUpdate", message);
        }
    }
}
