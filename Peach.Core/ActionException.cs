using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core
{
	/// <summary>
	/// Thrown when an error occurs in the action of a state model
	/// requiring the state model to exit.
	/// </summary>
	/// <remarks>
	/// This is not an error that will end fuzzing, instead we will exit
	/// the state model and continue to the next iteration.
	/// </remarks>
	public class ActionException : Exception
	{
	}
}
