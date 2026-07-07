using System.Threading;
using System.Threading.Tasks;

namespace ShopNest.Application.Interfaces;

public interface INotificationProvider
{
    string Channel { get; }
    Task<bool> SendAsync(string recipient, string title, string message, CancellationToken cancellationToken);
}
