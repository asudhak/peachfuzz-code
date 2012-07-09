#!/usr/bin/env python

import os.path
from waflib.TaskGen import feature, after_method, before_method
from waflib import Utils, Logs, Configure

out = 'slag'

def init(ctx):
	if Logs.verbose == 0:
		def null_msg(self, msg, result, color=None):
			pass
		setattr(Configure.ConfigurationContext, 'msg', null_msg)

def configure(config):
	config.find_program(['csc', 'dmcs'], var='MCS')
	config.load('cs')
	config.load('resx', ['build/tools'])
	config.load('utils', ['build/tools'])

	config.env.append_value('CSFLAGS', '-define:PEACH')
	config.env.BINDIR = config.env.LIBDIR = 'output'

def build(bld):
	subdirs = [ str(x.parent) for x in bld.path.ant_glob('*/wscript_build', maxdepth=1) ]
	bld.recurse(subdirs)

#	bld(
#		features = 'cs',
#		#use = 'Peach.Core.Debuggers.Windows.dll',
#		source = bld.path.ant_glob('Peach.Core.Debuggers.Windows/**/*.cs'),
#		name='Peach.Core.Debuggers.Windows.dll',
#	)

#	bld(
#		features = 'cs',
#		use = 'Peach.Core.dll Peach.Core.Debuggers.Windows.dll System.Drawing.dll NLog.dll System.Runtime.Remoting.dll System.Management.dll System.ServiceProcess.dll',
#		source = bld.path.ant_glob('Peach.Core.OS.Windows/**/*.cs'),
#		name='Peach.Core.OS.Windows.dll',
#	)

#	bld(
#		features = 'cs',
#		use = 'NLog.dll nunit.framework.dll Peach.Core.dll Peach.Core.OS.OSX.dll Peach.Core.OS.Windows.dll Peach.Core.OS.Linux.dll',
#		source = bld.path.ant_glob('Peach.Core.Test/**/*.cs'),
#		name='Peach.Core.Test.dll',
#	)

#	bld(
#		features = 'cs',
#		use = 'Peach.Core.dll System.Windows.Forms.dll System.Data.dll System.Drawing.dll System.ServiceProcess.dll System.Xml.dll',
#		source = bld.path.ant_glob('PeachFuzzBang/**/*.cs'),
#		name='PeachFuzzBang.exe',
#	)
