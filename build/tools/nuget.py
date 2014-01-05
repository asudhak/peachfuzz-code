import xml.sax.handler

from waflib.Configure import conf

class PackageHandler(xml.sax.handler.ContentHandler):
	def __init__(self, ctx, excl, mapping):
		self.ctx = ctx
		self.excl = excl
		self.mapping = mapping

	def startElement(self, name, attrs):
		if name != 'package':
			return

		ctx = self.ctx
		name = str(attrs['id'])
		version = str(attrs['version'])
		target = str(attrs['targetFramework'])
		path = ctx.path.find_dir(['%s.%s' % (name, version)])

		if not path:
			return

		basename = self.ctx.env.BASENAME
		if self.mapping and basename != self.mapping.get(name, basename):
			return

		pat = 'lib/*.dll lib/net/*.dll lib/%s/*.dll' % target

		excl=self.excl and self.excl.get(name, None) or None
		if excl:
			nodes = path.ant_glob(pat, excl=excl, ignorecase=True)
		else:
			nodes = path.ant_glob(pat, ignorecase=True)

		for n in nodes:
			ctx.read_csshlib(n.name, paths = [ n.parent ])

		content = path.find_dir('content')
		if content:
			extras = content.ant_glob('**/*')
			self.ctx.install_files('${BINDIR}', extras, env=self.ctx.env, cwd=content, relative_trick=True)

@conf
def read_nuget(self, config, excl=None, mapping=None):
	"""
	Parse nuget packages.config and run read_csslib on each line
	"""

	if not self.env.MCS:
		return

	src = self.path.find_resource(config)
	if src:
		handler = PackageHandler(self, excl, mapping)
		parser = xml.sax.make_parser()
		parser.setContentHandler(handler)
		parser.parse(src.abspath())
