
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

namespace Peach.Core.Fixups
{
	[Description("Standard sequential random fixup.")]
	[Fixup("SequenceRandomFixup", true)]
	[Fixup("sequence.SequenceRandomFixup")]
	[Serializable]
	public class SequenceRandomFixup : Fixup
	{
		public SequenceRandomFixup(DataElement parent, Dictionary<string, Variant> args)
			: base(parent, args)
		{
			Core.Dom.StateModel.Starting += new StateModelStartingEventHandler(StateModel_Starting);
		}

		void StateModel_Starting(StateModel model)
		{
			Core.Dom.StateModel.Starting -= new StateModelStartingEventHandler(StateModel_Starting);

			Dom.Dom dom = model.parent as Dom.Dom;
			dom.context.engine.IterationStarting += new Engine.IterationStartingEventHandler(Engine_IterationStarting);
			dom.context.engine.TestFinished += new Engine.TestFinishedEventHandler(Engine_TestFinished);

			Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);

			Engine_IterationStarting(dom.context, dom.context.test.strategy.Iteration, null);
		}

		void Engine_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			Random rng = new Random(context.config.randomSeed + currentIteration);
			context.iterationStateStore["SequenceRandomFixup"] = rng;
		}

		void Engine_TestFinished(RunContext context)
		{
			Core.Dom.Action.Starting -= Action_Starting;
			context.engine.TestFinished -= Engine_TestFinished;
			context.engine.IterationStarting -= Engine_IterationStarting;
		}

		void Action_Starting(Dom.Action action)
		{
			if (action.type != ActionType.Output)
				return;

			var elem = action.dataModel.find(parent.fullName);
			if (elem != null)
			{
				elem.Invalidate();
				GetRandom(elem, action, true);
			}
		}

		static Variant GetRandom(DataElement elem, Dom.Action action, bool update)
		{
			Dom.Number num = elem as Dom.Number;
			if (num == null && !(elem is Dom.String && elem.Hints.ContainsKey("NumericalString")))
				throw new PeachException("SequenceRandomFixup has non numeric parent '" + elem.fullName + "'.");

			string key = "SequenceRandomFixup." + elem.fullName;

			Dom.Dom dom = action.parent.parent.parent as Dom.Dom;
			object obj;
			if (!dom.context.iterationStateStore.TryGetValue(key, out obj))
				obj = elem.DefaultValue;

			Variant var = obj as Variant;
			System.Diagnostics.Debug.Assert(var != null);

			if (!update)
				return var;

			Random rng = (Random)dom.context.iterationStateStore["SequenceRandomFixup"];
			
			dynamic random;

			if (num != null)
			{
				if (num.Signed)
				{
					if (num.MaxValue == long.MaxValue)
						random = rng.NextInt64();
					else
						random = rng.Next((long)num.MinValue, (long)num.MaxValue + 1);
				}
				else
				{
					if (num.MaxValue == ulong.MaxValue)
						random = rng.NextUInt64();
					else
						random = rng.Next((ulong)num.MinValue, (ulong)num.MaxValue + 1);
				}
			}
			else
			{
				random = rng.NextInt32();
			}

			var = new Variant(random);
			dom.context.iterationStateStore[key] = var;
			return var;
		}

		protected override Variant fixupImpl()
		{
			DataModel dm = parent.getRoot() as DataModel;

			if (dm == null || dm.action == null)
				return parent.DefaultValue;

			return GetRandom(parent, dm.action, false);
		}
	}
}

// end
