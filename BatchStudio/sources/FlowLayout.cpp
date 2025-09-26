// FlowLayout.cpp
#include "FlowLayout.h"

#include <QWidget>
#include <QWidgetItem>
#include <QStyle>
#include <QRect>

FlowLayout::FlowLayout(QWidget *parent, int margin, int hSpacing, int vSpacing)
    : QLayout(parent), m_HSpace(hSpacing), m_VSpace(vSpacing)
{
    setContentsMargins(margin, margin, margin, margin);
}

FlowLayout::FlowLayout(int margin, int hSpacing, int vSpacing)
    : m_HSpace(hSpacing), m_VSpace(vSpacing)
{
    setContentsMargins(margin, margin, margin, margin);
}

FlowLayout::~FlowLayout()
{
    QLayoutItem *item;
    while ((item = takeAt(0)))
        delete item;
}

void FlowLayout::addItem(QLayoutItem *item)
{
    m_ItemList.append(item);
}

void FlowLayout::addWidget(QWidget *w)
{
    addItem(new QWidgetItem(w));
    addChildWidget(w);
}

int FlowLayout::horizontalSpacing() const
{
    if (m_HSpace >= 0) return m_HSpace;
    return smartSpacing(QStyle::PM_LayoutHorizontalSpacing);
}

int FlowLayout::verticalSpacing() const
{
    if (m_VSpace >= 0) return m_VSpace;
    return smartSpacing(QStyle::PM_LayoutVerticalSpacing);
}

int FlowLayout::count() const
{
    return m_ItemList.size();
}

QLayoutItem *FlowLayout::itemAt(int index) const
{
    return m_ItemList.value(index);
}

QLayoutItem *FlowLayout::takeAt(int index)
{
    if (index >= 0 && index < m_ItemList.size())
        return m_ItemList.takeAt(index);
    return nullptr;
}

Qt::Orientations FlowLayout::expandingDirections() const
{
    return {};
}

bool FlowLayout::hasHeightForWidth() const
{
    return true;
}

int FlowLayout::heightForWidth(int width) const
{
    return doLayout(QRect(0, 0, width, 0), true);
}

void FlowLayout::setGeometry(const QRect &rect)
{
    QLayout::setGeometry(rect);
    doLayout(rect, false);
}

QSize FlowLayout::sizeHint() const
{
    return minimumSize();
}

QSize FlowLayout::minimumSize() const
{
    QSize size;
    for (QLayoutItem *item : m_ItemList)
        size = size.expandedTo(item->minimumSize());
    int left, top, right, bottom;
    getContentsMargins(&left, &top, &right, &bottom);
    size += QSize(left + right, top + bottom);
    return size;
}

int FlowLayout::doLayout(const QRect &rect, bool testOnly) const
{
    int left, top, right, bottom;
    getContentsMargins(&left, &top, &right, &bottom);
    QRect effectiveRect = rect.adjusted(+left, +top, -right, -bottom);
    int x = effectiveRect.x();
    int y = effectiveRect.y();
    int lineHeight = 0;

    int hSpace = horizontalSpacing();
    if (hSpace == -1) hSpace = 6;
    int vSpace = verticalSpacing();
    if (vSpace == -1) vSpace = 6;

    for (QLayoutItem *item : m_ItemList) {
        QSize itemSize = item->sizeHint();
        int nextX = x + itemSize.width() + hSpace;
        if (nextX - hSpace > effectiveRect.right() + 1 && lineHeight > 0) {
            x = effectiveRect.x();
            y = y + lineHeight + vSpace;
            nextX = x + itemSize.width() + hSpace;
            lineHeight = 0;
        }

        if (!testOnly)
            item->setGeometry(QRect(QPoint(x, y), itemSize));

        x = nextX;
        lineHeight = qMax(lineHeight, itemSize.height());
    }
    return y + lineHeight - rect.y() + bottom;
}

int FlowLayout::smartSpacing(QStyle::PixelMetric pm) const
{
    QObject *parentObj = this->parent();
    if (!parentObj) return -1;
    if (parentObj->isWidgetType()) {
        QWidget *pw = static_cast<QWidget *>(parentObj);
        return pw->style()->pixelMetric(pm, nullptr, pw);
    } else {
        return static_cast<QLayout *>(parentObj)->spacing();
    }
}

int FlowLayout::indexOf(QWidget *w) const
{
    if (!w) return -1;
    for (int i = 0; i < m_ItemList.size(); ++i) {
        QLayoutItem *it = m_ItemList.at(i);
        if (it->widget() == w)
            return i;
    }
    return -1;
}

bool FlowLayout::removeWidget(QWidget *w)
{
    int idx = indexOf(w);
    if (idx < 0) return false;
    QLayoutItem *it = takeAt(idx);
    if (!it) return false;
    delete it;
    if (w) {
        w->setParent(nullptr);
        delete w;
    }
    return true;
}

QWidget *FlowLayout::takeWidget(int index)
{
    if (index < 0 || index >= m_ItemList.size()) return nullptr;
    QLayoutItem *it = takeAt(index);
    if (!it) return nullptr;
    QWidget *w = it->widget();
    delete it;
    return w;
}

QWidget *FlowLayout::takeLastWidget()
{
    return takeWidget(m_ItemList.size() - 1);
}

bool FlowLayout::removeLastWidget()
{
    if (m_ItemList.isEmpty()) return false;
    QLayoutItem *it = takeAt(m_ItemList.size() - 1);
    if (!it) return false;
    QWidget *w = it->widget();
    delete it;
    if (w) {
        w->setParent(nullptr);
        delete w;
    }
    return true;
}

void FlowLayout::clear()
{
    QLayoutItem *item;
    while ((item = takeAt(0))) {
        QWidget *w = item->widget();
        delete item;
        if (w) {
            w->setParent(nullptr);
            delete w;
        }
    }
}