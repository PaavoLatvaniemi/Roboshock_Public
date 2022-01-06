
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
namespace Assets.Scripts.Config
{
    public static class ConfigManager
    {
        static Config config;
        //Tekee tämän metodin ohjelman aloituksen yhteydessä
        [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            InitializeConfig();
        }
        public static string getConfigKeyValue(string key)
        {
            if (config == null)
            {
                InitializeConfig();
            }
            return config.getConfigDirective(key).Value;
        }
        public static void saveConfigKeyValue(string key, string value)
        {
            config.setConfigDirective(key, value);

        }
        //Tallennetaan pelaajan tiedot json tiedostoon, joka on StreamingAssets kansiossa.
        public static void serializeConfigValues()
        {
            string currentPath = System.IO.Path.Combine(Application.streamingAssetsPath, "config.json");
            File.WriteAllText(currentPath, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        private static void InitializeConfig()
        {
            //Tarvitaan currentpath, jotta löydetään configi
            string currentPath = System.IO.Path.Combine(Application.streamingAssetsPath, "config.json");
            if (File.Exists(currentPath))
            {
                try
                {
                    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(currentPath));
                }
                catch (Exception)
                {

                    throw;
                }
            }

            //Jos jsonia ei löydy... luodaan se default tiedoilla ja tehdään siitä jsoni.
            if (config == null)
            {
                config = new Config();

                File.WriteAllText(currentPath, JsonConvert.SerializeObject(config, Formatting.Indented));
            }

        }
    }
}
