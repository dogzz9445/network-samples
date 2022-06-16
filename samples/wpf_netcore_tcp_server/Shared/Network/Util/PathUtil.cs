using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingNetwork.Util
{

    public static class PathUtil
    {
        public static string Combine(string uriLeft, string uriRight)
        {
            var uri1 = uriLeft.TrimEnd('/', '\\');
            var uri2 = uriRight.TrimStart('/', '\\');
            return $"{uri1}/{uri2}";
        }
    }
}
