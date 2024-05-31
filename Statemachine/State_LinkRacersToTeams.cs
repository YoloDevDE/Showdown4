using Showdown4.Commands;
using Showdown4.Tmp;
using Showdown4.Utils;
using ZeepSDK.Chat;
using ZeepSDK.Messaging;

namespace Showdown4.Statemachine;

public class State_LinkRacersToTeams : IState
{
    private readonly TeamService _teamService = new TeamService();
    private readonly ZeepkistNetworkService _zeepkistNetworkService = new ZeepkistNetworkService();
    private MatchStateMachine _context;
    private bool _isTeamASet;
    private bool _isTeamBSet;

    private MatchService _matchService = new MatchService();
    private RoundService _roundService = new RoundService();
    private bool _setNextTeam;

    private Team _teamA, _teamB, _currentTeam;


    public void Enter(IStateMachine context)
    {
        _setNextTeam = false;
        _context = (MatchStateMachine)context;

        _teamA = _context.CurrentMatch.TeamA;
        _teamB = _context.CurrentMatch.TeamB;

        CommandLinkPlayerToTeam.CommandInvoked += OnLinkRacerToTeam;

        LobbyController.SetServerMessage(ServerMessageColor.orange,
            new ChatMessage.Builder()
                .TextLine("Linking up Racers:").NewLine()
                .TextLine($"{_teamA.GetNameWithTag()} vs {_teamB.GetNameWithTag()} {_teamB.GetTag()}")
                .Build().Message);

        CheckIfRacersAreLinked();
    }

    public void Exit()
    {
        MessengerApi.Log("Match Preparation finished!");
        CommandLinkPlayerToTeam.CommandInvoked -= OnLinkRacerToTeam;
    }

    public void CheckIfRacersAreLinked()
    {
        if (_teamA.Racers.Count < _teamA.MaxTeamSize)
        {
            _currentTeam = _teamA;
        }
        else if (_teamB.Racers.Count < _teamB.MaxTeamSize)
        {
            _currentTeam = _teamB;
        }
        else
        {
            _context.TransitionTo(_context, new State_PreRacing());
            return;
        }

        if (_currentTeam.Racers.Count < 1)
        {
            ChatApi.SendMessage(new ChatMessage.Builder().NewLine()
                .TextLine($"{_currentTeam.GetNameWithTag()}: If you identify yourself with this team then please proceed to write #link in the chat :smile:").NewLine()
                .DashedLine().Build().Message);
        }
    }

    private void OnLinkRacerToTeam(ulong steamId)
    {
        string steamName = _zeepkistNetworkService.GetSteamNameFromSteamId(steamId);
        Racer racer = new Racer(steamId, steamName);
        // First fill teamA then fill teamB
        _teamService.AddRacer(_currentTeam, racer);
        ChatApi.SendMessage(
            new ChatMessage.Builder().NewLine()
                .DashedLine().NewLine()
                .TextLine($"{steamName} has been linked to {_currentTeam.GetNameWithTag()}").NewLine()
                .TextLine($"Currently linked Racers: {_currentTeam.GetLinkedRacersToString()}").NewLine()
                .DashedLine().Build().Message
        );
        CheckIfRacersAreLinked();
    }
}