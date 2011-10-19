#!/usr/bin/env python
# encoding: utf-8
# Carlos Rafael Giani, 2007 (dv)
# Thomas Nagy, 2008-2010 (ita)

import sys
from waflib import Utils
from waflib.Tools import ar, d, ccroot
from waflib.Configure import conf

ccroot.USELIB_VARS['dstlib'].add('D2LINKFLAGS')

@conf
def find_dmd(conf):
	"""
	Find the program *dmd* or *ldc* and set the variable *D*
	"""
	conf.find_program(['dmd', 'ldc'], var='D')

@conf
def common_flags_ldc(conf):
	"""
	Set the D flags required by *ldc*
	"""
	v = conf.env
	v['DFLAGS']        = ['-d-version=Posix']
	v['LINKFLAGS']     = []
	v['DFLAGS_dshlib'] = ['-relocation-model=pic']

@conf
def common_flags_dmd(conf):
	"""
	Set the flags required by *dmd*
	"""

	v = conf.env

	# _DFLAGS _DIMPORTFLAGS

	# Compiler is dmd so 'gdc' part will be ignored, just
	# ensure key is there, so wscript can append flags to it
	#v['DFLAGS']            = ['-version=Posix']

	v['D_SRC_F']           = ['-c']
	v['D_TGT_F']           = '-of%s'

	# linker
	v['D_LINKER']          = v['D']
	v['DLNK_SRC_F']        = ''
	v['DLNK_TGT_F']        = '-of%s'
	v['DINC_ST']           = '-I%s'

	v['DSHLIB_MARKER'] = v['DSTLIB_MARKER'] = ''
	v['DSTLIB_ST'] = v['DSHLIB_ST']         = '-L-l%s'
	v['DSTLIBPATH_ST'] = v['DLIBPATH_ST']   = '-L-L%s'

	v['LINKFLAGS_dprogram']= ['-quiet']

	v['DFLAGS_dshlib']     = ['-fPIC']
	v['LINKFLAGS_dshlib']  = ['-L-shared']

	v['DHEADER_ext']       = '.di'
	v.DFLAGS_d_with_header = ['-H', '-Hf']
	v['D_HDR_F']           = '%s'

class d2program(ccroot.link_task):
	run_str = '${D} ${LINKFLAGS} ${D2LINKFLAGS} ${SRC} ${DLNK_TGT_F:TGT}'
	inst_to = '${BINDIR}'
	chmod   = Utils.O755

class d2shlib(d2program):
	"Link object files into a d shared library"
	inst_to = '${LIBDIR}'

class d2stlib(d2program):
	"Link object files into a d static library"
	inst_to = None

def configure(conf):
	"""
	Configuration for dmd/ldc
	"""
	conf.find_dmd()

	out = conf.cmd_and_log([conf.env.D, '--help'])
	if out.find("D Compiler v2.") > -1:
		conf.env.D2 = 1
		conf.env.DLNK_TGT_F = '-of%s'
		if Utils.destos_to_binfmt(conf.env.DEST_OS) == 'pe':
			conf.env['d2program_PATTERN'] = '%s.exe'
			conf.env['d2shlib_PATTERN']   = 'lib%s.dll'
			conf.env['d2stlib_PATTERN']   = 'lib%s.a'
		else:
			conf.env['d2program_PATTERN'] = '%s'
			conf.env['d2shlib_PATTERN']   = 'lib%s.so'
			conf.env['d2stlib_PATTERN']   = 'lib%s.a'

		conf.env.D2LINKFLAGS_dstlib = ['-lib']
	else:
		conf.load('ar')
		conf.load('d')
		conf.common_flags_dmd()
		conf.d_platform_flags()

		if str(conf.env.D).find('ldc') > -1:
			conf.common_flags_ldc()

