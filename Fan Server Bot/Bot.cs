using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Win32;

namespace Fan_Server_Bot
{
    class BotClient : DiscordSocketClient
    {
        private EventLog mainEventLog;

        internal async Task Start(EventLog eventLog)
        {
            BotConfig config = BotConfig.Instance;
            mainEventLog = eventLog;
            await LoginAsync(Discord.TokenType.Bot, config.Token);
            await StartAsync();
            mainEventLog.WriteEntry("Successfully logged in.");

            if (CurrentUser.Username != config.Name)
            {
                mainEventLog.WriteEntry("The username is incorrect and will be modified.");
                await CurrentUser.ModifyAsync(user =>
                    user.Username = config.Name);
            }
        }
    }

    internal class BotConfig
    {
        private static BotConfig _instance;
        private static string _regKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\CJFanServerBot";

        private BotConfig()
        {
        }

        public static BotConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BotConfig();
                return _instance;
            }
        }

        internal string Token
        {
            get
            {
                return (string) Registry.GetValue(_regKey, "Token", "");
            }
        }

        public string Name
        {
            get
            {
                return (string) Registry.GetValue(_regKey, "Name", "Toy");
            }
        }
    }
}
