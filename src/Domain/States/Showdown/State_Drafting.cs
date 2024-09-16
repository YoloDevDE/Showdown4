using System;
using System.Collections;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Tmp;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.Domain.States.Showdown;

public class State_Drafting : IState
{
    private readonly CountdownTimer _draftTimer;
    private readonly CountdownTimer _readyCheckTimer;
    private bool _isDraftLocked; // Lock drafts during the 2-second window
    private bool _isTeamADrafting = true;
    private int _picksMade; // Track the number of picks made

    public State_Drafting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
        _draftTimer = new CountdownTimer(30);
        _readyCheckTimer = new CountdownTimer(15);
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;
    private Team _teamA => _Showdown.Match.TeamA;
    private Team _teamB => _Showdown.Match.TeamB;
    private Team _currentlyDrafting => _isTeamADrafting ? _teamA : _teamB;
    private Team _notDrafting => _isTeamADrafting ? _teamB : _teamA;
    private bool DraftComplete => _picksMade >= 2 || _teamA.Picks == 0 && _teamB.Picks == 0 && _teamA.Bans == 0 && _teamB.Bans == 0;

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        AttachEvents();
        _draftTimer.Start();
    }

    public void Execute()
    {
        SendDraftMessage(); // Always send draft message with history
    }

    public void Exit()
    {
        DetachEvents();
        _draftTimer.Stop();
    }

// Add this method to your class
    private void SendDraftMessage()
    {
        // Create and send the current draft message
        ServerMessage msg = CreateDraftMessage();
        msg.Send();
    }

    // Event Handling
    private void AttachEvents()
    {
        CommandPick.CommandInvoked += OnPick;
        CommandBan.CommandInvoked += OnBan;
        _draftTimer.CountdownTick += OnDraftTimerTick;
        _readyCheckTimer.CountdownTick += OnReadyCheckTimerTick;
        _readyCheckTimer.CountdownFinished += OnReadyCheckTimerFinished;
        _draftTimer.CountdownFinished += OnDraftTimerReady;
        RacingApi.RoundEnded += OnRoundEnded;
    }

    private void DetachEvents()
    {
        CommandPick.CommandInvoked -= OnPick;
        CommandBan.CommandInvoked -= OnBan;
        _draftTimer.CountdownTick -= OnDraftTimerTick;
        _readyCheckTimer.CountdownTick -= OnReadyCheckTimerTick;
        _readyCheckTimer.CountdownFinished -= OnReadyCheckTimerFinished;
        _draftTimer.CountdownFinished -= OnDraftTimerReady;
        RacingApi.RoundEnded -= OnRoundEnded;
    }

    // Draft Logic
    private void OnPick(ulong steamId, string levelIndex)
    {
        HandleDraftAction(steamId, levelIndex, _Showdown.Match.DoPick);
        _picksMade++;

        if (_picksMade >= 2 || DraftComplete)
        {
            StartEndCountdown();
        }
    }

    private void OnBan(ulong steamId, string levelIndex)
    {
        HandleDraftAction(steamId, levelIndex, _Showdown.Match.DoBan);
        if (DraftComplete)
        {
            StartEndCountdown();
        }
    }

    private void HandleDraftAction(ulong steamId, string levelIndex, Action<Team, Level> draftAction)
    {
        if (_isDraftLocked)
        {
            return; // Ignore the draft if locked
        }

        ZeepkistNetworkPlayer player = GetPlayer(steamId);
        if (player == null || _currentlyDrafting.Racers.All(r => r.SteamId != steamId))
        {
            return;
        }

        Level level = GetLevelFromIndex(levelIndex, player);
        if (level == null)
        {
            ChatApi.SendMessage($"{player.Username} tried to use an invalid level index: {levelIndex}");
            return;
        }

        try
        {
            draftAction(_currentlyDrafting, level);

            // Lock the draft for 3 seconds, show drafted message, then switch
            _isDraftLocked = true;
            CoroutineStarter.Instance.StartExternalCoroutine(ShowDraftMessageForSeconds(_currentlyDrafting, level, 3));
        }
        catch (InvalidOperationException ex)
        {
            ChatApi.SendMessage($"{player.Username} -> {ex.Message}");
        }
    }

    private IEnumerator ShowDraftMessageForSeconds(Team team, Level level, int seconds)
    {
        // Show "Team XYZ drafted level XYZ" and history, replacing the countdown message temporarily
        ServerMessage msg = CreateDraftMessage(true, team, level);
        msg.Send();

        yield return new WaitForSeconds(seconds);

        // After delay, unlock and switch teams
        _isDraftLocked = false;
        AddDraftToHistory(team, level); // After showing the drafted message, add it to history
        SwitchDrafter();
    }

    // Timer and Event Logic
    private void OnDraftTimerTick()
    {
        Execute(); // Continuously update the draft message with countdown
    }

    private void OnDraftTimerReady()
    {
        ChatApi.SendMessage($"{_currentlyDrafting.GetNameWithTag()} failed to draft. {_notDrafting.GetNameWithTag()} now drafting!");
        SwitchDrafter();
    }

    private void OnReadyCheckTimerTick()
    {
        Execute();
    }

    private void OnReadyCheckTimerFinished()
    {
        _readyCheckTimer.Stop();
        Finished?.Invoke();
    }

    private void OnRoundEnded()
    {
        Finished?.Invoke();
    }

    private void SwitchDrafter()
    {
        _isTeamADrafting = !_isTeamADrafting;
        _draftTimer.Reset(30);
        Execute();
    }

    private void StartEndCountdown()
    {
        _draftTimer.Stop();
        CoroutineStarter.Instance.StartExternalCoroutine(EndCountdown(5));
    }

    private IEnumerator EndCountdown(int countdownDuration)
    {
        while (countdownDuration > 0)
        {
            ServerMessage msg = new ServerMessage()
                .ShowdownHeader()
                .AddLine(line => line
                    .AddBlock("Draft complete! Starting next phase in: ")
                    .AddBlock($"{countdownDuration} seconds", f => f.Color("#00ff00"))
                )
                .AddSeparator();
            AddDraftHistory(msg); // Keep showing the draft history
            msg.Send();

            yield return new WaitForSeconds(1f);
            countdownDuration--;
        }

        // Final message: "Drafting complete, Switching to Readycheck"
        ServerMessage completeMsg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock("Drafting complete, Switching to Readycheck", f => f.Color("#00ff00"))
            )
            .AddSeparator();
        AddDraftHistory(completeMsg);
        completeMsg.Send();

        Finished?.Invoke(); // Notify when the countdown finishes
    }

    // Helper Methods
    private ZeepkistNetworkPlayer GetPlayer(ulong steamId)
    {
        ZeepkistNetwork.TryGetPlayer(steamId, out ZeepkistNetworkPlayer player);
        return player;
    }

    private Level GetLevelFromIndex(string levelIndex, ZeepkistNetworkPlayer player)
    {
        if (!int.TryParse(levelIndex, out int levelNumber))
        {
            ChatApi.SendMessage($"{player.Username} tried to use an invalid level index: {levelIndex}");
            return null;
        }

        levelNumber = Math.Clamp(levelNumber, 1, 7);
        return _Showdown.Match.LevelDrafts.ToList()[levelNumber - 1].Level;
    }

    private ServerMessage CreateDraftMessage(bool showDraftedMessage = false, Team draftedTeam = null, Level draftedLevel = null)
    {
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock($"{_teamA.GetNameWithTag()}", b => b.Color(_teamA.Color))
                .AddBlock(" VS ")
                .AddBlock($"{_teamB.GetNameWithTag()}", b => b.Color(_teamB.Color)))
            .AddSeparator();

        if (showDraftedMessage && draftedTeam != null && draftedLevel != null)
        {
            // Show drafted message instead of countdown
            msg.AddLine(line => line
                .AddBlock($"{draftedTeam.GetNameWithTag()}", f => f.Bold().Color(draftedTeam.Color))
                .AddBlock(" drafted ")
                .AddBlock($"{draftedLevel.OnlineZeeplevel.Name}", f => f.Bold().Color("#00ff00"))
            );
        }
        else
        {
            // Show normal countdown
            msg.AddLine(line => line
                .AddBlock($"{_currentlyDrafting.Tag}", f => f.Bold().Color(_currentlyDrafting.Color))
                .AddBlock(" is drafting: ")
                .AddBlock($"{TimeFormatter.FormatDuration(_draftTimer.SecondsLeft)}", f => f.Bold().Color(_draftTimer.SecondsLeft <= 10 ? "#ff0000" : "#ffffff"))
                .AddBlock($"{(_draftTimer.SecondsLeft <= 10 ? $" Hurry! {_notDrafting.Tag} will take your turn!" : "")}")
            );
        }

        AddDraftHistory(msg); // Always add the draft history
        return msg;
    }

    private void AddDraftHistory(ServerMessage msg)
    {
        if (_Showdown.Match.LevelDrafts == null)
        {
            return;
        }

        // Iterate over the LevelDrafts
        for (int i = 0; i < _Showdown.Match.LevelDrafts.Count; i++)
        {
            LevelDraft draft = _Showdown.Match.LevelDrafts.ToList()[i];

            // Check if the level or any critical elements are null
            if (draft?.Level?.OnlineZeeplevel == null)
            {
                continue; // Skip this draft if critical elements are null
            }

            // Add the level name to the message
            msg.AddInLine(line => line.AddBlock($"{draft.Level.OnlineZeeplevel.Name}"));

            // If the team is not null, show the "Banned/Picked" info
            if (draft.Team != null)
            {
                msg.AddInLine(line => line
                    .Indent("500")
                    .AddBlock(draft.LevelDraftType == LevelDraftType.BAN ? "Banned" : "Picked", f => f.Color(draft.LevelDraftType == LevelDraftType.BAN ? "#aa0000" : "#00aa00"))
                    .AddBlock(" by ")
                    .AddBlock(draft.Team.Tag, f => f.Color(draft.Team.Color)));
            }

            msg.AddLine("");
        }
    }

    private void AddDraftToHistory(Team team, Level level)
    {
        // Add the team and level to the internal match draft history
        _Showdown.Match.LevelDrafts.Add(new LevelDraft
        {
            Team = team,
            Level = level,
            LevelDraftType = LevelDraftType.PICK // Assuming it's a pick for this example
        });
    }
}