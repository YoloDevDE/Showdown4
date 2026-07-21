using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;
using ZeepkistNetworking;
using ZeepSDK.Chat;
using Random = System.Random;

namespace Showdown4.States.Showdown;

public class StateDrafting : ShowdownStateBase
{
	private const int DraftTime = 90;
	private const int Countdown = 3;

	private const int MaxLevelIndex = 7;

	private readonly Random _random = new();

	private int _draftCountdownTime = DraftTime;
	private bool _isDraftCompleteCountdownStarted;
	private bool _isInitiationPhaseComplete;

	public StateDrafting(IStateMachine stateMachine) : base(stateMachine)
	{
	}

	private Team CurrentTeam => CurrentDraft.GetCurrentTeam();
	private Team OtherTeam => CurrentDraft.GetOtherTeam();
	private Team InitiativeTeam => Match.Initiative;

	private List<OnlineZeeplevel> AvailableMaps => CurrentDraft.AvailableLevels;

	public override void Enter()
	{
		_isDraftCompleteCountdownStarted = false;
		_draftCountdownTime = DraftTime;
		_isInitiationPhaseComplete = false;

		Match.AddDraft();
		ChatApi.SendMessage("/settime 86400");

		CoroutineManager.Instance.StartExternalCoroutine(DelayDraftInitiation());
	}

	public override void Execute()
	{
		if (!_isInitiationPhaseComplete)
		{
			return;
		}

		ServerMessage draftMessage = new ServerMessage().ShowdownHeader(true)
			.AddLine(l => l.Size(30).Bold().AddBlock(Match.ScoreColored()).Indent("585%"));

		if (CurrentDraft.IsDraftComplete())
		{
			if (!_isDraftCompleteCountdownStarted && HandleDraftComplete())
			{
				// Draft is incomplete - the state machine moves on to StateDraftIncomplete,
				// so this state must not render or send anything anymore.
				return;
			}

			draftMessage.AddMessage(DraftCompleteMessage());
		}
		else
		{
			draftMessage.AddMessage(DraftingStateMessage());
		}

		draftMessage
			.AddSeparator()
			.AddMessage(GetDraftLevelList())
			.AddSeparator()
			.AddLine(line =>
				line.Italic().AddBlock("To pick use")
					.AddBlock("'!pick 1-7'", block => block.Color(ShowdownColors.Yellow)))
			.AddLine(line =>
				line.Italic().AddBlock("To ban use")
					.AddBlock("'!ban 1-7'", block => block.Color(ShowdownColors.Yellow)))
			.Send();

		if (_draftCountdownTime >= DraftTime && !CurrentDraft.IsDraftComplete())
		{
			SendTurnAnnouncements();
		}
	}

	public override void Exit()
	{
		CommandBan.CommandInvoked -= OnBan;
		CommandPick.CommandInvoked -= OnPick;
	}

	// Returns true when the draft is incomplete and the state machine should move on
	// to StateDraftIncomplete instead of completing the draft here.
	private bool HandleDraftComplete()
	{
		if (CurrentDraft.PickedLevels.Count == 0)
		{
			// In Draftphase II the remaining map is auto-picked when only one level is left,
			// instead of handing over to the incomplete-draft state.
			if (Match.IsDraftphaseTwo && AvailableMaps.Count == 1)
			{
				AutoPickLastRemainingLevel();
			}
			else
			{
				_isDraftCompleteCountdownStarted = true;
				InvokeFinish();
				return true;
			}
		}

		List<OnlineZeeplevel> matchPlaylist =
			new(CurrentDraft.PickedLevels.Select(draftAction => draftAction.Level));
		matchPlaylist.Add(PlaylistManager
			.GetLocalLevelsByPlaylistName(MyConfig.IntermissionLevelPlaylistNameConfig.Value)
			.First());
		PlaylistManager.SetServerPlaylist(matchPlaylist);

		_isDraftCompleteCountdownStarted = true;
		CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(Countdown,
			OnDraftCompleteTick,
			InvokeFinish));
		return false;
	}

	private void SendTurnAnnouncements()
	{
		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);

		string history = BuildDraftHistory();
		string actionDescription = BuildActionDescription();

		// First show draft history if any exists
		if (history.Length > 0)
		{
			ChatMessage.SendCustomMessage($"Draft History:<br>{history}");
		}

		Team current = CurrentTeam;
		Team other = OtherTeam;

		// Then show current turn message to the drafting team
		ChatMessage.SendCustomMessage(
			$"@{MentionRacer(current, 0)} + @{MentionRacer(current, 1)}" +
			$"<br>Your turn! {actionDescription}" +
			$"<br><color={ShowdownColors.Gray}><i><color={ShowdownColors.Yellow}>Warning</color>: Your selection will be FINAL and cannot be undone!</i></color>",
			current.Racers[0].SteamId,
			current.Racers[1].SteamId);

		// Send message to team that needs to wait
		ChatMessage.SendCustomMessage(
			$"@{MentionRacer(other, 0)} + @{MentionRacer(other, 1)}" +
			$"<br><b>Please wait</b> - It's the other team's ({current.GetColoredTag()}) turn to make their selection.",
			other.Racers[0].SteamId,
			other.Racers[1].SteamId);
	}

	private static string MentionRacer(Team team, int index)
	{
		return $"<color={team.Color}><b>{team.Racers[index].SteamName}</b></color>";
	}

	private string BuildDraftHistory()
	{
		StringBuilder historyMessage = new();
		foreach (DraftAction draftAction in CurrentDraft.BannedLevels.Concat(CurrentDraft.PickedLevels))
		{
			string actionType = draftAction.IsPick ? "picked" : "banned";
			string actionColor = draftAction.IsPick ? ShowdownColors.Green : ShowdownColors.Red;
			historyMessage.AppendLine($"<color={draftAction.Team.Color}>{draftAction.Team.GetTag()}</color> " +
			                          $"<color={actionColor}>{actionType}</color> " +
			                          $"<color={ShowdownColors.Cyan}>{draftAction.Level.Name}</color>");
		}

		return historyMessage.ToString();
	}

	private string BuildActionDescription()
	{
		Team current = CurrentTeam;

		if (current.Picks > 0)
		{
			if (OtherTeam.Picks == 0)
			{
				return $"It's time to <color={ShowdownColors.Green}><b>pick</b></color> a map!" +
				       $"<br>Use <color={ShowdownColors.Yellow}><b>!pick 1-7</b></color> to pick a map.";
			}

			return
				$"You can <color={ShowdownColors.Green}><b>pick</b></color> or <color={ShowdownColors.Red}><b>ban</b></color> a map!" +
				$"<br>Use <color={ShowdownColors.Yellow}><b>!pick 1-7</b></color> to pick or <color={ShowdownColors.Yellow}><b>!ban 1-7</b></color> to ban a map.";
		}

		if (current.Bans > 0)
		{
			return $"You can only <color={ShowdownColors.Red}><b>ban</b></color> a map!" +
			       $"<br>Use <color={ShowdownColors.Yellow}><b>!ban 1-7</b></color> to ban a map.";
		}

		return "";
	}

	private void OnDraftTick(int remainingSeconds)
	{
		_draftCountdownTime = remainingSeconds;
		Execute();
	}

	private void OnDraftTimeout()
	{
		_draftCountdownTime = DraftTime;
		CurrentTeam.MissedDraft = true;

		if (OtherTeam.Bans == 0 && OtherTeam.Picks == 0)
		{
			Team currentTeam = CurrentTeam;
			OnlineZeeplevel randomLevel = AvailableMaps[_random.Next(AvailableMaps.Count)];

			if (currentTeam.Picks > 0)
			{
				CurrentDraft.PickLevel(randomLevel);
				ChatMessage.SendCustomMessage(
					$"{currentTeam.GetColoredTag()} has randomly <color={ShowdownColors.Green}>picked</color> <color={ShowdownColors.Cyan}>{randomLevel.Name}</color>");
			}
			else if (currentTeam.Bans > 0)
			{
				CurrentDraft.BanLevel(randomLevel);
				ChatMessage.SendCustomMessage(
					$"{currentTeam.GetColoredTag()} has randomly <color={ShowdownColors.Red}>banned</color> <color={ShowdownColors.Cyan}>{randomLevel.Name}</color>");
			}

			Execute();
			return;
		}

		CurrentDraft.SwitchTeam();
		CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(DraftTime, OnDraftTick, OnDraftTimeout));
	}

	private void OnDraftCompleteTick(int remainingSeconds)
	{
		Execute();
	}

	private IEnumerator DelayDraftInitiation()
	{
		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);
		ChatMessage.SendCustomMessage(
			$"<color={ShowdownColors.Orange}><b>*** <color={ShowdownColors.Gold}>{Match.DraftphaseName}</color> loading - Please pay attention to the upcoming messages! ***</b></color>");
		ChatApi.SendMessage("/servermessage remove");
		yield return new WaitForSeconds(1.5f);

		ServerMessage initiationMessage = new ServerMessage("center").ShowdownHeader(false, "center", 50);
		initiationMessage.Send();
		yield return new WaitForSeconds(1.5f);

		initiationMessage.AddLine(l =>
			l.Size(30).Bold().AddBlock(Match.ScoreWithFullNameColoredAndPadded()).NoBreak());
		initiationMessage.Send();
		yield return new WaitForSeconds(1.5f);

		initiationMessage.AddLine(line =>
			line.AddBlock(Match.DraftphaseName,
				builder => builder.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold).Bold()
					.AllCaps().Size(40)));
		initiationMessage.Send();
		yield return new WaitForSeconds(1.5f);

		initiationMessage.AddLine(line => line
			.AddBlock($"{InitiativeTeam.GetTag()}", b => b.Color(InitiativeTeam.Color))
			.AddBlock("has initiative!"));
		initiationMessage.Send();
		yield return new WaitForSeconds(1.5f);

		CommandBan.CommandInvoked += OnBan;
		CommandPick.CommandInvoked += OnPick;

		_isInitiationPhaseComplete = true;

		CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(DraftTime, OnDraftTick, OnDraftTimeout));
	}

	private ServerMessage DraftingStateMessage()
	{
		Team current = CurrentTeam;
		bool isTimeRunningLow = _draftCountdownTime <= 10;

		ServerMessage tmp = new();

		tmp.AddLine(line =>
			line.AddBlock(Match.DraftphaseName,
				builder => builder.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold).Bold()
					.AllCaps().Size(40))
		).AddSeparator(0).AddLine(line =>
		{
			line.AddBlock($"{current.GetTag()}", block => block.Color(current.Color))
				.AddBlock("is drafting:")
				.AddBlock($"{TimeFormatter.FormatDuration(_draftCountdownTime)}",
					block => block.Color(isTimeRunningLow ? ShowdownColors.Red : ShowdownColors.Yellow));
			if (isTimeRunningLow)
			{
				line.AddBlock("Time is running low! Make your choice!",
					block => block.Color(ShowdownColors.Yellow).Bold());
			}
		});

		tmp.AddLine(line => line
			.AddBlock("Picks left:")
			.AddBlock($"{current.Picks}", block => block.Color(ShowdownColors.Green))
			.AddBlock("| Bans left:")
			.AddBlock($"{current.Bans}", block => block.Color(ShowdownColors.Red)));

		return tmp;
	}

	private ServerMessage DraftCompleteMessage()
	{
		ServerMessage tmp = new();

		tmp.AddLine(line => line
			.AddBlock(Match.DraftphaseName,
				builder => builder.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold))
			.AddBlock("-")
			.AddBlock("complete!", builder => builder.Color(ShowdownColors.Green))
			.Bold().AllCaps().Size(40));

		return tmp;
	}

	private void AutoPickLastRemainingLevel()
	{
		OnlineZeeplevel lastLevel = AvailableMaps[0];
		CurrentDraft.PickedLevels.Add(new DraftAction(lastLevel, Team.CreateShowdownTeam(), true));
		ChatMessage.SendCustomMessage(
			$"Only one map left - automatically <color={ShowdownColors.Green}>picked</color> <color={ShowdownColors.Cyan}>{lastLevel.Name}</color>");
	}

	private ServerMessage GetDraftLevelList()
	{
		Draft draft = CurrentDraft;
		ServerMessage tmp = new();

		foreach (OnlineZeeplevel level in draft.AllLevels)
			tmp.AddLine(line =>
			{
				line.AddBlock(level.Name, block =>
				{
					if (draft.AvailableLevels.All(l => l.Name != level.Name) ||
					    draft.UnAvailableLevels.Any(l => l.Name == level.Name))
					{
						block.Strikethrough();
					}

					if (draft.UnAvailableLevels.Any(l => l.Name == level.Name))
					{
						block.Color(ShowdownColors.Yellow);
					}

					if (draft.BannedLevels.Any(l => l.Level.Name == level.Name))
					{
						block.Color(ShowdownColors.Red);
					}

					if (draft.PickedLevels.Any(l => l.Level.Name == level.Name))
					{
						block.Color(ShowdownColors.Green);
					}
				});

				DraftAction bannedLevel = draft.BannedLevels.FirstOrDefault(l => l.Level.Name == level.Name);
				DraftAction pickedLevel = draft.PickedLevels.FirstOrDefault(l => l.Level.Name == level.Name);

				if (bannedLevel != null)
				{
					line.AddBlock("banned", block => block.Color(ShowdownColors.Red).Indent("500%"))
						.AddBlock("by")
						.AddBlock($"{bannedLevel.Team.GetTag()}", block => block.Color(bannedLevel.Team.Color));
				}
				else if (pickedLevel != null)
				{
					line.AddBlock("picked", block => block.Color(ShowdownColors.Green).Indent("500%"))
						.AddBlock("by")
						.AddBlock($"{pickedLevel.Team.GetTag()}", block => block.Color(pickedLevel.Team.Color));
				}
			});

		return tmp;
	}

	private void OnPick(ulong steamId, string levelIndexStr)
	{
		HandleDraft(false, steamId, levelIndexStr);
	}

	private void OnBan(ulong steamId, string levelIndexStr)
	{
		HandleDraft(true, steamId, levelIndexStr);
	}

	private void HandleDraft(bool isBan, ulong steamId, string levelIndexStr)
	{
		if (!ZeepkistNetwork.TryGetPlayer(steamId, out ZeepkistNetworkPlayer player))
		{
			ChatMessage.SendCustomMessage("Error: Could not find the player for the provided Steam ID.");
			return;
		}

		Draft currentDraft = CurrentDraft;
		Team currentTeam = currentDraft.GetCurrentTeam();

		if (!player.IsLocal && currentTeam.Racers.All(racer => racer.SteamId != steamId))
		{
			ChatMessage.SendCustomMessage(
				$"{player.Username} is not a member of the current drafting team ({currentTeam.GetColoredTag()}).");
			return;
		}

		if (!int.TryParse(levelIndexStr, out int levelIndex) || levelIndex < 1 || levelIndex > MaxLevelIndex ||
		    levelIndex > currentDraft.AllLevels.Count)
		{
			ChatMessage.SendCustomMessage(
				$"Invalid level index: {levelIndexStr}. Must be between 1 and {MaxLevelIndex}.");
			return;
		}

		OnlineZeeplevel levelToPickOrBan = currentDraft.AllLevels[levelIndex - 1];

		try
		{
			if (isBan)
			{
				HandleBan(currentDraft, currentTeam, levelToPickOrBan);
			}
			else
			{
				HandlePick(currentDraft, levelToPickOrBan, currentTeam, player);
			}

			CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(DraftTime, OnDraftTick,
				OnDraftTimeout));
			Execute();
		}
		catch (InvalidOperationException ex)
		{
			ChatMessage.SendCustomMessage(ex.Message);
		}
	}

	private void HandleBan(Draft currentDraft, Team currentTeam, OnlineZeeplevel levelToPickOrBan)
	{
		currentDraft.BanLevel(levelToPickOrBan);
		ChatMessage.SendCustomMessage(
			$"{currentTeam.GetColoredTag()} has <color={ShowdownColors.Red}>banned</color> <color={ShowdownColors.Cyan}>{levelToPickOrBan.Name}</color>");
	}

	private void HandlePick(Draft currentDraft, OnlineZeeplevel level, Team currentTeam, ZeepkistNetworkPlayer player)
	{
		if (player.IsLocal && currentDraft.IsDraftComplete())
		{
			Team showdownTeam = Team.CreateShowdownTeam();
			currentDraft.PickedLevels.Add(new DraftAction(level, showdownTeam, true));
			ChatMessage.SendCustomMessage(
				$"{showdownTeam.GetColoredTag()} has <color={ShowdownColors.Green}>picked</color> <color={ShowdownColors.Cyan}>{level.Name}</color>");
		}
		else
		{
			currentDraft.PickLevel(level);
			ChatMessage.SendCustomMessage(
				$"{currentTeam.GetColoredTag()} has <color={ShowdownColors.Green}>picked</color> <color={ShowdownColors.Cyan}>{level.Name}</color>");
		}
	}
}