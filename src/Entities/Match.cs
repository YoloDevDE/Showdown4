using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Managers;
using ZeepkistNetworking;

namespace Showdown4.Entities;

public class Match
{
	public Match(Team teamA, Team teamB)
	{
		TeamA = teamA;
		TeamB = teamB;
		Initiative = teamA;
	}

	public Team TeamA { get; set; }
	public Team TeamB { get; set; }
	public List<Round> Rounds { get; } = [];

	public List<Draft> Drafts { get; } = new();
	public Draft CurrentDraft => Drafts[^1];
	public Round CurrentRound => Rounds[^1];
	public List<OnlineZeeplevel> TrackedLevels { get; } = [];
	public Team Initiative { get; set; }
	public Team NonInitiative => Initiative == TeamA ? TeamB : TeamA;

	public string ScoreWithFullNameColored()
	{
		return $"{TeamA.GetFullColoredTagAndName()} {TeamA.Wins}:{TeamB.Wins} {TeamB.GetFullColoredTagAndName()}";
	}

	public string ScoreWithFullNameColoredAndPadded()
	{
		int maxLength = Math.Max(TeamA.GetFullColoredTagAndName().Length, TeamB.GetFullColoredTagAndName().Length);
		return
			$"{TeamA.GetFullColoredTagAndName().PadLeft(maxLength)} {TeamA.Wins}:{TeamB.Wins} {TeamB.GetFullColoredTagAndName().PadRight(maxLength)}";
	}

	public string ScoreColored()
	{
		return
			$"<color={TeamA.Color}>{TeamA.GetTag()}</color> {TeamA.Wins}:{TeamB.Wins} <color={TeamB.Color}>{TeamB.GetTag()}</color>";
	}

	public string Score()
	{
		return $"{TeamA.GetColoredTag()} {TeamA.Wins}:{TeamB.Wins} {TeamB.GetColoredTag()}";
	}

	public string ScoreOnly()
	{
		return $"{TeamA.Wins}:{TeamB.Wins}";
	}

	public void AddRound(Round round)
	{
		Rounds.Add(round);
	}

	public void AddDraft()
	{
		List<OnlineZeeplevel> unAvaiableLevels = new();
		List<OnlineZeeplevel> allLevels =
			PlaylistManager.GetLocalLevelsByPlaylistName(MyConfig.CompetitionLevelsPlaylistNameConfig.Value);
		if (Drafts.Count > 0)
			unAvaiableLevels =
				new List<OnlineZeeplevel>(CurrentDraft.PickedLevels.Select(draftAction => draftAction.Level));

		Draft draft = new(Initiative, NonInitiative, allLevels, unAvaiableLevels);
		Drafts.Add(draft);
	}

	public int RoundCounter()
	{
		return Rounds.Count;
	}

	public Team GetTeamBySteamId(ulong steamId)
	{
		if (TeamA.Racers.Any(player => player.SteamId == steamId)) return TeamA;

		if (TeamB.Racers.Any(player => player.SteamId == steamId)) return TeamB;

		return null;
	}
}