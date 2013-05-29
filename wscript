#!/usr/bin/env python

# Import wscript contents from build/tools/wscript.py

import os.path
from tools import wscript

out = 'slag'
inst = 'output'
appname = 'peach'

def supported_variant(name):
	return True;

def init(ctx):
	wscript.init(ctx)

def options(opt):
	wscript.options(opt)

def configure(ctx):
	wscript.configure(ctx)

def build(ctx):
	wscript.build(ctx)
