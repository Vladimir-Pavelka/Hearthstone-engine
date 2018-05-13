namespace Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source)
            {
                action(element);
            }
        }

        public static IEnumerable<TResult> CartesianProduct<TSource, TSecond, TResult>(this IEnumerable<TSource> source,
            IEnumerable<TSecond> second, Func<TSource, TSecond, TResult> resultSelector)
        {
            var secondEnumerated = second as TSecond[] ?? second.ToArray();
            return source.SelectMany(x => secondEnumerated.Select(y => resultSelector(x, y)));
        }

        public static IEnumerable<T> Yield<T>(this T element)
        {
            yield return element;
        }
    }
}