using Newtonsoft.Json;
using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Bridge.Contract
{
    public class ConfigHelper<T>
    {
        public virtual T ReadConfig(string configFileName, bool folderMode, string location, string configuration)
        {
            string configPath = null;
            string mergePath = null;
            if (!string.IsNullOrWhiteSpace(configuration))
            {
                configPath = GetConfigPath(configFileName.Insert(configFileName.LastIndexOf(".", StringComparison.Ordinal), "." + configuration), folderMode, location);
                mergePath = GetConfigPath(configFileName, folderMode, location);

                if (configPath == null)
                {
                    configPath = mergePath;
                    mergePath = null;
                }
            }
            else
            {
                configPath = GetConfigPath(configFileName, folderMode, location);

                if (configPath == null)
                {
                    configPath = GetConfigPath(configFileName.Insert(configFileName.LastIndexOf(".", StringComparison.Ordinal), ".debug"), folderMode, location);
                }

                if (configPath == null)
                {
                    configPath = GetConfigPath(configFileName.Insert(configFileName.LastIndexOf(".", StringComparison.Ordinal), ".release"), folderMode, location);
                }
            }

            if (configPath == null)
            {
                return default(T);
            }

            try
            {
                var json = File.ReadAllText(configPath);
                T config;
                if (mergePath != null)
                {
                    var jsonMerge = File.ReadAllText(mergePath);
                    var cfgMain = JObject.Parse(json);
                    var cfgMerge = JObject.Parse(jsonMerge);

                    cfgMain.Merge(cfgMerge);
                    config = cfgMain.ToObject<T>();
                }
                else
                {
                    config = JsonConvert.DeserializeObject<T>(json);    
                }
                
                
                if (config == null)
                {
                    return default(T);
                }

                return config;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Cannot read " + configFileName, e);
            }
        }

        public static string GetConfigPath(string configFileName, bool folderMode, string location)
        {
            var folder = folderMode ? location : Path.GetDirectoryName(location);
            var path = folder + Path.DirectorySeparatorChar + "Bridge" + Path.DirectorySeparatorChar + configFileName;

            if (!File.Exists(path))
            {
                path = folder + Path.DirectorySeparatorChar + configFileName;
            }

            if (!File.Exists(path))
            {
                path = folder + Path.DirectorySeparatorChar + "Bridge.NET" + Path.DirectorySeparatorChar + configFileName;
            }

            if (!File.Exists(path))
            {
                return null;
            }

            return path;
        }

        public string ConvertPath(string path)
        {
            if (Path.DirectorySeparatorChar != '/')
            {
                path = path.Replace('/', Path.DirectorySeparatorChar);
            }

            if (Path.DirectorySeparatorChar != '\\')
            {
                path.Replace('\\', Path.DirectorySeparatorChar);
            }

            return path;
        }
    }
}