using WaveEngine.Common.Math;

namespace Samples
{
    public static class MatrixExtensions
    {
        public static float[] ToArray(this Matrix matrix)
        {
            var array = new float[16];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = matrix[i];
            }

            return array;
        }
    }
}
