using Bot.res;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bot
{
	class RuleAgreement : ILoadable
	{
		private double refreshTime = 0;
		private RestUserMessage codeMessage;

		private const ulong APPROVED_ROLE = 459849869339787277;
		private const ulong RULES_CHANNEL = 465662927064793108;
		private DateTime EPOCH = new DateTime(1970, 1, 1);
		private const double REFRESH_DELAY = 30000;
		private const int CODE_LENGTH = 8;

		public async void Load(DiscordSocketClient client)
		{
			SocketTextChannel rulesChannel = (SocketTextChannel)client.GetChannel(RULES_CHANNEL);
			try
			{
				foreach (IMessage message in await rulesChannel.GetMessagesAsync(2).Flatten())
				{
					if (!message.Equals(codeMessage) && message.Author.Id.Equals(client.CurrentUser.Id))
					{
						await message.DeleteAsync();
					}
				}
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
			}
			codeMessage = await rulesChannel.SendMessageAsync(getCodeMessage(getCurrentTime()));

			client.UserJoined += OnUserJoined;
			client.MessageReceived += OnMessage;
		}

		private async Task OnUserJoined(SocketGuildUser user)
		{
			// todo: Try to dm the user about needing to agree to the rules.
		}

		private async Task OnMessage(SocketMessage message)
		{
			// If the user does not have the Little Fish role,
			//  If the message is the invite code, accept it (give the user the Little Fish role, schedule the code refresh).
			//  Delete the message.
		}

		private double getCurrentTime()
		{
			return (DateTime.UtcNow - EPOCH).TotalMilliseconds;
		}

		private string getCodeMessage(double currentTime)
		{
			return strings.AgreeMessage +
				Convert.ToInt64(currentTime).ToString("X").Substring(0, CODE_LENGTH);
		}

		private async void UpdateMessage()
		{
			double currentTime = getCurrentTime();
			if (currentTime > refreshTime)
			{
				refreshTime = int.MaxValue;
				await codeMessage.ModifyAsync((messageProps) =>
				{
					messageProps.Content = getCodeMessage(currentTime);
				}, new RequestOptions()
				{
					RetryMode = RetryMode.RetryTimeouts | RetryMode.Retry502
				});
				refreshTime = currentTime + REFRESH_DELAY;
			}
		}
	}
}
