using System;
using System.Collections;
using System.Collections.Generic;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistNetworking;
using ZeepSDK.Chat;
using Random = System.Random;

namespace Showdown4.States.Showdown;

public class State_RandomMapSelection : IState
{
    private const int MinSpinLoops = 8;
    private const int MaxSpinLoops = 30;
    private ShowdownStateMachine _showdown;

    public State_RandomMapSelection(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        _showdown = StateMachine as ShowdownStateMachine;
        CoroutineManager.AddCoroutine(RandomSelectionAnimation());
    }

    public void Execute()
    {
    }

    public void Exit()
    {
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private IEnumerator RandomSelectionAnimation()
    {
        float delay = 0.10f;
        int selectedIndex = 0;
        int randomLoops = new Random().Next(MinSpinLoops, MaxSpinLoops);
        List<OnlineZeeplevel> availableMaps = _showdown.Match.CurrentDraft.AvailableLevels;

        for (int i = 0; i < randomLoops; i++)
        {
            selectedIndex = (selectedIndex + 1) % availableMaps.Count;
            UpdateRandomSelectionMessage(selectedIndex);
            delay += 0.05f;
            yield return new WaitForSeconds(delay);
        }

        SelectRandomMap(availableMaps[selectedIndex]);
        InvokeFinish();
    }

    private void UpdateRandomSelectionMessage(int currentIndex)
    {
        List<OnlineZeeplevel> availableMaps = _showdown.Match.CurrentDraft.AvailableLevels;
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line.AddBlock("Random Map Selection..."))
            .AddSeparator();

        for (int i = 0; i < availableMaps.Count; i++)
        {
            OnlineZeeplevel level = availableMaps[i];

            if (i == currentIndex)
            {
                msg.AddLine(line => line.AddBlock($"> {level.Name}", block => block.Bold().Color("#ffff00")));
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
        ChatApi.SendMessage($"Randomly selected map: {selectedLevel.Name}");
        _showdown.Match.CurrentDraft.PickedLevels.Add(new DraftAction(selectedLevel, new Team("Showdown", "Showdown", "#ff0000"), true));
    }
}