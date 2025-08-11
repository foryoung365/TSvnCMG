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

            // 检查一下关键信息是否存在
            if (string.IsNullOrEmpty(config?.api_key))
            {
                MessageBox.Show("api_key in missing in Plugin parameters。", "Parameters Error");
                // ... 返回并中断 ...
            }

            // 准备好默认的输出参数
            bugIDOut = bugID;
            revPropNames = null;
            revPropValues = null;

            try
            {
                // 创建一个 SvnClient 实例
                using (SvnClient client = new SvnClient())
                {
                    // diff 的内容将会被写入这个内存流中
                    using (MemoryStream diffStream = new MemoryStream())
                    {
                        SvnDiffArgs args = new SvnDiffArgs();
                        args.IgnoreContentType = true;

                        // 遍历所有待提交的文件
                        foreach (string path in pathList)
                        {
                            // 定义比较的两个版本：
                            // 1. 修改前的版本 (Base)
                            SvnPathTarget baseTarget = new SvnPathTarget(path, SvnRevision.Base);
                            // 2. 当前工作副本中的版本 (Working)
                            SvnPathTarget workingTarget = new SvnPathTarget(path, SvnRevision.Working);

                            // 执行 Diff 操作
                            client.Diff(baseTarget, workingTarget, args, diffStream);
                        }

                        // 将流中的内容转换成字符串
                        diffStream.Position = 0; // 重置流的位置到开头
                        string diffContent = new StreamReader(diffStream, Encoding.UTF8).ReadToEnd();

                        // --- 测试步骤 ---
                        // 弹出一个消息框，显示我们获取到的 diff 内容
                        if (!string.IsNullOrEmpty(diffContent))
                        {
                            MessageBox.Show(diffContent, "文件 Diff 内容");
                        }
                        else
                        {
                            MessageBox.Show("没有检测到修改内容（这通常发生在添加新文件时）。", "提示");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果出错，也弹窗显示错误信息，方便排查
                MessageBox.Show("Error getting Diff:\n" + ex.ToString(), "Plugin Error");
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
