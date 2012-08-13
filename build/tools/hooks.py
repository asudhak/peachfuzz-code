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
		path_list = self.env['PATH']
	kw['path_list'] = path_list
	
	var = kw.get('var', '')
	filename = self.env[var] or filename
	self.env[var] = None

	if not isinstance(filename, list) and os.path.isabs(filename):
		parts = os.path.split(filename)
		kw['exts'] = ''
		kw['path_list'] = [ parts[0] ]
		filename = parts[1]

	return self.base_find_program(filename, **kw)

@hook
def add_to_group(self, tgen, group=None):
	features = set(Utils.to_list(getattr(tgen, 'features', [])))
	available = set(Utils.to_list(tgen.env['supported_features']))
	intersect = features & available
	
	if intersect == features:
		self.base_add_to_group(tgen, group)
	elif Logs.verbose > 0:
		missing = [ x for x in (features - intersect)]
		Logs.warn('Skipping %r due to missing features: %s' % (tgen.name, missing))

def colorize(value, color):
	return color + Logs.colors.BOLD + value + Logs.colors.NORMAL

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
	n = len(str(total))
	fs = '[%%%dd/%%%dd]' % (n, n)
	pct_str = fs % (cur(), total)
	var_str = self.generator.bld.variant
	bld_str = getattr(self.generator, 'name', self.generator.__class__.__name__)
	tsk_str = self.__class__.__name__
	src_str = str([ x.name for x in self.inputs] )
	tgt_str = str([ x.name for x in self.outputs ])

	if isinstance(self.generator, Build.inst):
		return None

	return "%s | %s | %s | %s | %s | %s\n" % (
		colorize(pct_str, Logs.colors.NORMAL),
		colorize(var_str, Logs.colors.YELLOW),
		colorize(bld_str, Logs.colors.RED),
		colorize(tsk_str, Logs.colors.NORMAL),
		colorize(src_str, Logs.colors.CYAN),
		colorize(tgt_str, Logs.colors.GREEN))

@hook
def do_install(self, src, tgt, chmod=Utils.O644):
	if self.progress_bar == 0:
		self.progress_bar = -1;

	ret = self.base_do_install(src, tgt, chmod)

	if self.progress_bar != -1 or str(ret) == 'False':
		return

	dest = os.path.split(tgt)[1]
	filename = os.path.split(src)[1]
	target = str(os.path.relpath(tgt, os.path.join(self.srcnode.abspath(), self.env.PREFIX)))

	msg = "%s | %s | %s | %s\n" % (
		colorize(self.variant, Logs.colors.YELLOW),
		colorize('Install', Logs.colors.NORMAL),
		colorize(filename, Logs.colors.CYAN),
		colorize(target, Logs.colors.GREEN))

	self.to_log(msg)
