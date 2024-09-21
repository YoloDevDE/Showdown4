using System;
using Showdown4.Entities;
using Showdown4.Utils;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown
{
    public class State_Racing : IState
    {
        private Round _currentRound;
        private Leaderboard _leaderboard;
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

            // Subscribe to relevant events
            ZeepkistNetwork.PlayerResultsChanged += OnLeaderBoardUpdated;
            RacingApi.RoundEnded += OnRoundEnd;
        }

        public void Execute()
        {
            // Start a new round and add it to the match
            _currentRound = new Round(_showdownStateMachine.Match.RoundCounter() + 1);
            _showdownStateMachine.Match.Rounds.Add(_currentRound);
            _leaderboard = new Leaderboard(_currentRound); // Initialize leaderboard with the current round

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
            Racer racer;
            Result result;

            if (netRacer != null && netRacer.CurrentResult != null)
            {
                // Convert netRacer to Racer if valid
                racer = new Racer(netRacer.SteamID, netRacer.Username);
                result = new Result(racer, netRacer.CurrentResult.Time);
            }
            else
            {
                // Handle null netRacer, use default Racer and result
                racer = new Racer(0, "Unknown Racer");
                result = new Result(racer, double.MaxValue); // Default time to indicate no result
            }

            // Add the result to the round's leaderboard
            _currentRound.AddResult(result);

            // Send the updated leaderboard message
            SendTeamLeaderboard();
        }

        private void SendTeamLeaderboard()
        {
            // Generate and send the team-focused leaderboard message
            ServerMessage leaderboardMessage = _leaderboard.GenerateLeaderboardMessage(_teamA, _teamB);
            leaderboardMessage.Send();
        }
    }
}
