using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Showdown4.Managers;

public class CoroutineManager : MonoBehaviour
{
	private readonly List<Coroutine> _activeCoroutines = new();

	public static CoroutineManager Instance
	{
		get
		{
			if (field)
			{
				return field;
			}


			GameObject obj = new("CoroutineManager");
			field = obj.AddComponent<CoroutineManager>();
			DontDestroyOnLoad(obj); // Optional: Keeps this object across scenes

			return field;
		}
	}

	public Coroutine StartExternalCoroutine(IEnumerator coroutine)
	{
		StopAllExternalCoroutines();
		Coroutine startedCoroutine = StartCoroutine(coroutine);
		_activeCoroutines.Add(startedCoroutine); // Track the coroutine
		return startedCoroutine;
	}

	public Coroutine AddExternalCoroutine(IEnumerator coroutine)
	{
		Coroutine startedCoroutine = StartCoroutine(coroutine);
		_activeCoroutines.Add(startedCoroutine); // Track the coroutine
		return startedCoroutine;
	}

	public void StopExternalCoroutine(Coroutine coroutine)
	{
		if (coroutine == null || !_activeCoroutines.Contains(coroutine))
		{
			return;
		}

		StopCoroutine(coroutine);
		_activeCoroutines.Remove(coroutine); // Remove it from the list
	}

	public void StopAllExternalCoroutines()
	{
		if (_activeCoroutines == null || _activeCoroutines.Count == 0)
		{
			return;
		}

		foreach (Coroutine coroutine in _activeCoroutines.Where(coroutine => coroutine != null))
			StopCoroutine(coroutine);

		_activeCoroutines.Clear(); // Clear the list once all are stopped
	}
}