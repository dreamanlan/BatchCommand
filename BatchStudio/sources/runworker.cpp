#include <QDebug>
#include "runworker.h"
#include "processutils.h"
#include "HostCLR.h"

RunWorker::RunWorker(const QString& selInTree, const QString& selInList, QObject *parent)
    : QObject(parent), m_SelInTree(selInTree), m_SelInList(selInList)
{
}

void RunWorker::execute()
{
    emit started();
#ifdef QT_DEBUG
    qDebug() << "Running program" << m_SelInTree << m_SelInList;
#endif
    ProcessResult result{};
    if (run_prog_fptr) {
        std::string s0 = m_SelInTree.toStdString();
        std::string s1 = m_SelInList.toStdString();
        int r = run_prog_fptr(s0.c_str(), s1.c_str(), this, &result);
#ifdef QT_DEBUG
        qDebug() << "CSharp running program returned code" << r;
#endif
        if (r > 0) {
            emit runFinished(m_SelInTree, m_SelInList);
            emit finished();
            return;
        }
        else if (r < 0) {
            emit runFailed(m_SelInTree, m_SelInList);
            return;
        }
    }
    emit runFailed(m_SelInTree, m_SelInList);
}
