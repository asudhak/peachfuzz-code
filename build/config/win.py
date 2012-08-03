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
        env = conf.env

        env['MSVC_VERSIONS'] = ['msvc 10.0']
        env['MSVC_TARGETS']  = [ env.SUBARCH ]

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
