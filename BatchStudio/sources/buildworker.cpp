#include <QDebug>
#include "buildworker.h"
#include "processutils.h"
#include "HostCLR.h"

BuildWorker::BuildWorker(const QString& selInTree, const QString& selInList, QObject *parent)
    : QObject(parent), m_SelInTree(selInTree), m_SelInList(selInList)
{
}

void BuildWorker::build()
{
    emit started();
#ifdef QT_DEBUG
    qDebug() << "Building" << m_SelInTree << m_SelInList;
#endif
    ProcessResult result{};
    if (build_fptr) {
        std::string s0 = m_SelInTree.toStdString();
        std::string s1 = m_SelInList.toStdString();
        int r = build_fptr(s0.c_str(), s1.c_str(), this, &result);
#ifdef QT_DEBUG
        qDebug() << "CSharp build returned code" << r;
#endif
        if (r > 0) {
            emit buildFinished(m_SelInTree, m_SelInList);
            emit finished();
            return;
        }
        else if (r < 0) {
            emit buildFailed(m_SelInTree, m_SelInList);
            return;
        }
    }
    emit buildFailed(m_SelInTree, m_SelInList);
}
