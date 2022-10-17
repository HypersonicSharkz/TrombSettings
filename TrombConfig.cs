using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TrombSettings
{
    [HarmonyPatch]
    [BepInPlugin("com.hypersonicsharkz.trombsettings", "TrombSettings", "1.0.0")]
    public class TrombConfig : BaseUnityPlugin
    {
        public static TrombSettings TrombSettings = new TrombSettings();

        private void Awake()
        {
            new Harmony("com.hypersonicsharkz.trombsettings").PatchAll();
        }

        [HarmonyPatch(typeof(HomeController), "Start")]
        private static void Prefix(HomeController __instance)
        {
            Debug.Log("TrombSettings || Start");
            ModSettingsController controller = __instance.fullsettingspanel.AddComponent<ModSettingsController>();
            controller.controller = __instance;
            controller.Init();
        }
    }

    public class BaseConfig
    {
        public ConfigEntryBase entry { get; set; }

        public BaseConfig(ConfigEntryBase entry)
        {
            this.entry = entry;
        }
    }

    public class StepSliderConfig : BaseConfig
    {
        public float max { get; set; }
        public float min { get; set; }
        public float increment { get; set; }
        public bool integerOnly { get; set; }

        public StepSliderConfig(float min, float max, float increment, bool integerOnly, ConfigEntryBase entry) : base(entry)
        {
            this.max = max;
            this.min = min;
            this.increment = increment;
            this.integerOnly = integerOnly;
        }
    }

    public class TrombEntryList : List<BaseConfig>
    {
        public new void Add(BaseConfig configEntry)
        {
            base.Add(configEntry);
        }
        public void Add(ConfigEntryBase configEntry)
        {
            Add(new BaseConfig(configEntry));
        }
    }


    public class TrombSettings : Dictionary<string, TrombEntryList>
    {
        public TrombSettings()
        {
            Add("Settings", null);
        }

        public new TrombEntryList this[string key]
        {
            get
            {
                if (!ContainsKey(key))
                    Add(key, new TrombEntryList());
                return base[key];
            }
        }
    }
}
