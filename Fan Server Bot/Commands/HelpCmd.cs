﻿using Discord;
using Discord.WebSocket;
using Fan_Server_Bot;
using Fan_Server_Bot.res;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HelpCmd
{
	class Cmd : ICmd, IDoc
	{
		private readonly BotConfig config;
		private readonly Color primaryColor = new Color(231, 76, 60);
		// Syntax: https://developers.google.com/style/code-syntax
		private readonly string body = strings.HelpBody;

		public Regex Pattern => new Regex(@"(?:help|\?)");
		public bool OwnerOnly => false;
		private IEnumerable<ICmd> Cmds { get; set; }

		public string Name => strings.HelpName;
		public string[] Description => new string[] {
			strings.HelpDescription1
		};
		public string[] Usage => new string[] {
			"help"
		};
		public string Example => "help";

		internal Cmd(IEnumerable<ICmd> cmds)
		{
			config = BotConfig.Instance;
			Cmds = cmds;
		}

		public async Task Execute(SocketMessage message, CaptureCollection args, CancellationToken cancelToken)
		{
			EmbedBuilder builder = new EmbedBuilder
			{
				Title = Name,
				Color = primaryColor,
				Description = body
			};
			AddCmdFields((from cmd in Cmds
			              where cmd is IDoc && CmdsManager.AllowedCmd(cmd, message.Author)
			              select (IDoc) cmd), builder);

			await message.Channel.SendMessageAsync("", false,
				builder.Build());
		}

		private void AddCmdFields(IEnumerable<IDoc> cmds, EmbedBuilder builder)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (IDoc cmd in cmds)
			{
				stringBuilder.AppendLine(String.Join("\n", cmd.Description));
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("**");
				stringBuilder.Append(strings.HelpUsage);
				stringBuilder.Append("**");
				stringBuilder.Append(config.CmdPrefix);
				stringBuilder.AppendLine(String.Join("\n", cmd.Usage));
				stringBuilder.AppendLine();
				stringBuilder.Append("**");
				stringBuilder.Append(strings.HelpExample);
				stringBuilder.Append("** *");
				stringBuilder.Append(config.CmdPrefix);
				stringBuilder.Append(cmd.Example);
				stringBuilder.Append("*");
				builder.AddField(cmd.Name, stringBuilder.ToString());
				stringBuilder.Clear();
			}
		}
	}
	
	public interface IDoc
	{
		string Name { get; }
		string[] Description { get; }
		string[] Usage { get; }
		string Example { get; }
	}
}
