using Discord.WebSocket;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fan_Server_Bot
{
	public interface ICmd
	{
		Regex Pattern { get; }
		bool OwnerOnly { get; }
		Task Execute(SocketMessage message, CaptureCollection args, CancellationToken cancelToken);
	}
}
