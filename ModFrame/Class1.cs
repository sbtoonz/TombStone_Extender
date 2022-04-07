using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;

namespace TombStoneExtender
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class TombStoneExtender : BaseUnityPlugin
    {
        private const string ModName = "TombStoneExtender";
        private const string ModVersion = "1.0";
        private const string ModGUID = "com.zarboz.TombStoneExtender";
        private static Harmony harmony = null!;
        private static ConfigEntry<int>? SlotCount;
        ConfigSync configSync = new(ModGUID) 
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion};
        internal static ConfigEntry<bool> ServerConfigLocked = null!;
        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }
        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            harmony = new(ModGUID);
            harmony.PatchAll(assembly);
            ServerConfigLocked = config("1 - General", "Lock Configuration", true, "If on, the configuration is locked and can be changed by server admins only.");
            SlotCount = config("1 - General", "Slot row count", 3, new ConfigDescription("How many rows to increase tombstone",  new  AcceptableValueRange<int>(4, 24)));
            configSync.AddLockingConfigEntry(ServerConfigLocked);
            
        }

        [HarmonyPatch(typeof(Player), nameof(Player.CreateTombStone))]
        public static class TombStonePatch
        {
            public static void Prefix(Player __instance)
            {
                var cont = __instance.m_tombstone.GetComponent<Container>();
                cont.m_width = 8;
                cont.m_height = SlotCount!.Value;
            }
        }
        public void OnDestroy()
        {
            harmony.UnpatchSelf();
        }
    }
}
