using System;
using Showdown4.Entities;
using Showdown4.Managers;
using ZeepSDK.Level;

namespace Showdown4.States.Showdown;

public class StateShowdownStarting : ShowdownStateBase
{
	public StateShowdownStarting(IStateMachine stateMachine) : base(stateMachine)
	{
	}

	public override void Enter()
	{
	}

	public override void Execute()
	{
		string tmp = "";
		try
		{
			PlaylistManager.SetServerPlaylist(MyConfig.IntermissionLevelPlaylistNameConfig.Value);
			if (!LevelApi.CurrentLevel.UID.Equals(
				    PlaylistManager.GetLocalLevelsByPlaylistName(MyConfig.IntermissionLevelPlaylistNameConfig.Value)[0]
					    .UID))
			{
				tmp = "Skipping to HoF...";
				Managers.LobbyManager.SkipToLevel(0);
			}
			else
			{
				tmp = "Already on HoF :smile:";
			}

			ChatMessage.SendCustomMessage(new ChatMessage.Builder()
				.ClearChat()
				.NewLine()
				.DashedLine()
				.NewLine()
				.TextLine("Showdown Season 6 started")
				.NewLine()
				.DashedLine()
				.NewLine()
				.TextLine(tmp)
				.Build().Message);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
		finally
		{
			InvokeFinish();
		}
	}

	public override void Exit()
	{
	}
}