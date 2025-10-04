using Microsoft.AspNetCore.SignalR;
namespace ResellBook.Hubs
{

    public class ChatHub : Hub
    {
        // Send message to specific user
        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message, DateTime.UtcNow);
        }
    }

}
