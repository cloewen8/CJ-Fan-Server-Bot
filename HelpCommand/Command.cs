using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Fan_Server_Bot;

namespace HelpCommand
{
    class HelpCommand : ICmd
    {
        public Regex Pattern => throw new NotImplementedException();

        public bool OwnerOnly => throw new NotImplementedException();

        public Task Execute(SocketMessage message, CaptureCollection args, CancellationToken cancelToken)
        {
            throw new NotImplementedException();
        }
    }
}
