using System;
using Showdown4.Entities;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

internal class State_PostRacing : IState
{
    public State_PostRacing(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;


    public IStateMachine StateMachine { get; }

    public void Enter()
    {
        RacingApi.RoundStarted += OnRoundStarted;
    }

    public void Execute()
    {
        Team winnerTeam = _Showdown.Match.CurrentRound.GetWinnerTeam;
        winnerTeam.AddWin();

        // Get the current round index
        int currentRoundCounter = _Showdown.Match.RoundCounter();

        // Default message in case of an error
        string nextLevelMessage = "";
        // Check if the current round index is within the range of picked levels
        if (currentRoundCounter < _Showdown.Match.CurrentDraft.PickedLevels.Count)
        {
            DraftAction nextLevel = _Showdown.Match.CurrentDraft.PickedLevels[_Showdown.Match.RoundCounter()];
            // If valid, show the next level's name
            nextLevelMessage += new ChatMessage.Builder().TextLine($"Starting Round {_Showdown.Match.RoundCounter() + 1}").NewLine()
                .DashedLine().NewLine()
                .TextLine($"Level <color=#00ffff>'{nextLevel.Level.Name}'</color>").NewLine()
                .TextLine($"picked by {nextLevel.Team.GetColoredTag()}")
                .Build().Message;
        }
        else if (winnerTeam.Wins > 1)
        {
            nextLevelMessage += "Upcoming -> Intermission";
        }
        else
        {
            _Showdown.Match.Initiative = _Showdown.Match.NonInitiative;
            nextLevelMessage +=
                new ChatMessage.Builder().TextLine("Upcoming -> Draftphase II").NewLine()
                    .DashedLine().NewLine()
                    .TextLine($"Initiative: {_Showdown.Match.Initiative.GetColoredTag()}").NewLine()
                    .TextLine("Prepare yourself! It will start almost immediately!")
                    .Build().Message;
        }


        // Create a cool and separated chat message using your original ChatMessage class
        ChatMessage.SendCustomMessage(
            new ChatMessage.Builder()
                .ClearChat()
                .DashedLine().NewLine() // Dashed separator
                .TextLine($"<b>Round {currentRoundCounter}</b> over!").NewLine() // Centered round completion message
                .TextLine($"{winnerTeam.GetColoredTag()} scored").NewLine() // Display the winning team
                .DashedLine().NewLine() // Dashed line separating content
                .TextLine(nextLevelMessage).NewLine() // Show the next level or intermission
                .DashedLine().NewLine() // Dashed line to separate next section
                .TextLine($"{_Showdown.Match.Score()}") // Display the score
                .Build().Message); // Final dashed line for closure
    }

    public void Exit()
    {
        RacingApi.RoundStarted -= OnRoundStarted;
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    public event Action Finished;


    private void OnRoundStarted()
    {
        Finished?.Invoke();
    }
}