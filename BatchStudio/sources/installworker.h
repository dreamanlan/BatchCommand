#ifndef INSTALLWORKER_H
#define INSTALLWORKER_H

#include <QObject>

class InstallWorker : public QObject
{
    Q_OBJECT
public:
    explicit InstallWorker(const QString& selInTree, const QString& selInList, QObject *parent = nullptr);
    void install();
private:
    QString m_SelInTree;
    QString m_SelInList;
signals:
    void finished();
    void installFailed(const QString& selInTree, const QString& selInList);
    void installFinished(const QString& selInTree, const QString& selInList);
    void started();
};

#endif // INSTALLWORKER_H
