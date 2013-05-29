import os.path, re
from waflib.TaskGen import feature, before_method, after_method
from waflib.Configure import conf
from waflib import Utils, Logs, Task, Context, Errors

@feature('*')
@before_method('process_source')
def default_variant(self):
	if not self.env.VARIANT:
		return

	features = set(Utils.to_list(self.features))
	available = set(Utils.to_list(self.env.VARIANTS))
	intersect = features & available

	if not intersect:
		features.add(self.env.VARIANT)
		self.features = list(features)

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

@feature('win', 'linux', 'osx', 'debug', 'release', 'com', 'pin', 'network')
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

@feature('cs', 'msbuild')
@before_method('apply_cs', 'apply_mbuild')
def cs_helpers(self):
	# set self.gen based off self.name since they are usually the same
	if not getattr(self, 'gen', None):
		setattr(self, 'gen', self.name)

	# ensure all binaries get chmod 755
	setattr(self, 'chmod', Utils.O755)

	# add optional csflags
	csflags = getattr(self, 'csflags', [])
	if csflags:
		self.env.append_value('CSFLAGS', csflags)

	# ensure the appropriate platform is being set on the command line
	setattr(self, 'platform', self.env.CSPLATFORM)

	# ensure install_path is set
	if not getattr(self, 'install_path', None):
		setattr(self, 'install_path', '${BINDIR}')

@feature('cs')
@after_method('apply_cs')
def cs_resource(self):
	base = getattr(self, 'namespace', os.path.splitext(self.gen)[0])

	if getattr(self, 'unsafe', False):
		self.env.append_value('CSFLAGS', ['/unsafe+'])

	keyfile = self.to_nodes(getattr(self, 'keyfile', []))
	self.cs_task.dep_nodes.extend(keyfile)
	if keyfile:
		self.env.append_value('CSFLAGS', '/keyfile:%s' % (keyfile[0].abspath()))

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

	if 'exe' in self.cs_task.env.CSTYPE:
		# if this is an exe, require app.config and install to ${BINDIR}
		cfg = self.path.find_or_declare('app.config')
	else:
		# if this is an assembly, app.config is optional
		cfg = self.path.find_resource('app.config')

	if cfg:
		inst_to = getattr(self, 'install_path', '${BINDIR}')
		self.bld.install_as('%s/%s.config' % (inst_to, self.gen), cfg, env=self.env, chmod=Utils.O755)

@conf
def clone_env(self, variant):
	env = self.all_envs.get(variant, None)
	if env is None:
		return None
	copy = env.derive()
	copy.PREFIX = self.env.PREFIX
	copy.BINDIR = self.env.BINDIR
	copy.LIBDIR = self.env.LIBDIR
	copy.DOCDIR = self.env.DOCDIR
	return copy

@conf
def ensure_version(self, tool, ver_exp):
	ver_exp = Utils.to_list(ver_exp)
	env = self.env
	environ = dict(self.environ)
	environ.update(PATH = ';'.join(env['PATH']))
	cmd = self.cmd_to_list(env[tool])
	(out,err) = self.cmd_and_log(cmd + ['/help'], env=environ, output=Context.BOTH)
	exe = os.path.split(cmd[0])[1].lower()
	ver_re = re.compile('.*ersion (\d+\.\d+\.\d+\.\d+)')
	m = ver_re.match(out)
	if not m:
		m = ver_re.match(err)
	if not m:
		raise Errors.WafError("Could not verify version of %s" % (exe))
	ver = m.group(1)
	found = False
	for v in ver_exp:
		found = ver.startswith(v) or found
	if not found:
		raise Errors.WafError("Requires %s %s but found version %s" % (exe, ver_exp, ver))

@feature('emit')
@before_method('process_rule')
def apply_emit(self):
	self.env.EMIT_SOURCE = self.source
	self.source = []
	self.meths.remove('process_source')
	outputs = [ self.path.find_or_declare(self.target) ]
	self.create_task('emit', None, outputs)

class emit(Task.Task):
	color = 'PINK'

	vars = [ 'EMIT_SOURCE' ]

	def run(self):
		text = self.env['EMIT_SOURCE']
		self.outputs[0].write(text)
