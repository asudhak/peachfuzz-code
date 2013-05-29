
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)
//   Ross Salpino (rsal42@gmail.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Peach.Core.Dom;
using System.Runtime.Serialization;
using Action = Peach.Core.Dom.Action;

namespace Peach.Core.Fixups
{
	[Description("Standard sequential increment fixup.")]
	[Fixup("SequenceIncrementFixup", true)]
	[Fixup("sequence.SequenceIncrementFixup")]
	[Parameter("Offset", typeof(uint?), "Sets the per-iteration initial value to Offset * (Iteration - 1)", "")]
	[Parameter("Once", typeof(bool), "Only increment once per iteration", "false")]
	[Serializable]
	public class SequenceIncrementFixup : Fixup
	{
		public uint? Offset { get; private set; }
		public bool Once { get; private set; }

		public SequenceIncrementFixup(DataElement parent, Dictionary<string, Variant> args)
			: base(parent, args)
		{
			ParameterParser.Parse(this, args);
		}

		void StateModel_Finished(StateModel model)
		{
			Core.Dom.Action.Starting -= Action_Starting;
			Core.Dom.StateModel.Finished -= StateModel_Finished;
		}

		void Action_Starting(Action action)
		{
			var root = parent.getRoot() as DataModel;
			if (root.action != action)
				return;

			if (action.dataModel == root || action.parameters.Any(a => a.dataModel == root))
			{
				if (action.type != ActionType.Output)
					return;

				parent.Invalidate();
				Update(parent, action, Offset, Once, true);
			}
		}

		static Variant Update(DataElement elem, Action action, uint? offset, bool once, bool increment)
		{
			if (!(elem is Dom.Number) && !(elem is Dom.String && elem.Hints.ContainsKey("NumericalString")))
				throw new PeachException("SequenceIncrementFixup has non numeric parent '" + elem.fullName + "'.");

			ulong max = elem is Dom.Number ? ((Dom.Number)elem).MaxValue : ulong.MaxValue;
			ulong value = 0;
			object obj = null;

			string key = "SequenceIncrementFixup." + elem.fullName;
			Dom.Dom dom = action.parent.parent.parent as Dom.Dom;

			if (dom.context.stateStore.TryGetValue(key, out obj))
				value = (ulong)obj;

			if (dom.context.iterationStateStore.ContainsKey(key))
				increment &= !once;
			else if (offset.HasValue)
				value = (ulong)offset.Value * (dom.context.test.strategy.Iteration - 1);

			// For 2 bit number, offset is 2, 2 actions per iter:
			// Iter:  1a,1b,2a,2b,3a,3b,4a,4b,5a,5b,6a,6b
			// It-1:  0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5
			// Pre:   0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,11
			// Want:  0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2
			// Final: 1, 2, 3, 1, 2, 3, 1, 2, 3, 1, 2, 3
			if (value > max)
				value = value % max;

			if (increment)
			{
				if (++value > max)
					value -= max;

				dom.context.stateStore[key] = value;
				dom.context.iterationStateStore[key] = value;
			}

			return new Variant(value);
		}

		protected override Variant fixupImpl()
		{
			DataModel dm = parent.getRoot() as DataModel;

			if (dm == null || dm.action == null)
				return parent.DefaultValue;

			return Update(parent, dm.action, Offset, Once, false);
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			DataElement.CloneContext ctx = context.Context as DataElement.CloneContext;
			if (ctx == null)
				return;

			Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);
			Core.Dom.StateModel.Finished += new StateModelFinishedEventHandler(StateModel_Finished);
		}

	}
}

// end
