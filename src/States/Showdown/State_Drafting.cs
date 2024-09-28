using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;
using ZeepkistNetworking;
using ZeepSDK.Chat;

namespace Showdown4.States.Showdown;

public class State_Drafting : IState
{
    private const int DraftTime = 60;
    private const int Countdown = 5;
    private const int MaxSpinLoops = 8;

    private int _draftCompleteCountDownTick = Countdown;
    private int _draftCountdownTime = DraftTime;
    private bool _hasInitiativeBanned;
    private bool _isDraftCompleteCountdownStarted;
    private bool _isInitiationPhaseComplete; // Flag to control the initiation phase
    private bool _isRandomSelectionActive;

    public State_Drafting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _showdown => StateMachine as ShowdownStateMachine;
    private Team _teamA => _showdown.Match.TeamA;
    private Team _teamB => _showdown.Match.TeamB;
    private Team _initiativeTeam => _showdown.Match.Initiative;

    private List<OnlineZeeplevel> _availableMaps => _showdown.Match.CurrentDraft.AvailableLevels;

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        // Initialize relevant variables
        _isDraftCompleteCountdownStarted = false;
        _draftCountdownTime = DraftTime;
        _isInitiationPhaseComplete = false; // Set initiation phase as not complete

        // Subscribe to events
        CommandBan.CommandInvoked += OnBan;
        CommandPick.CommandInvoked += OnPick;
        CommandStartRandom.CommandInvoked += OnStartRandom;

        // Add a new draft for the match
        _showdown.Match.AddDraft();

        // Set the lobby time to 86400 seconds (24 hours)
        ChatApi.SendMessage("/settime 86400");

        // Start a coroutine for the 3-second delay to mark the initiation phase
        CoroutineManager.Instance.StartExternalCoroutine(DelayDraftInitiation());
    }

    public void Execute()
    {
        // Check if the initiation phase is still running
        if (!_isInitiationPhaseComplete)
        {
            // Don't proceed to the actual drafting logic yet
            return;
        }

        ServerMessage DraftMessage = new ServerMessage().ShowdownHeader(true)
            .AddLine(l => l.FontSize(30).Bold().AddBlock(_showdown.Match.ScoreColored()).Indent("600%"))
            .AddSeparator();

        if (_showdown.Match.CurrentDraft.IsDraftComplete())
        {
            DraftMessage.AddMessage(DraftCompleteMessage());

            if (!_isDraftCompleteCountdownStarted)
            {
                if (_showdown.Match.CurrentDraft.PickedLevels.Count == 0)
                {
                    // _showdown.Match.CurrentDraft.PickedLevels.Add(
                    //     new DraftAction(
                    //         _showdown.Match.CurrentDraft.AvailableLevels.Last(),
                    //         new Team("Showdown", "Showdown", "#ff0000"),
                    //         true
                    //     ));
                    ChatApi.SendMessage("Draft incomplete! Waiting for Host to initiate Random Map Selection");
                    CoroutineManager.Instance.StopAllExternalCoroutines();
                }
                else
                {
                    // Create a copy of the PickedLevels list by extracting the OnlineZeeplevel from DraftAction
                    List<OnlineZeeplevel> MatchPlaylist = new List<OnlineZeeplevel>(_showdown.Match.CurrentDraft.PickedLevels.Select(draftAction => draftAction.Level));

                    // Add the current playlist level to the copied list
                    MatchPlaylist.Add(PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value).First());

                    // Set the match playlist with the modified copy
                    PlaylistManager.SetServerPlaylist(MatchPlaylist);

                    // Start the draft complete countdown using CountdownTimer
                    _isDraftCompleteCountdownStarted = true;
                    CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(Countdown, OnDraftCompleteTick, InvokeFinish));
                }
            }
        }
        else
        {
            DraftMessage.AddMessage(DraftingStateMessage());
        }

        DraftMessage
            .AddSeparator()
            .AddMessage(GetDraftLevelList());

        DraftMessage.Send();
    }

    public void Exit()
    {
        CommandBan.CommandInvoked -= OnBan;
        CommandPick.CommandInvoked -= OnPick;
        CommandStartRandom.CommandInvoked -= OnStartRandom;
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private IEnumerator DelayDraftInitiation()
    {
        // Send a message announcing the start of the draft with a 3-second delay
        ServerMessage initiationMessage = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock("Initiating Draft...")
                .AddBlock($"{_initiativeTeam.GetTag()}", b => b.Color(_initiativeTeam.Color))
                .AddBlock("starts.")
            )
            .AddSeparator()
            .AddLine(line => line.Italic()
                    .AddBlock("To pick use")
                    .AddBlock("'!pick 1-7'", block => block.Color("#ffff00")) // '!pick' in yellow
            )
            .AddLine(line => line.Italic()
                    .AddBlock("To ban use")
                    .AddBlock("'!ban 1-7'", block => block.Color("#ffff00")) // '!ban' in yellow
            )
            .AddSeparator();
        initiationMessage.Send();

        // Wait for 3 seconds before allowing Execute to proceed with the draft
        yield return new WaitForSeconds(2);
        initiationMessage = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock("Initiating Draft...")
                .AddBlock($"{_initiativeTeam.GetTag()}", b => b.Color(_initiativeTeam.Color))
                .AddBlock("starts.")
                .AddBlock("GO!!", b => b.Color("#00ff00"))
            )
            .AddSeparator()
            .AddLine(line => line.Italic()
                    .AddBlock("To pick use")
                    .AddBlock("'!pick 1-7'", block => block.Color("#ffff00")) // '!pick' in yellow
            )
            .AddLine(line => line.Italic()
                    .AddBlock("To ban use")
                    .AddBlock("'!ban 1-7'", block => block.Color("#ffff00")) // '!ban' in yellow
            )
            .AddSeparator();
        initiationMessage.Send();
        yield return new WaitForSeconds(1);

        // Mark initiation phase as complete
        _isInitiationPhaseComplete = true;
        CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(DraftTime, OnDraftTick, OnDraftTimeout));
        // Now we can continue the normal flow in the Execute method
        Execute();
    }

    private void OnDraftTick(int remainingSeconds)
    {
        _draftCountdownTime = remainingSeconds;
        Execute(); // Update the draft state display
    }

    private void OnDraftTimeout()
    {
        _draftCountdownTime = DraftTime;
        _showdown.Match.CurrentDraft.SwitchTeam();
        // Restart the countdown for the next team
        CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(DraftTime, OnDraftTick, OnDraftTimeout));
    }

    private void OnDraftCompleteTick(int remainingSeconds)
    {
        // Update the draft completion countdown message
        _draftCompleteCountDownTick = remainingSeconds;
        Execute();
    }

    private ServerMessage DraftingStateMessage()
    {
        ServerMessage tmp = new ServerMessage();

        // Check if the time is 10 seconds or less
        bool isTimeRunningLow = _draftCountdownTime <= 10;

        tmp.AddLine(line =>
        {
            line
                .AddBlock($"{_showdown.Match.CurrentDraft.GetCurrentTeam().GetTag()}", block => { block.Color(_showdown.Match.CurrentDraft.GetCurrentTeam().Color); })
                .AddBlock("is drafting:")
                .AddBlock($"{TimeFormatter.FormatDuration(_draftCountdownTime)}", block =>
                {
                    block.Color(isTimeRunningLow ? "#ff0000" : "#ffff00"); // Red if time is <= 10, yellow otherwise
                });
            if (isTimeRunningLow)
            {
                line.AddBlock("Time is running low! Make your choice!", block => block.Color("#ff0000").Bold());
            }
        });

        tmp.AddLine(line => line
            .AddBlock("Picks left:")
            .AddBlock($"{_showdown.Match.CurrentDraft.GetCurrentTeam().Picks}", block => block.Color("#00ff00")) // Green for remaining picks
            .AddBlock("| Bans left:")
            .AddBlock($"{_showdown.Match.CurrentDraft.GetCurrentTeam().Bans}", block => block.Color("#ff0000")));
        return tmp;
    }

    private ServerMessage DraftCompleteMessage()
    {
        ServerMessage tmp = new ServerMessage();

        tmp.AddLine(line =>
        {
            line.AddBlock("Draft complete!");
            if (_isDraftCompleteCountdownStarted)
            {
                line
                    .AddBlock("Continue to")
                    .AddBlock("'Pre-Racing'", block => block.Color("#ffff00"))
                    .AddBlock("in")
                    .AddBlock($"{_draftCompleteCountDownTick}", block => block.Color("#00ff00"))
                    .AddBlock("seconds...");
            }
        });

        return tmp;
    }

    private ServerMessage GetDraftLevelList()
    {
        ServerMessage tmp = new ServerMessage();

        foreach (OnlineZeeplevel level in _showdown.Match.CurrentDraft.AllLevels)
        {
            tmp.AddLine(line =>
            {
                line.AddBlock(level.Name, block =>
                {
                    if (_showdown.Match.CurrentDraft.AvailableLevels.All(l => l.Name != level.Name) ||
                        _showdown.Match.CurrentDraft.UnAvailableLevels.Any(l => l.Name == level.Name))
                    {
                        block.Strikethrough();
                    }

                    if (_showdown.Match.CurrentDraft.UnAvailableLevels.Any(l => l.Name == level.Name))
                    {
                        block.Color("#ffff00");
                    }

                    if (_showdown.Match.CurrentDraft.BannedLevels.Any(l => l.Level.Name == level.Name))
                    {
                        block.Color("#ff0000");
                    }

                    if (_showdown.Match.CurrentDraft.PickedLevels.Any(l => l.Level.Name == level.Name))
                    {
                        block.Color("#00ff00");
                    }
                });

                DraftAction bannedLevel = _showdown.Match.CurrentDraft.BannedLevels.FirstOrDefault(l => l.Level.Name == level.Name);
                DraftAction pickedLevel = _showdown.Match.CurrentDraft.PickedLevels.FirstOrDefault(l => l.Level.Name == level.Name);

                if (bannedLevel != null)
                {
                    line.AddBlock("banned", block => block.Color("#ff0000").Indent("500%"))
                        .AddBlock("by")
                        .AddBlock($"{bannedLevel.Team.GetTag()}", block => block.Color(bannedLevel.Team.Color)); // Using the Indent method
                }
                else if (pickedLevel != null)
                {
                    line.AddBlock("picked", block => block.Color("#00ff00").Indent("500%"))
                        .AddBlock("by")
                        .AddBlock($"{pickedLevel.Team.GetTag()}", block => block.Color(pickedLevel.Team.Color)); // Using the Indent method
                }
            });
        }

        return tmp;
    }


    public void OnStartRandom()
    {
        if (_availableMaps == null || _availableMaps.Count < 2)
        {
            ChatApi.SendMessage("Not enough maps in the pool to perform random selection.");
            return;
        }

        _isRandomSelectionActive = true;
        CoroutineManager.Instance.StartExternalCoroutine(RandomSelectionAnimation());
    }

    private void UpdateRandomSelectionMessage(int currentIndex)
    {
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock("Random Map Selection...")
            )
            .AddSeparator();

        for (int i = 0; i < _availableMaps.Count; i++)
        {
            OnlineZeeplevel level = _availableMaps[i];

            if (i == currentIndex)
            {
                msg.AddLine(line => line.AddBlock($"> {level.Name}", block => block.Bold().Color("#ffff00"))); // Highlight current selection
            }
            else
            {
                msg.AddLine(line => line.AddBlock(level.Name)); // Normal display
            }
        }

        msg.AddSeparator();
        msg.Send();
    }

    private IEnumerator RandomSelectionAnimation()
    {
        float delay = 0.10f;
        int selectedIndex = 0;

        // Loop through the available maps with a slowing down effect
        for (int i = 0; i < MaxSpinLoops; i++)
        {
            selectedIndex = (selectedIndex + 1) % _availableMaps.Count;
            UpdateRandomSelectionMessage(selectedIndex);
            delay += 0.1f; // Slow down the iteration

            yield return new WaitForSeconds(delay);
        }

        // Final selection made
        SelectRandomMap(_availableMaps[selectedIndex]);
        _isRandomSelectionActive = false;
        Execute();
    }

    private void SelectRandomMap(OnlineZeeplevel selectedLevel)
    {
        ChatApi.SendMessage($"Randomly selected map: {selectedLevel.Name}");

        _showdown.Match.CurrentDraft.PickedLevels.Add(
            new DraftAction(
                selectedLevel,
                new Team("Showdown", "Showdown", "#ff0000"),
                true
            ));
    }

    private void HandleDraft(bool isBan, ulong steamId, string levelIndexStr)
    {
        // Get the player name from steamId
        if (!ZeepkistNetwork.TryGetPlayer(steamId, out ZeepkistNetworkPlayer player))
        {
            ChatApi.SendMessage("Error: Could not find the player for the provided Steam ID.");
            return;
        }

        string playerName = player.Username;

        // Get the current draft from the match (CurrentDraft assumed here)
        Draft currentDraft = _showdown.Match.CurrentDraft;

        // Check if the steamId is part of the current team's racers
        Team currentTeam = currentDraft.GetCurrentTeam();
        if (currentTeam.Racers.All(racer => racer.SteamId != steamId))
        {
            ChatApi.SendMessage($"{playerName} is not a member of the current drafting team.");
            return;
        }

        // Validate if levelIndexStr is a valid integer between 1 and 7
        if (!int.TryParse(levelIndexStr, out int levelIndex) || levelIndex < 1 || levelIndex > 7)
        {
            ChatApi.SendMessage($"Please enter a valid level index between 1 and 7. You entered: {levelIndexStr}");
            return;
        }

        // Retrieve the list of available levels from the current draft
        if (levelIndex > currentDraft.AllLevels.Count)
        {
            ChatApi.SendMessage($"The selected level index {levelIndex} is out of range. There are only {currentDraft.AvailableLevels.Count} levels available.");
            return;
        }

        // Get the level to ban/pick
        OnlineZeeplevel levelToPickOrBan = currentDraft.AllLevels[levelIndex - 1]; // Assuming index matches the level

        // Depending on whether this is a pick or a ban, handle appropriately
        try
        {
            if (isBan)
            {
                if (currentTeam.Equals(_showdown.Match.Initiative) && _hasInitiativeBanned == false)
                {
                    _hasInitiativeBanned = true;
                    _showdown.Match.Initiative = _showdown.Match.NonInitiative;
                }

                currentDraft.BanLevel(levelToPickOrBan);
                ChatApi.SendMessage($"{currentTeam.GetTag()} has banned the level {levelToPickOrBan.Name}");
            }
            else
            {
                currentDraft.PickLevel(levelToPickOrBan);
                ChatApi.SendMessage($"{currentTeam.GetTag()} has picked the level {levelToPickOrBan.Name}");
            }

            // Restart draft countdown after a pick/ban
            CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(DraftTime, OnDraftTick, OnDraftTimeout));
            Execute();
        }
        catch (InvalidOperationException ex)
        {
            ChatApi.SendMessage(ex.Message);
        }
    }

    private void OnPick(ulong steamId, string levelIndexStr)
    {
        HandleDraft(false, steamId, levelIndexStr);
    }

    private void OnBan(ulong steamId, string levelIndexStr)
    {
        HandleDraft(true, steamId, levelIndexStr);
    }
}