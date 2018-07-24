using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
	class RuleAgreement
	{
		

		internal RuleAgreement()
		{
		}

		internal async Task OnUserJoined(SocketGuildUser user)
		{
			// todo: Try to dm the user about needing to agree to the rules.
		}

		internal async Task OnMessage(SocketMessage message)
		{
			// If the user does not have the Little Fish role,
			//  If the message is the invite code, accept it (give the user the Little Fish role, schedule the code refresh).
			//  Delete the message.
		}

		private async void InvalidateCode()
		{
			// If the refresh time passed,
			//  Edit the code message.
		}
	}
}
