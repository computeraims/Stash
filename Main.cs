using SDG.Framework.Modules;
using Stash.Utils;
using System;
using System.IO;
using UnityEngine;

namespace Stash
{
    public class Main : MonoBehaviour, IModuleNexus
    {
        private static GameObject StashObject;

        public static Main Instance;

        public static Config Config;

        public void initialize()
        {
            Instance = this;
            Console.WriteLine("Stash by Corbyn loaded");

            StashObject = new GameObject("Stash");
            DontDestroyOnLoad(StashObject);

            string path = Directory.GetCurrentDirectory();

            ConfigHelper.EnsureConfig($"{path}\\Modules\\Stash\\config.json");

            Config = ConfigHelper.ReadConfig($"{path}\\Modules\\Stash\\config.json");

            StashObject.AddComponent<StashManager>();
        }


        public void shutdown()
        {
            Instance = null;
        }
    }
}
