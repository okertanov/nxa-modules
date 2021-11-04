using Neo.IO.Data.LevelDB;
using Neo.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nxa.Plugins.Persistence.Db
{
    public class LevelDbStore : IStore
    {
        private readonly DB db;
        public LevelDbStore(string path)
        {
            db = DB.Open(Path.GetFullPath(path), new Options { CreateIfMissing = true });
        }

        public void Delete(byte[] key)
        {
            db.Delete(WriteOptions.Default, key);
        }

        public void Put(byte[] key, byte[] value)
        {

            db.Put(WriteOptions.Default, key, value);
        }

        public bool Contains(byte[] key)
        {
            return db.Contains(ReadOptions.Default, key);
        }

        public IEnumerable<(byte[] Key, byte[] Value)> Seek(byte[] prefix, SeekDirection direction = SeekDirection.Forward)
        {
            return db.Seek(ReadOptions.Default, prefix, (k, v) => (k, v));
        }

        public byte[] TryGet(byte[] key)
        {
            return db.Get(ReadOptions.Default, key);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
