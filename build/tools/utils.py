import os.path
from waflib.TaskGen import feature, before_method, after_method
from waflib import Utils, Logs, Task

@feature('*')
@after_method('process_source')
def install_extras(self):
	try:
		inst_to = self.install_path
	except AttributeError:
		inst_to = hasattr(self, 'link_task') and getattr(self.link_task.__class__, 'inst_to', None)

	if not inst_to:
		if getattr(self, 'install', []):
			Logs.warn('\'%s\' has no install path but is supposed to install: %s' % (self.name, self.install))
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

	self.bld.install_files('${LIBDIR}', self.link_task.outputs, chmod=Utils.O755)

@feature('pin')
def pin_disable_debug(self):
        e = self.env
        e['CFLAGS']   = filter(lambda x: '/MD'   not in x, e['CFLAGS'])
        e['CXXFLAGS'] = filter(lambda x: '/MD'   not in x, e['CXXFLAGS'])
        e['DEFINES']  = filter(lambda x: 'DEBUG' not in x, e['DEFINES'])
