import os, os.path, platform
from waflib import Utils

archs = [ 'x86', 'x64' ]

tools = [
	'msvc',
	'csc',
	'resx',
	'utils',
]

def setup_paths(env, paths):
        return [ os.path.join(os.environ[env], x) for x in paths ]

def setup_pfiles(paths):
        ret = setup_paths('PROGRAMFILES', paths)
        if platform.architecture()[0] == '64bit':
                ret.extend( setup_paths('PROGRAMFILES(X86)', paths) )
        return ret

def setup_windir(paths):
        return setup_paths('windir', paths)

def prepare(conf):
        env = conf.env

        env['TOOLCHAIN_PATH_x86'] = setup_pfiles([
                'Microsoft SDKs\\Windows\\v7.1\\bin',
                'Microsoft SDKs\\Windows\\v7.0A\\bin',
                'Microsoft Visual Studio 10.0\\VC\\bin',
                'Microsoft Visual Studio 10.0\\Common7\\IDE',
        ]) + setup_windir([
                'Microsoft.NET\\Framework\\v4.0.30319'
        ])

        env['TOOLCHAIN_PATH_x64'] = setup_pfiles([
                'Microsoft SDKs\\Windows\\v7.1\\bin',
                'Microsoft SDKs\\Windows\\v7.0A\\bin',
                'Microsoft Visual Studio 10.0\\VC\\bin\\x86_amd64',
                'Microsoft Visual Studio 10.0\\Common7\\IDE',
        ]) + setup_windir([
                'Microsoft.NET\\Framework64\\v4.0.30319'
        ])

        env['TOOLCHAIN_LIBS_x86'] = setup_pfiles([
                'Microsoft SDKs\\Windows\\v7.1\\Lib',
                'Microsoft SDKs\\Windows\\v7.0A\\Lib',
                'Microsoft Visual Studio 10.0\\VC\\lib',
        ])

        env['TOOLCHAIN_LIBS_x64'] = setup_pfiles([
                'Microsoft SDKs\\Windows\\v7.1\\Lib\\x64',
                'Microsoft SDKs\\Windows\\v7.0A\\Lib\\x64',
                'Microsoft Visual Studio 10.0\\VC\\lib\\x64',
        ])

        env['TOOLCHAIN_INCS'] = setup_pfiles([
                'Microsoft SDKs\\Windows\\v7.1\\Include',
                'Microsoft SDKs\\Windows\\v7.0A\\Include',
                'Microsoft Visual Studio 10.0\\VC\\include',
        ])

        env['TOOLCHAIN_PATH'] = env['TOOLCHAIN_PATH_%s' % env.SUBARCH]
        env['TOOLCHAIN_LIBS'] = env['TOOLCHAIN_LIBS_%s' % env.SUBARCH]

def configure(conf):
        env = conf.env
 
        env.append_value('CSFLAGS', [
                '/warn:4',
                '/define:PEACH',
        ])

        return [ 'debug', 'release' ]

def debug(env):
        env.CSDEBUG = 'full'
        env.append_value('CSFLAGS', ['/define:DEBUG,TRACE'])
        env.append_value('DEFINES', ['DEBUG'])

def release(env):
        env.CSDEBUG = 'pdbonly'
