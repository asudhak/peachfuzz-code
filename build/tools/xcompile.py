from waflib import Logs

template = '''
#if defined(__cplusplus)
#include <iostream>
#else
#include <stdio.h>
#endif

int main(int argc, char** argv) {
	return 0;
}
'''

def configure(conf):
	old_start = conf.start_msg
	old_end = conf.end_msg

	if Logs.verbose == 0:
		def null_msg(self, msg='', color=None, **kw):
			pass

		conf.start_msg = null_msg
		conf.end_msg = null_msg

	try:
		conf.check_cc(fragment=template, msg='Verifying cc compilation', errmsg='Failed, ensure gcc/gcc-multilib is installed', okmsg='Success')
		conf.check_cxx(fragment=template, msg='Verifying cxx compilation', errmsg='Failed, ensure g++/g++-multilib is installed', okmsg='Success')
	finally:
		conf.start_msg = old_start
		conf.end_msg = old_end


