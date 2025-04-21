using Showdown4.States;
using UnityEngine;

namespace Showdown4.Managers;

public class StateManager : MonoBehaviour
{
    private static StateManager _instance;

    private IState _currentState;

    public static StateManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject stateManagerObj = new GameObject("StateManager");
                _instance = stateManagerObj.AddComponent<StateManager>();
                DontDestroyOnLoad(stateManagerObj); // Keep alive across scenes
            }

            return _instance;
        }
    }

    private void Update()
    {
        // Check for input on the current state
        if (_currentState != null)
        {
            _currentState.HandleInput();
        }
    }

    // Assign the current state to be handled for input detection
    public void SetCurrentState(IState state)
    {
        _currentState = state;
    }
}