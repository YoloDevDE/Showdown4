using System;

namespace Showdown4;

public static partial class MyConfig
{
	// The BepInEx config file is user-editable, so the raw values may be
	// negative, zero or absurdly large. All numeric settings are read through
	// this single, validated entry point which clamps them into safe bounds.
	// This keeps validation in one place and avoids scattering repeated
	// "MyConfig.XxxConfig.Value" lookups (and their potential bad values)
	// across the states.
	public static class Validated
	{
		// Roman numerals only cover 1-3999; keep the season inside that range.
		public static int SeasonNumber => Clamp(SeasonNumberConfig.Value, 1, 3999);

		// A draft turn needs at least one second to be usable.
		public static int DraftTime => Clamp(DraftTimeConfig.Value, 1, 3600);

		// A zero countdown is fine (instant), negatives are not.
		public static int DraftCompleteCountdown => Clamp(DraftCompleteCountdownConfig.Value, 0, 3600);

		// The animation needs at least one loop to show anything.
		public static int RandomSelectionSpinLoops => Clamp(RandomSelectionSpinLoopsConfig.Value, 1, 1000);

		public static int ReadyCheckDuration => Clamp(ReadyCheckDurationConfig.Value, 1, 3600);

		public static int ReadyConfirmCountdown => Clamp(ReadyConfirmCountdownConfig.Value, 0, 3600);

		public static int PreRaceCountdown => Clamp(PreRaceCountdownConfig.Value, 0, 3600);

		public static int LobbyTime => Clamp(LobbyTimeConfig.Value, 1, 86400);

		public static int MatchEndKickCountdown => Clamp(MatchEndKickCountdownConfig.Value, 0, 3600);

		private static int Clamp(int value, int min, int max)
		{
			return Math.Min(Math.Max(value, min), max);
		}
	}
}