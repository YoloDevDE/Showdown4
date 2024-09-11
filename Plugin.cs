using BepInEx;
using HarmonyLib;
using Showdown4.Commands;
using Showdown4.Domain.Entities;
using Showdown4.Domain.States;
using Showdown4.Domain.States.Master;
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

    private void Awake()
    {
        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");


        ModStorage.Initialize(this);

        RegisterCommands();
        MasterStateMachine = new MasterStateMachine();
        MasterStateMachine.Start();
    }

    private void Start()
    {
        ZeepkistNetwork.ConnectedToMasterServer += ConnectedToMasterServer;
    }

    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
        harmony = null;
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