using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Showdown4.Entities;
using Showdown4.States;
using Showdown4.States.Showdown;
using ZeepkistClient;
using ZeepkistNetworking;
using ZeepSDK.Chat;
using ZeepSDK.Level;

namespace Showdown4.Utils;

/// <summary>
///     Broadcasts the current Showdown match state to companion mods (e.g. the Tournament Casting UI
///     overlay) as a single hidden chat line whenever the match enters a new state.
///
///     The payload is a compact JSON object, base64-encoded and wrapped in <c>@SDSTATE@</c> sentinels
///     so that player names or rich-text in the state can never collide with TMP's parser, and it is
///     rendered at <c>size 0</c> so it stays invisible to players in chat.
///
///     Design notes:
///     <list type="bullet">
///         <item>Only the host emits - it is the only client that runs the state machine.</item>
///         <item>
///             It is sent as a normal chat message (not <c>/servermessage</c>), so it never competes
///             for the single server-message overlay slot the leaderboard uses, and has no length
///             pressure from the leaderboard message. Receivers listen on
///             <see cref="ChatApi.ChatMessageReceived" />.
///         </item>
///         <item>
///             Purely additive telemetry: it only reads match state and sends one chat line. It never
///             touches scoring, the leaderboard or gameplay, and any failure is swallowed so it can
///             never disturb the match.
///         </item>
///     </list>
/// </summary>
public static class CastBroadcast
{
	private const int SchemaVersion = 1;

	// Guards against re-sending an identical payload when a state is re-entered without any observable
	// state change.
	private static string _lastPayload;

	/// <summary>
	///     Called from <see cref="IStateMachine" /> right after a new state has been entered. Everything
	///     that is not a live Showdown match on the host is ignored.
	/// </summary>
	public static void OnStateEntered(IStateMachine machine)
	{
		try
		{
			if (machine is not ShowdownStateMachine showdown)
			{
				return;
			}

			Match match = showdown.Match;
			if (match == null || !ZeepkistNetwork.IsMasterClient)
			{
				return;
			}

			string json = BuildJson(match, PhaseOf(showdown.CurrentState));
			string payload = "@SDSTATE@" + Convert.ToBase64String(Encoding.UTF8.GetBytes(json)) + "@SDSTATE@";

			if (payload == _lastPayload)
			{
				return;
			}

			_lastPayload = payload;

			// size 0 -> invisible to players; plain chat message -> off the /servermessage overlay slot.
			ChatApi.SendMessage("<size=0%>" + payload + "</size>");
		}
		catch
		{
			// Telemetry must never disturb the match.
		}
	}

	// "StateRacing" -> "racing". Informational only; the receiver keys off the structured fields.
	private static string PhaseOf(IState state)
	{
		if (state == null)
		{
			return "unknown";
		}

		string name = state.GetType().Name;
		if (name.StartsWith("State", StringComparison.Ordinal))
		{
			name = name.Substring("State".Length);
		}

		return name.ToLowerInvariant();
	}

	private static string BuildJson(Match match, string phase)
	{
		StringBuilder sb = new();
		sb.Append('{');
		sb.Append("\"type\":\"showdown_state\",");
		sb.Append("\"v\":").Append(SchemaVersion).Append(',');
		sb.Append("\"phase\":").Append(Str(phase)).Append(',');

		sb.Append("\"match\":{");
		sb.Append("\"bestOf\":3,");
		sb.Append("\"scoreA\":").Append(match.TeamA.Wins).Append(',');
		sb.Append("\"scoreB\":").Append(match.TeamB.Wins).Append(',');
		sb.Append("\"roundWinners\":").Append(RoundWinners(match));
		sb.Append("},");

		sb.Append("\"teams\":{");
		sb.Append("\"A\":").Append(TeamJson(match.TeamA)).Append(',');
		sb.Append("\"B\":").Append(TeamJson(match.TeamB));
		sb.Append("},");

		sb.Append("\"map\":").Append(MapJson(match));

		sb.Append('}');
		return sb.ToString();
	}

	// One "A"/"B"/null per round played, in order. Reads the already-evaluated result field instead of
	// calling GetWinnerTeam, so an in-progress round is never force-evaluated (which could pick a random
	// winner) - it is simply reported as null until it is decided.
	private static string RoundWinners(Match match)
	{
		StringBuilder sb = new();
		sb.Append('[');
		for (int i = 0; i < match.Rounds.Count; i++)
		{
			if (i > 0)
			{
				sb.Append(',');
			}

			List<Team> sorted = match.Rounds[i].TeamsSortedByWinAsc;
			if (sorted is { Count: > 0 })
			{
				sb.Append(ReferenceEquals(sorted[0], match.TeamA) ? "\"A\"" : "\"B\"");
			}
			else
			{
				sb.Append("null");
			}
		}

		sb.Append(']');
		return sb.ToString();
	}

	private static string TeamJson(Team team)
	{
		StringBuilder sb = new();
		sb.Append('{');
		sb.Append("\"tag\":").Append(Str(team.Tag)).Append(',');
		sb.Append("\"name\":").Append(Str(team.Name)).Append(',');
		sb.Append("\"color\":").Append(Str(team.Color)).Append(',');
		sb.Append("\"players\":[");
		for (int i = 0; i < team.Racers.Count; i++)
		{
			if (i > 0)
			{
				sb.Append(',');
			}

			Racer racer = team.Racers[i];
			sb.Append('{');
			sb.Append("\"steamId\":").Append(Str(racer.SteamId.ToString(CultureInfo.InvariantCulture))).Append(',');
			sb.Append("\"name\":").Append(Str(racer.SteamName)).Append(',');
			sb.Append("\"qual\":").Append(Num(racer.QualificationTime));
			sb.Append('}');
		}

		sb.Append("]}");
		return sb.ToString();
	}

	// Only the current level's GTR hash (for record/PB lookups on the receiver) and which team picked it.
	private static string MapJson(Match match)
	{
		string hash = null;
		string currentUid = null;
		try
		{
			hash = LevelApi.CurrentHash;
		}
		catch
		{
			// ignored - no level loaded yet
		}

		try
		{
			currentUid = LevelApi.CurrentLevel?.UID;
		}
		catch
		{
			// ignored
		}

		string picker = PickerFor(match, currentUid);

		StringBuilder sb = new();
		sb.Append('{');
		sb.Append("\"hash\":").Append(Str(hash ?? ""));
		if (picker != null)
		{
			sb.Append(",\"picker\":").Append(Str(picker));
		}

		sb.Append('}');
		return sb.ToString();
	}

	// Matches the loaded level against every draft's picks (UID is comparable between the loaded level
	// and the drafted OnlineZeeplevel, cf. PlaylistManager), so the picker is correct across draft phases.
	private static string PickerFor(Match match, string currentUid)
	{
		if (string.IsNullOrEmpty(currentUid))
		{
			return null;
		}

		foreach (Draft draft in match.Drafts)
		{
			foreach (DraftAction action in draft.PickedLevels)
			{
				if (action.Level == null || action.Level.UID != currentUid)
				{
					continue;
				}

				if (ReferenceEquals(action.Team, match.TeamA))
				{
					return "A";
				}

				return ReferenceEquals(action.Team, match.TeamB) ? "B" : "random";
			}
		}

		return null;
	}

	private static string Str(string s)
	{
		if (s == null)
		{
			return "\"\"";
		}

		StringBuilder sb = new();
		sb.Append('"');
		foreach (char c in s)
		{
			switch (c)
			{
				case '"':
					sb.Append("\\\"");
					break;
				case '\\':
					sb.Append("\\\\");
					break;
				case '\b':
					sb.Append("\\b");
					break;
				case '\f':
					sb.Append("\\f");
					break;
				case '\n':
					sb.Append("\\n");
					break;
				case '\r':
					sb.Append("\\r");
					break;
				case '\t':
					sb.Append("\\t");
					break;
				default:
					if (c < 0x20)
					{
						sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
					}
					else
					{
						sb.Append(c);
					}

					break;
			}
		}

		sb.Append('"');
		return sb.ToString();
	}

	private static string Num(double d)
	{
		if (double.IsNaN(d) || double.IsInfinity(d))
		{
			return "0";
		}

		return d.ToString("0.###", CultureInfo.InvariantCulture);
	}
}
