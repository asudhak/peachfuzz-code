using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Peach.Core.Dom
{
	[Serializable]
	[DebuggerDisplay("From={FromName} Of={ofName}")]
	public class Binding
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		private DataElement of;
		private string ofName;

		public Binding(DataElement parent)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			From = parent;
			FromName = parent.name;
		}

		public virtual void Clear()
		{
			if (of != null)
			{
				of.relations.Remove(this);
				of = null;
			}
		}

		public virtual void Resolve()
		{
			if (of != null)
				return;

			if (OfName == null)
				return;

			of = From.find(OfName);

			if (of == null)
			{
				logger.Error("Error, unable to resolve relation '" + ofName + "' attached to '" + From.fullName + "'.");
			}
			else if (From.CommonParent(of) is Choice)
			{
				logger.Error("Error, relation '" + ofName + "' attached to '" + From.fullName + "' cannot share a common parent that is of type 'Choice'.");
			}
			else
			{
				of.relations.Add(this);
			}
		}

		/// <summary>
		/// The DataElement that owns the binding.
		/// </summary>
		public DataElement From { get; private set; }

		/// <summary>
		/// The name of the DataElement that owns the binding.
		/// </summary>
		public string FromName { get; private set; }

		/// <summary>
		/// The DataElement on the remote side of the binding.
		/// </summary>
		[DebuggerDisplay("{of}")]
		public DataElement Of
		{
			get
			{
				Resolve();

				return of;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				if (of != null)
					throw new NotSupportedException("Property has already been set.");

				of = value;
				ofName = value.name;

				From.Invalidate();
			}
		}

		/// <summary>
		/// The name of the DataElement on the remote side of the binding.
		/// </summary>
		public string OfName
		{
			get
			{
				return ofName;
			}
			set
			{
				if (ofName == value)
					return;

				UpdateBinding(null, value);
			}
		}

		private void UpdateBinding(DataElement elem, string name)
		{
			if (of != null)
				of.relations.Remove(this);

			// Ensure common parent is not choice
			if (elem != null && From.CommonParent(elem) is Choice)
				throw new NotSupportedException("Binding '" + name + "' attached to '" + From.fullName + "' cannot share a common parent that is of type 'Choice'.");

			of = elem;
			ofName = name;

			if (of != null)
				of.relations.Add(this);

			// We need to invalidate now that we have a new of
			From.Invalidate();
		}

		[OnCloned]
		private void OnCloned(Relation original, object context)
		{
			// DataElement.Invalidated is not serialized, so register for a re-subscribe to the event

			DataElement.CloneContext ctx = context as DataElement.CloneContext;

			if (ctx != null)
			{
				// If 'From' or 'Of' was renamed, ensure the name is correct
				FromName = ctx.UpdateRefName(original.From, original.From, FromName);
				ofName = ctx.UpdateRefName(original.From, original.of, ofName);

				// If this 'From' is the same as the original,
				// then the data element was not a child of the data element
				// that was cloned.  This binding needs to be added to the
				// original 'From' element.
				if (From == original.From)
				{
					System.Diagnostics.Debug.Assert(!original.From.relations.Contains(this));
					From.relations.Add(this);
				}

				// If this 'Of' is the same as the original,
				// then the data element was not a child of the data element
				// that was cloned.  This binding needs to be added to the
				// original 'Of' element.
				if (of != null && of == original.of)
				{
					System.Diagnostics.Debug.Assert(!original.of.relations.Contains(this));
					of.relations.Add(this);
				}
			}
		}
	}
}
