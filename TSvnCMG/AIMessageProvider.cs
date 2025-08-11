using System;
using SharpSvn;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Newtonsoft.Json;


namespace TSvnCMG
{
    [ComVisible(true),
    Guid("BCEDE820-24C2-444E-AF71-83F197705116"),
        ClassInterface(ClassInterfaceType.None)]
    public class AIMessageProvider : Interop.BugTraqProvider.IBugTraqProvider2, Interop.BugTraqProvider.IBugTraqProvider
    {


        public bool ValidateParameters(IntPtr hParentWnd, string parameters)
        {
            return true;
        }

        public string GetLinkText(IntPtr hParentWnd, string parameters)
        {
            return "AI commit Message";
        }

        public string GetCommitMessage(IntPtr hParentWnd, string parameters, string commonRoot, string[] pathList,
                                       string originalMessage)
        {
            string[] revPropNames = new string[0];
            string[] revPropValues = new string[0];
            string dummystring = "";
            return GetCommitMessage2(hParentWnd, parameters, "", commonRoot, pathList, originalMessage, "", out dummystring, out revPropNames, out revPropValues);
        }

        public string GetCommitMessage2(IntPtr hParentWnd, string parameters, string commonURL, string commonRoot, string[] pathList,
                               string originalMessage, string bugID, out string bugIDOut, out string[] revPropNames, out string[] revPropValues)
        {
            // 准备好默认的输出参数
            bugIDOut = bugID;
            revPropNames = null;
            revPropValues = null;

            // --- 新增的解析代码 ---
            OpenAIConfig config = null;
            try
            {
                // 使用 System.Text.Json 解析参数字符串
                config = JsonConvert.DeserializeObject<OpenAIConfig>(parameters);
            }
            catch (Exception ex)
            {
                // 如果用户的 JSON 格式错误，则弹窗提示
                MessageBox.Show("Plugin parameters (JSON) invalid: " + ex.Message + "parameters:" + parameters, "Parameters Error");

                // 返回原始信息，中断后续操作
                bugIDOut = bugID;
                revPropNames = null;
                revPropValues = null;
                return originalMessage;
            }



            // 2. 获取 Diff 内容
            string diffContent = "";
            try
            {
                using (SvnClient client = new SvnClient())
                {
                    using (MemoryStream diffStream = new MemoryStream())
                    {
                        SvnDiffArgs args = new SvnDiffArgs { IgnoreContentType = true };
                        foreach (string path in pathList)
                        {
                            client.Diff(new SvnPathTarget(path, SvnRevision.Base), new SvnPathTarget(path, SvnRevision.Working), args, diffStream);
                        }
                        diffStream.Position = 0;
                        diffContent = new StreamReader(diffStream, Encoding.UTF8).ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取 Diff 时出错:\n" + ex.ToString(), "插件错误");
                return originalMessage;
            }

            if (string.IsNullOrWhiteSpace(diffContent))
            {
                return originalMessage; // 如果没有变更内容，则不做任何事
            }

            // 3. 创建并显示加载窗口，然后等待它关闭
            var loadingForm = new LoadingForm(config, diffContent);
            loadingForm.ShowDialog(); // 这会打开加载窗口，并在它关闭前一直“卡”在这里

            // 4. 从加载窗口取回 AI 生成的结果
            if (!string.IsNullOrEmpty(loadingForm.AiMessage))
            {
                return loadingForm.AiMessage; // 返回AI消息
            }


            // 暂时还是返回原始信息，不改变用户的提交信息
            return originalMessage;
        }

        public string CheckCommit(IntPtr hParentWnd, string parameters, string commonURL, string commonRoot, string[] pathList, string commitMessage)
        {
            return null;
        }

        public string OnCommitFinished(IntPtr hParentWnd, string commonRoot, string[] pathList, string logMessage, int revision)
        {
            return null;
        }

        public bool HasOptions()
        {
            return true;
        }

        public string ShowOptionsDialog(IntPtr hParentWnd, string parameters)
        {
            return null;
        }

    }
}
