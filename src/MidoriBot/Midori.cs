﻿using System;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using MidoriBot.Events;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace MidoriBot
{
    public class Midori
    {
        public static DiscordSocketClient MidoriClient;
        public static string Version = "1.0";
        public DiscordSocketConfig MidoriSocketConfig = new DiscordSocketConfig
        {
            DownloadUsersOnGuildAvailable = true,
            LogLevel = LogSeverity.Info,
            MessageCacheSize = 10
        };
        public CommandService MidoriCommands;
        public static Dictionary<string, object> MidoriConfig;
        public CommandServiceConfig MidoriCommandsConfig = new CommandServiceConfig
        {
            DefaultRunMode = RunMode.Sync
        };
        public MidoriHandler CommandHandler;

        public static string GetDescription()
        {
            return (Midori.MidoriClient.GetApplicationInfoAsync().GetAwaiter().GetResult()).Description;
        }

        public static void Main(string[] args)
        {
            Midori MidoriBot = new Midori();
            try
            {
                Console.WriteLine("Attempting to hand over control to async...");
                MidoriBot.Start().GetAwaiter().GetResult();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Failed to complete handover.");
                Console.WriteLine($"Cannot connect to Discord! D:");
                Console.WriteLine($"Exception details:");
                Console.WriteLine(e);
            }
        }



        public async Task Start()
        {
            MidoriClient = new DiscordSocketClient(MidoriSocketConfig);
            MidoriCommands = new CommandService(MidoriCommandsConfig);
            CommandHandler = new MidoriHandler();
            Console.WriteLine("Handover success.");
            Console.WriteLine("Created client, command service and command handler.");
            try
            {
                // Import JSON
                StreamReader RawOpen = File.OpenText(@"./midori_config.json");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("You dumb shit ! I can't find midori_config.json. Follow this guide and try again: https://github.com/lofdat/Midori/blob/master/README.md");
                Environment.FailFast("cannot find midori_config.json");
                await Task.Delay(-1);
            }
            StreamReader Raw = File.OpenText(@"./midori_config.json");
            JsonTextReader TextReader = new JsonTextReader(Raw);
            JObject MidoriJConfig = (JObject)JToken.ReadFrom(TextReader);
            MidoriConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(MidoriJConfig.ToString());

            // Setup dependencies
            IDependencyMap MidoriDeps = new DependencyMap();
            MidoriDeps.Add(MidoriClient);
            MidoriDeps.Add(MidoriCommands);
            MidoriDeps.Add(MidoriJConfig);
            Console.WriteLine("Organized dependency library.");

            // Events handler
            MidoriEvents MidoriEvents = new MidoriEvents();
            MidoriEvents.Install();
            Console.WriteLine("Installed event handler.");

            // Login and connect
            await MidoriClient.LoginAsync(TokenType.Bot, MidoriConfig["Connection_Token"].ToString());
            Console.WriteLine("Sent login information to Discord.");
            await MidoriClient.ConnectAsync();
            Console.WriteLine("Connected.");
            await MidoriClient.DownloadAllUsersAsync();

            // Install command handling
            await CommandHandler.Setup(MidoriDeps);
            Console.WriteLine("Installed commands handler.");

            // Keep the bot running
            await Task.Delay(-1);
        }
    }
}
