﻿using System;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.Threading.Tasks;
using DiscordAnnouncer.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Linq;
using System.IO;

namespace DiscordAnnouncer
{
    [ApiVersion(2, 1)]
    public class AnnouncerPlugin : TerrariaPlugin
    {
        DiscordBotModel botInfo;
        MessageModel messageModel;

        /// <summary>
        /// Gets the author(s) of this plugin
        /// </summary>
        public override string Author
        {
            get { return "SkyeRaf"; }
        }

        /// <summary>
        /// Gets the description of this plugin.
        /// A short, one lined description that tells people what your plugin does.
        /// </summary>
        public override string Description
        {
            get { return "A plugin that integrates functionality with discord"; }
        }

        /// <summary>
        /// Gets the name of this plugin.
        /// </summary>
        public override string Name
        {
            get { return "Discord Announcer"; }
        }

        /// <summary>
        /// Gets the version of this plugin.
        /// </summary>
        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        /// <summary>
        /// Initializes a new instance of the TestPlugin class.
        /// This is where you set the plugin's order and perfrom other constructor logic
        /// </summary>
        public AnnouncerPlugin(Main game) : base(game)
        {

        }

        /// <summary>
        /// Handles plugin initialization. 
        /// Fired when the server is started and the plugin is being loaded.
        /// You may register hooks, perform loading procedures etc here.
        /// </summary>
        public override void Initialize()
        {
            messageModel = new MessageModel()
            {
                content = "",
                tts = false
            };

            Commands.ChatCommands.Add(new Command("config.reload.permission", OnReloadConfig, "reloadconfig", "rcfg")
            {
                HelpText = "Reloads the configuration file for Discord Announcer."
			});

            string configPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory() + @"\").FullName, "DiscordAnnouncer.json");

            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, JsonConvert.SerializeObject(new DiscordBotModel() { BotToken = "TOKEN_HERE", ChannelID = 0 }, Formatting.Indented));
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

        /// <summary>
        /// Handles plugin disposal logic.
        /// *Supposed* to fire when the server shuts down.
        /// You should deregister hooks and free all resources here.
        /// </summary>
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

            CreateMessage(messageModel, botInfo.ChannelID);
        }

        void OnPlayerLeave(LeaveEventArgs args)
        {
            if (botInfo.EnableAnnouncer == false)
                return;

            if (TShock.Players[args.Who] != null)
			{
                messageModel.content = $"Player: \"{TShock.Players[args.Who].Name}\" has disconnected from the server. {TShock.Utils.GetActivePlayerCount() - 1}/16 players online.";

                CreateMessage(messageModel, botInfo.ChannelID);
            }
        }

        private void OnServerBroadcast(ServerBroadcastEventArgs args)
        {
            if (botInfo.EnableAnnouncer == false)
                return;

            if ((args.Color.R == 50 && args.Color.G == 255 && args.Color.B == 130) || (args.Color.R == 175 && args.Color.G == 75 && args.Color.B == 255))
            {
                messageModel.content = $"Server Message: {args.Message}";

                CreateMessage(messageModel, botInfo.ChannelID);
            }
        }

        public void CreateMessage(MessageModel message, long channelID)
        {
            var authUrl = $"https://discordapp.com/api/channels/{channelID}/messages";
            
            using (var httpClient = new HttpClient())
            {
                var postBody = JsonConvert.SerializeObject(message);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botInfo.BotToken);
                httpClient.PostAsync(authUrl, new StringContent(postBody, Encoding.UTF8, "application/json"));
            }
        }

        public void OnReloadConfig(CommandArgs args)
        {
            try
            {
                string configPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory() + @"\").FullName, "DiscordAnnouncer.json");

                if (!File.Exists(configPath))
                {
                    File.WriteAllText(configPath, JsonConvert.SerializeObject(new DiscordBotModel() { BotToken = "TOKEN_HERE", ChannelID = 0 }, Formatting.Indented));
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
}