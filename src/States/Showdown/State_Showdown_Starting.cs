using System;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using ZeepSDK.Level;

namespace Showdown4.States.Showdown;

public class StateShowdownStarting(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	public override void Enter()
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
				ChatCommandService.SkipToLevel(0);
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
				.TextLine($"Showdown Season {MyConfig.SeasonNumberConfig.Value} started")
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