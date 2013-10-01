using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Peach.Core.Dom
{
	[Serializable]
	[DebuggerDisplay("From={FromName} Of={OfName}")]
	public class Binding
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		private DataElement of;

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
				bool removed = of.relations.Remove(this);
				System.Diagnostics.Debug.Assert(removed);
				of.Invalidated -= OfInvalidated;
				of = null;

				// When Of is lost, From needs to be invalidated
				From.Invalidate();
			}
		}

		public virtual void Resolve()
		{
			if (of == null && OfName != null)
			{
				of = From.find(OfName);

				if (of == null)
				{
					logger.Error("Error, unable to resolve relation '" + OfName + "' attached to '" + From.fullName + "'.");
				}
				else if (From.CommonParent(of) is Choice)
				{
					logger.Error("Error, relation '" + OfName + "' attached to '" + From.fullName + "' cannot share a common parent that is of type 'Choice'.");
				}
				else
				{
					of.Invalidated += new InvalidatedEventHandler(OfInvalidated);
					of.relations.Add(this);
				}
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
		/// The name of the DataElement on the remote side of the binding.
		/// </summary>
		public string OfName { get; set; }

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

				if (From.CommonParent(value) is Choice)
					throw new ArgumentException("Binding '" + value.fullName + "' attached to '" + From.fullName + "' cannot share a common parent that is of type 'Choice'.");

				Clear();

				OfName = value.name;
				of = value;
				of.Invalidated += new InvalidatedEventHandler(OfInvalidated);
				of.relations.Add(this);

				From.Invalidate();
			}
		}

		private void OfInvalidated(object sender, EventArgs e)
		{
			From.Invalidate();
		}

		[OnCloned]
		private void OnCloned(Relation original, object context)
		{
			// DataElement.Invalidated is not serialized, so register for a re-subscribe to the event
			if (of != null)
				of.Invalidated += new InvalidatedEventHandler(OfInvalidated);

			DataElement.CloneContext ctx = context as DataElement.CloneContext;

			if (ctx != null)
			{
				// If 'From' or 'Of' was renamed, ensure the name is correct
				FromName = ctx.UpdateRefName(original.From, original.From, FromName);
				OfName = ctx.UpdateRefName(original.From, original.of, OfName);

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
