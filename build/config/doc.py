from waflib import Utils, Errors
from waflib.TaskGen import feature
import os.path

host_plat = [ 'win32', 'linux', 'darwin' ]

archs = [ ]

tools = [
	'misc',
	'tools.asciidoc',
	'tools.utils',
]

def prepare(conf):
	pass

def configure(conf):
	env = conf.env

	env['IS_MONO'] = 'False'
	env.append_value('supported_features', [
		'asciidoc',
		'emit',
		'subst',
	])
