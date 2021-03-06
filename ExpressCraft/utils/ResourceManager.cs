﻿using System.Collections.Generic;

namespace ExpressCraft
{
    public static class ResourceManager
    {
        private static Dictionary<string, string> cacheResourceString = new Dictionary<string, string>();

        public static string GetResourceString(string name)
        {
            if(cacheResourceString.ContainsKey(name))
                return cacheResourceString[name];
            return (cacheResourceString[name] = Settings.GetStyleRuleValue(Settings.resourceManangerSheets, "content", "." + name));
        }
    }
}