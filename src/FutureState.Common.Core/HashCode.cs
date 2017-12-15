using System.Linq;

namespace FutureState
{
    public static class HashCode
    {
        /// <summary>
        /// A default prime number for hashing: 5381
        /// This is a chosen prime number by .NET Framework.
        /// </summary>
        public const int HashPrime = 5381;

        /// <summary>
        /// The accumulator function of two hashes.
        /// </summary>
        public static int Accumulate(int hash1, int hash2)
        {
            return unchecked(HashPrime + hash1 * HashPrime + hash2);
        }

        /// <summary>
        /// Calculates the compound hash code using the Accumulate method.
        /// </summary>
        /// <param name="objects">An array of objects.</param>
        /// <returns>The compound hash code.</returns>
        public static int Aggregate(params object[] objects)
        {
            var hashes = objects.Select(obj => obj == null ? 0 : obj.GetHashCode()).ToArray();

            return Compound(hashes);
        }

        /// <summary>
        /// Calculates the compound hash code using the Accumulate method.
        /// </summary>
        /// <param name="hashes">An array of hash codes.</param>
        /// <returns>The compound hash code.</returns>
        public static int Compound(params int[] hashes)
        {
            if (hashes == null || hashes.Length == 0)
            {
                return 0;
            }

            return hashes.Aggregate(Accumulate);
        }
    }
}