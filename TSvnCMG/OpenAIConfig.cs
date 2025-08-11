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
        public string language { get; set; }

        public int max_tokens { get; set; }
    }

    // --- 这些类用来匹配 OpenAI API 返回的 JSON 结构 ---
    public class OpenAIResponse
    {
        public List<Choice> choices { get; set; }
    }
    public class Choice
    {
        public Message message { get; set; }
    }
    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}
