# Discord-Announcer
A TShock plugin that integrates Discord notifications in a TShock server.

How to use: Copy the DiscordAnnouncer.dll into your "ServerPlugins" folder. Start TerrariaServer.exe, this will create a DiscordAnnouncer.json file in the same directory as TerrariaServer.exe.

Inside that file there will be 3 fields:
- EnableAnnouncer - true/false if you want the bot to enable or disable the plugin.
- WebhookToken - Copy your webhook token.
- WebhookID - Copy your webhook ID.

Currently the following messages will be posted on Discord:
- A player connected/Disconnected from the server.
- An event such as: Blood moon, solar eclipse, pirate/goblin/martian invasion, Slime, a boss spawned/died, wave messages from Frost/Pumpkin/Old Man's Army events

# How To Use:
By default the announcer will be enabled. If you want to disable it, change the EnableAnnouncer to false and type /rcfg or /reloadconfig to reload the plugin's configuration
