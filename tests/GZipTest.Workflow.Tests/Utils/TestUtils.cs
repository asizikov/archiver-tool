using System;
using System.Collections.Generic;

namespace GZipTest.Workflow.Tests.Utils
{
    public static class TestUtils
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list) where T : class
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}