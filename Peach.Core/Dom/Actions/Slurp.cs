using System;
using System.Xml;
using System.Collections.Generic;
using Peach.Core.Dom.XPath;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Peach.Core.Dom.Actions
{
	[Action("Slurp")]
	public class Slurp : Action
	{
		/// <summary>
		/// xpath for selecting set targets during slurp.
		/// </summary>
		/// <remarks>
		/// Can return multiple elements.  All returned elements
		/// will be updated with a new value.
		/// </remarks>
		[XmlAttribute]
		[DefaultValue(null)]
		public string setXpath { get; set; }

		/// <summary>
		/// xpath for selecting value during slurp
		/// </summary>
		/// <remarks>
		/// Must return a single element.
		/// </remarks>
		[XmlAttribute]
		[DefaultValue(null)]
		public string valueXpath { get; set; }

		protected override void OnRun(Publisher publisher, RunContext context)
		{
			var resolver = new PeachXmlNamespaceResolver();
			var navi = new PeachXPathNavigator(context.dom);
			var iter = navi.Select(valueXpath, resolver);
			if (!iter.MoveNext())
				throw new SoftException("Error, slurp valueXpath returned no values. [" + valueXpath + "]");

			var valueElement = ((PeachXPathNavigator)iter.Current).currentNode as DataElement;
			if (valueElement == null)
				throw new SoftException("Error, slurp valueXpath did not return a Data Element. [" + valueXpath + "]");

			if (iter.MoveNext())
				throw new SoftException("Error, slurp valueXpath returned multiple values. [" + valueXpath + "]");

			iter = navi.Select(setXpath, resolver);

			if (!iter.MoveNext())
				throw new SoftException("Error, slurp setXpath returned no values. [" + setXpath + "]");

			do
			{
				var setElement = ((PeachXPathNavigator)iter.Current).currentNode as DataElement;
				if (setElement == null)
					throw new PeachException("Error, slurp setXpath did not return a Data Element. [" + valueXpath + "]");

				logger.Debug("Slurp, setting " + setElement.fullName + " from " + valueElement.fullName);
				setElement.DefaultValue = valueElement.DefaultValue;
			}
			while (iter.MoveNext());
		}

		class PeachXmlNamespaceResolver : IXmlNamespaceResolver
		{
			public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
			{
				return new Dictionary<string, string>();
			}

			public string LookupNamespace(string prefix)
			{
				return prefix;
			}

			public string LookupPrefix(string namespaceName)
			{
				return namespaceName;
			}
		}
	}
}
