using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bot
{
	public interface ILoadable
	{
		void Load(DiscordSocketClient client);
	}
}
