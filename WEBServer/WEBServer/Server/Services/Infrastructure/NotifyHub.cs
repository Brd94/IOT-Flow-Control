using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WEBServer.Shared;

namespace WEBServer.Server.Services.Infrastructure
{
    public class NotifyHub : Hub
    {
        int Connected = 0;

        public override Task OnConnectedAsync()
        {
            ++Connected;
            Console.WriteLine("Client connesso. Totali : " + Connected);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            --Connected;
            Console.WriteLine("Client disconnesso. Totali : " + Connected);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task UpdateCompany(object message)
        {
           await this.Clients.All.SendAsync("UpdateReceived", message);

        }
    }
}
