using Microsoft.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
	public class CurrentEnvironment
	{
		private static Environment? current = null;
		
		public static Environment Get
		{
			get
			{
				if (current == null)
				{
					switch (CloudConfigurationManager.GetSetting("Bot.Environment"))
					{
						case "PRODUCTION":
							current = Environment.PRODUCTION;
							break;
						default:
							current = Environment.DEVELOPMENT;
							break;
					}
				}
				return current.Value;
			}
		}
	}

	public enum Environment
	{
		DEVELOPMENT, PRODUCTION
	}
}
