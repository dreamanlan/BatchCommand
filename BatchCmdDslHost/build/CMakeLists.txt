# CMakeLists for BatchCmdDslHost, source dir is the project root (../)
cmake_minimum_required(VERSION 3.11)

if(APPLE)
  set(CMAKE_OSX_ARCHITECTURES "arm64" CACHE STRING "Build architectures for macOS")
endif()

project(BatchCmdDslHost CXX)

option(BUILD_AS_OSX_DYLIB "build as macOS dylib" OFF)
option(BUILD_AS_OSX_BUNDLE "build as macOS bundle" OFF)

set(HOST_ROOT ${CMAKE_CURRENT_SOURCE_DIR}/..)

# coreclr headers are included as "coreclr/xxx.h", so the project root must be on the include path
include_directories(${HOST_ROOT})

set(HOST_SOURCES
  ${HOST_ROOT}/BatchCmdDslHost.cpp
  ${HOST_ROOT}/path_utils.cpp
)

# NOTE: do NOT define __LINUX__ here, sources use _MSC_VER / __APPLE__ / __linux__ only
if(NOT WIN32)
  add_compile_options(-std=c++17 -fwritable-strings)
  add_compile_definitions(_CRT_SECURE_NO_WARNINGS)
endif()

if(WIN32)
  add_executable(BatchCmdDslHost ${HOST_SOURCES})
elseif(APPLE AND BUILD_AS_OSX_DYLIB)
  add_library(BatchCmdDslHost SHARED ${HOST_SOURCES})
elseif(APPLE AND BUILD_AS_OSX_BUNDLE)
  add_executable(BatchCmdDslHost MACOSX_BUNDLE ${HOST_SOURCES})
else()
  add_executable(BatchCmdDslHost ${HOST_SOURCES})
endif()

if(WIN32)
  target_link_libraries(BatchCmdDslHost ${HOST_ROOT}/coreclr/libnethost.lib)
else()
  target_link_libraries(BatchCmdDslHost ${HOST_ROOT}/coreclr/libnethost.a dl pthread)
endif()
