using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulator
{
    static class Combinations
    {
        public static IEnumerable<T[]> CombinationsOfK<T>(T[] data, int k)
        {
            var size = data.Length;

            IEnumerable<T[]> Runner(T[] list, int n)
            {
                var skip = 1;
                foreach (var headList in list.Take(size - k + 1).Select(h => new T[] { h }))
                {
                    if (n == 1)
                        yield return headList;
                    else
                    {
                        foreach (var tailList in Runner(list.Skip(skip).ToArray(), n - 1))
                        {
                            yield return headList.Concat(tailList).ToArray();
                        }
                        skip++;
                    }
                }
            }

            return Runner(data, k);
        }
    }
}
