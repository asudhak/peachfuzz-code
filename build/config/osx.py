from waflib import Utils

archs = [ ]
tools = [
	'gcc',
	'gxx',
	'cs',
	'resx',
	'csprogram',
]

def prepare(conf):
	env = conf.env

	env['TOOLCHAIN_PATH'] = [
		'/Library/Frameworks/Mono.framework/Commands',
		'/Developer/usr/bin',
	]

	env['TOOLCHAIN_LIBS'] = None
	env['TOOLCHAIN_INCS'] = None

	env['TOOLCHAIN_MCS']  = 'dmcs'
	env['TOOLCHAIN_CC']   = 'llvm-gcc-4.2'
	env['TOOLCHAIN_CXX']  = 'llvm-g++-4.2'

def configure(conf):
	env = conf.env

	env.append_value('CSFLAGS', [
		'/warn:4',
		'/define:PEACH',
	])

	env['CSPLATFORM'] = 'anycpu'
	
	arch_flags = [
		'-mmacosx-version-min=10.6',
		'-isysroot',
		'/Developer/SDKs/MacOSX10.6.sdk',
		'-arch',
		'i386',
		'-arch',
		'x86_64',
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
