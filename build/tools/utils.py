import os.path
from waflib.TaskGen import feature, before_method, after_method
from waflib import Utils

@feature('cprogram', 'cxxprogram', 'csprogram')
@after_method('process_source')
def install_extras(self):
	try:
		inst_to = self.install_path
	except AttributeError:
		inst_to = hasattr(self, 'link_task') and self.link_task.__class__.inst_to or None

	if not inst_to:
		return

	extras = self.to_nodes(getattr(self, 'install', []))
	for x in extras:
		rel_path = x.path_from(self.path)
		self.bld.install_as(os.path.join(inst_to, rel_path), x, env=self.env, chmod=Utils.O644)
