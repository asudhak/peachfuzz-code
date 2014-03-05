import os.path
from waflib import Utils
from waflib.TaskGen import feature

host_plat = [ 'linux' ]

archs = [ 'x86', 'x86_64' ]

tools = [
	'gcc',
	'gxx',
	'cs',
	'resx',
	'misc',
	'tools.utils',
	'tools.externals',
	'tools.test',
	'tools.version',
	'tools.xcompile',
	'tools.mdoc',
	'tools.zip',
]

def prepare(conf):
	env = conf.env
	j = os.path.join

	env['MCS']  = 'dmcs'
	env['CC']   = 'gcc'
	env['CXX']  = 'g++'

	if env.SUBARCH == 'x86':
		env['ARCH'] = [ '32' ]
		pin_tgt = 'ia32'
		pin_def = 'IA32'
	else:
		env['ARCH'] = [ '64' ]
		pin_tgt = 'intel64'
		pin_def = 'IA32E'

	env['ARCH_ST'] = '-m%s'

	env['PIN_VER'] = 'pin-2.13-61206-gcc.4.4.7-linux'

	pin = j(conf.get_peach_dir(), '3rdParty', 'pin', env['PIN_VER'])

	env['EXTERNALS'] = {
		'pin' : {
			'INCLUDES'  : [
				j(pin, 'source', 'include', 'pin'),
				j(pin, 'source', 'include', 'pin', 'gen'),
				j(pin, 'extras', 'components', 'include'),
				j(pin, 'extras', 'xed2-%s' % pin_tgt, 'include'),
			],
			'HEADERS'   : [],
			'STLIBPATH' : [
				j(pin, pin_tgt, 'lib'),
				j(pin, pin_tgt, 'lib-ext'),
				j(pin, 'extras', 'xed2-%s' % pin_tgt, 'lib'),
			],
			'LIBPATH'   : [],
			'LIB'       : [],
			'STLIB'     : [ 'pin', 'xed', 'dwarf', 'elf' ],
			'DEFINES'   : [ 'BIGARRAY_MULTIPLIER=1', 'TARGET_LINUX', 'TARGET_%s' % pin_def, 'HOST_%s' % pin_def, 'USING_XED', ],
			'CFLAGS'    : [],
			'CXXFLAGS'  : [ '-fno-stack-protector', '-fomit-frame-pointer', '-fno-strict-aliasing' ],
			'LINKFLAGS' : [],
			'ENV'       : { 'STLIB_MARKER' : '-Wl,-Bsymbolic', 'SHLIB_MARKER' : '-Wl,-Bsymbolic', 'cxxshlib_PATTERN' : '%s.so' },
		},
	}

	env['TARGET_FRAMEWORK'] = 'v4.0'
	env['TARGET_FRAMEWORK_NAME'] = '.NET Framework 4'

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
		'debug',
		'release',
		'emit',
		'vnum',
		'subst',
		'network',
	])

def configure(conf):
	env = conf.env

	env.append_value('CSFLAGS', [
		'/warn:4',
		'/define:PEACH,UNIX,MONO',
		'/warnaserror',
		'/nowarn:1591', # Missing XML comment for publicly visible type
	])

	env.append_value('CSFLAGS_debug', [
		'/define:DEBUG;TRACE;MONO',
	])

	env.append_value('CSFLAGS_release', [
		'/define:TRACE;MONO',
		'/optimize+',
	])

	env['CSPLATFORM'] = 'AnyCPU'
	env['CSDOC'] = True

	env.append_value('DEFINES_debug', [
		'DEBUG',
	])

	cppflags = [
		'-pipe',
		'-Werror',
		'-Wno-unused',
	]

	cppflags_debug = [
		'-ggdb',
	]

	cppflags_release = [
		'-O3',
	]

	env.append_value('CPPFLAGS', cppflags)
	env.append_value('CPPFLAGS_debug', cppflags_debug)
	env.append_value('CPPFLAGS_release', cppflags_release)

	env.append_value('LIB', [ 'dl' ])

	env['VARIANTS'] = [ 'debug', 'release' ]

def debug(env):
	env.CSDEBUG = 'full'

def release(env):
	env.CSDEBUG = 'pdbonly'
