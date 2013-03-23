using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Peach.Core.Dom;
using NLog;

namespace Peach.Core.Fixups
{
	/// <summary>
	/// Proxy class to allow writing fixups in a scripting language like python
	/// or ruby.
	/// </summary>
	/// <remarks>
	/// The constructor will be passed a reference to our instance as the only
	/// argument.  A method "fixup" will be called, passing in the element and expecting
	/// a byte[] array as output.
	/// </remarks>
	[Description("A Python or Ruby fixup.")]
	[Fixup("ScriptFixup", true)]
	[Parameter("class", typeof(string), "Reference to data element")]
	[Parameter("ref", typeof(DataElement), "Reference to data element")]
	[Serializable]
	public class ScriptFixup : Fixup
	{
		[NonSerialized]
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		[NonSerialized]
		dynamic _pythonFixup = null;

		public ScriptFixup(DataElement parent, Dictionary<string, Variant> args)
			: base(parent, args, "ref")
		{
			try
			{
				Dictionary<string, object> state = new Dictionary<string, object>();
				state["fixupSelf"] = this;

				_pythonFixup = Scripting.EvalExpression(
					string.Format("{0}(fixupSelf)",
					(string)args["class"]),
					state);

				if (_pythonFixup == null)
					throw new PeachException("Error, unable to create an instance of the \"" + (string)args["class"] + "\" script class.");

				logger.Debug("ScriptFixup(): _pythonFixup != null");
			}
			catch (Exception ex)
			{
				logger.Debug("class: " + (string)args["class"]);
				logger.Error(ex.Message);
				throw;
			}
		}

		protected override Variant fixupImpl()
		{
			if (_pythonFixup == null)
			{
				Dictionary<string, object> state = new Dictionary<string, object>();
				state["fixupSelf"] = this;

				_pythonFixup = Scripting.EvalExpression(
					string.Format("{0}(fixupSelf)",
					(string)args["class"]),
					state);
			}

			var from = elements["ref"];

			logger.Debug("fixupImpl(): ref: " + from.GetHashCode().ToString());

			object data = _pythonFixup.fixup(from);

			if (data is byte[])
			{
				return new Variant((byte[])data);
			}
			else if (data is string)
			{
				return new Variant((string)data);
			}
			else if (data is int)
			{
				return new Variant((int)data);
			}

			logger.Error("Error, unknown type [" + data.GetType().ToString() + "].");
			throw new ApplicationException("Error, unknown type [" + data.GetType().ToString() + "].");
		}
	}
}

// end
