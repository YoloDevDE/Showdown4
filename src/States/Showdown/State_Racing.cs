using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Entities;
using Showdown4.Utils;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class State_Racing : IState
{
    private Team _teamA, _teamB;

    public State_Racing(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _showdownStateMachine => StateMachine as ShowdownStateMachine;

    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        _teamA = _showdownStateMachine.Match.TeamA;
        _teamB = _showdownStateMachine.Match.TeamB;

        // Start a new round and add it to the match
        Round round = new(_showdownStateMachine.Match.RoundCounter() + 1);
        _showdownStateMachine.Match.Rounds.Add(round);

        // Set the lobby time to 300 seconds (5 minutes)
        ChatApi.SendMessage("/settime 300");

        // Announce the start of the round
        ChatApi.SendMessage(
            new ChatMessage.Builder().ClearChat()
                .DashedLine().NewLine()
                .CenterTextLine($"Round {_showdownStateMachine.Match.RoundCounter()} started").NewLine()
                .DashedLine().NewLine()
                .CenterTextLine($"{_teamA.GetTag()}").NewLine()
                .CenterTextLine("vs").NewLine()
                .CenterTextLine($"{_teamB.GetTag()}").NewLine()
                .DashedLine().NewLine()
                .TextLine("Good Luck, Have Fun! :smile:").Build().Message
        );

        // Subscribe to relevant events
        ZeepkistNetwork.PlayerResultsChanged += OnLeaderBoardUpdated;
        RacingApi.RoundEnded += OnRoundEnd;
    }

    public void Execute()
    {
    }

    public void Exit()
    {
        // Unsubscribe from events when exiting the state
        ZeepkistNetwork.PlayerResultsChanged -= OnLeaderBoardUpdated;
        RacingApi.RoundEnded -= OnRoundEnd;
    }

    private void OnRoundEnd()
    {
        // When the round ends, invoke the Finished event to signal the state transition
        ChatApi.SendMessage("Round has ended. Moving to the next state.");
        Finished?.Invoke();
    }

    private void OnLeaderBoardUpdated(ZeepkistNetworkPlayer netRacer)
    {
        // Retrieve the leaderboard and send it as a server message
        SendLeaderBoard();
    }

    private void SendLeaderBoard()
    {
        // Filter out valid players with valid results
        IEnumerable<KeyValuePair<uint, ZeepkistNetworkPlayer>> players = ZeepkistNetwork.Players
            .Where(p => p.Value != null && p.Value.CurrentResult != null) // Ensure player and result are not null
            .OrderByDescending(p => p.Value.CurrentResult.Time) // Assuming players are ranked by time
            .Take(10); // Limit to top 10 players

        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line.AddBlock("Current Leaderboard:").Bold());

        int rank = 1;
        foreach (KeyValuePair<uint, ZeepkistNetworkPlayer> player in players)
        {
            msg.AddLine(line => line
                .AddBlock($"#{rank} ", f => f.Bold())
                .AddBlock($"{player.Value.Username}", f => f.Color("#00ff00")) // Player name in green
                .AddBlock($" - {player.Value.CurrentResult.Time} seconds", f => f.Color("#ffffff"))); // Time in white
            rank++;
        }

        msg.Send(); // Send the leaderboard message
    }
}