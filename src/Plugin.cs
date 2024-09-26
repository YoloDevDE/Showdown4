using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Showdown4.Commands;
using Showdown4.Managers;
using Showdown4.States;
using Showdown4.States.Master;
using Showdown4.Utils;
using ZeepSDK.ChatCommands;

namespace Showdown4;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK")]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony;

    private IStateMachine MasterStateMachine;

    public static ConfigEntry<string> CompetitionLevelsPlaylistName { get; set; }
    public static ConfigEntry<string> IntermissionLevelPlaylistName { get; set; }


    private void Awake()
    {
        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        ModLogger.Initialize(Logger);
        ModLogger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        _ = CoroutineManager.Instance;
        BindConfigs();
        RegisterCommands();
        InitializeStateMachine();
    }


    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
        harmony = null;
    }

    private void InitializeStateMachine()
    {
        MasterStateMachine = new MasterStateMachine();
        MasterStateMachine.Start();
    }

    private void RegisterCommands()
    {
        ChatCommandApi.RegisterLocalChatCommand<CommandSetupMatch>();
        ChatCommandApi.RegisterLocalChatCommand<CommandStartMatch>();
        ChatCommandApi.RegisterLocalChatCommand<CommandShowdownStart>();
        ChatCommandApi.RegisterLocalChatCommand<CommandShowdownStop>();
        ChatCommandApi.RegisterLocalChatCommand<CommandStopMatch>();
        ChatCommandApi.RegisterMixedChatCommand<CommandLinkRacer>();

        ChatCommandApi.RegisterMixedChatCommand<CommandPick>();
        ChatCommandApi.RegisterMixedChatCommand<CommandBan>();
    }

    private void BindConfigs()
    {
        // Existing config entry for Level Pool Playlist
        CompetitionLevelsPlaylistName = Config.Bind(
            "General", // Category
            "Competition Levels Playlistname", // Key
            "S4_Pool", // Default value
            "Name of the playlist to use for the level pool"
        );

        // New config entry for Showdown Playlist
        IntermissionLevelPlaylistName = Config.Bind(
            "General", // Category
            "Intermissionlevel Playlistname", // Key
            "S4_Live", // Default value
            ""
        );
    }
}