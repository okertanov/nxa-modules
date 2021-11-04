using Neo.ConsoleService;
using Nxa.Plugins.Persistence;
using Nxa.Plugins.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nxa.Plugins
{
    public partial class NXABlockListenerPlugin
    {


        //[ConsoleCommand("search blocks", Category = "BlockListener", Description = "search blocks")]
        //public void SearchBlocks()
        //{

        //    var result = searchTxTest.searchNonEmptyBlocks();
        //}
        //private SearchTxTest searchTxTest;

        [ConsoleCommand("test", Category = "BlockListener", Description = "test stuff")]
        public void test()
        {
            TaskObject taskObject = new TaskObject();

            taskObject = new TaskObject("67e0db06-3a85-48d3-94fd-ee6913df80b0");
            taskObject.ActiveBlock = 0;
            taskObject.TaskState = TaskState.Active;
            taskObject.TaskType = TaskType.BlockListener;
            StorageManager.Manager.AddTaskObject(taskObject);

            taskObject = new TaskObject("f35cd6e5-f523-4dc9-8382-37b511eb9417");
            taskObject.ActiveBlock = 0;
            taskObject.TaskState = TaskState.Canceled;
            taskObject.TaskType = TaskType.BlockListener;
            StorageManager.Manager.AddTaskObject(taskObject);

            taskObject = new TaskObject("15b1fa37-0fdd-41b5-9025-8336b3cbe2b6");
            taskObject.ActiveBlock = 0;
            taskObject.TaskState = TaskState.Active;
            taskObject.TaskType = TaskType.Search;
            taskObject.TaskParameters = new TaskParameters()
            {
                FromBlock = 0,
                ToBlock = 100000,
            };
            StorageManager.Manager.AddTaskObject(taskObject);

            taskObject = new TaskObject("9e717369-3c19-4cbf-905a-3f5ac9c6f22a");
            taskObject.ActiveBlock = 0;
            taskObject.TaskState = TaskState.Finished;
            taskObject.TaskType = TaskType.Search;
            taskObject.TaskParameters = new TaskParameters()
            {
                FromBlock = 0,
                ToBlock = 100000,
            };
            StorageManager.Manager.AddTaskObject(taskObject);

            taskObject = new TaskObject("ed158038-2643-40ed-a25e-d23771e0b9f7");
            taskObject.ActiveBlock = 0;
            taskObject.TaskState = TaskState.Error;
            taskObject.TaskType = TaskType.Search;
            taskObject.TaskParameters = new TaskParameters()
            {
                FromBlock = 0,
                ToBlock = 100000,
            };
            StorageManager.Manager.AddTaskObject(taskObject);

            taskObject = new TaskObject("aba78b59-bdc9-4eea-bbb7-08993eb255e6");
            taskObject.ActiveBlock = 0;
            taskObject.TaskState = TaskState.Canceled;
            taskObject.TaskType = TaskType.Search;
            taskObject.TaskParameters = new TaskParameters()
            {
                FromBlock = 0,
                ToBlock = 100000,
            };
            StorageManager.Manager.AddTaskObject(taskObject);

            taskObject = new TaskObject("76e65bcd-80c9-4d6c-8ae8-26120077e267");
            taskObject.ActiveBlock = 0;
            taskObject.TaskState = TaskState.Active;
            taskObject.TaskType = TaskType.Search;
            taskObject.TaskParameters = new TaskParameters()
            {
                FromBlock = 0,
                ToBlock = 100000,
                SearchJSON = Neo.IO.Json.JObject.Parse("{\"block\":{\"hash\": \"0xf34593eb02437a099cf7f703e23dcf6f585534caa8d1769b6df250abb2f6d718\"},\"Transaction\":{},\"Transfer\":{},\"SCDeployment\":{}}")
            };
            StorageManager.Manager.AddTaskObject(taskObject);
        }


        //[ConsoleCommand("start task", Category = "BlockListener", Description = "start task")]
        //public void startTask()
        //{
        //    //new search task guid 
        //    //{95b514e5-a2fc-496a-bcce-01d7d90a3162}

        //    taskManager.StartTask(new Guid("95b514e5-a2fc-496a-bcce-01d7d90a3162"));
        //}

        //[ConsoleCommand("stop task", Category = "BlockListener", Description = "stop task")]
        //public void stopTask()
        //{
        //    //new search task guid 
        //    //{95b514e5-a2fc-496a-bcce-01d7d90a3162}

        //    taskManager.StopTask(new Guid("95b514e5-a2fc-496a-bcce-01d7d90a3162"));
        //}

        //[ConsoleCommand("create task", Category = "BlockListener", Description = "create task")]
        //public void createTask()
        //{
        //    //new search task guid 
        //    //{95b514e5-a2fc-496a-bcce-01d7d90a3162}

        //    TaskObject taskObject = new TaskObject("95b514e5-a2fc-496a-bcce-01d7d90a3162");
        //    taskObject.ActiveBlock = 0;
        //    taskObject.TaskType = TaskType.Search;
        //    taskObject.TaskState = TaskState.None;
        //    taskObject.TaskParameters = new TaskParameters()
        //    {
        //        FromBlock = 0,
        //        ToBlock = 100000
        //    };
        //    taskObject.TaskParameters.SearchJSON = JObject.Parse("{\"block\":{\"hash\":\"0xf34593eb02437a099cf7f703e23dcf6f585534caa8d1769b6df250abb2f6d718\"},\"transaction\":{},\"transfer\":{},\"scdeployment\":{}}");

        //    //StorageManager.Manager.AddTaskObject(taskObject);
        //    //var obj = StorageManager.Manager.GetTaskObject(taskObject.Id);

        //    this.taskManager.CreateTask(taskObject);
        //}
    }
}
