// using System;
// using System.Collections.Generic;
// using Showdown4.Commands;
// using Showdown4.Entities;
// using ZeepSDK.Chat;
//
// namespace Showdown4.Statemachine;
//
// public class State_SelectInitiative : IState
// {
//     private MatchStateMachine _context;
//     private Team _teamWhoDoesntPickCoin;
//     private Team _teamWhoPicksCoin;
//     private List<string> headsOrTails;
//
//     public void Enter(IStateMachine context)
//     {
//         _context = (MatchStateMachine)context;
//         string serverMessage = "Draft Phase";
//
//         _teamWhoPicksCoin = new Random().Next(0, 1) == 0 ? _context.CurrentMatch.TeamA : _context.CurrentMatch.TeamB;
//         _teamWhoDoesntPickCoin = _teamWhoPicksCoin == _context.CurrentMatch.TeamA ? _context.CurrentMatch.TeamA : _context.CurrentMatch.TeamB;
//
//         ChatApi.SendMessage("/servermessage red 0 " + serverMessage);
//         ChatApi.SendMessage($"<br>Draft Phase started:<br>Selecting Initiative<br><br>" +
//                             $"{_teamWhoPicksCoin.GetNameWithTag()} Please use #heads or #tails");
//
//         CommandTails.CommandInvoked += OnCommandTails;
//         CommandHeads.CommandInvoked += OnCommandHeads;
//     }
//
//     public void Exit()
//     {
//     }
//
//     private void OnCommandHeads(ulong steamId)
//     {
//         if (_teamWhoPicksCoin.RacerA.SteamId == steamId || _teamWhoPicksCoin.RacerB.SteamId == steamId)
//         {
//             headsOrTails = new List<string> { "heads", "tails" };
//
//             HeadsOrTails();
//         }
//     }
//
//     private void OnCommandTails(ulong steamId)
//     {
//         if (_teamWhoPicksCoin.RacerA.SteamId == steamId || _teamWhoPicksCoin.RacerB.SteamId == steamId)
//         {
//             headsOrTails = new List<string> { "tails", "heads" };
//             HeadsOrTails();
//         }
//     }
//
//     private void HeadsOrTails()
//     {
//         ChatApi.SendMessage($"<br>" +
//                             $"{_teamWhoPicksCoin.GetNameWithTag()} has chosen: {headsOrTails[0]}!" +
//                             $"<br>" +
//                             $"{_teamWhoDoesntPickCoin.GetNameWithTag()} is remaining with: {headsOrTails[1]}");
//     }
// }

