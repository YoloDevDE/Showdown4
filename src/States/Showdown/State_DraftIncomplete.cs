using System.Collections;
using System.Collections.Generic;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistNetworking;
using Random = System.Random;

namespace Showdown4.States.Showdown;

/// <summary>
///     Runs when the draft ended without a single pick and more than one map is still open. Showdown
///     then spins through the remaining maps and picks one of them at random.
/// </summary>
public class StateDraftIncomplete(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	// The spin starts at this delay between two maps and slows down by the step below on every
	// iteration, so the selection visibly comes to a halt.
	private const float InitialSpinDelay = 0.10f;
	private const float SpinDelayStep = 0.05f;

	// Chance (1 in n) for the spin to reverse its direction, purely for the show.
	private const int ReverseDirectionChance = 10;

	private readonly Random _random = new();

	private List<OnlineZeeplevel> AvailableMaps => CurrentDraft.AvailableLevels;

	public override void Enter()
	{
		// Nothing left to choose from - only reachable when the configured draft inventory does not fit
		// the level pool. There is no map to raffle off, so hand over instead of spinning an empty wheel.
		if (AvailableMaps.Count == 0)
		{
			ChatMessage.SendCustomMessage(
				$"<{ShowdownColors.Red}>No levels left to select from</color> - check the level pool and the " +
				"configured picks/bans per team.");
			InvokeFinish();
			return;
		}

		ChatMessage.SendCustomMessage(
			$"Draft incomplete - <{ShowdownColors.Red}>Showdown</color> will now pick a map at random!");

		// The random selection used to be triggered manually via '/sd random'. It now runs
		// automatically whenever the draft ends incomplete with more than one map still open.
		Showdown.StartCoroutine(RandomSelectionAnimation());
	}

	public override IState GetNextState()
	{
		return new StateDraftCompleted(StateMachine);
	}

	private IEnumerator RandomSelectionAnimation()
	{
		float delay = InitialSpinDelay;
		int selectedIndex = 0;

		int spinLoops = MyConfig.RandomSelectionSpinLoopsConfig.Value;
		for (int i = 0; i < spinLoops; i++)
		{
			if (_random.Next(ReverseDirectionChance) == 0)
			{
				selectedIndex = (selectedIndex - 1 + AvailableMaps.Count) % AvailableMaps.Count;
			}
			else
			{
				selectedIndex = (selectedIndex + 1) % AvailableMaps.Count;
			}

			UpdateRandomSelectionMessage(selectedIndex);
			delay += SpinDelayStep;

			yield return new WaitForSeconds(delay);
		}

		SelectRandomMap(AvailableMaps[selectedIndex]);
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