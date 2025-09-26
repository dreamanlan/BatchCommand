#include <QDebug>
#include "commandworker.h"
#include "processutils.h"
#include "HostCLR.h"

CommandWorker::CommandWorker(const QString& cmdType, const QString& cmdArgs, const QString& selInTree, const QString& selInList, QObject *parent)
    : QObject(parent), m_CmdType(cmdType), m_CmdArgs(cmdArgs), m_SelInTree(selInTree), m_SelInList(selInList)
{
}

void CommandWorker::execute()
{
    emit started();
#ifdef QT_DEBUG
    qDebug() << "Command Executing" << m_CmdType << m_CmdArgs << m_SelInTree << m_SelInList;
#endif
    ProcessResult result{};
    if (execute_command_fptr) {
        std::string s0 = m_CmdType.toStdString();
        std::string s1 = m_CmdArgs.toStdString();
        std::string s2 = m_SelInTree.toStdString();
        std::string s3 = m_SelInList.toStdString();
        int r = execute_command_fptr(s0.c_str(), s1.c_str(), s2.c_str(), s3.c_str(), this, &result);
#ifdef QT_DEBUG
        qDebug() << "CSharp execute command returned code" << r;
#endif
        if (r > 0) {
            emit executeFinished(m_CmdType, m_CmdArgs, m_SelInTree, m_SelInList);
            emit finished();
            return;
        }
        else if (r < 0) {
            emit executeFailed(m_CmdType, m_CmdArgs, m_SelInTree, m_SelInList);
            return;
        }
    }
    emit executeFailed(m_CmdType, m_CmdArgs, m_SelInTree, m_SelInList);
}