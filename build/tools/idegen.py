import os
import os.path
import sys

from collections import OrderedDict

from waflib.extras import msvs
from waflib.Build import BuildContext
from waflib import Utils, TaskGen, Logs, Task, Context, Node, Options, Errors

msvs.msvs_generator.cmd = 'msvs2010'

CS_PROJECT_TEMPLATE = r'''<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">${project.build_properties[0].configuration}</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">${project.build_properties[0].platform}</Platform>
    <ProductVersion>8.0.30703</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    ${for k, v in project.globals.iteritems()}
    <${k}>${str(v)}</${k}>
    ${endfor}
  </PropertyGroup>

  ${for props in project.build_properties}
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == '${props.configuration}|${props.platform}' ">
    ${for k, v in props.properties.iteritems()}
    <${k}>${str(v)}</${k}>
    ${endfor}
  </PropertyGroup>
  ${endfor}

  ${if project.references}
  <ItemGroup>
    ${for k,v in project.references.iteritems()}
    ${if v}
    <Reference Include="${k}">
      <HintPath>${v}</HintPath>
    </Reference>
    ${else}
    <Reference Include="${k}" />
    ${endif}
    ${endfor}
  </ItemGroup>
  ${endif}

  ${if project.project_refs}
  <ItemGroup>
    ${for r in project.project_refs}
    <ProjectReference Include="${r.path}">
      <Project>{${r.uuid}}</Project>
      <Name>${r.name}</Name>
    </ProjectReference>
    ${endfor}
  </ItemGroup>
  ${endif}

  ${if project.source_files}
  <ItemGroup>
    ${for src in project.source_files.itervalues()}
    <${src.how} Include="${src.name}"${if not src.attrs} />${else}>
      ${for k,v in src.attrs.iteritems()}
      <${k}>${str(v)}</${k}>
      ${endfor}
    </${src.how}>${endif}
    ${endfor}
  </ItemGroup>
  ${endif}

  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />

</Project>'''

# Note, no newline at end of template file!

class source_file(object):
	def __init__(self, how, ctx, node):
		self.how = how
		self.node = node
		self.attrs = OrderedDict()

		proj_path = node.path_from(ctx.tg.path)
		rel_path = node.path_from(ctx.base)

		if not node.is_child_of(ctx.tg.path):
			proj_path = node.name

		self.name = rel_path

		if proj_path != rel_path:
			self.attrs['Link'] = proj_path

class vsnode_cs_target(msvs.vsnode_project):
	VS_GUID_CSPROJ = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"
	def ptype(self):
		return self.VS_GUID_CSPROJ

	def __init__(self, ctx, tg):
		self.base = getattr(ctx, 'projects_dir', None) or tg.path
		if getattr(ctx, 'csproj_in_tree', True):
			self.base = tg.path
		node = self.base.make_node(os.path.splitext(tg.name)[0] + '.csproj') # the project file as a Node
		msvs.vsnode_project.__init__(self, ctx, node)
		self.name = os.path.splitext(tg.gen)[0]
		self.tg = tg # task generators

		features = set(Utils.to_list(getattr(tg, 'features', [])))
		available = set(Utils.to_list(tg.env['supported_features']))
		intersect = features & available

		# Mark this project as active if the features are all supported
		self.is_active = intersect == features

		# Note: Must use ordered dict so order is preserved
		self.globals      = OrderedDict()
		self.properties   = OrderedDict()
		self.references   = OrderedDict() # Name -> HintPath
		self.source_files = OrderedDict() # Node -> Record
		self.project_refs = [] # uuid

	def combine_flags(self, flag):
		tg = self.tg
		final = OrderedDict()
		for item in tg.env.CSFLAGS:
			if item.startswith(flag):
				opt = item[len(flag):]
				for x in opt.split(';'):
					final.setdefault(x)
		return ';'.join(final.keys())

	def collect_use(self):
		tg = self.tg

		self.other_tgen = []

		names = tg.to_list(getattr(tg, 'use', []))
		get = tg.bld.get_tgen_by_name

		for x in names:
			asm_name = os.path.splitext(x)[0]
			try:
				y = get(x)
			except Errors.WafError:
				self.references.setdefault(asm_name)
				continue
			y.post()

			tsk = getattr(y, 'cs_task', None) or getattr(y, 'link_task', None)
			if not tsk:
				self.bld.fatal('cs task has no link task for use %r' % self)

			if 'fake_lib' in y.features:
				self.references[asm_name] = y.link_task.outputs[0].path_from(self.base)
				continue

			base = self.base == tg.path and y.path or self.base
			other = base.make_node(os.path.splitext(y.name)[0] + '.csproj')
			
			dep = msvs.build_property()
			dep.path = other.path_from(self.base)
			dep.uuid = msvs.make_uuid(other.abspath())
			dep.name = y.name

			self.project_refs.append(dep)

	def collect_source(self):
		tg = self.tg
		lst = self.source_files

		# Process compiled sources
		srcs = tg.to_nodes(tg.cs_task.inputs, [])
		for x in srcs:
			lst[x] = source_file('Compile', self, x)

		# Process compiled resx files
		for tsk in filter(lambda x: x.__class__.__name__ is 'resgen', tg.tasks):
			r = source_file('EmbeddedResource', self, tsk.inputs[0])
			lst[r.node] = r

		# Process embedded resources
		srcs = tg.to_nodes(getattr(tg, 'resource', []))
		for x in srcs:
			r = source_file('EmbeddedResource', self, x)
			lst[x] = r

		# Process installed files
		srcs = []
		srcs.extend(tg.to_nodes(getattr(tg, 'install_644', [])))
		srcs.extend(tg.to_nodes(getattr(tg, 'install_755', [])))
		for x in srcs:
			r = lst.get(x, None)
			if not r:
				r = source_file('Content', self, x)
			r.attrs['CopyToOutputDirectory'] = 'PreserveNewest'
			lst[x] = r

		# Add app.config
		cfg = getattr(tg, 'app_config', None)
		if cfg:
			lst[cfg] = source_file('None', self, cfg)

		settings = []

		# Try and glue up Designer files
		for k,v in lst.iteritems():
			if not k.name.endswith('.Designer.cs'):
				continue

			name = k.name[:-12]

			cs      = lst.get(k.parent.find_resource(name + '.cs'), None)
			resx    = lst.get(k.parent.find_resource(name + '.resx'), None)
			setting = k.parent.find_resource(name + '.settings')

			if cs and resx:
				# If cs & resx, 's' & 'resx' are dependent upon 'cs'
				if Logs.verbose > 0:
					print 'Designer: cs & resx - %s' % k.abspath()
				v.attrs['DependentUpon'] = cs.node.name
				cs.attrs['SubType'] = 'Form'
				resx.attrs['DependentUpon'] = cs.node.name
			elif cs:
				# If cs only, 's' is dependent upon 'cs'
				if Logs.verbose > 0:
					print 'Designer: cs - %s' % k.abspath()
				v.attrs['DependentUpon'] = cs.node.name
				cs.attrs['SubType'] = 'Form'
			elif resx:
				# If resx only, 's' is autogen
				if Logs.verbose > 0:
					print 'Designer: resx - %s' % k.abspath()
				v.attrs['AutoGen'] = True
				v.attrs['DependentUpon'] = resx.node.name
				v.attrs['DesignTime'] = True
				resx.attrs['Generator'] = 'ResXFileCodeGenerator'
				resx.attrs['LastGenOutput'] = k.name
			elif setting:
				# If settings, add to source file list
				if Logs.verbose > 0:
					print 'Designer: settings - %s' % k.abspath()
				f = source_file('None', self, setting)
				f.attrs['Generator'] = 'SettingsSingleFileGenerator'
				f.attrs['LastGenOutput'] = k.name
				v.attrs['AutoGen'] = True
				v.attrs['DependentUpon'] = f.node.name
				v.attrs['DesignTimeSharedInput'] = True

				# Defer adding until we are done iterating
				settings.append(f)

		for x in settings:
			lst[x.node] = x

		self.collect_use()

	def write(self):
		Logs.debug('msvs: creating %r' % self.path)

		# first write the project file
		template1 = msvs.compile_template(CS_PROJECT_TEMPLATE)
		proj_str = template1(self)
		proj_str = msvs.rm_blank_lines(proj_str)
		self.path.stealth_write(proj_str)

	def collect_properties(self):
		tg = self.tg
		g = self.globals

		asm_name = os.path.splitext(tg.cs_task.outputs[0].name)[0]
		base = getattr(self.ctx, 'projects_dir', None) or tg.path

		env = tg.env
		platform = getattr(tg, 'platform', 'AnyCPU')
		config = '%s_%s' % (env.TARGET, env.VARIANT)

		out = base.make_node(['bin', platform, config]).path_from(self.base)

		# Order matters!
		g['ProjectGuid'] = '{%s}' % self.uuid
		g['OutputType'] = getattr(tg, 'bintype', tg.gen.endswith('.dll') and 'library' or 'exe')
		g['BaseIntermediateOutputPath'] = base.make_node('obj').path_from(self.base)

		# This should get rid of the obj/<arch>/<cfg>/TempPE folder
		# but it still exists.  More info available here:
		# http://social.msdn.microsoft.com/Forums/vstudio/en-US/eb1b5a4c-7348-4926-89eb-b57a9d811863/vs-inproc-compiler-msbuild-and-the-obj-subdir
		# g['UseHostCompilerIfAvailable'] = base == tg.path

		g['AppDesignerFolder'] = 'Properties'
		g['RootNamespace'] = getattr(tg, 'namespace', self.name)
		g['AssemblyName'] = asm_name
		g['TargetFrameworkVersion'] = 'v4.0'
		g['TargetFrameworkProfile'] = os.linesep + '    '
		g['FileAlignment'] = '512'
		g['ResolveAssemblyReferenceIgnoreTargetFrameworkAttributeVersionMismatch'] = True

		keyfile = tg.to_nodes(getattr(tg, 'keyfile', []))
		if keyfile:
			f = source_file('None', self, keyfile[0])
			g['SignAssembly'] = True
			g['AssemblyOriginatorKeyFile'] = f.name
			self.source_files[f.node] = f

		p = self.properties

		# Order matters!
		p['PlatformTarget'] = platform
		p['DebugSymbols'] = getattr(tg, 'csdebug', tg.env.CSDEBUG) and True or False
		p['DebugType'] = getattr(tg, 'csdebug', tg.env.CSDEBUG)
		p['Optimize'] = '/optimize+' in tg.env.CSFLAGS
		p['OutputPath'] = out
		p['DefineConstants'] = self.combine_flags('/define:')
		p['ErrorReport'] = 'prompt'
		p['WarningLevel'] = self.combine_flags('/warn:')
		p['NoWarn'] = self.combine_flags('/nowarn:')
		p['TreatWarningsAsErrors'] = '/warnaserror' in tg.env.CSFLAGS
		p['DocumentationFile'] = getattr(tg, 'csdoc', tg.env.CSDOC) and out + os.sep + asm_name + '.xml' or ''
		p['AllowUnsafeBlocks'] = getattr(tg, 'unsafe', False)


class idegen(msvs.msvs_generator):
	all_projs = OrderedDict()
	is_idegen = True
	depth = 0

	def init(self):
		msvs.msvs_generator.init(self)

		#self.projects_dir = None
		#self.csproj_in_tree = False
		self.solution_name = self.env.APPNAME + '.sln'

		if not getattr(self, 'vsnode_cs_target', None):
			self.vsnode_cs_target = vsnode_cs_target

	def execute(self):
		idegen.depth += 1
		msvs.msvs_generator.execute(self)

	def write_files(self):
		if self.all_projects:
			# Move up all the projects one level
			remove = {}
			for p in self.all_projects:
				if not hasattr(p, 'tg'):
					continue

				parent = getattr(p.tg, 'ide_path', p.tg.path).parent
				path = p.tg.path

				while path != parent:
					remove.setdefault(p.parent)
					p.parent = p.parent.parent
					path = path.parent

			idegen.all_projs[self.variant] = [ p for p in self.all_projects if p not in remove ]

		idegen.depth -= 1
		if idegen.depth == 0:
			self.all_projects = self.flatten_projects()

			if Logs.verbose == 0:
				sys.stderr.write('\n')

			msvs.msvs_generator.write_files(self)

	def flatten_projects(self):
		ret = OrderedDict()
		configs = OrderedDict()
		platforms = OrderedDict()

		# TODO: Might need to implement conditional project refereces
		# as well as assembly references based on the selected
		# configuration/platform
		for k,v in idegen.all_projs.iteritems():
			for p in v:
				p.ctx = self
				ret.setdefault(p.uuid, p)

				if not getattr(p, 'tg', []):
					continue

				main = ret[p.uuid]

				env = p.tg.env

				prop = msvs.build_property()
				prop.configuration = '%s_%s' % (env.TARGET, env.VARIANT)
				prop.platform = p.properties['PlatformTarget']
				prop.properties = p.properties

				configs.setdefault(prop.configuration)
				platforms.setdefault(prop.platform)

				main.build_properties.append(prop)

		self.configurations = configs.keys()
		self.platforms = platforms.keys()
		
		return ret.values()

	def add_aliases(self):
		pass

	def collect_targets(self):
		"""
		Process the list of task generators
		"""
		for g in self.groups:
			for tg in g:
				if not isinstance(tg, TaskGen.task_gen):
					continue

				# TODO: Look for c/c++ link_task and add vcxproj

				tg.post()
				if not getattr(tg, 'cs_task', None):
					continue

				p = self.vsnode_cs_target(self, tg)
				p.collect_source() # delegate this processing
				p.collect_properties()
				self.all_projects.append(p)

				if Logs.verbose == 0:
					sys.stderr.write('.')
					sys.stderr.flush()
