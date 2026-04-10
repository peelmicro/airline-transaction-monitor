using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Gateway.Hubs;

/// <summary>
/// SignalR hub for real-time dashboard updates.
/// Clients connect here to receive live transaction, metrics, and alert events.
/// The hub itself is passive — the NatsToSignalRService pushes events to connected clients.
/// </summary>
[Authorize]
public class TransactionHub : Hub
{
    private readonly ILogger<TransactionHub> _logger;

    public TransactionHub(ILogger<TransactionHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
