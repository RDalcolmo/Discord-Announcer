using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordAnnouncer.Models
{
	class DiscordBotModel
	{
		public bool EnableAnnouncer { get; set; }
		public string WebhookToken { get; set; }
		public long WebhookID { get; set; }
	}
}
