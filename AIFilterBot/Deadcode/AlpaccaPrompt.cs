using CliWrap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFilterBot.Deadcode
{
    // Simply run alpacca with the argument required then it will exit and needs to be restarted on next run
    internal class AlpaccaPrompt
    {
        public SemaphoreSlim AlpacaSemaphore { get; } = new SemaphoreSlim(1);

        string _Path;
        public AlpaccaPrompt(string programPath)
        {
            _Path = programPath;
        }

        public async Task<string> RunWith(string prompt, CancellationToken cancellationToken = default)
        {
            StringBuilder resultBuilder = new StringBuilder();
            string programFolder = Path.GetDirectoryName(_Path);

            await Console.Out.WriteLineAsync(programFolder);

            Command command =
            Cli.Wrap(_Path)
                .WithWorkingDirectory(programFolder)
                .WithStandardErrorPipe(PipeTarget.ToDelegate(Console.WriteLine))
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .WithArguments(new[] { "-p", "\"" + prompt + "\"", "-n", "10" })
                //.WithArguments("-n"+" 10")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(resultBuilder));

            await AlpacaSemaphore.WaitAsync(cancellationToken);
            await command.ExecuteAsync(cancellationToken);
            AlpacaSemaphore.Release();

            return resultBuilder.ToString();
        }

    }
}
