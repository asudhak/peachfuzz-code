from waflib import Utils, Errors, Task
from waflib.Tools import cs
from waflib.TaskGen import feature, before_method, after_method
import os.path, tempfile

@feature('cs')
@before_method('apply_cs')
def cs_helpers(self):
	if not getattr(self, 'gen', None):
		setattr(self, 'gen', self.name)
	
	setattr(self, 'platform', self.env.CSPLATFORM)

@feature('csprogram')
@before_method('apply_cs')
@after_method('cs_helpers')
def apply_csprogram(self):
	base = os.path.splitext(self.gen)[0]

	cfg = self.path.find_resource('app.config')
	if not cfg:
		raise Errors.WafError('required resource \'app.config\' not found when building %r' % self.name)

	try:
		inst_to = self.install_path
	except AttributeError:
		inst_to = '${BINDIR}/%s' % base

	if inst_to:
		self.install_path = inst_to
		self.bld.install_as('%s/%s.config' % (inst_to, base), cfg, env=self.env, chmod=Utils.O644)

@feature('csprogram')
@after_method('use_cs')
def use_cprogram(self):
	if not self.install_path:
		return

	base = os.path.splitext(self.gen)[0]
	names = self.to_list(getattr(self, 'use', []))
	get = self.bld.get_tgen_by_name
	for x in names:
		try:
			y = get(x)
		except Errors.WafError:
			continue

		tsk = getattr(y, 'cs_task', None) or getattr(y, 'link_task', None)
		if not tsk:
			self.bld.fatal('cs task has no link task for use %r' % self)
		
		self.bld.install_files(self.install_path, tsk.outputs)

		# if a use task has 'install' attribute, install those deps as well
		extra_inst = y.to_nodes(getattr(y, 'install', []))
		if (extra_inst):
			self.bld.install_files(self.install_path, extra_inst)

	icon = getattr(self, 'icon', None)
	if icon:
		node = self.path.find_resource(icon)
		if not node:
			raise Errors.WafError('invalid icon file %r' % icon)
		self.env.append_value('CSFLAGS', ['/win32icon:%s' % node.path_from(self.bld.bldnode)])

def quote_response_command(self, flag):
	# /noconfig is not allowed when using response files
	if flag.lower() == '/noconfig':
		return ''

	if flag.find(' ') > -1:
		for x in ('/r:', '/reference:', '/resource:', '/lib:'):
			if flag.startswith(x):
				flag = '%s"%s"' % (x, flag[len(x):])
				break
		else:
			flag = '"%s"' % flag
	return flag

def exec_command_mcs(self, *k, **kw):
	bld = self.generator.bld
	cmd = k[0]

	try:
		if not kw.get('cwd', None):
			kw['cwd'] = bld.cwd
	except AttributeError:
		bld.cwd = kw['cwd'] = bld.variant_dir

	try:
		tmp = None
		if isinstance(cmd, list) and len(' '.join(cmd)) >= 8192:
			program = cmd[0] #unquoted program name, otherwise exec_command will fail
			cmd = [self.quote_response_command(x) for x in cmd]
			(fd, tmp) = tempfile.mkstemp()
			os.write(fd, '\r\n'.join(i.replace('\\', '\\\\') for i in cmd[1:]).encode())
			os.close(fd)
			cmd = [program, '@' + tmp]
		# no return here, that's on purpose
		ret = self.generator.bld.exec_command(cmd, **kw)
	finally:
		if tmp:
			try:
				os.remove(tmp)
			except OSError:
				pass # anti-virus and indexers can keep the files open -_-
	return ret

if Utils.is_win32:
	cls = Task.classes.get('mcs', None)
	if not cls:
		raise Errors.WafError('msc class could not be found, ensure the cs tool has been loaded.')

	cls.exec_command = exec_command_mcs
	cls.quote_response_command = quote_response_command
