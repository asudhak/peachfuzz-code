from waflib import Build
from waflib.Configure import ConfigurationContext

def apply_hook(cls, name, fun):
	old = getattr(cls, name, None)
	if old:
		setattr(cls, 'base_%s' % name, old)
	setattr(cls, name, fun)

def hook(f):
	def fun(*k, **kw):
		return f(*k, **kw)

	apply_hook(ConfigurationContext, f.__name__, fun)
	apply_hook(Build.BuildContext, f.__name__, fun)

	return f

@hook
def find_program(self, filename, **kw):
	path_list = kw.get('path_list', None)
	if not path_list:
		kw['path_list'] = self.env['TOOLCHAIN_PATH']

	var = kw.get('var', '')
	filename = self.env['TOOLCHAIN_%s' % var] or filename

	return self.base_find_program(filename, **kw)
