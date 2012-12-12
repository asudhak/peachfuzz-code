using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Peach.Core.IO;
using Peach.Core.Dom;
using NLog;

namespace Peach.Core.Test
{
	[TestFixture]
	class DomGeneralTests
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		[Test]
		public void Find()
		{
			DataModel dm = new DataModel("root");
			dm.Add(new Block("block1"));
			dm.Add(new Block("block2"));
			((DataElementContainer)dm[0]).Add(new Block("block1_1"));
			((DataElementContainer)dm[0]).Add(new Block("block1_2"));
			((DataElementContainer)dm[1]).Add(new Block("block2_1"));
			((DataElementContainer)dm[1]).Add(new Block("block2_2"));

			((DataElementContainer)((DataElementContainer)dm[0])[0]).Add(new Dom.String("string1_1_1"));
			((DataElementContainer)((DataElementContainer)dm[0])[0]).Add(new Dom.String("string1_1_2"));
			((DataElementContainer)((DataElementContainer)dm[0])[1]).Add(new Dom.String("string1_2_1"));
			((DataElementContainer)((DataElementContainer)dm[0])[1]).Add(new Dom.String("string1_2_2"));

			((DataElementContainer)((DataElementContainer)dm[1])[0]).Add(new Dom.String("string2_1_1"));
			((DataElementContainer)((DataElementContainer)dm[1])[0]).Add(new Dom.String("string2_1_2"));
			((DataElementContainer)((DataElementContainer)dm[1])[1]).Add(new Dom.String("string2_2_1"));
			((DataElementContainer)((DataElementContainer)dm[1])[1]).Add(new Dom.String("string2_2_2"));

			Assert.NotNull(dm.find("string1_1_1"));
			Assert.NotNull(dm.find("string1_1_1").find("string2_1_2"));
		}

		[Test]
		public void PluginAttributes()
		{
			var errors = new StringBuilder();

			// All plugins should have:
			// 1) One plugin attribute with default=true
			// 2) A description
			// 3) No duplicated parameters
			// 4) No plugin attribute names should conflict
			// 5) Plugins can only be of one type (Monitor/Publisher/Fixup)

			var pluginsByType = new Dictionary<Type, List<PluginAttribute>>();
			var pluginsByName = new Dictionary<string, KeyValuePair<PluginAttribute, Type>>();

			foreach (var kv in ClassLoader.GetAllByAttribute<Peach.Core.PluginAttribute>(null))
			{
				var attr = kv.Key;
				var type = kv.Value;

				if (!pluginsByType.ContainsKey(type))
					pluginsByType.Add(type, new List<PluginAttribute>());

				pluginsByType[type].Add(attr);

				string pluginName = string.Format("{0} '{1}'", attr.Type.Name, attr.Name);

				// Verify #4 (no name collisions)
				if (pluginsByName.ContainsKey(pluginName))
				{
					var old = pluginsByName[pluginName];

					errors.AppendLine();

					if (old.Value == kv.Value)
					{
						errors.AppendFormat("{0} declared more than once in assembly '{1}' class '{2}'.",
							pluginName, kv.Value.Assembly.Location, kv.Value.FullName);
					}
					else
					{
						errors.AppendFormat("{0} declared in assembly '{1}' class '{2}' and in assembly {3} and class '{4}'.",
							pluginName, kv.Value.Assembly.Location, kv.Value.FullName, old.Value.Assembly.Location, old.Value.FullName);
					}
				}
				else
				{
					pluginsByName.Add(pluginName, kv);
				}
			}

			foreach (var kv in pluginsByType)
			{
				var type = kv.Key;
				var attrs = kv.Value;

				// Verify #4 (eEnsure all plugin attributes are of the same type)
				var pluginTypes = attrs.Select(a => a.Type.Name).Distinct().ToList();
				if (pluginTypes.Count != 1)
				{
					errors.AppendLine();
					errors.AppendFormat("Plugin declared in assembly '{1}' class '{2}' has multiple types: '{3}'",
						attrs[0].Type.Name, type.Assembly.Location, type.FullName, string.Join("', '", pluginTypes));
				}

				// Verify #1 (ensure there is a single default)
				var defaults = attrs.Where(a => a.IsDefault).Select(a => a.Name).ToList();
				if (defaults.Count == 0)
				{
					errors.AppendLine();
					errors.AppendFormat("{0} declared in assembly '{1}' class '{2}' has no default name.",
						attrs[0].Type.Name, type.Assembly.Location, type.FullName);
				}
				else if (defaults.Count != 1)
				{
					errors.AppendLine();
					errors.AppendFormat("{0} declared in assembly '{1}' class '{2}' has multiple defaults: '{3}'",
						attrs[0].Type.Name, type.Assembly.Location, type.FullName, string.Join("', '", defaults));
				}

				// Verify #2 (ensure there is a description)
				//var desc = type.GetAttributes<DescriptionAttribute>(null).FirstOrDefault();
				//if (desc == null)
				//{
				//    errors.AppendLine();
				//    errors.AppendFormat("{0} declared in assembly '{1}' class '{2}' has no description.",
				//        attrs[0].Type.Name, type.Assembly.Location, type.FullName);
				//}

				// Verify #3 (all the parameters must be unique in name)
				var paramAttrs = type.GetAttributes<ParameterAttribute>(null).Select(a => a.name).ToList();
				var dupes = paramAttrs.GroupBy(a => a).SelectMany(g => g.Skip(1)).Distinct().ToList();
				if (dupes.Count != 0)
				{
					errors.AppendLine();
					errors.AppendFormat("{0} declared in assembly '{1}' class '{2}' has duplicate parameters: '{3}'",
						attrs[0].Type.Name, type.Assembly.Location, type.FullName, string.Join("', '", dupes));
				}

			}

			string msg = errors.ToString();

			if (!string.IsNullOrEmpty(msg))
			{
				logger.Debug(msg);
				Assert.Null(msg);
			}
		}

		[Test]
		public void DataElementAttributes()
		{
			var errors = new StringBuilder();

			var deByType = new Dictionary<Type, DataElementAttribute>();
			var deByName = new Dictionary<string, KeyValuePair<DataElementAttribute, Type>>();

			foreach (var kv in ClassLoader.GetAllByAttribute<DataElementAttribute>(null))
			{
				var attr = kv.Key;
				var type = kv.Value;

				// Verify only 1 DataElement attribute per type
				if (deByType.ContainsKey(type))
				{
					var old = deByType[type];

					errors.AppendLine();
					errors.AppendFormat("DataElement in assembly '{0}' class '{1}' declared both '{2}' and '{3}.",
						type.Assembly.Location, type.FullName, attr.elementName, old.elementName);
				}
				else
				{
					deByType.Add(type, attr);
				}

				// Verify no elementName collissions
				if (deByName.ContainsKey(attr.elementName))
				{
					var old = deByName[attr.elementName];

					errors.AppendLine();
					errors.AppendFormat("DataElement '{0}' declared in assembly '{1}' class '{2}' and in assembly {3} and class '{4}'.",
						attr.elementName, kv.Value.Assembly.Location, kv.Value.FullName, old.Value.Assembly.Location, old.Value.FullName);
				}
				else
				{
					deByName.Add(attr.elementName, kv);
				}

				var paramAttrs = type.GetAttributes<ParameterAttribute>(null).Select(a => a.name).ToList();
				var dupes = paramAttrs.GroupBy(a => a).SelectMany(g => g.Skip(1)).Distinct().ToList();
				if (dupes.Count != 0)
				{
					errors.AppendLine();
					errors.AppendFormat("DataElement '{0}' declared in assembly '{1}' class '{2}' has duplicate parameters: '{3}'",
						attr.elementName, type.Assembly.Location, type.FullName, string.Join("', '", dupes));
				}
			}

			string msg = errors.ToString();

			if (!string.IsNullOrEmpty(msg))
			{
				logger.Debug(msg);
				Assert.Null(msg);
			}
		}
	}
}
