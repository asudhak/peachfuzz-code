#!/usr/bin/env python
# encoding: utf-8
# Brant Young, 2007

"Process *.rc* files for C/C++: X{.rc -> [.res|.rc.o]}"

from waflib import Task, Logs, Utils
from waflib.TaskGen import extension
from waflib.Tools import c_preproc
import re

@extension('.rc')
def rc_file(self, node):
	"""
	Bind the .rc extension to a winrc task
	"""
	obj_ext = '.rc.o'
	if self.env['WINRC_TGT_F'] == '/fo':
		obj_ext = '.res'
	rctask = self.create_task('winrc', node, node.change_ext(obj_ext))
	try:
		self.compiled_tasks.append(rctask)
	except AttributeError:
		self.compiled_tasks = [rctask]

re_lines = re.compile(
	'(?:^[ \t]*(#|%:)[ \t]*(ifdef|ifndef|if|else|elif|endif|include|import|define|undef|pragma)[ \t]*(.*?)\s*$)|'\
	'(?:^\w+[ \t]*(ICON|BITMAP|CURSOR|HTML|FONT|MESSAGETABLE|TYPELIB|REGISTRY|D3DFX)[ \t]*(.*?)\s*$)',
	re.IGNORECASE | re.MULTILINE)

class rc_parser(c_preproc.c_parser):
	def filter_comments(self, filepath):
		code = Utils.readf(filepath)
		if c_preproc.use_trigraphs:
			for (a, b) in c_preproc.trig_def: code = code.split(a).join(b)
		code = c_preproc.re_nl.sub('', code)
		code = c_preproc.re_cpp.sub(c_preproc.repl, code)
		ret = []
		for m in re.finditer(re_lines, code):
			if m.group(2):
				ret.append((m.group(2), m.group(3)))
			else:
				ret.append(('include', m.group(5)))
		return ret

class winrc(Task.Task):
	"""
	Task for compiling resource files
	"""
	run_str = '${WINRC} ${WINRCFLAGS} ${CPPPATH_ST:INCPATHS} ${DEFINES_ST:DEFINES} ${WINRC_TGT_F} ${TGT} ${WINRC_SRC_F} ${SRC}'
	color   = 'BLUE'

	def scan(self):
		tmp = rc_parser(self.generator.includes_nodes)
		tmp.start(self.inputs[0], self.env)
		nodes = tmp.nodes
		names = tmp.names

		if Logs.verbose:
			Logs.debug('deps: deps for %s: %r; unresolved %r' % (str(self), nodes, names))

		return (nodes, names)

def configure(conf):
	"""
	Detect the programs RC or windres, depending on the C/C++ compiler in use
	"""
	v = conf.env
	v['WINRC_TGT_F'] = '-o'
	v['WINRC_SRC_F'] = '-i'

	# find rc.exe
	if not conf.env.WINRC:
		if v.CC_NAME == 'msvc':
			conf.find_program('RC', var='WINRC', path_list = v['PATH'])
			v['WINRC_TGT_F'] = '/fo'
			v['WINRC_SRC_F'] = ''
		else:
			conf.find_program('windres', var='WINRC', path_list = v['PATH'])
	if not conf.env.WINRC:
		conf.fatal('winrc was not found!')

	v['WINRCFLAGS'] = []

