using System.Collections.Generic;
using UnityEngine;

public class Timer : Disposable
{
	private float _value;
	private float _max;
	private bool _pause;

	private static HashSet<Timer> AllTimers;

	/// <summary>
	/// Call 'Tick' with a delta time to count down the timer.
	/// 'Tick' returns true if it expired that 'tick'.
	/// </summary>
	/// <param name="max">Time until timer 'fires'</param>
	public Timer(float max)
	{
		_max = max;
		_value = 0;
		if (AllTimers == null)
			AllTimers = new HashSet<Timer>();
		AllTimers.Add(this);
	}

	protected override void InternalDispose()
	{
		AllTimers.Remove(this);
	}


	public float TimeRemaining
	{
		get { return _max - _value; }
		set { _value = _max - value; }
	}

	public bool Tick(float delta)
	{
		if (_pause)
			return false;
		_value += delta;
		if (_value < _max)
			return false;
		_value = 0;
		_pause = true;
		return true;
	}

	public void Pause()
	{
		_pause = true;
	}

	public void Resume()
	{
		_pause = false;
	}

	public void Restart()
	{
		_value = 0;
		Resume();
	}

	public void ResetInterval(float interval)
	{
		_max = interval;
		Restart();
	}

	/// <summary>
	/// Pauses all timers
	/// </summary>
	public static void PauseAll()
	{
		foreach (var timer in Timer.AllTimers)
		{
			timer.Pause();
		}
	}

	/// <summary>
	/// Resumes all timers
	/// </summary>
	public static void ResumeAll()
	{
		foreach (var timer in Timer.AllTimers)
		{
			timer.Resume();
		}
	}

}