using System.Collections.Generic;
using Imui.Controls;
using Imui.Core;
using Showdown4.Config;
using Showdown4.Entities;
using UnityEngine;
using ZeepSDK.UI;

namespace Showdown4.Utils;

/// <summary>
///     In-game team-selection panel drawn through ZeepSDK's immediate-mode GUI.
///     Toggle with <see cref="MyConfig.GuiToggleKeyConfig" /> (default F4).
///     Press "Refresh" to reload teams from the JSON configured in the config file.
/// </summary>
public class ShowdownGuiDrawer : IZeepGUIDrawer
{
	private List<TeamJsonEntry> _teams = new();
	private bool _visible;

	public void OnZeepGUI(ImGui gui)
	{
		if (Input.GetKeyDown(MyConfig.GuiToggleKeyConfig.Value))
		{
			_visible = !_visible;
			if (_visible)
			{
				LoadTeams();
			}
		}

		if (!_visible)
		{
			return;
		}

		if (!gui.BeginWindow("Team Selection", ref _visible, (400, 500)))
		{
			return;
		}

		gui.Text("Teams");
		gui.AddSpacing();

		if (gui.Button("Refresh from Config", (200, 32)))
		{
			LoadTeams();
		}

		gui.AddSpacing();
		gui.Separator();
		gui.AddSpacing();

		if (_teams.Count == 0)
		{
			gui.Text("No teams found. Check your config file.");
		}
		else
		{
			gui.BeginScrollable();

			foreach (TeamJsonEntry team in _teams)
			{
				string playerCount = team.Players != null ? team.Players.Count.ToString() : "0";
				gui.Text($"[{team.Tag}] {team.Name}  ({playerCount} players)");
			}

			gui.EndScrollable();
		}

		gui.EndWindow();
	}

	private void LoadTeams()
	{
		TeamData data = MyConfig.LoadTeams();
		_teams = data.Teams ?? new List<TeamJsonEntry>();
	}
}