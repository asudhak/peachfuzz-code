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
	TARGET_ARCH_NAME = intel64
else
	TARGET_ARCH_NAME = $(TARGET)
endif

## linux only
## the tests run on ia32 and Intel64 
## Linux 2.4 is not supported 
ifeq ($(TARGET_OS),l)
    AVX2_CHECK_ASM_OBJ = avx2_support_$(TARGET_ARCH_NAME).o
    AVX2_CHECK = $(OBJDIR)avx2_check
    BUILD_ALL = build_all
    TEST_ALL = test_all
ifeq ($(TARGET),ia32e)
	SUPPORTS_AVX2_ASM_OBJ = avx2_support_intel64.o
else
	SUPPORTS_AVX2_ASM_OBJ = avx2_support_ia32.o
endif
endif



ifeq ($(TARGET_OS),w)
    AVX2_CHECK_ASM_OBJ = avx2_support_$(TARGET_ARCH_NAME).obj
    AVX2_CHECK = $(OBJDIR)avx2_check
    BUILD_ALL = build_all
    TEST_ALL = test_all
ifeq ($(TARGET),ia32e)
    SUPPORTS_AVX2_ASM_OBJ = avx2_support_intel64.obj
else
    SUPPORTS_AVX2_ASM_OBJ = avx2_support_ia32.obj
endif
endif


TEST_TOOLS_NO_AVX2_SUPPORT = 


APPS_BINARY_FILES =	$(APPS:%=$(OBJDIR)%)
TOOLS =	$(TEST_TOOLS:%=$(OBJDIR)%$(PINTOOL_SUFFIX))
TESTS_TO_RUN = $(TEST_ROOTS:%=%.test)
ifeq ($(TARGET_OS),w)
    TEST_TOOLS += gather_emu_rewrite_add4 gather_emu_rewrite1
	TESTS_TO_RUN += gather_emu_rewrite1_dps.test gather_emu_rewrite1_dpd.test gather_emu_rewrite1_qps.test gather_emu_rewrite1_qpd.test
TESTS_TO_RUN += gather_emu_rewrite_add4_dps.test gather_emu_rewrite_add4_dpd.test gather_emu_rewrite_add4_qps.test gather_emu_rewrite_add4_qpd.test
#cannot run on Wow64 due to mantis# 2282
ifeq ($(TARGET),ia32)
ifneq (${HOST_ARCH},ia32e)
	TESTS_TO_RUN += gather_emu_exception_and_rewrite1.test
endif
else
    TESTS_TO_RUN += gather_emu_exception_and_rewrite1.test
endif
endif
TESTS_TO_RUN_NO_AVX2_SUPPORT = $(TEST_ROOTS_NO_AVX2_SUPPORT:%=%.test)

MACHINE_SUPPORTS_AVX2_INSTRUCTIONS =`./$(OBJDIR)avx2_check|grep -q Yes`


	

test_avx2: $(TESTS_TO_RUN)
test_no_avx2: $(TESTS_TO_RUN_NO_AVX2_SUPPORT)

build_avx2: $(APPS_BINARY_FILES) $(TOOLS)
build_no_avx2: $(TEST_TOOLS_NO_AVX2_SUPPORT:%=$(OBJDIR)%$(PINTOOL_SUFFIX))

$(OBJDIR):
	mkdir -p $(OBJDIR)
	

tests-sanity: test

all: $(BUILD_ALL)

build_all:
	make $(AVX2_CHECK)
	if $(MACHINE_SUPPORTS_AVX2_INSTRUCTIONS); then \
		echo "Building AVX2 tools and applications"; \
		make build_avx2; \
		else \
		echo "AVX2 instructions are not supported on this machine"; \
		make build_no_avx2; \
	fi

ifeq ($(TARGET_OS),w)	
$(OBJDIR)$(AVX2_CHECK_ASM_OBJ): avx2_support_$(TARGET_ARCH_NAME).asm
	mkdir -p $(OBJDIR)
	$(MASM) /nologo /c /Fo$@ $<

$(OBJDIR)avx2_check: avx2_check.c $(OBJDIR)$(AVX2_CHECK_ASM_OBJ)
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} avx2_check.c  $(OBJDIR)$(AVX2_CHECK_ASM_OBJ) -o $@



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
$(OBJDIR)$(AVX2_CHECK_ASM_OBJ): avx2_support_$(TARGET_ARCH_NAME).s
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} -c avx2_support_$(TARGET_ARCH_NAME).s -o $(OBJDIR)avx2_support_$(TARGET_ARCH_NAME).o

$(OBJDIR)avx2_check: avx2_check.c $(OBJDIR)$(AVX2_CHECK_ASM_OBJ)
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} avx2_check.c -Wl,$(OBJDIR)$(AVX2_CHECK_ASM_OBJ) -o $@



endif



test: $(TEST_ALL)

test_all:
	make $(AVX2_CHECK)
	if $(MACHINE_SUPPORTS_AVX2_INSTRUCTIONS); then \
	   echo "Testing AVX2 tools and applications"; \
	   make test_avx2; \
	else \
	   echo "AVX2 instructions can not be tested on this machine"; \
	   make test_no_avx2; \
	fi


	
##======================================================
##  tests
##======================================================



gather_emu_exception_and_rewrite1.test: $(OBJDIR)gather_emu_rewrite1$(PINTOOL_SUFFIX) gather_emu_exception_and_rewrite1.tested gather_emu_exception_and_rewrite1.failed gather_with_exception_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gather_with_exception_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu_exception_and_rewrite1.failed $<.out 

gather_emu_rewrite1.test: $(OBJDIR)gather_emu_rewrite1$(PINTOOL_SUFFIX) gather_emu_rewrite1.tested gather_emu_rewrite1.failed gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu_rewrite1.failed $<.out 

gather_emu_rewrite1_dps.test: $(OBJDIR)gather_emu_rewrite1$(PINTOOL_SUFFIX) gather_emu_rewrite1_dps.tested gather_emu_rewrite1_dps.failed gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dps.out 2>&1
	rm gather_emu_rewrite1_dps.failed $<_dps.out 

gather_emu_rewrite1_dpd.test: $(OBJDIR)gather_emu_rewrite1$(PINTOOL_SUFFIX) gather_emu_rewrite1_dpd.tested gather_emu_rewrite1_dpd.failed gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dpd.out 2>&1
	rm gather_emu_rewrite1_dpd.failed $<_dpd.out 

gather_emu_rewrite1_qps.test: $(OBJDIR)gather_emu_rewrite1$(PINTOOL_SUFFIX) gather_emu_rewrite1_qps.tested gather_emu_rewrite1_qps.failed gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qps.out 2>&1
	rm gather_emu_rewrite1_qps.failed $<_qps.out 

gather_emu_rewrite1_qpd.test: $(OBJDIR)gather_emu_rewrite1$(PINTOOL_SUFFIX) gather_emu_rewrite1_qpd.tested gather_emu_rewrite1_qpd.failed gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qpd.out 2>&1
	rm gather_emu_rewrite1_qpd.failed $<_qpd.out 

gather_emu_rewrite_add4.test: $(OBJDIR)gather_emu_rewrite_add4$(PINTOOL_SUFFIX) gather_emu_rewrite_add4.tested gather_emu_rewrite_add4.failed gather_for_rewrite_add4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gather_for_rewrite_add4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu_rewrite_add4.failed $<.out

gather_emu_rewrite_add4_dpd.test: $(OBJDIR)gather_emu_rewrite_add4$(PINTOOL_SUFFIX) gather_emu_rewrite_add4_dpd.tested gather_emu_rewrite_add4_dpd.failed gatherdpd_for_rewrite_add4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdpd_for_rewrite_add4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu_rewrite_add4_dpd.failed $<.out

gather_emu_rewrite_add4_dps.test: $(OBJDIR)gather_emu_rewrite_add4$(PINTOOL_SUFFIX) gather_emu_rewrite_add4_dps.tested gather_emu_rewrite_add4_dps.failed gatherdps_for_rewrite_add4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdps_for_rewrite_add4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu_rewrite_add4_dps.failed $<.out

gather_emu_rewrite_add4_qpd.test: $(OBJDIR)gather_emu_rewrite_add4$(PINTOOL_SUFFIX) gather_emu_rewrite_add4_qpd.tested gather_emu_rewrite_add4_qpd.failed gatherqpd_for_rewrite_add4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqpd_for_rewrite_add4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu_rewrite_add4_qpd.failed $<.out

gather_emu_rewrite_add4_qps.test: $(OBJDIR)gather_emu_rewrite_add4$(PINTOOL_SUFFIX) gather_emu_rewrite_add4_qps.tested gather_emu_rewrite_add4_qps.failed gatherqps_for_rewrite_add4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqps_for_rewrite_add4_app_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu_rewrite_add4_qps.failed $<.out
 
$(OBJDIR)%.o : %.cpp
	$(CXX) ${COPT} $(CXXFLAGS) $(PIN_CXXFLAGS) ${OUTOPT}$@ $<
    
$(TOOLS): $(OBJDIR)%$(PINTOOL_SUFFIX) :	$(OBJDIR)%.o $(PIN_LIBNAMES)
	${PIN_LD} $(PIN_LDFLAGS) $(LINK_DEBUG) ${LINK_OUT}$@ $<	${PIN_LPATHS} $(PIN_LIBS) $(DBG)




## cleaning
clean:
	-rm	-rf	$(OBJDIR) *.tested *.failed	*.obj *.o *.out *.log* *.txt *.pdb



