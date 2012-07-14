using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Peach.Core;

namespace Peach.Core.Analysis
{
	public abstract class Coverage
	{
		public static Coverage CreateInstance()
		{
			return (Coverage) ClassLoader.FindAndCreateByTypeAndName(typeof(Coverage), "CoverageImpl");
		}

		public abstract List<ulong> BasicBlocksForExecutable(string executable);
		public abstract List<ulong> CodeCoverageForExecutable(string executable, string arguments, List<ulong> basicBlocks = null);
	}
}
