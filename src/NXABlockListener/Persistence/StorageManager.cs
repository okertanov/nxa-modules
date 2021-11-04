using Neo.Persistence;
using Nxa.Plugins.Persistence.Cache;
using Nxa.Plugins.Persistence.Db;
using Nxa.Plugins.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Nxa.Plugins.Persistence
{
    public enum StoragePrefix : byte
    {
        TaskState,
        TaskBlock,
        TaskParam,
        TaskType
    }

    public class StorageManager : IDisposable
    {
        private readonly StorageCache storage;
        public static StorageManager Manager { get; private set; }

        public StorageManager(string path)
        {
            storage = new StorageCache(new LevelDbStore(path));
        }

        public static void Load(string path = null)
        {
            if (Manager != null)
            {
                Manager.storage?.Dispose();
            }

            if (String.IsNullOrEmpty(path))
            {
                path = string.Format(Plugins.Settings.Default.Db.Path, Plugins.Settings.Default.Network.ToString("X8"));
            }
            Manager = new StorageManager(path);
        }
        public void Dispose()
        {
            storage?.Dispose();
        }


        public void AddTaskObject(TaskObject taskObject)
        {
            storage.AddOrUpdate(new StorageKey(taskObject.Id.ToByteArray(), StoragePrefix.TaskParam)
                , taskObject.SerializeTaskParameters());

            storage.AddOrUpdate(new StorageKey(taskObject.Id.ToByteArray(), StoragePrefix.TaskState)
                , taskObject.SerializeTaskState());

            storage.AddOrUpdate(new StorageKey(taskObject.Id.ToByteArray(), StoragePrefix.TaskBlock)
                , taskObject.SerializeActiveBlock());

            storage.AddOrUpdate(new StorageKey(taskObject.Id.ToByteArray(), StoragePrefix.TaskType)
                , taskObject.SerializeTaskType());
        }
        public void UpdateTaskActiveBlock(TaskObject taskObject)
        {
            storage.AddOrUpdate(new StorageKey(taskObject.Id.ToByteArray(), StoragePrefix.TaskBlock)
                , taskObject.SerializeActiveBlock());
        }
        public void UpdateTaskState(TaskObject taskObject)
        {
            storage.AddOrUpdate(new StorageKey(taskObject.Id.ToByteArray(), StoragePrefix.TaskState)
                , taskObject.SerializeTaskState());
        }


        public TaskObject GetTaskObject(Guid guid)
        {
            return GetTaskObject(guid.ToByteArray());
        }
        public TaskObject GetTaskObject(TaskObject taskObject)
        {
            return GetTaskObject(taskObject.Id.ToByteArray());
        }
        public TaskObject GetTaskObject(byte[] guid)
        {
            if (!CheckIfKeyExist(guid))
                return null;

            TaskObject taskObject = new TaskObject(new Guid(guid));
            taskObject.DeserializeTaskParameters(storage.TryGet(new StorageKey(guid, StoragePrefix.TaskParam)));

            taskObject.DeserializeTaskState(storage.TryGet(new StorageKey(guid, StoragePrefix.TaskState)));

            taskObject.DeserializeActiveBlock(storage.TryGet(new StorageKey(guid, StoragePrefix.TaskBlock)));

            taskObject.DeserializeTaskType(storage.TryGet(new StorageKey(guid, StoragePrefix.TaskType)));

            return taskObject;
        }


        public bool CheckIfKeyExist(Guid guid)
        {
            return CheckIfKeyExist(guid.ToByteArray());
        }
        public bool CheckIfKeyExist(TaskObject taskObject)
        {
            return CheckIfKeyExist(taskObject.Id.ToByteArray());
        }
        public bool CheckIfKeyExist(byte[] guid)
        {
            return storage.Contains(new StorageKey(guid, StoragePrefix.TaskType));
        }


        public IEnumerable<TaskObject> GetTasks(TaskState[] taskStates = null)
        {
            var prefix = Encoding.ASCII.GetBytes(StoragePrefix.TaskState.ToString());
            var values = taskStates == null ? null : taskStates.Select(x => BitConverter.GetBytes((int)x)).ToArray();

            return storage.Seek(prefix, values).Select(x => GetTaskObject(x.Key.KeyIdentifier));
        }

    }
}
