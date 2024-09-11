using System;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.Multiplayer;
using ZeepSDK.Playlist;

namespace Showdown4.Domain.States.Showdown;

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
    }

    public void Execute()
    {
        PlaylistSaveJSON playlist = PlaylistApi.GetPlaylist("Zeepkist Showdown - Season 3 - Live");
        ZeepkistNetwork.CurrentLobby.Playlist = playlist.levels;
        MultiplayerApi.UpdateServerPlaylist();
        ChatApi.SendMessage($"/fs {playlist.levels.Count - 1}");
        Finished?.Invoke();
    }

    public void Exit()
    {
    }
}