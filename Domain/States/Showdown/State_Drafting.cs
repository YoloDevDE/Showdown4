using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Tmp;
using Showdown4.Utils;
using ZeepkistClient;
using ZeepkistNetworking;
using ZeepSDK.Chat;

namespace Showdown4.Domain.States.Showdown;

public class State_Drafting : IState
{
    private readonly List<ServerMessage> drafting = new List<ServerMessage>();
    private bool blink = true;
    private List<Level> DraftedLevels = new List<Level>();
    private bool IsTeamADrafting = true;

    public State_Drafting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;
    private Team _teamA => _Showdown.Match.TeamA;
    private Team _teamB => _Showdown.Match.TeamB;
    private Team _currentlydrafting => IsTeamADrafting ? _teamA : _teamB;

    private string RightBlink => blink ? " <" : "  ";
    private string LeftBlink => blink ? "> " : "  ";

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        CommandPick.CommandInvoked += OnPick;
        CommandBan.CommandInvoked += OnBan;
        _Showdown.Timer.Start();
        _Showdown.Timer.Tick += OnTick;
    }

    public void Execute()
    {
        int teamAPad = _teamA.GetNameWithTag().Length;
        int teamBPad = 4 + _teamB.GetNameWithTag().Length;

        // Creating the header and base message
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock($"{_Showdown.Match.TeamA.GetNameWithTag()} ", b => b.Color(_Showdown.Match.TeamA.Color))
                .AddBlock("VS ")
                .AddBlock($"{_Showdown.Match.TeamB.GetNameWithTag()} ", b => b.Color(_Showdown.Match.TeamB.Color))
            )
            .AddSeparator(teamAPad + teamBPad);


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
                            .AddBlock("Banned", format => format.Color("#aa0000"))
                            .AddBlock("by")
                            .AddBlock($" {levelDraft.Team.Tag} ", format => format.Color(levelDraft.Team.Color))
                        );
                    break;
                case LevelDraftType.PICK:
                    msg
                        .AddInLine(line => line
                            .AddBlock("Picked", format => format.Color("#00aa00"))
                            .AddBlock("by")
                            .AddBlock($" {levelDraft.Team.Tag} ", format => format.Color(levelDraft.Team.Color))
                        );
                    break;
            }

            msg.AddLine("");
        }

        msg.Send(); // Send the message
    }

    public void Exit()
    {
        _Showdown.Timer.Stop();
        _Showdown.Timer.Tick -= OnTick;
        CommandBan.CommandInvoked -= OnBan;
        CommandPick.CommandInvoked -= OnPick;
    }

    private void OnTick()
    {
        blink = !blink;
        Execute();
    }

    private void OnPick(ulong steamId, string levelIndex)
    {
    }

    private void OnBan(ulong steamId, string levelIndex)
    {
        if (_currentlydrafting.Racers.All(racer => racer.SteamId != steamId))
        {
            return;
        }

        // Retrieve the player information
        ZeepkistNetwork.TryGetPlayer(steamId, out ZeepkistNetworkPlayer player);

        // Convert levelIndex to an integer
        if (!int.TryParse(levelIndex, out int levelNumber))
        {
            // Handle invalid level index
            ChatApi.SendMessage(player.Username + " tried to ban an invalid level index: " + levelIndex);
            return;
        }

        // Get the playlist and clamp the level number
        List<OnlineZeeplevel> playlist = ZeepkistNetwork.CurrentLobby.Playlist;
        levelNumber = Math.Clamp(levelNumber, 1, playlist.Count);
        Level level = _Showdown.Match.LevelDrafts.ToList()[levelNumber - 1].Level;


        _Showdown.Match.DoBan(_currentlydrafting, level);
        IsTeamADrafting = !IsTeamADrafting;
    }
}