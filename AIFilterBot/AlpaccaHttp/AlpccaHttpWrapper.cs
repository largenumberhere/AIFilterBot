using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFilterBot.AlpaccaHttp
{
    internal class AlpccaHttpWrapper
    {

        HttpClient _HttpClient;
        SemaphoreSlim _SemaphoreSlim;

        public string ServerAddress { get; }
        
        public AlpccaHttpWrapper(string serverAddress)
        {
            ServerAddress = serverAddress;
            _HttpClient = new HttpClient();
            _SemaphoreSlim = new SemaphoreSlim(1);

        }

        AlpccaHttpWrapper(string serverAddress, HttpClient httpClient, SemaphoreSlim semaphoreSlim)
        {
            ServerAddress = serverAddress;
            _HttpClient = httpClient;
            _SemaphoreSlim = semaphoreSlim;
        }

        public async Task<bool> TryConnect(CancellationToken cancellationToken = default)
        {
            
            HttpResponseMessage? response = null;
            try
            {
                response = await _HttpClient.GetAsync(ServerAddress);
            }
            catch(HttpRequestException ex)
            {
                // alpacca http just abadons the connection if it has no Text query
                if (ex.InnerException?.Message == "The response ended prematurely.")
                {
                    return true;
                }

                return false;
            }
              
            return false;
        }

        public async Task<AlpaccaHttpResponse> PostPrompt(string promptText, CancellationToken cancellationToken = default)
        {
            await _SemaphoreSlim.WaitAsync();


            string uri;
            {
                StringBuilder uriBuilder = new StringBuilder();

                uriBuilder.Append(ServerAddress);
                uriBuilder.Append("?text=");
                uriBuilder.Append(promptText);
                uri = uriBuilder.ToString();
            }

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            HttpResponseMessage rawResponse = await _HttpClient.SendAsync(httpRequestMessage, cancellationToken);
            _SemaphoreSlim.Release();

            string json = await rawResponse.Content.ReadAsStringAsync();
            var response =  JsonConvert.DeserializeObject<AlpaccaHttpResponse>(json);
            return response;

        }





    }
}
