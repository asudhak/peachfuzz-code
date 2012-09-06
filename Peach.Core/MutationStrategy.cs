
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
		public delegate void MutationEventHandler(string elementName, string mutatorName);

		public static event MutationEventHandler Mutating;

		protected RunContext _context;
		protected Engine _engine;
		protected uint _seed;
		protected Random _random;

		public MutationStrategy(Dictionary<string, Variant> args)
		{
		}

		public virtual void Initialize(RunContext context, Engine engine)
		{
			_context = context;
			_engine = engine;
		}

		public virtual void Finalize(RunContext context, Engine engine)
		{
			_context = null;
			_engine = null;
		}

		public abstract uint Count
		{
			get;
		}

		public abstract uint Iteration
		{
			get;
			set;
		}

		public Random Random
		{
			get { return _random; }
		}

		public uint Seed
		{
			get { return _seed; }
		}

		protected void SeedRandom()
		{
			_random = new Random(Seed + Iteration);
		}

		protected void OnMutating(string elementName, string mutatorName)
		{
			if (Mutating != null)
				Mutating(elementName, mutatorName);
		}

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

		private static int CompareMutator(Type lhs, Type rhs)
		{
			return string.Compare(lhs.Name, rhs.Name, StringComparison.Ordinal);
		}

		/// <summary>
		/// Enumerate mutators valid to use in this test.
		/// </summary>
		/// <remarks>
		/// Function checks against included/exluded mutators list.
		/// </remarks>
		/// <returns></returns>
		protected IEnumerable<Type> EnumerateValidMutators()
		{
			if (_context.test == null)
				throw new ArgumentException("Error, _context.test == null");

			List<Type> ret = new List<Type>();

			// Locate all mutators
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				// Reflection of this type not supported on
				// dynamic assemblies.
				if (a.IsDynamic)
					continue;

				foreach (Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is MutatorAttribute)
						{
							if (_context.test.includedMutators != null && !_context.test.includedMutators.Contains(t.Name))
								continue;

							if (_context.test.exludedMutators != null && _context.test.exludedMutators.Contains(t.Name))
								continue;

							ret.Add(t);
						}
					}
				}
			}

			// Different environments enumerate the mutators in different orders.
			// To ensure mutation strategies run mutators in the same order everywhere
			// we have to have a well defined order.
			ret.Sort(new Comparison<Type>(CompareMutator));
			return ret;
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

	[AttributeUsage(AttributeTargets.Class, Inherited=false)]
	public class DefaultMutationStrategyAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
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
