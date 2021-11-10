namespace Nxa.Plugins.Persistence.Cache
{
    public static class Helper
    {
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

        public static bool CompareByteArray(byte[] array1, byte[] array2)
        {
            if (array1 == null || array2 == null)
                return false;

            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CompareByteArray(byte[] array1, byte[][] array2)
        {
            if (array1 == null || array2 == null)
                return false;

            bool result = false;
            foreach (var arr in array2)
            {
                result = CompareByteArray(array1, arr);
                if (result)
                    return result;
            }
            return result;
        }

    }
}
