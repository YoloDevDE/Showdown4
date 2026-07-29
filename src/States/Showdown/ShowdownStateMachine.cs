using System;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;
using ZeepSDK.Multiplayer;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class ShowdownStateMachine : MonoBehaviour, IStateMachine
{
	// Small head start the racers get when the run is resumed, so nobody is surprised by a timer
	// that continues the very same frame.
	private const int ResumeGraceSeconds = 5;

	// While the run is paused with 'sd pause', we suspend the in-game lobby timer and remember
	// the round time that was still remaining (only when we are actually racing). 'sd resume'
	// then restores it (plus the grace period above). We will also want to persist this for the
	// full match log in the future.
	private int? _pausedRoundTime;

	public Match Match { get; set; }

	// The state machine itself is a MonoBehaviour, so Unity drives the per-frame loop here:
	// keyboard input, the active state's countdown, and the server-message auto-vanish timer.
	private void Update()
	{
		CurrentState?.HandleInput();
		(CurrentState as ShowdownStateBase)?.Tick();
		ServerMessage.AutoVanish.Tick();
	}

	// Always return new instances for InitialState and FinalState
	public IState InitialState => new StateShowdownStarting(this);
	public IState FinalState => new StateMatchEnd(this);
	public IState CurrentState { get; set; }

	public event Action StateMachineFinished;

	public void InvokeFinish()
	{
		StateMachineFinished?.Invoke();
	}

	public void Dispose()
	{
		StateMachineOperations.Dispose(this);
	}

	public void StateMachineFinishedNotify()
	{
		StateMachineOperations.StateMachineFinishedNotify(this);
	}

	public void Start()
	{
		StateMachineOperations.Start(this);
	}

	public void TransitionTo(IState nextState)
	{
		StateMachineOperations.TransitionTo(this, nextState);
	}

	public void TransitionToPreviousState()
	{
		StateMachineOperations.TransitionToPreviousState(this);
	}

	public void RestartCurrentState()
	{
		StateMachineOperations.RestartCurrentState(this);
	}

	public void OnCurrentStateFinished()
	{
		StateMachineOperations.OnCurrentStateFinished(this);
	}

	// Note: IStateMachine.TransitionTo() calls StopAllCoroutines() before every state change.
	// This class satisfies that with the plain MonoBehaviour.StopAllCoroutines(), so leftover
	// effect/intro coroutines from the previous state are cleaned up automatically. Countdowns
	// are unaffected - they are not coroutines (see Countdown/ShowdownStateBase.Tick()).

	public void SubscribeToEvents()
	{
		RacingApi.RoundStarted += HandleRoundStarted;
		RacingApi.RoundEnded += HandleRoundEnded;
		RacingApi.LevelLoaded += HandleLevelLoaded;
		ZeepkistNetwork.PlayerResultsChanged += HandlePlayerResultsChanged;
		MultiplayerApi.PlayerJoined += HandlePlayerJoined;

		CommandBan.CommandInvoked += HandleBan;
		CommandPick.CommandInvoked += HandlePick;
		CommandLinkRacer.CommandInvoked += HandleLinkRacer;
		CommandUnLinkRacer.CommandInvoked += HandleUnlinkRacer;
		CommandPass.CommandInvoked += HandlePass;
		CommandReady.CommandInvoked += HandleReady;
	}

	public void UnsubscribeFromEvents()
	{
		RacingApi.RoundStarted -= HandleRoundStarted;
		RacingApi.RoundEnded -= HandleRoundEnded;
		RacingApi.LevelLoaded -= HandleLevelLoaded;
		ZeepkistNetwork.PlayerResultsChanged -= HandlePlayerResultsChanged;
		MultiplayerApi.PlayerJoined -= HandlePlayerJoined;

		CommandBan.CommandInvoked -= HandleBan;
		CommandPick.CommandInvoked -= HandlePick;
		CommandLinkRacer.CommandInvoked -= HandleLinkRacer;
		CommandUnLinkRacer.CommandInvoked -= HandleUnlinkRacer;
		CommandPass.CommandInvoked -= HandlePass;
		CommandReady.CommandInvoked -= HandleReady;
	}

	// Pauses every timer: the active state's own countdown is frozen and the in-game lobby timer is
	// suspended. While racing we remember the remaining round time so Resume() can restore it.
	public void Pause()
	{
		(CurrentState as ShowdownStateBase)?.PauseCountdown();

		if (CurrentState is StateRacing)
		{
			_pausedRoundTime = GetRemainingRoundTimeSeconds();
		}

		ChatCommandService.SuspendTimer();
	}

	// Resumes every timer: the active state's countdown continues and, if a round time was saved
	// on Pause(), the lobby timer is restored to that value plus a short grace period.
	public void Resume()
	{
		(CurrentState as ShowdownStateBase)?.ResumeCountdown();

		if (_pausedRoundTime.HasValue)
		{
			ChatCommandService.SetTime(_pausedRoundTime.Value + ResumeGraceSeconds);
			_pausedRoundTime = null;
		}
	}

	// Remaining seconds of the current online round, or null when it cannot be determined.
	// RoundTime is the round's total duration; LevelLoadedAtTime marks when it started (both in
	// server-synced ZeepkistNetwork.Time units).
	private static int? GetRemainingRoundTimeSeconds()
	{
		ZeepkistLobby lobby = ZeepkistNetwork.CurrentLobby;
		if (lobby == null)
		{
			return null;
		}

		double remaining = lobby.RoundTime - (ZeepkistNetwork.Time - lobby.LevelLoadedAtTime);
		if (remaining <= 0)
		{
			return null;
		}

		return Mathf.CeilToInt((float)remaining);
	}

	private void HandleRoundStarted()
	{
		CurrentState?.OnRoundStarted();
	}

	private void HandleRoundEnded()
	{
		CurrentState?.OnRoundEnded();
	}

	private void HandleLevelLoaded()
	{
		CurrentState?.OnLevelLoaded();
	}

	private void HandlePlayerResultsChanged(ZeepkistNetworkPlayer player)
	{
		CurrentState?.OnPlayerResultsChanged(player);
	}

	private void HandlePlayerJoined(ZeepkistNetworkPlayer player)
	{
		CurrentState?.OnPlayerJoined(player);
		CastBroadcast.OnPlayerJoined(this); // late joiners have no chat history - resend the state
	}

	private void HandleBan(ulong steamId, string levelIndex)
	{
		CurrentState?.OnBan(steamId, levelIndex);
	}

	private void HandlePick(ulong steamId, string levelIndex)
	{
		CurrentState?.OnPick(steamId, levelIndex);
	}

	private void HandleLinkRacer(ulong steamId, string arguments)
	{
		CurrentState?.OnLinkRacer(steamId, arguments);
	}

	private void HandleUnlinkRacer(ulong steamId)
	{
		CurrentState?.OnUnlinkRacer(steamId);
	}

	private void HandlePass(ulong steamId)
	{
		CurrentState?.OnPass(steamId);
	}

	private void HandleReady(ulong steamId, string arguments)
	{
		CurrentState?.OnReady(steamId, arguments);
	}
}