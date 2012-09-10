
import os, sys
from pyew_core import CPyew

sys.path.append("plugins")

#import diagrams
#import easygui
#import graphs
#import ole2
#import OleFileIO_PL
#import packer
#import pdf
#import pdfid_PL
#import shellcode
#import threatexpert
#import url
#import virustotal
#import vmdetect
#import xdot

filename = sys.argv[1]

pyew = CPyew(batch=True)
pyew.loadFile(filename, "rb")
pyew.offset = 0

#pyew.pe.header

for addr in pyew.basic_blocks.keys():
	print addr

