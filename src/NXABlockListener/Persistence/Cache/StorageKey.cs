using Neo.Cryptography;
using System;
using System.Text;

namespace Nxa.Plugins.Persistence.Cache
{
    public class StorageKey : IEquatable<StorageKey>
    {

        public byte[] Prefix { get; private set; }
        public byte[] KeyIdentifier { get; set; }

        public byte[] Key
        {
            get
            {
                if (Prefix != null && Prefix.Length > 0)
                {
                    byte[] arr = new byte[Prefix.Length + KeyIdentifier.Length];
                    Buffer.BlockCopy(Prefix, 0, arr, 0, Prefix.Length);
                    Buffer.BlockCopy(KeyIdentifier, 0, arr, Prefix.Length, KeyIdentifier.Length);
                    return arr;
                }
                else
                {
                    return KeyIdentifier;
                }
            }
        }

        public StorageKey()
        {
        }
        public StorageKey(byte[] key)
        {
            //KeyIdentifier = key;
            //Prefix = prefix;
            foreach (var en in Enum.GetValues(typeof(StoragePrefix)))
            {
                var prefix = Encoding.ASCII.GetBytes(en.ToString());

                if (Helper.CheckForPrefix(key, prefix))
                {
                    KeyIdentifier = new byte[key.Length - prefix.Length];
                    Prefix = prefix;
                    Buffer.BlockCopy(key, prefix.Length, KeyIdentifier, 0, KeyIdentifier.Length);
                }

            }
        }
        public StorageKey(byte[] key, StoragePrefix? prefix = null)
        {
            KeyIdentifier = key;
            if (prefix.HasValue)
            {
                Prefix = Encoding.ASCII.GetBytes(prefix.ToString());
            }
        }
        public StorageKey(string key, StoragePrefix? prefix = null)
        {
            KeyIdentifier = Encoding.ASCII.GetBytes(key);
            if (prefix.HasValue)
            {
                Prefix = Encoding.ASCII.GetBytes(prefix.ToString());
            }
        }

        public bool Equals(StorageKey other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return MemoryExtensions.SequenceEqual<byte>(Key, other.Key);
        }
        public override bool Equals(object obj)
        {
            if (obj is not StorageKey other) return false;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Key.Murmur32(0);
        }

        public string Deserialize()
        {
            if (KeyIdentifier == null) return null;
            return Encoding.UTF8.GetString(KeyIdentifier);
        }

        public void Serialize(string key)
        {
            if (key == null) { return; }
            KeyIdentifier = Encoding.ASCII.GetBytes(key);
        }

        public void SetPrefix(StoragePrefix prefix)
        {
            Prefix = Encoding.ASCII.GetBytes(prefix.ToString());
        }


        //private  byte[] CreateSearchPrefix(int id, ReadOnlySpan<byte> prefix)
        //{
        //    byte[] buffer = new byte[sizeof(int) + prefix.Length];
        //    BinaryPrimitives.WriteInt32LittleEndian(buffer, id);
        //    prefix.CopyTo(buffer.AsSpan(sizeof(int)));
        //    return buffer;
        //}
    }
}
