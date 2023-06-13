using System;

public class Transition<T>
{
	
	public event Action<T> OnTransition = delegate { };
	public T Input { get { return input; } }
	public States<T> TargetState { get { return targetState; } }

	T input;
	States<T> targetState;


	public void OnTransitionExecute(T input)
	{
		OnTransition(input);
	}

	public Transition(T input, States<T> targetState)
	{
		this.input = input;
		this.targetState = targetState;
	}
	
}
