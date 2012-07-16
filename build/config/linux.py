from waflib import Utils

archs = [ 'x86', 'x86_64' ]
tools = [
	'gcc',
	'gxx',
	'cs',
	'resx',
	'csprogram',
]

def prepare(conf):
	env = conf.env

	env['TOOLCHAIN_PATH'] = None
	env['TOOLCHAIN_LIBS'] = None
	env['TOOLCHAIN_INCS'] = None

	env['TOOLCHAIN_MCS']  = 'dmcs'

def configure(conf):
	env = conf.env

	env.append_value('CSFLAGS', [
		'/warn:4',
		'/define:PEACH',
	])

	env['CSPLATFORM'] = 'anycpu'

	arch_flags = [
		'-m%s' % ('64' in env.SUBARCH and '64' or '32'),
	]

	cflags = [
		'-pipe',
		'-Werror',
		'-Wno-unused',
	]
	
	env.append_value('CFLAGS', arch_flags + cflags)
	env.append_value('CXXFLAGS', arch_flags + cflags)
	env.append_value('LINKFLAGS', arch_flags)
	
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
