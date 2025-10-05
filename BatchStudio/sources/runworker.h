#ifndef RUNWORKER_H
#define RUNWORKER_H

#include <QObject>

class RunWorker : public QObject
{
    Q_OBJECT
public:
    explicit RunWorker(const QString& selInTree, const QString& selInList, QObject *parent = nullptr);
    void execute();
private:
    QString m_SelInTree;
    QString m_SelInList;
signals:
    void finished();
    void runFailed(const QString& selInTree, const QString& selInList);
    void runFinished(const QString& selInTree, const QString& selInList);
    void started();
};

#endif // RUNWORKER_H
