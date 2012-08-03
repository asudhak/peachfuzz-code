from waflib import Utils
from waflib.TaskGen import feature

archs = [ 'x86', 'x86_64' ]

tools = [
	'gcc',
	'gxx',
	'cs',
	'resx',
	'csprogram',
	'utils',
	'test',
]

def prepare(conf):
	env = conf.env

	env['MCS']  = 'dmcs'
	env['CC']   = 'gcc-4.6'
	env['CXX']  = 'g++-4.6'

	env['ARCH']    = ['-m%s' % ('64' in env.SUBARCH and '64' or '32')]

def configure(conf):
	env = conf.env

	env.append_value('supported_features', [
		'linux',
		'c',
		'cstlib',
		'cshlib',
		'cprogram',
		'cxx',
		'cxxstlib',
		'cxxshlib',
		'cxxprogram',
		'fake_lib',
		'cs',
		'csprogram',
		'test',
	])

	env['ARCH_ST'] = env['ARCH']

	env.append_value('CSFLAGS', [
		'/warn:4',
		'/define:PEACH',
	])

	env['CSPLATFORM'] = 'anycpu'

	cflags = [
		'-pipe',
		'-Werror',
		'-Wno-unused',
	]
	
	env.append_value('CFLAGS', cflags)
	env.append_value('CXXFLAGS', cflags)

	env.append_value('LIB', [ 'dl' ])

	return [ 'debug', 'release' ]

def debug(env):
	env.CSDEBUG = 'full'

	cflags = [
		'-ggdb',
	]

	env.append_value('CSFLAGS', ['/define:DEBUG,TRACE'])
	env.append_value('DEFINES', ['DEBUG'])
	env.append_value('CFLAGS', cflags)
	env.append_value('CXXFLAGS', cflags)

def release(env):
	env.CSDEBUG = 'pdbonly'

	cflags = [
		'-O3',
	]

	env.append_value('CSFLAGS', ['/define:TRACE', '/optimize+'])
	env.append_value('CFLAGS', cflags)
	env.append_value('CXXFLAGS', cflags)
