// using Showdown4.Utils;
// using ZeepSDK.Chat;
// using ZeepSDK.Racing;
//
// namespace Showdown4.Domain.States;
//
// internal class State_PostRacing : IState
// {
//     private MatchStateMachine _context;
//
//     public string Test { get; set; }
//
//     public void Exit()
//     {
//         RacingApi.LevelLoaded -= OnLevelLoaded;
//     }
//
//     public void Enter(IStateMachine context)
//     {
//         _context = (MatchStateMachine)context;
//         RacingApi.LevelLoaded += OnLevelLoaded;
//
//         ChatApi.SendMessage(
//             new ChatMessage.Builder().NewLine()
//                 .DashedLine().NewLine()
//                 .TextLine($"Round {_context.CurrentMatch.RoundCounter()} over!!").NewLine()
//                 .DashedLine().Build().Message);
//     }
//
//     private void OnLevelLoaded()
//     {
//         _context.TransitionTo(new State_RaceEvaluation());
//     }
// }

