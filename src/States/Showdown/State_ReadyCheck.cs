using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;

namespace Showdown4.States.Showdown;

public class State_ReadyCheck : IState
{
    private HashSet<ulong> _readyPlayers; // Store the IDs of players who are ready

    public State_ReadyCheck(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;
    private Team _teamA => _Showdown.Match.TeamA;
    private Team _teamB => _Showdown.Match.TeamB;
    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        _readyPlayers = new HashSet<ulong>();
        CommandReady.CommandInvoked += OnReady;
    }

    public void Execute()
    {
        ServerMessage msg = ShowPickedMaps()
            .AddSeparator()
            .AddLine(line => line.AddBlock("Ready Status:"));

        foreach (KeyValuePair<uint, ZeepkistNetworkPlayer> playerEntry in ZeepkistNetwork.Players)
        {
            ZeepkistNetworkPlayer player = playerEntry.Value;
            bool isReady = _readyPlayers.Contains(player.SteamID);
            msg.AddLine(line => line
                .AddBlock($"{player.Username}:")
                .AddBlock(isReady ? "Ready" : "Not Ready", block => block.Color(isReady ? "#00ff00" : "#ff0000"))
            );
        }

        msg.Send();
    }

    public void Exit()
    {
        CommandReady.CommandInvoked -= OnReady;
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private void OnReady(ulong steamId, string arg)
    {
        if (!_readyPlayers.Contains(steamId))
        {
            _readyPlayers.Add(steamId); // Mark player as ready

            // Send a message confirming the player's readiness
            ChatMessage.SendCustomMessage($"{ZeepkistNetworkService.GetSteamNameFromSteamId(steamId)} is ready!");

            // Check if all players are ready
            if (_readyPlayers.Count >= ZeepkistNetwork.Players.Count)
            {
                ConfirmReady(); // All players ready, finish the state
            }
            else
            {
                Execute(); // Update the ready status in the server message
            }
        }
    }

    private ServerMessage ShowPickedMaps()
    {
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock($"{_teamA.GetNameWithTag()}", b => b.Color(_teamA.Color))
                .AddBlock("VS")
                .AddBlock($"{_teamB.GetNameWithTag()}", b => b.Color(_teamB.Color))
            )
            .AddSeparator()
            .AddLine(line => line
                .AddBlock("Picked Maps:"));

        int round = 1;
        foreach (DraftAction pickedLevel in _Showdown.Match.Drafts.SelectMany(matchDraft => matchDraft.PickedLevels))
        {
            msg.AddLine(line =>
            {
                if (round <= _Showdown.Match.RoundCounter())
                {
                    line.StrikeThrough();
                }

                line
                    .AddBlock($"Round {round}:")
                    .AddBlock($"'{pickedLevel.Level.Name}'", block => block.Color("#00ffff"))
                    .AddBlock("picked by", block => block.Indent("600%"))
                    .AddBlock($"{pickedLevel.Team.GetColoredTag()}")
                    .Bold();
                round++;
            });
        }

        // Add the ready check instruction at the end
        msg
            .AddSeparator()
            .AddLine(line => line
                .AddBlock("To ready up, type")
                .AddBlock("'!ready'", b => b.Color("#ffff00").Bold())
                .AddBlock("in the chat.")
            );

        return msg;
    }

    private void ConfirmReady()
    {
        // Notify that all players are ready
        ChatMessage.SendCustomMessage("All players are ready.");

        // Display message in the server with a 2-second delay before proceeding
        CoroutineManager.Instance.StartExternalCoroutine(ReadyCountdown());
    }

    private IEnumerator ReadyCountdown()
    {
        ServerMessage msg = ShowPickedMaps()
            .AddSeparator()
            .AddLine(line => line.AddBlock("Ready Status:"));

        foreach (KeyValuePair<uint, ZeepkistNetworkPlayer> playerEntry in ZeepkistNetwork.Players)
        {
            ZeepkistNetworkPlayer player = playerEntry.Value;
            bool isReady = _readyPlayers.Contains(player.SteamID);
            msg.AddLine(line => line
                .AddBlock($"{player.Username}: ", block => block.Bold())
                .AddBlock(isReady ? "Ready" : "Not Ready", block => block.Color(isReady ? "#00ff00" : "#ff0000"))
            );
        }

        msg
            .AddSeparator()
            .AddLine(line => line
                .AddBlock("Everyone ready. Prepare for battle!", block => block.Color("#00ff00").Bold()))
            .AddSeparator();
        msg.Send();

        // Wait for 2 seconds before transitioning to the next state
        yield return new WaitForSeconds(2);

        // Proceed to the next state
        Finished?.Invoke();
    }
}