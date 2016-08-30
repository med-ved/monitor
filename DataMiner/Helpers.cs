using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMiner
{
    class Helpers
    {
        public static dynamic GetIfExists(dynamic data, string value)
        {
            return GetIfExists(data, new[] { value });
        }

        public static dynamic GetIfExists(dynamic data, string[] path)
        {
            dynamic result = data;
            for (int i = 0; i < path.Length; i++)
            {
                string name = path[i];
                if (HasProperty(result, name))
                {
                    result = result[name];
                }
                else
                {
                    return null;
                }
            }

            return result;
        }

        public static DateTime GetSpbCurrentTime()
        {
            var utc = DateTime.UtcNow;
            return utc.AddHours(3);
        }

        private static bool HasProperty(dynamic obj, string name)
        {
            try
            {
                return ((ICollection<string>)obj.Keys).Contains(name);
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
