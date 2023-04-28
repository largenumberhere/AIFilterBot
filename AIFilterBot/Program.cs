using AIFilterBot.AlpaccaHttp;
using AIFilterBot.Deadcode;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using static OperationResult.Helpers;

namespace AIFilterBot
{
    internal class Program
    {
        public static string AlpaccaHttpAddress = "http://127.0.0.1:1000";
        public AlpccaHttpWrapper AlpccaHttpWrapper { get; } = new AlpccaHttpWrapper(AlpaccaHttpAddress);
        public static ILogger Logger { get; } =
            new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Verbose()
            .CreateLogger();

        public static readonly DataFile DataFile = new DataBuilder<DataFile>()
            .SetPath(Path.Combine(Environment.CurrentDirectory, "data.file"))
            .CreateIfNotExists()
            .PromptIfEmpty()
            .Build();

        public static TaskCompletionSource DiscordSocketClientReady;

        public static DiscordSocketClient DiscordSocketClient = new DiscordSocketClientBuilder()
            .AddConfig(
                new DiscordSocketConfig() { GatewayIntents = GatewayIntents.None | GatewayIntents.MessageContent | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages}
            )
            .RegisterReadyTaskCompletionSource(out DiscordSocketClientReady)
            .AddLogger(Logger)
            .AddBotToken(DataFile.DiscordToken)
            .AddGenericMessageHandler(MessageHandler)
            .Build();

        static AlpccaHttpWrapper _AlpaccaWrapper = new AlpccaHttpWrapper(AlpaccaHttpAddress);
        static MessageCache MessageCache = new MessageCache(new AlpaccaQueue(_AlpaccaWrapper, 10, Logger), Logger);

        private static async Task MessageHandler(SocketMessage message)
        {
            Logger.Debug("requesting response from handler");

            var isOffensive = await MessageCache.GetValue(message.Content, message.Id);
            if(isOffensive == true)
            {
                Logger.Debug("state=false an offensive message was sent! '{0}' by the user {1}'", message.Content,message.Author.Username);
                await message.Channel.SendMessageAsync("Do not say that! It is offensive");
            }
            else
            {
                Logger.Debug("state=true a non-offensive message was sent '{0}' by the user {1}'", message.Content, message.Author.Username);
            }
        }


        static async Task Main(string[] args)
        {
            /*TaskCompletionSource discordSocketClientDisconnected = new TaskCompletionSource();

            DiscordClient.Disconnected += async(a)=> { discordSocketClientDisconnected.TrySetResult(); };
            DiscordClient.Log += async(a)=> { Logger.Write(Serilog.Events.LogEventLevel.Debug, a.Exception, "discord client: Message:{0}, Source:{1}, Level:{2}", a.Message, a.Source, a.Severity.ToString());  };
            await DiscordClient.LoginAsync(Discord.TokenType.Bot, DataFile.DiscordToken, true);
            await DiscordClient.StartAsync();*/

            if( ! await _AlpaccaWrapper.TryConnect())
            {
                throw new Exception("failed to connect to alpacca on "+_AlpaccaWrapper.ServerAddress+" . Please make sure you have alpaccaHttp running. Read README.txt for more details");
            }


            await Task.Delay(1000);
            await DiscordSocketClient.StartAsync();
            await DiscordSocketClientReady.Task;


            await Task.Delay(-1);





            //await Console.Out.WriteLineAsync(DataFile.DiscordToken);

            //throw new Exception();

            //28/04/2023 11:58:05prompting...
            // 13:07:00 - no logging

            /*if (! await AlpccaHttpWrapper.TryConnect()) {
                throw new Exception("failed to connect to alpacca http. It is expected to be accessible at " + AlpccaHttpWrapper.ServerAddress+" . Please make sure it is up an running with the right port and ip"); ;
            }*/

            /*Console.WriteLine("creating queue");
            AlpaccaQueue alpaccaQueue = new AlpaccaQueue(AlpccaHttpWrapper, 10);
            Console.WriteLine("begining queue");
            _= alpaccaQueue.RunService((id) => { Console.WriteLine($"{id} said something offensive!"); });

            await Console.Out.WriteLineAsync("enqueing..");
            await alpaccaQueue.Enqueue("hello there!", 0);

            await Console.Out.WriteLineAsync("enqueing..");
            await alpaccaQueue.Enqueue("fuck you. You're an asshole. Suck on my dick!", 1);


            await Task.Delay(-1);*/

            //var response =  await AlpccaHttpWrapper.PostPrompt("gidday mate!");


            //Console.WriteLine(response.output);


            /*            var cache = new MessageCache(new AlpaccaQueue(AlpccaHttpWrapper,10,Logger), Logger);

                        for (int i = 0; i < 3; i++)
                        {
                            await Console.Out.WriteLineAsync(DateTime.Now.ToString()+ "prompting... ");
                            await Console.Out.WriteLineAsync(
                                DateTime.Now.ToString()+
                                (await cache.GetValue("hello world!", i+1)).ToString()
                            );
                        }

                        await Console.Out.WriteLineAsync("fuck you faggot is offensive? :"+await cache.GetValue("fuck you faggot", -1,default));



                        (Logger as IDisposable)?.Dispose();*/
            ;
            (Logger as IDisposable)?.Dispose();
        }
        


        
    }
}