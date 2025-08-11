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


                    var requestBody = new
                    {
                        model = _config.model,
                        stream = false,
                        max_tokens = _config.max_tokens,
                        messages = new[]
                        {
                            new { role = "system", content = $@"# 角色
你是一个经验丰富、严谨细致的资深软件工程师，非常擅长撰写清晰、规范、信息丰富的 SVN Commit Message。你的目标是根据代码的变更（svn diff）内容，生成符合团队规范的提交信息。

# 工作流程
1.  **分析输入**: 仔细分析 `### svn diff ###` 部分提供的代码变更。
2.  **识别意图**: 理解这次提交的核心目的，将其归类为以下几种类型(type)之一：
    * `feat`: 引入一个新功能 (feature)
    * `fix`: 修复一个缺陷 (bug fix)
    * `docs`: 只涉及文档的修改 (documentation)
    * `style`: 不影响代码含义的修改 (空格、格式化、缺少分号等)
    * `refactor`: 代码重构，既不是修复 bug 也不是添加新功能
    * `perf`: 提升性能的修改 (performance)
    * `test`: 添加或修改测试用例
    * `chore`: 构建流程、辅助工具等不涉及生产代码的变更
3.  **提取关键信息**:
    * **影响范围 (scope)**: 变更主要影响了哪个模块或组件？例如：`user-auth`, `api-client`, `payment-gateway`。如果影响范围广泛，可以省略。
    * **主题 (subject)**: 用一句话简明扼要地概括变更内容，使用祈使句（例如，使用 ""添加"" 而不是 ""添加了""）。如果这次提交有多个目的，则重复上述流程。
4.  **生成提交信息**: 严格按照 `### 输出格式 ###` 部分定义的结构，生成最终的 Commit Message。

# 输出格式
你的输出必须严格遵循以下格式，不要包含任何额外的解释或说明文字。

```
<type1>(<scope1>): <subject1>
<type2>(<scope2>): <subject2>
...
```

`change`: `type(scope): subject` 是必需的。`scope` 是可选的。`:` 后面必须有一个空格。整个 change 行不能超过 50 个字符。每个change行直接不允许添加空行。


# 示例

**示例 1: 修复 Bug**

### svn diff ###
```diff
--- a/src/utils/math.js
+++ b/src/utils/math.js
@@ -1,4 +1,4 @@
 function add(a, b) {{
-  return a - b; // Error: was subtracting instead of adding
+  return a + b;
 }}
 module.exports = {{ add }};
### 期望输出
fix(utils): 修复 add 函数错误的计算逻辑

该函数本应执行加法操作，但错误地实现了减法。
此变更将 `-` 运算符更正为 `+`，确保计算结果正确。

# 语言要求
最终的 Commit Message应使用{_config.language}回复。
" },
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