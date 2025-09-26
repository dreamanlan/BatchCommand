// FlowLayout.h (新增 remove/clear 接口)
#ifndef FLOWLAYOUT_H
#define FLOWLAYOUT_H

#include <QLayout>
#include <QList>
#include <QStyle>

class QWidgetItem;
class QWidget;

class FlowLayout : public QLayout
{
public:
    explicit FlowLayout(QWidget *parent = nullptr, int margin = -1, int hSpacing = -1, int vSpacing = -1);
    explicit FlowLayout(int margin = -1, int hSpacing = -1, int vSpacing = -1);
    ~FlowLayout() override;

    void addItem(QLayoutItem *item) override;
    void addWidget(QWidget *w);
    int horizontalSpacing() const;
    int verticalSpacing() const;
    int count() const override;
    QLayoutItem *itemAt(int index) const override;
    QLayoutItem *takeAt(int index) override;
    Qt::Orientations expandingDirections() const override;
    bool hasHeightForWidth() const override;
    int heightForWidth(int width) const override;
    void setGeometry(const QRect &rect) override;
    QSize sizeHint() const override;
    QSize minimumSize() const override;

    bool removeWidget(QWidget *w);
    QWidget *takeWidget(int index);
    QWidget *takeLastWidget();
    bool removeLastWidget();
    void clear();
    int indexOf(QWidget *w) const;

private:
    int doLayout(const QRect &rect, bool testOnly) const;
    int smartSpacing(QStyle::PixelMetric pm) const;

    QList<QLayoutItem *> m_ItemList;
    int m_HSpace;
    int m_VSpace;
};

#endif // FLOWLAYOUT_H