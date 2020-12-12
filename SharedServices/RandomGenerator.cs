using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.SharedServices
{
	public class RandomGenerator : Random
	{
		public RandomGenerator(int seed) : base(seed) 
		{ }

		public RandomGenerator(long seed) : base((int)(seed % int.MaxValue))
		{ }

		public RandomGenerator() : this(DateTime.Now.Ticks)
		{ }
	}

	public class DoomedService
	{
		private readonly RandomGenerator random;

		public DoomedService(RandomGenerator rand)
		{
			random = rand;
		}

		/// <summary>
		/// This function is a mock function that fails a lot (two-third of the time, to be exact), which gives us the opportunity to test our retry policy.
		/// </summary>
		public void GonnaFailTwoOutOfThree()
		{
			var decider = random.NextDouble();
			if (decider >= 1.0 / 3.0)
				throw new Exception("This is a special message, designed by the top experts on the " +
					"subject, in order to catch a special kind of exceptions, " +
					"a.k.a. this exception right here, and no one else!");
		}
	}
}
