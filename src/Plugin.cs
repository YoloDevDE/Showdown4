using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Showdown4.Commands;
using Showdown4.Managers;
using Showdown4.States;
using Showdown4.States.Master;
using Showdown4.Utils;
using UnityEngine.SceneManagement;
using ZeepkistClient;
using ZeepSDK.ChatCommands;

namespace Showdown4;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK")]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony;


    private double margin;
    private IStateMachine MasterStateMachine;

    public static ConfigEntry<string> LevelPoolPlaylistName { get; set; }
    public static ConfigEntry<string> ShowdownPlaylistName { get; set; } // New ConfigEntry for Showdown Playlist


    private void Awake()
    {
        ModLogger.Initialize(Logger);
        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        // Existing config entry for Level Pool Playlist
        LevelPoolPlaylistName = Config.Bind(
            "General", // Category
            "Level-Pool Playlistname", // Key
            "S4_Pool", // Default value
            "Name of the playlist to use for the level pool"
        );

        // New config entry for Showdown Playlist
        ShowdownPlaylistName = Config.Bind(
            "General", // Category
            "Showdown Playlistname", // Key
            "S4_Live", // Default value
            "Name of the playlist to use for the showdown start"
        );

        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        _ = CoroutineManager.Instance;

        ModStorage.Initialize(this);

        RegisterCommands();
        MasterStateMachine = new MasterStateMachine();
        MasterStateMachine.Start();
    }

    private void Start()
    {
        ZeepkistNetwork.ConnectedToMasterServer += ConnectedToMasterServer;
        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
    }

    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
        harmony = null;
    }

    private void SceneManagerOnsceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "3D_MainMenu")
        {
            SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
            SceneManager.LoadScene("Online Lobby");
        }
    }

    private void SceneManagerOnactiveSceneChanged(Scene arg0, Scene arg1)
    {
        Console.WriteLine(arg0.name + " -> " + arg1.name);
    }

    private void ConnectedToMasterServer()
    {
        ZeepkistNetwork.ConnectedToMasterServer -= ConnectedToMasterServer;
        ZeepkistNetwork.CreateLobby("Im testing mods", 64, false);
    }

    private static void RegisterCommands()
    {
        ChatCommandApi.RegisterLocalChatCommand<CommandSetupMatch>();
        ChatCommandApi.RegisterLocalChatCommand<CommandStartMatch>();
        ChatCommandApi.RegisterLocalChatCommand<CommandShowdownStart>();
        ChatCommandApi.RegisterLocalChatCommand<CommandShowdownStop>();
        ChatCommandApi.RegisterLocalChatCommand<CommandStopMatch>();
        ChatCommandApi.RegisterMixedChatCommand<CommandLinkPlayerToTeam>();
        ChatCommandApi.RegisterMixedChatCommand<CommandHeads>();
        ChatCommandApi.RegisterMixedChatCommand<CommandTails>();
        ChatCommandApi.RegisterMixedChatCommand<CommandPick>();
        ChatCommandApi.RegisterMixedChatCommand<CommandBan>();
    }
}