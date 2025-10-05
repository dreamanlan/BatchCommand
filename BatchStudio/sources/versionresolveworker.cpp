#include <QDebug>
#include <QRegularExpression>
#include <QSettings>
#include "versionresolveworker.h"
#include "processutils.h"

#define REGEXP_ADB_VERSION "version (\\d+\\.\\d+\\.\\d+)$"
#define REGEXP_JAVA_VERSION "version \"([\\d._]+)\""
#define REGEXP_UAS_VERSION "Version: (\\d+\\.\\d+\\.\\d+)$"

VersionResolveWorker::VersionResolveWorker(QObject *parent)
    : QObject(parent)
{
}

void VersionResolveWorker::resolve()
{
    emit started();
    QSettings settings;
    bool useJavaAndAdb = settings.value("use_java_and_adb", false).toBool();
    if (!useJavaAndAdb) {
        emit finished();
        return;
    }

#ifdef QT_DEBUG
    qDebug() << "Using 'java' from" << ProcessUtils::javaExe();
#endif
    bool found = false;
    const QString java = ProcessUtils::javaExe();
    if (!java.isEmpty()) {
        ProcessResult result = ProcessUtils::runCommand(java, QStringList() << "-version");
#ifdef QT_DEBUG
        qDebug() << "Java returned code" << result.code;
#endif
        if (result.code == 0) {
#ifdef QT_DEBUG
            qDebug() << "Java returned" << result.output[0];
#endif
            QRegularExpression regexp(REGEXP_JAVA_VERSION);
            QRegularExpressionMatch match = regexp.match(result.output[0]);
            if (match.hasMatch()) {
                emit versionResolved("java", match.captured(1));
                found = true;
            }
        }
    }
    if (!found) {
        emit versionResolved("java", QString());
    }
#ifdef QT_DEBUG
    qDebug() << "Using 'jadx' from" << ProcessUtils::jadxExe();
#endif
    found = false;
    const QString jadx = ProcessUtils::jadxExe();
    if (!jadx.isEmpty()) {
        ProcessResult result = ProcessUtils::runCommand(jadx, QStringList() << "--version");
#ifdef QT_DEBUG
        qDebug() << "Jadx returned code" << result.code;
#endif
        if ((result.code == 0) && !result.output.isEmpty()) {
#ifdef QT_DEBUG
            qDebug() << "Jadx returned" << result.output.first();
#endif
            emit versionResolved("jadx", result.output.first().trimmed());
            found = true;
        }
    }
    if (!found) {
        emit versionResolved("jadx", QString());
    }
#ifdef QT_DEBUG
    qDebug() << "Using 'adb' from" << ProcessUtils::adbExe();
#endif
    found = false;
    const QString adb = ProcessUtils::adbExe();
    if (!adb.isEmpty()) {
        ProcessResult result = ProcessUtils::runCommand(adb, QStringList() << "--version");
#ifdef QT_DEBUG
        qDebug() << "ADB returned code" << result.code;
#endif
        if ((result.code == 0) && !result.output.isEmpty()) {
#ifdef QT_DEBUG
            qDebug() << "ADB returned" << result.output.first();
#endif
            QRegularExpression regexp(REGEXP_ADB_VERSION);
            QRegularExpressionMatch match = regexp.match(result.output.first());
            if (match.hasMatch()) {
                emit versionResolved("adb", match.captured(1));
                found = true;
            }
        }
    }
    if (!found) {
        emit versionResolved("adb", QString());
    }
    emit finished();
}
