import os, os.path, sys
from waflib.TaskGen import feature, after_method
from waflib import Utils, Task, Logs, Options, Errors
testlock = Utils.threading.Lock()

def summary(bld):
	lst = getattr(bld, 'utest_results', [])
	if lst:
		Logs.pprint('CYAN', 'execution summary')

		total = len(lst)
		tfail = len([x for x in lst if x[1]])

		Logs.pprint('CYAN', '  tests that pass %d/%d' % (total-tfail, total))
		for (f, code) in lst:
			if not code:
				Logs.pprint('CYAN', '    %s' % f)

		Logs.pprint('CYAN', '  tests that fail %d/%d' % (tfail, total))
		for (f, code) in lst:
			if code:
				Logs.pprint('CYAN', '    %s' % f)

def prepare_nunit_test(self):
	self.ut_exec = [ self.generator.bld.env.NUNIT, '/xml:%s' % self.outputs[0].abspath(), self.inputs[0].abspath() ]

@feature('test')
@after_method('apply_link')
def make_test(self):
	self.bld.add_post_fun(summary)	

	inputs = []
	outputs = []

	if getattr(self, 'link_task', None):
		inputs = [ self.link_task.outputs[0] ]

	if getattr(self, 'cs_task', None):
		inputs = [ self.cs_task.outputs[0] ]
		if self.gen.endswith('.dll'):
			self.ut_fun = prepare_nunit_test
			outputs = [ inputs[0].change_ext('.xml') ]

	if not inputs:
		raise Errors.WafError('No test to run at: %r' % self)

	if Logs.verbose == 0:
		outputs.append( inputs[0].change_ext('.log') )

	test = self.create_task('utest', inputs, outputs)
	self.bld.install_files('${PREFIX}/utest', test.outputs)

class utest(Task.Task):
	color = 'PINK'
	after = ['vnum', 'inst']
	vars = []

	def runnable_status(self):
		if getattr(self.generator.bld, 'is_test', None):
			return Task.RUN_ME
		return Task.SKIP_ME

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

			def add_path(dct, path, var):
				dct[var] = os.pathsep.join(Utils.to_list(path) + [os.environ.get(var, '')])

			if Utils.is_win32:
				add_path(fu, lst, 'PATH')
			elif Utils.unversioned_sys_platform() == 'darwin':
				add_path(fu, lst, 'DYLD_LIBRARY_PATH')
				add_path(fu, lst, 'LD_LIBRARY_PATH')
			else:
				add_path(fu, lst, 'LD_LIBRARY_PATH')

			path_var = self.generator.env.CS_NAME == 'mono' and 'MONO_PATH' or 'DEVPATH'
			fu[path_var] = os.pathsep.join(asm)

		Logs.debug('runner: %r' % self.ut_exec)
		cwd = getattr(self.generator, 'ut_cwd', '') or self.inputs[0].parent.abspath()

		if Logs.verbose > 0:
			proc = Utils.subprocess.Popen(self.ut_exec, cwd=cwd, env=fu, stderr=None, stdout=None)
			ret = proc.wait()
		else:
			with open(self.outputs[-1].abspath(), "wb") as out:
				proc = Utils.subprocess.Popen(self.ut_exec, cwd=cwd, env=fu, stderr=Utils.subprocess.STDOUT, stdout=out)
				ret = proc.wait()

		tup = (self.inputs[0].name, ret)
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
	nunit_path = os.path.join(conf.path.abspath(), '3rdParty', 'NUnit-2.6.0.12051', 'bin')
	nunit_name = '64' in conf.env.SUBARCH and 'nunit-console' or 'nunit-console-x86'
	conf.find_program([nunit_name], var='NUNIT', exts='.exe', path_list=[nunit_path])
