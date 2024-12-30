#
# This simple makefile just kickstarts MSBuild system
#  for actual build details refer to *.csproj files
#
# Build SDK: 8.0
# Target SDK: 6.0
#
# - Building using SDK 8.0 so that generated PE32 executables contain resources section
# - UI target currently supports only Windows
# - CLI target supports Linux (project for debugging)
#
# Usage examples:
#  'make' or 'make all'                : builds all targets (currently ui and cli)
#  'make ARCH=x86 CONFIG=Release'      : builds all targets into 32-bit windows executable with optimizations
#  'make AGSUnpacker.CLI TARGET=linux' : build CLI and its dependencies for debugging targeting linux
#  'make publish CONFIG=Release'       : use this to publish releases
#
DOTNET ?= dotnet
RM     ?= rm

TARGET  ?= win
ARCH    ?= x64
CONFIG  ?= Debug
VERSION ?= 0.8
AUTHORS ?= Unknown
PUBLIC_RELEASE ?=

BUILD_DIR       ?= build/
BUILD_ARTIFACTS ?= $(BUILD_DIR)artifacts/
BUILD_OUTPUT    ?= $(BUILD_DIR)
BUILD_PUBLISH   ?= $(BUILD_DIR)package/

BUILD_OPTIONS  = --os $(TARGET) -a $(ARCH) -c $(CONFIG)
# 'artifacts-path' not available on .net 6
# BUILD_OPTIONS += --artifacts-path $(BUILD_ARTIFACTS)
BUILD_OPTIONS += --no-self-contained
BUILD_OPTIONS += --nologo
BUILD_OPTIONS += /p:Version="$(VERSION)"
BUILD_OPTIONS += /p:Authors="$(AUTHORS)"
ifneq ($(strip $(PUBLIC_RELEASE)),)
BUILD_OPTIONS += /p:PublicReleaseVersion="$(PUBLIC_RELEASE)"
endif

PROJECT_UI ?= AGSUnpacker.UI/AGSUnpacker.UI.csproj

BUILD_PROJECTS  =
BUILD_PROJECTS += AGSUnpacker.CLI
BUILD_PROJECTS += AGSUnpacker.UI

ARTIFACTS ::= $(wildcard */bin */obj)

.PHONY: all publish $(BUILD_PROJECTS) clean

all: $(BUILD_PROJECTS)

publish:
	$(DOTNET) publish $(PROJECT_UI) $(BUILD_OPTIONS) -o $(BUILD_PUBLISH)

$(BUILD_PROJECTS):
	$(DOTNET) build $@/$@.csproj $(BUILD_OPTIONS) -o $(BUILD_OUTPUT)

clean:
	-$(RM) -r $(BUILD_DIR)
	-$(RM) -r $(ARTIFACTS)
