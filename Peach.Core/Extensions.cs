using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Peach.Core
{
	public static class IpAddressExtensions
	{
		public static bool IsMulticast(this System.Net.IPAddress ip)
		{
			// IPv6 -> 1st byte is 0xff
			// IPv4 -> 1st byte is 0xE0 -> 0xEF

			byte[] buf = ip.GetAddressBytes();

			if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
				return (buf[0] & 0xe0) == 0xe0;
			else
				return (buf[0] == 0xff);
		}
	}

	public static class ByteArrayExtensions
	{
		public static bool IsSame(this byte[] buffer, byte[] other)
		{
			return buffer.IsSame(other, 0, other.Length);
		}

		public static bool IsSame(this byte[] buffer, byte[] other, int offset, int count)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || (offset + count) > other.Length)
				throw new ArgumentOutOfRangeException("count");

			if (buffer.Length != count)
				return false;

			for (int i = 0; i < buffer.Length; ++i)
			{
				if (buffer[i] != other[offset + i])
					return false;
			}

			return true;
		}
	}

	public static class XmlExtensions
	{
		/// <summary>
		/// Get attribute from XmlNode object.
		/// </summary>
		/// <param name="node">XmlNode to get attribute from</param>
		/// <param name="name">Name of attribute</param>
		/// <returns>Returns innerText or null.</returns>
		public static string getAttribute(this XmlNode node, string name)
		{
			XmlAttribute attr = node.Attributes.GetNamedItem(name) as XmlAttribute;

			if (attr != null)
				return attr.InnerText;
			else
				return null;
		}

		/// <summary>
		/// Check to see if XmlNode has specific attribute.
		/// </summary>
		/// <param name="node">XmlNode to check</param>
		/// <param name="name">Name of attribute</param>
		/// <returns>Returns boolean true or false.</returns>
		public static bool hasAttribute(this XmlNode node, string name)
		{
			string ret = node.getAttribute(name);
			return ret != null;
		}

		/// <summary>
		/// Get attribute from XmlNode object.
		/// </summary>
		/// <param name="node">XmlNode to get attribute from</param>
		/// <param name="name">Name of attribute</param>
		/// <param name="defaultValue">Default value if attribute is missing</param>
		/// <returns>Returns true/false or default value</returns>
		public static bool getAttributeBool(this XmlNode node, string name, bool defaultValue)
		{
			string value = node.getAttribute(name);
			if (value == null)
				return defaultValue;

			switch (value.ToLower())
			{
				case "1":
				case "true":
					return true;
				case "0":
				case "false":
					return false;
				default:
					throw new PeachException("Error, " + name + " has unknown value, should be boolean.");
			}
		}

		/// <summary>
		/// Set attribute on XmlNode object.
		/// </summary>
		/// <param name="node">XmlNode to set attribute on</param>
		/// <param name="name">Name of attribute</param>
		/// <param name="value">Value of attribute</param>
		public static void AppendAttribute(this XmlNode node, string name, string value)
		{
			if (!String.IsNullOrEmpty(value))
				((XmlElement)node).SetAttribute(name, value);
		}
	}
}
