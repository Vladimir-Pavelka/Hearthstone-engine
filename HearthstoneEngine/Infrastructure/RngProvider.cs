namespace Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RngProvider
    {
        private readonly Random _rngProvider = new Random();

        public IEnumerable<T> PickAtRandom<T>(IEnumerable<T> source, int count)
        {
            var sourceEnumerated = source as T[] ?? source.ToArray();

            return Enumerable.Range(0, count).Select(_ =>
            {
                var index = _rngProvider.Next(0, sourceEnumerated.Count());

                return sourceEnumerated[index];
            });
        }

        public IEnumerable<T> GetRandomSubset<T>(IEnumerable<T> source, int count)
        {
            var sourceEnumerated = source.ToList();
            if (count > sourceEnumerated.Count) throw new ArgumentException("Count was higher then the source length !");

            return Enumerable.Range(0, count).Select(_ =>
            {
                var index = _rngProvider.Next(0, sourceEnumerated.Count());

                var returnValue = sourceEnumerated[index];
                sourceEnumerated.RemoveAt(index);

                return returnValue;
            });
        }
    }
}