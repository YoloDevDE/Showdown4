using System;
using Showdown4.Commands;
using Showdown4.Entities;
using UnityEngine;
using ZeepkistClient;
using ZeepSDK.Multiplayer;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class ShowdownStateMachine : MonoBehaviour, IStateMachine
{
	public Match Match { get; set; }

	// The state machine itself is a MonoBehaviour, so Unity drives the per-frame
	// loop here and forwards keyboard input to the active state.
	private void Update()
	{
		CurrentState?.HandleInput();
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