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
	public class CmdsManager : ILoadable
	{
		private const string CANCEL_MESSAGE = "cancel";

		private HelpCmd.Cmd helpCmd;
		private string botMention;
		private Regex mentionPrefix;
		private Queue<Queue<MessageRequest>> requests;

		public List<ICmd> Cmds { get; private set; }

		public async void Load(DiscordSocketClient client)
		{
			Cmds = new List<ICmd>();
			botMention = "<@" + client.CurrentUser.Id + ">";
			mentionPrefix = new Regex("^" + botMention + "\\s*");
			foreach (Type cmd in System.Reflection.Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where((type) => type.GetInterfaces().Contains(typeof(ICmd))))
			{
				RegisterCmd((ICmd) Activator.CreateInstance(cmd));
			}
			helpCmd = (HelpCmd.Cmd) Cmds.First((cmd) => typeof(HelpCmd.Cmd).IsInstanceOfType(cmd)); // fixme: Returns no result.
			requests = new Queue<Queue<MessageRequest>>();
			MessageExtensions.Cmds = this;

			client.LoggedOut += Unload;
			client.MessageReceived += OnMessage;
		}

		private async Task Unload()
		{
			ClearRequests();
		}

		private async Task OnMessage(SocketMessage message)
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
					request.Call.RegisterInteraction(message);
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
				string prefix = Call.Prefix;
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
					ICmd found = (from cmd in Cmds
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
				Task executeTask = cmd.Execute(new Call(this,
					message,
					args,
					cancelSource.Token));
				await executeTask;
				bool cancelled = cancelSource.IsCancellationRequested;
				cancelSource.Dispose();
				if (cancelled)
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
			Cmds.Add(cmd);
		}

		internal void RegisterRequests(params MessageRequest[] newRequests)
		{
			requests.Enqueue(new Queue<MessageRequest>(newRequests));
			newRequests.First().Prompt();
		}

		private void ClearRequests()
		{
			requests.Clear();
		}
	}
}
