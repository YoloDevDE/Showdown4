using BepInEx;
using HarmonyLib;
using Showdown4.Commands;
using Showdown4.Config;
using Showdown4.States;
using Showdown4.States.Master;
using Showdown4.Utils;
using ZeepSDK;
using ZeepSDK.ChatCommands;
using ZeepSDK.Storage;
using ZeepSDK.UI;

namespace Showdown4;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK")]
public class Plugin : BaseUnityPlugin
{
	private ShowdownGuiDrawer _guiDrawer;
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
		MyConfig.Register(Config);
		RegisterCommands();
		InitializeStateMachine();

		_guiDrawer = new ShowdownGuiDrawer();
		UIApi.AddZeepGUIDrawer(_guiDrawer);
	}

	private void OnDestroy()
	{
		// Clear the static instance
		Instance = null;

		if (_guiDrawer != null)
		{
			UIApi.RemoveZeepGUIDrawer(_guiDrawer);
			_guiDrawer = null;
		}

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
		// Only the commands needed to control the plugin globally are registered here. Everything
		// else (the master control commands and the in-game draft/ready/link commands) is registered
		// and unregistered on demand by the states that actually handle them, so a command is only
		// known to the chat while it can be used.
		ChatCommandApi.RegisterLocalChatCommand<CommandShowdownStart>();
		ChatCommandApi.RegisterLocalChatCommand<CommandShowdownStop>();
	}
}