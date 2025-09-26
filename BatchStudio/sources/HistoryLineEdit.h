// HistoryLineEdit.h
#ifndef HISTORYLINEEDIT_H
#define HISTORYLINEEDIT_H

#include <QLineEdit>
#include <QStringList>

class HistoryLineEdit : public QLineEdit
{
    Q_OBJECT
public:
    explicit HistoryLineEdit(QWidget *parent = nullptr);

    // 设置/获取历史最大条数（默认 50）
    void setMaxHistory(int max);
    int maxHistory() const;

    // 手动管理历史
    void addHistory(const QString& text); // 将一条记录加入历史（会去重并放到末尾）
    QStringList history() const;
    void clearHistory();

signals:
    // 当按回车提交时发出（返回当前文本）
    void submitted(const QString& text);

protected:
    void keyPressEvent(QKeyEvent *event) override;

private:
    QStringList m_history;
    int m_maxHistory;
    int m_index;            // -1 表示未在历史浏览状态；>=0 表示当前浏览到历史索引
    QString m_editBuffer;   // 浏览历史前临时保存当前输入，回到末尾时恢复
};

#endif // HISTORYLINEEDIT_H