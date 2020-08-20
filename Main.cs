using SDG.Framework.Modules;
using Stash.Utils;
using System;
using System.IO;
using System.Reflection;
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

            UnityThread.initUnityThread();

            StashObject = new GameObject("Stash");
            DontDestroyOnLoad(StashObject);

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            ConfigHelper.EnsureConfig($"{path}{Path.DirectorySeparatorChar}config.json");

            Config = ConfigHelper.ReadConfig($"{path}{Path.DirectorySeparatorChar}config.json");

            StashObject.AddComponent<StashManager>();
        }


        public void shutdown()
        {
            Instance = null;
        }
    }
}
