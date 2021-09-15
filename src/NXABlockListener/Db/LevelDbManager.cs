using Neo.IO.Data.LevelDB;
using System;
using System.IO;
using System.Text;

namespace Nxa.Plugins.Db
{
    public class LevelDbManager : IDisposable
    {
        private DB db;

        private const string rmq_block_index = "rmq_block_index";
        private byte[] rmq_block_index_key;

        public LevelDbManager()
        {
            rmq_block_index_key = Encoding.ASCII.GetBytes(rmq_block_index);
            string path = string.Format(Plugins.Settings.Default.Db.Path, Plugins.Settings.Default.Network.ToString("X8"));
            db = DB.Open(Path.GetFullPath(path), new Options { CreateIfMissing = true });
        }

        public uint GetRMQBlockIndex()
        {
            var value = db.Get(ReadOptions.Default, rmq_block_index_key);
            if (value == null)
                return 0;
            else
                return BitConverter.ToUInt32(value, 0);
        }

        public void SetRMQBlockIndex(uint index)
        {
            var value = BitConverter.GetBytes(index);
            db.Put(WriteOptions.Default, rmq_block_index_key, value);
        }

        #region dispose
        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void dispose(bool disposing)
        {
            if(disposing)
            {
                db.Dispose();
                db = null;
            }
        }

        #endregion
    }
}
