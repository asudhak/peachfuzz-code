from waflib.Tools import cs

def configure(conf):
	path = conf.env['TOOLCHAIN_PATH']

	conf.find_program(['csc'], var='MCS', path_list=path)
	conf.env.ASS_ST = '/r:%s'
	conf.env.RES_ST = '/resource:%s'
	conf.env.CS_NAME = 'csc'
