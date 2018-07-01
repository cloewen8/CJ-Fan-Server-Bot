using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Win32;

namespace Fan_Server_Bot
{
    class Bot
    {
		private ServiceBase service;
		private BotConfig config;
		private DiscordSocketClient client;
		private EventLog mainEventLog;

        internal async Task StartAsync(ServiceBase service, EventLog eventLog)
        {
            config = BotConfig.Instance;
			client = new DiscordSocketClient();
			mainEventLog = eventLog;
			this.service = service;

			client.Log += OnLog;
			client.Connected += OnConnected;
			client.Disconnected += OnFatalError;
			
            await client.LoginAsync(Discord.TokenType.Bot, config.Token);
			await client.StartAsync();
        }

        internal async void StopAsync()
        {
			await client.LogoutAsync();
            await client.StopAsync();
        }

		private async Task OnConnected()
		{
			if (config.Name != null && client.CurrentUser.Username != config.Name)
			{
				mainEventLog.WriteEntry("The username is incorrect and will be modified.");
				await client.CurrentUser.ModifyAsync(user =>
					user.Username = config.Name);
			}
		}

		private async Task OnLog(Discord.LogMessage message)
		{
			StringBuilder excBuilder = new StringBuilder();
			EventLogEntryType entryType;
			bool critical = false;
			excBuilder.Append("A message was logged: ");
			excBuilder.Append(message.Message);
			excBuilder.AppendLine();
			excBuilder.AppendLine();
			excBuilder.Append(message.Exception.StackTrace);

			switch (message.Severity)
			{
				case Discord.LogSeverity.Critical:
					entryType = EventLogEntryType.Error;
					critical = true;
					break;
				case Discord.LogSeverity.Error:
					entryType = EventLogEntryType.Error;
					break;
				case Discord.LogSeverity.Debug:
					entryType = EventLogEntryType.Warning;
					break;
				case Discord.LogSeverity.Warning:
					entryType = EventLogEntryType.Warning;
					break;
				default:
					entryType = EventLogEntryType.Information;
					break;
			}

			mainEventLog.WriteEntry(excBuilder.ToString(), entryType);
			if (critical)
				service.Stop();
		}

		private async Task OnFatalError(Exception exc)
		{
			StringBuilder excBuilder = new StringBuilder();
			excBuilder.Append("An unexpected exception occured: ");
			excBuilder.Append(exc.Message);
			excBuilder.AppendLine();
			excBuilder.AppendLine();
			excBuilder.Append(exc.StackTrace);
			mainEventLog.WriteEntry(excBuilder.ToString(), EventLogEntryType.Error);
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
                return (string) Registry.GetValue(_regKey, "Token", null);
            }
        }

        public string Name
        {
            get
            {
				return (string) Registry.GetValue(_regKey, "Name", null);
            }
        }
    }
}
