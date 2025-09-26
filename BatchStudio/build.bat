set QTDIR=D:\Qt\6.9.2\msvc2022_64
set PATH="%QTDIR%\bin";%PATH%
qmake ./BatchStudio.pro CONFIG+=release ../BatchStudio
nmake
windeployqt release\BatchStudio.exe