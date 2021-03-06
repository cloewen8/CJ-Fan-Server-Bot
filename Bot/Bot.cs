﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Configuration;

namespace Bot
{
	public class Bot
	{
		private ManualResetEvent runComplete;
		private DiscordSocketClient client;

		public static Configuration.BotSection Config { get; private set; }

		public async Task StartAsync()
		{
			Config = (Configuration.BotSection) ConfigurationManager.GetSection("bot");
			runComplete = new ManualResetEvent(false);
			client = new DiscordSocketClient();

			client.Log += OnLog;
			client.Ready += OnReady;
			client.Disconnected += OnDisconnected;
			
			Trace.WriteLine("Logging in.");
			await client.LoginAsync(Discord.TokenType.Bot,
				Config.Token);
			await client.StartAsync();
			runComplete.WaitOne();
			StopAsync();
		}

		private async void StopAsync()
		{
			await client.LogoutAsync();
			await client.StopAsync();
			runComplete.Set();
		}

		private async Task OnReady()
		{
			foreach (Type loadable in AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany((assembly) => assembly.GetTypes())
				.Where((type) => type.GetInterfaces().Contains(typeof(ILoadable))))
			{
				((ILoadable) Activator.CreateInstance(loadable)).Load(client);
			}
		}

		private async Task OnLog(Discord.LogMessage message)
		{
			StringBuilder excBuilder = new StringBuilder();
			TraceLevel entryType;
			bool critical = false;
			excBuilder.Append("A message was logged: ");
			excBuilder.Append(message.Message);
			excBuilder.AppendLine();
			excBuilder.AppendLine();
			excBuilder.Append(message.Exception.StackTrace);

			switch (message.Severity)
			{
				case Discord.LogSeverity.Critical:
					entryType = TraceLevel.Error;
					critical = true;
					break;
				case Discord.LogSeverity.Error:
					entryType = TraceLevel.Error;
					break;
				case Discord.LogSeverity.Debug:
					entryType = TraceLevel.Info;
					break;
				case Discord.LogSeverity.Warning:
					entryType = TraceLevel.Warning;
					break;
				default:
					entryType = TraceLevel.Info;
					break;
			}

			switch (entryType) {
				case TraceLevel.Error:
					Trace.TraceError(excBuilder.ToString());
					break;
				case TraceLevel.Info:
					Trace.TraceInformation(excBuilder.ToString());
					break;
				case TraceLevel.Warning:
					Trace.TraceWarning(excBuilder.ToString());
					break;
			}
			if (critical)
				runComplete.Set();
		}

		private async Task OnDisconnected(Exception exc)
		{
			StringBuilder excBuilder = new StringBuilder();
			excBuilder.Append("The client was disconnected: ");
			excBuilder.Append(exc.Message);
			excBuilder.AppendLine();
			excBuilder.AppendLine();
			excBuilder.Append(exc.StackTrace);
			Trace.TraceError(excBuilder.ToString());
			runComplete.Set();
		}
	}
}
