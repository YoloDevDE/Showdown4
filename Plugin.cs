using BepInEx;
using HarmonyLib;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Statemachine;
using ZeepSDK.ChatCommands;

namespace Showdown4;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK")]
public class Plugin : BaseUnityPlugin
{
    private IStateMachine _matchStateMachine;
    private Harmony harmony;

    private void Awake()
    {
        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");


        ModStorage.Initialize(this);

        ChatCommandApi.RegisterLocalChatCommand<CommandSetTeam>();
        ChatCommandApi.RegisterLocalChatCommand<CommandStartMatch>();
        ChatCommandApi.RegisterLocalChatCommand<CommandStopMatch>();
        ChatCommandApi.RegisterMixedChatCommand<CommandLinkPlayerToTeam>();
        ChatCommandApi.RegisterMixedChatCommand<CommandHeads>();
        ChatCommandApi.RegisterMixedChatCommand<CommandTails>();

        _matchStateMachine = new MatchStateMachine();
    }

    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
        harmony = null;
    }
}