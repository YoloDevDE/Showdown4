namespace Showdown4.Entities;

public class Racer(ulong steamId, string steamName)
{
	public ulong SteamId { get; set; } = steamId;
	public string SteamName { get; set; } = steamName;
}