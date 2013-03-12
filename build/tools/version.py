from waflib.TaskGen import feature, before_method, after_method
from waflib import Utils, Errors, Task
import datetime, os.path, re

re_guid = re.compile('\[assembly:\s*Guid\s*\(\s*"([^"]+)"\s*\)\s*\]')
re_com = re.compile('\[assembly:\s*ComVisible\s*\((\w+)\s*\)\s*\]')

template_cs = """
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("{TITLE}")]
[assembly: AssemblyDescription("{DESC}")]
[assembly: AssemblyConfiguration("{CONFIG}")]
[assembly: AssemblyCompany("{COMPANY}")]
[assembly: AssemblyProduct("{PRODUCT}")]
[assembly: AssemblyCopyright("Copyright (c) {YEAR} {COMPANY}")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("{VER0}.{VER1}.{VER2}.{VER3}")]
[assembly: AssemblyFileVersion("{VER0}.{VER1}.{VER2}.{VER3}")]
[assembly: AssemblyInformationalVersion("{VER0}.{VER1}.{VER2}.{VER3} {VARIANT}")]
"""

template_cs_com = """
[assembly: ComVisible({COM})]
[assembly: Guid("{GUID}")]
"""

template_c = """
#if defined(__cplusplus)
extern "C" {{
#endif
const char* g_buildTag = "{VER0}.{VER1}.{VER2}.{VER3}";
const char* g_buildDesc = "{PRODUCT} Version {VER0}.{VER1}.{VER2}.{VER3} ({CONFIG}), Built " __DATE__ " " __TIME__;
#if defined(__cplusplus)
}}
#endif
"""

template_rc = """
#include <winver.h>
VS_VERSION_INFO VERSIONINFO
 FILEVERSION {VER0},{VER1},{VER2},{VER3}
 PRODUCTVERSION {VER0},{VER1},{VER2},{VER3}
 FILEFLAGS {FILEFLAGS} // 0, VS_FF_DEBUG, VS_FF_PRERELEASE
 FILEOS VOS_NT_WINDOWS32
 FILETYPE {FILETYPE} // VFT_DLL, VFT_APP
BEGIN
    BLOCK "StringFileInfo"
    BEGIN
        BLOCK "04090000"
        BEGIN
            VALUE "CompanyName", "{COMPANY}"
            VALUE "FileDescription", "{DESC}"
            VALUE "FileVersion", "{VER0}.{VER1}.{VER2}.{VER3}"
            VALUE "LegalCopyright", "Copyright (c) {YEAR} {COMPANY}"
            VALUE "InternalName", "{FILENAME}"
            VALUE "OriginalFilename", "{FILENAME}"
            VALUE "ProductName", "{PRODUCT}"
            VALUE "ProductVersion", "{VER0}.{VER1}.{VER2}.{VER3} {VARIANT}"
        END
    END
    BLOCK "VarFileInfo"
    BEGIN
        VALUE "Translation", 0x409, 0000
    END
END
"""

def configure(conf):
	env = conf.env

	env.VER_COMPANY = 'Deja vu Security'

	env.VER_TEMPLATE = {
		'.c'      : template_c,
		'.cpp'    : template_c,
		'.cs'     : template_cs,
		'.rc'     : template_rc,
		'.cs_com' : template_cs_com,
	}

def apply_version(self, name, inputs, exts):
	if not self.env.VARIANT:
		return

	if not getattr(self, 'version', True):
		return

	exts = Utils.to_list(exts)

	self.env.VER_TITLE   = getattr(self, 'ver_title', name)
	self.env.VER_DESC    = getattr(self, 'ver_desc', name)
	self.env.VER_PRODUCT = getattr(self, 'ver_product', name)

	outputs = [ self.path.find_or_declare(name + '_version' + ext) for ext in exts ]
	tsk = self.create_task('version', inputs, outputs)
	self.source = Utils.to_list(self.source) + tsk.outputs

@feature('cs', 'msbuild')
@before_method('apply_cs', 'apply_msbuild')
@after_method('cs_helpers', 'msbuild_helpers')
def apply_version_cs(self):
	if not getattr(self, 'version', True):
		return

	asm = []
	others = []
	for x in self.to_nodes(self.source):
		if x.name.endswith('AssemblyInfo.cs'):
			asm.append(x)
		else:
			others.append(x)

	if len(asm) > 1:
		self.bld.fatal('cs task has more than one AssemblyInfo.cs for use %r' % self)

	self.source = others

	apply_version(self, os.path.splitext(self.gen)[0], asm, '.cs')

@feature('cxxprogram', 'cxxshlib')
@before_method('process_source')
def apply_version_cxx(self):
	exts = [ '.cpp' ]
	if self.env.CC_NAME == 'msvc':
		exts.append('.rc')
	apply_version(self, self.name, [], exts)

@feature('cprogram', 'cshlib')
@before_method('process_source')
def apply_version_c(self):
	exts = [ '.c' ]
	if self.env.CC_NAME == 'msvc':
		exts.append('.rc')
	apply_version(self, self.name, [], exts)

class version(Task.Task):
	color = 'PINK'
	vars = [ 'BUILDTAG', 'VER_COMPANY', 'VER_TITLE', 'VER_DESC', 'VER_PRODUCT', 'VER_TEMPLATE' ]

	def run(self):
		parts = self.env.BUILDTAG.split('.')

		fileflags = '0'
		if 'debug' in self.generator.bld.variant:
			fileflags += '|VS_FF_DEBUG'
			config = 'Debug'
		else:
			config = 'Release'

		if '0' == self.env.BUILDTAG:
			fileflags += '|VS_FF_PRERELEASE'

		filetype = 'VFT_UNKNOWN'
		tsk = getattr(self.generator, 'link_task', None)
		if tsk:
			filetype = 'program' in tsk.__class__.__name__ and 'VFT_APP' or 'VFT_DLL'
		else:
			tsk = getattr(self.generator, 'cs_task', None)
			if not tsk:
				raise Errors.WafError('Couldn\'t find link task on generator %s' % self.generator)

		fmt = {
			'TITLE'     : self.env.VER_TITLE,
			'DESC'      : self.env.VER_DESC,
			'YEAR'      : datetime.datetime.now().year,
			'CONFIG'    : config,
			'VARIANT'   : self.generator.bld.variant,
			'COMPANY'   : self.env.VER_COMPANY,
			'PRODUCT'   : self.env.VER_PRODUCT,
			'VER0'      : parts[0],
			'VER1'      : parts[1],
			'VER2'      : parts[2],
			'VER3'      : len(parts) == 4 and parts[3] or '0',
			'FILEFLAGS' : fileflags,
			'FILETYPE'  : filetype,
			'FILENAME'  : tsk.outputs[0].name,
		}

		for f in self.outputs:
			ext = os.path.splitext(f.name)[1]

			text = self.env.VER_TEMPLATE[ext].format(**fmt)

			if self.inputs:
				src = self.inputs[0].read()
				m = re.search(re_guid, src)
				if m:
				    guid = m.group(1)
				    m = re.search(re_com, src)
				    com = m and m.group(1) or 'false'
				    text += self.env.VER_TEMPLATE[ext + '_com'].format(COM=com, GUID=guid)

			f.write(text)
