#include <QDir>
#include <QFileDialog>
#include <QFormLayout>
#include <QVBoxLayout>
#include <QScrollArea>
#include <QHBoxLayout>
#include <QLabel>
#include <QPushButton>
#include <QSettings>
#include "binarysettingswidget.h"
#include "processutils.h"
#include "HostCLR.h"

BinarySettingsWidget* BinarySettingsWidget::s_pInstance = nullptr;

BinarySettingsWidget::BinarySettingsWidget(QWidget *parent)
    : QWidget(parent)
{
    s_pInstance = this;
    buildForm();
}

void BinarySettingsWidget::buildForm()
{
    QSettings settings;
    m_UseJavaAndAdb = settings.value("use_java_and_adb", false).toBool();

    QScrollArea* scroll = new QScrollArea();
    scroll->setWidgetResizable(true);

    QWidget* scrollWidget = new QWidget;
    m_ContainerLayout = new QFormLayout();
    scrollWidget->setLayout(m_ContainerLayout);
    scroll->setWidget(scrollWidget);

    QVBoxLayout *lay = new QVBoxLayout(this);
    lay->setContentsMargins(0, 0, 0, 0);
    lay->setSpacing(0);
    lay->addWidget(scroll);

    scroll->setSizePolicy(QSizePolicy::Expanding, QSizePolicy::Expanding);

    auto layout = m_ContainerLayout;
    QPushButton* button;
    QLabel* label;
    QHBoxLayout* child;

    child = new QHBoxLayout();
    child->addWidget(m_EditDslScript = new QLineEdit(this));
    child->addWidget(button = new QPushButton(tr("Browse"), this));
    connect(button, &QPushButton::pressed, this, &BinarySettingsWidget::handleBrowseDsl);
    layout->addRow("Dsl Script", child);

    if (m_UseJavaAndAdb) {
        layout->addRow(tr("Java"), m_EditJavaExe = new QLineEdit(this));
        child = new QHBoxLayout();
        child->addWidget(button = new QPushButton(tr("Browse"), this));
        connect(button, &QPushButton::pressed, this, &BinarySettingsWidget::handleBrowseJava);
        child->addWidget(label = new QLabel(QString("<a href=\"https://www.oracle.com/technetwork/java/javase/downloads/index.html\">%1</a>").arg(tr("Get it here!")), this), 1);
        label->setOpenExternalLinks(true);
        label->setTextInteractionFlags(Qt::TextBrowserInteraction);
        label->setTextFormat(Qt::RichText);
        layout->addRow("", child);

        layout->addRow(tr("Java Heap (MBs)"), m_SpinJavaHeap = new QSpinBox(this));
        m_SpinJavaHeap->setMinimum(10);
        m_SpinJavaHeap->setMaximum(65535);
        m_SpinJavaHeap->setSingleStep(1);

        layout->addRow(tr("Jadx"), m_EditJadxExe = new QLineEdit(this));
        child = new QHBoxLayout();
        child->addWidget(button = new QPushButton(tr("Browse"), this));
        connect(button, &QPushButton::pressed, this, &BinarySettingsWidget::handleBrowseJadx);
        child->addWidget(label = new QLabel(QString("<a href=\"https://github.com/skylot/jadx/releases\">%1</a>").arg(tr("Get it here!")), this), 1);
        label->setOpenExternalLinks(true);
        label->setTextInteractionFlags(Qt::TextBrowserInteraction);
        label->setTextFormat(Qt::RichText);
        layout->addRow("", child);

        layout->addRow(tr("ADB"), m_EditAdbExe = new QLineEdit(this));
        child = new QHBoxLayout();
        child->addWidget(button = new QPushButton(tr("Browse"), this));
        connect(button, &QPushButton::pressed, this, &BinarySettingsWidget::handleBrowseAdb);
        child->addWidget(label = new QLabel(QString("<a href=\"https://developer.android.com/studio/releases/platform-tools\">%1</a>").arg(tr("Get it here!")), this), 1);
        label->setOpenExternalLinks(true);
        label->setTextInteractionFlags(Qt::TextBrowserInteraction);
        label->setTextFormat(Qt::RichText);
        layout->addRow("", child);

        layout->addRow(tr("Zip Align"), m_EditZipAlignExe = new QLineEdit(this));
        child = new QHBoxLayout();
        child->addWidget(button = new QPushButton(tr("Browse"), this));
        connect(button, &QPushButton::pressed, this, &BinarySettingsWidget::handleBrowseZipAlign);
        child->addWidget(label = new QLabel(QString("<a href=\"https://developer.android.com/studio/releases/build-tools\">%1</a>").arg(tr("Get it here!")), this), 1);
        label->setOpenExternalLinks(true);
        label->setTextInteractionFlags(Qt::TextBrowserInteraction);
        label->setTextFormat(Qt::RichText);
        layout->addRow("", child);
    }

    auto dsl = settings.value("dsl_script", "../Managed/Script.dsl").toString();
    if (dsl.isEmpty()) {
        m_EditDslScript->setText(ProcessUtils::dslScript());
    }
    else {
        m_EditDslScript->setText(dsl);
    }

    if (m_UseJavaAndAdb) {
        auto adb = settings.value("adb_exe").toString();
        if (adb.isEmpty()) {
            m_EditAdbExe->setText(ProcessUtils::adbExe());
        }
        else {
            m_EditAdbExe->setText(adb);
        }
        m_EditJadxExe->setText(settings.value("jadx_exe").toString());
        auto java = settings.value("java_exe").toString();
        if (adb.isEmpty()) {
            m_EditJavaExe->setText(ProcessUtils::javaExe());
        }
        else {
            m_EditJavaExe->setText(java);
        }
        m_EditZipAlignExe->setText(settings.value("zipalign_exe").toString());
        m_SpinJavaHeap->setValue(ProcessUtils::javaHeapSize());
    }

    if (load_setting_fptr) {
        ProcessResult result;
        load_setting_fptr(&result);
    }
}

void BinarySettingsWidget::addSettingItem(const QString& name, const QString& labelStr, const QString& tooltip, int type, const QString& default_value, const QString& ext, const QString& link)
{
    auto layout = m_ContainerLayout;
    QLineEdit* edit;
    QPushButton* button;
    QLabel* label;
    QHBoxLayout* child;
    QSpinBox* spin;
    QDoubleSpinBox* dspin;

    switch (type) {
    case ST_FILE:
        if (!link.isEmpty()) {
            layout->addRow(labelStr, edit = new QLineEdit(this));
            child = new QHBoxLayout();
            child->addWidget(button = new QPushButton(tr("Browse"), this));
            QString nameStr(name);
            QString extStr(ext);
            connect(button, &QPushButton::pressed, this, [=]() {
                handleBrowse(edit, nameStr, extStr);
                });
            child->addWidget(label = new QLabel(QString("<a href=\"%1\">%2</a>").arg(link, tr("Get it here!")), this), 1);
            label->setOpenExternalLinks(true);
            label->setTextInteractionFlags(Qt::TextBrowserInteraction);
            label->setTextFormat(Qt::RichText);
            layout->addRow("", child);
        }
        else {
            child = new QHBoxLayout();
            child->addWidget(edit = new QLineEdit(this));
            child->addWidget(button = new QPushButton(tr("Browse"), this));
            connect(button, &QPushButton::pressed, this, [=]() {
                handleBrowse(edit, name, ext);
                });
            layout->addRow(labelStr, child);
        }
        if (!default_value.isEmpty()) {
            QSettings settings;
            edit->setText(settings.value(name, default_value).toString());
        }
        edit->setToolTip(tooltip);
        break;
    case ST_INT: {
        child = new QHBoxLayout();
        child->addWidget(spin = new QSpinBox(this));
        spin->setRange(std::numeric_limits<int>::min(), std::numeric_limits<int>::max());
        spin->setSingleStep(1);
        child->addWidget(button = new QPushButton(tr("Save"), this));
        connect(button, &QPushButton::pressed, this, [=]() {
            QSettings settings;
            settings.setValue(name, spin->value());
            settings.sync();
            });
        layout->addRow(labelStr, child);
        if (!default_value.isEmpty()) {
            QSettings settings;
            spin->setValue(settings.value(name, QString(default_value)).toInt());
        }
        spin->setToolTip(tooltip);
        break;
    }
    case ST_FLOAT: {
        child = new QHBoxLayout();
        child->addWidget(dspin = new QDoubleSpinBox(this));
        dspin->setRange(std::numeric_limits<float>::min(), std::numeric_limits<float>::max());
        dspin->setDecimals(7);
        dspin->setSingleStep(1.0);
        child->addWidget(button = new QPushButton(tr("Save"), this));
        connect(button, &QPushButton::pressed, this, [=]() {
            QSettings settings;
            settings.setValue(name, static_cast<float>(dspin->value()));
            settings.sync();
            });
        layout->addRow(labelStr, child);
        if (!default_value.isEmpty()) {
            QSettings settings;
            dspin->setValue(settings.value(name, QString(default_value)).toFloat());
        }
        dspin->setToolTip(tooltip);
        break;
    }
    case ST_DOUBLE: {
        child = new QHBoxLayout();
        child->addWidget(dspin = new QDoubleSpinBox(this));
        dspin->setRange(std::numeric_limits<double>::min(), std::numeric_limits<double>::max());
        dspin->setDecimals(15);
        dspin->setSingleStep(1.0);
        child->addWidget(button = new QPushButton(tr("Save"), this));
        connect(button, &QPushButton::pressed, this, [=]() {
            QSettings settings;
            settings.setValue(name, dspin->value());
            settings.sync();
            });
        layout->addRow(labelStr, child);
        if (!default_value.isEmpty()) {
            QSettings settings;
            dspin->setValue(settings.value(name, QString(default_value)).toDouble());
        }
        dspin->setToolTip(tooltip);
        break;
    }
    case ST_STRING: {
        child = new QHBoxLayout();
        child->addWidget(edit = new QLineEdit(this));
        child->addWidget(button = new QPushButton(tr("Save"), this));
        connect(button, &QPushButton::pressed, this, [=]() {
            QSettings settings;
            settings.setValue(name, edit->text());
            settings.sync();
            });
        layout->addRow(labelStr, child);
        if (!default_value.isEmpty()) {
            QSettings settings;
            edit->setText(settings.value(name, default_value).toString());
        }
        edit->setToolTip(tooltip);
        break;
    }
    }
}

void BinarySettingsWidget::handleBrowse(QLineEdit* edit, const QString& name, const QString& ext)
{
    const QString path = QFileDialog::getOpenFileName(this,
        tr("Browse File (%1)").arg(ext),
        edit->text()
        , tr("File(s) (%1)").arg(ext)
    );
    if (!path.isEmpty()) {
        edit->setText(QDir::toNativeSeparators(path));
        QSettings settings;
        settings.setValue(name, path);
        settings.sync();
    }
}

void BinarySettingsWidget::handleBrowseDsl()
{
    const QString path = QFileDialog::getOpenFileName(this,
        tr("Browse Dsl Script (*.dsl)"),
        m_EditDslScript->text()
        , tr("Dsl Script File(s) (*.dsl)")
    );
    if (!path.isEmpty()) {
        m_EditDslScript->setText(QDir::toNativeSeparators(path));
    }
}

void BinarySettingsWidget::handleBrowseAdb()
{
    const QString path = QFileDialog::getOpenFileName(this,
#ifdef Q_OS_WIN
                                                      tr("Browse ADB (adb.exe)"),
#else
                                                      tr("Browse ADB"),
#endif
                                                      m_EditAdbExe->text()
#ifdef Q_OS_WIN
                                                      , tr("Executable File(s) (*.exe)")
#endif
                                                      );
    if (!path.isEmpty()) {
        m_EditAdbExe->setText(QDir::toNativeSeparators(path));
    }
}

void BinarySettingsWidget::handleBrowseJadx()
{
    const QString path = QFileDialog::getOpenFileName(this,
#ifdef Q_OS_WIN
                                                      tr("Browse Jadx (jadx.bat)"),
#else
                                                      tr("Browse Jadx"),
#endif
                                                      m_EditJadxExe->text()
#ifdef Q_OS_WIN
                                                      , tr("Windows Batch File(s) (*.bat)")
#endif
                                                      );
    if (!path.isEmpty()) {
        m_EditJadxExe->setText(QDir::toNativeSeparators(path));
    }
}

void BinarySettingsWidget::handleBrowseJava()
{
    const QString path = QFileDialog::getOpenFileName(this,
#ifdef Q_OS_WIN
                                                      tr("Browse Java (java.exe)"),
#else
                                                      tr("Browse Java"),
#endif
                                                      m_EditJavaExe->text()
#ifdef Q_OS_WIN
                                                      , tr("Executable File(s) (*.exe)")
#endif
                                                      );
    if (!path.isEmpty()) {
        m_EditJavaExe->setText(QDir::toNativeSeparators(path));
    }
}

void BinarySettingsWidget::handleBrowseZipAlign()
{
    const QString path = QFileDialog::getOpenFileName(this,
#ifdef Q_OS_WIN
                                                      tr("Browse Zip Align (zipalign.exe)"),
#else
                                                      tr("Browse Zip Align"),
#endif
                                                      m_EditAdbExe->text()
#ifdef Q_OS_WIN
                                                      , tr("Executable File(s) (*.exe)")
#endif
                                                      );
    if (!path.isEmpty()) {
        m_EditZipAlignExe->setText(QDir::toNativeSeparators(path));
    }
}

void BinarySettingsWidget::save()
{
    QSettings settings;
    settings.setValue("dsl_script", m_EditDslScript->text());
    if (m_UseJavaAndAdb) {
        settings.setValue("adb_exe", m_EditAdbExe->text());
        settings.setValue("jadx_exe", m_EditJadxExe->text());
        settings.setValue("java_exe", m_EditJavaExe->text());
        settings.setValue("java_heap", m_SpinJavaHeap->value());
        settings.setValue("zipalign_exe", m_EditZipAlignExe->text());
    }
    settings.sync();
}

BinarySettingsWidget::~BinarySettingsWidget()
{
    s_pInstance = nullptr;
}

BinarySettingsWidget* BinarySettingsWidget::instance()
{
    return s_pInstance;
}
