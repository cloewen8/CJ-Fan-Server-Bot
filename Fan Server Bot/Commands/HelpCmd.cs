using Discord;
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
	class Cmd : ICmd, IDoc
	{
		private readonly BotConfig config;
		private readonly Color primaryColor = new Color(231, 76, 60);
		// Syntax: https://developers.google.com/style/code-syntax
		private readonly string description =
@"Below is a list of commands that you currently have access to.

To execute a command, chat the syntax provided for the desired command.

- If a command spans multiple lines, you will need to provide multiple messages.
- Text items on their own are required.
- Optional items are surrounded by square brackets.
- You need to choose an item for text items surrounded by braces, separated by vertical bars.

**Example:** */help*";

		public Regex Pattern => new Regex(@"(?:help|\?)");
		public bool OwnerOnly => false;
		private IEnumerable<ICmd> Cmds { get; set; }

		public string Name => "Help";
		public string[] Description => new string[] {
			"Displays help information for the bot."
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
				Title = "Help",
				Color = primaryColor,
				Description = description
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
				stringBuilder.AppendLine("**Usage**");
				stringBuilder.Append(config.CmdPrefix);
				stringBuilder.AppendLine(String.Join("\n", cmd.Usage));
				stringBuilder.AppendLine();
				stringBuilder.Append("**Example:** *");
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
