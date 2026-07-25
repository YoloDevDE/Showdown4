using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using Imui.Controls;
using Imui.Core;
using Showdown4.Commands;
using Showdown4.Config;
using Showdown4.Entities;
using UnityEngine;
using ZeepSDK.Playlist;
using ZeepSDK.UI;

namespace Showdown4.Utils;

/// <summary>
///     In-game control panel drawn through ZeepSDK's immediate-mode GUI (<see cref="IZeepGUIDrawer" />).
///     Three tabs:
///     - Controls: buttons for all sd-commands (start/stop, next/prev, pause/resume, restart/restartstate)
///     - Playlists: competition and intermission playlist pickers
///     - Teams: full team editor with per-player name, SteamId and qualification time
///     Toggle with <see cref="MyConfig.GuiToggleKeyConfig" /> (default F4).
/// </summary>
public class ShowdownGuiDrawer : IZeepGUIDrawer
{
	private Tab _activeTab = Tab.Controls;

	// ── Playlists ─────────────────────────────────────────────────────────
	private bool _competitionExpanded;
	private bool _intermissionExpanded;
	private string[] _playlistNames = Array.Empty<string>();

	// ── Team editor state ─────────────────────────────────────────────────
	// Editable mirror of the config; loaded once when the panel opens or via "Reload".
	private List<TeamEditorEntry> _teams = new();
	private bool _teamsDirty; // true = unsaved changes

	// ── Visibility ────────────────────────────────────────────────────────
	private bool _visible;

	// ─────────────────────────────────────────────────────────────────────
	// IZeepGUIDrawer
	// ─────────────────────────────────────────────────────────────────────

	public void OnZeepGUI(ImGui gui)
	{
		if (Input.GetKeyDown(MyConfig.GuiToggleKeyConfig.Value))
		{
			_visible = !_visible;
			if (_visible)
			{
				RefreshPlaylists();
				LoadTeamsFromConfig();
			}
		}

		if (!_visible)
		{
			return;
		}

		if (!gui.BeginWindow("Showdown Control Panel", ref _visible, (520, 680)))
		{
			return;
		}

		DrawTabBar(gui);
		gui.Separator();

		switch (_activeTab)
		{
			case Tab.Controls:
				DrawControlsTab(gui);
				break;
			case Tab.Playlists:
				DrawPlaylistsTab(gui);
				break;
			case Tab.Teams:
				DrawTeamsTab(gui);
				break;
		}

		gui.EndWindow();
	}

	// ─────────────────────────────────────────────────────────────────────
	// Tab bar
	// ─────────────────────────────────────────────────────────────────────

	private void DrawTabBar(ImGui gui)
	{
		// Simple button-row tab bar — no native tab widget needed.
		string teamsLabel = _teamsDirty ? "Teams *" : "Teams";
		if (gui.Button("Controls", (160, 28)))
		{
			_activeTab = Tab.Controls;
		}

		if (gui.Button("Playlists", (160, 28)))
		{
			_activeTab = Tab.Playlists;
		}

		if (gui.Button(teamsLabel, (160, 28)))
		{
			_activeTab = Tab.Teams;
		}

		gui.AddSpacing();
	}

	// ─────────────────────────────────────────────────────────────────────
	// Controls tab
	// ─────────────────────────────────────────────────────────────────────

	private void DrawControlsTab(ImGui gui)
	{
		gui.Text("Showdown Controls");
		gui.AddSpacing();

		// Start / Stop
		if (gui.Button("▶  Start Showdown", (240, 36)))
		{
			CommandShowdownStart.Fire();
		}

		if (gui.Button("⏹  Stop Showdown", (240, 36)))
		{
			CommandShowdownStop.Fire();
		}

		gui.AddSpacing();
		gui.Separator();
		gui.AddSpacing();

		// State navigation
		gui.Text("State Navigation");
		gui.AddSpacing();

		if (gui.Button("⏭  Next State  (sd next)", (240, 32)))
		{
			CommandFinishState.Fire();
		}

		if (gui.Button("⏮  Prev State  (sd prev)", (240, 32)))
		{
			CommandShowdownPrev.Fire();
		}

		gui.AddSpacing();
		gui.Separator();
		gui.AddSpacing();

		// Timers
		gui.Text("Timers");
		gui.AddSpacing();

		if (gui.Button("⏸  Pause  (sd pause)", (240, 32)))
		{
			CommandShowdownPause.Fire();
		}

		if (gui.Button("▶  Resume  (sd resume)", (240, 32)))
		{
			CommandShowdownResume.Fire();
		}

		gui.AddSpacing();
		gui.Separator();
		gui.AddSpacing();

		// Restart
		gui.Text("Restart");
		gui.AddSpacing();

		if (gui.Button("↺  Restart Showdown  (sd restart)", (240, 32)))
		{
			CommandShowdownRestart.Fire();
		}

		if (gui.Button("↺  Restart State  (sd restartstate)", (240, 32)))
		{
			CommandStateRestart.Fire();
		}
	}

	// ─────────────────────────────────────────────────────────────────────
	// Playlists tab
	// ─────────────────────────────────────────────────────────────────────

	private void DrawPlaylistsTab(ImGui gui)
	{
		gui.Text("Levels / Playlists");
		gui.AddSpacing();

		if (_playlistNames.Length == 0)
		{
			gui.Text("No playlists found on disk.");
			if (gui.Button("Refresh", (120, 30)))
			{
				RefreshPlaylists();
			}

			return;
		}

		DrawPlaylistPicker(gui, "Competition level pool",
			MyConfig.CompetitionLevelsPlaylistNameConfig,
			ref _competitionExpanded);

		gui.AddSpacing();

		DrawPlaylistPicker(gui, "Intermission level",
			MyConfig.IntermissionLevelPlaylistNameConfig,
			ref _intermissionExpanded);

		gui.AddSpacing();

		if (gui.Button("Refresh playlists", (180, 30)))
		{
			RefreshPlaylists();
		}
	}

	private void DrawPlaylistPicker(ImGui gui, string label,
		ConfigEntry<string> configEntry, ref bool expanded)
	{
		gui.Text(label + ":");
		string current = string.IsNullOrEmpty(configEntry.Value) ? "<none>" : configEntry.Value;
		if (gui.Button(current + "  ▼", (300, 30)))
		{
			expanded = !expanded;
		}

		if (!expanded)
		{
			return;
		}

		foreach (string name in _playlistNames)
			if (gui.Button("   " + name, (300, 28)))
			{
				configEntry.Value = name;
				expanded = false;
			}
	}

	// ─────────────────────────────────────────────────────────────────────
	// Teams tab
	// ─────────────────────────────────────────────────────────────────────

	private void DrawTeamsTab(ImGui gui)
	{
		// Header row
		gui.Text("Team Editor");
		gui.AddSpacing();

		// Action buttons
		if (gui.Button("+ Add Team", (120, 28)) && _teams.Count < MyConfig.TeamSlotCount)
		{
			_teams.Add(new TeamEditorEntry { Name = "New Team", Tag = "TAG", Color = "#ffffff" });
			_teamsDirty = true;
		}

		if (gui.Button("Reload from Config", (160, 28)))
		{
			LoadTeamsFromConfig();
		}

		if (gui.Button(_teamsDirty ? "Save *" : "Save", (100, 28)))
		{
			SaveTeamsToConfig();
		}

		gui.AddSpacing();
		gui.Separator();
		gui.AddSpacing();

		// Scrollable team list
		gui.BeginScrollable();

		for (int ti = 0; ti < _teams.Count; ti++)
		{
			TeamEditorEntry team = _teams[ti];

			// Team header: collapse/expand toggle + remove button
			string header = $"[{(team.Expanded ? "▼" : "▶")}] [{team.Tag}] {team.Name}";
			if (gui.Button(header, (360, 28)))
			{
				team.Expanded = !team.Expanded;
			}

			if (gui.Button("✕", (36, 28)))
			{
				_teams.RemoveAt(ti);
				_teamsDirty = true;
				ti--;
				continue;
			}

			if (!team.Expanded)
			{
				continue;
			}

			// Team fields
			gui.AddSpacing();
			DrawTeamFields(gui, team);

			gui.AddSpacing();

			// Players header
			gui.Text($"  Players ({team.Players.Count}):");
			gui.AddSpacing();

			for (int pi = 0; pi < team.Players.Count; pi++)
			{
				PlayerEditorEntry player = team.Players[pi];
				DrawPlayerRow(gui, team, player, pi);
			}

			if (gui.Button("  + Add Player", (160, 26)))
			{
				team.Players.Add(new PlayerEditorEntry());
				_teamsDirty = true;
			}

			gui.AddSpacing();
			gui.Separator();
			gui.AddSpacing();
		}

		gui.EndScrollable();
	}

	private void DrawTeamFields(ImGui gui, TeamEditorEntry team)
	{
		// Name
		gui.Text("  Name:");
		string name = team.Name;
		if (gui.TextEdit(ref name, (260, 26)))
		{
			team.Name = name;
			_teamsDirty = true;
		}

		// Tag
		gui.Text("  Tag:");
		string tag = team.Tag;
		if (gui.TextEdit(ref tag, (120, 26)))
		{
			team.Tag = tag;
			_teamsDirty = true;
		}

		// Color
		gui.Text("  Color (hex):");
		string color = team.Color;
		if (gui.TextEdit(ref color, (120, 26)))
		{
			team.Color = color;
			_teamsDirty = true;
		}
	}

	private void DrawPlayerRow(ImGui gui, TeamEditorEntry team, PlayerEditorEntry player, int index)
	{
		gui.Text($"  P{index + 1} Name:");
		string pname = player.Name;
		if (gui.TextEdit(ref pname, (200, 24)))
		{
			player.Name = pname;
			_teamsDirty = true;
		}

		gui.Text("  SteamId:");
		string sid = player.SteamId;
		if (gui.TextEdit(ref sid, (160, 24)))
		{
			player.SteamId = sid;
			_teamsDirty = true;
		}

		gui.Text("  Quali (s):");
		string qt = player.QualificationTime;
		if (gui.TextEdit(ref qt, (100, 24)))
		{
			player.QualificationTime = qt;
			_teamsDirty = true;
		}

		if (gui.Button("✕", (36, 24)))
		{
			team.Players.RemoveAt(index);
			_teamsDirty = true;
		}

		gui.AddSpacing();
	}

	// ─────────────────────────────────────────────────────────────────────
	// Config <-> editor state
	// ─────────────────────────────────────────────────────────────────────

	private void LoadTeamsFromConfig()
	{
		TeamData data = MyConfig.LoadTeams();
		_teams = (data.Teams ?? new List<TeamJsonEntry>())
			.Select(t => new TeamEditorEntry
			{
				Name = t.Name ?? "",
				Tag = t.Tag ?? "",
				Color = t.Color ?? "#ffffff",
				Players = (t.Players ?? new List<TeamPlayerJsonEntry>())
					.Select(p => new PlayerEditorEntry
					{
						Name = p.Name ?? "",
						SteamId = p.SteamId.ToString(),
						QualificationTime = p.QualificationTime.ToString("G")
					})
					.ToList()
			})
			.ToList();
		_teamsDirty = false;
	}

	private void SaveTeamsToConfig()
	{
		TeamData data = new()
		{
			Teams = _teams
				.Select(t => new TeamJsonEntry
				{
					Name = t.Name,
					Tag = t.Tag,
					Color = t.Color,
					Players = t.Players
						.Select(p => new TeamPlayerJsonEntry
						{
							Name = p.Name,
							SteamId = ulong.TryParse(p.SteamId, out ulong sid) ? sid : 0UL,
							QualificationTime = double.TryParse(
								p.QualificationTime,
								NumberStyles.Any,
								CultureInfo.InvariantCulture,
								out double qt)
								? qt
								: 0.0
						})
						.ToList()
				})
				.ToList()
		};

		MyConfig.SaveTeams(data);
		_teamsDirty = false;
	}

	private void RefreshPlaylists()
	{
		_playlistNames = PlaylistApi.GetPlaylists()
			.Select(playlist => playlist.name)
			.Where(name => !string.IsNullOrEmpty(name))
			.ToArray();
	}

	// ── Tab ──────────────────────────────────────────────────────────────
	private enum Tab
	{
		Controls,
		Playlists,
		Teams
	}

	// ─────────────────────────────────────────────────────────────────────
	// Editor entry types (GUI state only — not persisted directly)
	// ─────────────────────────────────────────────────────────────────────

	private class TeamEditorEntry
	{
		public string Color = "#ffffff";
		public bool Expanded = true;
		public string Name = "";
		public List<PlayerEditorEntry> Players = new();
		public string Tag = "";
	}

	private class PlayerEditorEntry
	{
		public string Name = "";
		public string QualificationTime = "0";
		public string SteamId = "";
	}
}