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
            this.Hint.Text = "Get Response from " + config.base_url + "chat/completions, using model:" + config.model +
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

                    // 注意：这里我们不再请求流式响应 (stream: false)
                    var requestBody = new
                    {
                        model = _config.model,
                        stream = false,
                        messages = new[]
                        {
                            new { role = "system", content = "You are an expert programmer. Your task is to write a concise and informative commit message based on the provided code diff. The message should be in English." },
                            new { role = "user", content = _diffContent }
                        }
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    var requestUrl = new Uri(new Uri(_config.base_url), "chat/completions");

                    var request = new HttpRequestMessage(HttpMethod.Post, requestUrl) { Content = content };
                    var response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var openAIResponse = JsonConvert.DeserializeObject<OpenAIResponse>(jsonResponse);
                        resultMessage = openAIResponse?.choices?.FirstOrDefault()?.message?.content?.Trim();
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"API 请求失败: {response.StatusCode}\n{errorContent}", "API 错误");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("调用 AI API 时出错:\n" + ex.ToString(), "插件错误");
            }
            return resultMessage;
        }
    }
}