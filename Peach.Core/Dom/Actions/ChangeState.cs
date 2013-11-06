using System;

namespace Peach.Core.Dom.Actions
{
	[Action("ChangeState")]
	public class ChangeState : Action
	{
		/// <summary>
		/// Name of state to change to, type=ChangeState
		/// </summary>
		public string reference { get; set; }

		protected override void OnRun(Publisher publisher, RunContext context)
		{
			State newState;

			if (!parent.parent.states.TryGetValue(reference, out newState))
			{
				logger.Debug("Error, unable to locate state '{0}'", reference);
				throw new PeachException("Error, unable to locate state '" + reference + "' provided to action '" + name + "'");
			}

			logger.Debug("Changing to state: {0}", reference);
			throw new ActionChangeStateException(newState);
		}
	}
}
