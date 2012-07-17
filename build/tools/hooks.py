from waflib import Build, Utils, Logs, Task
from waflib.Configure import ConfigurationContext
import os.path

def apply_hook(cls, name, fun):
	# Ensure we haven't already hooked an object cls inherits from
	if getattr(cls, 'base_%s' % name, None):
		return

	old = getattr(cls, name, None)
	if old:
		setattr(cls, 'base_%s' % name, old)
		setattr(cls, name, fun)

def hook(f):
	def fun(*k, **kw):
		return f(*k, **kw)

	apply_hook(Task.TaskBase, f.__name__, fun)
	apply_hook(ConfigurationContext, f.__name__, fun)
	apply_hook(Build.BuildContext, f.__name__, fun)
	apply_hook(Build.InstallContext, f.__name__, fun)

	return f

@hook
def find_program(self, filename, **kw):
	path_list = kw.get('path_list', None)
	if not path_list:
		kw['path_list'] = self.env['TOOLCHAIN_PATH']

	var = kw.get('var', '')
	filename = self.env['TOOLCHAIN_%s' % var] or filename

	return self.base_find_program(filename, **kw)

@hook
def add_to_group(self, tgen, group=None):
	return self.base_add_to_group(tgen, group)

@hook
def display(self):
	sep = Logs.colors.NORMAL + ' | '
	master = self.master

	def cur():
		# the current task position, computed as late as possible
		tmp = -1
		if hasattr(master, 'ready'):
			tmp -= master.ready.qsize()
		return master.processed + tmp

	if self.generator.bld.progress_bar > 0:
		return self.base_display()

	total = master.total
	bld_str = getattr(self.generator, 'name', self.generator.__class__.__name__)
	tsk_str = self.__class__.__name__
	src_str = str([ x.name for x in self.inputs] )
	tgt_str = str([ x.name for x in self.outputs ])
	n = len(str(total))
	fs = '[%%%dd/%%%dd]' % (n, n)

	if isinstance(self.generator, Build.inst):
		return None

	return str(fs % (cur(), total)) + \
		sep + \
		Logs.colors.YELLOW + Logs.colors.BOLD + \
		self.generator.bld.variant + \
		sep + \
		Logs.colors.RED + Logs.colors.BOLD + \
		bld_str + \
		sep + \
		Logs.colors.BOLD + \
		tsk_str + \
		sep + \
		Logs.colors.CYAN + Logs.colors.BOLD + \
		src_str + \
		Logs.colors.NORMAL + \
		' -> ' + \
		Logs.colors.GREEN + Logs.colors.BOLD + \
		tgt_str + \
		Logs.colors.NORMAL + '\n'

@hook
def do_install(self, src, tgt, chmod=Utils.O644):
	if self.progress_bar == 0:
		self.progress_bar = -1;

	ret = self.base_do_install(src, tgt, chmod)

	if self.progress_bar != -1 or str(ret) == 'False':
		return

	sep = Logs.colors.NORMAL + ' | '
	dest = os.path.split(tgt)[1]
	filename = os.path.split(src)[1]
	target = str(os.path.relpath(tgt, os.path.join(self.srcnode.abspath(), self.env.OUTDIR)))

	msg = \
		Logs.colors.YELLOW + Logs.colors.BOLD + \
		self.variant + \
		sep + \
		Logs.colors.BOLD + Logs.colors.BOLD + \
		'Install' + \
		sep + \
		Logs.colors.CYAN + Logs.colors.BOLD + \
		filename + \
		Logs.colors.NORMAL + \
		' -> ' + \
		Logs.colors.GREEN + Logs.colors.BOLD + \
		target + \
		Logs.colors.NORMAL + '\n'

	self.to_log(msg)
