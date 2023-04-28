using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFilterBot.AlpaccaHttp
{
    internal class AlpaccaHttpResponse
    {

        public long created { get; set; }
        public string model { get; set; }
        public string output { get; set; }
        public Usage usage { get; set; }
        

        public class Usage
        {
            public int completion_tokens { get; set; }
            public int prompt_tokens { get; set; }
            public int total_tokens { get; set; }
        }
    }
}
