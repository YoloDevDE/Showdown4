using System;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.Multiplayer;
using ZeepSDK.Playlist;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class State_Showdown_Starting : IState
{
    public State_Showdown_Starting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        RacingApi.LevelLoaded += OnLevelLoaded;
    }

    public void Execute()
    {
        // Use the showdown playlist name from the new config entry
        string showdownPlaylistName = Plugin.ShowdownPlaylistName.Value;

        // Fetch the playlist using the configurable playlist name
        PlaylistSaveJSON playlist = PlaylistApi.GetPlaylist(showdownPlaylistName);
        ZeepkistNetwork.CurrentLobby.Playlist = playlist.levels;
        MultiplayerApi.UpdateServerPlaylist();

        // Send the force start command with the appropriate playlist size
        ChatApi.SendMessage($"/fs {playlist.levels.Count - 1}");
    }

    public void Exit()
    {
        RacingApi.LevelLoaded -= OnLevelLoaded;
    }

    private void OnLevelLoaded()
    {
        Finished?.Invoke();
    }
}