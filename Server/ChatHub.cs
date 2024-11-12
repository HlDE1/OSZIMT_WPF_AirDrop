using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected", Clients.All.Count);
        SendMessage("Server", "Welcome to the chat!");
        await base.OnConnectedAsync();
    }

    // A method to send messages to all connected clients
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

}
