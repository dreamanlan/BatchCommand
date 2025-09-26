set QTDIR=D:\Qt\6.9.2\msvc2022_64
set PATH="%QTDIR%\bin";%PATH%
qmake -tp vc ./BatchStudio.pro
rem qmake ./BatchStudio.pro CONFIG+=debug ../BatchStudio
rem nmake
windeployqt debug\BatchStudio.exe