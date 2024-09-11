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
    private Team _teamA => _Showdown.CurrentMatch.TeamA;
    private Team _teamB => _Showdown.CurrentMatch.TeamB;

    private Team _currentlydrafting => IsTeamADrafting ? _teamA : _teamB;

    private string RightBlink => blink ? " <" : "  ";

    private string LeftBlink => blink ? "> " : "  ";

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        CommandPick.CommandInvoked += OnPick;
        CommandBan.CommandInvoked += OnBan;
        _Showdown.ShowdownTimer.Start();
        _Showdown.ShowdownTimer.Tick += OnTick;
    }


    public void Execute()
    {
        int teamAPad = _teamA.GetNameWithTag().Length;
        int teamBPad = 4 + _teamB.GetNameWithTag().Length;

        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .SetLineEffects(sle => sle
                .FontSize(20)
                .Italic()
            )
            .AddLine(serverMessage => serverMessage
                .AddContent(contentBuilder => contentBuilder
                    .AddText($"{_Showdown.CurrentMatch.TeamA.GetNameWithTag()} ")
                    .Color($"{_Showdown.CurrentMatch.TeamA.Color}")
                )
                .AddContent(contentBuilder => contentBuilder
                    .AddText("VS ")
                ).AddContent(contentBuilder => contentBuilder
                    .AddText($"{_Showdown.CurrentMatch.TeamB.GetNameWithTag()} ")
                    .Color($"{_Showdown.CurrentMatch.TeamB.Color}")
                )
            )
            .AddSeparator(teamAPad + teamBPad);

        if (IsTeamADrafting)
        {
            msg
                .AddHeadline(r => r
                    .AddContent(c => c
                        .AddText($"{_teamA.Tag}")
                        .Color(_teamA.Color)
                    )
                    .AddContent(c => c
                        .AddText("<<< Drafting".PadLeft(teamAPad + 2))
                        .Color("#ffAA00")
                        .Bold()
                        .AllCaps()
                    )
                    .AddContent(c => c
                        .AddText($"{_teamB.Tag}".PadLeft(teamBPad - 8))
                        .Color(_teamB.Color)
                    )
                );
        }
        else
        {
            msg
                .AddHeadline(r => r
                    .AddContent(c => c
                        .AddText($"{_teamA.Tag}")
                        .Color(_teamA.Color)
                    )
                    .AddContent(c => c
                        .AddText("Drafting".PadLeft(teamAPad + 2))
                        .Color("#ffAA00")
                        .Bold()
                        .AllCaps()
                    )
                    .AddContent(c => c
                        .AddText(" >>>")
                        .Color("#ffAA00")
                        .Bold()
                    ).AddContent(c => c
                        .AddText($"{_teamB.Tag}".PadLeft(teamBPad - 8))
                        .Color(_teamB.Color)
                    )
                );
        }

        foreach (ServerMessage serverMessage in drafting)
        {
            msg.AddMessage(serverMessage);
        }

        msg.Send(); // Send the message
    }


    public void Exit()
    {
        _Showdown.ShowdownTimer.Stop();
        _Showdown.ShowdownTimer.Tick -= OnTick;

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
        int teamAPad = _teamA.GetNameWithTag().Length;
        int teamBPad = 4 + _teamB.GetNameWithTag().Length;
        // Check if the wrong team picks 
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
            ChatApi.SendMessage(player.Username + " picked an invalid level index: " + levelIndex);
            return;
        }

        List<OnlineZeeplevel> playlist = ZeepkistNetwork.CurrentLobby.Playlist;
        levelNumber = Math.Clamp(levelNumber, 1, playlist.Count - 1);
        string levelDraftString = $"#{levelNumber} {ZeepkistNetwork.CurrentLobby.Playlist[levelNumber - 1].Name}";
        // Proceed with the valid level number
        ChatApi.SendMessage($"{player.Username} picked {levelDraftString}");

        if (IsTeamADrafting)
        {
            drafting.Add(new ServerMessage().AddLine(a => a
                .AddContent(b => b
                    .AddText("picked")
                    .Color("#00AA00")
                    .AddText($" -> {levelDraftString}")
                )
            ));
        }
        else
        {
            drafting.Add(new ServerMessage().AddLine(a => a
                .AddContent(b => b
                    .AddText("picked".PadLeft(teamAPad + teamBPad - $" -> {levelDraftString}".Length))
                    .Color("#00AA00")
                    .AddText($" -> {levelDraftString}")
                )
            ));
        }

        IsTeamADrafting = !IsTeamADrafting;
        Execute();
    }

    private void OnBan(ulong steamId, string levelIndex)
    {
        int teamAPad = _teamA.GetNameWithTag().Length;
        int teamBPad = 4 + _teamB.GetNameWithTag().Length;

        // Check if the wrong team bans
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

        // Get the level to be banned
        OnlineZeeplevel levelToBan = playlist[levelNumber - 1];


        string levelBanString = $"#{levelNumber} {levelToBan.Name}";

        // Notify the lobby that a level was banned
        ChatApi.SendMessage($"{player.Username} banned {levelBanString}");

        // Add the ban message to the drafting display
        if (IsTeamADrafting)
        {
            drafting.Add(new ServerMessage().AddLine(a => a
                .AddContent(b => b
                    .AddText("banned")
                    .Color("#AA0000")
                    .AddText($" -> {levelBanString}")
                )
            ));
        }
        else
        {
            drafting.Add(new ServerMessage().AddLine(a => a
                .AddContent(b => b
                    .AddText("banned".PadLeft(teamAPad + teamBPad - $" -> {levelBanString}".Length))
                    .Color("#AA0000")
                    .AddText($" -> {levelBanString}")
                )
            ));
        }

        // Toggle the drafting team
        IsTeamADrafting = !IsTeamADrafting;

        // Execute the next step (e.g., updating the drafting screen)
        Execute();
    }

    private void OnTimerTick()
    {
    }
}

public class Level
{
    public Level(OnlineZeeplevel onlineZeeplevel, Team draftedBy, bool isBanned)
    {
        OnlineZeeplevel = onlineZeeplevel;
        DraftedBy = draftedBy;
        IsBanned = isBanned;
    }

    public OnlineZeeplevel OnlineZeeplevel { get; }
    public Team DraftedBy { get; }
    public bool IsBanned { get; }
}