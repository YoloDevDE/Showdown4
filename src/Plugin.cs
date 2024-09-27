using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.States;
using Showdown4.States.Master;
using Showdown4.Utils;
using ZeepSDK.Chat;
using ZeepSDK.ChatCommands;
using ZeepSDK.Storage;

namespace Showdown4;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK")]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony;

    private IStateMachine MasterStateMachine;
    public static IModStorage Storage => StorageApi.CreateModStorage(Instance);
    public static Plugin Instance { get; private set; }

    public static ConfigEntry<string> CompetitionLevelsPlaylistName { get; set; }
    public static ConfigEntry<string> IntermissionLevelPlaylistName { get; set; }
    public static ConfigEntry<string> TeamFile { get; set; }

    private void Awake()
    {
        // Set the static instance
        Instance = this;

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
        // Clear the static instance
        Instance = null;

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
        ChatCommandApi.RegisterLocalChatCommand<CommandFinishState>();
        ChatCommandApi.RegisterLocalChatCommand<CommandCreateTeam>();
        ChatCommandApi.RegisterLocalChatCommand<CommandShowdownStart>();
        ChatCommandApi.RegisterLocalChatCommand<CommandShowdownStop>();
        ChatCommandApi.RegisterLocalChatCommand<CommandStopMatch>();
        ChatCommandApi.RegisterMixedChatCommand<CommandLinkRacer>();
        ChatCommandApi.RegisterMixedChatCommand<CommandReady>();

        ChatCommandApi.RegisterMixedChatCommand<CommandPick>();
        ChatCommandApi.RegisterMixedChatCommand<CommandBan>();

        CommandCreateTeam.CommandInvoked += CreateTeam;
    }

    public void CreateTeam(string args)
    {
        string color = null;
        string tag = null;
        string filename = null;
        string name = null;

        // Split the args into parts by spaces
        List<string> splitArgs = args.Split(' ').ToList();

        // Extract the flags and their values first
        for (int i = 0; i < splitArgs.Count - 1; i++) // The last argument will be the name
        {
            string argument = splitArgs[i];

            if (argument.StartsWith("-"))
            {
                // Get the next value for the flag
                if (i + 1 < splitArgs.Count && !splitArgs[i + 1].StartsWith("-"))
                {
                    string value = splitArgs[i + 1];
                    switch (argument)
                    {
                        case "-c":
                            color = value;
                            break;
                        case "-t":
                            tag = value;
                            break;
                        case "-f":
                            filename = value;
                            break;
                    }

                    i++; // Skip the next value since we already processed it
                }
                else
                {
                    ChatApi.SendMessage($"Error: Flag {argument} is missing a value.");
                    return;
                }
            }
        }

        // The last part after the flags is the team name
        name = splitArgs.Last();

        // Validate that all required fields are present
        if (string.IsNullOrEmpty(color) || string.IsNullOrEmpty(tag) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(filename))
        {
            ChatApi.SendMessage("Error: Missing one or more required fields (-c, -t, -f, name)");
            return;
        }

        // Load existing teams from the file if it exists, otherwise create a new list
        List<Team> teams = Storage.JsonFileExists(filename)
            ? Storage.LoadFromJson<List<Team>>(filename)
            : new List<Team>();

        // Create the new team
        Team newTeam = new Team(name, tag, color);

        // Add the new team to the list
        teams.Add(newTeam);

        // Save the updated team list back to the file
        Storage.SaveToJson(filename, teams);

        ChatApi.SendMessage($"Team '{newTeam.GetNameWithTag()}' has been created and saved to {filename}.json");
    }

    private void BindConfigs()
    {
        CompetitionLevelsPlaylistName = Config.Bind(
            "General",
            "Competition Levels Playlistname",
            "S4_Pool",
            "Name of the playlist to use for the level pool"
        );

        IntermissionLevelPlaylistName = Config.Bind(
            "General",
            "Intermissionlevel Playlistname",
            "S4_Live",
            ""
        );

        TeamFile = Config.Bind(
            "General",
            "Teams Json",
            "Teams",
            ""
        );
    }
}