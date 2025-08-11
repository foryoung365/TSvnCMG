using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;

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
            bugIDOut = bugID;
            revPropNames = null;
            revPropValues = null;
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
