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
}