using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fan_Server_Bot
{
	public static class MessageExtensions
	{
		internal static CmdsManager Cmds { private get; set; }

		private static Task<SocketMessage> RequestMessage(MessageRequest[] cluster, SocketMessage original,
			string text, bool isTTS = false, Embed embed = null, RequestOptions options = null,
			Regex pattern = null)
		{
			TaskCompletionSource<SocketMessage> taskSource = new TaskCompletionSource<SocketMessage>();
			cluster[cluster.Length - 1] = new MessageRequest(taskSource.SetResult, taskSource.SetCanceled, original.Author,
				original.Channel, text, isTTS, embed, options,
				pattern);
			return taskSource.Task;
		}

		public static Task<SocketMessage> RequestMessage(this SocketMessage original,
			string text, bool isTTS = false, Embed embed = null, RequestOptions options = null,
			Regex pattern = null)
		{
			MessageRequest[] cluster = new MessageRequest[1];
			Task<SocketMessage> task = RequestMessage(cluster, original, text, isTTS, embed, options, pattern);
			Cmds.RegisterRequests(cluster);
			return task;
		}

		public static Task<SocketMessage[]> RequestMessages(this SocketMessage original,
			string[] text, bool[] isTTS = null, Embed[] embed = null, RequestOptions[] options = null,
			Regex[] pattern = null)
		{
			if (text.Length < 2)
			{
				throw new ArgumentOutOfRangeException("text", "Use RequestMessage for singular requests.");
			}
			else if (isTTS != null && isTTS.Length != text.Length)
			{
				throw new ArgumentOutOfRangeException("isTTS", "Either pass in null or an equal number of text.");
			}
			else if (embed != null && embed.Length != text.Length)
			{
				throw new ArgumentOutOfRangeException("embed", "Either pass in null or an equal number of text.");
			}
			else if (options != null && options.Length != text.Length)
			{
				throw new ArgumentOutOfRangeException("options", "Either pass in null or an equal number of text.");
			}
			else if (pattern != null && pattern.Length != text.Length)
			{
				throw new ArgumentOutOfRangeException("pattern", "Either pass in null or an equal number of text.");
			}

			Task<SocketMessage>[] tasks = new Task<SocketMessage>[text.Length];
			MessageRequest[] cluster = new MessageRequest[text.Length];
			for (int position = 0; position < text.Length; position++)
			{
				tasks[tasks.Length - 1] = RequestMessage(cluster, original,
					text[position], isTTS[position], embed[position], options[position],
					pattern[position]);
			}
			Cmds.RegisterRequests(cluster);
			return Task.WhenAll(tasks);
		}
	}

	internal class MessageRequest
	{
		private readonly SocketUser author;
		private readonly Regex pattern;
		private readonly ISocketMessageChannel channel;
		private readonly string text;
		private readonly bool isTTS;
		private readonly Embed embed;
		private readonly RequestOptions options;

		public Action<SocketMessage> Resolve { get; private set; }
		public Action Cancel { get; private set; }
		
		public MessageRequest(Action<SocketMessage> resolve, Action cancel, SocketUser author,
			ISocketMessageChannel channel, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null,
			Regex pattern = null)
		{
			Resolve = resolve;
			Cancel = cancel;
			this.author = author;
			this.pattern = pattern ?? new Regex(".");
			this.channel = channel;
			this.text = text;
			this.isTTS = isTTS;
			this.embed = embed;
			this.options = options;
		}

		public async void Prompt()
		{
			await channel.SendMessageAsync(text, isTTS, embed, options);
		}

		public bool Matches(SocketMessage candidate)
		{
			return candidate.Equals(author) && pattern.IsMatch(candidate.Content);
		}
	}
}
