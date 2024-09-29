using System;
using System.Collections;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
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
        _Showdown.Match.AddRound(new Round(_teamA, _teamB)); // Initialize round
        _currentRound = _Showdown.Match.CurrentRound;

        ZeepkistNetwork.PlayerResultsChanged += OnLeaderBoardUpdated;
        RacingApi.RoundEnded += OnRoundEnd;

        ChatApi.SendMessage(
            new ChatMessage.Builder().ClearChat()
                .DashedLine().NewLine()
                .TextLine($"Round {_Showdown.Match.RoundCounter()} started").NewLine()
                .DashedLine().NewLine()
                .TextLine($"{_Showdown.Match.Score()}").NewLine()
                .DashedLine().NewLine()
                .TextLine("Good Luck, Have Fun! :smile:").Build().Message
        );
        // Start the cool race intro message for the first 10 seconds
        CoroutineManager.Instance.StartExternalCoroutine(DisplayRaceIntroMessage());
    }

    public void Execute()
    {
        _leaderboard = new Leaderboard(_currentRound);
    }

    public void Exit()
    {
        ZeepkistNetwork.PlayerResultsChanged -= OnLeaderBoardUpdated;
        RacingApi.RoundEnded -= OnRoundEnd;
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private void OnRoundEnd()
    {
        InvokeFinish();
    }

    private void OnLeaderBoardUpdated(ZeepkistNetworkPlayer netRacer)
    {
        if (netRacer?.CurrentResult == null)
        {
            ChatApi.AddLocalMessage("Error: Received an invalid racer or result.");
            return;
        }

        Racer racer = new Racer(netRacer.SteamID, netRacer.Username);
        Result result = new Result(racer, netRacer.CurrentResult.Time);

        if (_currentRound == null)
        {
            ChatApi.AddLocalMessage("Error: _currentRound is not initialized.");
            return;
        }

        _currentRound.AddResult(result);
        SendTeamLeaderboard();
    }

    private void SendTeamLeaderboard()
    {
        ServerMessage leaderboardMessage = _leaderboard.GenerateLeaderboardMessage(_Showdown.Match);
        leaderboardMessage.Send();
    }

    // Coroutine for showing the cool intro message for the first 10 seconds
// Coroutine for showing the cool intro message for the first 10 seconds
// Coroutine for showing the cool intro message for the first 10 seconds
    private IEnumerator DisplayRaceIntroMessage()
    {
        ServerMessage introMessage = new ServerMessage()
                .ShowdownHeader()
                .AddLine(line => line
                    .AddBlock($"{_teamA.GetNameWithTag()}", b => b.Color(_teamA.Color))
                    .AddBlock("VS")
                    .AddBlock($"{_teamB.GetNameWithTag()}", b => b.Color(_teamB.Color))
                )
            ;

        // Determine the number of rows needed based on the maximum number of racers in either team
        int maxRacers = Mathf.Max(_teamA.Racers.Count, _teamB.Racers.Count);

        for (int i = 0; i < maxRacers; i++)
        {
            Racer teamARacer = i < _teamA.Racers.Count ? _teamA.Racers[i] : null;
            Racer teamBRacer = i < _teamB.Racers.Count ? _teamB.Racers[i] : null;

            introMessage.AddLine(line =>
            {
                if (teamARacer != null)
                {
                    line.AddBlock($"{teamARacer.SteamName}", f => f.Color(_teamA.Color));
                }
                else
                {
                    line.AddBlock(" "); // Empty space for alignment
                }


                if (teamBRacer != null)
                {
                    line.AddBlock($"{teamBRacer.SteamName}".PadLeft(1 + _teamA.GetNameWithTag().Length + "VS".Length + _teamB.GetNameWithTag().Length - teamARacer.SteamName.Length), f => f.Color(_teamB.Color));
                }
            });
        }

        introMessage.AddSeparator(_teamA.GetNameWithTag().Length + 4 + _teamB.GetNameWithTag().Length);
        introMessage.Send();

        yield return new WaitForSeconds(10);

        // After 10 seconds, clear the intro message and proceed to the normal race flow
        ChatApi.SendMessage(new ChatMessage.Builder().ClearChat().Build().Message);
        SendTeamLeaderboard();
    }
}