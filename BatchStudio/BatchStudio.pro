include(QHexView/QHexView.pri)

QT += core gui widgets

TARGET = BatchStudio
TEMPLATE = app

CONFIG += c++11

HEADERS += \
    DarkStyle/DarkStyle.h \
    sources/buildworker.h \
    sources/installworker.h \
    sources/commandworker.h \
    sources/appearancesettingswidget.h \
    sources/binarysettingswidget.h \
    sources/findreplacedialog.h \
    sources/flickcharm.h \
    sources/hexedit.h \
    sources/imageviewerwidget.h \
    sources/mainwindow.h \
    sources/processutils.h \
    sources/settingsdialog.h \
    sources/sourcecodeedit.h \
    sources/splashwindow.h \
    sources/themedsyntaxhighlighter.h \
    sources/versionresolveworker.h \
    sources/HostCLR.h \
    sources/FlowLayout.h \
    sources/HistoryLineEdit.h

SOURCES += \
    DarkStyle/DarkStyle.cpp \
    sources/flickcharm.cpp \
    sources/hexedit.cpp \
    sources/imageviewerwidget.cpp \
    sources/main.cpp \
    sources/buildworker.cpp \
    sources/installworker.cpp \
    sources/commandworker.cpp \
    sources/appearancesettingswidget.cpp \
    sources/binarysettingswidget.cpp \
    sources/findreplacedialog.cpp \
    sources/mainwindow.cpp \
    sources/processutils.cpp \
    sources/settingsdialog.cpp \
    sources/sourcecodeedit.cpp \
    sources/splashwindow.cpp \
    sources/themedsyntaxhighlighter.cpp \
    sources/versionresolveworker.cpp \
    sources/HostCLR.cpp \
    sources/FlowLayout.cpp \
    sources/HistoryLineEdit.cpp

RESOURCES += \
    DarkStyle/darkstyle.qrc \
    resources/all.qrc

OTHER_FILES += \
    resources/batchstudio.desktop

mac:ICON = resources/icon.icns

unix {
    isEmpty(PREFIX) {
        PREFIX = /usr/local
    }

    target.path = $$PREFIX/bin

    icons.files += resources/icon.png
    icons.path = $$PREFIX/share/pixmaps/

    shortcut.files = resources/batchstudio.desktop
    shortcut.path = $$PREFIX/share/applications/

    INSTALLS += target icons shortcut
    TARGET = batchstudio
}

win32:RC_ICONS += resources/icon.ico

QMAKE_TARGET_COMPANY = NationPhone
QMAKE_TARGET_COPYRIGHT = Free
QMAKE_TARGET_DESCRIPTION = Open-source, cross-platform Qt based IDE for batch command.
QMAKE_TARGET_PRODUCT = Batch Studio

win32 {
    NULL_DEVICE = NUL
} else {
    NULL_DEVICE = /dev/null
}

GIT_CMD_BASE = git --git-dir $$PWD/.git --work-tree $$PWD
GIT_BRANCH = $$system($$GIT_CMD_BASE rev-parse --abbrev-ref HEAD 2> $$NULL_DEVICE)
GIT_COMMIT_FULL = $$system($$GIT_CMD_BASE rev-parse HEAD 2> $$NULL_DEVICE)
GIT_COMMIT_NUMBER = $$system($$GIT_CMD_BASE rev-list HEAD --count 2> $$NULL_DEVICE)
GIT_COMMIT_SHORT = $$system($$GIT_CMD_BASE rev-parse --short HEAD 2> $$NULL_DEVICE)
GIT_TAG = $$system($$GIT_CMD_BASE tag -l --points-at HEAD 2> $$NULL_DEVICE)

DEFINES += GIT_BRANCH=\\\"$$GIT_BRANCH\\\" \
    GIT_COMMIT_FULL=\\\"$$GIT_COMMIT_FULL\\\" \
    GIT_COMMIT_NUMBER=\\\"$$GIT_COMMIT_NUMBER\\\" \
    GIT_COMMIT_SHORT=\\\"$$GIT_COMMIT_SHORT\\\" \
    GIT_TAG=\\\"$$GIT_TAG\\\"

!isEmpty(GIT_TAG) {
    win32 {
        VERSION = $${GIT_TAG}.$${GIT_COMMIT_NUMBER}
    } else {
        VERSION = $$GIT_TAG
    }
    macx {
        INFO_PLIST_PATH = $$shell_quote($${OUT_PWD}/$${TARGET}.app/Contents/Info.plist)
        QMAKE_POST_LINK += /usr/libexec/PlistBuddy -c \"Set :CFBundleShortVersionString $${VERSION}\" $${INFO_PLIST_PATH}
    }
}

win32-msvc {
    QMAKE_LFLAGS += /STACK:20000000
    LIBS += "./sources/coreclr/nethost.lib" "User32.lib"
}