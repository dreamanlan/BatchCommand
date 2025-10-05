#ifndef BINARYSETTINGSWIDGET_H
#define BINARYSETTINGSWIDGET_H

#include <QCheckBox>
#include <QLineEdit>
#include <QSpinBox>
#include <QWidget>

class QFormLayout;
class BinarySettingsWidget : public QWidget
{
    Q_OBJECT
public:
    explicit BinarySettingsWidget(QWidget *parent = nullptr);
    ~BinarySettingsWidget();
public:
    void addSettingItem(const QString& name, const QString& label, const QString& tooltip, int type, const QString& default_value, const QString& ext, const QString& link);
private:
    QFormLayout* m_ContainerLayout = nullptr; // 容器中每行的垂直布局
    bool m_UseJavaAndAdb = false;

    QLineEdit *m_EditDslScript;
    QLineEdit *m_EditAdbExe;
    QLineEdit *m_EditJadxExe;
    QLineEdit *m_EditJavaExe;
    QLineEdit *m_EditZipAlignExe;
    QSpinBox *m_SpinJavaHeap;
    void buildForm();
    void handleBrowse(QLineEdit* edit, const QString& name, const QString& ext);
private slots:
    void handleBrowseDsl();
    void handleBrowseAdb();
    void handleBrowseJadx();
    void handleBrowseJava();
    void handleBrowseZipAlign();
public slots:
    void save();
public:
    static BinarySettingsWidget* instance();
private:
    static BinarySettingsWidget* s_pInstance;
};

#endif // BINARYSETTINGSWIDGET_H
