using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Discovery;

using Peach.Core;
using NLog;

namespace Peach.Core.Publishers
{
	[Publisher("WebService", true)]
	[ParameterAttribute("Url", typeof(string), "WebService URL", true)]
	[ParameterAttribute("Service", typeof(string), "Service name", true)]
	[ParameterAttribute("Wsdl", typeof(string), "Optional path or URL to WSDL for web service.", false)]
	[ParameterAttribute("Timeout", typeof(int), "How long to wait in milliseconds for data/connection (default 3 seconds)", false)]
	[ParameterAttribute("Throttle", typeof(int), "Time in milliseconds to wait between connections", false)]
	public class WebServicePublisher : Publisher
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		protected Uri _url = null;
		protected string _service = null;
		protected string _wsdl = null;
		protected int _timeout = 3 * 1000;
		protected int _throttle = 0;
		protected MemoryStream _buffer = new MemoryStream();
		protected int _pos = 0;

		protected byte[] receiveBuffer = new byte[1024];

		public WebServicePublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			_url = new Uri((string)args["Url"]);
			_service = (string)args["Service"];

			if(args.ContainsKey("Wsdl"))
				_wsdl = (string)args["Wsdl"];

			if (args.ContainsKey("Timeout"))
				_timeout = (int)args["Timeout"];
			if (args.ContainsKey("Throttle"))
				_throttle = (int)args["Throttle"];
		}

		public int Timeout
		{
			get { return _timeout; }
			set { _timeout = value; }
		}

		public int Throttle
		{
			get { return _throttle; }
			set { _throttle = value; }
		}

		public override Variant call(Dom.Action action, string method, List<Dom.ActionParameter> args)
		{
			object [] parameters = new object[args.Count];
			int count = 0;
			foreach(var arg in args)
			{
				parameters[count] = arg.data;
				count++;
			}

			WebServiceInvoker invoker = new WebServiceInvoker(_url);
			object ret = invoker.InvokeMethod<object>(_service, method, parameters);

			return new Variant(ret.ToString());
		}
	}

	class WebServiceInvoker
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		Dictionary<string, Type> availableTypes;

		/// <summary>
		/// Text description of the available services within this web service.
		/// </summary>
		public List<string> AvailableServices
		{
			get { return this.services; }
		}

		string GetUrlContent(Uri uri)
		{
			logger.Debug("GetUrlContent: " + uri.ToString());

			using (WebClient client = new WebClient())
			using (Stream stream = client.OpenRead(uri))
			using (MemoryStream sin = new MemoryStream())
			{
				stream.CopyTo(sin);
				sin.Position = 0;

				return UTF8Encoding.UTF8.GetString(sin.ToArray());
			}
		}

		public string FlattenWsdl(Uri wsdlUrl, List<string> seenImports = null)
		{
			return FlattenWsdl(GetUrlContent(wsdlUrl), seenImports);
		}

		public string FlattenWsdl(string wsdl, 
			List<string> seenImports = null, 
			XmlDocument topLevelDoc = null, 
			XmlNode schemaPlaceholder = null)
		{
			logger.Debug("FlattenWsdl()");

			if (seenImports == null)
				seenImports = new List<string>();

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(wsdl);

			if (topLevelDoc == null)
				topLevelDoc = doc;

			foreach (XmlNode import in doc.SelectNodes("//*[@schemaLocation]"))
			{
				string ns = import.Attributes["namespace"].Value;
				string url = import.Attributes["schemaLocation"].Value;
				
				if (schemaPlaceholder == null)
					schemaPlaceholder = import.ParentNode;

				if (seenImports.Contains(ns))
				{
					import.ParentNode.RemoveChild(import);
					continue;
				}

				seenImports.Add(ns);

				logger.Debug("FlattenWsdl: NS: " + ns + " Url: " + url);

				string importXml = GetUrlContent(new Uri(url));
				importXml = FlattenWsdl(importXml, seenImports);

				XmlDocument importDoc = new XmlDocument();
				importDoc.LoadXml(importXml);
				XmlNode importNode = null;

				foreach (XmlNode child in importDoc.ChildNodes)
				{
					if (child.NodeType == XmlNodeType.Element)
					{
						importNode = doc.ImportNode(child, true);

						logger.Debug("FlattenWsdl: Inserting node: " + importNode.Name + " - " + importNode.NodeType.ToString());
						logger.Debug("FlattenWsdl: Parent of insert: " + import.ParentNode.Name);

						import.ParentNode.ReplaceChild(importNode, import);

						//foreach (XmlNode subChild in child.ChildNodes)
						//{
						//    importNode = doc.ImportNode(subChild, true);

						//    logger.Debug("FlattenWsdl: Inserting node: " + importNode.Name + " - " + importNode.NodeType.ToString());
						//    logger.Debug("FlattenWsdl: Parent of insert: " + import.ParentNode.Name);
							
						//    import.ParentNode.InsertBefore(importNode, import);
						//}

						break;
					}
				}

				if (importNode == null)
					throw new PeachException("Error, while trying to flatten the WSDL definition we were unable to import '" + url + "'.");

				//import.ParentNode.RemoveChild(import);
			}

			return doc.InnerXml;
		}

		/// <summary>
		/// Creates the service invoker using the specified web service.
		/// </summary>
		/// <param name="webServiceUri"></param>
		public WebServiceInvoker(Uri webServiceUri)
		{
			this.services = new List<string>(); // available services
			this.availableTypes = new Dictionary<string, Type>(); // available types

			// create an assembly from the web service description
			this.webServiceAssembly = BuildAssemblyFromWSDL(webServiceUri);

			// see what service types are available
			Type[] types = this.webServiceAssembly.GetExportedTypes();

			// and save them
			foreach (Type type in types)
			{
				services.Add(type.FullName);
				availableTypes.Add(type.FullName, type);
			}
		}

		/// <summary>
		/// Gets a list of all methods available for the specified service.
		/// </summary>
		/// <param name="serviceName"></param>
		/// <returns></returns>
		public List<string> EnumerateServiceMethods(string serviceName)
		{
			List<string> methods = new List<string>();

			if (!this.availableTypes.ContainsKey(serviceName))
				throw new Exception("Service Not Available");
			else
			{
				Type type = this.availableTypes[serviceName];

				// only find methods of this object type (the one we generated)
				// we don't want inherited members (this type inherited from SoapHttpClientProtocol)
				foreach (MethodInfo minfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
					methods.Add(minfo.Name);

				return methods;
			}
		}

		/// <summary>
		/// Invokes the specified method of the named service.
		/// </summary>
		/// <typeparam name="T">The expected return type.</typeparam>
		/// <param name="serviceName">The name of the service to use.</param>
		/// <param name="methodName">The name of the method to call.</param>
		/// <param name="args">The arguments to the method.</param>
		/// <returns>The return value from the web service method.</returns>
		public T InvokeMethod<T>(string serviceName, string methodName, params object[] args)
		{
			// create an instance of the specified service
			// and invoke the method
			object obj = this.webServiceAssembly.CreateInstance(serviceName);

			Type type = obj.GetType();

			return (T)type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, obj, args);
		}

		/// <summary>
		/// Builds the web service description importer, which allows us to generate a proxy class based on the 
		/// content of the WSDL described by the XmlTextReader.
		/// </summary>
		/// <param name="xmlreader">The WSDL content, described by XML.</param>
		/// <returns>A ServiceDescriptionImporter that can be used to create a proxy class.</returns>
		private ServiceDescriptionImporter BuildServiceDescriptionImporter(string webserviceUri)
		{
			ServiceDescriptionImporter descriptionImporter = null;
			ServiceDescription serviceDescription = null;

			// Figure out how to handle Proxy if we need to.
			//WebClient client = new WebClient { Proxy = new WebProxy(host, port) };
			using (WebClient client = new WebClient())
			{
				using (Stream stream = client.OpenRead(webserviceUri))
				{
					string xml;

					using (var sin = new MemoryStream())
					{
						stream.CopyTo(sin);
						sin.Position = 0;
						xml = UTF8Encoding.UTF8.GetString(sin.ToArray());
					}

					xml = FlattenWsdl(xml);
					File.WriteAllText(@"c:\temp\flat2.xml", xml);
					//xml = File.ReadAllText(@"c:\temp\flat.xml");

					using (var sout = new MemoryStream(UTF8Encoding.UTF8.GetBytes(xml)))
					{
						using (XmlReader xmlreader = XmlReader.Create(sout))
						{
							serviceDescription = ServiceDescription.Read(xmlreader);
						}
					}
				}
			}

			// build an importer, that assumes the SOAP protocol, client binding, and generates properties
			descriptionImporter = new ServiceDescriptionImporter();
			//descriptionImporter.ProtocolName = "Soap12";
			descriptionImporter.ProtocolName = "Soap12";
			descriptionImporter.AddServiceDescription(serviceDescription, null, null);
			descriptionImporter.Style = ServiceDescriptionImportStyle.Client;
			descriptionImporter.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties;

			return descriptionImporter;
		}

		/// <summary>
		/// Compiles an assembly from the proxy class provided by the ServiceDescriptionImporter.
		/// </summary>
		/// <param name="descriptionImporter"></param>
		/// <returns>An assembly that can be used to execute the web service methods.</returns>
		private Assembly CompileAssembly(ServiceDescriptionImporter descriptionImporter)
		{
			// a namespace and compile unit are needed by importer
			CodeNamespace codeNamespace = new CodeNamespace();
			CodeCompileUnit codeUnit = new CodeCompileUnit();

			codeUnit.Namespaces.Add(codeNamespace);

			ServiceDescriptionImportWarnings importWarnings = descriptionImporter.Import(codeNamespace, codeUnit);

			if (importWarnings == 0) // no warnings
			{
				// create a c# compiler
				CodeDomProvider compiler = CodeDomProvider.CreateProvider("CSharp");

				// include the assembly references needed to compile
				string[] references = new string[2] { "System.Web.Services.dll", "System.Xml.dll" };

				CompilerParameters parameters = new CompilerParameters(references);

				// compile into assembly
				CompilerResults results = compiler.CompileAssemblyFromDom(parameters, codeUnit);

				foreach (CompilerError oops in results.Errors)
				{
					// trap these errors and make them available to exception object
					throw new Exception("Compilation Error Creating Assembly");
				}

				// all done....
				return results.CompiledAssembly;
			}
			else
			{
				//// warnings issued from importers, something wrong with WSDL
				throw new Exception("Invalid WSDL: "+importWarnings.ToString());
			}
		}

		/// <summary>
		/// Builds an assembly from a web service description.
		/// The assembly can be used to execute the web service methods.
		/// </summary>
		/// <param name="webServiceUri">Location of WSDL.</param>
		/// <returns>A web service assembly.</returns>
		private Assembly BuildAssemblyFromWSDL(Uri webServiceUri)
		{
			if (String.IsNullOrEmpty(webServiceUri.ToString()))
				throw new Exception("Web Service Not Found");

			ServiceDescriptionImporter descriptionImporter = BuildServiceDescriptionImporter(webServiceUri.ToString() + "?wsdl");

			return CompileAssembly(descriptionImporter);
		}

		private Assembly webServiceAssembly;
		private List<string> services;
	}
}
