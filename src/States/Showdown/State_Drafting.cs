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
    private const int countdown = 6;
    public bool countDownStarted;

    private int countdownTime = countdown;
    private int DraftCountdownTime = DraftTime;

    public State_Drafting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;
    private Team _teamA => _Showdown.Match.TeamA;
    private Team _teamB => _Showdown.Match.TeamB;
    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        // Initialize all relevant variables
        countDownStarted = false;
        countdownTime = countdown;
        DraftCountdownTime = DraftTime;

        // Subscribe to events
        CommandBan.CommandInvoked += OnBan;
        CommandPick.CommandInvoked += OnPick;

        // Add a new draft for the match
        _Showdown.Match.AddDraft();

        // Set the lobby time to 86400 seconds (24 hours)
        ChatApi.SendMessage("/settime 86400");

        // Start the draft countdown coroutine
        CoroutineManager.Instance.StartExternalCoroutine(DraftCountdown());
    }

    public void Execute()
    {
        ServerMessage DraftMessage = new ServerMessage().ShowdownHeader(true)
            .AddLine(l => l.FontSize(30).Bold().AddBlock(_Showdown.Match.ScoreColored()).Indent("600%"))
            .AddSeparator();
        if (_Showdown.Match.CurrentDraft.IsDraftComplete())
        {
            DraftMessage.AddMessage(DraftCompleteMessage());

            if (!countDownStarted)
            {
                if (_Showdown.Match.CurrentDraft.PickedLevels.Count == 0)
                {
                    _Showdown.Match.CurrentDraft.PickedLevels.Add(
                        new DraftAction(
                            _Showdown.Match.CurrentDraft.AvailableLevels.Last(),
                            new Team("Showdown", "Showdown", "#ff0000"),
                            true
                        ));
                }

                // Create a copy of the PickedLevels list by extracting the OnlineZeeplevel from DraftAction
                List<OnlineZeeplevel> MatchPlaylist = new List<OnlineZeeplevel>(_Showdown.Match.CurrentDraft.PickedLevels.Select(draftAction => draftAction.Level));


                // Add the current playlist level to the copied list
                MatchPlaylist.Add(PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value).First());

                // Set the match playlist with the modified copy
                PlaylistManager.SetServerPlaylist(MatchPlaylist);

                // Stop any running coroutines and start the countdown coroutine
                CoroutineManager.Instance.StopAllExternalCoroutines();
                CoroutineManager.Instance.StartExternalCoroutine(DraftCompleteCountdown());
            }
        }
        else
        {
            DraftMessage
                .AddMessage(DraftingStateMessage());
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
    }

    private void FinishState()
    {
        Finished?.Invoke();
    }


    private ServerMessage DraftingStateMessage()
    {
        ServerMessage tmp = new ServerMessage();

        // Check if the time is 10 seconds or less
        bool isTimeRunningLow = DraftCountdownTime <= 10;

        tmp.AddLine(line =>
        {
            line
                .AddBlock($"{_Showdown.Match.CurrentDraft.GetCurrentTeam().GetTag()}", block => { block.Color(_Showdown.Match.CurrentDraft.GetCurrentTeam().Color); })
                .AddBlock("is drafting:")
                .AddBlock($"{TimeFormatter.FormatDuration(DraftCountdownTime)}", block =>
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
            .AddBlock($"{_Showdown.Match.CurrentDraft.GetCurrentTeam().Picks}", block => block.Color("#00ff00")) // Green for remaining picks
            .AddBlock("| Bans left:")
            .AddBlock($"{_Showdown.Match.CurrentDraft.GetCurrentTeam().Bans}", block => block.Color("#ff0000")));
        return tmp;
    }

    private ServerMessage DraftCompleteMessage()
    {
        ServerMessage tmp = new ServerMessage();

        tmp.AddLine(line =>
        {
            line.AddBlock("Draft complete!");
            if (countdownTime < 5)
            {
                line
                    .AddBlock("Initiating Match-Start-Procedure in:", block => block.Italic())
                    .AddBlock($"{TimeFormatter.FormatDuration(countdownTime)}", block => { block.Color("#00ff00"); })
                    ;
            }
        });


        return tmp;
    }

    private ServerMessage GetDraftLevelList()
    {
        ServerMessage tmp = new ServerMessage();

        foreach (OnlineZeeplevel level in _Showdown.Match.CurrentDraft.AllLevels)
        {
            tmp.AddLine(line =>
            {
                line.AddBlock(level.Name, block =>
                {
                    if (_Showdown.Match.CurrentDraft.AvailableLevels.All(l => l.Name != level.Name) ||
                        _Showdown.Match.CurrentDraft.UnAvailableLevels.Any(l => l.Name == level.Name))
                    {
                        block.Strikethrough();
                    }

                    if (_Showdown.Match.CurrentDraft.UnAvailableLevels.Any(l => l.Name == level.Name))
                    {
                        block.Color("#ffff00");
                    }

                    if (_Showdown.Match.CurrentDraft.BannedLevels.Any(l => l.Level.Name == level.Name))
                    {
                        block.Color("#ff0000");
                    }

                    if (_Showdown.Match.CurrentDraft.PickedLevels.Any(l => l.Level.Name == level.Name))
                    {
                        block.Color("#00ff00");
                    }
                });

                DraftAction bannedLevel = _Showdown.Match.CurrentDraft.BannedLevels.FirstOrDefault(l => l.Level.Name == level.Name);
                DraftAction pickedLevel = _Showdown.Match.CurrentDraft.PickedLevels.FirstOrDefault(l => l.Level.Name == level.Name);

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

    private void ResetDraftCountdown()
    {
        CoroutineManager.Instance.StopAllExternalCoroutines();
        DraftCountdownTime = DraftTime;
        CoroutineManager.Instance.StartExternalCoroutine(DraftCountdown());
    }

    private IEnumerator DraftCountdown()
    {
        while (DraftCountdownTime > 0)
        {
            // After countdown reaches 0, execute the next state
            Execute();
            // Wait for 1 second
            yield return new WaitForSeconds(1);

            // Decrease the countdown
            DraftCountdownTime--;
        }

        DraftCountdownTime = DraftTime;
        _Showdown.Match.CurrentDraft.SwitchTeam();
        ResetDraftCountdown();
    }

    private IEnumerator DraftCompleteCountdown()
    {
        countDownStarted = true;


        while (countdownTime > 0)
        {
            // After countdown reaches 0, execute the next state
            Execute();
            // Wait for 1 second
            yield return new WaitForSeconds(1);

            // Decrease the countdown
            countdownTime--;
        }

        FinishState();
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
        Draft currentDraft = _Showdown.Match.CurrentDraft;

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
                currentDraft.BanLevel(levelToPickOrBan);
                ChatApi.SendMessage($"{currentTeam.GetTag()} has banned the level {levelToPickOrBan.Name}");
            }
            else
            {
                currentDraft.PickLevel(levelToPickOrBan);
                ChatApi.SendMessage($"{currentTeam.GetTag()} has picked the level {levelToPickOrBan.Name}");
            }

            ResetDraftCountdown();
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