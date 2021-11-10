using Neo.IO.Json;
using System;

namespace Nxa.Plugins.Tasks
{
    public enum TaskState
    {
        None = 0,
        Active = 1,
        Finished = 3,
        Canceled = 10,
        Error = 11,

    }

    public enum TaskType
    {
        BlockListener = 0,
        Search = 1
    }

    public class TaskObject
    {
        public TaskObject()
        {
            Id = Guid.NewGuid();
        }
        public TaskObject(Guid guid)
        {
            Id = guid;
        }
        public TaskObject(string guid)
        {
            Id = Guid.Parse(guid);
        }

        public TaskParameters TaskParameters { get; set; } = new TaskParameters();

        public Guid Id { get; set; }
        public TaskState TaskState { get; set; }
        public TaskType TaskType { get; set; }

        private uint _activeBlock = 0;
        public uint ActiveBlock
        {
            get
            {
                if (_activeBlock < TaskParameters.FromBlock)
                {
                    _activeBlock = TaskParameters.FromBlock;
                }
                return _activeBlock;
            }
            set
            {
                _activeBlock = value;
            }

        }

        public JObject ToJson()
        {
            JObject jObject = new JObject();
            jObject["Id"] = Id.ToString();
            jObject["TaskState"] = TaskState.ToString();
            jObject["TaskType"] = TaskType.ToString();
            jObject["ActiveBlock"] = ActiveBlock;
            jObject["TaskParameters"] = TaskParameters.ToJson();

            return jObject;
        }

        public static TaskObject FromJson(JObject jObject)
        {
            TaskObject taskObject = new TaskObject();

            if (!jObject.ContainsProperty("TaskType"))
                return null;
            if (!jObject.ContainsProperty("ActiveBlock"))
                return null;

            taskObject.TaskType = Enum.Parse<TaskType>(jObject["TaskType"].AsString());
            taskObject.ActiveBlock = (uint)(jObject["ActiveBlock"].AsNumber());

            if (taskObject.TaskType == TaskType.Search && jObject.ContainsProperty("TaskParameters"))
            {
                taskObject.TaskParameters = TaskParameters.FromJson(jObject["TaskParameters"]);
                if (taskObject.TaskParameters == null)
                {
                    return null;
                }
            }

            return taskObject;
        }

        #region serialization for objects
        public byte[] SerializeTaskState()
        {
            return BitConverter.GetBytes((int)TaskState);
        }
        public byte[] SerializeTaskType()
        {
            return BitConverter.GetBytes((int)TaskType);
        }
        public byte[] SerializeActiveBlock()
        {
            return BitConverter.GetBytes(ActiveBlock);
        }
        public byte[] SerializeTaskParameters()
        {
            return TaskParameters.Serialize();
        }

        public void DeserializeTaskState(byte[] arr)
        {
            if (arr == null)
                return;

            TaskState = (TaskState)BitConverter.ToInt32(arr);
        }
        public void DeserializeTaskType(byte[] arr)
        {
            if (arr == null)
                return;
            TaskType = (TaskType)BitConverter.ToInt32(arr);
        }
        public void DeserializeActiveBlock(byte[] arr)
        {
            if (arr == null)
                return;
            ActiveBlock = BitConverter.ToUInt32(arr);
        }
        public void DeserializeTaskParameters(byte[] arr)
        {
            if (arr == null)
                return;
            if (TaskParameters == null)
                TaskParameters = new TaskParameters();

            TaskParameters.Deserialize(arr);
        }

        #endregion
    }

}
