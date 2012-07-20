from waflib import Task
from waflib.TaskGen import extension, feature, before_method
import os

def configure(conf):
	conf.find_program(['midl'], var='MIDL')

	conf.env.MIDLFLAGS = [
		'/nologo',
		'/D',
		'_DEBUG',
		'/W1',
		'/char',
		'signed',
		'/Oicf',
	]

@feature('c', 'cxx', 'cstlib', 'cshlib')
@before_method('process_source')
def idl_file(self):
	idl_nodes = []
	src_nodes = []
	for x in self.to_nodes(self.source):
		if x.name.endswith('.idl'):
			idl_nodes.append(x)
		else:
			src_nodes.append(x)

	for x in idl_nodes:
		base = x.name[0:-4]
		h = self.path.find_or_declare(base + '_i.h')
		c = self.path.find_or_declare(base + '_i.c')
		tsk = self.create_task('midl', x, [c, h])
		src_nodes.append(c)

	self.source = src_nodes

class midl(Task.Task):
	"""
	Compile idl files
	"""
	color   = 'YELLOW'
	run_str = '${MIDL} ${MIDLFLAGS} ${CPPPATH_ST:INCLUDES} /h ${TGT[1].abspath()} /iid ${TGT[0].abspath()} ${SRC}'

def exec_command_midl(self, *k, **kw):
	if self.env['PATH']:
		env = self.env.env or dict(os.environ)
		env.update(PATH = ';'.join(self.env['PATH']))
		kw['env'] = env

	bld = self.generator.bld

	try:
		if not kw.get('cwd', None):
			kw['cwd'] = bld.cwd
	except AttributeError:
		bld.cwd = kw['cwd'] = bld.variant_dir

	return bld.exec_command(k[0], **kw)

cls = Task.classes.get('midl', None)
cls.exec_command = exec_command_midl
