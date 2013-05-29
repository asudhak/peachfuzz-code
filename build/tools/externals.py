import os, os.path
from waflib import Logs, Task, Utils
from waflib.TaskGen import feature, before_method

def find_external(conf, filename, **kw):
	ret = None
	path_list = kw.get('path_list', '')
	fmt = kw.get('fmt', '%s')
	filename = fmt % filename

	if path_list:
		path_list = Utils.to_list(path_list)
	else:
		path_list = os.environ.get('PATH', '').split(os.pathsep)

	for d in path_list:
		f = os.path.expanduser(os.path.join(d, filename))
		if os.path.isfile(f):
			ret = f
			break

	conf.msg('Checking for file ' + filename, ret)
	conf.to_log('find external=%r paths=%r -> %r' % (filename, path_list, ret))

	if not ret:
		conf.fatal('Could not find the file %s' % filename)

def config_uselib(conf, var, name, ext):
	uselib = '%s_%s' % (var, name)
	value = ext.get(var)
	if value:
		conf.env[uselib] = value

def config_external(conf, name, ext):
	msvc = Utils.to_list(ext.get('MSVC', []))
	if msvc:
		ver = conf.env['MSVC_VERSION']
		conf.msg('Checking for msvc ' + str(msvc), ver)
		conf.to_log('msvc external=%r supported=%r -> %r' % (name, msvc, ver))
		if str(ver) not in msvc:
			conf.fatal('msvc version %s not in supported list of %s' % (ver, msvc))

	paths = ext.get('INCLUDES', conf.env['INCLUDES'])
	for x in ext.get('HEADERS', []):
		find_external(conf, x, path_list=paths)

	paths = ext.get('LIBPATH', conf.env['LIBPATH'])
	for x in ext.get('LIB', []):
		find_external(conf, x, path_list=paths, fmt=conf.env['cshlib_PATTERN'])

	paths = ext.get('STLIBPATH', conf.env['LIBPATH'])
	for x in ext.get('STLIB', []):
		find_external(conf, x, path_list=paths, fmt=conf.env['cstlib_PATTERN'])

	config_uselib(conf, 'INCLUDES', name, ext)
	config_uselib(conf, 'LIBPATH', name, ext)
	config_uselib(conf, 'STLIBPATH', name, ext)
	config_uselib(conf, 'LIB', name, ext)
	config_uselib(conf, 'STLIB', name, ext)
	config_uselib(conf, 'DEFINES', name, ext)
	config_uselib(conf, 'CFLAGS', name, ext)
	config_uselib(conf, 'CPPFLAGS', name, ext)
	config_uselib(conf, 'CXXFLAGS', name, ext)
	config_uselib(conf, 'LINKFLAGS', name, ext)

def configure(conf):
	exts = conf.env['EXTERNALS']
	for k in exts:
		try:
			config_external(conf, k, exts[k])
			conf.env.append_value('supported_features', [ k ])
		except Exception, e:
			conf.env.append_value('missing_features', [ k ])
			if Logs.verbose:
				Logs.warn('External library \'%s\' is not available: %s' % (k, e))

@feature('*')
@before_method('propagate_uselib_vars')
def apply_externals(self):
	exts = set([ k for k in self.env['EXTERNALS'] ])
	feat = set(self.to_list(getattr(self, 'features', [])))
	use = set(self.to_list(getattr(self, 'use', [])))
	use |= exts & feat
	setattr(self, 'use', [ x for x in use ])
