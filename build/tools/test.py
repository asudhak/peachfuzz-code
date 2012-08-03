import os, os.path
from waflib.TaskGen import feature, before_method, after_method
from waflib import Utils, Task

def configure(conf):
	nunit_path = os.path.join(conf.path.abspath(), '3rdParty', 'NUnit-2.6.0.12051', 'bin')
	nunit_name = '64' in conf.env.SUBARCH and 'nunit-console' or 'nunit-console-x86'

	conf.load('waf_unit_test')

	conf.find_program([nunit_name], var='NUNIT', exts='.exe', path_list=[nunit_path])

def prepare_csprg_test(self):
	try:
		fu = getattr(self.generator.bld, 'all_test_paths')
		return
	except AttributeError:
		pass
	
	fu = os.environ.copy()

	lst = []
	for g in self.generator.bld.groups:
		for tg in g:
			if getattr(tg, 'link_task', None):
				lst.append(tg.link_task.outputs[0].parent.abspath())

	def add_path(dct, path, var):
		dct[var] = os.pathsep.join(Utils.to_list(path) + [os.environ.get(var, '')])

	if Utils.is_win32:
		add_path(fu, lst, 'PATH')
	elif Utils.unversioned_sys_platform() == 'darwin':
		add_path(fu, lst, 'DYLD_LIBRARY_PATH')
		add_path(fu, lst, 'LD_LIBRARY_PATH')
	else:
		add_path(fu, lst, 'LD_LIBRARY_PATH')
	
	lst = []
	for g in self.generator.bld.groups:
		for tg in g:
			if getattr(tg, 'cs_task', None):
				if tg.gen.endswith('.dll'):
					lst.append(tg.cs_task.outputs[0].parent.abspath())
			if getattr(tg, 'link_task', None):
				if tg.link_task.__class__.__name__ == 'fake_csshlib':
					lst.append(tg.link_task.outputs[0].parent.abspath())

	path_var = self.generator.env.CS_NAME == 'mono' and 'MONO_PATH' or 'DEVPATH'
	fu[path_var] = os.pathsep.join(lst)

	self.generator.bld.all_test_paths = fu

def prepare_nunit_test(self):
	prepare_csprg_test(self)
	self.ut_exec = [ self.generator.bld.env.NUNIT, self.ut_exec[0] ]

@feature('test')
@after_method('make_test')
def make_cs_test(self):
	if getattr(self, 'cs_task', None):
		self.create_task('utest', self.cs_task.outputs)
		if self.gen.endswith('.dll'):
			self.ut_fun = prepare_nunit_test
		else:
			self.ut_fun = prepare_csprg_test

@feature('test')
@after_method('make_cs_test')
def filter_tests(self):
	if getattr(self.bld, 'is_test', None):
		from waflib.Tools import waf_unit_test
		self.bld.add_post_fun(waf_unit_test.summary)	

	for t in self.tasks:
		if t.__class__.__name__ == 'utest':
			t.inputs = [ t.inputs[0] ]
			if not getattr(self.bld, 'is_test', None):
				t.hasrun = Task.SKIP_ME
