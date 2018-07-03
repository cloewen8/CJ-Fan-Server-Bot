using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fan_Server_Bot
{
	class CmdsManager
	{
		private List<ICmd> cmds = new List<ICmd>();
		private readonly ICmd helpCmd;
		private readonly string botMention;
		private BotConfig config = BotConfig.Instance;

		internal CmdsManager(ulong botId)
		{
			helpCmd = new HelpCmd.Cmd(cmds);
			botMention = "<@" + botId + ">";
			RegisterCmd(helpCmd);
		}

		internal async Task OnMessage(SocketMessage message)
		{
			if (message.Content.Equals(botMention))
			{
				await ExecuteCmd(message, helpCmd, null);
			}
			if (message.Content.StartsWith(config.CmdPrefix))
			{
				bool isOwner = false;
				if (message.Author is IGuildUser guildUser)
					isOwner = guildUser.RoleIds.Any((role) => role == config.OwnerRoleId);
				ICmd found = (from cmd in cmds
				              where AllowedCmd(cmd, message.Author) &&
				              	cmd.Pattern.IsMatch(message.Content, 1)
				              select cmd).FirstOrDefault();
				if (found != null)
				{
					await ExecuteCmd(message, found, found.Pattern.Match(message.Content, 1).Captures);
				}
			}
		}

		public static bool AllowedCmd(ICmd cmd, SocketUser user)
		{
			bool isOwner = false;
			if (user is IGuildUser guildUser)
				isOwner = guildUser.RoleIds.Any((role) => role == BotConfig.Instance.OwnerRoleId);
			return isOwner || !cmd.OwnerOnly;
		}

		private async Task ExecuteCmd(SocketMessage message, ICmd cmd, CaptureCollection args)
		{
			try
			{
				CancellationTokenSource cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(config.CmdTimeout));
				Task executeTask = cmd.Execute(message,
					args,
					cancelSource.Token);
				await executeTask;
				cancelSource.Dispose();
				if (cancelSource.IsCancellationRequested)
				{
					throw new TimeoutException();
				}
				else if (executeTask.Exception != null)
				{
					Exception exc = executeTask.Exception;
					while (exc.InnerException != null)
						exc = exc.InnerException;
					throw exc;
				}
			}
			catch (TimeoutException)
			{
				Bot.MainEventLog.WriteEntry("A command ran out of time to respond.", EventLogEntryType.Warning);
				await message.Channel.SendMessageAsync(
					"The " + config.Name + " ran out of time to respond! Try again shortly.");
			}
			catch (Exception exc)
			{
				StringBuilder excBuilder = new StringBuilder();
				excBuilder.Append("A command failed to execute: ");
				excBuilder.Append(exc.Message);
				excBuilder.AppendLine();
				excBuilder.AppendLine();
				excBuilder.Append(exc.StackTrace);
				Bot.MainEventLog.WriteEntry(excBuilder.ToString(), EventLogEntryType.Error);
				await message.Channel.SendMessageAsync("A command failed to execute. Please contact the server owner.");
			}
		}

		private void RegisterCmd(ICmd cmd)
		{
			cmds.Add(cmd);
		}
	}
}
