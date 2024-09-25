using System;
using Showdown4.Entities;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

internal class State_PostRacing : IState
{
    public State_PostRacing(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _showdownStateMachine => StateMachine as ShowdownStateMachine;


    public IStateMachine StateMachine { get; }

    public void Enter()
    {
        RacingApi.RoundStarted += OnRoundStarted;
    }

    public void Execute()
    {
        Team winnerTeam = _showdownStateMachine.Match.CurrentRound.GetWinnerTeam;
        winnerTeam.AddWin();

        // Get the current round index
        int currentRoundCounter = _showdownStateMachine.Match.RoundCounter();

        // Default message in case of an error
        string nextLevelMessage = "Upcoming -> ";

        // Check if the current round index is within the range of picked levels
        if (currentRoundCounter < _showdownStateMachine.Match.CurrentDraft.PickedLevels.Count)
        {
            // If valid, show the next level's name
            nextLevelMessage +=
                $"Round {currentRoundCounter + 1}<br>{_showdownStateMachine.Match.CurrentDraft.PickedLevels[currentRoundCounter].Level.Name}<br>picked by '{_showdownStateMachine.Match.CurrentDraft.PickedLevels[currentRoundCounter].Team.GetTag()}'";
        }
        else if (winnerTeam.Wins > 1)
        {
            nextLevelMessage += "Intermission";
        }
        else
        {
            nextLevelMessage += "Draftphase II";
        }

        // Create a cool and separated chat message using your original ChatMessage class
        ChatApi.SendMessage(
            new ChatMessage.Builder()
                .ClearChat()
                .DashedLine().NewLine() // Dashed separator
                .CenterTextLine($"Round {currentRoundCounter} finished!").NewLine() // Centered round completion message
                .TextLine($"Team '{winnerTeam.GetNameWithTag()}' scored!").NewLine() // Display the winning team
                .DashedLine().NewLine() // Dashed line separating content
                .TextLine("Current Standings:").NewLine() // Standings section
                .TextLine($"{_showdownStateMachine.Match.Score()}").NewLine() // Display the score
                .DashedLine().NewLine() // Dashed line to separate next section
                .TextLine(nextLevelMessage).NewLine() // Show the next level or intermission
                .DashedLine().Build().Message); // Final dashed line for closure
    }

    public void Exit()
    {
        RacingApi.RoundStarted -= OnRoundStarted;
    }

    public event Action Finished;


    private void OnRoundStarted()
    {
        Finished?.Invoke();
    }
}