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
	if extras:
		self.bld.install_files(inst_to, extras, env=self.env, cwd=self.path, relative_trick=True, chmod=Utils.O644)

@feature('win', 'linux', 'osx')
def dummy_platform(self):
	# prevent warnings about features with unbound methods
	pass

@feature('fake_lib')
@after_method('process_lib')
def install_csshlib(self):
	if self.link_task.__class__.__name__ != 'fake_csshlib':
		return

	# install 3rdParty libs into ${LIBDIR}
	self.bld.install_files('${LIBDIR}', self.link_task.outputs, chmod=Utils.O755)

	# install any .config files into ${LIBDIR}
	for lib in self.link_task.outputs:
		config = lib.parent.find_resource(lib.name + '.config')
		if config:
			self.bld.install_files('${LIBDIR}', config, chmod=Utils.O755)

@feature('pin')
def pin_disable_debug(self):
	# ensure pin debug builds are built in release mode
	e = self.env
	e['CFLAGS']   = filter(lambda x: '/MD'   not in x, e['CFLAGS'])
	e['CXXFLAGS'] = filter(lambda x: '/MD'   not in x, e['CXXFLAGS'])
	e['DEFINES']  = filter(lambda x: 'DEBUG' not in x, e['DEFINES'])

@feature('cs')
@before_method('apply_cs')
def cs_helpers(self):
	# set self.gen based off self.name since they are usually the same
	if not getattr(self, 'gen', None):
		setattr(self, 'gen', self.name)

	# ensure the appropriate platform is being set on the command line
	setattr(self, 'platform', self.env.CSPLATFORM)

	# ensure install_path is set
	if not getattr(self, 'install_path', None):
		setattr(self, 'install_path', '${BINDIR}')

@feature('cs')
@after_method('apply_cs')
def cs_resource(self):
	base = os.path.splitext(self.gen)[0]

	# add external resources to the dependency list and compilation command line
	resources = self.to_nodes(getattr(self, 'resource', []))
	self.cs_task.dep_nodes.extend(resources)
	for x in resources:
		rel_path = x.path_from(self.path)
		name = rel_path.replace('\\', '.').replace('/', '.')
		final = base + '.' + name
		self.env.append_value('CSFLAGS', '/resource:%s,%s' % (x.abspath(), final))

	# win32 icon support
	icon = getattr(self, 'icon', None)
	if icon:
		node = self.path.find_or_declare(icon)
		self.cs_task.dep_nodes.append(node)
		self.env.append_value('CSFLAGS', ['/win32icon:%s' % node.path_from(self.bld.bldnode)])

	# if this is an exe, look for app.config and install to ${BINDIR}
	if 'exe' in self.cs_task.env.CSTYPE:
		inst_to = getattr(self, 'install_path', '${BINDIR}')
		cfg = self.path.find_or_declare('app.config')
		self.bld.install_as('%s/%s.config' % (inst_to, base), cfg, env=self.env, chmod=Utils.O755)
