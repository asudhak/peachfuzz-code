
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.MutationStrategies;
using Peach.Core.Dom;
using System.Reflection;

namespace Peach.Core
{
	/// <summary>
	/// Mutation strategies drive the fuzzing
	/// that Peach performs.  Creating a fuzzing
	/// strategy allows one to fully control which elements
	/// are mutated, by which mutators, and when.
	/// </summary>
	[Serializable]
	public abstract class MutationStrategy
	{
		protected RunContext _context;
		protected Engine _engine;

		public MutationStrategy(Dictionary<string, string> args)
		{
		}

		public virtual void Initialize(RunContext context, Engine engine)
		{
			_context = context;
			_engine = engine;
		}

		public bool isFinite
		{
			get { return false; }
		}

		public abstract uint count
		{
			get;
		}

		public Random random
		{
			get { return _context.random; }
		}

		public abstract Mutator currentMutator();

		public abstract void next();

		/// <summary>
		/// Call supportedDataElement method on Mutator type.
		/// </summary>
		/// <param name="mutator"></param>
		/// <param name="elem"></param>
		/// <returns>Returns true or false</returns>
		protected bool SupportedDataElement(Type mutator, DataElement elem)
		{
			MethodInfo supportedDataElement = mutator.GetMethod("supportedDataElement");

			object[] args = new object[1];
			args[0] = elem;

			return (bool)supportedDataElement.Invoke(null, args);
		}

		protected Mutator GetMutatorInstance(Type t, DataElement obj)
		{
            Mutator mutator = (Mutator)t.GetConstructor(new Type[] { typeof(DataElement) }).Invoke(new object[] { obj });
            mutator.context = this;

            return mutator;
		}

		protected void RecursevlyGetElements(DataElementContainer d, List<DataElement> all)
		{
			foreach (DataElement elem in d)
			{
				all.Add(elem);

				if (elem is DataElementContainer)
				{
					RecursevlyGetElements(elem as DataElementContainer, all);
				}
			}
		}

	}

	public class DefaultMutationStrategyAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
	public class MutationStrategyAttribute : Attribute
	{
		public string name = null;
		public MutationStrategyAttribute(string name)
		{
			this.name = name;
		}
	}
}

// end
