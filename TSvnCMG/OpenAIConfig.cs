using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSvnCMG
{
    // 这个类用来存储从 parameters 字符串解析出来的配置
    public class OpenAIConfig
    {
        public string api_key { get; set; }
        public string model { get; set; }
        public string base_url { get; set; }
    }
}
