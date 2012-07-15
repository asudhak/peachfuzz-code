from waflib import Utils, Errors
from waflib.Tools import cs
from waflib.TaskGen import feature, before_method, after_method
import os.path

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
			self.env.append_value('CSFLAGS', '/reference:%s' % x)
			continue

		tsk = getattr(y, 'cs_task', None) or getattr(y, 'link_task', None)
		if not tsk:
			self.bld.fatal('cs task has no link task for use %r' % self)
		
		self.bld.install_files(self.install_path, tsk.outputs)
	
	extras = self.to_nodes(getattr(self, 'install', []))
	for x in extras:
		rel_path = x.path_from(self.path)
		self.bld.install_as(os.path.join(self.install_path, rel_path), x, env=self.env, chmod=Utils.O644)

