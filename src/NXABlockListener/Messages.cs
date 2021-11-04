using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nxa.Plugins
{
    public static class Messages
    {
        #region BlockListener main messages

        public static readonly string BlockListener_IsWorking = "NXABlockListener active: {0}";
        public static readonly string BlockListener_TurnedOff = "NXABlockListener turned off. Check config file.";
        public static readonly string BlockListener_Starting = "Starting NXABlockListener listener";
        public static readonly string BlockListener_Started = "NXABlockListener listener started";

        #endregion


        #region Rpc messages
        public static readonly string testc = "";

        #endregion

        #region Error messages

        public static readonly string RpcParameterError = "Parameter error";
        
        public static readonly string RpcNotFound = "Object not found";

        public static readonly string TaskNotFound = "Task not found";

        public static readonly string TaskNotFoundOrInactive = "Task is not active";

        #endregion


    }
}
