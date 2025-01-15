using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using DiscordAnnouncer.Models;
using Newtonsoft.Json;

namespace DiscordAnnouncer;

[ApiVersion(2, 1)]
public class AnnouncerPlugin : TerrariaPlugin
{
	DiscordBotModel botInfo;
	MessageModel messageModel;

	public override string Author
	{
		get { return "SkyeRaf"; }
	}

	public override string Description
	{
		get { return "A plugin that integrates functionality with discord"; }
	}

	public override string Name
	{
		get { return "Discord Announcer"; }
	}

	public override Version Version
	{
		get { return new Version(1, 1, 0, 0); }
	}

	public AnnouncerPlugin(Main game) : base(game)
	{

	}

	public override void Initialize()
	{
		messageModel = new MessageModel()
		{
			content = "",
		};

		Commands.ChatCommands.Add(new Command("config.reload.permission", OnReloadConfig, "reloadconfig", "rcfg")
		{
			HelpText = "Reloads the configuration file for Discord Announcer."
		});

		string configPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory() + @"\").FullName, "DiscordAnnouncer.json");

		if (!File.Exists(configPath))
		{
			File.WriteAllText(configPath, JsonConvert.SerializeObject(new DiscordBotModel() { EnableAnnouncer = true, WebhookToken = "TOKEN_HERE", WebhookID = 0 }, Formatting.Indented));
		}

		try
		{
			botInfo = JsonConvert.DeserializeObject<DiscordBotModel>(File.ReadAllText(configPath));

			ServerApi.Hooks.ServerJoin.Register(this, OnPlayerJoin);
			ServerApi.Hooks.ServerLeave.Register(this, OnPlayerLeave);
			ServerApi.Hooks.ServerBroadcast.Register(this, OnServerBroadcast);
		}
		catch (FileNotFoundException)
		{
			Console.Write("Could not find config file 'DiscordAnnouncer.json'");
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			// Deregister hooks here
			ServerApi.Hooks.ServerJoin.Deregister(this, OnPlayerJoin);
			ServerApi.Hooks.ServerLeave.Deregister(this, OnPlayerLeave);
			ServerApi.Hooks.ServerBroadcast.Deregister(this, OnServerBroadcast);
		}
		base.Dispose(disposing);
	}

	void OnPlayerJoin(JoinEventArgs args)
	{
		if (botInfo.EnableAnnouncer == false)
			return;

		messageModel.content = $"Player: \"{TShock.Players[args.Who].Name}\" has connected to the server. {TShock.Utils.GetActivePlayerCount() + 1}/16 players online.";

		Task.Run(async ()=> await CreateMessage(messageModel, botInfo.WebhookID, botInfo.WebhookToken));
	}

	void OnPlayerLeave(LeaveEventArgs args)
	{
		if (botInfo.EnableAnnouncer == false)
			return;

		if (TShock.Players[args.Who] != null)
		{
			messageModel.content = $"Player: \"{TShock.Players[args.Who].Name}\" has disconnected from the server. {TShock.Utils.GetActivePlayerCount() - 1}/16 players online.";

			Task.Run(async ()=> await CreateMessage(messageModel, botInfo.WebhookID, botInfo.WebhookToken));
		}
	}

	private void OnServerBroadcast(ServerBroadcastEventArgs args)
	{
		if (botInfo.EnableAnnouncer == false)
			return;

		if ((args.Color.R == 50 && args.Color.G == 255 && args.Color.B == 130) || (args.Color.R == 175 && args.Color.G == 75 && args.Color.B == 255))
		{
			messageModel.content = $"Server Message: {args.Message}";

			Task.Run(async ()=> await CreateMessage(messageModel, botInfo.WebhookID, botInfo.WebhookToken));
		}
	}

	public async Task CreateMessage(MessageModel message, long webhookID, string webhookToken)
	{
		var authUrl = $"https://discordapp.com/api/webhooks/{webhookID}/{webhookToken}";

        using var httpClient = new HttpClient();
        var postBody = JsonConvert.SerializeObject(message);
        await httpClient.PostAsync(authUrl, new StringContent(postBody, Encoding.UTF8, "application/json"));
    }

	public void OnReloadConfig(CommandArgs args)
	{
		try
		{
			string configPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory() + @"\").FullName, "DiscordAnnouncer.json");

			if (!File.Exists(configPath))
			{
				File.WriteAllText(configPath, JsonConvert.SerializeObject(new DiscordBotModel() { EnableAnnouncer = true, WebhookToken = "TOKEN_HERE", WebhookID = 0 }, Formatting.Indented));
			}

			botInfo = JsonConvert.DeserializeObject<DiscordBotModel>(File.ReadAllText(configPath));

			Console.Write("DiscordAnnouncer config file reloaded successfully.");
		}
		catch
		{
			Console.WriteLine("Could not reload DiscordAnnouncer config file.");
		}
	}
}