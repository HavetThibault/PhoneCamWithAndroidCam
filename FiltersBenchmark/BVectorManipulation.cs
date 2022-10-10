using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FiltersBenchmark
{
    public class BVectorManipulation
    {
        private byte[] array1;
        private byte[] array2;
        private byte[] result;
        private Vector<byte> byteVector1;
        private Vector<byte> byteVector2;

        [GlobalSetup]
        public void InitVector()
        {
            result = new byte[32];
            array1 = new byte[32];
            for(int i = 0; i < 32; i++)
                array1[i] = (byte)i;
            array2 = new byte[32];
            for (int i = 0; i < 32; i++)
                array2[i] = (byte)(32 - (byte)i);
            byteVector1 = new(array1);
            byteVector2 = new(array2);
        }

        [Benchmark]
        public Vector<byte> VectorSimdAddition()
        {
            return byteVector1 + byteVector2;
        }

        [Benchmark]
        public byte[] ArrayAddition()
        {
            for(int i = 0; i < array1.Length; i++)
            {
                result[i] = (byte)(array1[i] + array2[i]);
            }
            return result;
        }
    }
}
