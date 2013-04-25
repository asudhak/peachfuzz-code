using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Schema;
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
	[Parameter("Url", typeof(string), "WebService URL")]
	[Parameter("Service", typeof(string), "Service name")]
	[Parameter("Wsdl", typeof(string), "Optional path or URL to WSDL for web service.", "")]
	[Parameter("ErrorOnStatusCode", typeof(bool), "Error when status code isn't 200 (defaults to true)", "true")]
	[Parameter("Timeout", typeof(int), "How long to wait in milliseconds for data/connection (default 3000)", "3000")]
	[Parameter("Throttle", typeof(int), "Time in milliseconds to wait between connections", "0")]
	public class WebServicePublisher : Publisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public string Url { get; set; }
		public string Service { get; set; }
		public string Wsdl { get; set; }
		public int Timeout { get; set; }
		public int Throttle { get; set; }
        public string StatusCode = HttpStatusCode.OK.ToString();
		public bool ErrorOnStatusCode { get; set; }
		public override string Result { get { return StatusCode;} set { StatusCode = value; } }

		protected MemoryStream _buffer = new MemoryStream();
		protected int _pos = 0;

		protected byte[] receiveBuffer = new byte[1024];

		public WebServicePublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

        protected object GetVariantValue(Variant value)
        {
            switch (value.GetVariantType())
            {
                //case Variant.VariantType.Boolean:
                //    return (bool)value;
                case Variant.VariantType.Int:
                    return (int)value;
                case Variant.VariantType.Long:
                    return (long)value;
                case Variant.VariantType.String:
                    return (string)value;
                case Variant.VariantType.ULong:
                    return (ulong)value;
                case Variant.VariantType.BitStream:
                    return (byte[])value;
            }

            return null;
        }

		protected override Variant OnCall(string method, List<Dom.ActionParameter> args)
		{
			object [] parameters = new object[args.Count];
			int count = 0;
			foreach(var arg in args)
			{
				try
				{
					parameters[count] = GetVariantValue(arg.dataModel[0].InternalValue);
				}
				catch(Exception ex)
				{
					logger.Debug("OnCall: Warning, unable to get value for parameter #" + count + ".  Setting parameter to null.  Exception: " + ex.ToString());
					parameters[count] = null;
				}

				count++;
			}

            try
            {
                WebServiceInvoker invoker = new WebServiceInvoker(new Uri(this.Url));
                object ret = invoker.InvokeMethod<object>(this.Service, method, parameters);

                StatusCode = HttpStatusCode.OK.ToString();

				if (ret == null)
					return null;

                return new Variant(ret.ToString());
            }
            catch (Exception ex)
            {
				if (ex.InnerException is ArgumentException &&
					ex.InnerException.Message.IndexOf("surrogate") != -1)
					throw new SoftException(ex.InnerException);
				if (ex.InnerException is InvalidOperationException &&
					ex.InnerException.Message.IndexOf("XML") != -1)
					throw new SoftException(ex.InnerException);

                if (!(ex.InnerException is WebException))
                    throw;

                var webEx = ex.InnerException as WebException;
                var response = webEx.Response as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    StatusCode = response.StatusCode.ToString();
					logger.Debug("OnCall: Warning: Status code was: " + StatusCode);

					if (ErrorOnStatusCode)
					{
						var sex = new SoftException(ex); // Soft or ignore?
						throw sex;
					}
					else
					{
						return null;
					}
                }

                throw;
            }
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

            var method = type.GetMethod(methodName);
            return (T)method.Invoke(obj, args);
		}

		/// <summary>
		/// Builds the web service description importer, which allows us to generate a proxy class based on the 
		/// content of the WSDL described by the XmlTextReader.
		/// </summary>
		/// <param name="webserviceUri">The WSDL content, described by XML.</param>
		/// <returns>A ServiceDescriptionImporter that can be used to create a proxy class.</returns>
		private ServiceDescriptionImporter BuildServiceDescriptionImporter(string webserviceUri)
		{
			ServiceDescriptionImporter descriptionImporter = null;

			// build an importer, that assumes the SOAP protocol, client binding, and generates properties
			descriptionImporter = new ServiceDescriptionImporter();
			//descriptionImporter.ProtocolName = "Soap12";
			descriptionImporter.ProtocolName = "Soap";
			descriptionImporter.Style = ServiceDescriptionImportStyle.Client;
			descriptionImporter.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties;

            // This is the better way rather then trying to flatten document ourselves.

            DiscoveryClientProtocol clientProtocol = new DiscoveryClientProtocol();
            clientProtocol.DiscoverAny(webserviceUri);
            clientProtocol.ResolveAll();

            clientProtocol.Documents.Values.OfType<object>()
                                   .Select(document =>
                                   {
                                       if (document is ServiceDescription)
                                           descriptionImporter.AddServiceDescription(document as ServiceDescription, string.Empty, string.Empty);
                                       else if (document is XmlSchema)
                                           descriptionImporter.Schemas.Add(document as XmlSchema);
                                       return true;
                                   })
                                   .ToList();

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
