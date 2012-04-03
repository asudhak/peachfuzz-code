##
## PIN tools
##

##############################################################
#
# Here are some things you might want to configure
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
    include ../makefile.gnu.config
    CXXFLAGS ?= -I$(PIN_HOME)/InstLib -Wall -Werror -Wno-unknown-pragmas $(DBG) $(OPT) -MMD
endif

ifeq ($(TARGET_COMPILER),ms)
    include ../makefile.ms.config
    DBG?=
endif

##############################################################
#
# Tools sets
#
##############################################################

TOOL_ROOTS =

ifeq ($(TARGET_OS),m)
  # here on mac snow-leopard
  ifeq ($(findstring 10.8,$(OSREL)),10.8)
    TOOL_ROOTS = thcnt
    APPS = wqtimer
  endif
endif

APPS_BINARY_FILES = $(APPS:%=$(OBJDIR)%)

##############################################################
#
# build rules
#
##############################################################

all: tools $(APPS_BINARY_FILES)

TOOLS = $(TOOL_ROOTS:%=$(OBJDIR)%$(PINTOOL_SUFFIX))

tools: $(OBJDIR) $(TOOLS)

test: $(OBJDIR) $(TOOL_ROOTS:%=%.test)


## build rules

$(OBJDIR):
	mkdir -p $(OBJDIR)


$(OBJDIR)%.o : %.cpp
	$(CXX) -c $(CXXFLAGS) $(PIN_CXXFLAGS) ${OUTOPT}$@ $<

$(TOOLS): $(PIN_LIBNAMES)
$(TOOLS): $(OBJDIR)%$(PINTOOL_SUFFIX) : $(OBJDIR)%.o
	${PIN_LD} ${PIN_LDFLAGS} $(LINK_DEBUG) ${LINK_OUT}$@ $< ${PIN_LPATHS} ${PIN_LIBS} $(DBG)

# wqtimer is using __block, always use /usr/bin/gcc for it
$(OBJDIR)wqtimer: wqtimer.c
	/usr/bin/gcc $< -o $@ $(APP_CXXFLAGS)

thcnt.test: $(OBJDIR)thcnt$(PINTOOL_SUFFIX) thcnt.tested thcnt.failed $(OBJDIR)wqtimer
	$(PIN) -t $(OBJDIR)thcnt$(PINTOOL_SUFFIX) -- ./$(OBJDIR)wqtimer
	rm thcnt.out $(@:.test=.failed)

## cleaning
clean:
	-rm -rf $(OBJDIR) *.out *.tested *.failed *.d pin.log

