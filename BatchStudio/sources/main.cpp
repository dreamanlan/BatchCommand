#include <QApplication>
#include <QSettings>
#include "splashwindow.h"
#include "../DarkStyle/DarkStyle.h"
#include <cstdio>
#if defined(Q_OS_WIN)
#include <windows.h>
#endif

#if defined(Q_OS_WIN)
void qtMessageToStdout(QtMsgType type, const QMessageLogContext& context, const QString& msg)
{
    QByteArray localMsg = msg.toLocal8Bit();
    fprintf(stdout, "%s\n", localMsg.constData());
    fflush(stdout);
}

static bool attachOrCreateConsole()
{
    if (AttachConsole(ATTACH_PARENT_PROCESS)) {
    }
    else {
        if (!AllocConsole()) return false;
    }

    FILE* fp;
    freopen_s(&fp, "CONOUT$", "w", stdout);
    freopen_s(&fp, "CONOUT$", "w", stderr);
    freopen_s(&fp, "CONIN$", "r", stdin);

    std::ios::sync_with_stdio();

    return true;
}
#endif

#define CODE_RESTART 60600

int main(int argc, char *argv[])
{
#if defined(Q_OS_WIN)
    attachOrCreateConsole();
    qInstallMessageHandler(qtMessageToStdout);
    ShowWindow(GetConsoleWindow(), SW_HIDE);
#endif

    QApplication::setApplicationName("Batch Studio");
    QApplication::setOrganizationDomain("-");
    QApplication::setOrganizationName("dreaman");
    int code = 0;
    do {
        QApplication app(argc, argv);
        app.setWindowIcon(QIcon(":/images/icon.png"));
        QSettings settings;
        const bool dark = settings.value("dark_theme", false).toBool();
        if (dark) {
            app.setStyle(new DarkStyle);
        }
        SplashWindow window;
        window.show();
        code = app.exec();
    } while (code == CODE_RESTART);

#if defined(Q_OS_WIN)
    FreeConsole();
#endif
    return code;
}
