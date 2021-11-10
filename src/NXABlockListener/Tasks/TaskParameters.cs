using Neo.IO.Json;
using System.IO;

namespace Nxa.Plugins.Tasks
{
    public class TaskParameters
    {
        public TaskParameters()
        {
            SearchJSON = new JObject();
        }

        public uint FromBlock { get; set; }
        public uint ToBlock { get; set; }

        public JObject SearchJSON { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(FromBlock);
                    writer.Write(ToBlock);
                    writer.Write(SearchJSON.AsString());
                }
                return m.ToArray();
            }
        }

        public void Deserialize(byte[] arr)
        {
            if (arr == null)
                return;
            using (MemoryStream m = new MemoryStream(arr))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    FromBlock = reader.ReadUInt32();
                    ToBlock = reader.ReadUInt32();
                    SearchJSON = JObject.Parse(reader.ReadString());
                }
            }
        }

        public JObject ToJson()
        {
            JObject jObject = new JObject();
            jObject["SearchJSON"] = SearchJSON;
            jObject["FromBlock"] = FromBlock;
            jObject["ToBlock"] = ToBlock;

            return jObject;
        }

        public static TaskParameters FromJson(JObject jObject)
        {
            TaskParameters taskParameters = new TaskParameters();

            if (!jObject.ContainsProperty("FromBlock"))
                return null;
            if (!jObject.ContainsProperty("ToBlock"))
                return null;
            if (!jObject.ContainsProperty("SearchJSON"))
                return null;

            taskParameters.FromBlock = (uint)(jObject["FromBlock"].AsNumber());
            taskParameters.ToBlock = (uint)(jObject["ToBlock"].AsNumber());
            taskParameters.SearchJSON = jObject["SearchJSON"];

            return taskParameters;
        }
    }
}
