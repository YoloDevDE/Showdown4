using BepInEx.Configuration;

namespace Showdown4.Config;

// The BepInEx config file is user-editable, so raw values could be negative,
// zero or absurdly large. Instead of clamping every value by hand, each numeric
// entry is bound with an AcceptableValueRange so BepInEx itself keeps the value
// inside safe bounds. All settings are exposed through their generated
// "XxxConfig" entries and consumed via "MyConfig.XxxConfig.Value".
public static class MyConfig
{
	public static ConfigEntry<string> CompetitionLevelsPlaylistNameConfig;
	public static ConfigEntry<string> IntermissionLevelPlaylistNameConfig;
	public static ConfigEntry<string> TeamFileConfig;

	public static ConfigEntry<int> SeasonNumberConfig;
	public static ConfigEntry<int> TutorialStepDurationConfig;
	public static ConfigEntry<int> DraftTimeConfig;
	public static ConfigEntry<int> DraftCompleteCountdownConfig;
	public static ConfigEntry<int> RandomSelectionSpinLoopsConfig;
	public static ConfigEntry<int> PreDraftInstructionDurationConfig;
	public static ConfigEntry<int> InitiativeAnnouncementDurationConfig;
	public static ConfigEntry<int> AutoPickRevealCountdownConfig;
	public static ConfigEntry<int> ReadyCheckDurationConfig;
	public static ConfigEntry<int> ReadyConfirmCountdownConfig;
	public static ConfigEntry<int> PreRaceCountdownConfig;
	public static ConfigEntry<int> LobbyTimeConfig;
	public static ConfigEntry<int> MatchEndKickCountdownConfig;

	public static ConfigEntry<float> LeaderboardOverrideResetDelayConfig;
	public static ConfigEntry<string> LeaderboardGainedArrowConfig;
	public static ConfigEntry<string> LeaderboardGainedColorConfig;
	public static ConfigEntry<string> LeaderboardLostArrowConfig;
	public static ConfigEntry<string> LeaderboardLostColorConfig;
	public static ConfigEntry<string> LeaderboardEqualSymbolConfig;
	public static ConfigEntry<string> LeaderboardEqualColorConfig;

	public static void Register(ConfigFile configFile)
	{
		CompetitionLevelsPlaylistNameConfig = configFile.Bind("General", "Competition Levels Playlistname",
			"S4_Pool", "Name of the playlist to use for the level pool");

		IntermissionLevelPlaylistNameConfig = configFile.Bind("General", "Intermissionlevel Playlistname",
			"S4_Live", "");

		TeamFileConfig = configFile.Bind("General", "Teams Json", "Teams", "");

		// Roman numerals only cover 1-3999; keep the season inside that range.
		SeasonNumberConfig = configFile.Bind("General", "Showdown Season Number", 6,
			new ConfigDescription(
				"The season number shown in the header and announcements (Roman numeral range).",
				new AcceptableValueRange<int>(1, 3999)));

		// A draft turn needs at least one second to be usable.
		DraftTimeConfig = configFile.Bind("Draft", "Draft Time", 90,
			new ConfigDescription(
				"Seconds each team has to make a single pick/ban before their turn times out.",
				new AcceptableValueRange<int>(1, 3600)));

		// A zero countdown is fine (instant), negatives are not.
		DraftCompleteCountdownConfig = configFile.Bind("Draft", "Draft Complete Countdown", 3,
			new ConfigDescription(
				"Seconds to wait after the draft is complete before moving on to the ready check.",
				new AcceptableValueRange<int>(0, 3600)));

		// The animation needs at least one loop to show anything.
		RandomSelectionSpinLoopsConfig = configFile.Bind("Draft", "Random Selection Spin Loops", 30,
			new ConfigDescription(
				"Number of animation steps the random map roulette spins through when the draft is incomplete (purely visual).",
				new AcceptableValueRange<int>(1, 1000)));

		// How long the pre-draft instructions stay up; zero is fine (instant).
		PreDraftInstructionDurationConfig = configFile.Bind("Draft", "Pre Draft Instruction Duration", 6,
			new ConfigDescription(
				"Seconds the pick/ban/pass instructions are shown before the draft actually starts.",
				new AcceptableValueRange<int>(0, 3600)));

		// How long the "TeamX has initiative!" announcement is prominently displayed before
		// moving on; zero is fine (instant).
		InitiativeAnnouncementDurationConfig = configFile.Bind("Draft", "Initiative Announcement Duration", 5,
			new ConfigDescription(
				"Seconds the 'has initiative' announcement is clearly displayed before the draft continues.",
				new AcceptableValueRange<int>(0, 3600)));

		// Visual delay before the auto-picked last map is locked in; zero is fine (instant).
		AutoPickRevealCountdownConfig = configFile.Bind("Draft", "Auto Pick Reveal Countdown", 3,
			new ConfigDescription(
				"Seconds of visual feedback shown when Showdown automatically picks the last remaining map.",
				new AcceptableValueRange<int>(0, 3600)));

		ReadyCheckDurationConfig = configFile.Bind("Ready Check", "Ready Check Duration", 300,
			new ConfigDescription(
				"Seconds the ready check waits for all players before timing out.",
				new AcceptableValueRange<int>(1, 3600)));

		ReadyConfirmCountdownConfig = configFile.Bind("Ready Check", "Ready Confirm Countdown", 3,
			new ConfigDescription(
				"Seconds to wait after everyone is ready before racing starts.",
				new AcceptableValueRange<int>(0, 3600)));

		PreRaceCountdownConfig = configFile.Bind("Racing", "Pre Race Countdown", 10,
			new ConfigDescription(
				"Countdown in seconds shown before a race starts.",
				new AcceptableValueRange<int>(0, 3600)));

		LobbyTimeConfig = configFile.Bind("Racing", "Lobby Time", 300,
			new ConfigDescription(
				"Lobby time in seconds set while racing (used with /settime).",
				new AcceptableValueRange<int>(1, 86400)));

		MatchEndKickCountdownConfig = configFile.Bind("Match End", "Kick Countdown", 60,
			new ConfigDescription(
				"Seconds after the match ends before the teams get kicked.",
				new AcceptableValueRange<int>(0, 3600)));

		LeaderboardOverrideResetDelayConfig = configFile.Bind("Leaderboard Overrides", "Reset Delay", 3.5f,
			new ConfigDescription(
				"Seconds a gained/lost/equal/new position override stays visible before it reverts back to the normal time display.",
				new AcceptableValueRange<float>(0f, 60f)));

		LeaderboardGainedArrowConfig = configFile.Bind("Leaderboard Overrides", "Gained Position Arrow", "+",
			"Symbol shown in front of the position override when a racer gains positions.");

		LeaderboardGainedColorConfig = configFile.Bind("Leaderboard Overrides", "Gained Position Color", "#00ff44",
			"Color (hex) used for the gained-position arrow and the improvement time override.");

		LeaderboardLostArrowConfig = configFile.Bind("Leaderboard Overrides", "Lost Position Arrow", "-",
			"Symbol shown in front of the position override when a racer loses positions.");

		LeaderboardLostColorConfig = configFile.Bind("Leaderboard Overrides", "Lost Position Color", "#ff2200",
			"Color (hex) used for the lost-position arrow.");

		LeaderboardEqualSymbolConfig = configFile.Bind("Leaderboard Overrides", "Equal Position Symbol", "\u00b1",
			"Symbol shown when a racer improves their time without changing position.");

		LeaderboardEqualColorConfig = configFile.Bind("Leaderboard Overrides", "Equal Position Color", "yellow",
			"Color used for the equal-position symbol.");
	}
}