using System;

namespace Peach.Core.Dom.Actions
{
	[Action("Close")]
	public class Close : Action
	{
		protected override void OnRun(Publisher publisher, RunContext context)
		{
			publisher.start();
			publisher.close();
		}
	}
}
