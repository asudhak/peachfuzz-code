
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
using Peach.Core.Dom;
using System.Runtime.Serialization;

namespace Peach.Core.Fixups
{
	[Description("Standard sequential increment fixup.")]
	[Fixup("SequenceIncrementFixup", true)]
	[Fixup("sequence.SequenceIncrementFixup")]
	[Serializable]
	public class SequenceIncrementFixup : Fixup
	{
		public SequenceIncrementFixup(DataElement parent, Dictionary<string, Variant> args)
			: base(parent, args)
		{
			Core.Dom.StateModel.Starting += new StateModelStartingEventHandler(StateModel_Starting);
		}

		void StateModel_Starting(StateModel model)
		{
			Core.Dom.StateModel.Starting -= new StateModelStartingEventHandler(StateModel_Starting);

			Dom.Dom dom = model.parent as Dom.Dom;
			dom.context.engine.TestFinished += new Engine.TestFinishedEventHandler(Engine_TestFinished);

			Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);
		}

		void Engine_TestFinished(RunContext context)
		{
			Core.Dom.Action.Starting -= Action_Starting;
			context.engine.TestFinished -= Engine_TestFinished;
		}

		void Action_Starting(Dom.Action action)
		{
			if (action.type != ActionType.Output)
				return;

			var elem = action.dataModel.find(parent.fullName);
			if (elem != null)
			{
				elem.Invalidate();
				IncrementBy(elem, action, 1);
			}
		}

		static Variant IncrementBy(Dom.DataElement elem, Dom.Action action, uint value)
		{
			string key = "SequenceIncrementFixup." + elem.fullName;

			Dom.Dom dom = action.parent.parent.parent as Dom.Dom;
			object obj;
			bool cached = dom.context.iterationStateStore.TryGetValue(key, out obj);
			if (!cached)
				obj = elem.DefaultValue;

			Variant var = obj as Variant;
			System.Diagnostics.Debug.Assert(var != null);

			if (cached && value == 0)
				return var;

			dynamic num;

			if (elem is Dom.String && elem.Hints.ContainsKey("NumericalString"))
			{
				num = int.Parse((string)var);
			}
			else if (elem is Dom.Number)
			{
				if (((Dom.Number)elem).Signed)
					num = (long)var;
				else
					num = (ulong)var;
			}
			else
			{
				throw new PeachException("SequenceIncrementFixup has non numeric parent '" + elem.fullName + "'.");
			}

			var = new Variant(num + value);

			if (value != 0)
				dom.context.iterationStateStore[key] = var;

			return var;
		}

		protected override Variant fixupImpl()
		{
			DataModel dm = parent.getRoot() as DataModel;

			if (dm == null || dm.action == null)
				return parent.DefaultValue;

			return IncrementBy(parent, dm.action, 0);
		}
	}
}

// end
