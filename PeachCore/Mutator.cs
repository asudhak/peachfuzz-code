using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeachCore
{
	/// <summary>
	/// Base class for Mutators.
	/// </summary>
	public abstract class Mutator
	{
		/// <summary>
		/// Weight this mutator will get chosen in random mutation mode.
		/// </summary>
		public int weight = 1;

		/// <summary>
		/// Name of this mutator
		/// </summary>
		public string name = "Unknown";

		/// <summary>
		/// Check to see if DataElement is supported by this 
		/// mutator.
		/// </summary>
		/// <param name="obj">DataElement to check</param>
		/// <returns>True if object is supported, else False</returns>
		public static bool supportedDataElement(DataElement obj);

		/// <summary>
		/// Move to next mutation.  Throws MutatorCompleted
		/// when no more mutations are available.
		/// </summary>
		public void next();

		/// <summary>
		/// Returns the total number of mutations this
		/// mutator is able to perform.
		/// </summary>
		/// <returns>Returns number of mutations mutater can generate.</returns>
		public int getCount();

		/// <summary>
		/// Perform a sequencial mutation.
		/// </summary>
		/// <param name="obj"></param>
		public void sequencialMutation(DataElement obj);

		/// <summary>
		/// Perform a random mutation.
		/// </summary>
		/// <param name="obj"></param>
		public void randomMutation(DataElement obj);
	}

	public class MutatorCompleted : Exception
	{
	}
}
