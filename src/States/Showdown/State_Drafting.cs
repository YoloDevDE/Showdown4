using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;
using ZeepkistNetworking;
using ZeepSDK.Chat;
using Random = System.Random;

namespace Showdown4.States.Showdown;

public class State_Drafting : IState
{
    private const int DraftTime = 90;
    private const int Countdown = 3;
    private const int MaxSpinLoops = 30;
    private const int MinSpinLoops = 8;

    private int _draftCompleteCountDownTick = Countdown;
    private int _draftCountdownTime = DraftTime;
    private bool _hasInitiativeBanned;
    private bool _isDraftCompleteCountdownStarted;
    private bool _isInitiationPhaseComplete;
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
        _isDraftCompleteCountdownStarted = false;
        _draftCountdownTime = DraftTime;
        _isInitiationPhaseComplete = false;


        _showdown.Match.AddDraft();
        ChatApi.SendMessage("/settime 86400");

        CoroutineManager.Instance.StartExternalCoroutine(DelayDraftInitiation());
    }

    public void Execute()
    {
        if (!_isInitiationPhaseComplete)
        {
            return;
        }

        ServerMessage DraftMessage = new ServerMessage().ShowdownHeader(true)
            .AddLine(l => l.Size(30).Bold().AddBlock(_showdown.Match.ScoreColored()).Indent("590%"));

        if (_showdown.Match.CurrentDraft.IsDraftComplete())
        {
            DraftMessage.AddMessage(DraftCompleteMessage());

            if (!_isDraftCompleteCountdownStarted)
            {
                if (_showdown.Match.CurrentDraft.PickedLevels.Count == 0)
                {
                    ChatMessage.SendCustomMessage("Draft incomplete! Waiting for Host to initiate Random Map Selection");
                    CoroutineManager.Instance.StopAllExternalCoroutines();
                }
                else
                {
                    List<OnlineZeeplevel> MatchPlaylist = new List<OnlineZeeplevel>(_showdown.Match.CurrentDraft.PickedLevels.Select(draftAction => draftAction.Level));
                    MatchPlaylist.Add(PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value).First());
                    PlaylistManager.SetServerPlaylist(MatchPlaylist);

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
            .AddMessage(GetDraftLevelList())
            .AddSeparator()
            .AddLine(line => line.Italic().AddBlock("To pick use").AddBlock("'!pick 1-7'", block => block.Color("#ffff00")))
            .AddLine(line => line.Italic().AddBlock("To ban use").AddBlock("'!ban 1-7'", block => block.Color("#ffff00"))).Send();
        if (_draftCountdownTime >= DraftTime && !_showdown.Match.CurrentDraft.IsDraftComplete())
        {
            ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);
            // Build history of picks and bans
            StringBuilder historyMessage = new StringBuilder();
            foreach (DraftAction draftAction in _showdown.Match.CurrentDraft.BannedLevels.Concat(_showdown.Match.CurrentDraft.PickedLevels))
            {
                string actionType = draftAction.IsPick ? "picked" : "banned";
                string actionColor = draftAction.IsPick ? "#00ff00" : "#ff0000";
                historyMessage.AppendLine($"<color={draftAction.Team.Color}>{draftAction.Team.GetTag()}</color> " +
                                          $"<color={actionColor}>{actionType}</color> " +
                                          $"<color=#00ffff>{draftAction.Level.Name}</color>");
            }

            // Build current turn action description
            string actionDescription = "";
            if (_showdown.Match.CurrentDraft.GetCurrentTeam().Picks > 0)
            {
                if (_showdown.Match.CurrentDraft.GetOtherTeam().Picks == 0)
                {
                    actionDescription = "It's time to <color=#00ff00><b>pick</b></color> a map!" +
                                        "<br>Use <color=#ffff00><b>!pick 1-7</b></color> to pick a map.";
                }
                else
                {
                    actionDescription = "You can <color=#00ff00><b>pick</b></color> or <color=#ff0000><b>ban</b></color> a map!" +
                                        "<br>Use <color=#ffff00><b>!pick 1-7</b></color> to pick or <color=#ffff00><b>!ban 1-7</b></color> to ban a map.";
                }
            }
            else if (_showdown.Match.CurrentDraft.GetCurrentTeam().Bans > 0)
            {
                actionDescription = "You can only <color=#ff0000><b>ban</b></color> a map!" +
                                    "<br>Use <color=#ffff00><b>!ban 1-7</b></color> to ban a map.";
            }

            // Send messages

            // First show draft history if any exists
            if (historyMessage.Length > 0)
            {
                ChatMessage.SendCustomMessage($"Draft History:<br>{historyMessage}");
            }

            // Then show current turn message
            ChatMessage.SendCustomMessage(
                $"@<color={_showdown.Match.CurrentDraft.currentTeam.Color}><b>{_showdown.Match.CurrentDraft.GetCurrentTeam().Racers[0].SteamName}</b></color> + @<color={_showdown.Match.CurrentDraft.currentTeam.Color}><b>{_showdown.Match.CurrentDraft.GetCurrentTeam().Racers[1].SteamName}</b></color>" +
                $"<br>Your turn! {actionDescription}" +
                "<br><color=#888888><i><color=#ffff00>Warning</color>: Your selection will be FINAL and cannot be undone!</i></color>",
                _showdown.Match.CurrentDraft.GetCurrentTeam().Racers[0].SteamId,
                _showdown.Match.CurrentDraft.GetCurrentTeam().Racers[1].SteamId);
            // Send message to team that needs to wait
            ChatMessage.SendCustomMessage(
                $"@<color={_showdown.Match.CurrentDraft.GetOtherTeam().Color}><b>{_showdown.Match.CurrentDraft.GetOtherTeam().Racers[0].SteamName}</b></color> + @<color={_showdown.Match.CurrentDraft.GetOtherTeam().Color}><b>{_showdown.Match.CurrentDraft.GetOtherTeam().Racers[1].SteamName}</b></color>" +
                $"<br><b>Please wait</b> - It's the other team's ({_showdown.Match.CurrentDraft.GetCurrentTeam().GetColoredTag()}) turn to make their selection.",
                _showdown.Match.CurrentDraft.GetOtherTeam().Racers[0].SteamId,
                _showdown.Match.CurrentDraft.GetOtherTeam().Racers[1].SteamId);
        }
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

    private void OnDraftTick(int remainingSeconds)
    {
        _draftCountdownTime = remainingSeconds;
        Execute();
    }

    private void OnDraftTimeout()
    {
        _draftCountdownTime = DraftTime;
        _showdown.Match.CurrentDraft.GetCurrentTeam().MissedDraft = true;
        if (_showdown.Match.CurrentDraft.GetOtherTeam().Bans == 0 && _showdown.Match.CurrentDraft.GetOtherTeam().Picks == 0)
        {
            Team currentTeam = _showdown.Match.CurrentDraft.GetCurrentTeam();
            OnlineZeeplevel randomLevel = _availableMaps[new Random().Next(_availableMaps.Count)];

            if (currentTeam.Picks > 0)
            {
                _showdown.Match.CurrentDraft.PickLevel(randomLevel);
                ChatMessage.SendCustomMessage($"{currentTeam.GetColoredTag()} has randomly <color=#00ff00>picked</color> <color=#00ffff>{randomLevel.Name}</color>");
            }
            else if (currentTeam.Bans > 0)
            {
                _showdown.Match.CurrentDraft.BanLevel(randomLevel);
                ChatMessage.SendCustomMessage($"{currentTeam.GetColoredTag()} has randomly <color=#ff0000>banned</color> <color=#00ffff>{randomLevel.Name}</color>");
            }

            Execute();
            return;
        }

        _showdown.Match.CurrentDraft.SwitchTeam();
        CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(DraftTime, OnDraftTick, OnDraftTimeout));
    }

    private void OnDraftCompleteTick(int remainingSeconds)
    {
        _draftCompleteCountDownTick = remainingSeconds;
        Execute();
    }

    private IEnumerator DelayDraftInitiation()
    {
        ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);
        ChatMessage.SendCustomMessage($"<color=#FFA500><b>*** <color=#b19d63>{DraftphaseString()}</color> loading - Please pay attention to the upcoming messages! ***</b></color>");
        ChatApi.SendMessage("/servermessage remove");
        yield return new WaitForSeconds(1.5f);
        ServerMessage initiationMessage = new ServerMessage("center").ShowdownHeader(false, "center", 50);

        initiationMessage.Send();
        yield return new WaitForSeconds(1.5f);


        initiationMessage.AddLine(l => l.Size(30).Bold().AddBlock(_showdown.Match.ScoreWithFullNameColoredAndPadded()).NoBreak());
        initiationMessage.Send();
        yield return new WaitForSeconds(1.5f);
        initiationMessage = initiationMessage
            .AddLine(line =>
                line.AddBlock(DraftphaseString(), builder => builder.Gradients("#b19d63", "#FFFFFF", "#b19d63").Bold().AllCaps().Size(40))
            );
        initiationMessage.Send();
        yield return new WaitForSeconds(1.5f);

        initiationMessage = initiationMessage
            .AddLine(line => line
                .AddBlock($"{_initiativeTeam.GetTag()}", b => b.Color(_initiativeTeam.Color))
                .AddBlock("has initiative!")
            );
        initiationMessage.Send();
        yield return new WaitForSeconds(1.5f);

        CommandBan.CommandInvoked += OnBan;
        CommandPick.CommandInvoked += OnPick;
        CommandStartRandom.CommandInvoked += OnStartRandom;

        _isInitiationPhaseComplete = true;

        CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(DraftTime, OnDraftTick, OnDraftTimeout));
    }

    private ServerMessage DraftingStateMessage()
    {
        ServerMessage tmp = new ServerMessage();
        bool isTimeRunningLow = _draftCountdownTime <= 10;

        tmp.AddLine(line =>
            line.AddBlock(DraftphaseString(), builder => builder.Gradients("#b19d63", "#FFFFFF", "#b19d63").Bold().AllCaps().Size(40))
        ).AddSeparator(0).AddLine(line =>
        {
            line.AddBlock($"{_showdown.Match.CurrentDraft.GetCurrentTeam().GetTag()}", block => block.Color(_showdown.Match.CurrentDraft.GetCurrentTeam().Color))
                .AddBlock("is drafting:")
                .AddBlock($"{TimeFormatter.FormatDuration(_draftCountdownTime)}", block => block.Color(isTimeRunningLow ? "#ff0000" : "#ffff00"));
            if (isTimeRunningLow)
            {
                line.AddBlock("Time is running low! Make your choice!", block => block.Color("#ffff00").Bold());
            }
        });

        tmp
            .AddLine(line => line
                .AddBlock("Picks left:")
                .AddBlock($"{_showdown.Match.CurrentDraft.GetCurrentTeam().Picks}", block => block.Color("#00ff00"))
                .AddBlock("| Bans left:")
                .AddBlock($"{_showdown.Match.CurrentDraft.GetCurrentTeam().Bans}", block => block.Color("#ff0000")));
        return tmp;
    }

    private ServerMessage DraftCompleteMessage()
    {
        ServerMessage tmp = new ServerMessage();

        tmp.AddLine(line => line
            .AddBlock(DraftphaseString(), builder => builder
                .Gradients("#b19d63", "#FFFFFF", "#b19d63")
            )
            .AddBlock("-")
            .AddBlock("complete!", builder => builder.Color("#00ff00"))
            .Bold().AllCaps().Size(40));


        return tmp;
    }

    private string DraftphaseString()
    {
        return _showdown.Match.Rounds.Count > 0 ? "Draftphase II" : "Draftphase I";
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
                    if (_showdown.Match.CurrentDraft.AvailableLevels.All(l => l.Name != level.Name) || _showdown.Match.CurrentDraft.UnAvailableLevels.Any(l => l.Name == level.Name))
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
                        .AddBlock($"{bannedLevel.Team.GetTag()}", block => block.Color(bannedLevel.Team.Color));
                }
                else if (pickedLevel != null)
                {
                    line.AddBlock("picked", block => block.Color("#00ff00").Indent("500%"))
                        .AddBlock("by")
                        .AddBlock($"{pickedLevel.Team.GetTag()}", block => block.Color(pickedLevel.Team.Color));
                }
            });
        }

        return tmp;
    }

    private void OnPick(ulong steamId, string levelIndexStr)
    {
        HandleDraft(false, steamId, levelIndexStr);
    }

    private void OnBan(ulong steamId, string levelIndexStr)
    {
        HandleDraft(true, steamId, levelIndexStr);
    }

    private void HandleDraft(bool isBan, ulong steamId, string levelIndexStr)
    {
        if (!ZeepkistNetwork.TryGetPlayer(steamId, out ZeepkistNetworkPlayer player))
        {
            ChatMessage.SendCustomMessage("Error: Could not find the player for the provided Steam ID.");
            return;
        }

        string playerName = player.Username;
        Draft currentDraft = _showdown.Match.CurrentDraft;
        Team currentTeam = currentDraft.GetCurrentTeam();

        if (!player.IsLocal)

        {
            if (currentTeam.Racers.All(racer => racer.SteamId != steamId))
            {
                ChatMessage.SendCustomMessage($"{playerName} is not a member of the current drafting team ({currentTeam.GetColoredTag()}).");
                return;
            }
        }

        if (!int.TryParse(levelIndexStr, out int levelIndex) || levelIndex < 1 || levelIndex > 7 || levelIndex > currentDraft.AllLevels.Count)
        {
            ChatMessage.SendCustomMessage($"Invalid level index: {levelIndexStr}. Must be between 1 and 7.");
            return;
        }

        OnlineZeeplevel levelToPickOrBan = currentDraft.AllLevels[levelIndex - 1];

        try
        {
            if (isBan)
            {
                HandleBan(currentDraft, currentTeam, levelToPickOrBan);
            }
            else
            {
                HandlePick(currentDraft, levelToPickOrBan, currentTeam, player);
            }

            CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(DraftTime, OnDraftTick, OnDraftTimeout));
            Execute();
        }
        catch (InvalidOperationException ex)
        {
            ChatMessage.SendCustomMessage(ex.Message);
        }
    }

    private void HandleBan(Draft currentDraft, Team currentTeam, OnlineZeeplevel levelToPickOrBan)
    {
        if (currentTeam.Equals(_showdown.Match.Initiative) && !_hasInitiativeBanned)
        {
            // _hasInitiativeBanned = true;
            // _showdown.Match.Initiative = _showdown.Match.NonInitiative;
        }

        currentDraft.BanLevel(levelToPickOrBan);
        ChatMessage.SendCustomMessage($"{currentTeam.GetColoredTag()} has <color=#ff0000>banned</color> <color=#00ffff>{levelToPickOrBan.Name}</color>");
    }

    private void HandlePick(Draft currentDraft, OnlineZeeplevel level, Team currentTeam, ZeepkistNetworkPlayer player)
    {
        if (player.IsLocal && currentDraft.IsDraftComplete())
        {
            Team showdownTeam = new Team("Showdown", "Showdown", "#ff0000");
            currentDraft.PickedLevels.Add(new DraftAction(level, showdownTeam, true));
            ChatMessage.SendCustomMessage($"{showdownTeam.GetColoredTag()} has <color=#00ff00>picked</color> <color=#00ffff>{level.Name}</color>");
        }
        else
        {
            currentDraft.PickLevel(level);
            ChatMessage.SendCustomMessage($"{currentTeam.GetColoredTag()} has <color=#00ff00>picked</color> <color=#00ffff>{level.Name}</color>");
        }
    }

    public void OnStartRandom()
    {
        _isRandomSelectionActive = true;
        CoroutineManager.Instance.StartExternalCoroutine(RandomSelectionAnimation());
    }

    private void UpdateRandomSelectionMessage(int currentIndex)
    {
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line.AddBlock("Random Map Selection..."))
            .AddSeparator();

        for (int i = 0; i < _availableMaps.Count; i++)
        {
            OnlineZeeplevel level = _availableMaps[i];

            if (i == currentIndex)
            {
                msg.AddLine(line => line.AddBlock($"> {level.Name}", block => block.Bold().Color("#ffff00")));
            }
            else
            {
                msg.AddLine(line => line.AddBlock(level.Name));
            }
        }

        msg.AddSeparator();
        msg.Send();
    }

    private IEnumerator RandomSelectionAnimation()
    {
        float delay = 0.10f;
        int selectedIndex = 0;
        int randomLoops = new Random().Next(MinSpinLoops, MaxSpinLoops);

        for (int i = 0; i < randomLoops; i++)
        {
            selectedIndex = (selectedIndex + 1) % _availableMaps.Count;
            UpdateRandomSelectionMessage(selectedIndex);
            delay += 0.05f;

            yield return new WaitForSeconds(delay);
        }

        SelectRandomMap(_availableMaps[selectedIndex]);
        _isRandomSelectionActive = false;
        Execute();
    }

    private void SelectRandomMap(OnlineZeeplevel selectedLevel)
    {
        ChatMessage.SendCustomMessage($"Randomly selected map: {selectedLevel.Name}");

        _showdown.Match.CurrentDraft.PickedLevels.Add(new DraftAction(selectedLevel, new Team("Showdown", "Showdown", "#ff0000"), true));
    }
}