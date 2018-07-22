using Discord.WebSocket;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bot
{
	public interface ICmd
	{
		Regex Pattern { get; }
		bool OwnerOnly { get; }
		Task Execute(Call call);
	}
}
