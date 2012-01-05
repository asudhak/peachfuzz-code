#!/usr/bin/env python
# encoding: utf-8
# Jérôme Carretero, 2012 (zougloub)

from waflib import Configure, Options, Utils, Task, TaskGen
from waflib.Tools import c, ccroot, c_preproc
from waflib.Configure import conf
from waflib.TaskGen import feature, before_method, taskgen_method

import os

opj = os.path.join

@conf
def find_ticc(conf):
	cc = conf.find_program(['cl6x'], var='CC', path_list=opj(getattr(Options.options, 'ti-cgt-dir', ""), 'bin'))
	cc = conf.cmd_to_list(cc)
	conf.env.CC_NAME = 'ticc'
	conf.env.CC = cc

@conf
def find_tild(conf):
	ld = conf.find_program(['lnk6x'], var='LINK_CC', path_list=opj(getattr(Options.options, 'ti-cgt-dir', ""), 'bin'))
	ld = conf.cmd_to_list(ld)
	conf.env.LINK_CC_NAME = 'tild'
	conf.env.LINK_CC = ld

@conf
def find_tiar(conf):
	ar = conf.find_program(['ar6x'], var='AR', path_list=opj(getattr(Options.options, 'ti-cgt-dir', ""), 'bin'))
	ar = conf.cmd_to_list(ar)
	conf.env.AR = ar
	conf.env.AR_NAME = 'tiar'
	conf.env.ARFLAGS = 'rcs'

@conf
def ticc_common_flags(conf):
	v = conf.env

	if not v['LINK_CC']: v['LINK_CC'] = v['CC']
	v['CCLNK_SRC_F']	 = []
	v['CCLNK_TGT_F']	 = ['-o']
	v['CPPPATH_ST']	  = '-I%s'
	v['DEFINES_ST']	  = '-d%s'

	v['LIB_ST']	      = '-l%s' # template for adding libs
	v['LIBPATH_ST']	  = '-i%s' # template for adding libpaths
	v['STLIB_ST']	    = '-l%s'
	v['STLIBPATH_ST']	= '-i%s'

	# program
	v['cprogram_PATTERN']    = '%s.out'

	# static lib
	#v['LINKFLAGS_cstlib']    = ['-Wl,-Bstatic']
	v['cstlib_PATTERN']      = 'lib%s.a'

def configure(conf):
	v = conf.env
	v.TI_CGT_DIR = getattr(Options.options, 'ti-cgt-dir', "")
	v.TI_DSPLINK_DIR = getattr(Options.options, 'ti-dsplink-dir', "")
	v.TI_BIOSUTILS_DIR = getattr(Options.options, 'ti-biosutils-dir', "")
	v.TI_DSPBIOS_DIR = getattr(Options.options, 'ti-dspbios-dir', "")
	v.TI_XDCTOOLS_DIR = getattr(Options.options, 'ti-xdctools-dir', "")
	conf.find_ticc()
	conf.find_tiar()
	conf.find_tild()
	conf.ticc_common_flags()
	conf.cc_load_tools()
	conf.cc_add_flags()
	conf.link_add_flags()
	v.TCONF = conf.cmd_to_list(conf.find_program(['tconf'], var='TCONF', path_list=v.TI_XDCTOOLS_DIR))

class ti_tconf(Task.Task):
	run_str = '${TCONF} ${TCONFINC} ${TCONFPROGNAME} ${SRC[0].bldpath()} ${PROCID}'
	color   = 'PINK'

	def scan(self):
		nodes = self.inputs
		names = []
		return (nodes, names)
		

@feature("ti-tconf")
@before_method('process_source')
def apply_tconf(self):
	bld = self.bld
	sources = [x.get_src() for x in self.to_nodes(self.source, path=bld.path.get_src())]
	node = sources[0]
	target = getattr(self, 'target', sources[0].name)

	# TODO prevent directory creation when using relative paths
	importpaths = []
	for x in getattr(self, 'includes', []):
		importpaths.append(bld.path.find_node(x).bldpath())

	task = self.create_task('ti_tconf', sources, node.change_ext('.cdb'))
	task.env = self.env
	task.env["TCONFINC"] = '-Dconfig.importPath=%s' % ";".join(importpaths)
	task.env['TCONFPROGNAME'] = '-Dconfig.programName=%s' % node.get_bld().change_ext("").bldpath()
	task.env['PROCID'] = '0'
	task.outputs = [node.change_ext("cfg_c.c"), node.change_ext("cfg.s62"), node.change_ext("cfg.cmd")]

	s62task = create_compiled_task(self, 'c', task.outputs[1])
	ctask = create_compiled_task(self, 'c', task.outputs[0])
	ctask.env.LINKFLAGS += [node.change_ext("cfg.cmd").abspath()]

	self.source = []

def options(opt):
	opt.add_option('--with-ti-cgt', type='string', dest='ti-cgt-dir', help = 'Specify alternate cgt root folder', default="")
	opt.add_option('--with-ti-biosutils', type='string', dest='ti-biosutils-dir', help = 'Specify alternate biosutils folder', default="")
	opt.add_option('--with-ti-dspbios', type='string', dest='ti-dspbios-dir', help = 'Specify alternate dspbios folder', default="")
	opt.add_option('--with-ti-dsplink', type='string', dest='ti-dsplink-dir', help = 'Specify alternate dsplink folder', default="")
	opt.add_option('--with-ti-xdctools', type='string', dest='ti-xdctools-dir', help = 'Specify alternate xdctools folder', default="")

@taskgen_method
def create_compiled_task(self, name, node):
	out = '%s' % (node.change_ext('.obj').name)
	task = self.create_task(name, node, node.parent.find_or_declare(out))
	self.env.OUT = '-fr%s' % (node.parent.get_bld().abspath())
	try:
		self.compiled_tasks.append(task)
	except AttributeError:
		self.compiled_tasks = [task]
	return task

ccroot.create_compiled_task = create_compiled_task

def hack():
	t = Task.classes['c']
	t.run_str = '${CC} ${ARCH_ST:ARCH} ${CFLAGS} ${CPPFLAGS} ${FRAMEWORKPATH_ST:FRAMEWORKPATH} ${CPPPATH_ST:INCPATHS} ${DEFINES_ST:DEFINES} ${SRC} -c ${OUT}'
	(f,dvars) = Task.compile_fun(t.run_str, t.shell)
	t.hcode = t.run_str
	t.run = f
	t.vars.extend(dvars)

hack()

