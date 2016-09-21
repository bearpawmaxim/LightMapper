using System;
using System.Collections.Generic;
using System.Linq;

namespace LightMapper.Concrete
{
    internal static class ExtensionMethods
    {
        internal static IEnumerable<T> Except<T, TKey>(this IEnumerable<T> list, IEnumerable<T> target, Func<T, TKey> lookup) where TKey : class
        {
            return list.Except(target, new CustomEqualityComparer<T, TKey>(lookup));
        }

        internal class CustomEqualityComparer<T, TKey> : IEqualityComparer<T> where TKey : class
        {
            Func<T, TKey> lookup;

            public CustomEqualityComparer(Func<T, TKey> lookup)
            {
                this.lookup = lookup;
            }

            public bool Equals(T x, T y)
            {
                return lookup(x).Equals(lookup(y));
            }

            public int GetHashCode(T obj)
            {
                return lookup(obj).GetHashCode();
            }
        }
    }
}
