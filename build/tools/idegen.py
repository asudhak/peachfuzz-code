import os
import os.path
import sys
import re

from collections import OrderedDict

from waflib.extras import msvs
from waflib.Build import BuildContext
from waflib import Utils, TaskGen, Logs, Task, Context, Node, Options, Errors

msvs.msvs_generator.cmd = 'msvs2010'

form_re = re.compile('Windows Form Designer generated code')

MONO_PROJECT_TEMPLATE = r'''<?xml version="1.0" encoding="utf-8"?>
<Project DefaultTargets="Build" ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">${project.build_properties[0].configuration}</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">${project.build_properties[0].platform_tgt}</Platform>
    <ProductVersion>10.0.0</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{${project.uuid}}</ProjectGuid>
    <Compiler>
      <Compiler ctype="GppCompiler" />
    </Compiler>
    <Language>CPP</Language>
    <Target>Bin</Target>
  </PropertyGroup>


  ${for b in project.build_properties}
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == '${b.configuration}|${b.platform}' ">
    <DebugSymbols>true</DebugSymbols>
    <OutputPath>${b.outdir}</OutputPath>
    <OutputName>${b.output_file}</OutputName>
    <CompileTarget>Bin</CompileTarget>
    <DefineSymbols>${xml:b.preprocessor_definitions}</DefineSymbols>
    <SourceDirectory>.</SourceDirectory>
    <CustomCommands>
      <CustomCommands>
        <Command type="Clean" command="${xml:project.get_clean_command(b)}" workingdir="${project.ctx.path.abspath()}" />
        <Command type="Build" command="${xml:project.get_build_command(b)}" workingdir="${project.ctx.path.abspath()}" />
      </CustomCommands>
    </CustomCommands>
  </PropertyGroup>
  ${endfor}

  <ItemGroup>
    ${for x in project.source}
    <${project.get_mono_key(x)} Include='${x.path_from(project.ctx.path)}'>
      <Link>${x.path_from(project.tg.path)}</Link>
    </${project.get_mono_key(x)}>
    ${endfor}
  </ItemGroup>

</Project>'''

CS_PROJECT_TEMPLATE = r'''<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">${project.build_properties[0].configuration}</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">${project.build_properties[0].platform_tgt}</Platform>
    <ProductVersion>8.0.30703</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    ${for k, v in project.globals.iteritems()}
    <${k}>${str(v)}</${k}>
    ${endfor}
  </PropertyGroup>

  ${for props in project.build_properties}
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == '${props.configuration}|${props.platform_tgt}' ">
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

  ${for p in project.build_properties}
  ${if p.sources}
  <ItemGroup Condition=" '$(Configuration)|$(Platform)' == '${p.configuration}|${p.platform_tgt}' ">
    ${for src in p.sources}
    <${src.how} Include="${src.name}" ${if not src.attrs} />${else}>
      ${for k,v in src.attrs.iteritems()}
      <${k}>${str(v)}</${k}>
      ${endfor}
    </${src.how}>${endif}
    ${endfor}
  </ItemGroup>
  ${endif}
  ${endfor}

  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />

  ${if any([x for x in project.build_properties if x.post_build])}
  <PropertyGroup>
    ${for p in project.build_properties}
    ${if p.post_build}
    <PostBuildEvent Condition=" '$(Configuration)|$(Platform)' == '${props.configuration}|${props.platform_tgt}' ">${p.post_build}</PostBuildEvent>
    ${endif}
    ${endfor}
  </PropertyGroup>
  ${endif}

</Project>'''

# Note, no newline at end of template file!

class source_file(object):
	def __init__(self, how, ctx, node, cwd=None):
		self.how = how
		self.node = node
		self.attrs = OrderedDict()

		if not cwd:
			cwd = ctx.tg.path

		rel_path = node.path_from(ctx.base)

		if not node.is_child_of(cwd):
			proj_path = node.name
		else:
			proj_path = node.path_from(cwd)

		self.name = rel_path

		if proj_path != rel_path:
			self.attrs['Link'] = proj_path

class vsnode_target(msvs.vsnode_target):
	def __init__(self, ctx, tg):
		msvs.vsnode_target.__init__(self, ctx, tg)
		self.proj_configs = OrderedDict() # Variant -> build_property

	def collect_properties(self):
		msvs.vsnode_target.collect_properties(self)
		self.is_active = True

	def get_mono_key(self, node):
		"""
		required for writing the source files
		"""
		name = node.name
		if name.endswith('.cpp') or name.endswith('.c'):
			return 'Compile'
		return 'None'

	def get_waf(self):
		return 'cd /d "%s" & "%s" %s' % (self.ctx.srcnode.abspath(), sys.executable, getattr(self.ctx, 'waf_command', 'waf'))

	def get_waf_mono(self):
		return '%s %s' % (sys.executable, getattr(self.ctx, 'waf_command', 'waf'))

	def get_build_params(self, props):
		(waf, opt) = msvs.vsnode_target.get_build_params(self, props)
		opt += " --variant=%s" % props.variant
		return (waf, opt)


class vsnode_cs_target(msvs.vsnode_project):
	VS_GUID_CSPROJ = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"
	def ptype(self):
		return self.VS_GUID_CSPROJ

	def __init__(self, ctx, tg):
		self.base = getattr(ctx, 'projects_dir', None) or tg.path
		if getattr(ctx, 'csproj_in_tree', True):
			self.base = tg.path
		namespace = getattr(tg, 'namespace', os.path.splitext(tg.gen)[0])
		node = self.base.make_node(namespace + '.csproj') # the project file as a Node
		msvs.vsnode_project.__init__(self, ctx, node)
		self.name = namespace
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
		self.source_files = OrderedDict() # Abspath -> Record
		self.project_refs = [] # uuid
		self.proj_configs = OrderedDict() # Variant -> build_property

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
		get = tg.bld.get_tgen_by_name

		names = tg.to_list(getattr(tg, 'use', []))

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
			dep.name = os.path.splitext(y.name)[0]

			self.project_refs.append(dep)

	def collect_install(self, lst, attr):
		val = getattr(self.tg, attr, [])

		if isinstance(val, dict):
			for cwd, items in val.iteritems():
				self.collect_install2(lst, items, cwd)
		else:
			self.collect_install2(lst, val, self.tg.path)

	def collect_install2(self, lst, items, path):
		extras = self.tg.to_nodes(items, path=path)

		for x in extras:
			r = lst.get(x.abspath(), None)
			if not r:
				r = source_file('Content', self, x, path)
			r.attrs['CopyToOutputDirectory'] = 'PreserveNewest'
			lst[x.abspath()] = r

	def collect_source(self):
		tg = self.tg
		lst = self.source_files

		# Process compiled sources
		srcs = tg.to_nodes(tg.cs_task.inputs, [])
		for x in srcs:
			lst[x.abspath()] = source_file('Compile', self, x)

		# Process compiled resx files
		for tsk in filter(lambda x: x.__class__.__name__ is 'resgen', tg.tasks):
			r = source_file('EmbeddedResource', self, tsk.inputs[0])
			lst[r.node.abspath()] = r

		# Process embedded resources
		srcs = tg.to_nodes(getattr(tg, 'resource', []))
		for x in srcs:
			r = source_file('EmbeddedResource', self, x)
			lst[x.abspath()] = r

		# Process installed files
		self.collect_install(lst, 'install_644')
		self.collect_install(lst, 'install_755')

		# Add app.config
		cfg = getattr(tg, 'app_config', None)
		if cfg:
			lst[cfg.abspath()] = source_file('None', self, cfg)

		settings = []

		# Try and glue up Designer files
		for k,v in lst.iteritems():
			n = v.node

			if not n.name.lower().endswith('.designer.cs'):
				continue

			if form_re.search(n.read()):
				subtype = 'Form'
			else:
				subtype = 'Component'

			name = n.name[:-12]

			cs = n.parent.find_resource(name + '.cs')
			if cs: cs = lst.get(cs.abspath(), None)
			resx = n.parent.find_resource(name + '.resx')
			if resx: resx = lst.get(resx.abspath(), None)
			setting = n.parent.find_resource(name + '.settings')

			if cs and resx:
				# If cs & resx, 's' & 'resx' are dependent upon 'cs'
				if Logs.verbose > 0:
					print 'Designer: cs & resx - %s %s' % (subtype, k)
				v.attrs['DependentUpon'] = cs.node.name
				cs.attrs['SubType'] = subtype
				resx.attrs['DependentUpon'] = cs.node.name
			elif cs:
				# If cs only, 's' is dependent upon 'cs'
				if Logs.verbose > 0:
					print 'Designer: cs - %s %s' % (subtype, k)
				v.attrs['DependentUpon'] = cs.node.name
				cs.attrs['SubType'] = subtype
			elif resx:
				# If resx only, 's' is autogen
				if Logs.verbose > 0:
					print 'Designer: resx - %s' % k
				v.attrs['AutoGen'] = True
				v.attrs['DependentUpon'] = resx.node.name
				v.attrs['DesignTime'] = True
				resx.attrs['Generator'] = 'ResXFileCodeGenerator'
				resx.attrs['LastGenOutput'] = n.name
			elif setting:
				# If settings, add to source file list
				if Logs.verbose > 0:
					print 'Designer: settings - %s' % k
				f = source_file('None', self, setting)
				f.attrs['Generator'] = 'SettingsSingleFileGenerator'
				f.attrs['LastGenOutput'] = n.name
				v.attrs['AutoGen'] = True
				v.attrs['DependentUpon'] = f.node.name
				v.attrs['DesignTimeSharedInput'] = True

				# Defer adding until we are done iterating
				settings.append(f)

		for x in settings:
			lst[x.node.abspath()] = x

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
		config = self.ctx.get_config(tg.bld, tg.env)

		out_node = base.make_node(['bin', platform, config])
		out = out_node.path_from(self.base)

		# Order matters!
		g['ProjectGuid'] = '{%s}' % self.uuid
		g['OutputType'] = getattr(tg, 'bintype', tg.gen.endswith('.dll') and 'library' or 'exe')
		g['BaseIntermediateOutputPath'] = base.make_node('obj').path_from(self.base)

		# This should get rid of the obj/<arch>/<cfg>/TempPE folder
		# but it still exists.  More info available here:
		# http://social.msdn.microsoft.com/Forums/vstudio/en-US/eb1b5a4c-7348-4926-89eb-b57a9d811863/vs-inproc-compiler-msbuild-and-the-obj-subdir
		# g['UseHostCompilerIfAvailable'] = base == tg.path

		g['AppDesignerFolder'] = 'Properties'
		g['RootNamespace'] = self.name
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
			self.source_files[f.node.abspath()] = f

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

		# Add ide_inst task generator outputs as post build copy
		# Using abspath since macros like $(ProjectDir) don't seem to work

		ide_inst = []
		names = names = tg.to_list(getattr(tg, 'ide_inst', []))
		for x in names:
			y = tg.bld.get_tgen_by_name(x)
			y.post()
			tsk = getattr(y, 'link_task', None)
			if not tsk:
				self.bld.fatal('cs task has no link task for ide_inst %r' % self)

			src = y.link_task.outputs[0]
			dst = out_node.make_node(src.name)
			ide_inst.append((src, dst))

		(inst_cmd, inst_args) = self.ctx.get_ide_inst(ide_inst)
		if inst_args:
			p[inst_cmd] = inst_args

class idegen(msvs.msvs_generator):
	all_projs = OrderedDict()
	sln_configs = OrderedDict() # Variant -> build_property
	is_idegen = True
	depth = 0
	copy_cmd = 'copy'

	def init(self):
		msvs.msvs_generator.init(self)

		#self.projects_dir = None
		#self.csproj_in_tree = False
		self.solution_name = self.env.APPNAME + '.sln'

		# Make monodevelop csproj
		if (Utils.unversioned_sys_platform() != 'win32'):
			msvs.PROJECT_TEMPLATE = MONO_PROJECT_TEMPLATE
			msvs.vsnode_project.VS_GUID_VCPROJ = '2857B73E-F847-4B02-9238-064979017E93'
			vsnode_target.get_waf = vsnode_target.get_waf_mono
			self.project_extension = '.cproj'
			self.get_platform = self.get_platform_mono
			self.get_config = self.get_config_mono
			self.get_ide_inst = self.get_ide_inst_mono
			idegen.copy_cmd = 'cp'

		self.vsnode_cs_target = vsnode_cs_target
		self.vsnode_target = vsnode_target

	def get_config(self, bld, env):
		return '%s_%s' % (env.TARGET, env.VARIANT)

	def get_config_mono(self, bld, env):
		return bld.variant

	def get_platform(self, env):
		return env.SUBARCH.replace('x86', 'Win32')

	def get_platform_mono(self, env):
		return 'Win32'

	def get_ide_inst(self, items):
		args = []
		for (src, dst) in items:
			args.append('copy "%s" "%s"' % (src.abspath(), dst.abspath()))
		return ('PostBuildEvent', os.linesep.join(args))

	def get_ide_inst_mono(self, items):
        	# <Command type="AfterBuild" command="cp &quot;foo bar&quot; &quot;bax quux&quot;" />
		args = []
		for (src, dst) in items:
			args.append('        <Command type="AfterBuild" command="cp &quot;%s&quot; &quot;%s&quot;" />' % (src.abspath(), dst.abspath()))

		if args:
			args.insert(0, '')
			args.insert(1, '      <CustomCommands>')
			args.append('      </CustomCommands>')
			args.append('    ')

		return ('CustomCommands', os.linesep.join(args))

	def execute(self):
		idegen.depth += 1
		msvs.msvs_generator.execute(self)

	def write_files(self):
		if self.all_projects:
			# Generate the sln config|plat for this variant
			prop = msvs.build_property()
			prop.platform_tgt = self.env.CSPLATFORM
			prop.platform = self.get_platform(self.env)
			prop.platform_sln = prop.platform_tgt.replace('AnyCPU', 'Any CPU')
			prop.configuration = self.get_config(self, self.env)
			prop.variant = self.variant

			idegen.sln_configs[prop.variant] = prop
			idegen.all_projs[self.variant] = self.all_projects

		idegen.depth -= 1
		if idegen.depth == 0:
			self.all_projects = self.flatten_projects()

			if Logs.verbose == 0:
				sys.stderr.write('\n')

			for p in self.all_projects:
				p.write()

			self.make_sln_configs()

			# and finally write the solution file
			node = self.get_solution_node()
			node.parent.mkdir()
			Logs.warn('Creating %r' % node)
			template1 = msvs.compile_template(msvs.SOLUTION_TEMPLATE)
			sln_str = template1(self)
			sln_str = msvs.rm_blank_lines(sln_str)
			node.stealth_write(sln_str)

	def collect_dirs(self):
		"""
		Create the folder structure in the Visual studio project view
		"""
		seen = {}
		def make_parents(proj):
			# look at a project, try to make a parent
			if getattr(proj, 'parent', None):
				# aliases already have parents
				return
			x = proj.iter_path
			if x in seen:
				proj.parent = seen[x]
				return

			# There is not vsnode_vsdir for x.
			# So create a project representing the folder "x"
			n = proj.parent = seen[x] = self.vsnode_vsdir(self, msvs.make_uuid(x.abspath()), x.name)
			n.iter_path = x.parent
			self.all_projects.append(n)

			# recurse up to the project directory
			if x.height() > self.srcnode.height() + 1:
				make_parents(n)

		for p in self.all_projects[:]: # iterate over a copy of all projects
			if not getattr(p, 'tg', None):
				# but only projects that have a task generator
				continue

			# make a folder for each task generator
			path = p.tg.path.parent
			ide_path = getattr(p.tg, 'ide_path', '.')
			if os.path.isabs(ide_path):
				p.iter_path = self.path.make_node(ide_path)
			else:
				p.iter_path = path.make_node(ide_path)

			if p.iter_path.height() > self.srcnode.height():
				make_parents(p)

	def project_configurations(self):
		ret = []
		for k,v in idegen.sln_configs.iteritems():
			ret.append((v.configuration, v.platform_sln))
		return ret

	def make_sln_configs(self):
		sln_cfg = idegen.sln_configs

		for p in self.all_projects:
			if not hasattr(p, 'tg'):
				continue

			if len(sln_cfg) == len(p.build_properties):
				continue

			props = []

			for k, v in sln_cfg.iteritems():
				other = p.proj_configs.get(k, None)
				if other:
					prop = msvs.build_property()
					prop.configuration = v.configuration
					prop.platform_sln = v.platform_sln
					prop.platform = other.platform
					prop.is_active = True
					props.append(prop)
					#print '%s %s %s|%s -> %s|%s' % (p.name, k, prop.configuration, prop.platform_sln, prop.configuration, prop.platform)

			p.build_properties = props

	def flatten_projects(self):
		ret = OrderedDict()

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
				config = self.get_config(p.tg.bld, p.tg.env)

				if isinstance(p, vsnode_cs_target):
					prop = msvs.build_property()
					prop.platform_tgt = p.properties['PlatformTarget']
					# Solution needs 'Any CPU' not 'AnyCPU'
					prop.platform = prop.platform_tgt.replace('AnyCPU', 'Any CPU')
					prop.platform_sln = prop.platform
					prop.properties = p.properties
					prop.sources = []
					prop.post_build = getattr(p, 'post_build', None)

					# MonoDevelop doesn't let us do conditions on a per
					# source basis. Condition only works on PropertyGroup
					# and Reference elements.
					'''
					if main != p:
						cur_src = set( main.source_files.iterkeys())
						new_src = set(p.source_files.iterkeys())

						in_both = new_src & cur_src
						from_old = cur_src.difference(in_both)
						from_new = new_src.difference(in_both)

						for item in from_old:
							# Item's in from_old need to be removed from source_list
							# and tracked per variant
							main.cond_source = True
							rec = main.source_files.pop(item)
							for other_prop in main.build_properties:
								other_prop.sources.append(rec)

						for item in from_new:
							# Item's in from_new need to be tracked per variant
							main.cond_source = True
							prop.sources.append(p.source_files[item])
					'''
				else:
					prop = p.build_properties[0]
					prop.platform_tgt = env.CSPLATFORM
					prop.platform = self.get_platform(env)
					prop.platform_sln = prop.platform_tgt.replace('AnyCPU', 'Any CPU')
					p.build_properties = []

				prop.configuration = config
				prop.variant = k

				main.proj_configs[k] = prop

				if not any([ x for x in main.build_properties if x.platform == prop.platform and x.configuration == config ]):
					main.build_properties.append(prop)

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

				if not hasattr(tg, 'msvs_includes'):
					tg.msvs_includes = tg.to_list(getattr(tg, 'includes', [])) + tg.to_list(getattr(tg, 'export_includes', []))

				tg.post()

				if 'fake_lib' in getattr(tg, 'features', ''):
					continue
				elif hasattr(tg, 'link_task'):
					p = self.vsnode_target(self, tg)
				elif hasattr(tg, 'cs_task'):
					p = self.vsnode_cs_target(self, tg)
				else:
					continue

				p.collect_source() # delegate this processing
				p.collect_properties()
				self.all_projects.append(p)

				if Logs.verbose == 0:
					sys.stderr.write('.')
					sys.stderr.flush()

def options(ctx):
	"""
	If the msvs option is used, try to detect if the build is made from visual studio
	"""
	ctx.add_option('--execsolution', action='store', help='when building with visual studio, use a build state file')

	old = BuildContext.execute
	def override_build_state(ctx):
		def lock(rm, add):
			uns = ctx.options.execsolution.replace('.sln', rm)
			uns = ctx.root.make_node(uns)
			try:
				uns.delete()
			except:
				pass

			uns = ctx.options.execsolution.replace('.sln', add)
			uns = ctx.root.make_node(uns)
			try:
				uns.write('')
			except:
				pass

		if ctx.options.execsolution:
			ctx.launch_dir = Context.top_dir # force a build for the whole project (invalid cwd when called by visual studio)
			lock('.lastbuildstate', '.unsuccessfulbuild')
			old(ctx)
			lock('.unsuccessfulbuild', '.lastbuildstate')
		else:
			old(ctx)
	BuildContext.execute = override_build_state
