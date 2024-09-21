using System;
using System.Collections;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class State_Drafting : IState
{
    private bool _isDraftLocked; // Lock drafts during the window
    private bool _isTeamADrafting = true; // Track which team is drafting

    public State_Drafting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;
    private Team _teamA => _Showdown.Match.TeamA;
    private Team _teamB => _Showdown.Match.TeamB;
    private Team _currentlyDrafting => _isTeamADrafting ? _teamA : _teamB;
    private Team _notDrafting => _isTeamADrafting ? _teamB : _teamA;

    // Draft is complete if 2 picks are made OR no picks and no bans are left for both teams
    private bool DraftComplete =>
        (_teamA.Picks == 0 && _teamB.Picks == 0) ||
        (_teamA.Picks == 0 && _teamB.Picks == 0 && _teamA.Bans == 0 &&
         _teamB.Bans == 0); // Both teams have made 2 picks

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        AttachEvents();
    }

    public void Execute()
    {
        SendDraftMessage(); // Always send the draft message with history
    }

    public void Exit()
    {
        DetachEvents();
    }

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

        // Finish the state if the draft is complete
        if (DraftComplete)
            Finished?.Invoke();
    }

    private void OnBan(ulong steamId, string levelIndex)
    {
        HandleDraftAction(steamId, levelIndex, _Showdown.Match.DoBan);

        // Finish the state if the draft is complete
        if (DraftComplete)
            Finished?.Invoke();
    }

    private void HandleDraftAction(ulong steamId, string levelIndex, Action<Team, Level> draftAction)
    {
        if (_isDraftLocked) return; // Ignore draft if locked

        ZeepkistNetworkPlayer player = GetPlayer(steamId);
        if (player == null || _currentlyDrafting.Racers.All(r => r.SteamId != steamId)) return;

        Level level = GetLevelFromIndex(levelIndex, player);
        if (level == null)
        {
            ChatApi.SendMessage($"{player.Username} tried to use an invalid level index: {levelIndex}");
            return;
        }

        try
        {
            draftAction(_currentlyDrafting, level);

            // Lock the draft, show drafted message, then switch
            _isDraftLocked = true;
            CoroutineManager.Instance.StartExternalCoroutine(ShowDraftMessageForSeconds(_currentlyDrafting, level));
        }
        catch (InvalidOperationException ex)
        {
            ChatApi.SendMessage($"{player.Username} -> {ex.Message}");
        }
    }

    private IEnumerator ShowDraftMessageForSeconds(Team team, Level level)
    {
        // Show "Team XYZ drafted level XYZ" and history
        ServerMessage msg = CreateDraftMessage(true, team, level);
        msg.Send();

        yield return null; // Directly unlock after displaying the message

        // Unlock and switch teams
        _isDraftLocked = false;
        AddDraftToHistory(team, level); // Add it to history after showing the drafted message
        SwitchDrafter();
    }

    // Event Logic
    private void OnRoundEnded()
    {
        // Finish the state if the draft is complete
        if (DraftComplete)
            Finished?.Invoke();
    }

    private void SwitchDrafter()
    {
        _isTeamADrafting = !_isTeamADrafting;
        Execute();
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

    private ServerMessage CreateDraftMessage(bool showDraftedMessage = false, Team draftedTeam = null,
        Level draftedLevel = null)
    {
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock($"{_teamA.GetNameWithTag()}", b => b.Color(_teamA.Color))
                .AddBlock(" VS ")
                .AddBlock($"{_teamB.GetNameWithTag()}", b => b.Color(_teamB.Color)))
            .AddSeparator();

        if (showDraftedMessage && draftedTeam != null && draftedLevel != null)
            // Show drafted message instead of countdown
            msg.AddLine(line => line
                .AddBlock($"{draftedTeam.GetNameWithTag()}", f => f.Bold().Color(draftedTeam.Color))
                .AddBlock(" drafted ")
                .AddBlock($"{draftedLevel.OnlineZeeplevel.Name}", f => f.Bold().Color("#00ff00")));
        else
            // Show normal message
            msg.AddLine(line => line
                .AddBlock($"{_currentlyDrafting.Tag}", f => f.Bold().Color(_currentlyDrafting.Color))
                .AddBlock(" is drafting."));

        AddDraftHistory(msg); // Always add the draft history
        return msg;
    }

    private void AddDraftHistory(ServerMessage msg)
    {
        if (_Showdown.Match.LevelDrafts == null) return;

        // Iterate over the LevelDrafts
        for (int i = 0; i < _Showdown.Match.LevelDrafts.Count; i++)
        {
            LevelDraft draft = _Showdown.Match.LevelDrafts.ToList()[i];

            // Skip if level or critical elements are null
            if (draft?.Level?.OnlineZeeplevel == null) continue;

            // Add level name to the message
            msg.AddInLine(line => line.AddBlock($"{draft.Level.OnlineZeeplevel.Name}"));

            // Show "Banned/Picked" info if the team is not null
            if (draft.Team != null)
                msg.AddInLine(line => line
                    .Indent("500")
                    .AddBlock(draft.LevelDraftType == LevelDraftType.BAN ? "Banned" : "Picked",
                        f => f.Color(draft.LevelDraftType == LevelDraftType.BAN ? "#aa0000" : "#00aa00"))
                    .AddBlock(" by ")
                    .AddBlock(draft.Team.Tag, f => f.Color(draft.Team.Color)));

            msg.AddLine("");
        }
    }

    private void AddDraftToHistory(Team team, Level level)
    {
        // Logic to add the draft to history (as per game logic)
    }
}