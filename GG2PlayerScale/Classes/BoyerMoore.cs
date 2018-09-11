using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace GG2PlayerScale
{
    static class BoyerMoore
    {
        public static List<long> IndexesOf(this byte[] value, byte[] pattern)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (pattern == null)
                throw new ArgumentNullException("pattern");

            long valueLength = value.LongLength;
            long patternLength = pattern.LongLength;

            if ((valueLength == 0) || (patternLength == 0) || (patternLength > valueLength))
                return (new List<long>());

            long[] badCharacters = new long[256];

            for (long i = 0; i < 256; ++i)
                badCharacters[i] = patternLength;

            long lastPatternByte = patternLength - 1;

            for (long i = 0; i < lastPatternByte; ++i)
                badCharacters[pattern[i]] = lastPatternByte - i;

            // Beginning

            long index = 0;
            List<long> indexes = new List<long>();

            while (index <= (valueLength - patternLength))
            {
                for (long i = lastPatternByte; value[(index + i)] == pattern[i]; --i)
                {
                    if (i == 0)
                    {
                        indexes.Add(index);
                        break;
                    }
                }

                index += badCharacters[value[(index + lastPatternByte)]];
            }

            return indexes;
        }

        public static byte[] SubArray(this byte[] data, int index, int length)
        {
            if (index >= data.Length)
            {
                return null;
            }

            if (length + index > data.Length)
            {
                length = data.Length - index;
            }

            byte[] result = new byte[length];
            
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        private static Dictionary<long[], byte[]> _generatedBuffers = new Dictionary<long[], byte[]>();

        public static int FindOffset(this byte[] data, long[] dataTypes)
        {
            return data.FindOffset(dataTypes, 0);
        }

        public static int FindOffset(this byte[] data, long[] dataTypes, int index)
        {
            byte[] sBuffer = null;
            if(_generatedBuffers.ContainsKey(dataTypes))
            {
                sBuffer = _generatedBuffers[dataTypes];
            }
            else
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                foreach (long dataType in dataTypes)
                {
                    bw.Write(dataType);
                }
                ms.Seek(0, SeekOrigin.Begin);
                sBuffer = new byte[(int)ms.Length];
                ms.Read(sBuffer, 0, (int)ms.Length);
                ms.Close();

                _generatedBuffers.Add(dataTypes, sBuffer);
            }

            List<long> results = data.IndexesOf(sBuffer);
            if(results.Count <= index)
            {
                return -1;
            }

            return (int)results[index];
        }
    }
}
