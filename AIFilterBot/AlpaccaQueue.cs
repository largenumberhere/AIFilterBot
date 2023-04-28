using AIFilterBot.AlpaccaHttp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AIFilterBot
{
    internal class AlpaccaQueue
    {
        const string alpaccaPromptTemplate = "Is '{0}' vulgar? Yes or no.";

        Queue<(ulong,string)> _MessageQueue;
        SemaphoreSlim _MessageQueueSemaphore = new SemaphoreSlim(1);
        AlpccaHttpWrapper _AlpccaHttp;
        ILogger _Logger;
        int _QueueMaxLength;

        public AlpaccaQueue(AlpccaHttpWrapper alpaccaHttp, int queueMaxLength, ILogger logger) {
            _MessageQueue = new Queue<(ulong,string)>();
            _QueueMaxLength = queueMaxLength;
            _AlpccaHttp = alpaccaHttp;
            _Logger = logger;
        }

        public async Task Enqueue(string messageContent,ulong messageId,CancellationToken cancellationToken = default)
        {
            _Logger.Verbose("waiting on semaphoreSlim: " + nameof(_MessageQueueSemaphore));
            await _MessageQueueSemaphore.WaitAsync(cancellationToken);
            bool isQueuePopulated = _MessageQueue.Count > _QueueMaxLength;
            _Logger.Verbose("{0}: {1}",nameof(isQueuePopulated),isQueuePopulated);
            if (isQueuePopulated)
            {
                _MessageQueueSemaphore.Release();
                _Logger.Verbose("{0} released", nameof(_MessageQueueSemaphore));

                while (!cancellationToken.IsCancellationRequested) {
                    await Task.Delay(100);

                    _Logger.Verbose("waiting on semaphoreSlim: " + nameof(_MessageQueueSemaphore));
                    await _MessageQueueSemaphore.WaitAsync(cancellationToken);
                    if (_MessageQueue.Count < _QueueMaxLength)
                    {
                        _MessageQueue.Enqueue((messageId,messageContent));
                        _MessageQueueSemaphore.Release();
                        _Logger.Verbose("{0} released", nameof(_MessageQueueSemaphore));
                        return;
                    }
                
                }
            }
            else
            {
                _MessageQueueSemaphore.Release();
                _Logger.Verbose("{0} released", nameof(_MessageQueueSemaphore));
            }

            _Logger.Verbose("waiting on semaphoreSlim: " + nameof(_MessageQueueSemaphore));
            await _MessageQueueSemaphore.WaitAsync(cancellationToken);
            _MessageQueue.Enqueue((messageId,messageContent));
            _MessageQueueSemaphore.Release();
            _Logger.Verbose("{0} released", nameof(_MessageQueueSemaphore));
        }

        public async Task RunService(Action<ulong> messageOffensiveAction, Action<ulong> messageInoffensiveAction, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {

                await Task.Delay(100);

                string? message = null;
                ulong? messageId = null;

                //_Logger.Verbose("waiting on semaphoreSlim: " + nameof(_MessageQueueSemaphore));
                await _MessageQueueSemaphore.WaitAsync(cancellationToken);
                if(_MessageQueue.Count == 0)
                {
                    //await Console.Out.WriteLineAsync("queue is empty");
                    _MessageQueueSemaphore.Release();
                    //_Logger.Verbose("semaphoreSlim released {0}", nameof(_MessageQueueSemaphore));
                    continue;
                }
                else
                {
                    (messageId, message) = _MessageQueue.Dequeue();
                    _MessageQueueSemaphore.Release();
                   // _Logger.Verbose("semaphoreSlim released {0}", nameof(_MessageQueueSemaphore));
                }

                string alpaccaMessage = string.Format(alpaccaPromptTemplate, message);
                

                _Logger.Verbose("waiting for alpaccaHttp response for '{0}'",alpaccaMessage);
                AlpaccaHttpResponse response = await _AlpccaHttp.PostPrompt(alpaccaMessage);
                _Logger.Verbose("alpaccaHttp response received");


                bool isOffensive = IsYes(response.output);
                _Logger.Verbose("'{0}' meanns it is considered offensive: {1}",response.output ,isOffensive);

                if (isOffensive)
                {
                    _Logger.Verbose("beginning callback on {0} for {1}",nameof(messageOffensiveAction), messageId.GetValueOrDefault());
                    messageOffensiveAction(messageId.Value);
                    _Logger.Verbose("offensive callback finished");
                }
                else
                {
                    _Logger.Verbose("beginning callback on {0} for {1}", nameof(messageInoffensiveAction), messageId.GetValueOrDefault());
                    messageInoffensiveAction(messageId.Value);
                    _Logger.Verbose("inoffensive callback finished");
                }
            }
        }

        private bool IsYes(string rawInput)
        {
            string input = rawInput.ToLower();

            if (input.Contains("yes"))
            {
                return true;
            }

            else return false;
        }
        
    }
}
