using Neo.IO.Json;
using Neo.Plugins;
using Nxa.Plugins.Persistence;
using Nxa.Plugins.Tasks;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nxa.Plugins
{
    public partial class NXABlockListenerPlugin
    {
        [RpcMethod]
        protected virtual JObject IsWorking(JArray _params)
        {
            RequestConsole(_params);

            JObject result = new JObject();
            result["message"] = String.Format("NXABlockListener active: {0}", taskManager.Active.ToString());
            result["success"] = true;
            return ResponseConsole(result);
        }

        [RpcMethod]
        protected virtual JObject StartBlockListener(JArray _params)
        {
            RequestConsole(_params);
            JObject result = new JObject();

            if (!Settings.Default.TurnedOn)
            {
                result["message"] = "NXABlockListener turned off. Check config file.";
                result["success"] = false;
                ResponseConsole(result);
            }

            if (taskManager != null)
            {
                ConsoleWriter.WriteLine("Starting block listener");
                taskManager.StartBlockListener();
                ConsoleWriter.WriteLine("Block listener started");

                result["success"] = true;
                result["message"] = "Block listener started";
                return ResponseConsole(result);
            }
            else
            {
                result["success"] = false;
                result["message"] = "NXABlockListener cannot start.";
                return ResponseConsole(result);
            }
        }

        [RpcMethod]
        protected virtual JObject StopBlockListener(JArray _params)
        {
            RequestConsole(_params);
            JObject result = new JObject();

            if (taskManager != null)
            {
                ConsoleWriter.WriteLine("Stopping block listener");
                taskManager.StopBlockListener();
            }
            ConsoleWriter.WriteLine("Block listener stopped");

            result["success"] = true;
            result["message"] = "Block listener stopped";
            return ResponseConsole(result);
        }

        [RpcMethod]
        protected virtual JObject GetTaskList(JArray _params)
        {
            RequestConsole(_params);

            TaskState[] taskStates = null;
            TaskType[] taskTypes = null;

            JObject result = new JObject();
            if (_params.Count > 0)
            {
                JObject requestParam = _params[0];
                if (requestParam.ContainsProperty("taskstate") && requestParam["taskstate"] is JArray)
                {
                    taskStates = (requestParam["taskstate"] as JArray).Select(x => Enum.Parse<TaskState>(x.AsString())).ToArray();
                }
                if (requestParam.ContainsProperty("tasktype") && requestParam["tasktype"] is JArray)
                {
                    taskTypes = (requestParam["tasktype"] as JArray).Select(x => Enum.Parse<TaskType>(x.AsString())).ToArray();
                }

                if (taskTypes == null && taskStates == null)
                {
                    result["success"] = false;
                    result["message"] = Messages.RpcParameterError;
                    return ResponseConsole(result);
                }
            }

            var tasks = StorageManager.Manager.GetTasks(taskStates);
            tasks = tasks.Where(x => taskTypes == null ? true : taskTypes.Contains(x.TaskType));

            result["success"] = true;
            result["message"] = new JArray(tasks.Select(x => x.ToJson()));
            return ResponseConsole(result);
        }

        [RpcMethod]
        protected virtual JObject GetTask(JArray _params)
        {
            RequestConsole(_params);

            JObject result = new JObject();
            Guid guid;
            if (_params.Count > 0)
            {
                guid = Guid.Parse(_params[0].AsString());
            }
            else
            {
                result["success"] = false;
                result["message"] = Messages.RpcParameterError;
                return ResponseConsole(result);
            }

            var taskObj = StorageManager.Manager.GetTaskObject(guid);

            result["success"] = taskObj == null ? false : true;
            result["message"] = taskObj == null ? Messages.TaskNotFound : taskObj.ToJson();
            return ResponseConsole(result);
        }

        [RpcMethod]
        protected virtual JObject StartTask(JArray _params)
        {
            RequestConsole(_params);

            JObject result = new JObject();
            Guid guid;
            if (_params.Count > 0)
            {
                guid = Guid.Parse(_params[0].AsString());
            }
            else
            {
                result["success"] = false;
                result["message"] = Messages.RpcParameterError;
                return ResponseConsole(result);
            }

            var taskObj = taskManager.StartTask(guid);

            result["success"] = taskObj == null ? false : true;
            result["message"] = taskObj == null ? Messages.TaskNotFound : taskObj.ToJson();
            return ResponseConsole(result);
        }

        [RpcMethod]
        protected virtual JObject StopTask(JArray _params)
        {
            RequestConsole(_params);

            JObject result = new JObject();
            Guid guid;
            if (_params.Count > 0)
            {
                guid = Guid.Parse(_params[0].AsString());
            }
            else
            {
                result["success"] = false;
                result["message"] = Messages.RpcParameterError;
                return ResponseConsole(result);
            }


            var taskObj = taskManager.StopTask(guid);

            result["success"] = taskObj == null ? false : true;
            result["message"] = taskObj == null ? Messages.TaskNotFoundOrInactive : taskObj.ToJson();
            return ResponseConsole(result);
        }

        [RpcMethod]
        protected virtual JObject CreateTask(JArray _params)
        {
            RequestConsole(_params);

            JObject result = new JObject();
            TaskObject taskObject;
            if (_params.Count > 0)
            {
                taskObject = TaskObject.FromJson(_params[0]);
            }
            else
            {
                result["success"] = false;
                result["message"] = Messages.RpcParameterError;
                return ResponseConsole(result);
            }

            if (taskManager.CreateTask(taskObject))
            {
                taskObject.TaskState = TaskState.Active;    //so we dont return state None if not updated yet
                result["success"] = true;
                result["message"] = taskObject.ToJson();
            }
            else
            {
                result["success"] = false;
                result["message"] = "Error while creating new task";
            }

            return ResponseConsole(result);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RequestConsole(JArray _params, int depth = 1)
        {
            var st = new StackTrace();
            var sf = st.GetFrame(depth);
            string method = sf.GetMethod().Name;
            ConsoleWriter.WriteLine(String.Format("{0} method request parameters: {1}", method, _params.Count == 0 ? "NONE" : _params.AsString()));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private JObject ResponseConsole(JObject response, int depth = 1)
        {
            var st = new StackTrace();
            var sf = st.GetFrame(depth);
            string method = sf.GetMethod().Name;
            ConsoleWriter.WriteLine(String.Format("{0} method response: {1}", method, response.AsString()));
            return response;
        }
    }
}
