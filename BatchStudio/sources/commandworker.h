#ifndef COMMANDWORKER_H
#define COMMANDWORKER_H

#include <QObject>

class CommandWorker : public QObject
{
    Q_OBJECT
public:
    explicit CommandWorker(const QString& cmdType, const QString& cmdArgs, const QString& selInTree, const QString& selInList, QObject *parent = nullptr);
    void execute();
private:
    QString m_CmdType;
    QString m_CmdArgs;
    QString m_SelInTree;
    QString m_SelInList;
signals:
    void finished();
    void executeFailed(const QString& cmdType, const QString& cmdArgs, const QString& selInTree, const QString& selInList);
    void executeFinished(const QString& cmdType, const QString& cmdArgs, const QString& selInTree, const QString& selInList);
    void started();
};

#endif // COMMANDWORKER_H
