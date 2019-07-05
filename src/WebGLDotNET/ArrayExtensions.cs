using System;

namespace WebGLDotNET
{
    public static class ArrayExtensions
    {
        public static T[] ToArray<T>(this WebAssembly.Core.Array array, Func<object, T> cast)
        {
            var length = array.Length;
            var result = new T[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = cast(array[i]);
            }

            return result;
        }
    }
}
