using System;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

public class State_LinkRacers : IState
{
    private const int CountdownDuration = 5;
    private Team _currentTeam;
    private bool _isCountdownRunning;

    public State_LinkRacers(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private Team _teamA => _Showdown.Match.TeamA;
    private Team _teamB => _Showdown.Match.TeamB;

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;

    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        CommandLinkRacer.CommandInvoked += OnLinkRacerToTeam;
    }

    public void Execute()
    {
        _currentTeam = _teamA;
        CheckIfRacersAreLinked();
    }

    public void Exit()
    {
        CommandLinkRacer.CommandInvoked -= OnLinkRacerToTeam;
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private void CheckIfRacersAreLinked()
    {
        if (_teamA.Racers.Count >= _teamA.MaxTeamSize && _teamB.Racers.Count >= _teamB.MaxTeamSize)
        {
            StartCountdown();
        }
        else
        {
            _currentTeam = _teamA.Racers.Count < _teamA.MaxTeamSize ? _teamA : _teamB;
            UpdateServerMessage(0);
        }
    }

    private void StartCountdown()
    {
        if (!_isCountdownRunning)
        {
            _isCountdownRunning = true;
            CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(
                CountdownDuration,
                UpdateServerMessage,
                InvokeFinish // Move to next state when countdown finishes
            ));
        }
    }

    private void UpdateServerMessage(int countdownTime)
    {
        // Create a consistent server message with appended countdown at the end
        ServerMessage msg = ServerMessageLinkedRacers();


        // Append the countdown timer if it's running
        if (_isCountdownRunning)
        {
            msg.AddSeparator()
                .AddLine(line => line
                    .AddBlock("All Racers are linked to their Teams! ", block => block.Color("#00ff00").Bold()))
                .AddSeparator()
                .AddLine(line => line
                    .AddBlock("Continue to")
                    .AddBlock("'Select Initiative'", block => block.Color("#ffff00"))
                    .AddBlock("in")
                    .AddBlock($"{countdownTime}", block => block.Color("#00ff00"))
                    .AddBlock("seconds...")
                );
        }

        msg.Send();
    }

    private void OnLinkRacerToTeam(ulong steamId)
    {
        string steamName = ZeepkistNetworkService.GetSteamNameFromSteamId(steamId);
        Racer racer = new Racer(steamId, steamName);
        _currentTeam.AddRacer(racer); // Add racer to current team

        CheckIfRacersAreLinked(); // Check again after each link
    }

    private ServerMessage ServerMessageLinkedRacers()
    {
        ServerMessage msg = new ServerMessage()
                .ShowdownHeader()
                .AddLine(line => line
                    .AddBlock($"{_teamA.GetNameWithTag()}", b => b.Color(_teamA.Color))
                    .AddBlock("VS")
                    .AddBlock($"{_teamB.GetNameWithTag()}", b => b.Color(_teamB.Color))
                )
                .AddSeparator()
                .AddLine(line => line
                    .AddBlock("Waiting for all racers of")
                    .AddBlock($"{_currentTeam.GetTag()} ", format => format.Color($"{_currentTeam.Color}").Bold())
                    .AddBlock("to link with their team by writing")
                    .AddBlock("'!link'", format => format.Color("#ffff00").Bold())
                    .AddBlock("in the chat.")
                )
                .AddSeparator()
                .AddLine("Currently linked racers:")
                .AddLine(line => line
                    .AddBlock($"{_teamA.GetTag()} ", format => format.Color(_teamA.Color))
                    .AddBlock($"{_teamA.GetLinkedRacersToString()}")
                )
                .AddLine(line => line
                    .AddBlock($"{_teamB.GetTag()} ", format => format.Color(_teamB.Color))
                    .AddBlock($"{_teamB.GetLinkedRacersToString()}")
                )
            ;
        return msg;
    }
}