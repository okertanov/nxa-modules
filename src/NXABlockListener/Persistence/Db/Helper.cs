using Neo.IO.Data.LevelDB;
using Neo.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nxa.Plugins.Persistence.Db
{
    public static class Helper
    {
        public static IEnumerable<T> Seek<T>(this DB db, ReadOptions options, byte[] prefix, Func<byte[], byte[], T> resultSelector)
        {
            using Iterator it = db.NewIterator(options);

            for (it.Seek(prefix); it.Valid() && CheckForPrefix(it.Key(), prefix); it.Next())
                yield return resultSelector(it.Key(), it.Value());

        }

        public static bool CheckForPrefix(byte[] array, byte[] prefix)
        {
            if (array == null || prefix == null)
                return false;

            if (prefix.Length > array.Length)
                return false;

            for (int i = 0; i < prefix.Length; i++)
            {
                if (array[i] != prefix[i])
                {
                    return false;
                }
            }
            return true;
        }

    }
}
