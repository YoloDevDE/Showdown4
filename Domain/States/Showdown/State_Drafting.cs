using System;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Tmp;
using Showdown4.Utils;
using ZeepkistClient;
using ZeepSDK.Chat;

namespace Showdown4.Domain.States.Showdown;

public class State_Drafting : IState
{
    private readonly CountdownTimer timer;
    private bool IsTeamADrafting = true;

    public State_Drafting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
        timer = new CountdownTimer(30);
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;
    private Team _teamA => _Showdown.Match.TeamA;
    private Team _teamB => _Showdown.Match.TeamB;
    private Team _currentlydrafting => IsTeamADrafting ? _teamA : _teamB;
    private Team _notdrafting => IsTeamADrafting ? _teamB : _teamA;
    private bool draftComplete => _teamA.Picks == 0 && _teamB.Picks == 0 || _teamA.Picks == 0 && _teamB.Picks == 0 && _teamA.Bans == 0 && _teamB.Bans == 0;

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        CommandPick.CommandInvoked += OnPick;
        CommandBan.CommandInvoked += OnBan;
        timer.CountdownTick += OnTick;
        timer.CountdownFinished += OnCountdownFinished;
        timer.Start();
    }

    public void Execute()
    {
        // Creating the header and base message
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock($"{_Showdown.Match.TeamA.GetNameWithTag()}", b => b.Color(_Showdown.Match.TeamA.Color))
                .AddBlock("VS")
                .AddBlock($"{_Showdown.Match.TeamB.GetNameWithTag()}", b => b.Color(_Showdown.Match.TeamB.Color))
            )
            .AddSeparator();
        if (draftComplete)
        {
            timer.CountdownFinished -= OnCountdownFinished;
            timer.Stop();
            msg
                .AddLine(line => line
                    .AddBlock("Draft complete! Waiting for everyone to /vs or timer reached 0: ")
                    .AddBlock($"{TimeFormatter.FormatDuration(timer.SecondsLeft)}", format => format.Color("#ffff00"))
                    .Bold()
                    .AllCaps()
                    .Color("#00aa00")
                );
        }
        else
        {
            msg.AddLine(line => line
                .AddBlock(_currentlydrafting.Tag, format => format.Bold()
                    .Color(_currentlydrafting.Color))
                .AddBlock("is drafting:", format => format.Bold())
                .AddBlock($"{TimeFormatter.FormatDuration(timer.SecondsLeft)}", format => format.Bold()
                    .Color(timer.SecondsLeft <= 10 ? "#ff0000" : "#ffffff")
                )
                .AddBlock($"{(timer.SecondsLeft <= 10 ? $"It's getting close! If you don't draft <color={_notdrafting.Color}>{_notdrafting.Tag}</color> will take your turn!" : "")}")
            );
        }

        msg.AddSeparator();
        // Adding previous drafted messages
        for (int index = 0; index < _Showdown.Match.LevelDrafts.Count; index++)
        {
            LevelDraft levelDraft = _Showdown.Match.LevelDrafts.ToList()[index];

            msg.AddInLine(line => line
                .AddBlock($"{index + 1}# " + levelDraft.Level.OnlineZeeplevel.Name)
            );
            switch (levelDraft.LevelDraftType)
            {
                case LevelDraftType.BAN:
                    msg
                        .AddInLine(line => line
                            .Indent("500")
                            .AddBlock("Banned", format => format.Color("#aa0000"))
                            .AddBlock("by")
                            .AddBlock($"{levelDraft.Team.Tag}", format => format.Color(levelDraft.Team.Color))
                        );
                    break;
                case LevelDraftType.PICK:
                    msg
                        .AddInLine(line => line
                            .Indent("500")
                            .AddBlock("Picked", format => format.Color("#00aa00"))
                            .AddBlock("by")
                            .AddBlock($"{levelDraft.Team.Tag}", format => format.Color(levelDraft.Team.Color))
                        );
                    break;
            }

            msg.AddLine("");
        }


        msg.Send(); // Send the message
    }

    public void Exit()
    {
        CommandPick.CommandInvoked -= OnPick;
        CommandBan.CommandInvoked -= OnBan;
        timer.CountdownTick -= OnTick;
        timer.CountdownFinished -= OnCountdownFinished;
        timer.Stop();
    }

    private void OnCountdownFinished()
    {
        ChatApi.SendMessage($"{_currentlydrafting.GetNameWithTag()} failed to draft. It's now {_notdrafting.GetNameWithTag()} turn!");
        SwitchDrafter();
    }

    private void OnTick()
    {
        Execute();
    }


    private Level GetLevelFromIndex(string levelIndex, ZeepkistNetworkPlayer player)
    {
        // Convert levelIndex to an integer
        if (!int.TryParse(levelIndex, out int levelNumber))
        {
            // Handle invalid level index
            ChatApi.SendMessage(player.Username + " tried to use an invalid level index: " + levelIndex);
            return null;
        }

        // Get the playlist and clamp the level number
        levelNumber = Math.Clamp(levelNumber, 1, 7);
        return _Showdown.Match.LevelDrafts.ToList()[levelNumber - 1].Level;
    }

    private void HandleDraftAction(ulong steamId, string levelIndex, Action<Team, Level> draftAction, string actionType)
    {
        // Check if the player is part of the currently drafting team
        if (_currentlydrafting.Racers.All(racer => racer.SteamId != steamId))
        {
            return;
        }

        // Retrieve the player information
        ZeepkistNetwork.TryGetPlayer(steamId, out ZeepkistNetworkPlayer player);

        // Use helper method to get the level
        Level level = GetLevelFromIndex(levelIndex, player);
        if (level == null)
        {
            return;
        }

        try
        {
            // Perform the draft action (ban or pick)
            draftAction(_currentlydrafting, level);

            // Toggle the drafting team after the action
            SwitchDrafter();
            // Update the UI with the current state
            Execute();
        }
        catch (InvalidOperationException ex)
        {
            // Send an error message to the player if any exception occurs
            ChatApi.SendMessage($"{player.Username} -> {ex.Message}");
        }
    }

    private void SwitchDrafter()
    {
        IsTeamADrafting = !IsTeamADrafting;
        timer.Reset(30);
        Execute();
    }

    // Refactored OnPick method
    private void OnPick(ulong steamId, string levelIndex)
    {
        HandleDraftAction(steamId, levelIndex, _Showdown.Match.DoPick, "pick");
    }


    // Refactored OnBan method
    private void OnBan(ulong steamId, string levelIndex)
    {
        HandleDraftAction(steamId, levelIndex, _Showdown.Match.DoBan, "ban");
    }
}