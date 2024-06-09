using BepInEx;
using HarmonyLib;
using Showdown4.Commands;
using Showdown4.Domain.Entities;
using Showdown4.Domain.States;
using Showdown4.Tmp;
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

        RegisterCommands();
        InitializeStates();
        _matchStateMachine = new MatchStateMachine();
    }

    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
        harmony = null;
    }

    private void InitializeStates()
    {
        Match match = new Match(null, null, 3);
        State_SetTeams state_setTeams = new State_SetTeams();
        state_setTeams.AddTransitionRule(), new State_LinkRacersToTeams()));
    }

    private static void RegisterCommands()
    {
        ChatCommandApi.RegisterLocalChatCommand<CommandSetTeam>();
        ChatCommandApi.RegisterLocalChatCommand<CommandStartMatch>();
        ChatCommandApi.RegisterLocalChatCommand<CommandStopMatch>();
        ChatCommandApi.RegisterMixedChatCommand<CommandLinkPlayerToTeam>();
        ChatCommandApi.RegisterMixedChatCommand<CommandHeads>();
        ChatCommandApi.RegisterMixedChatCommand<CommandTails>();
    }
}