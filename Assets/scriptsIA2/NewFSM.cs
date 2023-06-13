using System;

public class NewFSM<T>
{
	
	public States<T> Current { get { return current; } }
	private States<T> current;

	public NewFSM(States<T> initial)
	{
		current = initial;
		current.Enter(default(T));
	}

	public void SendInput(T input)
	{
		States<T> newState;

		if (current.CheckInput(input, out newState))
		{
			current.Exit(input);
			current = newState;
			current.Enter(input);
		}
	}


	public void Update()
	{
		current.Update();
	}

	public void LateUpdate()
	{
		current.LateUpdate();
	}

	public void FixedUpdate()
	{
		current.FixedUpdate();
	}
	
}
