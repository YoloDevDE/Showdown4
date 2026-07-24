using System.Collections;
using System.Collections.Generic;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistNetworking;
using Random = System.Random;

namespace Showdown4.States.Showdown;

public class StateDraftIncomplete(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private readonly Random _random = new();

	private bool _isSelectionDone;

	private List<OnlineZeeplevel> AvailableMaps => CurrentDraft.AvailableLevels;

	public override void Enter()
	{
		_isSelectionDone = false;

		ChatMessage.SendCustomMessage(
			$"Draft incomplete - <{ShowdownColors.Red}>Showdown</color> will now pick a map at random!");

		// The random selection used to be triggered manually via '/sd random'. It now runs
		// automatically whenever the draft ends incomplete with more than one map still open.
		CoroutineManager.Instance.StartExternalCoroutine(RandomSelectionAnimation());
	}

	public override void Exit()
	{
	}

	public override IState GetNextState()
	{
		return new StateDraftCompleted(StateMachine);
	}

	private IEnumerator RandomSelectionAnimation()
	{
		float delay = 0.10f;
		int selectedIndex = 0;

		int spinLoops = MyConfig.RandomSelectionSpinLoopsConfig.Value;
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

		if (_isSelectionDone)
		{
			yield break;
		}

		_isSelectionDone = true;
		InvokeFinish();
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
}