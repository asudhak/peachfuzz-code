import os, os.path, platform
from waflib import Utils, Errors
from waflib.TaskGen import feature

archs = [ 'x86', 'x64' ]

tools = [
	'msvc',
	'cs',
	'csprogram',
	'resx',
	'midl',
	'utils',
	'externals',
	'test',
]

def prepare(conf):
        root = conf.path.abspath()
        env = conf.env
        j = os.path.join

        env['MSVC_VERSIONS'] = ['msvc 10.0']
        env['MSVC_TARGETS']  = [ env.SUBARCH ]

        pin = j(root, '3rdParty', 'pin-msvc10-ia32_intel64-windows')

        env['EXTERNALS_x86'] = {
                'pin' : {
                        'INCLUDES'  : [
                                j(pin, 'source', 'include'),
                                j(pin, 'source', 'include', 'gen'),
                                j(pin, 'extras', 'components', 'include'),
                                j(pin, 'extras', 'xed2-ia32', 'include'),
                        ],
                        'STLIBPATH'   : [
                                j(pin, 'ia32', 'lib'),
                                j(pin, 'ia32', 'lib-ext'),
                                j(pin, 'extras', 'xed2-ia32', 'lib'),
                        ],
                        'STLIB'       : [ 'pin', 'ntdll-32', 'pinvm', 'libxed' ],
                        'DEFINES'   : [ 'BIGARRAY_MULTIPLIER=1', '_SECURE_SCL=0', 'TARGET_WINDOWS', 'TARGET_IA32', 'HOST_IA32', 'USING_XED', ],
                        'CFLAGS'    : [ '/MT' ],
                        'CXXFLAGS'  : [ '/MT' ],
                        'LINKFLAGS' : [ '/EXPORT:main', '/ENTRY:Ptrace_DllMainCRTStartup@12', '/BASE:0x55000000' ],
                },
        }

        env['EXTERNALS_x64'] = {
                'pin' : {
                        'INCLUDES'  : [
                                j(pin, 'source', 'include'),
                                j(pin, 'source', 'include', 'gen'),
                                j(pin, 'extras', 'components', 'include'),
                                j(pin, 'extras', 'xed2-intel64', 'include'),
                        ],
                        'STLIBPATH'   : [
                                j(pin, 'intel64', 'lib'),
                                j(pin, 'intel64', 'lib-ext'),
                                j(pin, 'extras', 'xed2-intel64', 'lib'),
                        ],
                        'STLIB'       : [ 'pin', 'ntdll-64', 'pinvm', 'libxed' ],
                        'DEFINES'   : [ 'BIGARRAY_MULTIPLIER=1', '_SECURE_SCL=0', 'TARGET_WINDOWS', 'TARGET_IA32E', 'HOST_IA32E', 'USING_XED', ],
                        'CFLAGS'    : [ '/MT' ],
                        'CXXFLAGS'  : [ '/MT' ],
                        'LINKFLAGS' : [ '/EXPORT:main', '/ENTRY:Ptrace_DllMainCRTStartup', '/BASE:0xC5000000' ],
                },
        }

        env['EXTERNALS'] = env['EXTERNALS_%s' % env.SUBARCH]

	# This is lame, the resgen that vcvars for x64 finds is the .net framework 3.5 version.
	# The .net 4 version is in the x86 search path.
	if env.SUBARCH == 'x64':
		env['RESGEN'] = 'c:\\Program Files (x86)\\Microsoft SDKs\\Windows\\v7.0A\\bin\\NETFX 4.0 Tools\\resgen.exe'

def configure(conf):
        env = conf.env
 
        env.append_value('supported_features', [
                'win',
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

        cflags = [
                '/nologo',
                '/W4',
                '/WX',
        ]

        env.append_value('CFLAGS', cflags)
        env.append_value('CXXFLAGS', cflags + [ '/EHsc' ])

        env.append_value('DEFINES', [
                'WIN32',
                '_CRT_SECURE_NO_WARNINGS',
        ])

        env.append_value('CSFLAGS', [
                '/noconfig',
                '/nologo',
                '/nostdlib+',
                '/warn:4',
                '/define:PEACH',
                '/errorreport:prompt',
        ])

        env.append_value('ASSEMBLIES', [
                'mscorlib.dll',
        ])

        env.append_value('LINKFLAGS', [
                '/NOLOGO',
                '/DEBUG',
                '/INCREMENTAL:NO',
                '/WX',
        ])

	env['CSPLATFORM'] = env.SUBARCH

        env.append_value('MIDLFLAGS', [
                '/%s' % ('x86' in env.SUBARCH and 'win32' or 'amd64'),
        ])

        return [ 'debug', 'release' ]

def debug(env):
        env.CSDEBUG = 'full'

        cflags = [
                '/MDd',
                '/Od',
        ]

        env.append_value('CFLAGS', cflags)
        env.append_value('CXXFLAGS', cflags)
        env.append_value('CSFLAGS', ['/define:DEBUG,TRACE'])
        env.append_value('DEFINES', ['DEBUG', '_DEBUG'])

def release(env):
        env.CSDEBUG = 'pdbonly'

        cflags = [
                '/MD',
                '/Ox',
        ]

        env.append_value('CFLAGS', cflags)
        env.append_value('CXXFLAGS', cflags)
        env.append_value('CSFLAGS', ['/define:TRACE', '/optimize+'])
