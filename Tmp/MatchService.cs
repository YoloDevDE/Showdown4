namespace Showdown4.Tmp;

public class MatchService
{
    public void AddRound(Match match, Round round)
    {
        match.Rounds.Add(round);
    }

    public void SetTeams(Team teamA, Team teamB, Match match)
    {
        match.TeamA = teamA;
        match.TeamB = teamB;
    }
}