from waflib import Utils, Errors
from waflib.TaskGen import feature
import os.path

host_plat = [ 'darwin' ]

archs = [ ]

tools = [
	'gcc',
	'gxx',
	'cs',
	'resx',
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
	root = conf.path.abspath()
	env = conf.env
	j = os.path.join

	env['PATH'] = [
		'/Library/Frameworks/Mono.framework/Commands',
		'/usr/bin',
	]

	env['MCS']  = 'dmcs'
	env['CC']   = 'clang'
	env['CXX']  = 'clang++'

	env['SYSROOT'] = find_directory('MacOSX10.7.sdk', [
		'/Developer/SDKs',
		'/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs',
	])

	pin = j(root, '3rdParty', 'pin', 'pin-2.12-54730-clang.3.0-mac')

	env['EXTERNALS'] = {
		'pin' : {
			'INCLUDES'  : [
				j(pin, 'source', 'include'),
				j(pin, 'source', 'include', 'gen'),
				j(pin, 'extras', 'components', 'include'),
			],
			'HEADERS'   : [ 'pin.h' ],
			'DEFINES'   : [ 'BIGARRAY_MULTIPLIER=1', 'TARGET_MAC', 'USING_XED', ],
			'CPPFLAGS'  : [
				'-Xarch_i386',   '-DTARGET_IA32',
				'-Xarch_i386',   '-DHOST_IA32',
				'-Xarch_i386',   '-I%s' % j(pin, 'extras', 'xed2-ia32', 'include'),

				'-Xarch_x86_64', '-DTARGET_IA32E',
				'-Xarch_x86_64', '-DHOST_IA32E',
				'-Xarch_x86_64', '-I%s' % j(pin, 'extras', 'xed2-intel64', 'include'),
			],
			'LINKFLAGS' : [
				'-Xarch_i386',   '-L%s' % j(pin, 'ia32', 'lib'),
				'-Xarch_i386',   '-L%s' % j(pin, 'ia32', 'lib-ext'),
				'-Xarch_i386',   '-L%s' % j(pin, 'extras', 'xed2-ia32', 'lib'),
				'-Xarch_i386',   '-l-lpin',
				'-Xarch_i386',   '-l-lxed',

				'-Xarch_x86_64', '-L%s' % j(pin, 'intel64', 'lib'),
				'-Xarch_x86_64', '-L%s' % j(pin, 'intel64', 'lib-ext'),
				'-Xarch_x86_64', '-L%s' % j(pin, 'extras', 'xed2-intel64', 'lib'),
				'-Xarch_x86_64', '-l-lpin',
				'-Xarch_x86_64', '-l-lxed',
			],
		},
	}

def configure(conf):
	env = conf.env

	env.append_value('supported_features', [
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
		'debug',
		'release',
	])

	env.append_value('CSFLAGS', [
		'/warn:4',
		'/define:PEACH,UNIX,MONO',
	])

	env.append_value('CSFLAGS_debug', [
		'/define:DEBUG,TRACE',
	])

	env.append_value('CSFLAGS_release', [
		'/define:TRACE',
		'/optimize+',
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

	cppflags = [
		'-pipe',
		'-Werror',
		'-Wno-unused',
	]

	cppflags_debug = [
		'-g',
	]

	cppflags_release = [
		'-O3',
	]

	env.append_value('CPPFLAGS', arch_flags + cppflags)
	env.append_value('CPPFLAGS_debug', cppflags_debug)
	env.append_value('CPPFLAGS_release', cppflags_release)

	env.append_value('LINKFLAGS', arch_flags)

	env.append_value('DEFINES_debug', ['DEBUG'])

	# Override g++ darwin defaults in tools/gxx.py
	env['CXXFLAGS_cxxshlib'] = [ '-fPIC' ]

	env['VARIANTS'] = [ 'debug', 'release' ]

def debug(env):
	env.CSDEBUG = 'full'

def release(env):
	env.CSDEBUG = 'pdbonly'
