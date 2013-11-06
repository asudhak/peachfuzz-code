using System;

namespace Peach.Core.Dom.Actions
{
	[Action("Stop")]
	public class Stop : Action
	{
		protected override void OnRun(Publisher publisher, RunContext context)
		{
			publisher.close();
			publisher.stop();
		}
	}
}
