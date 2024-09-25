using System;
using Showdown4.Entities;
using Showdown4.Utils;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class State_Racing : IState
{
    private Round _currentRound;
    private Leaderboard _leaderboard;
    private Team _teamA, _teamB;

    public State_Racing(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;

    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        _teamA = _Showdown.Match.TeamA;
        _teamB = _Showdown.Match.TeamB;
        // Start a new round and add it to the match
        _Showdown.Match.AddRound(new Round(_teamA, _teamB)); // Round is initialized with max times for all racers
        _currentRound = _Showdown.Match.CurrentRound;

        // Subscribe to relevant events
        ZeepkistNetwork.PlayerResultsChanged += OnLeaderBoardUpdated;
        RacingApi.RoundEnded += OnRoundEnd;
    }


    public void Execute()
    {
        _leaderboard = new Leaderboard(_currentRound); // Initialize leaderboard with the current round
        // Announce the start of the round
        ChatApi.SendMessage(
            new ChatMessage.Builder().ClearChat()
                .DashedLine().NewLine()
                .CenterTextLine($"Round {_Showdown.Match.RoundCounter()} started").NewLine()
                .DashedLine().NewLine()
                .CenterTextLine($"{_teamA.GetTag()}").NewLine()
                .CenterTextLine("vs").NewLine()
                .CenterTextLine($"{_teamB.GetTag()}").NewLine()
                .DashedLine().NewLine()
                .TextLine("Good Luck, Have Fun! :smile:").Build().Message
        );

        // Send default leaderboard at the start of the round
        SendDefaultLeaderboard();
    }

    public void Exit()
    {
        // Unsubscribe from events when exiting the state
        ZeepkistNetwork.PlayerResultsChanged -= OnLeaderBoardUpdated;
        RacingApi.RoundEnded -= OnRoundEnd;
    }

    private void OnRoundEnd()
    {
        Finished?.Invoke();
    }

    private void OnLeaderBoardUpdated(ZeepkistNetworkPlayer netRacer)
    {
        // Check if netRacer or netRacer.CurrentResult is null
        if (netRacer?.CurrentResult == null)
        {
            ChatApi.AddLocalMessage("Error: Received an invalid racer or result.");
            return;
        }

        // Convert netRacer to Racer if valid
        Racer racer = new Racer(netRacer.SteamID, netRacer.Username);
        Result result = new Result(racer, netRacer.CurrentResult.Time);

        // Ensure _currentRound is initialized
        if (_currentRound == null)
        {
            ChatApi.AddLocalMessage("Error: _currentRound is not initialized.");
            return;
        }

        // Add the result to the round's leaderboard
        _currentRound.AddResult(result);

        // Send the updated leaderboard message
        SendTeamLeaderboard();
    }

    private void SendDefaultLeaderboard()
    {
        // Generate a default leaderboard message with placeholder data
        ServerMessage leaderboardMessage = _leaderboard.GenerateDefaultLeaderboardMessage(_teamA, _teamB);
        leaderboardMessage.Send();
    }

    private void SendTeamLeaderboard()
    {
        // Generate and send the team-focused leaderboard message
        ServerMessage leaderboardMessage = _leaderboard.GenerateLeaderboardMessage();
        leaderboardMessage.Send();
    }
}