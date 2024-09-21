namespace Showdown4.Entities;

public class LevelDraft
{
    public Team Team { get; set; }
    public Level Level { get; set; }
    public LevelDraftType LevelDraftType { get; set; }
    public int DraftRound { get; set; }
}