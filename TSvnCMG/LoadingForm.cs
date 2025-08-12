using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;

namespace TSvnCMG
{
    public partial class LoadingForm : Form
    {
        private readonly OpenAIConfig _config;
        private readonly string _diffContent;
        public string AiMessage { get; private set; }

        public LoadingForm(OpenAIConfig config, string diffContent)
        {
            InitializeComponent();
            _config = config;
            _diffContent = diffContent;
            this.Hint.Text = "Generating Commit Message from " + config.base_url + "chat/completions.\r\n using model: " + config.model +
                        ".\r\nThis may take a few seconds, please wait...";
        }

        private async void LoadingForm_Load(object sender, EventArgs e)
        {
            AiMessage = await GetAiResponseAsync();
            // 任务完成，关闭窗口
            this.Close();
        }

        private async Task<string> GetAiResponseAsync()
        {
            string resultMessage = null;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.api_key);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string defualt_prompt = $@"You are an expert programmer. Your task is to write a concise and informative commit message based on the provided code diff. The message should be in {_config.language}.";

                    var requestBody = new Dictionary<string, object>
                    {
                        { "model", _config.model },
                        { "stream", false },
                        { "messages", new[]
                            {
                                new { role = "system", content = defualt_prompt },
                                new { role = "user", content = _diffContent }
                            }
                        }
                    };

                    // 如果 _config.max_tokens 设置了并且大于 0，才添加它
                    if (_config.max_tokens > 0)
                    {
                        requestBody.Add("max_tokens", _config.max_tokens);
                    }

                    var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    var requestUrl = new Uri(new Uri(_config.base_url), "chat/completions");

                    var request = new HttpRequestMessage(HttpMethod.Post, requestUrl) { Content = content };
                    var response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var openAIResponse = JsonConvert.DeserializeObject<OpenAIResponse>(jsonResponse);
                        string rawMessage = openAIResponse?.choices?.FirstOrDefault()?.message?.content; // 先不 Trim()

                        if (!string.IsNullOrEmpty(rawMessage))
                        {
                            // 使用正则表达式移除 <think>...</think> 标签及其中的所有内容
                            // RegexOptions.Singleline 确保可以处理跨行的标签
                            // 最后再 Trim() 清理首尾空格和换行
                            resultMessage = Regex.Replace(rawMessage, @"<think>.*?</think>", "", RegexOptions.Singleline).Trim();
                        }
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"API request Failed: {response.StatusCode}\n{errorContent}", "API Error");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("API request Failed:\n" + ex.ToString(), "Plugin Error");
            }
            return resultMessage;
        }
    }
}