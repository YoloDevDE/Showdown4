using System;
using System.Collections.Generic;
using System.Linq;
using ZeepkistNetworking;

namespace Showdown4.Entities;

public class Draft
{
	private readonly Team _teamA;
	private readonly Team _teamB;
	public Team CurrentTeam;

	public Draft(Team teamA, Team teamB, List<OnlineZeeplevel> allLevels,
		List<OnlineZeeplevel> unAvailableLevels = null)
	{
		AllLevels = new List<OnlineZeeplevel>(allLevels);

		// Initialize unavailable levels as an empty list if it's null
		unAvailableLevels ??= new List<OnlineZeeplevel>();

		// Make a copy of all levels as available levels
		AvailableLevels = new List<OnlineZeeplevel>(AllLevels);

		// Remove all unavailable levels from available levels by comparing their UID
		AvailableLevels.RemoveAll(level => unAvailableLevels.Any(unavailable => unavailable.UID == level.UID));

		UnAvailableLevels = new List<OnlineZeeplevel>(unAvailableLevels);

		_teamA = teamA;
		_teamB = teamB;
		CurrentTeam = teamA;
	}


	private Team OtherTeam => CurrentTeam.Equals(_teamA) ? _teamB : _teamA;

	public List<OnlineZeeplevel> AllLevels { get; }
	public List<OnlineZeeplevel> AvailableLevels { get; }
	public List<OnlineZeeplevel> UnAvailableLevels { get; }
	public List<DraftAction> PickedLevels { get; } = new();
	public List<DraftAction> BannedLevels { get; } = new();

	public bool IsPickPhase { get; set; }

	public void PickLevel(OnlineZeeplevel level)
	{
		if (CurrentTeam.Picks <= 0) throw new InvalidOperationException("You have no more picks left.");

		if (!AvailableLevels.Any(l => l.UID.Equals(level.UID)) || UnAvailableLevels.Any(l => l.UID.Equals(level.UID)))
			throw new InvalidOperationException("Level is not available anymore");

		IsPickPhase = true;
		PickedLevels.Add(new DraftAction(level, CurrentTeam, true));
		AvailableLevels.Remove(level);
		CurrentTeam.Picks -= 1;
		SwitchTeam();
	}

	public void BanLevel(OnlineZeeplevel level)
	{
		if (IsPickPhase && !OtherTeam.MissedDraft)
			throw new InvalidOperationException(
				$"{OtherTeam.GetColoredTag()} has picked a level and therefore you can't ban a level anymore.");

		GetOtherTeam().MissedDraft = false;
		if (CurrentTeam.Bans <= 0) throw new InvalidOperationException("You have no more bans left.");

		if (!AvailableLevels.Any(l => l.UID.Equals(level.UID)) || UnAvailableLevels.Any(l => l.UID.Equals(level.UID)))
			throw new InvalidOperationException("Level is not available anymore");

		BannedLevels.Add(new DraftAction(level, CurrentTeam, false));
		AvailableLevels.Remove(level);
		CurrentTeam.Bans -= 1;
		SwitchTeam();
	}

	public bool IsDraftComplete()
	{
		var noMoreInventoryLeft = _teamA.Bans + _teamB.Bans == 0 && _teamA.Picks + _teamB.Picks == 0;
		var noPicksLeft = _teamA.Picks + _teamB.Picks == 0 && IsPickPhase;

		return noPicksLeft || noMoreInventoryLeft;
	}

	public void SwitchTeam()
	{
		CurrentTeam = OtherTeam; // Switch turn
	}

	public Team GetCurrentTeam()
	{
		return CurrentTeam;
	}

	public Team GetOtherTeam()
	{
		return OtherTeam;
	}
}