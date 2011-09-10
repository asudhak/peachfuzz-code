#! /usr/bin/env python
# encoding: utf-8
# Thomas Nagy, 2005-2010

"""
to make a custom waf file use the option --tools

To add a tool that does not exist in the folder compat15, pass an absolute path:
./waf-light --make-waf --tools=compat15,/comp/waf/aba.py --prelude=$'\tfrom waflib.extras import aba\n\taba.foo()'
"""


VERSION="1.6.8"
APPNAME='waf'
REVISION=''

top = '.'
out = 'build'

demos = ['cpp', 'qt4', 'tex', 'ocaml', 'kde3', 'adv', 'cc', 'idl', 'docbook', 'xmlwaf', 'gnome']
zip_types = ['bz2', 'gz']

PRELUDE = '\timport waflib.extras.compat15'

#from tokenize import *
import tokenize

import os, sys, re, io, optparse

from waflib import Utils, Options
from hashlib import md5

from waflib import Configure
Configure.autoconfig = 1

def sub_file(fname, lst):

	f = open(fname, 'rU')
	txt = f.read()
	f.close()

	for (key, val) in lst:
		re_pat = re.compile(key, re.M)
		txt = re_pat.sub(val, txt)

	f = open(fname, 'w')
	f.write(txt)
	f.close()

print("------> Executing code from the top-level wscript <-----")
def init(*k, **kw):
	if Options.options.setver: # maintainer only (ita)
		ver = Options.options.setver
		hexver = Utils.num2ver(ver)
		hexver = '0x%x'%hexver
		sub_file('wscript', (('^VERSION=(.*)', 'VERSION="%s"' % ver), ))
		sub_file('waf-light', (('^VERSION=(.*)', 'VERSION="%s"' % ver), ))

		pats = []
		pats.append(('^WAFVERSION=(.*)', 'WAFVERSION="%s"' % ver))
		pats.append(('^HEXVERSION(.*)', 'HEXVERSION=%s' % hexver))

		try:
			rev = k[0].cmd_and_log('svnversion').strip().replace('M', '')
			pats.append(('^WAFREVISION(.*)', 'WAFREVISION="%s"' % rev))
		except:
			pass

		sub_file('waflib/Context.py', pats)

		sys.exit(0)
	elif Options.options.waf:
		create_waf()
		sys.exit(0)

def check(ctx):
	sys.path.insert(0,'')
	# some tests clobber g_module. We must preserve it here, otherwise we get an error
	# about an undefined shutdown function
	mod = Utils.g_module
	import test.Test
	test.Test.run_tests()
	Utils.g_module = mod

# this function is called before any other for parsing the command-line
def options(opt):

	# generate waf
	opt.add_option('--make-waf', action='store_true', default=False,
		help='creates the waf script', dest='waf')

	opt.add_option('--zip-type', action='store', default='bz2',
		help='specify the zip type [Allowed values: %s]' % ' '.join(zip_types), dest='zip')

	opt.add_option('--make-batch', action='store_true', default=False,
		help='creates a convenience waf.bat file (done automatically on win32 systems)',
		dest='make_batch')

	opt.add_option('--yes', action='store_true', default=False,
		help=optparse.SUPPRESS_HELP,
		dest='yes')

	# those ones are not too interesting
	opt.add_option('--set-version', default='',
		help='sets the version number for waf releases (for the maintainer)', dest='setver')

	opt.add_option('--strip', action='store_true', default=True,
		help='shrinks waf (strip docstrings, saves 33kb)',
		dest='strip_comments')
	opt.add_option('--nostrip', action='store_false', help='no shrinking',
		dest='strip_comments')
	opt.add_option('--tools', action='store', help='Comma-separated 3rd party tools to add, eg: "compat,ocaml" [Default: "compat15"]',
		dest='add3rdparty', default='compat15')
	opt.add_option('--prelude', action='store', help='Code to execute before calling waf', dest='prelude', default=PRELUDE)
	opt.load('python')

def compute_revision():
	global REVISION

	def visit(arg, dirname, names):
		for pos, name in enumerate(names):
			if name[0] == '.' or name in ['_build_', 'build']:
				del names[pos]
			elif name.endswith('.py'):
				arg.append(os.path.join(dirname, name))
	sources = []
	os.path.walk('waflib', visit, sources)
	sources.sort()
	m = md5()
	for source in sources:
		f = file(source,'rb')
		readBytes = 100000
		while (readBytes):
			readString = f.read(readBytes)
			m.update(readString)
			readBytes = len(readString)
		f.close()
	REVISION = m.hexdigest()

def process_tokens(tokens):
	accu = []
	prev = tokenize.NEWLINE

	indent = 0
	line_buf = []

	for (type, token, start, end, line) in tokens:
		token = token.replace('\r\n', '\n')
		if type == tokenize.NEWLINE:
			if line_buf:
				accu.append(indent * '\t')
				ln = "".join(line_buf)
				if ln == 'if __name__=="__main__":': break
				#ln = ln.replace('\n', '')
				accu.append(ln)
				accu.append('\n')
				line_buf = []
				prev = tokenize.NEWLINE
		elif type == tokenize.INDENT:
			indent += 1
		elif type == tokenize.DEDENT:
			indent -= 1
		elif type == tokenize.NAME:
			if prev == tokenize.NAME or prev == tokenize.NUMBER: line_buf.append(' ')
			line_buf.append(token)
		elif type == tokenize.NUMBER:
			if prev == tokenize.NAME or prev == tokenize.NUMBER: line_buf.append(' ')
			line_buf.append(token)
		elif type == tokenize.STRING:
			if not line_buf and token.startswith('"'): pass
			else: line_buf.append(token)
		elif type == tokenize.COMMENT:
			pass
		elif type == tokenize.OP:
			line_buf.append(token)
		else:
			if token != "\n": line_buf.append(token)

		if token != '\n':
			prev = type

	body = "".join(accu)
	return body

deco_re = re.compile('(def|class)\\s+(\w+)\\(.*')
def process_decorators(body):
	lst = body.split('\n')
	accu = []
	all_deco = []
	buf = [] # put the decorator lines
	for line in lst:
		if line.startswith('@'):
			buf.append(line[1:])
		elif buf:
			name = deco_re.sub('\\2', line)
			if not name:
				raise IOError("decorator not followed by a function!" + line)
			for x in buf:
				all_deco.append("%s(%s)" % (x, name))
			accu.append(line)
			buf = []
		else:
			accu.append(line)
	return "\n".join(accu+all_deco)

def sfilter(path):
	if sys.version_info[0] >= 3 and Options.options.strip_comments:
		f = open(path, "rb")
		tk = tokenize.tokenize(f.readline)
		next(tk) # the first one is always tokenize.ENCODING for Python 3, ignore it
		cnt = process_tokens(tk)
	elif Options.options.strip_comments and path.endswith('.py'):
		f = open(path, "r")
		cnt = process_tokens(tokenize.generate_tokens(f.readline))
	else:
		f = open(path, "r")
		cnt = f.read()

	f.close()
	if path.endswith('.py') :
		cnt = process_decorators(cnt)

		if cnt.find('set(') > -1:
			cnt = 'import sys\nif sys.hexversion < 0x020400f0: from sets import Set as set\n' + cnt
		cnt = '#! /usr/bin/env python\n# encoding: utf-8\n# WARNING! Do not edit! http://waf.googlecode.com/svn/docs/wafbook/single.html#_obtaining_the_waf_file\n\n' + cnt

	return (io.BytesIO(cnt.encode('utf-8')), len(cnt), cnt)

def create_waf(*k, **kw):
	#print("-> preparing waf")
	mw = 'tmp-waf-'+VERSION

	import tarfile, re

	zipType = Options.options.zip.strip().lower()
	if zipType not in zip_types:
		zipType = zip_types[0]

	#open a file as tar.[extension] for writing
	tar = tarfile.open('%s.tar.%s' % (mw, zipType), "w:%s" % zipType)

	files = []
	add3rdparty = []
	for x in Options.options.add3rdparty.split(','):
		if os.path.isabs(x):
			files.append(x)
		else:
			add3rdparty.append(x + '.py')

	for d in '. Tools extras'.split():
		dd = os.path.join('waflib', d)
		for k in os.listdir(dd):
			if k == '__init__.py':
				files.append(os.path.join(dd, k))
				continue
			if d == 'extras':
				if not k in add3rdparty:
					continue
			if k.endswith('.py'):
				files.append(os.path.join(dd, k))

	for x in files:
		tarinfo = tar.gettarinfo(x, x)
		tarinfo.uid   = tarinfo.gid   = 0
		tarinfo.uname = tarinfo.gname = 'root'
		(code, size, cnt) = sfilter(x)
		tarinfo.size = size

		if os.path.isabs(x):
			tarinfo.name = 'waflib/extras/' + os.path.split(x)[1]

		tar.addfile(tarinfo, code)
	tar.close()

	f = open('waf-light', 'rU')
	code1 = f.read()
	f.close()

	# now store the revision unique number in waf
	#compute_revision()
	#reg = re.compile('^REVISION=(.*)', re.M)
	#code1 = reg.sub(r'REVISION="%s"' % REVISION, code1)
	code1 = code1.replace("if sys.hexversion<0x206000f:\n\traise ImportError('Python >= 2.6 is required to create the waf file')\n", '')
	code1 = code1.replace('\timport waflib.extras.compat15#PRELUDE', Options.options.prelude)

	prefix = ''
	reg = re.compile('^INSTALL=(.*)', re.M)
	code1 = reg.sub(r'INSTALL=%r' % prefix, code1)
	#change the tarfile extension in the waf script
	reg = re.compile('bz2', re.M)
	code1 = reg.sub(zipType, code1)
	if zipType == 'gz':
		code1 = code1.replace('bunzip2', 'gzip -d')

	f = open('%s.tar.%s' % (mw, zipType), 'rb')
	cnt = f.read()
	f.close()

	# the REVISION value is the md5 sum of the binary blob (facilitate audits)
	m = md5()
	m.update(cnt)
	REVISION = m.hexdigest()
	reg = re.compile('^REVISION=(.*)', re.M)
	code1 = reg.sub(r'REVISION="%s"' % REVISION, code1)

	def find_unused(kd, ch):
		for i in range(35, 125):
			for j in range(35, 125):
				if i==j: continue
				if i == 39 or j == 39: continue
				if i == 92 or j == 92: continue
				s = chr(i) + chr(j)
				if -1 == kd.find(s.encode()):
					return (kd.replace(ch.encode(), s.encode()), s)
		raise

	# The reverse order prevent collisions
	(cnt, C2) = find_unused(cnt, '\r')
	(cnt, C1) = find_unused(cnt, '\n')
	f = open('waf', 'wb')

	ccc = code1.replace("C1='x'", "C1='%s'" % C1).replace("C2='x'", "C2='%s'" % C2)

	f.write(ccc.encode())
	f.write(b'#==>\n')
	f.write(b'#')
	f.write(cnt)
	f.write(b'\n')
	f.write(b'#<==\n')
	f.close()

	if sys.platform == 'win32' or Options.options.make_batch:
		f = open('waf.bat', 'w')
		f.write('@python -x "%~dp0waf" %* & exit /b\n')
		f.close()

	if sys.platform != 'win32':
		os.chmod('waf', Utils.O755)
	os.unlink('%s.tar.%s' % (mw, zipType))

def make_copy(inf, outf):
	(a, b, cnt) = sfilter(inf)
	f = open(outf, "wb")
	f.write(cnt)
	f.close()

def configure(conf):
	conf.load('python')
	conf.check_python_version((2,4))


def build(bld):
	waf = bld.path.make_node('waf') # create the node right here
	bld(name='create_waf', rule=create_waf, target=waf, always=True, color='PINK', update_outputs=True)

#def dist():
#	import Scripting
#	Scripting.g_dist_exts += ['Weak.py'] # shows how to exclude a file from dist
#	Scripting.Dist(APPNAME, VERSION)

