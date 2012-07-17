#!/usr/bin/env python

import os.path
from waflib.TaskGen import feature, after_method, before_method
from waflib import Utils, Logs, Configure, Context, Options, Errors
import tools.hooks

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

	ctx.env.FILTER_STDOUT = True
	base_env = ctx.env;
	base_env.OUTDIR = base_env.BINDIR = base_env.LIBDIR = 'output'

	tool_dir =  [
		os.path.join(ctx.path.abspath(), 'build', 'tools'),
		os.path.join(Context.waf_dir, 'waflib', 'Tools'),
	]

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
				arch_env.OUTDIR = os.path.join(base_env.OUTDIR, name)
				arch_env.BINDIR = os.path.join(base_env.BINDIR, name)
				arch_env.LIBDIR = os.path.join(base_env.LIBDIR, name)
				config.prepare(ctx)
				
				for tool in getattr(config, 'tools', []):
					ctx.load(tool, tool_dir)
				
				cfgs = config.configure(ctx)
				
				if not cfgs:
					base_env.append_value('variants', name)

				for cfg in cfgs:
					variant = '%s_%s' % (name, cfg)
					ctx.setenv(variant, env=arch_env)
					cfg_env = ctx.get_env()
					cfg_env.OUTDIR = os.path.join(base_env.BINDIR, variant)
					cfg_env.BINDIR = os.path.join(base_env.BINDIR, variant, 'bin')
					cfg_env.LIBDIR = os.path.join(base_env.LIBDIR, variant, 'lib')
					cfg_func = getattr(config, cfg)
					cfg_func(cfg_env)
					base_env.append_value('variants', variant)
					
				if Logs.verbose == 0:
					Logs.pprint('GREEN', 'Available')
				
			except Exception, e:
				if Logs.verbose == 0:
					Logs.pprint('YELLOW', 'Not Available')
				else:
					if Logs.verbose > 1:
						import traceback
						traceback.print_exc()
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
