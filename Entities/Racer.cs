namespace Showdown4.Statemachine;

public class Racer
{
    public Racer(string steamName, ulong steamId)
    {
        SteamName = steamName;
        SteamId = steamId;
    }

    public string SteamName { get; }
    public ulong SteamId { get; }

    public double Result { get; set; }
}