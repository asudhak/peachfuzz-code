import os.path
from waflib.TaskGen import feature, before_method, after_method
from waflib import Utils

@feature('*')
@after_method('process_source')
def install_extras(self):
	try:
		inst_to = self.install_path
	except AttributeError:
		inst_to = hasattr(self, 'link_task') and self.link_task.__class__.inst_to or None

	if not inst_to:
		return

	extras = self.to_nodes(getattr(self, 'install', []))
	for x in extras:
		rel_path = x.path_from(self.path)
		self.bld.install_as(os.path.join(inst_to, rel_path), x, env=self.env, chmod=Utils.O644)

@feature('win', 'linux', 'osx')
def dummy_platform(self):
	pass

@feature('fake_lib')
@after_method('process_lib')
def install_csshlib(self):
	if self.link_task.__class__.__name__ != 'fake_csshlib':
		return
		print self.target

	self.bld.install_files('${LIBDIR}', self.link_task.outputs, chmod=Utils.O755)
