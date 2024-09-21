namespace Showdown4.Entities;

public class Racer
{
    public Racer(ulong steamId, string steamName)
    {
        SteamId = steamId;
        SteamName = steamName;
    }

    public ulong SteamId { get; set; }
    public string SteamName { get; set; }
}