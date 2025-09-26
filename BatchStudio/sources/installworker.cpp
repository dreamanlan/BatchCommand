#include <QDebug>
#include "installworker.h"
#include "processutils.h"
#include "HostCLR.h"

InstallWorker::InstallWorker(const QString& selInTree, const QString& selInList, QObject *parent)
    : QObject(parent), m_SelInTree(selInTree), m_SelInList(selInList)
{
}

void InstallWorker::install()
{
    emit started();
#ifdef QT_DEBUG
    qDebug() << "Installing" << m_SelInTree << m_SelInList;
#endif
    ProcessResult result{};
    if (install_fptr) {
        std::string s0 = m_SelInTree.toStdString();
        std::string s1 = m_SelInList.toStdString();
        int r = install_fptr(s0.c_str(), s1.c_str(), this, &result);
#ifdef QT_DEBUG
        qDebug() << "CSharp install returned code" << r;
#endif
        if (r > 0) {
            emit installFinished(m_SelInTree, m_SelInList);
            emit finished();
            return;
        }
        else if (r < 0) {
            emit installFailed(m_SelInTree, m_SelInList);
            return;
        }
    }
    emit installFailed(m_SelInTree, m_SelInList);
}
