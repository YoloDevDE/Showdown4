using System.Collections.Generic;
using System.Linq;
using Showdown4.Config;
using Showdown4.Managers;
using Showdown4.Utils;
using ZeepkistNetworking;

namespace Showdown4.States.Showdown;

/// <summary>
///     Runs once the draft is finished (either normally, via auto-pick of the last map, or after
///     the random selection of the incomplete flow). It locks in the resulting playlist, shows the
///     "draft complete" banner and, after a short countdown, hands over to the ready check.
/// </summary>
public class StateDraftCompleted(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	public override void Enter()
	{
		List<OnlineZeeplevel> matchPlaylist =
			new(CurrentDraft.PickedLevels.Select(draftAction => draftAction.Level));
		matchPlaylist.Add(PlaylistManager
			.GetLocalLevelsByPlaylistName(MyConfig.IntermissionLevelPlaylistNameConfig.Value)
			.First());
		PlaylistManager.SetServerPlaylist(matchPlaylist);

		Render();

		CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(
			MyConfig.DraftCompleteCountdownConfig.Value,
			_ => Render(),
			InvokeFinish));
	}

	public override void Exit()
	{
	}

	private void Render()
	{
		new ServerMessage().ShowdownHeader(true)
			.AddLine(l => l.Size(30).Bold().AddBlock(Match.ScoreColored()).Indent("585%"))
			.AddMessage(DraftDisplay.CompleteHeader(Match))
			.AddSeparator()
			.AddMessage(DraftDisplay.LevelList(CurrentDraft))
			.Send();
	}
}