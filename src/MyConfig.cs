using BepInEx.Configuration.Generators;

namespace Showdown4;

[GenerateConfig]
public static partial class MyConfig
{
	[Entry("General", "Competition Levels Playlistname", "Name of the playlist to use for the level pool")]
	private static readonly string CompetitionLevelsPlaylistName = "S4_Pool";

	[Entry("General", "Intermissionlevel Playlistname", "")]
	private static readonly string IntermissionLevelPlaylistName = "S4_Live";

	[Entry("General", "Teams Json", "")] private static readonly string TeamFile = "Teams";

	[Entry("General", "Showdown Season Number",
		"The season number shown in the header and announcements. Valid range: 1-3999 (Roman numeral range); other values are clamped.")]
	private static readonly int SeasonNumber = 6;

	[Entry("Draft", "Draft Time",
		"Seconds each team has to make a single pick/ban before their turn times out. Clamped to 1-3600.")]
	private static readonly int DraftTime = 90;

	[Entry("Draft", "Draft Complete Countdown",
		"Seconds to wait after the draft is complete before moving on to the ready check. Clamped to 0-3600.")]
	private static readonly int DraftCompleteCountdown = 3;

	[Entry("Draft", "Random Selection Spin Loops",
		"Number of animation steps the random map roulette spins through when the draft is incomplete (purely visual). Clamped to 1-1000.")]
	private static readonly int RandomSelectionSpinLoops = 30;

	[Entry("Draft", "Pre Draft Instruction Duration",
		"Seconds the pick/ban/pass instructions are shown before the draft actually starts. Clamped to 0-3600.")]
	private static readonly int PreDraftInstructionDuration = 6;

	[Entry("Draft", "Auto Pick Reveal Countdown",
		"Seconds of visual feedback shown when Showdown automatically picks the last remaining map. Clamped to 0-3600.")]
	private static readonly int AutoPickRevealCountdown = 3;

	[Entry("Ready Check", "Ready Check Duration",
		"Seconds the ready check waits for all players before timing out. Clamped to 1-3600.")]
	private static readonly int ReadyCheckDuration = 300;

	[Entry("Ready Check", "Ready Confirm Countdown",
		"Seconds to wait after everyone is ready before racing starts. Clamped to 0-3600.")]
	private static readonly int ReadyConfirmCountdown = 3;

	[Entry("Racing", "Pre Race Countdown", "Countdown in seconds shown before a race starts. Clamped to 0-3600.")]
	private static readonly int PreRaceCountdown = 10;

	[Entry("Racing", "Lobby Time", "Lobby time in seconds set while racing (used with /settime). Clamped to 1-86400.")]
	private static readonly int LobbyTime = 300;

	[Entry("Match End", "Kick Countdown",
		"Seconds after the match ends before the teams get kicked. Clamped to 0-3600.")]
	private static readonly int MatchEndKickCountdown = 60;
}