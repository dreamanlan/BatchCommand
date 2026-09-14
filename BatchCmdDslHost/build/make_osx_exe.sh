#!/bin/sh
set -e
mkdir -p build_osx
cd build_osx
cmake -G Xcode -D BUILD_AS_OSX_DYLIB=0 -D BUILD_AS_OSX_BUNDLE=0 -D CMAKE_OSX_ARCHITECTURES='arm64' ../
cd ..
cmake --build build_osx --config Debug
cmake --build build_osx --config Release
mkdir -p ../../arm64/Debug
mkdir -p ../../arm64/Release
cp -rf build_osx/Debug/BatchCmdDslHost ../../arm64/Debug
cp -rf build_osx/Release/BatchCmdDslHost ../../arm64/Release
lipo -info ../../arm64/Debug/BatchCmdDslHost || true
