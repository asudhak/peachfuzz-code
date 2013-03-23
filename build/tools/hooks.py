from waflib.Build import BuildContext, InstallContext
from waflib.Configure import ConfigurationContext
from waflib.Context import Context
from waflib.Task import TaskBase
from waflib import Build, Utils, Logs, Errors
import os.path
import sys

# ConfigurationContext
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

# Context
def add_to_group(self, tgen, group=None):
	features = set(Utils.to_list(getattr(tgen, 'features', [])))
	available = set(Utils.to_list(tgen.env['supported_features']))
	intersect = features & available

	if intersect == features:
		self.base_add_to_group(tgen, group)
	elif Logs.verbose > 0:
		missing = [ x for x in (features - intersect)]
		Logs.warn('Skipping %r due to missing features: %s' % (tgen.name, missing))

# BuildContext
def exec_command(self, cmd, **kw):
	subprocess = Utils.subprocess
	kw['shell'] = isinstance(cmd, str)

	msg = self.logger or Logs
	msg.debug('runner: %r' % cmd)
	msg.debug('runner_env: kw=%s' % kw)

	kw['stderr'] = kw['stdout'] = subprocess.PIPE

	try:
		p = subprocess.Popen(cmd, **kw)
		(out, err) = p.communicate()
		ret = p.returncode
	except Exception as e:
		raise Errors.WafError('Execution failure: %s' % str(e), ex=e)

	if not isinstance(out, str):
		out = out.decode(sys.stdout.encoding or 'iso8859-1')
	if self.logger:
		self.logger.debug('out: %s' % out)
	elif ret or Logs.verbose > 0:
		sys.stdout.write(out)

	if not isinstance(err, str):
		err = err.decode(sys.stdout.encoding or 'iso8859-1')
	if self.logger:
		self.logger.debug('err: %s' % err)
	else:
		sys.stderr.write(err)

	return ret

def colorize(value, color):
	return color + Logs.colors.BOLD + value + Logs.colors.NORMAL

# TaskBase
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

# InstallContext
def do_install(self, src, tgt, chmod=Utils.O644):
	if Logs.verbose <= 1 and self.progress_bar == 0:
		self.progress_bar = -1;

	ret = self.base_do_install(src, tgt, chmod)

	if self.progress_bar != -1 or str(ret) == 'False':
		return ret

	dest = os.path.split(tgt)[1]
	filename = os.path.split(src)[1]
	target = str(os.path.relpath(tgt, os.path.join(self.srcnode.abspath(), self.env.PREFIX)))

	msg = "%s | %s | %s | %s\n" % (
		colorize(self.variant, Logs.colors.YELLOW),
		colorize('Install', Logs.colors.NORMAL),
		colorize(filename, Logs.colors.CYAN),
		colorize(target, Logs.colors.GREEN))

	self.to_log(msg)

TaskBase.base_display = TaskBase.display
TaskBase.display = display

Context.base_exec_command = Context.exec_command
Context.exec_command = exec_command

ConfigurationContext.base_find_program = ConfigurationContext.find_program
ConfigurationContext.find_program = find_program

BuildContext.base_add_to_group = BuildContext.add_to_group
BuildContext.add_to_group = add_to_group

InstallContext.base_do_install = InstallContext.do_install
InstallContext.do_install = do_install

