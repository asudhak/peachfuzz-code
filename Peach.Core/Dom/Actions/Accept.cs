using System;

namespace Peach.Core.Dom.Actions
{
	[Action("Accept")]
	public class Accept : Action
	{
		protected override void OnRun(Publisher publisher, RunContext context)
		{
			publisher.start();
			publisher.open();
			publisher.accept();
		}
	}
}
