from waflib.TaskGen import feature, before_method, after_method
from waflib.Task import Task
import re

def configure(conf):
	#conf.find_program('a2x', var='A2X')
	#conf.find_program('dia', var='DIA')
	#conf.find_program('convert', var='CONVERT')
	#conf.find_program('dot')
	conf.find_program('source-highlight', var='SOURCE_HIGHLIGHT')
	conf.find_program('asciidoc', var='ADOC')
	conf.env['ADOCOPTS'] = [ '--backend=xhtml11' ]

@feature('asciidoc')
def apply_asciidoc(self):
	self.source = self.to_nodes(getattr(self, 'source', []))
	if not self.source:
		return

	conf = self.to_nodes(getattr(self, 'conf', []))
	if conf:
		conf = conf[0]
		self.env.append_value('ADOCOPTS', [ '--conf-file=%s' % conf.bldpath()])

	tsks = []
	outputs = []
	for src in self.source:
		tsk = self.create_task('asciidoc', src, src.change_ext(".html"))
		tsks.append(tsk)
		outputs.extend(tsk.outputs)

	if conf:
		for src in self.source:
			self.bld.add_manual_dependency(src, conf)

	self.relative_trick = getattr(self, 'relative_trick', False)

	try:
		inst_to = self.install_path
	except AttributeError:
		inst_to = tsks[0].__class__.inst_to
	if inst_to:
		self.bld.install_files(inst_to, outputs, cwd = self.path.get_bld(), env = self.env, relative_trick = self.relative_trick)

	self.source = []

re_xi = re.compile('''^(include|image)::([^.]*.(txt|\\{PIC\\}))\[''', re.M)

def ascii_doc_scan(self):
	p = self.inputs[0].parent
	node_lst = [self.inputs[0]]
	seen = []
	depnodes = []
	while node_lst:
		nd = node_lst.pop(0)
		if nd in seen: continue
		seen.append(nd)

		code = nd.read()
		for m in re_xi.finditer(code):
			name = m.group(2)
			if m.group(3) == '{PIC}':

				ext = '.eps'
				if self.generator.rule.rfind('A2X') > 0:
					ext = '.png'

				k = p.find_resource(name.replace('{PIC}', ext))
				if k:
					depnodes.append(k)
			else:
				k = p.find_resource(name)
				if k:
					depnodes.append(k)
					node_lst.append(k)
	return [depnodes, ()]

class asciidoc(Task): 
	run_str = '${ADOC} ${ADOCOPTS} -o ${TGT} ${SRC}'
	color   = 'PINK'
	vars    = ['ADOCOPTS']
	scan    = ascii_doc_scan
	inst_to = '${DOCDIR}'
