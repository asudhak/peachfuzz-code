from waflib import Utils, Task, Options, Logs, Errors
from waflib.TaskGen import before_method, after_method, feature
from waflib.Tools import ccroot
import os.path

ccroot.USELIB_VARS['msbuild'] = set(['CSFLAGS'])

def configure(conf):
	conf.find_program('msbuild')

msbuild_fmt = '''<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003" DefaultTargets="Build" ToolsVersion="4.0">

	<PropertyGroup>
{PROPERTIES}
	</PropertyGroup>

{SOURCES}

	<ItemGroup>
{REFERENCES}
{REF_HINTS}
	</ItemGroup>


	<Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />

</Project>
'''

# Compile, EmbeddedResource, Page, Resource
src_fmt = '''	<ItemGroup>
		<{1} Include="{0}">
			<Link>{2}</Link>
		</{1}>
	</ItemGroup>'''

ref_fmt = '''		<Reference Include="{0}">
			<HintPath>{1}</HintPath>
		</Reference>'''

use_fmt = '''		<Reference Include="{0}"/>'''

cfg_fmt = '''		<{0}>{1}</{0}>'''

def get_source_type(name):
		if name.endswith('.cs'):
			return 'Compile'
		if name.endswith('.xaml'):
			return 'Page'
		return 'EmbeddedResource'

def get_link_path(self, node):
	if node.is_src():
		return str(node.path_from(self.path))
	else:
		return str(node.path_from(self.path.get_bld()))

@feature('msbuild')
@before_method('process_source')
def apply_msbuild(self):
	bintype = getattr(self, 'bintype', self.gen.endswith('.dll') and 'library' or 'exe')
	asm = self.path.find_or_declare(self.gen)

	cfg = {}

	cfg['OutputType'] = bintype
	cfg['AssemblyName'] = os.path.splitext(self.gen)[0]
	cfg['RootNamespace'] = getattr(self, 'namespace', cfg['AssemblyName'])
	cfg['TargetFrameworkVersion'] = 'v4.0'
	cfg['PlatformTarget'] = getattr(self, 'platform', 'anycpu')
	cfg['IntermediateOutputPath'] = 'obj'
	cfg['OutputPath'] = self.path.get_bld().abspath()
	cfg['UseCommonOutputDirectory'] = 'true'
	cfg['WarningLevel'] = '4'

	self.gen_task = self.create_task('genproj', [], asm.change_ext('.proj'))
	self.cs_task = self.create_task('msbuild', self.gen_task.outputs, asm)

	main = self.to_nodes(getattr(self, 'main', []))
	source = self.to_nodes(getattr(self, 'source', []))
	resource = self.to_nodes(getattr(self, 'resource', []))
	icon = self.to_nodes(getattr(self, 'icon', []))

	srcs = []

	for x in main:
		srcs.append( (x.abspath(), 'ApplicationDefinition', get_link_path(self, x)) )
		if x in source:
			source.remove(x)

	for x in source:
		srcs.append( (x.abspath(), get_source_type(x.name), get_link_path(self, x)) )

	for x in resource:
		srcs.append( (x.abspath(), 'Resource', get_link_path(self, x)) )

	if icon:
		cfg['ApplicationIcon'] = icon[0].abspath()

	self.gen_task.env.MSBUILD_FMT = msbuild_fmt
	self.gen_task.env.MSBUILD_CFG = cfg
	self.gen_task.env.MSBUILD_SRC = srcs
	self.gen_task.env.MSBUILD_REF = []
	self.gen_task.env.MSBUILD_USE = []

	self.cs_task.dep_nodes.extend(main + source + resource + icon)

	self.source = []
	x.y = 1
	inst_to = getattr(self, 'install_path', bintype=='exe' and '${BINDIR}' or '${LIBDIR}')
	if inst_to:
		# note: we are making a copy, so the files added to cs_task.outputs won't be installed automatically
		mod = getattr(self, 'chmod', bintype=='exe' and Utils.O755 or Utils.O644)
		self.install_task = self.bld.install_files(inst_to, self.cs_task.outputs[:], env=self.env, chmod=mod)

		# if this is an exe, look for app.config and install to ${BINDIR}
		if 'exe' in bintype:
			cfg = self.path.find_or_declare('app.config')
			self.bld.install_as('%s/%s.config' % (inst_to, self.gen), cfg, env=self.env, chmod=Utils.O755)

@feature('msbuild')
@after_method('propagate_uselib_vars')
def uselib_msbuild(self):
	ccroot.propagate_uselib_vars(self)

	flags = self.env.CSFLAGS
	defs = ','.join( f[8:] for f in flags if '/define:' in f)

	self.gen_task.env.MSBUILD_CFG['Optimize'] = '/optimize+' in flags and 'true' or 'false'
	self.gen_task.env.MSBUILD_CFG['DefineConstants'] = defs

@feature('msbuild')
@after_method('apply_msbuild')
def use_msbuild(self):
	names = self.to_list(getattr(self, 'use', []))
	get = self.bld.get_tgen_by_name
	for x in names:
		try:
			y = get(x)
		except Errors.WafError:
			self.gen_task.env.append_value('MSBUILD_USE', os.path.splitext(x)[0])
			continue
		y.post()

		tsk = getattr(y, 'cs_task', None) or getattr(y, 'link_task', None)
		if not tsk:
			self.bld.fatal('cs task has no link task for use %r' % self)
		self.cs_task.dep_nodes.extend(tsk.outputs) # dependency
		self.cs_task.set_run_after(tsk) # order (redundant, the order is infered from the nodes inputs/outputs)
		f = tsk.outputs[0]
		self.gen_task.env.MSBUILD_REF.append( (f.abspath(), os.path.splitext(f.name)[0]) )

@feature('msbuild')
@after_method('apply_msbuild', 'use_msbuild')
def debug_msbuild(self):
	csdebug = getattr(self, 'csdebug', self.env.CSDEBUG)
	if not csdebug:
		return

	node = self.cs_task.outputs[0]
	if self.env.CS_NAME == 'mono':
		out = node.parent.find_or_declare(node.name + '.mdb')
	else:
		out = node.change_ext('.pdb')
	self.cs_task.outputs.append(out)
	try:
		self.install_task.source.append(out)
	except AttributeError:
		pass

	if csdebug == 'pdbonly':
		self.gen_task.env.MSBUILD_CFG['DebugSymbols'] = 'true'
		self.gen_task.env.MSBUILD_CFG['DebugType'] = 'pdbonly'
	elif csdebug == 'full':
		self.gen_task.env.MSBUILD_CFG['DebugSymbols'] = 'true'
		self.gen_task.env.MSBUILD_CFG['DebugType'] = 'full'
	else:
		self.gen_task.env.MSBUILD_CFG['DebugSymbols'] = 'false'
		self.gen_task.env.MSBUILD_CFG['DebugType'] = 'none'


@feature('msbuild')
@after_method('apply_msbuild', 'use_msbuild')
def doc_msbuild(self):
	csdoc = getattr(self, 'csdoc', self.env.CSDOC)
	if not csdoc:
		return

	bintype = getattr(self, 'bintype', self.gen.endswith('.dll') and 'library' or 'exe')
	if bintype != 'library':
		return

	node = self.cs_task.outputs[0]
	out = node.change_ext('.xml')
	self.cs_task.outputs.append(out)
	try:
		self.install_task.source.append(out)
	except AttributeError:
		pass

	self.gen_task.env.MSBUILD_CFG['DocumentationFile'] = out.name

class msbuild(Task.Task):
	"""
	Run msbuild
	"""
	color   = 'YELLOW'
	run_str = '${MSBUILD} ${SRC}'

class genproj(Task.Task):
	color = 'PINK'
	vars = [ 'MSBUILD_FMT', 'MSBUILD_CFG', 'MSBUILD_SRC', 'MSBUILD_REF', 'MSBUILD_USE' ]

	def run(self):
		cfg = '\n'.join([ cfg_fmt.format(k, v) for k,v in self.env.MSBUILD_CFG.items()])
		src = '\n'.join([ src_fmt.format(n, t, l) for n,t,l in self.env.MSBUILD_SRC])
		ref = '\n'.join([ ref_fmt.format(n, p) for p,n in self.env.MSBUILD_REF])
		use = '\n'.join([ use_fmt.format(i) for i in self.env.MSBUILD_USE])

		fmt = {
			'PROPERTIES' : cfg,
			'SOURCES'    : src,
			'REF_HINTS'  : ref,
			'REFERENCES' : use,
		}

		txt = self.env.MSBUILD_FMT.format(**fmt)
		#print txt
		self.outputs[0].write(txt)