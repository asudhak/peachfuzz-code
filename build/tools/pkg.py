from waflib.Build import InstallContext
from waflib import Task, Utils, Logs, Configure, Context, Options, Errors
import os, zipfile, sys

class ZipContext(InstallContext):
	'''zip contents of output directory'''

	cmd = 'zip'

	def __init__(self, **kw):
		super(self.__class__, self).__init__(**kw)
		self.installed_files = []

	def do_install(self, src, tgt, chmod=Utils.O644):
		self.installed_files.append(tgt)
		super(self.__class__, self).do_install(src, tgt, chmod)

	def execute(self):
		super(self.__class__, self).execute()
		if self.installed_files:
			self.archive()

	def archive(self):
		env = self.env
		version = '%s' % (env.BUILDTAG)
		args = [ env.APPNAME, version, env.TARGET ]
		if env.SUBARCH: args.append(env.SUBARCH)
		if env.VARIANT: args.append(env.VARIANT)

		base_path = self.path.make_node(env.BINDIR)
		base_name = '-'.join(args).lower()
		arch_name = '%s.zip' % (os.path.join(env.OUTPUT, base_name))

		Logs.warn('Creating archive: %s' % arch_name)

		node = self.path.make_node(arch_name)

		try:
			node.delete()
		except Exception:
			pass

		zip = zipfile.ZipFile(node.abspath(), 'w', compression=zipfile.ZIP_DEFLATED)

		for x in self.installed_files:
			n = self.path.find_node(x)
			if not n:
				raise Errors.WafError("Source not found: %s" % x)
			if not n.is_child_of(base_path):
				continue
			archive_name = n.path_from(base_path)
			if Logs.verbose > 0:
				Logs.info(' + add %s (from %s)' % (archive_name, x))
			else:
				sys.stdout.write('.')
				sys.stdout.flush()
			zip.write(n.abspath(), archive_name, zipfile.ZIP_DEFLATED)

		zip.close()


		if Logs.verbose == 0:
			sys.stdout.write('\n')

		try:
			from hashlib import sha1 as sha
		except ImportError:
			from sha import sha

		digest = sha(node.read()).hexdigest()
		dgst = 	self.path.make_node(arch_name + '.sha1')
		try:
			dgst.delete()
		except Exception:
			pass
		dgst.write('SHA1(%s.zip)= %s\n' % (base_name, digest))

		Logs.warn('New archive created: %s (sha1=%s)' % (arch_name, digest))

class PkgContext(InstallContext):
	'''create product installers'''

	cmd = 'pkg'

	def __init__(self, **kw):
		super(self.__class__, self).__init__(**kw)
		self.is_pkg = True

class PkgTask(Task.Task):
	def runnable_status(self):
		if getattr(self.generator.bld, 'is_pkg', None):
			return super(PkgTask, self).runnable_status()
		else:
			return Task.SKIP_ME
