from waflib import Utils, Errors
from waflib.TaskGen import feature
import os.path

archs = [ ]

tools = [
	'gcc',
	'gxx',
	'cs',
	'resx',
	'csprogram',
	'utils',
	'externals',
	'test',
	'version',
]

def find_directory(dirname, paths):
	for path in paths:
		candidate = os.path.join(path, dirname)
		if os.path.exists(candidate):
			return candidate
	raise Errors.WafError('Could not find directory \'%s\'' % dirname)

def prepare(conf):
	env = conf.env

	env['PATH'] = [
		'/Library/Frameworks/Mono.framework/Commands',
		'/usr/bin',
	]

	env['MCS']  = 'dmcs'
	env['CC']   = 'llvm-gcc-4.2'
	env['CXX']  = 'llvm-g++-4.2'

	env['SYSROOT'] = find_directory('MacOSX10.6.sdk', [
		'/Developer/SDKs',
		'/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs',
	])

def configure(conf):
	env = conf.env

	env.supported_features = [
		'osx',
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
		'test',
	]

	env.append_value('CSFLAGS', [
		'/warn:4',
		'/define:PEACH',
	])

	env['CSPLATFORM'] = 'anycpu'
	
	arch_flags = [
		'-mmacosx-version-min=10.6',
		'-isysroot',
		env.SYSROOT,
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
