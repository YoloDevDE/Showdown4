using UnityEngine;

namespace Showdown4.States.Showdown;

public class StateManager : MonoBehaviour
{
	private static StateManager _instance;

	private IState _currentState;

	public static StateManager Instance
	{
		get
		{
			if (_instance) return _instance;

			GameObject stateManagerObj = new("StateManager");
			_instance = stateManagerObj.AddComponent<StateManager>();
			DontDestroyOnLoad(stateManagerObj);

			return _instance;
		}
	}

	private void Update()
	{
		// Check for input on the current state
		_currentState?.HandleInput();
	}

	// Assign the current state to be handled for input detection
	public void SetCurrentState(IState state)
	{
		_currentState = state;
	}
}