using Neo.IO.Data.LevelDB;
using Neo.Persistence;
using System;
using System.Collections.Generic;
using System.IO;

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


        #region Dispose

        private bool _disposedValue;

        ~LevelDbStore() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    db?.Dispose();
                }
                _disposedValue = true;
            }
        }

        #endregion
    }
}
