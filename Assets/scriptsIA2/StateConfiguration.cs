
using System.Collections.Generic;

public class StateConfiguration<T>
{


		States<T> instance;
		Dictionary<T, Transition<T>> transitions = new Dictionary<T, Transition<T>>();

		public StateConfiguration(States<T> state)
		{
			instance = state;
		}

		public StateConfiguration<T> SetTransition(T input, States<T> target)
		{
			transitions.Add(input, new Transition<T>(input, target));
			return this;
		}

		public void Done()
		{
			instance.Configure(transitions);
		}
	
}

public static class StateConfiguration
{
	
	public static StateConfiguration<T> Create<T>(States<T> state)
	{
		return new StateConfiguration<T>(state);
	}
	
}
