from waflib.TaskGen import feature, before_method, after_method, taskgen_method
from waflib import Task, Utils, Logs, Configure, Context, Options, Errors
import os, zipfile, sys, stat

def configure(conf):
	conf.env.append_value('supported_features', 'zip')

@taskgen_method
def use_zip_rec(self, name, **kw):
	if name in self.tmp_use_zip_not or name in self.tmp_use_zip_seen:
		return

	try:
		y = self.bld.get_tgen_by_name(name)
		self.tmp_use_zip_seen.append(name)
	except Errors.WafError:
		self.tmp_use_zip_not.append(name)
		return

	y.post()

	self.zip_use.append(y)

	for x in self.to_list(getattr(y, 'use', [])):
		self.use_zip_rec(x)

@taskgen_method
def get_zip_src(self, tsk):
	destpath = Utils.subst_vars(tsk.dest, tsk.env).replace('/', os.sep)
	bindir = Utils.subst_vars('${BINDIR}', tsk.env)
	destpath = os.path.relpath(destpath, bindir)

	for src in tsk.source:
		if not hasattr(tsk, 'relative_trick'):
			destfile = destpath
		elif tsk.relative_trick:
			destfile = os.path.join(destpath, src.path_from(tsk.path))
		else:
			destfile = os.path.join(destpath, src.name)

		external_attr = tsk.chmod << 16L

		destfile = os.path.normpath(destfile).replace('\\', '/')

		self.zip_inputs.append((src, destfile, external_attr))

@feature('zip')
@before_method('apply_zip_srcs')
def apply_zip_use(self):
	self.zip_use = []

	if not getattr(self.bld, 'is_pkg', False):
		return

	self.tmp_use_zip_not = []
	self.tmp_use_zip_seen = []

	for x in self.to_list(getattr(self, 'use', [])):
		self.use_zip_rec(x)

@feature('zip')
def apply_zip_srcs(self):
	self.zip_inputs = []

	for y in self.zip_use:
		tsk = getattr(y, 'install_task', None)
		if tsk:
			self.get_zip_src(tsk)
		for tsk in getattr(y, 'install_extras', []):
			self.get_zip_src(tsk)

	if self.zip_inputs:
		self.zip_inputs = sorted(self.zip_inputs, key=lambda x: x[1])
		srcs = [ x[0] for x in self.zip_inputs ]
		dest = self.path.find_or_declare(self.name + '.zip')
		self.zip_task = self.create_task('zip', srcs, dest)
		self.sha_task = self.create_task('sha', self.zip_task.outputs, dest.change_ext('.zip.sha1'))

		inst_to = getattr(self, 'install_path', '${OUTPUT}')
		self.install_files(inst_to, self.zip_task.outputs + self.sha_task.outputs)

class zip(Task.Task):
	color = 'PINK'

	def run(self):
		f = self.outputs[0]
		basename = os.path.splitext(f.name)[0]

		try:
			os.unlink(f.abspath())
		except Exception:
			pass

		z = zipfile.ZipFile(f.abspath(), 'w', compression=zipfile.ZIP_DEFLATED)

		for src, dest, attr in self.generator.zip_inputs:
			z.write(src.abspath(), dest)

			zi = z.getinfo(dest)
			zi.external_attr = attr

		z.close()

class sha(Task.Task):
	color = 'PINK'

	def run(self):
		try:
			from hashlib import sha1 as sha
		except ImportError:
			from sha import sha

		src = self.inputs[0]
		dst = self.outputs[0]

		digest = sha(src.read()).hexdigest()

		dst.write('SHA1(%s)= %s\n' % (src.name, digest))

