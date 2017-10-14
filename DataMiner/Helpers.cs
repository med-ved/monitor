namespace DataMiner
{
    using System;
    using System.Collections.Generic;

    static class Helpers
    {
        public static dynamic GetIfExists(dynamic data, string value)
        {
            if (data == null)
            {
                return null;
            }

            return GetIfExists(data, new[] { value });
        }

        public static dynamic GetIfExists(dynamic data, string[] path)
        {
            if (data == null)
            {
                return null;
            }

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
            catch (Exception)
            {
                return false;
            }
        }
    }
}
