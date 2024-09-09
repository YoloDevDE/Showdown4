using System;
using Showdown4.Domain.States.Showdown;
using Showdown4.Tmp;
using Showdown4.Utils;

namespace Showdown4.Domain.States;

public class State_Drafting : IState
{
    public State_Drafting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;
    private Team _teamA => _Showdown.CurrentMatch.TeamA;
    private Team _teamB => _Showdown.CurrentMatch.TeamB;

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
    }

    public void Execute()
    {
        int teamAPad = _teamA.GetNameWithTag().Length;
        int teamBPad = 4 + _teamB.GetNameWithTag().Length;

        new ServerMessage()
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
            .AddLine(line => line
                .AddContent(content => content
                    .AddText($"{_teamA.Racers[0].SteamName}".PadRight(teamAPad))
                    .Color(_teamA.Color)
                )
                .AddContent(content => content
                    .AddText($"{_teamB.Racers[0].SteamName}".PadLeft(teamBPad))
                    .Color(_teamB.Color)
                )
            )
            .AddLine(line => line
                .AddContent(content => content
                    .AddText($"{_teamA.Racers[1].SteamName}".PadRight(teamAPad))
                    .Color(_teamA.Color)
                )
                .AddContent(content => content
                    .AddText($"{_teamB.Racers[1].SteamName}".PadLeft(teamBPad))
                    .Color(_teamB.Color)
                )
            )
            .AddSeparator()
            .AddHeadline(r => r
                .AddContent(c => c
                    .AddText("Drafting Phase!")
                    .Color("#ff88ff"))
            )
            .Send(); // Send the message
    }

    public void Exit()
    {
    }
}