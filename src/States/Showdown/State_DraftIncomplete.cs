using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistNetworking;
using Random = System.Random;

namespace Showdown4.States.Showdown;

public class StateDraftIncomplete : ShowdownStateBase
{
	private readonly Random _random = new();

	private bool _isDraftCompleteCountdownStarted;

	public StateDraftIncomplete(IStateMachine stateMachine) : base(stateMachine)
	{
	}

	private List<OnlineZeeplevel> AvailableMaps => CurrentDraft.AvailableLevels;

	public override void Enter()
	{
		_isDraftCompleteCountdownStarted = false;

		CommandStartRandom.CommandInvoked += OnStartRandom;

		ChatMessage.SendCustomMessage(
			"Draft incomplete! Waiting for Host to initiate Random Map Selection");
	}

	public override void Execute()
	{
		ServerMessage draftMessage = new ServerMessage().ShowdownHeader(true)
			.AddLine(l => l.Size(30).Bold().AddBlock(Match.ScoreColored()).Indent("585%"));

		draftMessage.AddLine(line => line
			.AddBlock(Match.DraftphaseName,
				builder => builder.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold))
			.AddBlock("-")
			.AddBlock("incomplete!", builder => builder.Color(ShowdownColors.Yellow))
			.Bold().AllCaps().Size(40));

		draftMessage.Send();
	}

	public override void Exit()
	{
		CommandStartRandom.CommandInvoked -= OnStartRandom;
	}

	private void OnStartRandom()
	{
		CoroutineManager.Instance.StartExternalCoroutine(RandomSelectionAnimation());
	}

	private IEnumerator RandomSelectionAnimation()
	{
		float delay = 0.10f;
		int selectedIndex = 0;

		int spinLoops = MyConfig.Validated.RandomSelectionSpinLoops;
		for (int i = 0; i < spinLoops; i++)
		{
			if (_random.Next(10) == 0) // 10% chance to reverse direction
			{
				selectedIndex = (selectedIndex - 1 + AvailableMaps.Count) % AvailableMaps.Count;
			}
			else
			{
				selectedIndex = (selectedIndex + 1) % AvailableMaps.Count;
			}

			UpdateRandomSelectionMessage(selectedIndex);
			delay += 0.05f;

			yield return new WaitForSeconds(delay);
		}

		SelectRandomMap(AvailableMaps[selectedIndex]);
		FinalizeDraft();
	}

	private void UpdateRandomSelectionMessage(int currentIndex)
	{
		ServerMessage msg = new ServerMessage()
			.ShowdownHeader()
			.AddLine(line => line.AddBlock("Random Map Selection..."))
			.AddSeparator();

		for (int i = 0; i < AvailableMaps.Count; i++)
		{
			OnlineZeeplevel level = AvailableMaps[i];

			if (i == currentIndex)
			{
				msg.AddLine(line =>
					line.AddBlock($"> {level.Name}", block => block.Bold().Color(ShowdownColors.Yellow)));
			}
			else
			{
				msg.AddLine(line => line.AddBlock(level.Name));
			}
		}

		msg.AddSeparator();
		msg.Send();
	}

	private void SelectRandomMap(OnlineZeeplevel selectedLevel)
	{
		ChatMessage.SendCustomMessage($"Randomly selected map: {selectedLevel.Name}");

		CurrentDraft.PickedLevels.Add(new DraftAction(selectedLevel, Team.CreateShowdownTeam(), true));
	}

	private void FinalizeDraft()
	{
		if (_isDraftCompleteCountdownStarted)
		{
			return;
		}

		List<OnlineZeeplevel> matchPlaylist =
			new(CurrentDraft.PickedLevels.Select(draftAction => draftAction.Level));
		matchPlaylist.Add(PlaylistManager
			.GetLocalLevelsByPlaylistName(MyConfig.IntermissionLevelPlaylistNameConfig.Value)
			.First());
		PlaylistManager.SetServerPlaylist(matchPlaylist);

		_isDraftCompleteCountdownStarted = true;
		CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(
			MyConfig.Validated.DraftCompleteCountdown,
			_ => Execute(),
			InvokeFinish));
	}
}