using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core.Publishers
{
	[Publisher("FilePerIteration", true)]
	[ParameterAttribute("FileName", typeof(string), "Name of file to open for reading/writing", true)]
	public class FilePerIterationPublisher : FilePublisher
	{
		protected string fileTemplate;

		public FilePerIterationPublisher(Dictionary<string, Variant> args) : base(args)
		{
			fileTemplate = fileName;

			try
			{
				setFileName(0);

				if (fileName == fileTemplate)
					throw new PeachException("Error, FileName \"" + fileTemplate + "\" missing iteration format identifier.");

				fileName = null;
			}
			catch (FormatException)
			{
				throw new PeachException("Error, FileName \"" + fileTemplate + "\" is not a valid format string.");
			}
		}

		protected void setFileName(uint iteration)
		{
			fileName = string.Format(fileTemplate, iteration);
		}

		public override void open(Dom.Action action)
		{
			setFileName(this.Iteration);

			base.open(action);
		}
	}
}
