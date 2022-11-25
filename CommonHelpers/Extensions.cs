using RTSSSharedMemoryNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public static class Extensions
    {
        public static String JoinWith0(this IEnumerable<String> list)
        {
            return String.Join('\0', list);
        }

        public static String JoinWith0<TSource>(this IEnumerable<TSource> list, Func<TSource, String> selector)
        {
            return list.Select(selector).JoinWith0();
        }

        public static String[] SplitWith0(this String str)
        {
            return str.Split('\0', StringSplitOptions.RemoveEmptyEntries);
        }

        public static String JoinWithN(this IEnumerable<String> list)
        {
            return String.Join('\n', list);
        }

        public static String JoinWithN<TSource>(this IEnumerable<TSource> list, Func<TSource, String> selector)
        {
            return list.Select(selector).JoinWithN();
        }

        public static String[] SplitWithN(this String str)
        {
            return str.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
