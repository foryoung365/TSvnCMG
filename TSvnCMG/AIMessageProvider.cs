using System;
using SharpSvn;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Newtonsoft.Json;
using LibGit2Sharp;
using System.Linq;

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
            return "AI Message";
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
                // 智能检测仓库类型
                // 我们通过 LibGit2Sharp 的 Discover 功能来判断是不是 Git 仓库
                string gitRepoPath = Repository.Discover(pathList[0]);

                if (gitRepoPath != null)
                {
                    // 这是 Git 仓库
                    diffContent = GetGitDiff(pathList);
                }
                else
                {
                    // 否则，我们假定它是 SVN 仓库 (也可以增加一个 DiscoverSvnRoot 的类似检查)
                    // 在我们的场景里，如果不是Git，那就只能是Svn
                    diffContent = GetSvnDiff(pathList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Getting diff error:\n" + ex.ToString(), "Plugin Error");
                return originalMessage;
            }

            if (string.IsNullOrWhiteSpace(diffContent))
            {
                return originalMessage;
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

        private string GetGitDiff(string[] pathList)
        {
            // 即使 pathList 为空，我们仍然可以尝试 diff 整个仓库
            if (pathList == null || pathList.Length == 0)
            {
                return "// No files selected for commit.";
            }

            string repoPath = Repository.Discover(pathList[0]);
            if (repoPath == null)
            {
                return "// Could not discover repository from the provided file paths.";
            }

            using (var repo = new Repository(repoPath))
            {
                Tree headTree = repo.Head.Tip?.Tree;

                // 我们不再传递 pathList，而是直接比较整个工作目录和 HEAD
                // 这会获取所有已暂存和未暂存的变更，正是提交时需要的内容
                var patch = repo.Diff.Compare<Patch>(headTree, DiffTargets.Index);

                return patch.Content;
            }
        }

        // 我们将现有的 SVN diff 逻辑封装成一个方法
        private string GetSvnDiff(string[] pathList)
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
                    return new StreamReader(diffStream, Encoding.UTF8).ReadToEnd();
                }
            }
        }
    }
}
