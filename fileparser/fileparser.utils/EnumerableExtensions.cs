using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace fileparser.utils
{
    public static class EnumerableExtensions
    {
        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body, CancellationToken token)
        {
            return Task.WhenAll(from partition in Partitioner.Create(source).GetPartitions(dop)
                                select Task.Run(async delegate
                                {
                                    using (partition)
                                    {
                                        while (partition.MoveNext() && !token.IsCancellationRequested)
                                        {
                                            await body(partition.Current);
                                        }
                                    }
                                }, token));
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return ForEachAsync(source, dop, body, CancellationToken.None);
        }
    }
}
