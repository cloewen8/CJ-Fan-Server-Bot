using System.Configuration;

namespace Bot.Configuration
{
	public class BotSection : ConfigurationSection
	{
		[ConfigurationProperty("Token", IsRequired = true)]
		public string Token
		{
			get
			{
				return (string) this["Token"];
			}
			set
			{
				this["Token"] = value;
			}
		}

		[ConfigurationProperty("OwnerRoleId", IsRequired = true)]
		public ulong OwnerRoleId
		{
			get
			{
				return (ulong) this["OwnerRoleId"];
			}
			set
			{
				this["OwnerRoleId"] = value;
			}
		}

		[ConfigurationProperty("IsDevelopment", DefaultValue = false)]
		public bool IsDevelopment
		{
			get
			{
				return (bool) this["IsDevelopment"];
			}
			set
			{
				this["IsDevelopment"] = value;
			}
		}
	}

	public class CmdsManagerSection : ConfigurationSection
	{
		[ConfigurationProperty("Prefix", DefaultValue = "/")]
		public string Prefix
		{
			get
			{
				return (string) this["Prefix"];
			}
			set
			{
				this["Prefix"] = value;
			}
		}

		[ConfigurationProperty("Timeout", IsRequired = true)]
		public double Timeout
		{
			get
			{
				return (double) this["Timeout"];
			}
			set
			{
				this["Timeout"] = value;
			}
		}
	}
}
