using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Options;
using PeachCore;

namespace Peach
{
	class Program
	{
		static void Main(string[] args)
		{
			new Program(args);
		}

		public Program(string[] args)
		{
			try
			{
				string analyzer = null;
				string parser = null;
				string strategy = null;
				string range = null;
				string parallel = null;
				uint skipTo = 0;
				bool debug = false;
				bool justOne = false;
				bool test = false;
				bool count = false;
				bool agent = false;

				var p = new OptionSet()
				{
					{ "h|?|help", v => syntax() },
					{ "analyzer=", v => analyzer = v },
					{ "parser=", v => parser = v },
					{ "strategy=", v => strategy = v},
					{ "d|debug", v => debug = true },
					{ "1", v => justOne = true},
					{ "range=", v => range = v},
					{ "t|test", v => test = v},
					{ "c|count", v => count = true},
					{ "skipto=", v => skipTo = Convert.ToUInt32(v)},
					{ "p|parallel=", v => parallel = v},
					{ "a|agent", v = agent = true},
				};
				List<string> extra = p.Parse(args);
			}
			catch (SyntaxException e)
			{
			}
			catch (PeachException ee)
			{
				Console.WriteLine(ee.Message);
			}
		}

		public void syntax()
		{
			string syntax = @"This is the Peach Runtime.  The Peach Runtime is one of the many ways
to use Peach XML files.  Currently this runtime is still in development
but already exposes several abilities to the end-user such as performing
simple fuzzer runs, converting WireShark captures into Peach XML and
performing parsing tests of Peach XML files.

Please submit any bugs to Michael Eddington <mike@phed.org>.

Syntax:

  peach.py -a [port] [password]
  peach.py -c peach_xml_file [run_name]
  peach.py -g
  peach.py [--skipto #] peach_xml_flie [run_name]
  peach.py -p 10,2 [--skipto #] peach_xml_file [run_name]
  peach.py --range 100,200 peach_xml_file [run_name]
  peach.py -t peach_xml_file

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

Peach Agent

  Syntax: peach.py -a
  Syntax: peach.py -a port
  Syntax: peach.py -a port password
  
  Starts up a Peach Agent instance on this current machine.  Defaults to
  port 9000.  When specifying a password, the port # must also be given.

  Note: Local agents are started automatically.

Performing Fuzzing Run

  Syntax: peach.py peach_xml_flie [run_name]
  Syntax: peach.py --skipto 1234 peach_xml_flie [run_name]
  Syntax: peach.py --range 100,200 peach_xml_flie [run_name]
  
  A fuzzing run is started by by specifying the Peach XML file and the
  name of a run to perform.
  
  If a run is interupted for some reason it can be restarted using the
  --skipto parameter and providing the test # to start at.
  
  Additionally a range of test cases can be specified using --range.

Performing A Parellel Fuzzing Run

  Syntax: peach.py -p 10,2 peach_xml_flie [run_name]

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

  Syntax: peach.py -1 --debug peach_xml_file
  
  This will perform a single iteration (-1) of your pit file while displaying
  alot of debugging information (--debug).  The debugging information was
  origionally intended just for the developers, but can be usefull in pit
  debugging as well.
";
			Console.WriteLine(syntax);
			throw new SyntaxException();
		}
	}

	public class SyntaxException : Exception
	{
	}
}

// end
