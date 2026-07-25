using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Imui.Controls;
using Imui.Core;
using Showdown4.Config;
using Showdown4.Entities;
using UnityEngine;
using ZeepSDK.Playlist;
using ZeepSDK.UI;

namespace Showdown4.Utils;

/// <summary>
///     In-game control panel drawn through ZeepSDK's immediate-mode GUI (<see cref="IZeepGUIDrawer" />).
///     It lets the host pick the competition and intermission playlists from a dropdown - populated with
///     all playlists saved on disk (<see cref="PlaylistApi.GetPlaylists" />) - and writes the choice back
///     into the config. It also shows a quick overview of the teams currently configured in the config.
///     Toggle it with <see cref="MyConfig.GuiToggleKeyConfig" /> (default F6).
///     Imui's built-in <c>Dropdown</c> only works with enums, so the playlist pickers are implemented as
///     click-to-expand button lists, which is the immediate-mode equivalent of a dropdown for a
///     dynamically sized set of options.
/// </summary>
public class ShowdownGuiDrawer : IZeepGUIDrawer
{
	private bool _competitionExpanded;
	private bool _intermissionExpanded;

	// Cached playlist names for the pickers. Refreshed when the window is opened or via the button,
	// so we don't hit the playlist storage every single frame.
	private string[] _playlistNames = Array.Empty<string>();
	private bool _visible;

	public void OnZeepGUI(ImGui gui)
	{
		if (Input.GetKeyDown(MyConfig.GuiToggleKeyConfig.Value))
		{
			_visible = !_visible;
			if (_visible)
			{
				RefreshPlaylists();
			}
		}

		if (!_visible)
		{
			return;
		}

		if (!gui.BeginWindow("Showdown Control Panel", ref _visible, (460, 560)))
		{
			return;
		}

		DrawPlaylistSection(gui);
		gui.Separator();
		DrawTeamSection(gui);

		gui.EndWindow();
	}

	private void DrawPlaylistSection(ImGui gui)
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

		DrawPlaylistPicker(gui, "Competition level pool", MyConfig.CompetitionLevelsPlaylistNameConfig,
			ref _competitionExpanded);
		gui.AddSpacing();
		DrawPlaylistPicker(gui, "Intermission level", MyConfig.IntermissionLevelPlaylistNameConfig,
			ref _intermissionExpanded);

		gui.AddSpacing();
		if (gui.Button("Refresh playlists", (180, 30)))
		{
			RefreshPlaylists();
		}
	}

	private void DrawPlaylistPicker(ImGui gui, string label, ConfigEntry<string> configEntry,
		ref bool expanded)
	{
		gui.Text(label + ":");

		string current = string.IsNullOrEmpty(configEntry.Value) ? "<none>" : configEntry.Value;
		if (gui.Button(current + "  v", (300, 30)))
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

	private void DrawTeamSection(ImGui gui)
	{
		gui.Text("Configured teams");
		gui.AddSpacing();

		List<TeamJsonEntry> teams = MyConfig.LoadTeams().Teams;
		if (teams == null || teams.Count == 0)
		{
			gui.Text("No teams configured. Add them in the 'Teams' config section.");
			return;
		}

		gui.Text($"{teams.Count} team(s) configured:");
		foreach (TeamJsonEntry team in teams)
		{
			int playerCount = team.Players?.Count ?? 0;
			gui.Text($"[{team.Tag}] {team.Name} - {playerCount} player(s)");
		}
	}

	private void RefreshPlaylists()
	{
		_playlistNames = PlaylistApi.GetPlaylists()
			.Select(playlist => playlist.name)
			.Where(name => !string.IsNullOrEmpty(name))
			.ToArray();
	}
}