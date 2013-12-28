import xml.sax.handler

from waflib.Configure import conf

class PackageHandler(xml.sax.handler.ContentHandler):
	def __init__(self, ctx, excl):
		self.ctx = ctx
		self.excl = excl

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

		pat = 'lib/*.dll lib/net/*.dll lib/%s/*.dll' % target

		excl=self.excl.get(name, None)
		if excl:
			nodes = path.ant_glob(pat, excl=excl, ignorecase=True)
		else:
			nodes = path.ant_glob(pat, ignorecase=True)

		for n in nodes:
			ctx.read_csshlib(n.name, paths = [ n.parent ])

		content = path.find_node('content')
		if content:
			pass
			#print 'Install content: %s' % content.abspath()
			#x.y = 1

@conf
def read_nuget(self, config, excl=None):
	"""
	Parse nuget packages.config and run read_csslib on each line
	"""

	src = self.path.find_resource(config)
	if src:
		handler = PackageHandler(self, excl)
		parser = xml.sax.make_parser()
		parser.setContentHandler(handler)
		parser.parse(src.abspath())
