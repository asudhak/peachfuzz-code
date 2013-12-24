from waflib.TaskGen import before_method, after_method, feature
from waflib import Errors, Task, Logs, Utils
import os.path

def configure(conf):
	try:
		root = conf.path.abspath()
		mdoc_path = conf.env['MDOC_PATH'] or os.path.join(root, '3rdParty', 'mdoc-net-2010-01-04')
		conf.find_program('mdoc', var='MDOC', exts='.exe', path_list=[mdoc_path])
		conf.env.MDOC_ST = '--lib=%s'
		conf.env.MDOC_OUTPUT = '${PREFIX}/apidoc'

		if (Utils.unversioned_sys_platform() != 'win32'):
			conf.find_program('mono', var='MDOC_MONO')

	except Exception, e:
		if Logs.verbose > 0:
			Logs.warn('C# html documentation is not available: %s' % (e))

@feature('cs')
@after_method('doc_cs')
def make_mdoc(self):
	csdoc = getattr(self, 'csdoc', self.env.CSDOC)
	if not csdoc:
		return

	bintype = getattr(self, 'bintype', self.gen.endswith('.dll') and 'library' or 'exe')
	if bintype != 'library':
		return

	if not getattr(self, 'cs_task', None):
		return

	asm = None
	xml = None
	out = None

	for n in self.cs_task.outputs:
		parts = os.path.splitext(n.name)
		if parts[1] in [ '.mdb', '.pdb' ]:
			continue;
		if parts[1] == '.xml':
			xml = n
		else:
			asm = n
			out = n.parent.find_or_declare('apidoc/%s/index.xml' % parts[0]).parent

	if not asm or not xml:
		raise Errors.WafError("Bad sources in %s" % self.name)

	test = self.create_task('mdoc_update', [ asm, xml ], [ out ])

	names = self.to_list(getattr(self, 'use', []))
	get = self.bld.get_tgen_by_name
	for x in names:
		try:
			y = get(x)
		except Errors.WafError:
			continue
		y.post()

		tsk = getattr(y, 'cs_task', None) or getattr(y, 'link_task', None)
		if not tsk:
			self.bld.fatal('cs task has no link task for use %r' % self)
		test.env.append_value('MDOC_ASM_DIRS', tsk.outputs[0].bld_dir())

	html = getattr(self.bld, 'mdoc_gen', None)
	if not html:
		html = self.bld(name = 'monodoc')
		html.output_dir = html.path.find_or_declare('apidoc/index.html').parent
		tsk = html.create_task('mdoc_html', [ out ], [ html.output_dir ] )
		setattr(html, 'mdoc_tsk', tsk)
		setattr(self.bld, 'mdoc_gen', html)
	else:
		html.mdoc_tsk.inputs.append(out)

class mdoc_update(Task.Task):
	"""
	Run msbuild
	"""
	color   = 'YELLOW'
	run_str = '${MDOC_MONO} ${MDOC} update -i ${SRC[1].relpath()} ${MDOC_ST:MDOC_ASM_DIRS} -o ${TGT} ${SRC[0].relpath()}'

	def runnable_status(self):
		ret = Task.SKIP_ME

		if getattr(self.generator.bld, 'is_mdoc', None):
			ret = super(mdoc_update, self).runnable_status()

		return ret

class mdoc_html(Task.Task):
	"""
	Run msbuild
	"""
	color   = 'YELLOW'
	run_str = '${MDOC_MONO} ${MDOC} export-html -o ${TGT} ${SRC}'

	def runnable_status(self):
		ret = Task.SKIP_ME

		if getattr(self.generator.bld, 'is_mdoc', None):
			ret = super(mdoc_html, self).runnable_status()

		return ret

	def post_run(self):
		html = self.generator
		nodes = html.output_dir.ant_glob('**/*', quiet=True)
		print nodes[0:10]
		for x in nodes:
			x.sig = Utils.h_file(x.abspath())
		self.outputs += nodes
		html.bld.install_files(html.env.MDOC_OUTPUT, nodes, cwd=html.output_dir, relative_trick=True, postpone=False)
		return Task.Task.post_run(self)
