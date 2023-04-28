using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AIFilterBot
{
    internal class MessageCache
    {
        SemaphoreSlim messagesSemaphore;
        List<(bool,string)> _Messages;
        AlpaccaQueue _Queue;
        ILogger _Logger;

        public async Task<(bool, string)[]> GetMessagesCopy() {
            await messagesSemaphore.WaitAsync();
            var data = _Messages.ToArray();
            messagesSemaphore.Release();
            return data;
        }

        public MessageCache(AlpaccaQueue queue,ILogger logger) {
            messagesSemaphore = new SemaphoreSlim(1);
            _Messages = new List<(bool, string)>();
            this._Queue = queue;
            _Logger = logger;
            _ = queue.RunService(Messageoffensive, MessageInoffensive, default);
        }
        public MessageCache(AlpaccaQueue queue,ILogger logger, SemaphoreSlim listSemaphore, List<(bool, string)> list, CancellationToken queueServiceCanellationToken = default) {
            messagesSemaphore = listSemaphore;
            _Messages = list;
            this._Queue = queue;
            _Logger = logger;
            _ = queue.RunService(Messageoffensive, MessageInoffensive, queueServiceCanellationToken);
        }

        public bool IsCached(string value) {
            foreach (var message in _Messages) {
                if(message.Item2 == value) return true;
            }

            return false;
        }
        private bool GetSavedValue(string messageContent)
        {
            foreach (var message in _Messages) {
                if(message.Item2 == messageContent)
                {
                    return message.Item1;
                }
            }

            throw new Exception("not found");
        }

        private Action<ulong> MessageInoffensive => (i)=> {
            TrySetNewItem(i,false);
        };

        private void TrySetNewItem(ulong id, bool isOffensive) {
            for(int i = 0; i < queuedMessageIds.Count; i++)
            {
                var(messageId, taskCompletionSource, itemStatus) = queuedMessageIds[i];
                if (messageId == id)
                {
                    _Logger.Verbose("setting queuedMesasgeId '{0}' (aka [{1}])'s Item2 to {2}", queuedMessageIds[i].GetHashCode(),i, isOffensive);
                    queuedMessageIds[i] = new(messageId, taskCompletionSource, isOffensive);
                    taskCompletionSource.TrySetResult();
                }
            }
        }

        private Action<ulong> Messageoffensive => (i) =>{
            TrySetNewItem(i,true);
        };


        List<(ulong,TaskCompletionSource,bool?)> queuedMessageIds = new List<(ulong, TaskCompletionSource, bool?)> ();

        public async Task<bool> GetValue(string message, ulong messageId, CancellationToken cancellationToken = default)
        {
            bool messageCached = IsCached(message);
            _Logger.Verbose("{0}: {1}", nameof(messageCached), messageCached);
            if (messageCached)
            {
                return GetSavedValue(message);
            }

            TaskCompletionSource taskCompletionSource = new TaskCompletionSource();
            queuedMessageIds.Add((messageId, taskCompletionSource,null));
            await _Queue.Enqueue(message,messageId,cancellationToken);

            _Logger.Debug("waiting on task {0}", nameof(taskCompletionSource));
            await taskCompletionSource.Task;

            //remove the result from the queue
            (ulong, TaskCompletionSource,bool?)? results = null;
            foreach(var pair in queuedMessageIds)
            {
                if(pair.Item1==  messageId)
                {
                    results = pair;
                }
            }
            queuedMessageIds.Remove(results.Value);

            //cache the result 
            await messagesSemaphore.WaitAsync();
            if(!results.HasValue)
            {
                _Logger.Error("results has no value!");
            }
            else
            {
                _Logger.Verbose("value recieved: {0}",results.Value);
            }

            _Messages.Add((results.Value.Item3.Value, message));
            messagesSemaphore.Release();

            //return the result
            return results.Value.Item3.Value;
        }

    }
}
