#!/usr/bin/env python

import os.path
from waflib.TaskGen import feature, after_method, before_method
from waflib import Utils, Logs, Configure, Context, Options, Errors

out = 'slag'
inst = 'output'

hosts = [ 'win', 'linux', 'osx', 'foo' ]

def options(opt):
	opt.add_option('--variant',
	               action = 'store',
	               default = None,
	               help = 'Specifies the variant to build against')

def init(ctx):
	if Logs.verbose == 0:
		def null_msg(self, msg, result, color=None):
			pass
		setattr(Configure.ConfigurationContext, 'msg', null_msg)

def configure(ctx):
	if Logs.verbose == 0:
		def null_fatal(self, msg, ex=None):
			raise self.errors.ConfigurationError(msg, ex)
		setattr(Configure.ConfigurationContext, 'fatal', null_fatal)

	base_env = ctx.env;
	base_env.BINDIR = base_env.LIBDIR = 'output'

	for host in hosts:
		try:
			config = Context.load_tool(host, ['build/config'])
			ctx.msg("Loading '%s' config" % host, config.__file__)
		except:
			ctx.msg("Loading '%s' config" % host, 'not found', color='YELLOW')
			continue
			
		archs = getattr(config, 'archs', None)
		options = [ ('%s_%s' % (host, arch), arch) for arch in archs ] or [ (host, None) ]

		for (name, arch) in options:
			if Logs.verbose == 0:
				Logs.pprint('NORMAL', 'Configuring variant %s :' % name.ljust(20), sep='')

			try:
				ctx.setenv(name, env=base_env)
				arch_env = ctx.get_env()
				arch_env.SUBARCH = arch;
				arch_env.BINDIR = os.path.join(base_env.BINDIR, name)
				arch_env.LIBDIR = os.path.join(base_env.LIBDIR, name)
				config.prepare(ctx)
				
				for tool in getattr(config, 'tools', []):
					ctx.load(tool, ['build/tools'])
				
				cfgs = config.configure(ctx)
				
				if not cfgs:
					base_env.append_value('variants', name)

				for cfg in cfgs:
					variant = '%s_%s' % (name, cfg)
					ctx.setenv(variant, env=arch_env)
					cfg_env = ctx.get_env()
					cfg_env.BINDIR = os.path.join(base_env.BINDIR, variant, 'bin')
					cfg_env.LIBDIR = os.path.join(base_env.LIBDIR, variant, 'bin')
					cfg_func = getattr(config, cfg)
					cfg_func(cfg_env)
					base_env.append_value('variants', variant)
					
				if Logs.verbose == 0:
					Logs.pprint('GREEN', 'Available')
				
			except Exception, e:
				if Logs.verbose == 0:
					Logs.pprint('YELLOW', 'Not Available')
				else:
					Logs.warn('%s is not available: %s' % (name, e))

def build(bld):
	subdirs = [ str(x.parent) for x in bld.path.ant_glob('*/wscript_build', maxdepth=1) ]
	what = Options.options.variant or ''
	variants = what.split(',')

	success = False
	for opt in variants:
		for variant in bld.env.variants:
			if opt not in variant:
				continue

			print 'Running: %s' % variant
			ctx = Context.create_context(bld.cmd)
			ctx.cmd = bld.cmd
			ctx.fun = 'go'
			ctx.subdirs = subdirs
			ctx.options = Options.options
			ctx.variant = variant
			ctx.execute()
			success = True

	if not success:
		raise Errors.WafError('"%s" is not a supported variant' % what)

def go(bld):
	bld.fun = 'build'
	bld.recurse(bld.subdirs)

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
