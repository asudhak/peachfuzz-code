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

@feature('cs')
@after_method('apply_cs')
def cs_resource(self):
	inst_to = getattr(self, 'install_path', self.gen.endswith('.dll') and '${LIBDIR}' or None)
	if inst_to:
		self.install_path = inst_to

	base = os.path.splitext(self.gen)[0]

	resources = self.to_nodes(getattr(self, 'resource', []))
	self.cs_task.dep_nodes.extend(resources)

	for x in resources:
		rel_path = x.path_from(self.path)
		name = rel_path.replace('\\', '.').replace('/', '.')
		final = base + '.' + name
		self.env.append_value('CSFLAGS', '/resource:%s,%s' % (x.abspath(), final))

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

