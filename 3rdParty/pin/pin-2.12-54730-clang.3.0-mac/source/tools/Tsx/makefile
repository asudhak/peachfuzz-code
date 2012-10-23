##
## PIN tools
##

##############################################################
#
# Here are some	things you might want to configure
#
##############################################################

TARGET_COMPILER?=gnu
ifdef OS
	ifeq (${OS},Windows_NT)
		TARGET_COMPILER=ms
	endif
endif



##############################################################
#
# include *.config files
#
##############################################################

ifeq ($(TARGET_COMPILER),gnu)
	include	../makefile.gnu.config
	STATIC=-static
endif

ifeq ($(TARGET_COMPILER),ms)
	include	../makefile.ms.config
	DBG?=
endif

ifneq ($(ENABLE_VS), 1)
	VS_FLAG	= -xyzzy -virtual_segments 1
else
	VS_FLAG	=
endif

SUF	= $(PINTOOL_SUFFIX)



###============================================================
##
## Set up the test tools
##
TEST_ROOTS = 
TEST_TOOLS = 
THREAD_LIB=$(OBJDIR)threadlib.$(OBJEXT)
ifeq ($(TARGET_OS),w)
    OS_API=windows
else
    OS_API=unix
endif

ifeq ($(TARGET),ia32e)
	TARGET_ARCH_NAME = Intel64
else
	TARGET_ARCH_NAME = $(TARGET)
endif

## linux only
## the tests run on ia32 and Intel64 
## Linux 2.4 is not supported 
ifeq ($(TARGET_OS),l)
    TSX_CHECK_ASM_OBJ = tsx_support_$(TARGET_ARCH_NAME).o
    TSX_CHECK = $(OBJDIR)tsx_check
    BUILD_ALL = build_all
    TEST_ALL = test_all
ifeq ($(TARGET),ia32e)
	SUPPORTS_TSX_ASM_OBJ = tsx_support_intel64.o
else
	SUPPORTS_TSX_ASM_OBJ = tsx_support_ia32.o
endif
endif



ifeq ($(TARGET_OS),w)
    TSX_CHECK_ASM_OBJ = tsx_support_$(TARGET_ARCH_NAME).obj
    TSX_CHECK = $(OBJDIR)tsx_check
    BUILD_ALL = build_all
    TEST_ALL = test_all
ifeq ($(TARGET),ia32e)
    SUPPORTS_TSX_ASM_OBJ = tsx_support_intel64.obj
else
    SUPPORTS_TSX_ASM_OBJ = tsx_support_ia32.obj
	TSX_ASM_OBJ = tsx_asm_ia32.obj
endif
endif


TEST_TOOLS_NO_TSX_SUPPORT = 


APPS_BINARY_FILES =	$(APPS:%=$(OBJDIR)%)
TOOLS =	$(TEST_TOOLS:%=$(OBJDIR)%$(PINTOOL_SUFFIX))
TESTS_TO_RUN = $(TEST_ROOTS:%=%.test)
ifeq ($(TARGET_OS),w)
	TESTS_TO_RUN += tsx1t.test tsx1t_tool.test tsx2t.test tsx3t.test tsx4t.test hlet1.test hlet2.test hlet3.test hlet4.test hlet5.test hlet6.test
	TEST_TOOLS += tsx_tool
endif
TESTS_TO_RUN_NO_TSX_SUPPORT = $(TEST_ROOTS_NO_TSX_SUPPORT:%=%.test)

MACHINE_SUPPORTS_TSX_INSTRUCTIONS =`./$(OBJDIR)tsx_check|grep -q Yes`


	

test_tsx: $(TESTS_TO_RUN)
test_no_tsx: $(TESTS_TO_RUN_NO_TSX_SUPPORT)

build_tsx: $(APPS_BINARY_FILES) $(TOOLS)
build_no_tsx: $(TEST_TOOLS_NO_TSX_SUPPORT:%=$(OBJDIR)%$(PINTOOL_SUFFIX))

$(OBJDIR):
	mkdir -p $(OBJDIR)
	

tests-sanity: test

all: $(BUILD_ALL)

build_all:
	make $(TSX_CHECK)
	if $(MACHINE_SUPPORTS_TSX_INSTRUCTIONS); then \
		echo "Building TSX tools and applications"; \
		make build_tsx; \
		else \
		echo "TSX instructions are not supported on this machine"; \
		make build_no_tsx; \
	fi

ifeq ($(TARGET_OS),w)	
$(OBJDIR)$(TSX_CHECK_ASM_OBJ): tsx_support_$(TARGET_ARCH_NAME).asm
	mkdir -p $(OBJDIR)
	$(MASM) /nologo /c /Fo$@ $<

$(OBJDIR)tsx_check: tsx_check.c $(OBJDIR)$(TSX_CHECK_ASM_OBJ)
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} tsx_check.c  $(OBJDIR)$(TSX_CHECK_ASM_OBJ) -o $@



ifeq ($(TARGET),ia32e)
	ARCHOPT=x64
	arch=64
	ARCHOPT=x64
else
	ARCHOPT=x86
	arch=32
	ARCHOPT=x86
endif
NTDLL_LIB =ntdll_$(TARGET).lib



else
$(OBJDIR)$(TSX_CHECK_ASM_OBJ): tsx_support_$(TARGET_ARCH_NAME).s
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} -c tsx_support_$(TARGET_ARCH_NAME).s -o $(OBJDIR)tsx_support_$(TARGET_ARCH_NAME).o

$(OBJDIR)tsx_check: tsx_check.c $(OBJDIR)$(TSX_CHECK_ASM_OBJ)
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} tsx_check.c -Wl,$(OBJDIR)$(TSX_CHECK_ASM_OBJ) -o $@



endif


$(OBJDIR)tsx_support_intel64.o: tsx_support_intel64.s
	mkdir -p $(OBJDIR)
	$(APP_CC) $(COPT) $(APP_CXXFLAGS) ${OUTOPT}$@ $<

$(OBJDIR)tsx_support_ia32.o: tsx_support_ia32.s
	mkdir -p $(OBJDIR)
	$(APP_CC) $(COPT) $(APP_CXXFLAGS) ${OUTOPT}$@ $<


$(OBJDIR)tsx_asm_ia32.obj: tsx_asm_ia32.asm
	mkdir -p $(OBJDIR)
	ml /nologo /c /Fo$@ $<



test: $(TEST_ALL)

test_all:
	make $(TSX_CHECK)
	if $(MACHINE_SUPPORTS_TSX_INSTRUCTIONS); then \
	   echo "Testing TSX tools and applications"; \
	   make test_tsx; \
	else \
	   echo "TSX instructions can not be tested on this machine"; \
	   make test_no_tsx; \
	fi




	
##======================================================
##  tests
##======================================================



tsx1t.test: %.test: tsx1t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)  %.tested %.failed
	$(PIN) -xyzzy -max_jit_recursion_level 10 -- tsx1t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
	rm $*.failed 
	
tsx1t_tool.test: %.test: $(OBJDIR)tsx_tool$(PINTOOL_SUFFIX) tsx1t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)   %.tested %.failed
	$(PIN) -xyzzy -max_jit_recursion_level 10 -t $< -- tsx1t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
	rm $*.failed  

tsx2t.test: %.test: tsx2t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)  %.tested %.failed
	$(PIN) -xyzzy -max_jit_recursion_level 10 -- tsx2t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
	rm $*.failed  

tsx3t.test: %.test: tsx3t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)  %.tested %.failed
	$(PIN) -xyzzy -max_jit_recursion_level 10 -- tsx3t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
	rm $*.failed 

tsx4t.test: %.test: tsx4t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)  %.tested %.failed
	$(PIN) -- tsx4t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
	rm $*.failed  
	
hlet1.test: %.test: hlet1_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)  %.tested %.failed
	$(PIN) -- hlet1_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
	rm $*.failed 
	
hlet2.test: %.test: hlet2_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)  %.tested %.failed
	$(PIN) -- hlet2_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
	rm $*.failed 
	
hlet3.test: %.test: hlet3_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)  %.tested %.failed
	$(PIN) -- hlet3_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
	rm $*.failed 
	
hlet4.test: %.test: hlet4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)  %.tested %.failed
	$(PIN) -- hlet4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
	rm $*.failed 
	
hlet5.test: %.test: hlet5_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)  %.tested %.failed
	$(PIN) -xyzzy -max_jit_recursion_level 10 -- hlet5_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
	rm $*.failed 
	
hlet6.test: %.test: hlet6_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)  %.tested %.failed
	$(PIN) -- hlet6_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
	rm $*.failed  


 
$(OBJDIR)%.o : %.cpp
	$(CXX) ${COPT} $(CXXFLAGS) $(PIN_CXXFLAGS) ${OUTOPT}$@ $<
    
$(TOOLS): $(OBJDIR)%$(PINTOOL_SUFFIX) :	$(OBJDIR)%.o $(PIN_LIBNAMES)
	${PIN_LD} $(PIN_LDFLAGS) $(LINK_DEBUG) ${LINK_OUT}$@ $<	${PIN_LPATHS} $(PIN_LIBS) $(DBG)

$(THREAD_LIB): ../threadlib/threadlib_$(OS_API).c ../threadlib/threadlib.h
	$(CC) $(APP_CXXFLAGS) ${COPT} ${OUTOPT}$@ $< 

ifneq ($(TARGET_OS),w)
tsx1t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: tsx1t_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)tsx1t_app tsx1t_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)tsx1t_app tsx1t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
ifeq ($(TARGET_ARCH_NAME),ia32)
tsx1t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: tsx1t_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)tsx1t_app tsx1t_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)tsx1t_app tsx1t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
    # need to compile with icl because cl does not support inline assembly see compile_tsx1t_app.bat
endif
endif


    # tsx2t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS needs to compile with icl because cl does not support inline assembly see compile_tsx2t_app.bat
	# and also use of various icl compiler intrinsics


ifneq ($(TARGET_OS),w)
tsx3t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: tsx3t_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)tsx3t_app tsx3t_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)tsx3t_app tsx3t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
ifeq ($(TARGET_ARCH_NAME),ia32)
tsx3t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: tsx3t_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)tsx3t_app tsx3t_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)tsx3t_app tsx3t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
    # need to compile with icl because cl does not support inline assembly see compile_tsx3t_app.bat
endif
endif

ifneq ($(TARGET_OS),w)
tsx4t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: tsx4t_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)tsx4t_app tsx4t_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)tsx4t_app tsx4t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
ifeq ($(TARGET_ARCH_NAME),ia32)
tsx4t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: tsx4t_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)tsx4t_app tsx4t_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)tsx4t_app tsx4t_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
    # need to compile with icl because cl does not support inline assembly see compile_tsx4t_app.bat
endif
endif


ifneq ($(TARGET_OS),w)
hlet1_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: hlet1_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)hlet1_app hlet1_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)hlet1_app hlet1_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
ifeq ($(TARGET_ARCH_NAME),ia32)
hlet1_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: hlet1_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)hlet1_app hlet1_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)hlet1_app hlet1_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
    # need to compile with icl because cl does not support inline assembly see compile_hlet1_app.bat
endif
endif

ifneq ($(TARGET_OS),w)
hlet2_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: hlet2_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)hlet2_app hlet2_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)hlet2_app hlet2_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
ifeq ($(TARGET_ARCH_NAME),ia32)
hlet2_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: hlet2_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)hlet2_app hlet2_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)hlet2_app hlet2_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
    # need to compile with icl because cl does not support inline assembly see compile_hlet2_app.bat
endif
endif

ifneq ($(TARGET_OS),w)
hlet3_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: hlet3_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)hlet3_app hlet3_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)hlet3_app hlet3_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
ifeq ($(TARGET_ARCH_NAME),ia32)
hlet3_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: hlet3_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)hlet3_app hlet3_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)hlet3_app hlet3_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
    # need to compile with icl because cl does not support inline assembly see compile_hlet3_app.bat
endif
endif

ifneq ($(TARGET_OS),w)
hlet4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: hlet4_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)hlet4_app hlet4_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)hlet4_app hlet4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
ifeq ($(TARGET_ARCH_NAME),ia32)
hlet4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: hlet4_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)hlet4_app hlet4_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)hlet4_app hlet4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
    # need to compile with icl because cl does not support inline assembly see compile_hlet4_app.bat
endif
endif

ifneq ($(TARGET_OS),w)
hlet5_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: hlet5_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)hlet5_app hlet5_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)hlet5_app hlet5_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
ifeq ($(TARGET_ARCH_NAME),ia32)
hlet5_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: hlet5_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)hlet5_app hlet5_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)hlet5_app hlet5_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
    # need to compile with icl because cl does not support inline assembly see compile_hlet5_app.bat
endif
endif

ifneq ($(TARGET_OS),w)
hlet6_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: hlet6_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)hlet6_app hlet6_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)hlet6_app hlet6_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
ifeq ($(TARGET_ARCH_NAME),ia32)
hlet6_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)	: hlet6_app.cpp ../threadlib/threadlib.h $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS)  ${OPT}  ${OUTEXE}$(OBJDIR)hlet6_app hlet6_app.cpp  $(THREAD_LIB)
	cp $(OBJDIR)hlet6_app hlet6_app_$(TARGET_ARCH_NAME)_$(TARGET_OS)
else
    # need to compile with icl because cl does not support inline assembly see compile_hlet6_app.bat
endif
endif


## cleaning
clean:
	-rm	-rf	$(OBJDIR) *.tested *.failed	*.obj *.o *.out *.log* *.txt *.pdb



