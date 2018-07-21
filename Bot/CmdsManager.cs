using Bot.res;
using Discord;
using Discord.WebSocket;
using Microsoft.Azure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bot
{
	class CmdsManager
	{
		private const string CANCEL_MESSAGE = "cancel";

		private List<ICmd> cmds = new List<ICmd>();
		private readonly ICmd helpCmd;
		private readonly string botMention;
		private readonly Regex mentionPrefix;
		private Queue<Queue<MessageRequest>> requests;

		internal CmdsManager(ulong botId)
		{
			helpCmd = new HelpCmd.Cmd(cmds);
			botMention = "<@" + botId + ">";
			mentionPrefix = new Regex("^" + botMention + "\\s*");
			RegisterCmd(helpCmd);
			requests = new Queue<Queue<MessageRequest>>();
		}

		internal async Task OnMessage(SocketMessage message)
		{
			MessageRequest request = GetRequest(message);
			if (request != null)
			{
				if (message.Content.ToLower().Equals(CANCEL_MESSAGE))
				{
					request.Cancel();
				}
				else
				{
					request.Resolve(message);
				}
			}
			else if (message.Content.Equals(botMention))
			{
				await ExecuteCmd(message, helpCmd, null);
			}
			else
			{
				int offset = -1;
				string prefix = CloudConfigurationManager.GetSetting("Bot.CmdsManager.Prefix");
				ulong ownerRoleId = ulong.Parse(CloudConfigurationManager.GetSetting("Bot.OwnerRoleId"));
				if (message.Content.StartsWith(prefix))
				{
					offset = prefix.Length;
				}
				else if (mentionPrefix.IsMatch(message.Content))
				{
					offset = mentionPrefix.Match(message.Content).Length;
				}

				if (offset > 0)
				{
					bool isOwner = false;
					if (message.Author is IGuildUser guildUser)
						isOwner = guildUser.RoleIds.Any((role) => role == ownerRoleId);
					ICmd found = (from cmd in cmds
								  where AllowedCmd(cmd, message.Author) &&
									  cmd.Pattern.IsMatch(message.Content, offset)
								  select cmd).FirstOrDefault();
					if (found != null)
					{
						await ExecuteCmd(message, found, found.Pattern.Match(message.Content, offset).Captures);
					}
				}
			}
		}

		public static bool AllowedCmd(ICmd cmd, SocketUser user)
		{
			bool isOwner = false;
			ulong ownerRoleId = ulong.Parse(CloudConfigurationManager.GetSetting("Bot.OwnerRoleId"));
			if (user is IGuildUser guildUser)
				isOwner = guildUser.RoleIds.Any((role) => role == ownerRoleId);
			return isOwner || !cmd.OwnerOnly;
		}

		private async Task ExecuteCmd(SocketMessage message, ICmd cmd, CaptureCollection args)
		{
			try
			{
				double timeout = double.Parse(CloudConfigurationManager.GetSetting("Bot.CmdsManager.Timeout"));
				CancellationTokenSource cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
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
				string botName = CloudConfigurationManager.GetSetting("Bot.Name");
				Trace.TraceWarning(strings.CmdOutOfTime);
				await message.Channel.SendMessageAsync(
					"The " + botName + " ran out of time to respond! Try again shortly.");
			}
			catch (Exception exc)
			{
				StringBuilder excBuilder = new StringBuilder();
				excBuilder.Append("A command failed to execute: ");
				excBuilder.Append(exc.Message);
				excBuilder.AppendLine();
				excBuilder.AppendLine();
				excBuilder.Append(exc.StackTrace);
				Trace.TraceError(excBuilder.ToString());
				await message.Channel.SendMessageAsync(strings.CmdFailedToExecute);
			}
		}

		private MessageRequest GetRequest(SocketMessage message)
		{
			MessageRequest request = null;
			if (requests.Count > 0 && requests.Peek().Count > 0 &&
				requests.Peek().Peek().Matches(message))
			{
				request = requests.Peek().Dequeue();
				if (requests.Peek().Count == 0)
				{
					requests.Dequeue();
				}
				else
				{
					requests.Peek().Peek().Prompt();
				}
					
			}
			return request;
		}

		private void RegisterCmd(ICmd cmd)
		{
			cmds.Add(cmd);
		}

		internal void RegisterRequests(MessageRequest[] newRequests)
		{
			requests.Enqueue(new Queue<MessageRequest>(newRequests));
			newRequests.First().Prompt();
		}

		internal void ClearRequests()
		{
			requests.Clear();
		}
	}
}
