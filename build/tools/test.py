import os, os.path, sys
from waflib.TaskGen import feature, after_method
from waflib import Utils, Task, Logs, Options, Errors
import xml.sax.handler
testlock = Utils.threading.Lock()

class NunitCounter(xml.sax.handler.ContentHandler):
	def __init__(self):
		self.num_passed = 0;
		self.num_failed = 0;
		self.num_skipped = 0;

	def startElement(self, name, attrs):
		if name == 'test-case':
			if attrs['executed'] == 'False':
				self.num_skipped += 1
			elif attrs['success'] == 'True':
				self.num_passed += 1
			else:
				self.num_failed += 1

def get_nunit_stats(filename):
	if not filename:
		return ''

	handler = NunitCounter()
	parser = xml.sax.make_parser()
	parser.setContentHandler(handler)
	parser.parse(filename)
	return '     |   Passed: %s, Failed: %s, Skipped: %s' % (handler.num_passed, handler.num_failed, handler.num_skipped)

def summary(bld):
	lst = getattr(bld, 'utest_results', [])
	if lst:
		Logs.pprint('CYAN', 'execution summary')

		total = len(lst)
		tfail = len([x for x in lst if x[1]])

		Logs.pprint('CYAN', '  tests that pass %d/%d' % (total-tfail, total))
		for (f, code, xml) in lst:
			if not code:
				Logs.pprint('CYAN', '    %s%s' % (f.ljust(30), get_nunit_stats(xml)))

		Logs.pprint('CYAN', '  tests that fail %d/%d' % (tfail, total))
		for (f, code, xml) in lst:
			if code:
				Logs.pprint('CYAN', '    %-20s%s' % (f.ljust(30), get_nunit_stats(xml)))

		if tfail:
			raise Errors.WafError(msg='%d out of %d test suites failed' % (tfail, total))

def prepare_nunit_test(self):
	self.ut_exec = []

	if (Utils.unversioned_sys_platform() != 'win32'):
		self.ut_exec = [ 'mono', '--debug' ]

	self.ut_exec.extend([
		self.generator.bld.env.NUNIT,
		self.inputs[0].abspath(),
		'-nologo',
		'-out:%s' % self.outputs[1].abspath(),
		'-xml:%s' % self.outputs[0].abspath(),
	])

@feature('test')
@after_method('apply_link')
def make_test(self):
	if summary not in getattr(self.bld, 'post_funs', []):
		self.bld.add_post_fun(summary)

	inputs = []
	outputs = []

	if getattr(self, 'link_task', None):
		inputs = [ self.link_task.outputs[0] ]

	if getattr(self, 'cs_task', None):
		inputs = [ self.cs_task.outputs[0] ]
		if self.gen.endswith('.dll'):
			self.ut_nunit = True
			self.ut_fun = prepare_nunit_test
			name = os.path.splitext(inputs[0].name)[0]
			xml = inputs[0].parent.find_or_declare('utest/%s.xml' % name)
			outputs = [ xml, xml.change_ext('.log') ]

	if not inputs:
		raise Errors.WafError('No test to run at: %r' % self)

	test = self.create_task('utest', inputs, outputs)
	if outputs and getattr(self.bld, 'is_test', None):
		self.bld.install_files('${PREFIX}/utest', test.outputs)

class utest(Task.Task):
	vars = []

	def runnable_status(self):
		ret = Task.SKIP_ME

		if getattr(self.generator.bld, 'is_test', None):
			ret = super(utest, self).runnable_status()
			if ret == Task.SKIP_ME:
				ret = Task.RUN_ME

		return ret

	def run(self):
		self.ut_exec = getattr(self, 'ut_exec', [ self.inputs[0].abspath() ])
		if getattr(self.generator, 'ut_fun', None):
			self.generator.ut_fun(self)

		try:
			fu = getattr(self.generator.bld, 'all_test_paths')
		except AttributeError:
			fu = os.environ.copy()
			self.generator.bld.all_test_paths = fu

			lst = []
			asm = []
			for g in self.generator.bld.groups:
				for tg in g:
					if getattr(tg, 'link_task', None):
						if tg.link_task.__class__.__name__ == 'fake_csshlib':
							asm.append(tg.link_task.outputs[0].parent.abspath())
						lst.append(tg.link_task.outputs[0].parent.abspath())
					if getattr(tg, 'cs_task', None):
						if tg.gen.endswith('.dll'):
							asm.append(tg.cs_task.outputs[0].parent.abspath())
						lst.append(tg.cs_task.outputs[0].parent.abspath())
					if getattr(tg, 'install', None):
						lst.append(tg.path.get_src().abspath())

			def add_path(dct, path, var):
				dct[var] = os.pathsep.join(Utils.to_list(path) + [os.environ.get(var, '')])

			add_path(fu, lst, 'PATH')

			if Utils.unversioned_sys_platform() == 'darwin':
				add_path(fu, lst, 'DYLD_LIBRARY_PATH')
				add_path(fu, lst, 'LD_LIBRARY_PATH')
			else:
				add_path(fu, lst, 'LD_LIBRARY_PATH')

			path_var = self.generator.env.CS_NAME == 'mono' and 'MONO_PATH' or 'DEVPATH'
			fu[path_var] = os.pathsep.join(asm)

		Logs.debug('runner: %r' % self.ut_exec)
		cwd = getattr(self.generator, 'ut_cwd', '') or self.inputs[0].parent.abspath()

		stderr = stdout = None
		if Logs.verbose == 0:
			stderr = stdout = Utils.subprocess.PIPE

		proc = Utils.subprocess.Popen(self.ut_exec, cwd=cwd, env=fu, stderr=stderr, stdout=stdout)
		(stdout, stderr) = proc.communicate()

		xml = getattr(self.generator, 'ut_nunit', False) and self.outputs[0].abspath() or None

		tup = (self.inputs[0].name, proc.returncode, xml)
		self.generator.utest_result = tup

		testlock.acquire()
		try:
			bld = self.generator.bld
			Logs.debug("ut: %r", tup)
			try:
				bld.utest_results.append(tup)
			except AttributeError:
				bld.utest_results = [tup]
		finally:
			testlock.release()

def configure(conf):
	root = conf.path.abspath()
	nunit_path = conf.env['NUNIT_PATH'] or os.path.join(root, '3rdParty', 'NUnit-2.6.0.12051', 'bin')
	nunit_name = '64' in conf.env.SUBARCH and 'nunit-console' or 'nunit-console-x86'
	conf.find_program([nunit_name], var='NUNIT', exts='.exe', path_list=[nunit_path])
