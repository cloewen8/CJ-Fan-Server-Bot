using Discord.WebSocket;
using Fan_Server_Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HelpCmd
{
    class Cmd : ICmd
    {
        public Regex Pattern => new Regex(@"(?:help|\?)");

        public bool OwnerOnly => false;

        public async Task Execute(SocketMessage message, CaptureCollection args, CancellationToken cancelToken)
        {
            await message.Channel.SendMessageAsync("This bot is unfinished. Check back later.");
        }
    }
}
