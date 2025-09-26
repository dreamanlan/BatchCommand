#ifndef BUILDWORKER_H
#define BUILDWORKER_H

#include <QObject>

class BuildWorker : public QObject
{
    Q_OBJECT
public:
    explicit BuildWorker(const QString& selInTree, const QString& selInList, QObject *parent = nullptr);
    void build();
private:
    QString m_SelInTree;
    QString m_SelInList;
signals:
    void finished();
    void buildFailed(const QString& selInTree, const QString& selInList);
    void buildFinished(const QString& selInTree, const QString& selInList);
    void started();
};

#endif // BUILDWORKER_H
