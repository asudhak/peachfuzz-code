from waflib.Build import InstallContext
from waflib.TaskGen import before_method, after_method, feature, extension
from waflib import Task, Utils, Logs, Configure, Context, Options, Errors
from tools.pkg import PkgTask
from waflib.Tools import ccroot

import os
import os.path

#USELIB_VARS['c']        = set(['INCLUDES', 'FRAMEWORKPATH', 'DEFINES', 'CPPFLAGS', 'CCDEPS', 'CFLAGS', 'ARCH'])
#USELIB_VARS['cprogram'] = USELIB_VARS['cxxprogram'] = set(['LIB', 'STLIB', 'LIBPATH', 'STLIBPATH', 'LINKFLAGS', 'RPATH', 'LINKDEPS', 'FRAMEWORK', 'FRAMEWORKPATH', 'ARCH'])

def configure(conf):
	v = conf.env

	v['CANDLE_FLAGS'] = [ '-nologo' ]
	v['LIGHT_FLAGS'] = [ '-nologo' ]

	try:
		pfiles = os.getenv('PROGRAMFILES(X86)', os.getenv('PROGRAMFILES'))
		wix_path = os.path.join(pfiles, 'WiX Toolset v3.8', 'bin')
		conf.find_program('candle', var='CANDLE', exts='.exe', path_list=[wix_path])
		conf.find_program('light', var='LIGHT', exts='.exe', path_list=[wix_path])
		conf.find_program('heat', var='HEAT', exts='.exe', path_list=[wix_path])
		v.append_value('supported_features', 'msi')
	except Exception, e:
		v.append_value('missing_features', 'msi')
		if Logs.verbose > 0:
			Logs.warn('WiX toolset is not available: %s' % (e))

def wxs_scan(task):
	# Dep nodes, Unresolved names
	return ([], [])

class candle(PkgTask):
	"Compile wxs files into object files"
	#run_str = '${CC} ${ARCH_ST:ARCH} ${CFLAGS} ${CPPFLAGS} ${FRAMEWORKPATH_ST:FRAMEWORKPATH} ${CPPPATH_ST:INCPATHS} ${DEFINES_ST:DEFINES} ${CC_SRC_F}${SRC} ${CC_TGT_F}${TGT}'
	run_str = '${CANDLE} ${CANDLE_FLAGS} -out ${TGT} ${SRC}'
	vars    = ['CANDLE_DEPS'] # unused variable to depend on, just in case
	ext_in  = ['.wxs']
	scan    = wxs_scan

class light(PkgTask):
	"Link wixobj files into a msi installer"
	#run_str = '${LINK_CC} ${LINKFLAGS} ${CCLNK_SRC_F}${SRC} ${CCLNK_TGT_F}${TGT[0].abspath()} ${RPATH_ST:RPATH} ${FRAMEWORKPATH_ST:FRAMEWORKPATH} ${FRAMEWORK_ST:FRAMEWORK} ${ARCH_ST:ARCH} ${STLIB_MARKER} ${STLIBPATH_ST:STLIBPATH} ${STLIB_ST:STLIB} ${SHLIB_MARKER} ${LIBPATH_ST:LIBPATH} ${LIB_ST:LIB}'
	run_str = '${LIGHT} ${LIGHT_FLAGS} -out ${TGT[0].abspath()} ${SRC}'
	ext_out = ['.msi']
	vars    = ['LIGHT_DEPS']
	inst_to = '${PKGDIR}'
	chmod   = Utils.O755

@extension('.wxs')
def wxs_hook(self, node):
	ext_out = '.%d.wixobj' % (self.idx)
	task = self.create_task('candle', node, node.change_ext(ext_out))
	try:
		self.compiled_tasks.append(task)
	except AttributeError:
		self.compiled_tasks = [task]
	return task

@feature('msi')
@after_method('process_source')
def apply_msi(self):
	flags = []
	for ext in Utils.to_list(getattr(self, 'extensions', '')):
		flags.extend(['-ext', ext ])

	bindir = self.bld.srcnode.find_dir(self.env.BINDIR)

	flags.extend(['-b', 'BINDIR=%s' % bindir.abspath()])
	flags.extend(['-b', 'SRCDIR=%s' % self.path.abspath()])

	self.env.append_value('LIGHT_FLAGS', flags)

@feature('msi')
@after_method('process_source')
def apply_candle(self):
	objs = [t.outputs[0] for t in getattr(self, 'compiled_tasks', [])]
	self.link_task = self.create_task('light', objs)
	ext_out = self.link_task.__class__.ext_out[0]
	target = self.path.find_or_declare(self.target + ext_out)
	self.link_task.set_outputs(target)

	# remember that the install paths are given by the task generators
	try:
		inst_to = self.install_path
	except AttributeError:
		inst_to = self.link_task.__class__.inst_to
	if inst_to and getattr(self.bld, 'is_pkg', False):
		# install a copy of the node list we have at this moment (implib not added)
		self.install_task = self.bld.install_files(inst_to, self.link_task.outputs[:], env=self.env, chmod=self.link_task.chmod)
