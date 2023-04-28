using Discord;
using Discord.WebSocket;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFilterBot
{
    /// <summary>
    /// Configure a discordSocketClient and log it in
    /// </summary>
    internal class DiscordSocketClientBuilder
    {
        public DiscordSocketClientBuilder() { }

        string? _BotToken = null;

        public DiscordSocketClientBuilder AddBotToken(string discordBotToken)
        {
            _BotToken = discordBotToken;
            return this;
        }

        ILogger? _Logger = null;
        public DiscordSocketClientBuilder AddLogger(ILogger logger)
        {
            _Logger = logger;
            return this;
        }

        TaskCompletionSource? ReadyTaskCompletionSource = null;
        public DiscordSocketClientBuilder RegisterReadyTaskCompletionSource(out TaskCompletionSource taskCompletionSource)
        {
            if(ReadyTaskCompletionSource == null)
            {
                ReadyTaskCompletionSource = new TaskCompletionSource();
            }
            taskCompletionSource = ReadyTaskCompletionSource;
            return this;
        }


        List<Func<SocketMessage, Task>> GenericMessageHandlers = new List<Func<SocketMessage, Task>>();
        public DiscordSocketClientBuilder AddGenericMessageHandler(Func<SocketMessage,Task> handler)
        {
            GenericMessageHandlers.Add(handler);
            return this;
        }

        public DiscordSocketClientBuilder AddGenericMessageHandler(Action<SocketMessage> hander)
        {
            GenericMessageHandlers.Add((m) => { hander(m); return Task.CompletedTask; });
            return this;
        }


        DiscordSocketConfig? _DiscordConfig = null;    
        public DiscordSocketClientBuilder AddDefaultConfig()
        {
            _DiscordConfig = new DiscordSocketConfig();
            return this;
        }

        public DiscordSocketClientBuilder AddConfig(DiscordSocketConfig config)
        {
            _DiscordConfig = config;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DiscordSocketClient Build()
        {
            DiscordSocketClient client = new DiscordSocketClient(_DiscordConfig);
            if (ReadyTaskCompletionSource != null)
            {
                client.Ready += async () => { ReadyTaskCompletionSource.SetResult(); };
            }

            if(_Logger != null)
            {
                client.Log += CreateDiscordLogger(_Logger);
            }

            if(GenericMessageHandlers.Count > 0)
            {
                client.MessageReceived += async (message) => {
                    foreach (var handler in GenericMessageHandlers)
                    {
                        await handler(message);
                    }
                };
            }

            



            client.LoginAsync(TokenType.Bot, _BotToken, true).GetAwaiter().GetResult();

            
            return client;
        }

        private Func<LogMessage, Task> CreateDiscordLogger(ILogger logger)
        {
            var discordLogger = async (LogMessage logMessage) => {
                LogEventLevel serilogLevel = logMessage.Severity switch
                {
                    LogSeverity.Critical => LogEventLevel.Fatal,
                    LogSeverity.Error => LogEventLevel.Error,
                    LogSeverity.Warning => LogEventLevel.Warning,
                    LogSeverity.Info => LogEventLevel.Information,
                    LogSeverity.Verbose => LogEventLevel.Verbose,
                    LogSeverity.Debug => LogEventLevel.Debug,
                    _ => LogEventLevel.Information
                };

                logger.Write(serilogLevel, logMessage.Exception, "[{Source}] {Message}", logMessage.Source, logMessage.Message);
            };


            return discordLogger;
        }
    }
}
