from waflib.Tools import cs

def configure(conf):
	conf.find_program(['dmcs'], var='MCS')
	conf.env.ASS_ST = '/r:%s'
	conf.env.RES_ST = '/resource:%s'
	conf.env.CS_NAME = 'mono'
