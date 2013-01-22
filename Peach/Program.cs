
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
using System.IO;
using System.Xml;

using Peach.Options;
using Peach.Core.Dom;
using Peach.Core;
using Peach.Core.Agent;
using Peach.Core.Analyzers;

using SharpPcap;
using NLog;
using NLog.Targets;
using NLog.Config;

namespace Peach
{
	/// <summary>
	/// Command line interface for Peach 3.  Mostly backwards compatable with
	/// Peach 2.3.
	/// </summary>
	public class Program
	{
		static int Main(string[] args)
		{
			Peach.Core.AssertWriter.Register();

			return new Program(args).exitCode;
		}

		static ConsoleColor DefaultForground = ConsoleColor.DarkRed;
		static ConsoleColor DefaultBackground = ConsoleColor.DarkRed;

		public Dictionary<string, string> DefinedValues = new Dictionary<string,string>();
		public Dom dom;

		public int exitCode = 1;

		public Program(string[] args)
		{
			RunConfiguration config = new RunConfiguration();
			config.shouldStop = delegate() { return shouldStop; };
			config.debug = false;

			try
			{
				DefaultBackground = Console.BackgroundColor;
				DefaultForground = Console.ForegroundColor;

				Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

				string analyzer = null;
				bool test = false;
				string agent = null;
				string definedValuesFile = null;
				bool parseOnly = false;

				var color = Console.ForegroundColor;
				Console.Write("\n");
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.Write("[[ ");
				Console.ForegroundColor = ConsoleColor.DarkCyan;
				Console.WriteLine("Peach v3.0 DEV");
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.Write("[[ ");
				Console.ForegroundColor = ConsoleColor.DarkCyan;
				Console.WriteLine("Copyright (c) Michael Eddington\n");
				Console.ForegroundColor = color;

				if (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("\nError: Cannot use the 32bit version of Peach 3 on a 64bit operating system.");
					Console.ForegroundColor = color;
					return;
				}
				else if(Environment.Is64BitProcess && !Environment.Is64BitOperatingSystem)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("\nError: Cannot use the 64bit version of Peach 3 on a 32bit operating system.");
					Console.ForegroundColor = color;
					return;
				}

				if (args.Length == 0)
					syntax();

				var p = new OptionSet()
				{
					{ "h|?|help", v => syntax() },
					{ "analyzer=", v => analyzer = v },
					{ "debug", v => config.debug = true },
					{ "1", v => config.singleIteration = true},
					{ "range=", v => ParseRange(config, v)},
					{ "t|test", v => test = true},
					{ "c|count", v => config.countOnly = true},
					{ "skipto=", v => config.skipToIteration = Convert.ToUInt32(v)},
					{ "seed=", v => config.randomSeed = Convert.ToUInt32(v)},
					{ "p|parallel=", v => ParseParallel(config, v)},
					{ "a|agent=", v => agent = v},
					{ "D|define=", v => AddNewDefine(v) },
					{ "definedvalues=", v => definedValuesFile = v },
					{ "parseonly", v => parseOnly = true },
					{ "bob", var => bob() },
					{ "charlie", var => Charlie() },
					{ "showdevices", var => ShowDevices() },
					{ "showenv", var => ShowEnvironment() },
				};

				List<string> extra = p.Parse(args);

				if (extra.Count == 0 && agent == null && analyzer == null)
					syntax();

				Platform.LoadAssembly();

				if (definedValuesFile != null)
				{
					if (!File.Exists(definedValuesFile))
						throw new PeachException("Error, defined values file \"" + definedValuesFile + "\" does not exist.");

					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.Load(definedValuesFile);

					var root = xmlDoc.FirstChild;
					if (root.Name != "PitDefines")
					{
						root = xmlDoc.FirstChild.NextSibling;
						if (root.Name != "PitDefines")
							throw new PeachException("Error, definition file root element must be PitDefines.");
					}

					foreach (XmlNode node in root.ChildNodes)
					{
						if (hasXmlAttribute(node, "platform"))
						{
							switch (getXmlAttribute(node, "platform").ToLower())
							{
								case "osx":
									if (Platform.GetOS() != Platform.OS.OSX)
										continue;
									break;
								case "linux":
									if (Platform.GetOS() != Platform.OS.Linux)
										continue;
									break;
								case "windows":
									if (Platform.GetOS() != Platform.OS.Windows)
										continue;
									break;
								default:
									throw new PeachException("Error, unknown platform name \""+ getXmlAttribute(node, "platform") + "\" in definition file.");
							}
						}

						foreach (XmlNode defNode in node.ChildNodes)
						{
							if (defNode is XmlComment)
								continue;

							if (!hasXmlAttribute(defNode, "key") || !hasXmlAttribute(defNode, "value"))
								throw new PeachException("Error, Define elements in definition file must have both key and value attributes.");

							// Allow command line to override values in XML file.
							if (!DefinedValues.ContainsKey(getXmlAttribute(defNode, "key")))
							{
								DefinedValues[getXmlAttribute(defNode, "key")] =
									getXmlAttribute(defNode, "value");
							}
						}
					}
				}

				// Enable debugging if asked for
				if (config.debug)
				{
					var nconfig = new LoggingConfiguration();
					var consoleTarget = new ColoredConsoleTarget();
					nconfig.AddTarget("console", consoleTarget);
					consoleTarget.Layout = "${logger} ${message}";

					var rule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
					nconfig.LoggingRules.Add(rule);

					LogManager.Configuration = nconfig;
				}

				if (agent != null)
				{
					var agentType = ClassLoader.FindTypeByAttribute<AgentServerAttribute>((x, y) => y.name == agent);
					if (agentType == null)
					{
						Console.WriteLine("Error, unable to locate agent server for protocol '" + agent + "'.\n");
						return;
					}

					var agentServer = Activator.CreateInstance(agentType) as IAgentServer;

					ConsoleWatcher.WriteInfoMark();
					Console.WriteLine("Starting agent server");

					agentServer.Run(new Dictionary<string, string>());
					return;
				}

				if (analyzer != null)
				{
					var analyzerType = ClassLoader.FindTypeByAttribute<AnalyzerAttribute>((x, y) => y.Name == analyzer);
					if (analyzerType == null)
					{
						Console.WriteLine("Error, unable to locate analyzer called '" + analyzer + "'.\n");
						return;
					}

					var field = analyzerType.GetField("supportCommandLine", 
						BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
					if ((bool)field.GetValue(null) == false)
					{
						Console.WriteLine("Error, analyzer not configured to run from command line.");
						return;
					}

					var analyzerInstance = Activator.CreateInstance(analyzerType) as Analyzer;

					ConsoleWatcher.WriteInfoMark();
					Console.WriteLine("Starting Analyzer");

					analyzerInstance.asCommandLine(new Dictionary<string, string>());
					return;
				}

				if (test)
				{
					ConsoleWatcher.WriteInfoMark();
					Console.Write("Validating file [" + extra[0] + "]... ");
					Analyzer.defaultParser.asParserValidation(null, extra[0]);

					if (Type.GetType("Mono.Runtime") != null)
						Console.WriteLine("File parsed successfully, but XSD validation is not supported on the Mono runtime.");
					else
						Console.WriteLine("No Errors Found.");

					return;
				}

				Dictionary<string, object> parserArgs = new Dictionary<string, object>();
				parserArgs[PitParser.DEFINED_VALUES] = this.DefinedValues;

				Engine e = new Engine(new ConsoleWatcher());
				dom = Analyzer.defaultParser.asParser(parserArgs, extra[0]);
				config.pitFile = extra[0];

				// Used for unittests
				if (parseOnly)
					return;

				foreach (string arg in args)
					config.commandLine += arg + " ";

				if (extra.Count > 1)
				{
					if (!dom.tests.ContainsKey(extra[1]))
						throw new PeachException("Error, unable to locate test named \"" + extra[1] + "\".");

					e.startFuzzing(dom, dom.tests[extra[1]], config);
				}
				else
					e.startFuzzing(dom, config);

				exitCode = 0;
			}
			catch (SyntaxException)
			{
				// Ignore, thrown by syntax()
			}
			catch (PeachException ee)
			{
				if (config.debug)
					Console.WriteLine(ee);
				else
					Console.WriteLine(ee.Message + "\n");
			}
			finally
			{
				// HACK - Required on Mono with NLog 2.0
				LogManager.Configuration = null;

				// Reset console colors
				Console.ForegroundColor = DefaultForground;
				Console.BackgroundColor = DefaultBackground;
			}
		}

		private void ParseRange(RunConfiguration config, string v)
		{
			string[] parts = v.Split(',');
			if (parts.Length != 2)
				throw new PeachException("Invalid range: " + v);

			try
			{
				config.rangeStart = Convert.ToUInt32(parts[0]);
			}
			catch
			{
				throw new PeachException("Invalid range start iteration: " + parts[0]);
			}

			try
			{
				config.rangeStop = Convert.ToUInt32(parts[1]);
			}
			catch
			{
				throw new PeachException("Invalid range stop iteration: " + parts[1]);
			}

			if (config.parallel)
				throw new PeachException("--range is not supported when --parallel is specified");

			config.range = true;
		}

		private void ParseParallel(RunConfiguration config, string v)
		{
			string[] parts = v.Split(',');
			if (parts.Length != 2)
				throw new PeachException("Invalid parallel value: " + v);

			try
			{
				config.parallelTotal = Convert.ToUInt32(parts[0]);

				if (config.parallelTotal == 0)
					throw new ArgumentOutOfRangeException();
			}
			catch
			{
				throw new PeachException("Invalid parallel machine total: " + parts[0]);
			}

			try
			{
				config.parallelNum = Convert.ToUInt32(parts[1]);
				if (config.parallelNum == 0 || config.parallelNum > config.parallelTotal)
					throw new ArgumentOutOfRangeException();
			}
			catch
			{
				throw new PeachException("Invalid parallel machine number: " + parts[1]);
			}

			if (config.range)
				throw new PeachException("--parallel is not supported when --range is specified");

			config.parallel = true;
		}

		private static bool shouldStop = false;
		
		private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			Console.WriteLine();
			Console.WriteLine(" --- Ctrl+C Detected --- ");
		
			shouldStop = true;
			e.Cancel = true;
		}

		public void AddNewDefine(string value)
		{
			if(value.IndexOf("=") < 0)
				throw new PeachException("Error, defined values supplied via -D/--define must have an equals sign providing a key-pair set.");

			var kv = value.Split('=');
			DefinedValues[kv[0]] = kv[1];
		}

		/// <summary>
		/// Get attribute from XmlNode object.
		/// </summary>
		/// <param name="node">XmlNode to get attribute from</param>
		/// <param name="name">Name of attribute</param>
		/// <returns>Returns innerText or null.</returns>
		public string getXmlAttribute(XmlNode node, string name)
		{
			System.Xml.XmlAttribute attr = node.Attributes.GetNamedItem(name) as System.Xml.XmlAttribute;
			if (attr != null)
			{
				return attr.InnerText;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Get attribute from XmlNode object.
		/// </summary>
		/// <param name="node">XmlNode to get attribute from</param>
		/// <param name="name">Name of attribute</param>
		/// <param name="defaultValue">Default value if attribute is missing</param>
		/// <returns>Returns true/false or default value</returns>
		public bool getXmlAttributeAsBool(XmlNode node, string name, bool defaultValue)
		{
			string value = getXmlAttribute(node, name);
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
		/// Check to see if XmlNode has specific attribute.
		/// </summary>
		/// <param name="node">XmlNode to check</param>
		/// <param name="name">Name of attribute</param>
		/// <returns>Returns boolean true or false.</returns>
		public bool hasXmlAttribute(XmlNode node, string name)
		{
			if (node.Attributes == null)
				return false;

			object o = node.Attributes.GetNamedItem(name);
			return o != null;
		}

		public void syntax()
		{
			string syntax = @"This is the Peach Runtime.  The Peach Runtime is one of the many ways
to use Peach XML files.  Currently this runtime is still in development
but already exposes several abilities to the end-user such as performing
simple fuzzer runs, converting WireShark capture sinto Peach XML and
performing parsing tests of Peach XML files.

Please submit any bugs to Michael Eddington <mike@dejavusecurity.com>.

Syntax:

  peach -a channel
  peach -c peach_xml_file [test_name]
  peach -g
  peach [--skipto #] peach_xml_flie [test_name]
  peach -p 10,2 [--skipto #] peach_xml_file [test_name]
  peach --range 100,200 peach_xml_file [test_name]
  peach -t peach_xml_file

  -1                         Perform a single iteration
  -a,--agent                 Launch Peach Agent
  -c,--count                 Count test cases
  -t,--test xml_file         Test parse a Peach XML file
  -p,--parallel M,N          Parallel fuzzing.  Total of M machines, this
                             is machine N.
  --debug                    Enable debug messages. Usefull when debugging
                             your Peach XML file.  Warning: Messages are very
                             cryptic sometimes.
  --skipto N                 Skip to a specific test #.  This replaced -r
                             for restarting a Peach run.
  --range N,M                Provide a range of test #'s to be run.
  -D/define=KEY=VALUE        Define a substitution value.  In your PIT you can
                             ##KEY## and it will be replaced for VALUE.
  --definedvalues=FILENAME   XML file containing defined values


Peach Agent

  Syntax: peach.py -a channel
  
  Starts up a Peach Agent instance on this current machine.  User must provide
  a channel/protocol name (e.g. tcp).

  Note: Local agents are started automatically.

Performing Fuzzing Run

  Syntax: peach peach_xml_flie [test_name]
  Syntax: peach --skipto 1234 peach_xml_flie [test_name]
  Syntax: peach --range 100,200 peach_xml_flie [test_name]
  
  A fuzzing run is started by by specifying the Peach XML file and the
  name of a test to perform.
  
  If a run is interupted for some reason it can be restarted using the
  --skipto parameter and providing the test # to start at.
  
  Additionally a range of test cases can be specified using --range.

Performing A Parellel Fuzzing Run

  Syntax: peach -p 10,2 peach_xml_flie [test_name]

  A parallel fuzzing run uses multiple machines to perform the same fuzzing
  which shortens the time required.  To run in parallel mode we will need
  to know the total number of machines and which machine we are.  This
  information is fed into Peach via the " + "\"-p\""+@" command line argument in the
  format " + "\"total_machines,our_machine\"." + @"

Validate Peach XML File

  Syntax: peach.py -t peach_xml_file
  
  This will perform a parsing pass of the Peach XML file and display any
  errors that are found.

Debug Peach XML File

  Syntax: peach -1 --debug peach_xml_file
  
  This will perform a single iteration (-1) of your pit file while displaying
  alot of debugging information (--debug).  The debugging information was
  origionally intended just for the developers, but can be usefull in pit
  debugging as well.
";
			Console.WriteLine(syntax);
			throw new SyntaxException();
		}

		public void bob()
		{
			string bob = @"
@@@@@@@^^~~~~~~~~~~~~~~~~~~~~^@@@@@@@@@
@@@@@@^     ~^  @  @@ @ @ @ I  ~^@@@@@@
@@@@@            ~ ~~ ~I          @@@@@
@@@@'                  '  _,w@<    @@@@
@@@@     @@@@@@@@w___,w@@@@@@@@  @  @@@
@@@@     @@@@@@@@@@@@@@@@@@@@@@  I  @@@
@@@@     @@@@@@@@@@@@@@@@@@@@*@[ i  @@@
@@@@     @@@@@@@@@@@@@@@@@@@@[][ | ]@@@
@@@@     ~_,,_ ~@@@@@@@~ ____~ @    @@@
@@@@    _~ ,  ,  `@@@~  _  _`@ ]L  J@@@
@@@@  , @@w@ww+   @@@ww``,,@w@ ][  @@@@
@@@@,  @@@@www@@@ @@@@@@@ww@@@@@[  @@@@
@@@@@_|| @@@@@@P' @@P@@@@@@@@@@@[|c@@@@
@@@@@@w| '@@P~  P]@@@-~, ~Y@@^'],@@@@@@
@@@@@@@[   _        _J@@Tk     ]]@@@@@@
@@@@@@@@,@ @@, c,,,,,,,y ,w@@[ ,@@@@@@@
@@@@@@@@@ i @w   ====--_@@@@@  @@@@@@@@
@@@@@@@@@@`,P~ _ ~^^^^Y@@@@@  @@@@@@@@@
@@@@^^=^@@^   ^' ,ww,w@@@@@ _@@@@@@@@@@
@@@_xJ~ ~   ,    @@@@@@@P~_@@@@@@@@@@@@
@@   @,   ,@@@,_____   _,J@@@@@@@@@@@@@
@@L  `' ,@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
";
			Console.WriteLine(bob);
			throw new SyntaxException();
		}


		public void Charlie()
		{
			Console.WriteLine(@"
,-----.   
\======'.                                                                 
 \  {}   '.                                                               
  \   \/ V '.                                                             
   \  || |   '._                                 _,cmmmnc,_               
    \___68FS___\'-._=----+- _______________,.-=:3H)###C--  `c._           
    :|=--------------`---" + "\"" + @"'`.   `  `.   `.   `,   `~\" + "\"\"" + @"===" + "\"" + @"~`    `'-.___   
  ,dH] '       =(*)=         :       ---==;=--;  .   ;    +-- -_ .-`      
  :HH]_:______________  ____,.........__     _____,.----=-" + "\"" + @"~ `            
  ;:" + "\"" + @"+" + "\"" + @"\" + "\"" + @"+@" + "\"" + @"" + "\"" + @"+" + "\"" + @"\" + "\"" + @"" + "\"" + @"+@" + "\"" + @"'" + "\"" + @"+" + "\"" + @"\" + "\"" + @"+@" + "\"" + @"'----._.------\`  :          .   `.'`'" + "\"" + @"'" + "\"" + @"'" + "\"" + @"P
  |:      .-'==-.__)___\. :        .   .'`___L~___(                       
  |:  _.'`       '|   / \.:      .  .-`" + "\"" + @"" + "\"" + @"`                                
  `'" + "\"" + @"'            `--'   \:    ._.-'                                      
                         }_`============>-             
");
			throw new SyntaxException();
		}

		public void ShowDevices()
		{
			Console.WriteLine();
			Console.WriteLine("The following devices are available on this machine:");
			Console.WriteLine("----------------------------------------------------");
			Console.WriteLine();

			int i = 0;

			var devices = CaptureDeviceList.Instance;

			// Print out all available devices
			foreach (ICaptureDevice dev in devices)
			{
				Console.WriteLine("Name: {0}\nDescription: {1}\n\n", dev.Name, dev.Description);
				i++;
			}

			throw new SyntaxException();
		}

		public void ShowEnvironment()
		{
			Peach.Core.Usage.Print();

			throw new SyntaxException();
		}
	}


	public class SyntaxException : Exception
	{
	}
}

// end
