using Discord;
using Discord.WebSocket;
using Bot;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TranslateCmd
{
	class Cmd : ICmd
	{
		public Regex Pattern => new Regex("translate");

		public bool OwnerOnly => false;

		public Task Execute(Call call)
		{
			EmbedBuilder builder = new EmbedBuilder
			{
				Title = "User Requested Translation"
			};
			EmbedFooterBuilder footer = new EmbedFooterBuilder
			{
				Text = $"Submitted by: {call.Message.Author.Username} (${call.Message.Author.Id})"
			};
			builder.Footer = footer;

			Task.Run(async () =>
			{
				// todo: Handle a cancellation.
				SocketMessage[] messages = await call.Message.RequestMessages(call, new string[] {
					"What was the original text?",
					"What is the text translated?",
					"What language did you translate it to?"
				});

				// builder.AddField("Original", originalRequest.Result.Content);
				// builder.AddField("Translated", translateRequest.Result);
				// builder.AddField("Locale", localeRequest.Result);
				// call.Message.Channel.SendMessageAsync("", false, builder);
				call.Dispose();
			});

			return Task.CompletedTask;
		}
	}
}
