
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
using System.Reflection;

using Peach.Core.MutationStrategies;
using Peach.Core.Dom;

using NLog;

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
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public delegate void MutationEventHandler(string elementName, string mutatorName);

		public static event MutationEventHandler Mutating;

		protected RunContext _context;
		protected Engine _engine;
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

		public virtual RunContext Context
		{
			get { return _context; }
			set { _context = value; }
		}

		public virtual Engine Engine
		{
			get { return _engine; }
			set { _engine = value; }
		}

		public abstract bool IsDeterministic
		{
			get;
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
			get
			{
				return _context.config.randomSeed;
			}
		}

		protected string[] GetAllDataModelNames(Dom.Action action)
		{

			if(action.dataModel != null)
				return new string[] { GetDataModelName(action) };

			if(action.parameters.Count == 0)
				throw new ArgumentException();

			var names = new List<string>();

			foreach (var parameter in action.parameters)
				names.Add(GetDataModelName(action, parameter));

			return names.ToArray();
		}

		protected string GetDataModelName(Dom.Action action)
		{
			if (action.dataModel == null)
			{
				logger.Error("Error, in GetDataModelName, action.dataModel is null for action \""+action.name+"\".");
				throw new ArgumentException();
			}

			StringBuilder sb = new StringBuilder();

			sb.Append("Run ");
			sb.Append(action.parent.runCount);
			sb.Append('.');
			sb.Append(action.parent.name);
			sb.Append('.');
			sb.Append(action.name);
			sb.Append('.');
			sb.Append(action.dataModel.name);

			return sb.ToString();
		}

		protected string GetDataModelName(Dom.Action action, ActionParameter param)
		{
			if (param.dataModel == null)
				throw new ArgumentException();

			StringBuilder sb = new StringBuilder();

			sb.Append("Run ");
			sb.Append(action.parent.runCount);
			sb.Append('.');
			sb.Append(action.parent.name);
			sb.Append('.');
			sb.Append(action.name);
			sb.Append('.');
			sb.Append(action.parameters.IndexOf(param));
			sb.Append('.');
			sb.Append(param.dataModel.name);

			return sb.ToString();
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
		/// Allows mutation strategy to affect state change.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public virtual State MutateChangingState(State state)
		{
			return state;
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

		/// <summary>
		/// Call supportedDataElement method on Mutator type.
		/// </summary>
		/// <param name="mutator"></param>
		/// <param name="elem"></param>
		/// <returns>Returns true or false</returns>
		protected bool SupportedState(Type mutator, State elem)
		{
			MethodInfo supportedState = mutator.GetMethod("supportedState");
			if (supportedState == null)
				return false;

			object[] args = new object[1];
			args[0] = elem;

			return (bool)supportedState.Invoke(null, args);
		}

		protected Mutator GetMutatorInstance(Type t, DataElement obj)
		{
			try
			{
				Mutator mutator = (Mutator)t.GetConstructor(new Type[] { typeof(DataElement) }).Invoke(new object[] { obj });
				mutator.context = this;
				return mutator;
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;
				else
					throw;
			}
		}

		protected Mutator GetMutatorInstance(Type t, State obj)
		{
			try
			{
				Mutator mutator = (Mutator)t.GetConstructor(new Type[] { typeof(State) }).Invoke(new object[] { obj });
				mutator.context = this;
				return mutator;
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;
				else
					throw;
			}
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

			Func<Type, MutatorAttribute, bool> predicate = delegate(Type type, MutatorAttribute attr)
			{
				if (_context.test.includedMutators.Count > 0 && !_context.test.includedMutators.Contains(type.Name))
					return false;

				if (_context.test.excludedMutators.Count > 0 && _context.test.excludedMutators.Contains(type.Name))
					return false;

				return true;
			};
			var ret = new List<Type>(ClassLoader.GetAllTypesByAttribute(predicate));

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

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class MutationStrategyAttribute : PluginAttribute
	{
		public MutationStrategyAttribute(string name, bool isDefault = false)
			: base(typeof(MutationStrategy), name, isDefault)
		{
		}
	}
}

// end
