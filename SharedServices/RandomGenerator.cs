using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.SharedServices
{
	public class RandomGenerator : Random
	{
		public RandomGenerator(int seed) : base(seed)
		{ }
		
		public RandomGenerator() : this(DateTime.Now.Ticks)
		{ }

		public RandomGenerator(long seed) : base((int)(seed % int.MaxValue))
		{ }
	}
}
