// HistoryLineEdit.cpp
#include "HistoryLineEdit.h"
#include <QKeyEvent>

HistoryLineEdit::HistoryLineEdit(QWidget *parent)
    : QLineEdit(parent),
      m_maxHistory(50),
      m_index(-1)
{
}

void HistoryLineEdit::setMaxHistory(int max)
{
    if (max < 1) max = 1;
    m_maxHistory = max;
    // trim if necessary
    while (m_history.size() > m_maxHistory)
        m_history.removeFirst();
}

int HistoryLineEdit::maxHistory() const
{
    return m_maxHistory;
}

void HistoryLineEdit::addHistory(const QString& text)
{
    QString t = text.trimmed();
    if (t.isEmpty()) return;

    // 去掉历史中已有的相同项（保持唯一，并把最新放到末尾）
    m_history.removeAll(t);
    m_history.append(t);

    // 限制最大数目
    while (m_history.size() > m_maxHistory)
        m_history.removeFirst();

    // 退出历史浏览状态
    m_index = -1;
    m_editBuffer.clear();
}

QStringList HistoryLineEdit::history() const
{
    return m_history;
}

void HistoryLineEdit::clearHistory()
{
    m_history.clear();
    m_index = -1;
    m_editBuffer.clear();
}

void HistoryLineEdit::keyPressEvent(QKeyEvent *event)
{
    switch (event->key()) {
    case Qt::Key_Up:
        if (m_history.isEmpty()) {
            QLineEdit::keyPressEvent(event); // 退回默认处理（如果需要）
            return;
        }
        if (m_index == -1) {
            // 第一次进入历史浏览，保存当前编辑内容并定位到最新历史项
            m_editBuffer = this->text();
            m_index = m_history.size() - 1;
        } else if (m_index > 0) {
            --m_index;
        }
        if (m_index >= 0 && m_index < m_history.size())
            setText(m_history.at(m_index));
        selectAll();
        return;

    case Qt::Key_Down:
        if (m_index == -1) { // 不在历史浏览中，直接默认处理
            QLineEdit::keyPressEvent(event);
            return;
        }
        if (m_index < m_history.size() - 1) {
            ++m_index;
            setText(m_history.at(m_index));
        } else {
            // 越过最新历史，恢复编辑缓冲区
            m_index = -1;
            setText(m_editBuffer);
        }
        selectAll();
        return;

    case Qt::Key_Return:
    case Qt::Key_Enter: {
        QString t = this->text().trimmed();
        if (!t.isEmpty()) {
            addHistory(t); // 将当前提交加入历史（内部会去重并维护长度）
        } else {
            // 即使为空也重置浏览状态
            m_index = -1;
            m_editBuffer.clear();
        }
        emit submitted(this->text());
        // 仍然让基类处理（会触发 returnPressed 等信号），如果不需要可以不调用
        QLineEdit::keyPressEvent(event);
        return;
    }

    default:
        // 任何普通按键，都应退出历史浏览状态（即开始新的编辑）
        if (m_index != -1) {
            m_index = -1;
            m_editBuffer.clear();
        }
        QLineEdit::keyPressEvent(event);
        return;
    }
}