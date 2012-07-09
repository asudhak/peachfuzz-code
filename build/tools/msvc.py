from waflib.Tools import msvc

def configure(conf):
	def null_cmd_and_log(cmd, env):
		return True
	setattr(conf, 'cmd_and_log', null_cmd_and_log)

	env = conf.env

	env['PATH']     = env['TOOLCHAIN_PATH']
	env['INCLUDES'] = env['TOOLCHAIN_INCS']
	env['LIBPATH']  = env['TOOLCHAIN_LIBS']
	
	env['MSVC_COMPILER'] = 'msvc'
	env['MSVC_VERSION'] = 10
	
	conf.find_msvc()
	conf.msvc_common_flags()
	conf.cc_load_tools()
	conf.cxx_load_tools()
	conf.cc_add_flags()
	conf.cxx_add_flags()
	conf.link_add_flags()
	conf.visual_studio_add_flags()

