using System;
using Showdown4.Tmp;

namespace Showdown4.Service;

public static class DraftingService
{
    public static Action<Team, bool, int> OnDrafted;

    public static void Draft(Team team, bool isBan, int LevelIndex)
    {
    }
}