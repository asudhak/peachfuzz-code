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

ifeq ($(TARGET),ia32e)
	TARGET_ARCH_NAME = intel64
else
	TARGET_ARCH_NAME = $(TARGET)
endif



###============================================================
##
## Set up the test tools
##
TEST_ROOTS = vmovdqu vmovqu_noinline  vmovdqu_allscratch ymm_ref_actual set_fp_context_ymm_regs_const_context 
TEST_TOOLS = ymm_ref set_ymm_scratches ymm_ref_actual gather_emu gather_emu1 gather_emu_buffer gather_emu_context gather_emu1_context gather_emu_buffer_context  gather_emu_memory_callback
TEST_TOOLS += gather_emu_rewrite1 
APPS = vmovqu_app ymm_app test_iarg_partial_context_xsave_xrstor_emulation_app test_iarg_partial_context_xsave_xrstor_emulation_set_in_xrstor_app

## linux only
## the tests run on ia32 and intel64 
## Linux 2.4 is not supported 
ifeq ($(TARGET_OS),l)
ifneq ($(TARGET),ipf)

    TEST_TOOLS += attach_detach_jit_tool ymm_ref set_fp_context_ymm_regs 
    APPS += attach_in_sighandler_app detach_in_sighandler_app set_fp_context_ymm_regs_app$(EXEEXT)
    TEST_ROOTS += attach_in_sighandler_ymm attach_in_sighandler_ymm1 detach_in_sighandler_ymm detach_in_sighandler_ymm
    AVX_CHECK_ASM_OBJ = avx_support_$(TARGET_ARCH_NAME).o
    AVX2_CHECK_ASM_OBJ = avx2_support_$(TARGET_ARCH_NAME).o
    AVX2_CHECK = $(OBJDIR)avx2_check
    VMOVQU_APP_ASM_OBJ = vmovqu_app_$(TARGET_ARCH_NAME).o
    SET_YMM_SCRATCHES_ASM_OBJ = set_ymm_scratches_$(TARGET_ARCH_NAME).o
    VERIFY_FP_CONTEXT_ASM_OBJ = verify_fp_context_$(TARGET_ARCH_NAME).o
    YMM_APP_ASM_OBJ = ymm_$(TARGET_ARCH_NAME).o
    AVX_CHECK = $(OBJDIR)avx_check
    BUILD_ALL = build_all
    TEST_ALL = test_all
    SET_FP_CONTEXT_YMM_REGS_APP=set_fp_context_ymm_regs_app
    SET_FP_CONTEXT_YMM_REGS_XMMS_SPILLED_APP=set_fp_context_ymm_regs_xmms_spilled_app
ifeq ($(TARGET),ia32e)
	SUPPORTS_AVX2_ASM_OBJ = avx2_support_intel64.o
	TEST_IARG_PRESERVE_ASM_OBJ = test_iarg_preserve_intel64_gnu.o
	SSE-REF_ASM_OBJ = sse-ref_intel64.o
	SUPPORTS_AVX_ASM_OBJ = supports_avx_intel64.o
else
	SUPPORTS_AVX2_ASM_OBJ = avx2_support_ia32.o
    TEST_IARG_PRESERVE_ASM_OBJ = test_iarg_preserve_ia32_gnu.o
	SSE-REF_ASM_OBJ = sse-ref_ia32.o
	SUPPORTS_AVX_ASM_OBJ = supports_avx_ia32.o
endif
endif
endif


ifeq ($(TARGET_OS),w)
    TEST_TOOLS += exception_context_monitor raise_exception set_fp_context_ymm_regs set_fp_context_ymm_regs_xmms_spilled set_ymm_scratches_allscratch set_xmm_scratches
    APPS +=  suspend_context_app_win winapp_exception_ntcontinue set_fp_context_ymm_regs_app$(EXEEXT) set_fp_context_ymm_regs_xmms_spilled_app$(EXEEXT)
    TEST_ROOTS += exception_context_monitor raise_exception vmovdqu_set_ymm_scratches_allscratch set_fp_context_ymm_regs_xmms_spilled
    AVX_CHECK_ASM_OBJ = avx_support_$(TARGET_ARCH_NAME).obj
	AVX2_CHECK_ASM_OBJ = avx2_support_$(TARGET_ARCH_NAME).obj
    AVX2_CHECK = $(OBJDIR)avx2_check
    VMOVQU_APP_ASM_OBJ = vmovqu_app_$(TARGET_ARCH_NAME).obj
    SET_YMM_SCRATCHES_ASM_OBJ = set_ymm_scratches_$(TARGET_ARCH_NAME).obj
    VERIFY_FP_CONTEXT_ASM_OBJ = verify_fp_context_$(TARGET_ARCH_NAME).obj
    YMM_APP_ASM_OBJ = ymm_$(TARGET_ARCH_NAME).obj
    AVX_CHECK = $(OBJDIR)avx_check
    BUILD_ALL = build_all
    TEST_ALL = test_all
    SET_FP_CONTEXT_YMM_REGS_APP=set_fp_context_ymm_regs_app$(EXEEXT)
    SET_FP_CONTEXT_YMM_REGS_XMMS_SPILLED_APP=set_fp_context_ymm_regs_xmms_spilled_app$(EXEEXT)
ifeq ($(TARGET),ia32e)
	SUPPORTS_AVX2_ASM_OBJ = avx2_support_intel64.obj
    SET_XMM_SCRATCHES_ASM_OBJ = set_xmm_scratches_asmintel64.o
	TEST_IARG_PRESERVE_ASM_OBJ = test_iarg_preserve_intel64_ms.obj
	SSE-REF_ASM_OBJ = sse-ref_intel64.obj
	SUPPORTS_AVX_ASM_OBJ = supports_avx_intel64.obj
else
	SUPPORTS_AVX2_ASM_OBJ = avx2_support_ia32.obj
    SET_XMM_SCRATCHES_ASM_OBJ = set_xmm_scratches_ia32.o
    TEST_IARG_PRESERVE_ASM_OBJ = test_iarg_preserve_ia32_ms.obj
	SSE-REF_ASM_OBJ = sse-ref_ia32.obj
	SUPPORTS_AVX_ASM_OBJ = supports_avx_ia32.obj
endif
endif

TEST_ROOTS_NO_AVX_SUPPORT = ymm_ref
TEST_TOOLS_NO_AVX_SUPPORT = ymm_ref


APPS_BINARY_FILES =	$(APPS:%=$(OBJDIR)%)
TOOLS =	$(TEST_TOOLS:%=$(OBJDIR)%$(PINTOOL_SUFFIX))
TESTS_TO_RUN = $(TEST_ROOTS:%=%.test)
TESTS_TO_RUN += test_iarg_partial_context_xsave_xrstor_emulation_const_context.test test_iarg_partial_context_xsave_xrstor_emulation_set_in_xrstor.test
TESTS_TO_RUN += test_iarg_partial_context_xsave_xrstor_emulation_regs_all.test
TESTS_TO_RUN += vmovdqu_no_xmm_partial.test vmovdqu_no_xmm_partial_no_inline.test
TESTS_TO_RUN += gather_emu_dps.test gather_emu_dpd.test gather_emu_qps.test gather_emu_qpd.test
TESTS_TO_RUN += gather_emu1_dps.test gather_emu1_dpd.test gather_emu1_qps.test gather_emu1_qpd.test
TESTS_TO_RUN += gather_emu_buffer_dps.test gather_emu_buffer_dpd.test gather_emu_buffer_qps.test gather_emu_buffer_qpd.test
TESTS_TO_RUN += gather_emu_context_dps.test gather_emu_context_dpd.test gather_emu_context_qps.test gather_emu_context_qpd.test
TESTS_TO_RUN += gather_emu1_context_dps.test gather_emu1_context_dpd.test gather_emu1_context_qps.test gather_emu1_context_qpd.test
TESTS_TO_RUN += gather_emu_buffer_context_dps.test gather_emu_buffer_context_dpd.test gather_emu_buffer_context_qps.test gather_emu_buffer_context_qpd.test
TESTS_TO_RUN += gather_emu_memory_callback.test
MACHINE_DOES_NOT_SUPPORT_AVX2_INSTRUCTIONS =`./$(OBJDIR)avx2_check|grep -q No`
ifeq ($(MACHINE_DOES_NOT_SUPPORT_AVX2_INSTRUCTIONS),1)
    TESTS_TO_RUN += TESTS_TO_RUN_NO_AVX2_SUPPORT
endif
ifeq ($(TARGET_OS),w)
    TESTS_TO_RUN_NO_AVX_SUPPORT = $(TEST_ROOTS_NO_AVX_SUPPORT:%=%.test)
endif

MACHINE_SUPPORTS_AVX_INSTRUCTIONS =`./$(OBJDIR)avx_check|grep -q Yes`

	

test_avx: $(TESTS_TO_RUN)
test_no_avx: $(TESTS_TO_RUN_NO_AVX_SUPPORT)

build_avx: $(APPS_BINARY_FILES) $(TOOLS)
build_no_avx: $(TEST_TOOLS_NO_AVX_SUPPORT:%=$(OBJDIR)%$(PINTOOL_SUFFIX))

$(OBJDIR):
	mkdir -p $(OBJDIR)
	

tests-sanity: test

all: $(BUILD_ALL)

build_all:
	make $(AVX_CHECK)
	make $(AVX2_CHECK)
	if $(MACHINE_SUPPORTS_AVX_INSTRUCTIONS); then \
		echo "Building AVX tools and applications"; \
		make build_avx; \
		else \
		echo "AVX instructions are not supported on this machine"; \
		make build_no_avx; \
	fi

ifeq ($(TARGET_OS),w)	
$(OBJDIR)$(AVX_CHECK_ASM_OBJ): avx_support_$(TARGET_ARCH_NAME).asm
	mkdir -p $(OBJDIR)
	$(MASM) /nologo /c /Fo$@ $<

$(OBJDIR)avx_check: avx_check.c $(OBJDIR)$(AVX_CHECK_ASM_OBJ)
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} avx_check.c  $(OBJDIR)$(AVX_CHECK_ASM_OBJ) -o $@

$(OBJDIR)$(AVX2_CHECK_ASM_OBJ): avx2_support_$(TARGET_ARCH_NAME).asm
	mkdir -p $(OBJDIR)
	$(MASM) /nologo /c /Fo$@ $<

$(OBJDIR)avx2_check: avx2_check.c $(OBJDIR)$(AVX2_CHECK_ASM_OBJ)
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} avx2_check.c  $(OBJDIR)$(AVX2_CHECK_ASM_OBJ) -o $@

$(OBJDIR)$(VMOVQU_APP_ASM_OBJ): vmovqu_app_$(TARGET_ARCH_NAME).asm
	mkdir -p $(OBJDIR)
	$(MASM) /nologo /c /Fo$@ $<

$(OBJDIR)$(SET_YMM_SCRATCHES_ASM_OBJ): set_ymm_scratches_$(TARGET_ARCH_NAME).asm
	mkdir -p $(OBJDIR)
	$(MASM) /nologo /c /Fo$@ $<

$(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ): verify_fp_context_$(TARGET_ARCH_NAME).asm
	mkdir -p $(OBJDIR)
	$(MASM) /nologo /c /Fo$@ $<

$(OBJDIR)$(YMM_APP_ASM_OBJ): ymm_$(TARGET_ARCH_NAME).asm
	mkdir -p $(OBJDIR)
	$(MASM) /nologo /c /Fo$@ $<

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

$(OBJDIR)$(NTDLL_LIB):  ntdll.def
	cl /DWIN$(arch) /D_WIN32_WINNT=0x501 /D_NTDLL_IMPLIB /c /Tcntdll.h /Fo$(OBJDIR)ntdll_$(TARGET).obj /nologo
ifeq ($(TARGET),ia32e)
	lib $(OBJDIR)ntdll_$(TARGET).obj  /machine:x64 /def:ntdll.def /out:$(OBJDIR)$(NTDLL_LIB) /nologo
else
	lib $(OBJDIR)ntdll_$(TARGET).obj  /machine:x86 /def:ntdll.def /out:$(OBJDIR)$(NTDLL_LIB) /nologo
endif

else
$(OBJDIR)$(AVX_CHECK_ASM_OBJ): avx_support_$(TARGET_ARCH_NAME).s
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} -c avx_support_$(TARGET_ARCH_NAME).s -o $(OBJDIR)avx_support_$(TARGET_ARCH_NAME).o

$(OBJDIR)avx_check: avx_check.c $(OBJDIR)$(AVX_CHECK_ASM_OBJ)
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} avx_check.c -Wl,$(OBJDIR)$(AVX_CHECK_ASM_OBJ) -o $@

$(OBJDIR)$(AVX2_CHECK_ASM_OBJ): avx2_support_$(TARGET_ARCH_NAME).s
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} -c avx2_support_$(TARGET_ARCH_NAME).s -o $(OBJDIR)avx2_support_$(TARGET_ARCH_NAME).o

$(OBJDIR)avx2_check: avx2_check.c $(OBJDIR)$(AVX2_CHECK_ASM_OBJ)
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} avx2_check.c -Wl,$(OBJDIR)$(AVX2_CHECK_ASM_OBJ) -o $@

$(OBJDIR)$(VMOVQU_APP_ASM_OBJ): vmovqu_app_$(TARGET_ARCH_NAME).s
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} -c vmovqu_app_$(TARGET_ARCH_NAME).s -o $(OBJDIR)vmovqu_app_$(TARGET_ARCH_NAME).o

$(OBJDIR)$(SET_YMM_SCRATCHES_ASM_OBJ): set_ymm_scratches_$(TARGET_ARCH_NAME).s
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} -c set_ymm_scratches_$(TARGET_ARCH_NAME).s -o $(OBJDIR)set_ymm_scratches_$(TARGET_ARCH_NAME).o

$(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ): verify_fp_context_$(TARGET_ARCH_NAME).s
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} -c verify_fp_context_$(TARGET_ARCH_NAME).s -o $(OBJDIR)verify_fp_context_$(TARGET_ARCH_NAME).o

$(OBJDIR)$(YMM_APP_ASM_OBJ): ymm_$(TARGET_ARCH_NAME).s
	mkdir -p $(OBJDIR)
	$(APP_CC) ${APP_CXXFLAGS} -c ymm_$(TARGET_ARCH_NAME).s -o $(OBJDIR)ymm_$(TARGET_ARCH_NAME).o

endif

$(OBJDIR)test_iarg_preserve_intel64_ms.obj: test_iarg_preserve_intel64_ms.asm
	ml64 /nologo /c /Fo$@ $<

$(OBJDIR)test_iarg_preserve_ia32_ms.obj: test_iarg_preserve_ia32_ms.asm
	ml /nologo /c /Fo$@ $<

$(OBJDIR)test_iarg_preserve_ia32_gnu.o: test_iarg_preserve_ia32_gnu.s 
	$(APP_CC) $(APP_CXXFLAGS) -x assembler-with-cpp -c $< -o $@

$(OBJDIR)test_iarg_preserve_intel64_gnu.o: test_iarg_preserve_intel64_gnu.s 
	$(CC) $(APP_CXXFLAGS) -fPIC -x assembler-with-cpp -c $< -o $@

$(OBJDIR)sse-ref_intel64.obj: sse-ref_intel64.asm
	ml64 /nologo /c /Fo$@ $<

$(OBJDIR)sse-ref_ia32.obj: sse-ref_ia32.asm
	ml /nologo /c /Fo$@ $<

$(OBJDIR)sse-ref_ia32.o: sse-ref_ia32.s 
	$(APP_CC) $(APP_CXXFLAGS) -x assembler-with-cpp -c $< -o $@

$(OBJDIR)sse-ref_intel64.o: sse-ref_intel64.s 
	$(APP_CC) $(APP_CXXFLAGS) -fPIC -x assembler-with-cpp -c $< -o $@

$(OBJDIR)supports_avx_ia32.obj: supports_avx_ia32.asm
	ml /nologo /c /Fo$@ $<

$(OBJDIR)supports_avx_intel64.obj: supports_avx_intel64.asm
	ml64 /nologo /c /Fo$@ $<

ifeq ($(TARGET),ia32e)
$(OBJDIR)$(SET_XMM_SCRATCHES_ASM_OBJ): set_xmm_scratches_asmintel64.asm
	mkdir -p $(OBJDIR)
	$(MASM) /nologo /c /Fo$@ $<
else
$(OBJDIR)$(SET_XMM_SCRATCHES_ASM_OBJ): set_xmm_scratches_ia32.asm
	mkdir -p $(OBJDIR)
	$(MASM) /nologo /c /Fo$@ $<
endif

$(OBJDIR)supports_avx_intel64.o: supports_avx_intel64.s
	$(APP_CC) $(COPT) $(APP_CXXFLAGS) ${OUTOPT}$@ $<

$(OBJDIR)supports_avx_ia32.o: supports_avx_ia32.s
	$(APP_CC) $(COPT) $(APP_CXXFLAGS) ${OUTOPT}$@ $<

$(OBJDIR)vmovqu_app	: vmovqu_app.c $(OBJDIR)$(VMOVQU_APP_ASM_OBJ)
	$(APP_CC) $(APP_CXXFLAGS) ${OUTEXE}$(OBJDIR)vmovqu_app vmovqu_app.c $(OBJDIR)$(VMOVQU_APP_ASM_OBJ)

$(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation_app: test_iarg_partial_context_xsave_xrstor_emulation_app.cpp $(OBJDIR)$(TEST_IARG_PRESERVE_ASM_OBJ) $(OBJDIR)$(SSE-REF_ASM_OBJ)
	${APP_CXX} $(APP_CXXFLAGS) $(NO_LOGO) $(DBG) $(NO_OPTIMIZE) $(SSE2) ${OUTEXE}$@ $< $(APP_CXXLINK_FLAGS) $(OBJDIR)$(TEST_IARG_PRESERVE_ASM_OBJ) $(OBJDIR)$(SSE-REF_ASM_OBJ)

$(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation_set_in_xrstor_app: test_iarg_partial_context_xsave_xrstor_emulation_set_in_xrstor_app.cpp $(OBJDIR)$(TEST_IARG_PRESERVE_ASM_OBJ) $(OBJDIR)$(SSE-REF_ASM_OBJ)
	${APP_CXX} $(APP_CXXFLAGS) $(NO_LOGO) $(DBG) $(NO_OPTIMIZE) $(SSE2) ${OUTEXE}$@ $< $(APP_CXXLINK_FLAGS) $(OBJDIR)$(TEST_IARG_PRESERVE_ASM_OBJ) $(OBJDIR)$(SSE-REF_ASM_OBJ)


$(OBJDIR)set_fp_context_ymm_regs_app : set_fp_context_ymm_regs_app.cpp $(OBJDIR)$(YMM_APP_ASM_OBJ) $(THREAD_LIB)
	$(APP_CXX) $(APP_CXXFLAGS) ${OUTEXE}$(OBJDIR)set_fp_context_ymm_regs_app set_fp_context_ymm_regs_app.cpp $(OBJDIR)$(YMM_APP_ASM_OBJ) $(THREAD_LIB)

$(OBJDIR)set_fp_context_ymm_regs_xmms_spilled_app : set_fp_context_ymm_regs_xmms_spilled_app.cpp $(OBJDIR)$(YMM_APP_ASM_OBJ) $(THREAD_LIB) $(OBJDIR)$(SSE-REF_ASM_OBJ) 
	$(APP_CXX) $(APP_CXXFLAGS) ${OUTEXE}$(OBJDIR)set_fp_context_ymm_regs_xmms_spilled_app set_fp_context_ymm_regs_xmms_spilled_app.cpp $(OBJDIR)$(YMM_APP_ASM_OBJ) $(THREAD_LIB) $(OBJDIR)$(SSE-REF_ASM_OBJ) 

$(OBJDIR)set_fp_context_ymm_regs_xmms_spilled_app.exe : set_fp_context_ymm_regs_xmms_spilled_app.cpp $(OBJDIR)$(YMM_APP_ASM_OBJ) $(THREAD_LIB) $(OBJDIR)$(SSE-REF_ASM_OBJ) 
	${APP_CXX} $(APP_CXXFLAGS2) $(NO_LOGO) $(DBG) $(NO_OPTIMIZE) $(SSE2) ${OUTEXE}$@ $< $(APP_CXXLINK_FLAGS) $(OBJDIR)$(YMM_APP_ASM_OBJ) $(THREAD_LIB) $(OBJDIR)$(SSE-REF_ASM_OBJ)  

$(OBJDIR)ymm_app	: ymm_app.cpp $(OBJDIR)$(YMM_APP_ASM_OBJ)
	$(APP_CXX) $(APP_CXXFLAGS) ${OUTEXE}$(OBJDIR)ymm_app ymm_app.cpp $(OBJDIR)$(YMM_APP_ASM_OBJ)

$(OBJDIR)winapp_exception_context	: winapp_exception_context.cpp  $(OBJDIR)$(YMM_APP_ASM_OBJ)  $(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ)
	${APP_CXX} $(APP_CXXFLAGS) $(NO_LOGO) $(DBG) $(NO_OPTIMIZE)  ${OUTEXE}$@ $< $(APP_CXXLINK_FLAGS) $(OBJDIR)$(YMM_APP_ASM_OBJ) $(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ)

$(OBJDIR)raise_exception_app	: raise_exception_app.cpp  $(OBJDIR)$(YMM_APP_ASM_OBJ)  $(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ)
	${APP_CXX} $(APP_CXXFLAGS) $(NO_LOGO) $(DBG) $(NO_OPTIMIZE)  ${OUTEXE}$@ $< $(APP_CXXLINK_FLAGS)  $(OBJDIR)$(YMM_APP_ASM_OBJ)  $(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ)

$(OBJDIR)suspend_context_app_win	: suspend_context_app_win.cpp  $(OBJDIR)$(YMM_APP_ASM_OBJ) $(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ)
	${APP_CXX} $(APP_CXXFLAGS) $(NO_LOGO) $(DBG) $(NO_OPTIMIZE)  ${OUTEXE}$@ $< $(APP_CXXLINK_FLAGS)  $(OBJDIR)$(YMM_APP_ASM_OBJ) $(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ)
	
$(OBJDIR)set_fp_context_ymm_regs_app.exe: set_fp_context_ymm_regs_app.cpp $(OBJDIR)$(YMM_APP_ASM_OBJ) $(THREAD_LIB)
	${APP_CXX} $(APP_CXXFLAGS2) $(NO_LOGO) $(DBG) $(NO_OPTIMIZE) $(SSE2) ${OUTEXE}$@ $< $(APP_CXXLINK_FLAGS) $(OBJDIR)$(YMM_APP_ASM_OBJ) $(THREAD_LIB)

$(OBJDIR)winapp_exception_ntcontinue : winapp_exception_ntcontinue.cpp $(OBJDIR)$(YMM_APP_ASM_OBJ) $(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ) $(OBJDIR)$(NTDLL_LIB)
	${APP_CXX} $(APP_CXXFLAGS) $(NO_LOGO) $(DBG) $(NO_OPTIMIZE)  ${OUTEXE}$@ $< $(APP_CXXLINK_FLAGS)  $(OBJDIR)$(YMM_APP_ASM_OBJ) $(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ) $(OBJDIR)$(NTDLL_LIB) 

test: $(TEST_ALL)

test_all:
	make $(AVX_CHECK)
	make $(AVX2_CHECK)
	if $(MACHINE_SUPPORTS_AVX_INSTRUCTIONS); then \
	   echo "Testing AVX tools and applications"; \
	   make test_avx; \
	else \
	   echo "AVX instructions can not be tested on this machine"; \
	   make test_no_avx; \
	fi




	
##======================================================
##  tests
##======================================================

set_fp_context_ymm_regs.test : $(OBJDIR)set_fp_context_ymm_regs$(PINTOOL_SUFFIX) $(OBJDIR)$(SET_FP_CONTEXT_YMM_REGS_APP)  set_fp_context_ymm_regs.tested set_fp_context_ymm_regs.failed 
	$(PIN) -t $< -- ./$(OBJDIR)$(SET_FP_CONTEXT_YMM_REGS_APP) >  $<.out 2>&1 
ifeq ($(TARGET),ia32)
	$(PIN_DIFF) $<.out set_fp_context_ymm_regs_ia32.reference 
else
	$(PIN_DIFF) $<.out set_fp_context_ymm_regs_intel64.reference 
endif
	rm set_fp_context_ymm_regs.failed $<.out 

set_fp_context_ymm_regs_xmms_spilled.test : $(OBJDIR)set_fp_context_ymm_regs_xmms_spilled$(PINTOOL_SUFFIX) $(OBJDIR)$(SET_FP_CONTEXT_YMM_REGS_XMMS_SPILLED_APP)  set_fp_context_ymm_regs_xmms_spilled.tested set_fp_context_ymm_regs_xmms_spilled.failed 
	$(PIN) -t $< -- ./$(OBJDIR)$(SET_FP_CONTEXT_YMM_REGS_XMMS_SPILLED_APP) >  $<.out 2>&1 
	rm set_fp_context_ymm_regs_xmms_spilled.failed $<.out 
	
set_fp_context_ymm_regs_const_context.test : $(OBJDIR)set_fp_context_ymm_regs$(PINTOOL_SUFFIX) $(OBJDIR)$(SET_FP_CONTEXT_YMM_REGS_APP)  set_fp_context_ymm_regs_const_context.tested set_fp_context_ymm_regs_const_context.failed 
	$(PIN) -t $< -const_context -- ./$(OBJDIR)$(SET_FP_CONTEXT_YMM_REGS_APP) >  $<.out 2>&1 
ifeq ($(TARGET),ia32)
	$(PIN_DIFF) $<.out set_fp_context_ymm_regs_ia32.reference 
else
	$(PIN_DIFF) $<.out set_fp_context_ymm_regs_intel64.reference 
endif
	rm set_fp_context_ymm_regs_const_context.failed $<.out
	
attach_in_sighandler_ymm.test: %.test: $(OBJDIR)attach_in_sighandler_app $(OBJDIR)attach_detach_jit_tool${PINTOOL_SUFFIX} %.tested %.failed
	./$< -pin $(PIN) -pinarg  -t $(OBJDIR)attach_detach_jit_tool${PINTOOL_SUFFIX} -o $(OBJDIR)$*.log >$*.out  2>&1
	grep "ymm values are correct" $*.out
	rm $*.failed $*.out $(OBJDIR)$*.log

attach_in_sighandler_ymm1.test: %.test: $(OBJDIR)attach_in_sighandler_app $(OBJDIR)attach_detach_jit_tool${PINTOOL_SUFFIX} %.tested %.failed
	./$< -test 1 -pin $(PIN) -pinarg  -t $(OBJDIR)attach_detach_jit_tool${PINTOOL_SUFFIX} -o $(OBJDIR)$*.log >$*.out  2>&1
	grep "ymm values are correct" $*.out
	rm $*.failed $*.out $(OBJDIR)$*.log
                
detach_in_sighandler_ymm.test: %.test: $(OBJDIR)detach_in_sighandler_app $(OBJDIR)attach_detach_jit_tool${PINTOOL_SUFFIX} %.tested %.failed
	$(PIN) -t $(OBJDIR)attach_detach_jit_tool${PINTOOL_SUFFIX} -o $(OBJDIR)$*.log -- $(OBJDIR)detach_in_sighandler_app >$*.out  2>&1
	grep "ymm values are correct" $*.out
	rm $*.failed $*.out $(OBJDIR)$*.log

detach_in_sighandler_ymm1.test: %.test: $(OBJDIR)detach_in_sighandler_app $(OBJDIR)attach_detach_jit_tool${PINTOOL_SUFFIX} %.tested %.failed
	$(PIN) -t $(OBJDIR)attach_detach_jit_tool${PINTOOL_SUFFIX} -o $(OBJDIR)$*.log -- $(OBJDIR)detach_in_sighandler_app -test 1 >$*.out  2>&1
	grep "ymm values are correct" $*.out
	rm $*.failed $*.out $(OBJDIR)$*.log
    
ymm_ref.test : $(OBJDIR)ymm_ref$(PINTOOL_SUFFIX) ymm_ref.tested ymm_ref.failed
	echo "ymm_ref should fail.  Ignore the error."
	-$(PIN) -xyzzy  -logfile ymm_ref.log -t $< --  $(TESTAPP) makefile $<.makefile.copy >  $<.out 2>&1
	grep -q "ymm registers are NOT supported when Pin is running on as OS/processor without AVX support" ymm_ref.log
	rm ymm_ref.failed $<.out

ymm_ref_actual.test	: $(OBJDIR)ymm_ref_actual$(PINTOOL_SUFFIX)	ymm_ref_actual.tested ymm_ref_actual.failed	$(OBJDIR)ymm_app	
	$(PIN) -t $< -- ./$(OBJDIR)ymm_app
	rm -f ymm_ref_actual.failed 

vmovdqu.test	: $(OBJDIR)set_ymm_scratches$(PINTOOL_SUFFIX)	vmovqu.tested vmovqu.failed	$(OBJDIR)vmovqu_app
	echo echoing $(TESTS_TO_RUN_NO_AVX2_SUPPORT)
	$(PIN) -t $< -- ./$(OBJDIR)vmovqu_app
	rm -f vmovqu.failed

vmovdqu_set_ymm_scratches_allscratch.test    : $(OBJDIR)set_ymm_scratches_allscratch$(PINTOOL_SUFFIX)   vmovqu_set_ymm_scratches_allscratch.tested vmovqu_set_ymm_scratches_allscratch.failed     $(OBJDIR)vmovqu_app
	$(PIN) -t $< -- ./$(OBJDIR)vmovqu_app
	rm -f vmovqu_set_ymm_scratches_allscratch.failed

vmovdqu_no_xmm_partial.test : $(OBJDIR)set_ymm_scratches$(PINTOOL_SUFFIX)	vmovdqu_no_xmm_partial.tested vmovdqu_no_xmm_partial.failed	$(OBJDIR)vmovqu_app	
	$(PIN) -xyzzy -separately_handled_xmm 0 -t $< -- ./$(OBJDIR)vmovqu_app
	rm -f vmovdqu_no_xmm_partial.failed

vmovdqu_no_xmm_partial_no_inline.test : $(OBJDIR)set_ymm_scratches$(PINTOOL_SUFFIX)	vmovdqu_no_xmm_partial_no_inline.tested vmovdqu_no_xmm_partial_no_inline.failed	$(OBJDIR)vmovqu_app	
	$(PIN) -xyzzy -separately_handled_xmm 0 -inline 0 -t $< -- ./$(OBJDIR)vmovqu_app
	rm -f vmovdqu_no_xmm_partial_no_inline.failed

set_xmm_scratches.test	: $(OBJDIR)set_xmm_scratches$(PINTOOL_SUFFIX)	set_xmm_scratches.tested set_xmm_scratches.failed	$(OBJDIR)vmovqu_app	
	$(PIN) -t $< -- ./$(OBJDIR)vmovqu_app
	rm -f set_xmm_scratches.failed

set_xmm_scratches_no_inline.test	: $(OBJDIR)set_xmm_scratches$(PINTOOL_SUFFIX)	set_xmm_scratches_no_inline.tested set_xmm_scratches_no_inline.failed	$(OBJDIR)vmovqu_app	
	$(PIN) -xyzzy -inline 0 -t $< -- ./$(OBJDIR)vmovqu_app
	rm -f set_xmm_scratches_no_inline.failed

set_xmm_scratches_no_xmm_partial.test	: $(OBJDIR)set_xmm_scratches$(PINTOOL_SUFFIX)	set_xmm_scratches_no_xmm_partial.tested set_xmm_scratches_no_xmm_partial.failed	$(OBJDIR)vmovqu_app	
	$(PIN) -xyzzy -separately_handled_xmm 0  -t $< -- ./$(OBJDIR)vmovqu_app
	rm -f set_xmm_scratches_no_xmm_partial.failed

set_xmm_scratches_no_xmm_partial_no_inline.test	: $(OBJDIR)set_xmm_scratches$(PINTOOL_SUFFIX)	set_xmm_scratches_no_xmm_partial_no_inline.tested set_xmm_scratches_no_xmm_partial_no_inline.failed	$(OBJDIR)vmovqu_app	
	$(PIN) -xyzzy -inline 0 -separately_handled_xmm 0 -t $< -- ./$(OBJDIR)vmovqu_app
	rm -f set_xmm_scratches_no_xmm_partial_no_inline.failed


vmovqu_noinline.test	: $(OBJDIR)set_ymm_scratches$(PINTOOL_SUFFIX)	vmovqu_noinline.tested vmovqu_noinline.failed	$(OBJDIR)vmovqu_app	
	$(PIN) -xyzzy -inline 0 -t $< -- ./$(OBJDIR)vmovqu_app
	rm -f vmovqu_noinline.failed

vmovdqu_allscratch.test	: $(OBJDIR)set_ymm_scratches_allscratch$(PINTOOL_SUFFIX)	vmovdqu_allscratch.tested vmovdqu_allscratch.failed	$(OBJDIR)vmovqu_app	
	$(PIN) -t $< -- ./$(OBJDIR)vmovqu_app
	rm -f vmovdqu_allscratch.failed
	
exception_context_monitor.test	: $(OBJDIR)exception_context_monitor$(PINTOOL_SUFFIX)	exception_context_monitor.tested exception_context_monitor.failed	$(OBJDIR)winapp_exception_context	
	$(PIN) -t $< -- ./$(OBJDIR)winapp_exception_context
	rm -f exception_context_monitor.failed 
	
raise_exception.test	: $(OBJDIR)raise_exception$(PINTOOL_SUFFIX)	raise_exception.tested raise_exception.failed	$(OBJDIR)raise_exception_app	
	$(PIN) -t $< -- ./$(OBJDIR)raise_exception_app
	rm -f raise_exception.failed  

suspend_context_win.test	:	suspend_context.tested suspend_context.failed	$(OBJDIR)suspend_context_app_win	
	$(PIN)  -- ./$(OBJDIR)suspend_context_app_win
	rm -f suspend_context.failed 

exception_ntcontinue.test :	exception_ntcontinue.tested suspend_context.failed	$(OBJDIR)winapp_exception_ntcontinue
	$(PIN)  -- ./$(OBJDIR)winapp_exception_ntcontinue
	rm -f exception_ntcontinue.failed
	
test_iarg_partial_context_xsave_xrstor_emulation.test: %.test: $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation$(PINTOOL_SUFFIX) $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation_app test_iarg_partial_context_xsave_xrstor_emulation.tested test_iarg_partial_context_xsave_xrstor_emulation.failed
	$(PIN) -t $<  -- $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation_app
	rm test_iarg_partial_context_xsave_xrstor_emulation.failed 
	
test_iarg_partial_context_xsave_xrstor_emulation_const_context.test: %.test: $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation$(PINTOOL_SUFFIX) $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation_app test_iarg_partial_context_xsave_xrstor_emulation_const_context.tested test_iarg_partial_context_xsave_xrstor_emulation_const_context.failed
	$(PIN) -t $< -use_const_context 1 -- $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation_app
	rm test_iarg_partial_context_xsave_xrstor_emulation_const_context.failed 
	
test_iarg_partial_context_xsave_xrstor_emulation_set_in_xrstor.test: %.test: $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation$(PINTOOL_SUFFIX) $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation_set_in_xrstor_app test_iarg_partial_context_xsave_xrstor_emulation_set_in_xrstor.tested test_iarg_partial_context_xsave_xrstor_emulation_set_in_xrstor.failed
	$(PIN) -t $< -set_in_xrstor 1 -- $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation_set_in_xrstor_app
	rm test_iarg_partial_context_xsave_xrstor_emulation_set_in_xrstor.failed 
	
test_iarg_partial_context_xsave_xrstor_emulation_regs_all.test: %.test: $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation$(PINTOOL_SUFFIX) $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation_set_in_xrstor_app test_iarg_partial_context_xsave_xrstor_emulation_regs_all.tested test_iarg_partial_context_xsave_xrstor_emulation_regs_all.failed
	$(PIN) -t $< -regs_all 1 -set_in_xrstor 1 -- $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation_set_in_xrstor_app
	rm test_iarg_partial_context_xsave_xrstor_emulation_regs_all.failed
	
gather_emu.test: $(OBJDIR)gather_emu$(PINTOOL_SUFFIX) gather_emu.tested gather_emu.failed gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu.failed $<.out 

gather_emu_dps.test: $(OBJDIR)gather_emu$(PINTOOL_SUFFIX) gather_emu_dps.tested gather_emu_dps.failed gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dps.out 2>&1
	rm gather_emu_dps.failed $<_dps.out 

gather_emu_dpd.test: $(OBJDIR)gather_emu$(PINTOOL_SUFFIX) gather_emu_dpd.tested gather_emu_dpd.failed gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dpd.out 2>&1
	rm gather_emu_dpd.failed $<_dpd.out 

gather_emu_qps.test: $(OBJDIR)gather_emu$(PINTOOL_SUFFIX) gather_emu_qps.tested gather_emu_qps.failed gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qps.out 2>&1
	rm gather_emu_qps.failed $<_qps.out 

gather_emu_qpd.test: $(OBJDIR)gather_emu$(PINTOOL_SUFFIX) gather_emu_qpd.tested gather_emu_qpd.failed gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qpd.out 2>&1
	rm gather_emu_qpd.failed $<_qpd.out 

gather_emu1.test: $(OBJDIR)gather_emu1$(PINTOOL_SUFFIX) gather_emu1.tested gather_emu1.failed gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu1.failed $<.out 

gather_emu1_dps.test: $(OBJDIR)gather_emu1$(PINTOOL_SUFFIX) gather_emu1_dps.tested gather_emu1_dps.failed gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dps.out 2>&1
	rm gather_emu1_dps.failed $<_dps.out 

gather_emu1_dpd.test: $(OBJDIR)gather_emu1$(PINTOOL_SUFFIX) gather_emu1_dpd.tested gather_emu1_dpd.failed gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dpd.out 2>&1
	rm gather_emu1_dpd.failed $<_dpd.out 

gather_emu1_qps.test: $(OBJDIR)gather_emu1$(PINTOOL_SUFFIX) gather_emu1_qps.tested gather_emu1_qps.failed gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qps.out 2>&1
	rm gather_emu1_qps.failed $<_qps.out 

gather_emu1_qpd.test: $(OBJDIR)gather_emu1$(PINTOOL_SUFFIX) gather_emu1_qpd.tested gather_emu1_qpd.failed gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qpd.out 2>&1
	rm gather_emu1_qpd.failed $<_qpd.out 

gather_emu_buffer.test: $(OBJDIR)gather_emu_buffer$(PINTOOL_SUFFIX) gather_emu_buffer.tested gather_emu_buffer.failed gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu_buffer.failed $<.out 

gather_emu_buffer_dps.test: $(OBJDIR)gather_emu_buffer$(PINTOOL_SUFFIX) gather_emu_buffer_dps.tested gather_emu_buffer_dps.failed gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dps.out 2>&1
	rm gather_emu_buffer_dps.failed $<_dps.out 

gather_emu_buffer_dpd.test: $(OBJDIR)gather_emu_buffer$(PINTOOL_SUFFIX) gather_emu_buffer_dpd.tested gather_emu_buffer_dpd.failed gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dpd.out 2>&1
	rm gather_emu_buffer_dpd.failed $<_dpd.out 

gather_emu_buffer_qps.test: $(OBJDIR)gather_emu_buffer$(PINTOOL_SUFFIX) gather_emu_buffer_qps.tested gather_emu_buffer_qps.failed gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qps.out 2>&1
	rm gather_emu_buffer_qps.failed $<_qps.out 

gather_emu_buffer_qpd.test: $(OBJDIR)gather_emu_buffer$(PINTOOL_SUFFIX) gather_emu_buffer_qpd.tested gather_emu_buffer_qpd.failed gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qpd.out 2>&1
	rm gather_emu_buffer_qpd.failed $<_qpd.out 

gather_emu_context.test: $(OBJDIR)gather_emu_context$(PINTOOL_SUFFIX) gather_emu_context.tested gather_emu_context.failed gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu_context.failed $<.out 

gather_emu_context_dps.test: $(OBJDIR)gather_emu_context$(PINTOOL_SUFFIX) gather_emu_context_dps.tested gather_emu_context_dps.failed gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dps.out 2>&1
	rm gather_emu_context_dps.failed $<_dps.out 

gather_emu_context_dpd.test: $(OBJDIR)gather_emu_context$(PINTOOL_SUFFIX) gather_emu_context_dpd.tested gather_emu_context_dpd.failed gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dpd.out 2>&1
	rm gather_emu_context_dpd.failed $<_dpd.out 

gather_emu_context_qps.test: $(OBJDIR)gather_emu_context$(PINTOOL_SUFFIX) gather_emu_context_qps.tested gather_emu_context_qps.failed gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qps.out 2>&1
	rm gather_emu_context_qps.failed $<_qps.out 

gather_emu_context_qpd.test: $(OBJDIR)gather_emu_context$(PINTOOL_SUFFIX) gather_emu_context_qpd.tested gather_emu_context_qpd.failed gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qpd.out 2>&1
	rm gather_emu_context_qpd.failed $<_qpd.out 

gather_emu1_context.test: $(OBJDIR)gather_emu1_context$(PINTOOL_SUFFIX) gather_emu1_context.tested gather_emu1_context.failed gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu1_context.failed $<.out 

gather_emu1_context_dps.test: $(OBJDIR)gather_emu1_context$(PINTOOL_SUFFIX) gather_emu1_context_dps.tested gather_emu1_context_dps.failed gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dps.out 2>&1
	rm gather_emu1_context_dps.failed $<_dps.out 

gather_emu1_context_dpd.test: $(OBJDIR)gather_emu1_context$(PINTOOL_SUFFIX) gather_emu1_context_dpd.tested gather_emu1_context_dpd.failed gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dpd.out 2>&1
	rm gather_emu1_context_dpd.failed $<_dpd.out 

gather_emu1_context_qps.test: $(OBJDIR)gather_emu1_context$(PINTOOL_SUFFIX) gather_emu1_context_qps.tested gather_emu1_context_qps.failed gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qps.out 2>&1
	rm gather_emu1_context_qps.failed $<_qps.out 

gather_emu1_context_qpd.test: $(OBJDIR)gather_emu1_context$(PINTOOL_SUFFIX) gather_emu1_context_qpd.tested gather_emu1_context_qpd.failed gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qpd.out 2>&1
	rm gather_emu1_context_qpd.failed $<_qpd.out 

gather_emu_buffer_context.test: $(OBJDIR)gather_emu_buffer_context$(PINTOOL_SUFFIX) gather_emu_buffer_context.tested gather_emu_buffer_context.failed gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu_buffer_context.failed $<.out 

gather_emu_buffer_context_dps.test: $(OBJDIR)gather_emu_buffer_context$(PINTOOL_SUFFIX) gather_emu_buffer_context_dps.tested gather_emu_buffer_context_dps.failed gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dps.out 2>&1
	rm gather_emu_buffer_context_dps.failed $<_dps.out 

gather_emu_buffer_context_dpd.test: $(OBJDIR)gather_emu_buffer_context$(PINTOOL_SUFFIX) gather_emu_buffer_context_dpd.tested gather_emu_buffer_context_dpd.failed gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherdpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_dpd.out 2>&1
	rm gather_emu_buffer_context_dpd.failed $<_dpd.out 

gather_emu_buffer_context_qps.test: $(OBJDIR)gather_emu_buffer_context$(PINTOOL_SUFFIX) gather_emu_buffer_context_qps.tested gather_emu_buffer_context_qps.failed gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqps_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qps.out 2>&1
	rm gather_emu_buffer_context_qps.failed $<_qps.out 

gather_emu_buffer_context_qpd.test: $(OBJDIR)gather_emu_buffer_context$(PINTOOL_SUFFIX) gather_emu_buffer_context_qpd.tested gather_emu_buffer_context_qpd.failed gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gatherqpd_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<_qpd.out 2>&1
	rm gather_emu_buffer_context_qpd.failed  $<_qpd.out 

gather_emu_memory_callback.test: $(OBJDIR)gather_emu_memory_callback$(PINTOOL_SUFFIX) gather_emu_memory_callback.tested gather_emu_memory_callback.failed gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	$(PIN) -t $< -- ./gather_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	rm gather_emu_memory_callback.failed $<.out 



gather_emu_exception_and_rewrite1.test: $(OBJDIR)gather_emu_rewrite1$(PINTOOL_SUFFIX) gather_emu_exception_and_rewrite1.tested gather_emu_exception_and_rewrite1.failed gather_with_exception_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe
	-$(PIN) -t $< -- ./gather_with_exception_$(TARGET_ARCH_NAME)_$(TARGET_OS).exe >  $<.out 2>&1
	echo "gather_emu_exception_and_rewrite1.test pin execution should fail.  Ignore the error (above, but not any below!)."
	grep "Exception is Illegal Instruction - test is running on an OS/processor without AVX2 support" $<.out
	rm gather_emu_exception_and_rewrite1.failed $<.out 

##  build rules


$(OBJDIR)attach_in_sighandler_app: attach_in_sighandler_app.cpp $(OBJDIR)fp_save_restore.o
	$(CXX) -O0 --no-inline ${APP_CXXFLAGS} $(DBG) ${NO_COMDAT_FLAG} -c -o $(OBJDIR)attach_in_sighandler_app.o $<
	$(CXX) ${APP_CXXFLAGS} $(DBG) $(OBJDIR)fp_save_restore.o $(OBJDIR)attach_in_sighandler_app.o $(APP_CXXLINK_FLAGS) -lpthread -o $@
        
$(OBJDIR)detach_in_sighandler_app: detach_in_sighandler_app.cpp $(OBJDIR)fp_save_restore.o
	$(CXX) -O0 --no-inline ${APP_CXXFLAGS} $(DBG) ${NO_COMDAT_FLAG} -c -o $(OBJDIR)detach_in_sighandler_app.o $<
	$(CXX) ${APP_CXXFLAGS} $(DBG) $(OBJDIR)fp_save_restore.o $(OBJDIR)detach_in_sighandler_app.o $(APP_CXXLINK_FLAGS) -lpthread -o $@

$(OBJDIR)fp_save_restore.o: fp_save_restore_$(TARGET_ARCH_NAME).s
	$(CXX) ${APP_CXXFLAGS} -c fp_save_restore_$(TARGET_ARCH_NAME).s -o $(OBJDIR)fp_save_restore.o

	
	
$(OBJDIR)%.o : %.cpp
	$(CXX) ${COPT} $(CXXFLAGS) $(PIN_CXXFLAGS) ${OUTOPT}$@ $<
    
$(TOOLS): $(OBJDIR)%$(PINTOOL_SUFFIX) :	$(OBJDIR)%.o $(PIN_LIBNAMES)
	${PIN_LD} $(PIN_LDFLAGS) $(LINK_DEBUG) ${LINK_OUT}$@ $<	${PIN_LPATHS} $(PIN_LIBS) $(DBG)

$(OBJDIR)set_ymm_scratches$(PINTOOL_SUFFIX): $(OBJDIR)set_ymm_scratches.o $(OBJDIR)$(SET_YMM_SCRATCHES_ASM_OBJ)
	${PIN_LD} $(PIN_LDFLAGS) $(LINK_DEBUG) ${LINK_OUT}$@ $<	 $(OBJDIR)$(SET_YMM_SCRATCHES_ASM_OBJ) ${PIN_LPATHS} $(PIN_LIBS) $(DBG)

$(OBJDIR)set_ymm_scratches_allscratch$(PINTOOL_SUFFIX): $(OBJDIR)set_ymm_scratches_allscratch.o $(OBJDIR)$(SET_YMM_SCRATCHES_ASM_OBJ)
	${PIN_LD} $(PIN_LDFLAGS) $(LINK_DEBUG) ${LINK_OUT}$@ $<	 $(OBJDIR)$(SET_YMM_SCRATCHES_ASM_OBJ) ${PIN_LPATHS} $(PIN_LIBS) $(DBG)

$(OBJDIR)set_fp_context_ymm_regs_xmms_spilled$(PINTOOL_SUFFIX): $(OBJDIR)set_fp_context_ymm_regs_xmms_spilled.o   $(OBJDIR)$(SSE-REF_ASM_OBJ) $(OBJDIR)$(SET_XMM_SCRATCHES_ASM_OBJ)
	${PIN_LD} $(PIN_LDFLAGS) $(LINK_DEBUG) ${LINK_OUT}$@ $< ${PIN_LPATHS} $(PIN_LIBS) $(DBG)   $(OBJDIR)$(SSE-REF_ASM_OBJ) $(OBJDIR)$(SET_XMM_SCRATCHES_ASM_OBJ)

$(OBJDIR)exception_context_monitor$(PINTOOL_SUFFIX): $(OBJDIR)exception_context_monitor.o  $(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ)
	${PIN_LD} $(PIN_LDFLAGS) $(LINK_DEBUG) ${LINK_OUT}$@ $<	$(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ) ${PIN_LPATHS} $(PIN_LIBS) $(DBG)

$(OBJDIR)raise_exception$(PINTOOL_SUFFIX): $(OBJDIR)raise_exception.o  $(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ)
	${PIN_LD} $(PIN_LDFLAGS) $(LINK_DEBUG) ${LINK_OUT}$@  $< $(OBJDIR)$(VERIFY_FP_CONTEXT_ASM_OBJ) ${PIN_LPATHS} $(PIN_LIBS) $(DBG)

$(OBJDIR)set_xmm_scratches$(PINTOOL_SUFFIX): $(OBJDIR)set_xmm_scratches.o $(OBJDIR)$(SET_XMM_SCRATCHES_ASM_OBJ)
	${PIN_LD} $(PIN_LDFLAGS) $(LINK_DEBUG) ${LINK_OUT}$@ $<	 $(OBJDIR)$(SET_XMM_SCRATCHES_ASM_OBJ) ${PIN_LPATHS} $(PIN_LIBS) $(DBG)

$(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation$(PINTOOL_SUFFIX): $(OBJDIR)test_iarg_partial_context_xsave_xrstor_emulation.o $(OBJDIR)$(TEST_IARG_PRESERVE_ASM_OBJ) $(OBJDIR)$(SUPPORTS_AVX_ASM_OBJ)
	${PIN_LD} $(PIN_LDFLAGS) $(LINK_DEBUG) ${LINK_OUT}$@ $< ${PIN_LPATHS} $(PIN_LIBS) $(DBG) $(OBJDIR)$(TEST_IARG_PRESERVE_ASM_OBJ) $(OBJDIR)$(SUPPORTS_AVX_ASM_OBJ) 

## cleaning
clean:
	-rm	-rf	$(OBJDIR) *.tested *.failed	*.obj *.o *.out *.log* *.txt *.pdb


