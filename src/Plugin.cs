using BepInEx;
using HarmonyLib;
using Showdown4.Commands;
using Showdown4.Config;
using Showdown4.Managers;
using Showdown4.States;
using Showdown4.States.Master;
using ZeepSDK.ChatCommands;
using ZeepSDK.Storage;

namespace Showdown4;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK")]
public class Plugin : BaseUnityPlugin
{
	private Harmony _harmony;

	private IStateMachine _masterStateMachine;
	public static IModStorage Storage => StorageApi.CreateModStorage(Instance);
	public static Plugin Instance { get; private set; }

	private void Awake()
	{
		// Set the static instance
		Instance = this;

		_harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
		_harmony.PatchAll();

		Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
		_ = CoroutineManager.Instance;
		MyConfig.Register(Config);
		RegisterCommands();
		InitializeStateMachine();
	}

	private void OnDestroy()
	{
		// Clear the static instance
		Instance = null;

		_harmony?.UnpatchSelf();
		_harmony = null;
	}

	private void InitializeStateMachine()
	{
		_masterStateMachine = new MasterStateMachine();
		_masterStateMachine.Start();
	}

	private void RegisterCommands()
	{
		ChatCommandApi.RegisterLocalChatCommand<CommandFinishState>();
		ChatCommandApi.RegisterLocalChatCommand<CommandShowdownStart>();
		ChatCommandApi.RegisterLocalChatCommand<CommandShowdownStop>();

		ChatCommandApi.RegisterMixedChatCommand<CommandLinkRacer>();
		ChatCommandApi.RegisterMixedChatCommand<CommandUnLinkRacer>();
		ChatCommandApi.RegisterMixedChatCommand<CommandReady>();

		ChatCommandApi.RegisterMixedChatCommand<CommandSkipAction>();
		ChatCommandApi.RegisterMixedChatCommand<CommandPick>();
		ChatCommandApi.RegisterMixedChatCommand<CommandBan>();
		ChatCommandApi.RegisterMixedChatCommand<CommandPass>();

		// CommandCreateTeam.CommandInvoked += CreateTeam;
	}
}