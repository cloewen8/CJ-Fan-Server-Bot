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

		public static EventLog MainEventLog { get; internal set; }

		internal async Task StartAsync(ServiceBase service)
		{
			config = BotConfig.Instance;
			client = new DiscordSocketClient();
			this.service = service;

			client.Log += OnLog;
			client.Connected += OnConnected;
			client.Disconnected += OnDisconnected;

			if (config.Token != null)
			{
				await client.LoginAsync(Discord.TokenType.Bot, config.Token);
				await client.StartAsync();
			}
			else
			{
				MainEventLog.WriteEntry("Missing bot token!", EventLogEntryType.Error);
				service.Stop();
			}
		}

		internal async void StopAsync()
		{
			await client.LogoutAsync();
			await client.StopAsync();
		}

		private async Task OnConnected()
		{
			CmdsManager cmds = new CmdsManager(client.CurrentUser.Id);

			if (config.Name != null && client.CurrentUser.Username != config.Name)
			{
				MainEventLog.WriteEntry("The username is incorrect and will be modified.");
				await client.CurrentUser.ModifyAsync(user =>
					user.Username = config.Name);
			}

			client.MessageReceived += cmds.OnMessage;
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

			MainEventLog.WriteEntry(excBuilder.ToString(), entryType);
			if (critical)
				service.Stop();
		}

		private async Task OnDisconnected(Exception exc)
		{
			StringBuilder excBuilder = new StringBuilder();
			excBuilder.Append("The client was disconnected: ");
			excBuilder.Append(exc.Message);
			excBuilder.AppendLine();
			excBuilder.AppendLine();
			excBuilder.Append(exc.StackTrace);
			MainEventLog.WriteEntry(excBuilder.ToString(), EventLogEntryType.Error);
		}
	}

	public class BotConfig
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

		public string CmdPrefix
		{
			get
			{
				return (string)Registry.GetValue(_regKey, "CmdPrefix", "/");
			}
		}

		public ulong OwnerRoleId
		{
			get
			{
				return (ulong) (long) Registry.GetValue(_regKey, "OwnerRoleId", 0);
			}
		}

		internal int CmdTimeout
		{
			get
			{
				return (int) Registry.GetValue(_regKey, "CmdTimeout", Double.MaxValue);
			}
		}
	}
}
