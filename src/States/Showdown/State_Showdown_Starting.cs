using System;
using Showdown4.Entities;
using Showdown4.Managers;
using ZeepSDK.Level;

namespace Showdown4.States.Showdown;

public class StateShowdownStarting : IState
{
	public StateShowdownStarting(IStateMachine stateMachine)
	{
		StateMachine = stateMachine;
	}

	public IStateMachine StateMachine { get; }
	public event Action Finished;

	public void Enter()
	{
	}

	public void Execute()
	{
		var tmp = "";
		try
		{
			PlaylistManager.SetServerPlaylist(Plugin.IntermissionLevelPlaylistName.Value);
			if (!LevelApi.CurrentLevel.UID.Equals(
				    PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value)[0].UID))
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
			Finished?.Invoke();
		}
	}

	public void Exit()
	{
	}

	public void InvokeFinish()
	{
		Finished?.Invoke();
	}
}