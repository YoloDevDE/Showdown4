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
using Random = System.Random;

namespace Showdown4.States.Showdown;

public class State_Drafting : IState
{
    private const int DraftTime = 60;
    private const int Countdown = 10;
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

        CommandBan.CommandInvoked += OnBan;
        CommandPick.CommandInvoked += OnPick;
        CommandStartRandom.CommandInvoked += OnStartRandom;

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
            .AddLine(l => l.FontSize(30).Bold().AddBlock(_showdown.Match.ScoreColored()).Indent("600%"))
            .AddSeparator();

        if (_showdown.Match.CurrentDraft.IsDraftComplete())
        {
            DraftMessage.AddMessage(DraftCompleteMessage());

            if (!_isDraftCompleteCountdownStarted)
            {
                if (_showdown.Match.CurrentDraft.PickedLevels.Count == 0)
                {
                    ChatApi.SendMessage("Draft incomplete! Waiting for Host to initiate Random Map Selection");
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

        DraftMessage.AddSeparator().AddMessage(GetDraftLevelList()).Send();
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
        ServerMessage initiationMessage = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock("Initiating Draft...")
                .AddBlock($"{_initiativeTeam.GetTag()}", b => b.Color(_initiativeTeam.Color))
                .AddBlock("starts.")
            )
            .AddSeparator()
            .AddLine(line => line.Italic().AddBlock("To pick use").AddBlock("'!pick 1-7'", block => block.Color("#ffff00")))
            .AddLine(line => line.Italic().AddBlock("To ban use").AddBlock("'!ban 1-7'", block => block.Color("#ffff00")))
            .AddSeparator();
        initiationMessage.Send();

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
            .AddLine(line => line.Italic().AddBlock("To pick use").AddBlock("'!pick 1-7'", block => block.Color("#ffff00")))
            .AddLine(line => line.Italic().AddBlock("To ban use").AddBlock("'!ban 1-7'", block => block.Color("#ffff00")))
            .AddSeparator();
        initiationMessage.Send();

        yield return new WaitForSeconds(1);
        _isInitiationPhaseComplete = true;
        CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(DraftTime, OnDraftTick, OnDraftTimeout));
        Execute();
    }

    private ServerMessage DraftingStateMessage()
    {
        ServerMessage tmp = new ServerMessage();
        bool isTimeRunningLow = _draftCountdownTime <= 10;

        tmp.AddLine(line =>
        {
            line.AddBlock($"{_showdown.Match.CurrentDraft.GetCurrentTeam().GetTag()}", block => block.Color(_showdown.Match.CurrentDraft.GetCurrentTeam().Color))
                .AddBlock("is drafting:")
                .AddBlock($"{TimeFormatter.FormatDuration(_draftCountdownTime)}", block => block.Color(isTimeRunningLow ? "#ff0000" : "#ffff00"));
            if (isTimeRunningLow)
            {
                line.AddBlock("Time is running low! Make your choice!", block => block.Color("#ffff00").Bold());
            }
        });

        tmp.AddLine(line => line
            .AddBlock("Picks left:")
            .AddBlock($"{_showdown.Match.CurrentDraft.GetCurrentTeam().Picks}", block => block.Color("#00ff00"))
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
                line.AddBlock("Continue to")
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
            ChatApi.SendMessage("Error: Could not find the player for the provided Steam ID.");
            return;
        }

        string playerName = player.Username;
        Draft currentDraft = _showdown.Match.CurrentDraft;
        Team currentTeam = currentDraft.GetCurrentTeam();

        if (!player.IsLocal)

        {
            if (currentTeam.Racers.All(racer => racer.SteamId != steamId))
            {
                ChatApi.SendMessage($"{playerName} is not a member of the current drafting team.");
                return;
            }
        }

        if (!int.TryParse(levelIndexStr, out int levelIndex) || levelIndex < 1 || levelIndex > 7 || levelIndex > currentDraft.AllLevels.Count)
        {
            ChatApi.SendMessage($"Invalid level index: {levelIndexStr}. Must be between 1 and 7.");
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
            ChatApi.SendMessage(ex.Message);
        }
    }

    private void HandleBan(Draft currentDraft, Team currentTeam, OnlineZeeplevel levelToPickOrBan)
    {
        if (currentTeam.Equals(_showdown.Match.Initiative) && !_hasInitiativeBanned)
        {
            _hasInitiativeBanned = true;
            _showdown.Match.Initiative = _showdown.Match.NonInitiative;
        }

        currentDraft.BanLevel(levelToPickOrBan);
        ChatApi.SendMessage($"{currentTeam.GetTag()} has banned {levelToPickOrBan.Name}");
    }

    private void HandlePick(Draft currentDraft, OnlineZeeplevel level, Team currentTeam, ZeepkistNetworkPlayer player)
    {
        if (player.IsLocal && currentDraft.IsDraftComplete())
        {
            Team showdownTeam = new Team("Showdown", "Showdown", "#ff0000");
            currentDraft.PickedLevels.Add(new DraftAction(level, showdownTeam, true));
            ChatApi.SendMessage($"{showdownTeam.GetTag()} has picked {level.Name}");
        }
        else
        {
            currentDraft.PickLevel(level);
            ChatApi.SendMessage($"{currentTeam.GetTag()} has picked {level.Name}");
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
        ChatApi.SendMessage($"Randomly selected map: {selectedLevel.Name}");

        _showdown.Match.CurrentDraft.PickedLevels.Add(new DraftAction(selectedLevel, new Team("Showdown", "Showdown", "#ff0000"), true));
    }
}