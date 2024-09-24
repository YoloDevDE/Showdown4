using System;
using System.Collections;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class State_Drafting : IState
{
    private const int DraftTimeLimit = 30; // 30 seconds for each draft
    private const int SwitchTime = 2; // 3 seconds for switching teams
    private const int DraftedMessageTime = 3; // 3 seconds for the drafted message
    private Coroutine _currentCoroutine; // To hold the current coroutine reference
    private string _currentDraftLine; // Variable to hold the draft line
    private bool _isDraftLocked; // Lock drafts during the window
    private bool _isTeamADrafting = true; // Track which team is drafting
    private int _timeRemaining; // Time remaining for the draft
    private bool _warningIssued; // Track whether the 10-second warning has been issued

    public State_Drafting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;
    private Team _teamA => _Showdown.Match.TeamA;
    private Team _teamB => _Showdown.Match.TeamB;
    private Team _currentlyDrafting => _isTeamADrafting ? _teamA : _teamB;
    private Team _notDrafting => _isTeamADrafting ? _teamB : _teamA;

    // Draft is complete if no picks or bans are left for both teams
    private bool DraftComplete =>
        _teamA.Picks == 0 && _teamB.Picks == 0 ||
        _teamA.Picks == 0 && _teamB.Picks == 0 && _teamA.Bans == 0 && _teamB.Bans == 0;

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        AttachEvents();
        _timeRemaining = DraftTimeLimit; // Initialize countdown to 30 seconds
        StartDraftCountdown(); // Start the countdown timer
    }

    public void Execute()
    {
        SendDraftMessage(); // Always send the current draft message with the countdown
    }

    public void Exit()
    {
        DetachEvents();
        StopCurrentCoroutine(); // Stop any running coroutine to prevent overlap
    }

    private void StartDraftCountdown()
    {
        _warningIssued = false; // Reset the warning flag
        _timeRemaining = DraftTimeLimit; // Reset the time
        StopCurrentCoroutine(); // Ensure any previous countdown is stopped
        _currentCoroutine = CoroutineManager.Instance.StartExternalCoroutine(DraftCountdown()); // Start new countdown
    }

    private void StopCurrentCoroutine()
    {
        if (_currentCoroutine != null)
        {
            CoroutineManager.Instance.StopCoroutine(_currentCoroutine); // Stop the active coroutine
            _currentCoroutine = null;
        }
    }

    private void SendDraftMessage()
    {
        // Create and send the current draft message with the countdown timer
        ServerMessage msg = CreateDraftMessage(_timeRemaining);
        msg.Send();
    }

    // Event Handling
    private void AttachEvents()
    {
        CommandPick.CommandInvoked += OnPick;
        CommandBan.CommandInvoked += OnBan;
        RacingApi.RoundEnded += OnRoundEnded;
    }

    private void DetachEvents()
    {
        CommandPick.CommandInvoked -= OnPick;
        CommandBan.CommandInvoked -= OnBan;
        RacingApi.RoundEnded -= OnRoundEnded;
    }

    // Draft Logic
    private void OnPick(ulong steamId, string levelIndex)
    {
        HandleDraftAction(steamId, levelIndex, _Showdown.Match.DoPick);

        if (DraftComplete)
        {
            StartDraftCompleted(); // Trigger draft completion
        }
        else
        {
            StartShowDraftedMessage(); // Show "XYZ drafted" message for 3 seconds
        }
    }

    private void OnBan(ulong steamId, string levelIndex)
    {
        HandleDraftAction(steamId, levelIndex, _Showdown.Match.DoBan);

        if (DraftComplete)
        {
            StartDraftCompleted(); // Trigger draft completion
        }
        else
        {
            StartShowDraftedMessage(); // Show "XYZ drafted" message for 3 seconds
        }
    }

    private void HandleDraftAction(ulong steamId, string levelIndex, Action<Team, Level> draftAction)
    {
        if (_isDraftLocked)
        {
            return; // Ignore draft if locked
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
            // Stop the current draft countdown coroutine as soon as the pick/ban is made
            StopCurrentCoroutine();

            draftAction(_currentlyDrafting, level);

            _isDraftLocked = true; // Lock the draft
            AddDraftToHistory(_currentlyDrafting, level); // Add the draft to history
        }
        catch (InvalidOperationException ex)
        {
            ChatApi.SendMessage($"{player.Username} -> {ex.Message}");
        }
    }

    private void StartShowDraftedMessage()
    {
        StopCurrentCoroutine(); // Stop any previous coroutine
        _currentCoroutine =
            CoroutineManager.Instance
                .StartExternalCoroutine(ShowDraftedMessage()); // Show drafted message for 3 seconds
    }

    private IEnumerator ShowDraftedMessage()
    {
        // Show "Team XYZ drafted level XYZ" for 3 seconds
        ServerMessage msg = CreateDraftMessage(0, true, _currentlyDrafting);
        msg.Send();

        yield return new WaitForSeconds(DraftedMessageTime); // Wait for 3 seconds before switching

        // Unlock and switch teams
        _isDraftLocked = false;
        SwitchDrafter();
    }

    private IEnumerator DraftCountdown()
    {
        while (_timeRemaining > 0)
        {
            if (_timeRemaining == 10 && !_warningIssued)
            {
                _warningIssued = true; // Mark the warning issued when the timer hits 10 seconds
            }

            // Send updated draft message with the countdown
            SendDraftMessage();

            yield return new WaitForSeconds(1);
            _timeRemaining--;
        }

        // Draft expired, switch teams
        StartSwitchTeamCountdown();
    }

    private void StartSwitchTeamCountdown()
    {
        StopCurrentCoroutine(); // Stop the current countdown
        _currentCoroutine = CoroutineManager.Instance.StartExternalCoroutine(SwitchTeamCountdown()); // Start switching
    }

    private IEnumerator SwitchTeamCountdown()
    {
        for (int i = SwitchTime; i > 0; i--)
        {
            // Replace draft message with switching message on the same line
            _currentDraftLine = $"Switching to {_notDrafting.Tag} in {i} seconds";
            SendDraftMessage(); // Update message to show switching team

            yield return new WaitForSeconds(1);
        }

        // Switch teams after countdown
        SwitchDrafter();
    }

    private void StartDraftCompleted()
    {
        StopCurrentCoroutine(); // Ensure any active countdown stops
        _currentCoroutine =
            CoroutineManager.Instance.StartExternalCoroutine(DraftCompleted()); // Start the draft completed coroutine
    }

    private IEnumerator DraftCompleted()
    {
        for (int i = 5; i > 0; i--)
        {
            // Replace draft message with "Draft Completed" countdown
            _currentDraftLine = $"Drafting is complete! Starting next phase in {i} seconds.";
            SendDraftMessage();

            yield return new WaitForSeconds(1);
        }

        // Trigger the end of the state after the countdown
        Finished?.Invoke();
    }

    // Event Logic
    private void OnRoundEnded()
    {
        // Finish the state if the draft is complete
        if (DraftComplete)
        {
            StartDraftCompleted();
        }
    }

    private void SwitchDrafter()
    {
        _isTeamADrafting = !_isTeamADrafting;
        _warningIssued = false; // Reset warning flag for the next team
        _timeRemaining = DraftTimeLimit; // Reset countdown
        StartDraftCountdown(); // Start the next draft countdown
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

    private ServerMessage CreateDraftMessage(int timeRemaining, bool showDraftedMessage = false,
        Team draftedTeam = null, Level draftedLevel = null)
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
            // Show drafted message
            msg.AddLine(line => line
                .AddBlock($"{draftedTeam.GetNameWithTag()}", f => f.Bold().Color(draftedTeam.Color))
                .AddBlock(" drafted ")
                .AddBlock($"{draftedLevel.OnlineZeeplevel.Name}", f => f.Bold().Color("#00ff00")));
        }
        else
        {
            if (_warningIssued)
            {
                msg.AddLine(line => line
                    .AddBlock($"{_currentlyDrafting.Tag}", f => f.Bold().Color(_currentlyDrafting.Color))
                    .AddBlock("is drafting.")
                    .AddBlock($"{timeRemaining}", f => f.Bold().Color("#ff0000"))
                    .AddBlock("seconds left.")
                    .AddBlock("Time is running low!", f => f.Bold().Color("#ff0000"))
                );
            }
            else
            {
                msg.AddLine(line => line
                    .AddBlock($"{_currentlyDrafting.Tag}", f => f.Bold().Color(_currentlyDrafting.Color))
                    .AddBlock("is drafting.")
                    .AddBlock($"{timeRemaining}", f => f.Bold().Color("#ff0000"))
                    .AddBlock("seconds left.")
                );
            }
        }

        if (!string.IsNullOrEmpty(_currentDraftLine))
            // Add the switch/team-specific message to the same line if available
        {
            msg.AddLine(line => line.AddBlock(_currentDraftLine));
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

        for (int i = 0; i < _Showdown.Match.LevelDrafts.Count; i++)
        {
            LevelDraft draft = _Showdown.Match.LevelDrafts.ToList()[i];

            if (draft?.Level?.OnlineZeeplevel == null)
            {
                continue;
            }

            msg.AddInLine(line => line.AddBlock($"{draft.Level.OnlineZeeplevel.Name}"));

            if (draft.Team != null)
            {
                msg.AddInLine(line => line
                    .Indent("500")
                    .AddBlock(draft.LevelDraftType == LevelDraftType.BAN ? "Banned" : "Picked",
                        f => f.Color(draft.LevelDraftType == LevelDraftType.BAN ? "#aa0000" : "#00aa00"))
                    .AddBlock(" by ")
                    .AddBlock(draft.Team.Tag, f => f.Color(draft.Team.Color)));
            }

            msg.AddLine("");
        }
    }

    private void AddDraftToHistory(Team team, Level level)
    {
        // Logic to add the draft to history (as per game logic)
    }
}