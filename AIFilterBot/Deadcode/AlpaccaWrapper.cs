using CliWrap;
using CliWrap.EventStream;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFilterBot.Deadcode
{
    internal class AlpaccaWrapper : IDisposable
    {
        DataFile _DataFile;
        string _WorkingDirectory;
        MemoryStream _CliIn = new MemoryStream();
        MemoryStream _CliOut = new MemoryStream();

        public Task CommandTask;
        TaskCompletionSource TaskCompletionSource = new TaskCompletionSource();

        public AlpaccaWrapper(DataFile dataFile, string workingDirectory)
        {
            _DataFile = dataFile;
            _WorkingDirectory = workingDirectory;
        }

        //private Queue<string> OutputQueue = new Queue<string>();

        public async Task BeginAsync(CancellationToken cancellationToken)
        {
            using TextWriter _writer = new StreamWriter(_CliIn);
            _writer.Write("hello mate!");


            string path = Path.Combine(_WorkingDirectory, "alpaca-win", "chat.exe");
            await Console.Out.WriteLineAsync(path);
            await Console.Out.WriteLineAsync(Path.GetDirectoryName(path));
            Command command =
                Cli.Wrap(path)
                .WithArguments((a) =>
                {
                    a.Add("-i");
                    a.Build();
                })
                .WithWorkingDirectory(Path.GetDirectoryName(path))
                .WithStandardInputPipe(PipeSource.FromStream(_CliIn))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(Console.Error.WriteLine))
                .WithStandardOutputPipe(PipeTarget.ToDelegate((o) =>
                {
                    if (!TaskCompletionSource.Task.IsCompleted)
                    {
                        Console.WriteLine(o);
                    }

                }));


            //.WithValidation(CommandResultValidation.ZeroExitCode);

            /*a => {
            a.Add("--interactive-start");
            a.Build();
        });*/


            await Console.Out.WriteLineAsync(JsonConvert.SerializeObject(command));

            IObservable<CommandEvent> observable = command.Observe(default);


            //CommandTask = command.ExecuteAsync(cancellationToken);
            //command.ListenAsync(cancellationToken);
        }





        private async Task WaitForStarted()
        {
            await TaskCompletionSource.Task;
        }

        public async Task Prompt(string data, CancellationToken cancellationToken = default)
        {
            await WaitForStarted();
            cancellationToken.ThrowIfCancellationRequested();

            using TextWriter textWriter = new StreamWriter(_CliIn);
            await textWriter.WriteAsync(data);
        }

        public void Dispose()
        {
            _CliOut.Dispose();
            _CliIn.Dispose();
        }
    }
}
