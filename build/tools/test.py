import os, os.path, sys
from waflib.Build import InstallContext
from waflib.TaskGen import feature, after_method
from waflib import Utils, Task, Logs, Options, Errors

testlock = Utils.threading.Lock()

def prepare_nunit_test(self):
	self.ut_nunit = True
	self.ut_exec = []

	if (self.env.CS_NAME == 'mono'):
		self.ut_exec = [ 'mono', '--debug' ]

	self.outputs[0].parent.mkdir()

	self.ut_exec.extend([
		self.env.NUNIT,
		self.inputs[0].abspath(),
		'-nologo',
		'-out:%s' % self.outputs[1].abspath(),
		'-xml:%s' % self.outputs[0].abspath(),
	])

def get_inst_node(self, dest, name):
	dest = Utils.subst_vars(dest, self.env)
	dest = dest.replace('/', os.sep)
	if Options.options.destdir:
		dest = os.path.join(Options.options.destdir, os.path.splitdrive(dest)[1].lstrip(os.sep))

	return self.bld.srcnode.make_node([dest, name])

@feature('test')
@after_method('apply_link')
def make_test(self):
	inputs = []
	outputs = []

	if getattr(self, 'link_task', None):
		dest = getattr(self, 'install_path', self.link_task.__class__.inst_to)
		inputs = [ get_inst_node(self, dest, self.link_task.outputs[0].name) ]

	if getattr(self, 'cs_task', None):
		bintype = getattr(self, 'bintype', self.gen.endswith('.dll') and 'library' or 'exe')
		dest = getattr(self, 'install_path', bintype=='exe' and '${BINDIR}' or '${LIBDIR}')
		inputs = [ get_inst_node(self, dest, self.cs_task.outputs[0].name) ]

		if self.gen.endswith('.dll'):
			self.ut_fun = prepare_nunit_test
			name = os.path.splitext(inputs[0].name)[0]
			xml = get_inst_node(self, '${PREFIX}/utest', name + '.xml')
			log = get_inst_node(self, '${PREFIX}/utest', name + '.log')
			outputs = [ xml, log ]

	if not inputs:
		raise Errors.WafError('No test to run at: %r' % self)

	test = self.create_task('utest', inputs, outputs)

class utest(Task.Task):
	vars = []

	after = ['vnum', 'inst']

	def runnable_status(self):
		ret = Task.SKIP_ME

		if getattr(self.generator.bld, 'is_test', None):
			ret = super(utest, self).runnable_status()
			if ret == Task.SKIP_ME:
				ret = Task.RUN_ME

		return ret

	def run(self):
		if not os.path.isfile(self.inputs[0].abspath()):
			raise Errors.WafError('Could not find test input: %s' % self.inputs[0].abspath())

		self.ut_exec = getattr(self, 'ut_exec', [ self.inputs[0].abspath() ])
		if getattr(self.generator, 'ut_fun', None):
			self.generator.ut_fun(self)

		Logs.debug('runner: %r' % self.ut_exec)
		cwd = getattr(self.generator, 'ut_cwd', '') or self.inputs[0].parent.abspath()

		stderr = stdout = None
		if Logs.verbose < 0:
			stderr = stdout = Utils.subprocess.PIPE

		proc = Utils.subprocess.Popen(self.ut_exec, cwd=cwd, stderr=stderr, stdout=stdout)
		(stdout, stderr) = proc.communicate()

		tup = (self.inputs[0].name, proc.returncode)
		self.generator.test_result = tup

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
	j = os.path.join

	try:
		conf.env['NUNIT_VER'] = 'NUnit.Runners.2.6.3'
		nunit_path = j(conf.get_peach_dir(), '3rdParty', conf.env['NUNIT_VER'], 'tools')
		nunit_name = '64' in conf.env.SUBARCH and 'nunit-console' or 'nunit-console-x86'
		conf.find_program([nunit_name], var='NUNIT', exts='.exe', path_list=[nunit_path])
		conf.env.append_value('supported_features', [ 'test' ])
	except Exception, e:
		conf.env.append_value('missing_features', [ 'test' ])
		if Logs.verbose:
			Logs.warn('Unit test feature is not available: %s' % (e))

def options(opt):
	pass

class TestContext(InstallContext):
	'''runs the unit tests'''

	cmd = 'test'

	def __init__(self, **kw):
		super(TestContext, self).__init__(**kw)
		self.is_test = True
		self.add_post_fun(TestContext.summary)

	def summary(self):
		lst = getattr(self, 'utest_results', [])
		err = ', '.join(t[0] for t in filter(lambda x: x[1] != 0, lst))
		if err:
			raise Errors.WafError('Failures detected in test suites: %s' % err)
