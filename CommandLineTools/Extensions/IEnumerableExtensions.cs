using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandLineTools.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TElement> Append<TElement>(this IEnumerable<TElement> source, TElement appendix)
        {
            foreach (var ele in source)
            {
                yield return ele;
            }
            yield return appendix;
        }
    }
}