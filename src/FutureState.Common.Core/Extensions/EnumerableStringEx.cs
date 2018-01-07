#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace FutureState
{

    public static class EnumerableStringEx
    {
        public static int IndexOfIgnoreCase(this IEnumerable<string> list, string item)
        {
            var i = -1;
            foreach (var header in list.Where(m => m != null))
            {
                i++;

                if (header.Equals(item, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        public static bool MatchAllIgnoreCase(this IEnumerable<string> expectedFields, IEnumerable<string> actualFields)
        {
            foreach (var expectedField in expectedFields)
            {
                var contains = false;

                foreach (var actualField in actualFields)
                    if (string.Equals(actualField, expectedField, StringComparison.OrdinalIgnoreCase))
                    {
                        contains = true;
                        break;
                    }

                if (!contains)
                    return false;
            }

            return true;
        }
    }
}