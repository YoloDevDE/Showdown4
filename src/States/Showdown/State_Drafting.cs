using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Showdown4.Commands;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;
using ZeepkistNetworking;
using Random = System.Random;

namespace Showdown4.States.Showdown;

public class StateDrafting(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private const int MaxLevelIndex = 7;
	private readonly CommandBan _banCommand = new();
	private readonly CommandPass _passCommand = new();

	// The draft commands are only available while this state is active.
	private readonly CommandPick _pickCommand = new();

	private readonly Random _random = new();

	private int _draftCountdownTime = MyConfig.DraftTimeConfig.Value;

	private DraftingLeaderboard _draftingLeaderboard;
	private bool _isAutoPickInProgress;
	private bool _isDraftCompleteCountdownStarted;

	private Team CurrentTeam => CurrentDraft.GetCurrentTeam();
	private Team OtherTeam => CurrentDraft.GetOtherTeam();

	private List<OnlineZeeplevel> AvailableMaps => CurrentDraft.AvailableLevels;

	public override void Enter()
	{
		CommandRegistry.RegisterMixed(_pickCommand);
		CommandRegistry.RegisterMixed(_banCommand);
		CommandRegistry.RegisterMixed(_passCommand);

		_isDraftCompleteCountdownStarted = false;
		_isAutoPickInProgress = false;
		_draftCountdownTime = MyConfig.DraftTimeConfig.Value;

		Match.AddDraft();
		ChatCommandService.SetTime(86400);

		_draftingLeaderboard = new DraftingLeaderboard(Match.TeamA, Match.TeamB);
		_draftingLeaderboard.Activate(CurrentTeam, MyConfig.DraftTimeConfig.Value);

		Countdown.Start(MyConfig.DraftTimeConfig.Value, OnDraftTick, OnDraftTimeout);
	}

	public override void Exit()
	{
		CommandRegistry.Unregister(_pickCommand);
		CommandRegistry.Unregister(_banCommand);
		CommandRegistry.Unregister(_passCommand);

		_draftingLeaderboard?.Deactivate();
	}

	public override IState GetNextState()
	{
		return CurrentDraft.PickedLevels.Count == 0
			? new StateDraftIncomplete(StateMachine)
			: new StateDraftCompleted(StateMachine);
	}

	public override void OnPick(ulong steamId, string levelIndexStr)
	{
		if (IsDraftLocked())
		{
			return;
		}

		HandleDraft(false, steamId, levelIndexStr);
	}

	public override void OnBan(ulong steamId, string levelIndexStr)
	{
		if (IsDraftLocked())
		{
			return;
		}

		HandleDraft(true, steamId, levelIndexStr);
	}

	// Hands the current team's action over to the other team, mirroring a voluntary timeout.
	public override void OnPass(ulong steamId)
	{
		if (IsDraftLocked())
		{
			return;
		}

		if (!ZeepkistNetwork.TryGetPlayer(steamId, out ZeepkistNetworkPlayer player))
		{
			ChatMessage.SendCustomMessage("Error: Could not find the player for the provided Steam ID.");
			return;
		}

		Team currentTeam = CurrentDraft.GetCurrentTeam();
		if (!player.IsLocal && currentTeam.Racers.All(racer => racer.SteamId != steamId))
		{
			ChatMessage.SendCustomMessage(
				$"{player.Username} is not a member of the current drafting team ({currentTeam.GetColoredTag()}).");
			return;
		}

		Team otherTeam = CurrentDraft.GetOtherTeam();

		if (CurrentDraft.IsPickPhase && !otherTeam.MissedDraft)
		{
			ChatMessage.SendCustomMessage(
				$"{currentTeam.GetColoredTag()} cannot pass - a level has been picked, so you must pick a level too.");
			return;
		}

		if (otherTeam.Bans == 0 && otherTeam.Picks == 0)
		{
			// Passing only makes sense if the other team still has an action left to use it for -
			// otherwise there is nobody left to hand the turn over to.
			ChatMessage.SendCustomMessage(
				$"{currentTeam.GetColoredTag()} cannot pass - {otherTeam.GetColoredTag()} has no actions left.");
			return;
		}

		ChatMessage.SendCustomMessage(
			$"{currentTeam.GetColoredTag()} <{ShowdownColors.Yellow}>passed</color> their action to {otherTeam.GetColoredTag()}");

		// Record the pass so it shows up in the draft history.
		CurrentDraft.Pass();

		// A pass behaves exactly like letting the turn time out.
		OnDraftTimeout();
	}

	private void Render(string backgroundColor = "white")
	{
		if (_isAutoPickInProgress)
		{
			return;
		}

		if (CurrentDraft.IsDraftComplete())
		{
			if (!_isDraftCompleteCountdownStarted)
			{
				_isDraftCompleteCountdownStarted = true;
				HandleDraftComplete();
			}

			// The draft is finished - StateDraftCompleted / StateDraftIncomplete take over
			// the rendering from here, so this state must not send anything anymore.
			return;
		}

		new ServerMessage().ShowdownHeader(true)
			.BackgroundColor(backgroundColor)
			.AddLine(l => l.Size(30).Bold().AddBlock(Match.ScoreColored()).Indent("585%"))
			.AddMessage(DraftingStateMessage())
			.AddSeparator()
			.AddMessage(DraftDisplay.LevelList(CurrentDraft))
			.AddSeparator()
			.AddLine(line =>
				line.AddBlock("To pick use")
					.AddBlock("'!pick 1-7'", block => block.Command()))
			.AddLine(line =>
				line.AddBlock("To ban use")
					.AddBlock("'!ban 1-7'", block => block.Command()))
			.AddLine(line =>
				line.AddBlock("To pass use")
					.AddBlock("'!pass'", block => block.Command()))
			.Send();

		if (_draftCountdownTime >= MyConfig.DraftTimeConfig.Value && !CurrentDraft.IsDraftComplete())
		{
			SendTurnAnnouncements();
		}
	}

	// Briefly flashes the whole server message AND leaderboard white when a draft action is made,
	// then renders everything normally again.
	private IEnumerator FlashDraftAction()
	{
		// Flash white: server message + leaderboard
		_draftingLeaderboard?.FlashWhite();
		Render();
		yield return new WaitForSeconds(0.25f);

		// Restore leaderboard and render normally
		_draftingLeaderboard?.UpdateCountdown(CurrentTeam, _draftCountdownTime);
		Render();
	}

	// Decides how a finished draft continues:
	//  - exactly one map left and nothing picked yet -> Showdown auto-picks it (with visual feedback),
	//    afterwards there is a pick, so the machine moves on to StateDraftCompleted.
	//  - more than one map left and nothing picked -> StateDraftIncomplete (random selection).
	//  - normal completion (maps were picked) -> StateDraftCompleted.
	private void HandleDraftComplete()
	{
		// The draft is done - no more turns are coming, so the still-ticking turn countdown must be
		// stopped now. Otherwise a stray timeout could still fire afterwards.
		Countdown.Stop();

		if (CurrentDraft.PickedLevels.Count == 0 && AvailableMaps.Count == 1)
		{
			Showdown.StartCoroutine(AutoPickLastRemainingLevel());
			return;
		}

		InvokeFinish();
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
			$"<br><{ShowdownColors.Gray}><i><{ShowdownColors.Yellow}>Warning</color>: Your selection will be FINAL and cannot be undone!</i></color>",
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
		return $"<{team.Color}><b>{team.Racers[index].SteamName}</b></color>";
	}

	private string BuildDraftHistory()
	{
		StringBuilder historyMessage = new();
		foreach (DraftAction draftAction in CurrentDraft.Actions)
		{
			string actionType = draftAction.GetActionType();
			string actionColor;
			if (draftAction.IsPass)
			{
				actionColor = ShowdownColors.Yellow;
			}
			else
			{
				actionColor = draftAction.IsPick ? ShowdownColors.Green : ShowdownColors.Red;
			}

			string line = $"<{draftAction.Team.Color}>{draftAction.Team.GetTag()}</color> " +
			              $"<{actionColor}>{actionType}</color>";

			// A pass has no level associated with it.
			if (!draftAction.IsPass)
			{
				line += $" <{ShowdownColors.Cyan}>{draftAction.Level.Name}</color>";
			}

			historyMessage.AppendLine(line);
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
				return $"It's time to <{ShowdownColors.Green}><b>pick</b></color> a map!" +
				       $"<br>Use {ShowdownColors.Command("!pick 1-7")} to pick a map.";
			}

			return
				$"You can <{ShowdownColors.Green}><b>pick</b></color> or <{ShowdownColors.Red}><b>ban</b></color> a map!" +
				$"<br>Use {ShowdownColors.Command("!pick 1-7")} to pick or {ShowdownColors.Command("!ban 1-7")} to ban a map.";
		}

		if (current.Bans > 0)
		{
			return $"You can only <{ShowdownColors.Red}><b>ban</b></color> a map!" +
			       $"<br>Use {ShowdownColors.Command("!ban 1-7")} to ban a map.";
		}

		return "";
	}

	private void OnDraftTick(int remainingSeconds)
	{
		_draftCountdownTime = remainingSeconds;
		_draftingLeaderboard?.UpdateCountdown(CurrentTeam, remainingSeconds);
		Render();
	}

	private void OnDraftTimeout()
	{
		if (IsDraftLocked())
		{
			// The draft already finished (or is auto-picking) - a stray, still-ticking timer must
			// not be able to act anymore.
			return;
		}

		_draftCountdownTime = MyConfig.DraftTimeConfig.Value;
		CurrentTeam.MissedDraft = true;

		if (OtherTeam.Bans == 0 && OtherTeam.Picks == 0)
		{
			Team currentTeam = CurrentTeam;
			OnlineZeeplevel randomLevel = AvailableMaps[_random.Next(AvailableMaps.Count)];

			if (currentTeam.Picks > 0)
			{
				CurrentDraft.PickLevel(randomLevel);
				ChatMessage.SendCustomMessage(
					$"{currentTeam.GetColoredTag()} has randomly <{ShowdownColors.Green}>picked</color> <{ShowdownColors.Cyan}>{randomLevel.Name}</color>");
			}
			else if (currentTeam.Bans > 0)
			{
				CurrentDraft.BanLevel(randomLevel);
				ChatMessage.SendCustomMessage(
					$"{currentTeam.GetColoredTag()} has randomly <{ShowdownColors.Red}>banned</color> <{ShowdownColors.Cyan}>{randomLevel.Name}</color>");
			}

			Render();
			return;
		}

		CurrentDraft.SwitchTeam();
		_draftingLeaderboard?.UpdateActiveTeam(CurrentTeam, MyConfig.DraftTimeConfig.Value);
		Countdown.Start(MyConfig.DraftTimeConfig.Value, OnDraftTick, OnDraftTimeout);
	}

	// Only one map remains after the last possible action, so Showdown picks it automatically.
	// A short reveal countdown gives players time to see what is happening before it is locked in.
	private IEnumerator AutoPickLastRemainingLevel()
	{
		_isAutoPickInProgress = true;

		OnlineZeeplevel lastLevel = AvailableMaps[0];

		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);
		ChatMessage.SendCustomMessage(
			$"Only one map remains - <{ShowdownColors.Red}>Showdown</color> will automatically pick <{ShowdownColors.Cyan}>{lastLevel.Name}</color>!");

		for (int remaining = MyConfig.AutoPickRevealCountdownConfig.Value; remaining > 0; remaining--)
		{
			SendAutoPickMessage(lastLevel, remaining);
			yield return new WaitForSeconds(1f);
		}

		CurrentDraft.PickedLevels.Add(new DraftAction(lastLevel, Team.CreateShowdownTeam(), true));
		SendAutoPickMessage(lastLevel, 0);
		ChatMessage.SendCustomMessage(
			$"<{ShowdownColors.Red}>Showdown</color> automatically <{ShowdownColors.Green}>picked</color> <{ShowdownColors.Cyan}>{lastLevel.Name}</color>");

		_isAutoPickInProgress = false;
		InvokeFinish();
	}

	private void SendAutoPickMessage(OnlineZeeplevel level, int remaining)
	{
		ServerMessage msg = new ServerMessage().ShowdownHeader(true)
			.AddLine(l => l.Size(30).Bold().AddBlock(Match.ScoreColored()).Indent("585%"))
			.AddLine(line => line
				.AddBlock("Only one map left!", builder => builder.Color(ShowdownColors.Yellow).Bold().Size(40)))
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("Showdown auto-picks", builder => builder.Color(ShowdownColors.Red))
				.AddBlock(level.Name, builder => builder.Color(ShowdownColors.Green).Bold()));

		if (remaining > 0)
		{
			msg.AddSeparator()
				.AddLine(line => line
					.AddBlock("Locking in in")
					.AddBlock($"{remaining}", builder => builder.Color(ShowdownColors.Green).Bold())
					.AddBlock("seconds..."));
		}
		else
		{
			msg.AddSeparator()
				.AddLine(line => line
					.AddBlock("Locked in!", builder => builder.Color(ShowdownColors.Green).Bold()));
		}

		msg.Send();
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

	// The draft no longer accepts input once it is finished or the auto-pick reveal is running.
	private bool IsDraftLocked()
	{
		return _isAutoPickInProgress || _isDraftCompleteCountdownStarted;
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

			if (!currentDraft.IsDraftComplete())
			{
				_draftingLeaderboard?.UpdateActiveTeam(CurrentTeam, MyConfig.DraftTimeConfig.Value);
				Countdown.Start(MyConfig.DraftTimeConfig.Value, OnDraftTick, OnDraftTimeout);
			}

			// Flash white on both server message and leaderboard for any draft action (pick or ban).
			Showdown.StartCoroutine(FlashDraftAction());
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
			$"{currentTeam.GetColoredTag()} has <{ShowdownColors.Red}>banned</color> <{ShowdownColors.Cyan}>{levelToPickOrBan.Name}</color>");
	}

	private void HandlePick(Draft currentDraft, OnlineZeeplevel level, Team currentTeam, ZeepkistNetworkPlayer player)
	{
		if (player.IsLocal && currentDraft.IsDraftComplete())
		{
			Team showdownTeam = Team.CreateShowdownTeam();
			currentDraft.PickedLevels.Add(new DraftAction(level, showdownTeam, true));
			ChatMessage.SendCustomMessage(
				$"{showdownTeam.GetColoredTag()} has <{ShowdownColors.Green}>picked</color> <{ShowdownColors.Cyan}>{level.Name}</color>");
		}
		else
		{
			currentDraft.PickLevel(level);
			ChatMessage.SendCustomMessage(
				$"{currentTeam.GetColoredTag()} has <{ShowdownColors.Green}>picked</color> <{ShowdownColors.Cyan}>{level.Name}</color>");
		}
	}
}