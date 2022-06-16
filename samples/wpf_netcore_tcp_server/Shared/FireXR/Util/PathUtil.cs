using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireXR.Util
{

    public static class PathUtil
    {
        public static string Combine(string uriLeft, string uriRight)
        {
            var uri1 = uriLeft.TrimEnd('/', '\\');
            var uri2 = uriRight.TrimStart('/', '\\');
            return $"{uri1}/{uri2}";
        }

        public static string CombineForURL(string uriLeft, string uriRight)
        {
            var uri1 = uriLeft.TrimStart('/', '\\');
                uri1 = uri1.TrimEnd('/', '\\');
            var uri2 = uriRight.TrimStart('/', '\\');
                uri2 = uri2.TrimEnd('/', '\\');
            return $"/{uri1}/{uri2}/";
        }
    }
}
