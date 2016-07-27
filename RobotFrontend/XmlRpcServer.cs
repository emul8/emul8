//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Text;
using CookComputing.XmlRpc;

namespace Emul8.Robot
{
    internal class XmlRpcServer : XmlRpcListenerService, IDisposable
    {
        public XmlRpcServer(KeywordManager keywordManager)
        {
            this.keywordManager = keywordManager;
        }

        [XmlRpcMethod("get_keyword_names")]
        public string[] GetKeywordNames()
        {
            return keywordManager.GetRegisteredKeywords();
        }

        [XmlRpcMethod("run_keyword")]
        public XmlRpcStruct RunKeyword(string keywordName, string[] arguments)
        {
            var result = new XmlRpcStruct();

            Keyword keyword;
            if(!keywordManager.TryGetKeyword(keywordName, out keyword))
            {
                throw new XmlRpcFaultException(1, string.Format("Keyword \"{0}\" not found", keywordName));
            }

            try
            {
                var keywordResult = keyword.Execute(arguments);
                if(keywordResult != null)
                {
                    result.Add(KeywordResultValue, keywordResult.ToString());
                }
                result.Add(KeywordResultStatus, KeywordResultPass);
            }
            catch(Exception e)
            {
                result.Clear();

                result.Add(KeywordResultStatus, KeywordResultFail);
                result.Add(KeywordResultError, BuildRecursiveErrorMessage(e));
                result.Add(KeywordResultTraceback, e.StackTrace);
            }

            return result;
        }

        [XmlRpcMethod("stop_remote_server")]
        public void Dispose()
        {
            RobotFrontend.RobotFrontend.Shutdown();
        }

        private static string BuildRecursiveErrorMessage(Exception e)
        {
            var result = new StringBuilder();
            while(e != null)
            {
                result.AppendFormat("{0}: {1}\n", e.GetType().Name, e.Message);
                e = e.InnerException;
            }

            return result.ToString();
        }

        private readonly KeywordManager keywordManager;

        private const string KeywordResultValue = "return";
        private const string KeywordResultStatus = "status";
        private const string KeywordResultError = "error";
        private const string KeywordResultTraceback = "traceback";

        private const string KeywordResultPass = "PASS";
        private const string KeywordResultFail = "FAIL";
    }
}

