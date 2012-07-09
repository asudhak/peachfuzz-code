import os.path
from waflib.TaskGen import feature, before_method, after_method
from waflib import Utils

def configure(conf):
	pass

@feature('fake_lib')
@after_method('process_lib')
def install_csshlib(self):
	if self.lib_type != 'csshlib':
	    return
	try:
		inst_to = self.install_path
	except AttributeError:
		inst_to = '${LIBDIR}'
	if inst_to:
		self.install_task = self.bld.install_files(inst_to, self.link_task.outputs[:], env=self.env, chmod=Utils.O644)

@feature('cs')
@before_method('apply_cs')
def cs_helpers(self):
	if not getattr(self, 'gen', None):
		setattr(self, 'gen', self.name)
	
	cfg = getattr(self, 'config', None)
	if cfg:
		try:
			inst_to = self.install_path
		except AttributeError:
			inst_to = '${BINDIR}'
		if inst_to:
			node = self.path.find_node(cfg)
			base = os.path.splitext(self.gen)[0]
			self.bld.install_as('%s/%s.config' % (inst_to, base), node, env=self.env, chmod=Utils.O644)
