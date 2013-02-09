#!/usr/bin/env python

import os.path, re
from optparse import OptionValueError
from waflib.TaskGen import feature, after_method, before_method
from waflib.Build import InstallContext
from waflib import Utils, Logs, Configure, Context, Options, Errors
from tools import pkg, hooks

out = 'slag'
inst = 'output'
appname = 'peach'

targets = [ 'win', 'linux', 'osx', 'foo' ]


class TestContext(InstallContext):
	cmd = 'test'

	def __init__(self, **kw):
		super(TestContext, self).__init__(**kw)
		self.is_test = True

def store_version(option, opt, value, parser):
	if not re.match('^\d+\.\d+\.\d+(\.\d+)?$', value):
		raise OptionValueError('%s option is not valid - must be <int>.<int>.<int>' % opt)
	setattr(parser.values, option.dest, value)

def options(opt):
	opt.add_option('--variant',
	               action = 'store',
	               default = None,
	               help = 'Specifies the variant to build against')
	opt.add_option('--buildtag',
	               action = 'callback',
	               callback = store_version,
	               type = 'string',
	               default = '0.0.0',
	               help = 'Specifies the buildtag to embed in the binaries')


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

	base_env = ctx.env
	base_env.APPNAME = appname
	base_env.OUTPUT = base_env.PREFIX = base_env.BINDIR = base_env.LIBDIR = inst
	base_env.BUILDTAG = Options.options.buildtag

	tool_dir =  [
		os.path.join(ctx.path.abspath(), 'build', 'tools'),
		os.path.join(Context.waf_dir, 'waflib', 'Tools'),
		os.path.join(Context.waf_dir, 'waflib', 'extras'),
	]

	platform = Utils.unversioned_sys_platform()

	for tgt in targets:
		try:
			config = Context.load_tool(tgt, [os.path.join('build', 'config')])
			ctx.msg("Loading '%s' config" % tgt, config.__file__)
		except:
			ctx.msg("Loading '%s' config" % tgt, 'not found', color='YELLOW')
			continue

		platforms = getattr(config, 'host_plat', [])
		archs = getattr(config, 'archs', None)
		options = [ ('%s_%s' % (tgt, arch), arch) for arch in archs ] or [ (tgt, None) ]

		for (name, arch) in options:
			if Logs.verbose == 0:
				Logs.pprint('NORMAL', 'Configuring variant %s :' % name.ljust(20), sep='')

			try:
				if platform not in platforms:
					raise Exception('Unsupported build host')

				ctx.setenv(name, env=base_env)
				arch_env = ctx.get_env()
				arch_env.BASENAME = name;
				arch_env.TARGET = tgt;
				arch_env.SUBARCH = arch;
				arch_env.PREFIX = os.path.join(base_env.PREFIX, name)
				arch_env.BINDIR = os.path.join(base_env.BINDIR, name)
				arch_env.LIBDIR = os.path.join(base_env.LIBDIR, name)
				config.prepare(ctx)
				
				for tool in getattr(config, 'tools', []):
					ctx.load(tool, tool_dir)

				config.configure(ctx)
				cfgs = ctx.env.VARIANTS

				if not cfgs:
					base_env.append_value('variants', name)

				for cfg in cfgs:
					variant = '%s_%s' % (name, cfg)
					ctx.setenv(variant, env=arch_env)
					cfg_env = ctx.get_env()
					cfg_env.PREFIX = os.path.join(base_env.BINDIR, variant)
					cfg_env.BINDIR = os.path.join(base_env.BINDIR, variant, 'bin')
					cfg_env.LIBDIR = os.path.join(base_env.LIBDIR, variant, 'bin')
					cfg_env.VARIANT = cfg
					cfg_func = getattr(config, cfg)
					cfg_func(cfg_env)
					base_env.append_value('variants', variant)
					
				if Logs.verbose == 0:
					Logs.pprint('GREEN', 'Available')
				
			except Exception, e:
				if Logs.verbose == 0:
					Logs.pprint('YELLOW', 'Not Available - %s' % e)
				else:
					if Logs.verbose > 1:
						import traceback
						traceback.print_exc()
					Logs.warn('%s is not available: %s' % (name, e))


def build(bld):
	subdirs = [ x.parent.nice_path() for x in bld.path.ant_glob('**/wscript_build', maxdepth=1 ) ]
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

	# Suppress missing target warnings
	bld.targets = '*'

def go(bld):
	bld.fun = 'build'
	bld.recurse(bld.subdirs)


