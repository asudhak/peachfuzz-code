#! /usr/bin/env python
# encoding: utf-8

"""
Windows-specific optimizations
"""

import os
try: import cPickle
except: import pickle as cPickle
from waflib import Utils, Build, Context, Node, Logs

if Utils.is_win32:
	import ctypes, ctypes.wintypes

	Context.DBFILE += '_md5tstamp'

	try:
		Build.BuildContext.store_real
	except AttributeError:
		pass
	else:
		raise ValueError('win32opts has been loaded twice (or the incompatible md5_tstamp tool was loaded before)')

	Build.hashes_md5_tstamp = {}
	Build.SAVED_ATTRS.append('hashes_md5_tstamp')
	def store(self):
		# save the hash cache as part of the default pickle file
		self.hashes_md5_tstamp = Build.hashes_md5_tstamp
		self.store_real()
	Build.BuildContext.store_real = Build.BuildContext.store
	Build.BuildContext.store      = store

	def restore(self):
		# we need a module variable for h_file below
		self.restore_real()
		try:
			Build.hashes_md5_tstamp = self.hashes_md5_tstamp or {}
		except Exception as e:
			Build.hashes_md5_tstamp = {}

	Build.BuildContext.restore_real = Build.BuildContext.restore
	Build.BuildContext.restore      = restore

	FindFirstFile        = ctypes.windll.kernel32.FindFirstFileW
	FindNextFile         = ctypes.windll.kernel32.FindNextFileW
	FindClose            = ctypes.windll.kernel32.FindClose
	FILE_ATTRIBUTE_DIRECTORY = 0x10
	INVALID_HANDLE_VALUE = -1
	UPPER_FOLDERS = ('.', '..')
	try:
		UPPER_FOLDERS = [unicode(x) for x in UPPER_FOLDERS]
	except NameError:
		pass

	def cached_hash_file(self):
		try:
			cache = self.ctx.cache_listdir_cache_hash_file
		except AttributeError:
			cache = self.ctx.cache_listdir_cache_hash_file = {}

		if id(self.parent) in cache:
			try:
				t = cache[id(self.parent)][self.name]
			except KeyError:
				raise IOError('Not a file')
		else:
			# an opportunity to list the files and the timestamps at once
			findData = ctypes.wintypes.WIN32_FIND_DATAW()
			find     = FindFirstFile(u'%s\\*' % self.parent.abspath(), ctypes.byref(findData))

			if find == INVALID_HANDLE_VALUE:
				cache[id(self.parent)] = {}
				raise IOError('Not a file')

			cache[id(self.parent)] = lst_files = {}
			try:
				while True:
					if findData.cFileName not in UPPER_FOLDERS:
						thatsadir = findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY
						if not thatsadir:
							ts = findData.ftLastWriteTime
							d = (ts.dwLowDateTime << 32) | ts.dwHighDateTime
							lst_files[str(findData.cFileName)] = d
					if not FindNextFile(find, ctypes.byref(findData)):
						break
			except Exception, e:
				cache[id(self.parent)] = {}
				raise IOError('Not a file')
			finally:
				FindClose(find)
			t = lst_files[self.name]

		filename = self.abspath()
		if filename in Build.hashes_md5_tstamp:
			if Build.hashes_md5_tstamp[filename][0] == t:
				return Build.hashes_md5_tstamp[filename][1]
		m = Utils.md5()

		with open(filename, 'rb') as f:
			read = 1
			while read:
				read = f.read(100000)
				m.update(read)

		# ensure that the cache is overwritten
		Build.hashes_md5_tstamp[filename] = (t, m.digest())
		return m.digest()
	Node.Node.cached_hash_file = cached_hash_file

	def get_bld_sig_win32(self):
		try:
			return self.ctx.hash_cache[id(self)]
		except KeyError:
			pass
		except AttributeError:
			self.ctx.hash_cache = {}

		if not self.is_bld():
			if self.is_child_of(self.ctx.srcnode):
				self.sig = self.cached_hash_file()
			else:
				self.sig = Utils.h_file(self.abspath())
		self.ctx.hash_cache[id(self)] = ret = self.sig
		return ret
	Node.Node.get_bld_sig = get_bld_sig_win32

	def isfile_cached(self):
		# optimize for nt.stat calls, assuming there are many files for few folders
		try:
			cache = self.__class__.cache_isfile_cache
		except AttributeError:
			cache = self.__class__.cache_isfile_cache = {}

		try:
			c1 = cache[id(self.parent)]
		except KeyError:
			c1 = cache[id(self.parent)] = []

			curpath = self.parent.abspath()
			findData = ctypes.wintypes.WIN32_FIND_DATAW()
			find     = FindFirstFile(u'%s\\*' % curpath, ctypes.byref(findData))

			if find == INVALID_HANDLE_VALUE:
				Logs.error("invalid win32 handle isfile_cached %r" % self.abspath())
				return os.path.isfile(self.abspath())

			try:
				while True:
					if findData.cFileName not in UPPER_FOLDERS:
						thatsadir = findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY
						if not thatsadir:
							c1.append(str(findData.cFileName))
					if not FindNextFile(find, ctypes.byref(findData)):
						break
			except Exception, e:
				Logs.error('exception while listing a folder %r %r' % (self.abspath(), e))
				return os.path.isfile(self.abspath())
			finally:
				FindClose(find)
		return self.name in c1
	Node.Node.isfile_cached = isfile_cached

	def find_or_declare_win32(self, lst):
		# assuming that "find_or_declare" is called before the build starts, remove the calls to os.path.isfile
		if isinstance(lst, str):
			lst = [x for x in Node.split_path(lst) if x and x != '.']

		node = self.get_bld().search(lst)
		if node:
			if not node.isfile_cached():
				node.sig = None
				try:
					node.parent.mkdir()
				except:
					pass
			return node
		self = self.get_src()
		node = self.find_node(lst)
		if node:
			if not node.isfile_cached():
				node.sig = None
				try:
					node.parent.mkdir()
				except:
					pass
			return node
		node = self.get_bld().make_node(lst)
		node.parent.mkdir()
		return node
	Node.Node.find_or_declare = find_or_declare_win32

