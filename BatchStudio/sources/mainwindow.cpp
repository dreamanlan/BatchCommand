#include <QApplication>
#include <QClipboard>
#include <QCloseEvent>
#include <QDebug>
#include <QDesktopServices>
#include <QDockWidget>
#include <QFileDialog>
#include <QFrame>
#include <QHeaderView>
#include <QInputDialog>
#include <QMenuBar>
#include <QMessageBox>
#include <QProcess>
#include <QSettings>
#include <QStatusBar>
#include <QTabWidget>
#include <QTextDocumentFragment>
#include <QThread>
#include <QTimer>
#include <QToolBar>
#include <QTreeWidgetItem>
#include <QUrl>
#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QScrollArea>
#include <QPushButton>
#include <QToolButton>
#if defined(Q_OS_WIN)
#include <windows.h>
#endif
#include "buildworker.h"
#include "installworker.h"
#include "commandworker.h"
#include "findreplacedialog.h"
#include "hexedit.h"
#include "imageviewerwidget.h"
#include "mainwindow.h"
#include "settingsdialog.h"
#include "sourcecodeedit.h"
#include "HostCLR.h"
#include "FlowLayout.h"
#include "HistoryLineEdit.h"

#define COLOR_CODE 0x2ad2c9
#define COLOR_COMMAND 0xd0d2d3
#define COLOR_OUTPUT 0xffffff
#define COLOR_ERROR 0xfb0a2a

#define IMAGE_EXTENSIONS "gif|jpeg|jpg|png"
#define TEXT_EXTENSIONS "java|html|properties|smali|txt|log|json|xml|yaml|yml|c|cpp|cxx|h|hpp|hxx|m|mm|cs|py|lua|js|pl|jam|dsl|scp"

#define URL_CONTRIBUTE "https://github.com/dreamanlan/BatchCommand/tree/master/BatchStudio"
#define URL_DOCUMENTATION "https://github.com/dreamanlan/BatchCommand/tree/master/BatchStudio"
#define URL_ISSUES "https://github.com/dreamanlan/BatchCommand/issues"
#define URL_THANKS "https://forum.xda-developers.com/showthread.php?t=2493107"

#define WINDOW_WIDTH 800
#define WINDOW_HEIGHT 600

MainWindow* MainWindow::s_pInstance = nullptr;

MainWindow::MainWindow(const QMap<QString, QString> &versions, QWidget *parent)
    : QMainWindow(parent), m_ProgressDialog(nullptr), m_FindReplaceDialog(nullptr)
{
    s_pInstance = this;

    setDockNestingEnabled(true);
    addDockWidget(Qt::LeftDockWidgetArea, m_DockFolder = buildFolderDock());
    addDockWidget(Qt::LeftDockWidgetArea, m_DockFiles = buildFilesDock());
    addDockWidget(Qt::BottomDockWidgetArea, m_DockButtons = buildButtonsDock(), Qt::Vertical);
    addDockWidget(Qt::BottomDockWidgetArea, m_DockConsole = buildConsoleDock(), Qt::Vertical);
    m_DockFolder->setAllowedAreas(Qt::LeftDockWidgetArea | Qt::RightDockWidgetArea);
    m_DockFiles->setAllowedAreas(Qt::LeftDockWidgetArea | Qt::RightDockWidgetArea);
    m_DockButtons->setAllowedAreas(Qt::BottomDockWidgetArea | Qt::TopDockWidgetArea);
    m_DockConsole->setAllowedAreas(Qt::BottomDockWidgetArea | Qt::TopDockWidgetArea);
    refreshDockWidgets(1000);
    setMenuBar(buildMenuBar());
    addToolBar(Qt::LeftToolBarArea, buildMainToolBar());
    setCentralWidget(buildCentralWidget());
    setMinimumSize(WINDOW_WIDTH, WINDOW_HEIGHT);
    setStatusBar(buildStatusBar(versions));
    setWindowTitle(tr("Batch Studio").append(" - https://github.com/dreamanlan/BatchCommand/tree/master/BatchStudio"));
    connect(QApplication::clipboard(), &QClipboard::dataChanged, this, &MainWindow::handleClipboardDataChanged);
    QSettings settings;
    const bool dark = settings.value("dark_theme", false).toBool();
    QFile qss(QString(":/styles/%1.qss").arg(dark ? "dark" : "light"));
    qss.open(QIODevice::ReadOnly | QIODevice::Text);
    QTextStream contents(&qss);
    setStyleSheet(contents.readAll());
    qss.close();
    if (settings.value("app_maximized").toBool()) {
        showMaximized();
    } else {
        resize(settings.value("app_size", QSize(WINDOW_WIDTH, WINDOW_HEIGHT)).toSize());
    }
    const QVariant state = settings.value("dock_state");
    if (state.isValid()) {
        restoreState(state.toByteArray());
    }
    m_ActionViewFolder->setChecked(m_DockFolder->isVisible());
    m_ActionViewFiles->setChecked(m_DockFiles->isVisible());
    m_ActionViewButtons->setChecked(m_DockButtons->isVisible());
    m_ActionViewConsole->setChecked(m_DockConsole->isVisible());
    QTimer::singleShot(100, [=] {
        QSettings settings;
        const QStringList folders = settings.value("open_folders").toStringList();
        foreach(const QString & folder, folders) {
            if (QFile::exists(folder)) {
                openFolder(folder);
            }
        }
        const QStringList files = settings.value("open_files").toStringList();
        foreach (const QString& file, files) {
            if (QFile::exists(file)) {
                openFile(file);
            }
        }
        bool missing = false;
        QString missingBinary;
        add_log(tr("3rd-party binaries:"));
        foreach (const QString& binary, versions.keys()) {
            const QString& ver = versions.value(binary);
            if (ver.isEmpty()) {
#ifdef QT_DEBUG
                qDebug() << binary << "is missing";
#endif
                if (!missing) {
                    missing = true;
                    missingBinary = binary;
                }
                add_log(tr("\tbinary %1 is missing").arg(binary));
            }
            else {
                add_log(tr("\t%1: %2").arg(binary, ver));
            }
        }
        if (missing) {
            int btn = QMessageBox::information(
                        this,
                        tr("Requirements"),
                        tr("One or more required 3rd-party binaries are missing. Please review settings first. (%1)").arg(missingBinary),
                        QMessageBox::Ok,
                        QMessageBox::Cancel);
            if (btn == 0) {
                (new SettingsDialog(1, this))->exec();
            }
        }
    });

    QString exePath = QCoreApplication::applicationFilePath();
    QString absDir = QFileInfo(exePath).absolutePath();
    std::string base_path = absDir.toStdString();
    QCoreApplication::addLibraryPath(absDir);
    QDir::setCurrent(absDir);

    int r = load_hostfxr();
    if (r != 0) {
        QMessageBox::information(this, "Error", QString("Failed to load hostfxr: %1").arg(r));
        return;
    }
    load_dotnet_method();

    if (init_csharp_fptr) {
        ProcessResult result{};
        init_csharp_fptr(base_path.c_str(), &result);
    }
}

QMenuBar *MainWindow::buildMenuBar()
{
    auto menubar = new QMenuBar(this);
    auto file = menubar->addMenu(tr("File"));
    file->addAction(tr("New"), QKeySequence::New, this, &MainWindow::handleActionNew);
    auto open = file->addMenu(tr("Open"));
    open->addAction(tr("File"), QKeySequence::Open, this, &MainWindow::handleActionFile);
    open->addAction(tr("Folder"), QKeySequence(tr("Ctrl+D", "Folder|Open")), this, &MainWindow::handleActionFolder);
    file->addSeparator();
    m_ActionClose = file->addAction(tr("Close"), QKeySequence::Close, this, &MainWindow::handleActionClose);
    m_ActionClose->setEnabled(false);
    m_ActionCloseAll = file->addAction(tr("Close All"), this, &MainWindow::handleActionCloseAll);
    m_ActionCloseAll->setEnabled(false);
    file->addSeparator();
    m_ActionSave = file->addAction(tr("Save"), QKeySequence::Save, this, &MainWindow::handleActionSave);
    m_ActionSave->setEnabled(false);
    m_ActionSaveAll = file->addAction(tr("Save All"), this, &MainWindow::handleActionSaveAll);
    m_ActionSaveAll->setEnabled(false);
    file->addSeparator();
    file->addAction(tr("Quit"), QKeySequence::Quit, this, &MainWindow::handleActionQuit);
    auto edit = menubar->addMenu(tr("Edit"));
    m_ActionUndo = edit->addAction(tr("Undo"), QKeySequence::Undo, this, &MainWindow::handleActionUndo);
    m_ActionUndo->setEnabled(false);
    m_ActionRedo = edit->addAction(tr("Redo"), QKeySequence::Redo, this, &MainWindow::handleActionRedo);
    m_ActionRedo->setEnabled(false);
    edit->addSeparator();
    m_ActionCut = edit->addAction(tr("Cut"), QKeySequence::Cut, this, &MainWindow::handleActionCut);
    m_ActionCut->setEnabled(false);
    m_ActionCopy = edit->addAction(tr("Copy"), QKeySequence::Copy, this, &MainWindow::handleActionCopy);
    m_ActionCopy->setEnabled(false);
    m_ActionPaste = edit->addAction(tr("Paste"), QKeySequence::Paste, this, &MainWindow::handleActionPaste);
    m_ActionPaste->setEnabled(false);
    edit->addSeparator();
    m_ActionFind = edit->addAction(tr("Find"), QKeySequence::Find, this, &MainWindow::handleActionFind);
    m_ActionFind->setEnabled(false);
    m_ActionReplace = edit->addAction(tr("Replace"), QKeySequence::Replace, this, &MainWindow::handleActionReplace);
    m_ActionReplace->setEnabled(false);
    m_ActionGoto = edit->addAction(tr("Go To"), this, &MainWindow::handleActionGoto);
    m_ActionGoto->setEnabled(false);
    edit->addSeparator();
    edit->addAction(tr("Clear Console"), QKeySequence::NoMatch, this, &MainWindow::handleActionClearConsole);
    edit->addSeparator();
    edit->addAction(tr("Settings"), QKeySequence::Preferences, this, &MainWindow::handleActionSettings);
    auto view = menubar->addMenu(tr("View"));
    m_ActionViewFolder = view->addAction(tr("Folder"));
    m_ActionViewFolder->setCheckable(true);
    connect(m_ActionViewFolder, &QAction::toggled, m_DockFolder, &QDockWidget::setVisible);
    connect(m_DockFolder, &QDockWidget::visibilityChanged, m_ActionViewFolder, &QAction::setChecked);
    m_ActionViewFiles = view->addAction(tr("Files"));
    m_ActionViewFiles->setCheckable(true);
    connect(m_ActionViewFiles, &QAction::toggled, m_DockFiles, &QDockWidget::setVisible);
    connect(m_DockFiles, &QDockWidget::visibilityChanged, m_ActionViewFiles, &QAction::setChecked);
    m_ActionViewButtons = view->addAction(tr("Buttons"));
    m_ActionViewButtons->setCheckable(true);
    connect(m_ActionViewButtons, &QAction::toggled, m_DockButtons, &QDockWidget::setVisible);
    connect(m_DockButtons, &QDockWidget::visibilityChanged, m_ActionViewButtons, &QAction::setChecked);
    m_ActionViewConsole = view->addAction(tr("Console"));
    m_ActionViewConsole->setCheckable(true);
    connect(m_ActionViewConsole, &QAction::toggled, m_DockConsole, &QDockWidget::setVisible);
    connect(m_DockConsole, &QDockWidget::visibilityChanged, m_ActionViewConsole, &QAction::setChecked);
    view->addSeparator();
    m_ActionViewWindowsConsole = view->addAction(tr("Windows Console"));
    m_ActionViewWindowsConsole->setCheckable(true);
    connect(m_ActionViewWindowsConsole, &QAction::toggled, this, &MainWindow::handleActionWindowsConsole);
    view->addSeparator();
    view->addAction(tr("Refresh Layout"), QKeySequence::NoMatch, this, &MainWindow::handleActionRefreshLayout);
    auto commands = menubar->addMenu(tr("Commands"));
    commands->addAction(tr("Refresh"), QKeySequence::Refresh, this, &MainWindow::handleActionRefresh);
    commands->addSeparator();
    m_SchemesMenu = commands->addMenu(tr("Load Scheme"));
    commands->addSeparator();
    m_ActionBuild1 = commands->addAction(tr("Build"), this, &MainWindow::handleActionBuild);
    m_ActionBuild1->setEnabled(false);
    m_ActionInstall1 = commands->addAction(tr("Install"), this, &MainWindow::handleActionInstall);
    m_ActionInstall1->setEnabled(false);
    auto help = menubar->addMenu(tr("Help"));
    help->addAction(tr("About"), this, &MainWindow::handleActionAbout);
    help->addAction(tr("Documentation"), this, &MainWindow::handleActionDocumentation);
    help->addSeparator();
    auto feedback = help->addMenu(tr("Feedback"));
    feedback->addAction(tr("Say Thanks"), this, &MainWindow::handleActionSayThanks);
    feedback->addAction(tr("Report Issues"), this, &MainWindow::handleActionReportIssues);
    help->addAction(tr("Contribute"), this, &MainWindow::handleActionContribute);
    return menubar;
}

QToolBar *MainWindow::buildMainToolBar()
{
    auto toolbar = new QToolBar(this);
    QAction* refresh = toolbar->addAction(QIcon(":/icons/icons8/refresh.png"), tr("Refresh Schemes"), this, &MainWindow::handleActionRefresh);
    toolbar->addSeparator();

    QTimer::singleShot(1000, [=] {
        refresh->trigger();
    });

    m_SchemesButton = new QToolButton(this);
    m_SchemesButton->setText(tr("Load Scheme"));
    m_SchemesButton->setIcon(QIcon(":/icons/icons8/load_scheme.png"));
    m_SchemesButton->setMenu(m_SchemesMenu);

    m_SchemesButton->setPopupMode(QToolButton::InstantPopup);
    toolbar->addWidget(m_SchemesButton);
    toolbar->addSeparator();
    toolbar->addAction(QIcon(":/icons/icons8/new_file.png"), tr("New File"), this, &MainWindow::handleActionNew);
    toolbar->addAction(QIcon(":/icons/icons8/open_file.png"), tr("Open File"), this, &MainWindow::handleActionFile);
    toolbar->addAction(QIcon(":/icons/icons8/icons8-folder-48.png"), tr("Open Folder"), this, &MainWindow::handleActionFolder);
    toolbar->addSeparator();
    toolbar->addAction(QIcon(":/icons/icons8/icons8-gear-48.png"), tr("Settings"), this, &MainWindow::handleActionSettings);

    auto empty = new QWidget(this);
    empty->setSizePolicy(QSizePolicy::Expanding, QSizePolicy::Expanding);
    toolbar->addWidget(empty);
    m_ActionBuild2 = toolbar->addAction(QIcon(":/icons/icons8/icons8-hammer-48.png"), tr("Build"), this, &MainWindow::handleActionBuild);
    m_ActionBuild2->setEnabled(false);
    m_ActionInstall2 = toolbar->addAction(QIcon(":/icons/icons8/icons8-software-installer-48.png"), tr("Install"), this, &MainWindow::handleActionInstall);
    m_ActionInstall2->setEnabled(false);
    toolbar->setIconSize(QSize(48, 48));
    toolbar->setMovable(false);
    toolbar->setObjectName("Toolbar");
    return toolbar;
}

QDockWidget *MainWindow::buildFolderDock()
{
    auto dock = new QDockWidget(tr("Folder"), this);
    m_DirectoryTree = new QTreeWidget(this);
    m_DirectoryTree->header()->hide();
    m_DirectoryTree->setContextMenuPolicy(Qt::CustomContextMenu);
    m_DirectoryTree->setEditTriggers(QAbstractItemView::NoEditTriggers);
    m_DirectoryTree->setMinimumWidth(240);
    m_DirectoryTree->setSelectionBehavior(QAbstractItemView::SelectItems);
    m_DirectoryTree->setSelectionMode(QAbstractItemView::SingleSelection);
    m_DirectoryTree->setSortingEnabled(false);
    connect(m_DirectoryTree, &QTreeWidget::customContextMenuRequested, this, &MainWindow::handleTreeContextMenu);
    connect(m_DirectoryTree, &QTreeWidget::doubleClicked, this, &MainWindow::handleTreeDoubleClicked);
    connect(m_DirectoryTree->selectionModel(), &QItemSelectionModel::selectionChanged, this, &MainWindow::handleTreeSelectionChanged);
    dock->setObjectName("FolderDock");
    dock->setWidget(m_DirectoryTree);
    return dock;
}

QDockWidget* MainWindow::buildFilesDock()
{
    auto dock = new QDockWidget(tr("Files"), this);
    m_ListOpenFiles = new QListView(this);
    m_ListOpenFiles->setContextMenuPolicy(Qt::CustomContextMenu);
    m_ListOpenFiles->setEditTriggers(QAbstractItemView::NoEditTriggers);
    m_ListOpenFiles->setMinimumWidth(240);
    m_ListOpenFiles->setModel(m_ModelOpenFiles = new QStandardItemModel(m_ListOpenFiles));
    m_ListOpenFiles->setSelectionBehavior(QAbstractItemView::SelectItems);
    m_ListOpenFiles->setSelectionMode(QAbstractItemView::SingleSelection);
    connect(m_ListOpenFiles, &QListView::customContextMenuRequested, this, &MainWindow::handleListViewContextMenu);
    connect(m_ListOpenFiles->selectionModel(), &QItemSelectionModel::selectionChanged, this, &MainWindow::handleFilesSelectionChanged);
    dock->setObjectName("FilesDock");
    dock->setWidget(m_ListOpenFiles);
    return dock;
}

QWidget* MainWindow::buildCentralWidget()
{
    m_CentralStack = new QStackedWidget(this);
    auto empty = new QLabel(m_CentralStack);
    empty->setAlignment(Qt::AlignCenter);
    empty->setMargin(32);
    empty->setStyleSheet("QLabel { color: rgba(0, 0, 0, 50%) }");
    empty->setText(tr("<h1>%1</h1>")
        .arg("No files open."));
    empty->setWordWrap(true);
    m_CentralStack->addWidget(empty);
    m_CentralStack->addWidget(m_TabEditors = new QTabWidget(this));
    connect(m_TabEditors, &QTabWidget::currentChanged, this, &MainWindow::handleTabChanged);
    connect(m_TabEditors, &QTabWidget::tabCloseRequested, this, &MainWindow::handleTabCloseRequested);
    m_TabEditors->setTabsClosable(true);
    m_CentralStack->setCurrentIndex(0);
    return m_CentralStack;
}

QDockWidget* MainWindow::buildButtonsDock()
{
    auto dock = new QDockWidget(tr("Buttons"), this);
    dock->setObjectName("ButtonsDock");

    QWidget* dockContainer = new QWidget;
    QVBoxLayout* dockLayout = new QVBoxLayout(dockContainer);
    dockLayout->setContentsMargins(6, 6, 6, 6);
    dockLayout->setSpacing(6);

    QWidget* upperRow = new QWidget;
    QHBoxLayout* upperLayout = new QHBoxLayout(upperRow);
    upperLayout->setContentsMargins(0, 0, 0, 0);
    upperLayout->setSpacing(6);

    QLabel* lbl0 = new QLabel(tr("Dsl Script:"));
    lbl0->setMinimumWidth(64);
    HistoryLineEdit* lineEdit0 = new HistoryLineEdit;
    lineEdit0->setMaxHistory(100);
    QPushButton* addBtn0 = new QPushButton(tr("Execute"));

    upperLayout->addWidget(lbl0);
    upperLayout->addWidget(lineEdit0);
    upperLayout->addWidget(addBtn0);

    dockLayout->addWidget(upperRow);

    QScrollArea* scroll = new QScrollArea;
    scroll->setWidgetResizable(true);

    QWidget* scrollWidget = new QWidget;
    FlowLayout* flow = new FlowLayout(scrollWidget, /*margin*/0, /*hSpacing*/6, /*vSpacing*/6);
    scrollWidget->setLayout(flow);
    scroll->setWidget(scrollWidget);

    dockLayout->addWidget(scroll);

    QWidget* bottomRow1 = new QWidget;
    QHBoxLayout* bottomLayout1 = new QHBoxLayout(bottomRow1);
    bottomLayout1->setContentsMargins(0, 0, 0, 0);
    bottomLayout1->setSpacing(6);

    QLabel* lbl1 = new QLabel(tr("GM Script:"));
    lbl1->setMinimumWidth(64);
    HistoryLineEdit* lineEdit1 = new HistoryLineEdit;
    lineEdit1->setMaxHistory(100);
    QPushButton* addBtn1 = new QPushButton(tr("Execute"));

    bottomLayout1->addWidget(lbl1);
    bottomLayout1->addWidget(lineEdit1);
    bottomLayout1->addWidget(addBtn1);

    dockLayout->addWidget(bottomRow1);

    QWidget* bottomRow2 = new QWidget;
    QHBoxLayout* bottomLayout2 = new QHBoxLayout(bottomRow2);
    bottomLayout2->setContentsMargins(0, 0, 0, 0);
    bottomLayout2->setSpacing(6);

    QLabel* lbl2 = new QLabel(tr("ADB Cmd:"));
    lbl2->setMinimumWidth(64);
    HistoryLineEdit* lineEdit2 = new HistoryLineEdit;
    lineEdit2->setMaxHistory(100);
    QPushButton* addBtn2 = new QPushButton(tr("Execute"));

    bottomLayout2->addWidget(lbl2);
    bottomLayout2->addWidget(lineEdit2);
    bottomLayout2->addWidget(addBtn2);

    dockLayout->addWidget(bottomRow2);

    dockContainer->setLayout(dockLayout);
    dock->setWidget(dockContainer);
    //addDockWidget(Qt::LeftDockWidgetArea, dock);

    m_FlowLayout = flow;

    QObject::connect(lineEdit0, &HistoryLineEdit::submitted, [this, addBtn0, lineEdit0](const QString& text) {
        Q_UNUSED(text);
        if (!lineEdit0->text().trimmed().isEmpty()) {
            addBtn0->click();
            //QMetaObject::invokeMethod(addBtn0, "click", Qt::QueuedConnection);
        }
        });
    QObject::connect(lineEdit1, &HistoryLineEdit::submitted, [this, addBtn1, lineEdit1](const QString& text) {
        Q_UNUSED(text);
        if (!lineEdit1->text().trimmed().isEmpty()) {
            addBtn1->click();
            //QMetaObject::invokeMethod(addBtn1, "click", Qt::QueuedConnection);
        }
        });
    QObject::connect(lineEdit2, &HistoryLineEdit::submitted, [this, addBtn2, lineEdit2](const QString& text) {
        Q_UNUSED(text);
        if (!lineEdit2->text().trimmed().isEmpty()) {
            addBtn2->click();
            //QMetaObject::invokeMethod(addBtn2, "click", Qt::QueuedConnection);
        }
        });

    connect(addBtn0, &QPushButton::clicked, this, [this, lineEdit0]() {
        QString text = lineEdit0->text().trimmed();
        if (!text.isEmpty()) {
            executeDsl(text);
            //lineEdit0->clear();
        }
        });

    connect(addBtn1, &QPushButton::clicked, this, [this, lineEdit1]() {
        QString text = lineEdit1->text().trimmed();
        if (!text.isEmpty()) {
            executeCommand("gmscript", text);
        }
        });

    connect(addBtn2, &QPushButton::clicked, this, [this, lineEdit2]() {
        QString text = lineEdit2->text().trimmed();
        if (!text.isEmpty()) {
            executeCommand("adb", text);
        }
        });

    return dock;
}

QMenu* findOrCreateSubmenu(QMenu* parent, const QString& title) {
    for (QAction* a : parent->actions()) {
        if (a->menu() && a->text() == title) {
            return a->menu();
        }
    }
    return parent->addMenu(title);
}

void MainWindow::addSchemeMenu(const QString& path, const QString& tooltip)
{
    QStringList parts = path.split('/', Qt::SkipEmptyParts);
    if (parts.isEmpty())
        return;

    QMenu* cur = m_SchemesMenu;
    for (int i = 0; i < parts.size(); ++i) {
        const QString seg = parts.at(i);
        if (i < parts.size() - 1) {
            cur = findOrCreateSubmenu(cur, seg);
        }
        else {
            QAction* existingAction = nullptr;
            for (QAction* a : cur->actions()) {
                if (!a->menu() && a->text() == seg) {
                    existingAction = a;
                    break;
                }
            }
            QAction* act = existingAction;
            if (!act) {
                act = cur->addAction(seg);
                QObject::connect(act, &QAction::triggered, [this, path]() {
                    qDebug() << "Action triggered for path:" << path;
                    if (load_scheme_fptr) {
                        m_FlowLayout->clear();
                        ProcessResult result;
                        load_scheme_fptr(path.toStdString().c_str(), &result);
                    }
                    });
                act->setToolTip(tooltip);
            }
        }
    }
}

void MainWindow::addButton(const QString& label, const QString& tooltip, const QString& cmd_type, const QString& cmd_args)
{
    QPushButton* btn = new QPushButton(label);
    // btn->setMinimumWidth(100);
    btn->setToolTip(tooltip);

    m_FlowLayout->addWidget(btn);
    connect(btn, &QPushButton::clicked, this, [this, label, cmd_type, cmd_args]() {
        qDebug() << "Clicked:" << label;
        executeCommand(cmd_type, cmd_args);
    });
}

void MainWindow::addInput(const QString& label, const QString& tooltip, const QString& def_val, const QString& cmd_type, const QString& cmd_args)
{
    QWidget* inputRow = new QWidget;
    QHBoxLayout* inputLayout = new QHBoxLayout(inputRow);
    inputLayout->setContentsMargins(0, 0, 0, 0);
    inputLayout->setSpacing(6);

    HistoryLineEdit* lineEdit = new HistoryLineEdit;
    lineEdit->setMaxHistory(100);
    lineEdit->setToolTip(tooltip);
    lineEdit->setText(def_val);
    QPushButton* addBtn = new QPushButton(label);
    addBtn->setToolTip(tooltip);
    inputLayout->addWidget(lineEdit);
    inputLayout->addWidget(addBtn);

    m_FlowLayout->addWidget(inputRow);

    QObject::connect(lineEdit, &HistoryLineEdit::submitted, [this, lineEdit, addBtn](const QString& text) {
        Q_UNUSED(text);
        if (!lineEdit->text().trimmed().isEmpty()) {
            addBtn->click();
            //QMetaObject::invokeMethod(addBtn, "click", Qt::QueuedConnection);
        }
    });

    connect(addBtn, &QPushButton::clicked, this, [this, lineEdit, cmd_type, cmd_args]() {
        QString text = lineEdit->text().trimmed();
        if (!text.isEmpty()) {
            executeCommand(cmd_type, cmd_args.arg(text));
        }
    });
}

void MainWindow::clearConsole()
{
    m_EditConsole->clear();
}

void MainWindow::showWindowsConsole()
{
#if defined(Q_OS_WIN)
    ShowWindow(GetConsoleWindow(), SW_SHOW);
#endif
    m_ActionViewWindowsConsole->setChecked(true);
}

void MainWindow::hideWindowsConsole()
{
#if defined(Q_OS_WIN)
    ShowWindow(GetConsoleWindow(), SW_HIDE);
#endif
    m_ActionViewWindowsConsole->setChecked(false);
}

QDockWidget* MainWindow::buildConsoleDock()
{
    auto dock = new QDockWidget(tr("Console"), this);
    QFont font;
#ifdef Q_OS_WIN
    font.setFamily("Courier New");
#elif defined(Q_OS_MACOS)
    font.setFamily("Monaco");
#else
    font.setFamily("Ubuntu Mono");
#endif
    font.setFixedPitch(true);
    font.setPointSize(10);
    font.setStyleHint(QFont::Monospace);
    QFontMetrics metrics(font);
    QPalette palette;
    palette.setColor(QPalette::Active, QPalette::Base, QColor("#000000"));
    palette.setColor(QPalette::Inactive, QPalette::Base, QColor("#111111"));
    m_EditConsole = new QTextEdit(this);
    m_EditConsole->setFont(font);
    m_EditConsole->setFrameStyle(QFrame::NoFrame);
    m_EditConsole->setPalette(palette);
    m_EditConsole->setReadOnly(true);
    m_EditConsole->setTabStopDistance(4 * metrics.horizontalAdvance('8'));
    m_EditConsole->setWordWrapMode(QTextOption::NoWrap);
    connect(ProcessOutput::instance(), &ProcessOutput::outputLog, this, &MainWindow::handleOutputLog);
    connect(ProcessOutput::instance(), &ProcessOutput::progress, this, &MainWindow::handleProgress);
    connect(ProcessOutput::instance(), &ProcessOutput::commandFinished, this, &MainWindow::handleCommandFinished);
    connect(ProcessOutput::instance(), &ProcessOutput::commandStarting, this, &MainWindow::handleCommandStarting);
    setContentsMargins(2, 2, 2, 2);
    dock->setObjectName("ConsoleDock");
    dock->setWidget(m_EditConsole);
    return dock;
}

QStatusBar *MainWindow::buildStatusBar(const QMap<QString, QString> &versions)
{
    auto buildSeparator = [=] {
        auto frame = new QFrame(this);
        frame->setFrameStyle(QFrame::VLine);
        frame->setSizePolicy(QSizePolicy::Minimum, QSizePolicy::Expanding);
        return frame;
    };
    auto statusbar = new QStatusBar(this);
    statusbar->addPermanentWidget(new QLabel(tr("Java").append(": ").append(versions["java"]), this));
    statusbar->addPermanentWidget(buildSeparator());
    statusbar->addPermanentWidget(new QLabel(tr("Jadx").append(": ").append(versions["jadx"]), this));
    statusbar->addPermanentWidget(buildSeparator());
    statusbar->addPermanentWidget(new QLabel(tr("ADB").append(": ").append(versions["adb"]), this));
    statusbar->addPermanentWidget(buildSeparator());
    statusbar->addPermanentWidget(new QWidget(this), 1);
    statusbar->addPermanentWidget(m_StatusCursor = new QLabel("0:0", this));
    statusbar->addPermanentWidget(buildSeparator());
    statusbar->addPermanentWidget(m_StatusMessage = new QLabel(tr("Ready!"), this));
    statusbar->setContentsMargins(4, 4, 4, 4);
    statusbar->setStyleSheet("QStatusBar::item { border: none; }");
    return statusbar;
}

void MainWindow::closeEvent(QCloseEvent *event)
{
    Q_UNUSED(event)
    QSettings settings;
    bool maximized = isMaximized();
    settings.setValue("app_maximized", maximized);
    if (!maximized) {
        settings.setValue("app_size", size());
    }
    settings.setValue("dock_state", saveState());
    QStringList folders;
    const int total1 = m_DirectoryTree->topLevelItemCount();
    for (int i = 0; i < total1; ++i) {
        folders << m_DirectoryTree->topLevelItem(i)->data(0, Qt::UserRole + 2).toString();
    }
    settings.setValue("open_folders", QVariant::fromValue(folders));
    QStringList files;
    const int total2 = m_ModelOpenFiles->rowCount();
    for (int i = 0; i < total2; ++i) {
        files << m_ModelOpenFiles->index(i, 0).data(Qt::UserRole + 1).toString();
    }
    settings.setValue("open_files", QVariant::fromValue(files));
    settings.sync();
}

int MainWindow::findTabIndex(const QString& path)
{
    int total = m_TabEditors->count();
    for (int i = 0; i < total; i++) {
        QString path2;
        auto widget = m_TabEditors->widget(i);
        auto edit = dynamic_cast<SourceCodeEdit *>(widget);
        auto hex = dynamic_cast<HexEdit *>(widget);
        auto viewer = dynamic_cast<ImageViewerWidget *>(widget);
        if (edit) {
            path2 = edit->filePath();
        } else if (hex) {
            path2 = hex->filePath();
        } else if (viewer) {
            path2 = viewer->filePath();
        }
        if (QString::compare(path, path2) == 0) {
            return i;
        }
    }
    return -1;
}

void MainWindow::executeDsl(const QString& dsl)
{
    if (execute_dsl_fptr) {
        QString selInTree = selectionInDirectoryTree();
        QString selInList = selectionInFileList();
        ProcessResult result;
        execute_dsl_fptr(dsl.toStdString().c_str(), selInTree.toStdString().c_str(), selInList.toStdString().c_str(), &result);
    }
}

void MainWindow::executeCommand(const QString& cmdType, const QString& cmdArgs)
{
    QString selInTree = selectionInDirectoryTree();
    QString selInList = selectionInFileList();

    auto thread = new QThread();
    auto worker = new CommandWorker(cmdType, cmdArgs, selInTree, selInList);
    worker->moveToThread(thread);
    connect(worker, &CommandWorker::executeFailed, this, &MainWindow::handleExecuteFailed);
    connect(worker, &CommandWorker::executeFinished, this, &MainWindow::handleExecuteFinished);
    connect(thread, &QThread::started, worker, &CommandWorker::execute);
    connect(worker, &CommandWorker::finished, thread, &QThread::quit);
    connect(worker, &CommandWorker::finished, worker, &QObject::deleteLater);
    connect(thread, &QThread::finished, thread, &QObject::deleteLater);
    thread->start();
    /*
    if (nullptr == m_ProgressDialog) {
        m_ProgressDialog = new QProgressDialog(this);
        m_ProgressDialog->setCancelButton(nullptr);
        m_ProgressDialog->setLabelText(tr("Executing command..."));
        m_ProgressDialog->setRange(0, 100);
        m_ProgressDialog->setValue(50);
        m_ProgressDialog->setWindowFlags(m_ProgressDialog->windowFlags() & ~Qt::WindowCloseButtonHint);
        m_ProgressDialog->setWindowTitle(tr("Executing..."));
        m_ProgressDialog->exec();
    }
    */
}

void MainWindow::handleActionAbout()
{
    QMessageBox box;
    box.setIconPixmap(QPixmap(":/images/icon.png").scaledToWidth(128));
    QFile about(":/about.html");
    about.open(QIODevice::ReadOnly | QIODevice::Text);
    QTextStream stream(&about);
    box.setInformativeText(stream.readAll());
    about.close();
    box.setText(QString("<strong>Tag</strong>: %1<br><strong>Commit</strong>: %2").arg(GIT_TAG).arg(GIT_COMMIT_FULL));
    box.setWindowTitle(tr("About"));
    box.exec();
}

void MainWindow::handleActionRefresh()
{
    m_SchemesMenu->clear();
    m_FlowLayout->clear();

    if (load_scheme_menu_fptr) {
        ProcessResult result;
        load_scheme_menu_fptr(&result);
    }
}

void MainWindow::handleActionBuild()
{
    QString selInTree = selectionInDirectoryTree();
    QString selInList = selectionInFileList();
#ifdef QT_DEBUG
    qDebug() << "User wishes to build" << selInTree << selInList;
#endif
    auto thread = new QThread();
    auto worker = new BuildWorker(selInTree, selInList);
    worker->moveToThread(thread);
    connect(worker, &BuildWorker::buildFailed, this, &MainWindow::handleBuildFailed);
    connect(worker, &BuildWorker::buildFinished, this, &MainWindow::handleBuildFinished);
    connect(thread, &QThread::started, worker, &BuildWorker::build);
    connect(worker, &BuildWorker::finished, thread, &QThread::quit);
    connect(worker, &BuildWorker::finished, worker, &QObject::deleteLater);
    connect(thread, &QThread::finished, thread, &QObject::deleteLater);
    thread->start();
    if (nullptr == m_ProgressDialog) {
        m_ProgressDialog = new QProgressDialog(this);
        m_ProgressDialog->setCancelButton(nullptr);
        m_ProgressDialog->setLabelText(tr("Running build..."));
        m_ProgressDialog->setRange(0, 100);
        m_ProgressDialog->setValue(50);
        m_ProgressDialog->setWindowFlags(m_ProgressDialog->windowFlags() & ~Qt::WindowCloseButtonHint);
        m_ProgressDialog->setWindowTitle(tr("Building..."));
        m_ProgressDialog->exec();
    }
}

void MainWindow::handleActionInstall()
{
    QString selInTree = selectionInDirectoryTree();
    QString selInList = selectionInFileList();
#ifdef QT_DEBUG
    qDebug() << "User wishes to install" << selInTree << selInList;
#endif
    auto thread = new QThread();
    auto worker = new InstallWorker(selInTree, selInList);
    worker->moveToThread(thread);
    connect(worker, &InstallWorker::installFailed, this, &MainWindow::handleInstallFailed);
    connect(worker, &InstallWorker::installFinished, this, &MainWindow::handleInstallFinished);
    connect(thread, &QThread::started, worker, &InstallWorker::install);
    connect(worker, &InstallWorker::finished, thread, &QThread::quit);
    connect(worker, &InstallWorker::finished, worker, &QObject::deleteLater);
    connect(thread, &QThread::finished, thread, &QObject::deleteLater);
    thread->start();
    if (nullptr == m_ProgressDialog) {
        m_ProgressDialog = new QProgressDialog(this);
        m_ProgressDialog->setCancelButton(nullptr);
        m_ProgressDialog->setLabelText(tr("Running install..."));
        m_ProgressDialog->setRange(0, 100);
        m_ProgressDialog->setValue(50);
        m_ProgressDialog->setWindowFlags(m_ProgressDialog->windowFlags() & ~Qt::WindowCloseButtonHint);
        m_ProgressDialog->setWindowTitle(tr("Installing..."));
        m_ProgressDialog->exec();
    }
}

void MainWindow::handleActionClose()
{
    int i = m_TabEditors->currentIndex();
    if (i >= 0) {
        handleTabCloseRequested(i);
    }
}

void MainWindow::handleActionCloseAll()
{
    int i = m_TabEditors->count();
    for (int j = --i; j >= 0; j--) {
        handleTabCloseRequested(j);
    }
}

void MainWindow::handleActionContribute()
{
    QDesktopServices::openUrl(QUrl(URL_CONTRIBUTE));
}

void MainWindow::handleActionCopy()
{
    auto edit = dynamic_cast<SourceCodeEdit *>(m_TabEditors->currentWidget());
    edit->copy();
}

void MainWindow::handleActionCut()
{
    auto edit = dynamic_cast<SourceCodeEdit *>(m_TabEditors->currentWidget());
    edit->cut();
}

void MainWindow::handleActionDocumentation()
{
    QDesktopServices::openUrl(QUrl(URL_DOCUMENTATION));
}

void MainWindow::handleActionNew()
{
    const QString path = QFileDialog::getSaveFileName(this,
                                                      tr("New File"),
                                                      QString());
                                                      #ifdef QT_DEBUG
    qDebug() << "User selected to new" << path;
#endif
    if (!path.isEmpty()) {
        QFile file(path);
        file.open(QIODevice::WriteOnly);
        file.close();
        openFile(path);
    }
}

void MainWindow::handleActionFile()
{
    const QString path = QFileDialog::getOpenFileName(this,
                                                      tr("Browse File"),
                                                      QString());
#ifdef QT_DEBUG
    qDebug() << "User selected to open" << path;
#endif
    if (!path.isEmpty()) {
        openFile(path);
    }
}

void MainWindow::handleActionFind()
{
    auto edit = dynamic_cast<SourceCodeEdit *>(m_TabEditors->currentWidget());
    openFindReplaceDialog(edit, false);
}

void MainWindow::handleActionFolder()
{
    QSettings settings;
    const QString folder = settings.value("open_folder").toString();
    const QString path = QFileDialog::getExistingDirectory(this,
                                                      tr("Browse Folder"),
                                                      folder,
                                                      QFileDialog::ShowDirsOnly);
#ifdef QT_DEBUG
    qDebug() << "User selected to open" << path;
#endif
    if (!path.isEmpty()) {
        openFolder(QDir(path).absolutePath());
    }
}

void MainWindow::handleActionGoto()
{
    auto edit = dynamic_cast<SourceCodeEdit *>(m_TabEditors->currentWidget());
    QTextCursor cursor = edit->textCursor();
    const int line = QInputDialog::getInt(this, tr("Go To"), tr("Enter a line number:"), cursor.blockNumber() + 1, 1, edit->document()->lineCount());
    if (line > 0) {
        edit->gotoLine(line);
    }
}

void MainWindow::handleActionPaste()
{
    auto edit = dynamic_cast<SourceCodeEdit *>(m_TabEditors->currentWidget());
    if (edit->canPaste()) {
        edit->paste();
    }
}

void MainWindow::handleActionQuit()
{
    close();
}

void MainWindow::handleActionRedo()
{
    auto edit = dynamic_cast<SourceCodeEdit *>(m_TabEditors->currentWidget());
    edit->redo();
}

void MainWindow::handleActionReplace()
{
    auto edit = dynamic_cast<SourceCodeEdit *>(m_TabEditors->currentWidget());
    openFindReplaceDialog(edit, false);
}

void MainWindow::handleActionReportIssues()
{
    QDesktopServices::openUrl(QUrl(URL_ISSUES));
}

void MainWindow::handleActionSave()
{
    auto i = m_TabEditors->currentIndex();
    if (i >= 0) {
        saveTab(i);
    }
}

void MainWindow::handleActionSaveAll()
{
    int i = m_TabEditors->count();
    for (int j = 0; j < i; j++) {
        saveTab(j);
    }
}

void MainWindow::handleActionSayThanks()
{
    QDesktopServices::openUrl(QUrl(URL_THANKS));
}

void MainWindow::handleActionClearConsole()
{
    clearConsole();
}

void MainWindow::handleActionWindowsConsole(bool checked)
{
    if (checked) {
        showWindowsConsole();
    } else {
        hideWindowsConsole();
    }
}

void MainWindow::handleActionSettings()
{
    (new SettingsDialog(0, this))->exec();
}

void MainWindow::handleActionUndo()
{
    auto edit = dynamic_cast<SourceCodeEdit *>(m_TabEditors->currentWidget());
    edit->undo();
}

void MainWindow::handleClipboardDataChanged()
{
#ifdef QT_DEBUG
    qDebug() << "Something has changed on clipboard.";
#endif
    auto edit = dynamic_cast<SourceCodeEdit *>(m_TabEditors->currentWidget());
    if (edit) {
        m_ActionPaste->setEnabled(edit && edit->canPaste());
    }
}

void MainWindow::handleOutputLog(const QString& text)
{
    m_EditConsole->setTextColor(QColor(COLOR_OUTPUT));
    m_EditConsole->append(text);
}

void MainWindow::handleProgress(const int percent, const QString& message)
{
    m_ProgressDialog->setLabelText(message);
    m_ProgressDialog->setValue(percent);
}

void MainWindow::handleCommandFinished(const ProcessResult &result)
{
    if (!result.error.isEmpty()) {
        m_EditConsole->setTextColor(QColor(COLOR_ERROR));
        foreach (auto line, result.error) {
            m_EditConsole->append(line);
        }
    }
    if (!result.output.isEmpty()) {
        m_EditConsole->setTextColor(QColor(COLOR_OUTPUT));
        foreach (auto line, result.output) {
            m_EditConsole->append(line);
        }
    }
    m_EditConsole->setTextColor(QColor(COLOR_CODE));
    m_EditConsole->append(QString("Process exited with code %1.").arg(result.code));
    m_EditConsole->append(QString());
}

void MainWindow::handleCommandStarting(const QString& exe, const QStringList &args)
{
    QString line = "$ " + exe;
    foreach (auto arg, args) {
        QString argument(arg);
        if (arg.contains(' ')) {
            argument.prepend('"');
            argument.append('"');
        }
        line.append(' ' + arg);
    }
    m_EditConsole->setTextColor(QColor(COLOR_COMMAND));
    m_EditConsole->append(line.trimmed());
}

void MainWindow::handleCursorPositionChanged()
{
    auto edit = dynamic_cast<SourceCodeEdit *>(m_TabEditors->currentWidget());
    if (edit) {
        QTextCursor cursor = edit->textCursor();
        const QString position = QString("%1:%2").arg(cursor.blockNumber() + 1).arg(cursor.positionInBlock() + 1);
        m_StatusCursor->setText(position);
    } else {
        m_StatusCursor->setText("0:0");
    }
}

void MainWindow::handleFilesSelectionChanged(const QItemSelection &selected, const QItemSelection &deselected)
{
    Q_UNUSED(deselected)
    if (!selected.isEmpty()) {
        auto index = selected.indexes().first();
        const QString path = index.data(Qt::UserRole + 1).toString();
        openFolder(QFileInfo(path).dir().absolutePath(), true);
        m_TabEditors->setCurrentIndex(findTabIndex(path));
    }
}

void MainWindow::handleExecuteFailed(const QString& cmdType, const QString& cmdArgs, const QString& selInTree, const QString& selInList)
{
    Q_UNUSED(cmdType)
    Q_UNUSED(cmdArgs)
    Q_UNUSED(selInTree)
    Q_UNUSED(selInList)
    if (m_ProgressDialog) {
        m_ProgressDialog->close();
        m_ProgressDialog->deleteLater();
        m_ProgressDialog = nullptr;
    }
    m_StatusMessage->setText(tr("Command execute failed."));
}

void MainWindow::handleExecuteFinished(const QString& cmdType, const QString& cmdArgs, const QString& selInTree, const QString& selInList)
{
    Q_UNUSED(cmdType)
    Q_UNUSED(cmdArgs)
    Q_UNUSED(selInTree)
    Q_UNUSED(selInList)
    if (m_ProgressDialog) {
        m_ProgressDialog->close();
        m_ProgressDialog->deleteLater();
        m_ProgressDialog = nullptr;
    }
    m_StatusMessage->setText(tr("Command execute finished."));
}

void MainWindow::handleBuildFailed(const QString& selInTree, const QString& selInList)
{
    Q_UNUSED(selInTree)
    Q_UNUSED(selInList)
    if (m_ProgressDialog) {
        m_ProgressDialog->close();
        m_ProgressDialog->deleteLater();
        m_ProgressDialog = nullptr;
    }
    m_StatusMessage->setText(tr("Build failed."));
}

void MainWindow::handleBuildFinished(const QString& selInTree, const QString& selInList)
{
    Q_UNUSED(selInTree)
    Q_UNUSED(selInList)
    if (m_ProgressDialog) {
        m_ProgressDialog->close();
        m_ProgressDialog->deleteLater();
        m_ProgressDialog = nullptr;
    }
    m_StatusMessage->setText(tr("Build finished."));
}

void MainWindow::handleInstallFailed(const QString& selInTree, const QString& selInList)
{
    Q_UNUSED(selInTree)
    Q_UNUSED(selInList)
    if (m_ProgressDialog) {
        m_ProgressDialog->close();
        m_ProgressDialog->deleteLater();
        m_ProgressDialog = nullptr;
    }
    m_StatusMessage->setText(tr("Installation failed."));
}

void MainWindow::handleInstallFinished(const QString& selInTree, const QString& selInList)
{
    Q_UNUSED(selInTree)
    Q_UNUSED(selInList)
    if (m_ProgressDialog) {
        m_ProgressDialog->close();
        m_ProgressDialog->deleteLater();
        m_ProgressDialog = nullptr;
    }
    m_StatusMessage->setText(tr("Installation finished."));
}

void MainWindow::handleTabChanged(const int index)
{
#ifdef QT_DEBUG
    qDebug() << "User changed current tab" << index;
#endif
    QString path;
    auto widget = m_TabEditors->currentWidget();
    auto edit = dynamic_cast<SourceCodeEdit *>(widget);
    auto hex = dynamic_cast<HexEdit *>(widget);
    auto viewer = dynamic_cast<ImageViewerWidget *>(widget);
    if (edit) {
        path = edit->filePath();
    } else if (hex) {
        path = hex->filePath();
    } else if (viewer) {
        path = viewer->filePath();
    }
    const int total = m_ModelOpenFiles->rowCount();
    for (int i = 0; i < total; ++i) {
        const QModelIndex &mindex = m_ModelOpenFiles->index(i, 0);
        if (QString::compare(mindex.data(Qt::UserRole + 1).toString(), path) == 0) {
            m_ListOpenFiles->setCurrentIndex(mindex);
            break;
        }
    }
    m_ActionClose->setEnabled(index >= 0);
    m_ActionCloseAll->setEnabled(index >= 0);
    m_ActionCopy->setEnabled(false);
    m_ActionCut->setEnabled(false);
    m_ActionPaste->setEnabled(false);
    m_ActionRedo->setEnabled(false);
    m_ActionUndo->setEnabled(false);
    m_ActionFind->setEnabled(edit);
    m_ActionReplace->setEnabled(edit);
    m_ActionSave->setEnabled(edit || hex);
    m_ActionSaveAll->setEnabled(edit || hex);
    m_ActionGoto->setEnabled(edit);
    for (auto conn: m_EditorConnections) {
        disconnect(conn);
    }
    m_EditorConnections.clear();
    if (edit) {
        m_EditorConnections << connect(edit, &QPlainTextEdit::copyAvailable, m_ActionCopy, &QAction::setEnabled);
        m_EditorConnections << connect(edit, &QPlainTextEdit::copyAvailable, m_ActionCut, &QAction::setEnabled);
        m_EditorConnections << connect(edit, &QPlainTextEdit::redoAvailable, m_ActionRedo, &QAction::setEnabled);
        m_EditorConnections << connect(edit, &QPlainTextEdit::undoAvailable, m_ActionUndo, &QAction::setEnabled);
        m_EditorConnections << connect(edit, &QPlainTextEdit::cursorPositionChanged, this, &MainWindow::handleCursorPositionChanged);
        bool selected = !edit->textCursor().selection().isEmpty();
        m_ActionCut->setEnabled(selected);
        m_ActionCopy->setEnabled(selected);
        m_ActionPaste->setEnabled(edit->canPaste());
        m_ActionRedo->setEnabled(edit->document()->isRedoAvailable());
        m_ActionUndo->setEnabled(edit->document()->isUndoAvailable());
        if (m_FindReplaceDialog) {
            m_FindReplaceDialog->setTextEdit(edit);
        }
    }
    handleCursorPositionChanged();
}

void MainWindow::handleTabCloseRequested(const int index)
{
#ifdef QT_DEBUG
    qDebug() << "User requested to close tab" << index;
#endif
    QString path;
    auto widget = m_TabEditors->widget(index);
    auto edit = dynamic_cast<SourceCodeEdit *>(widget);
    auto hex = dynamic_cast<HexEdit *>(widget);
    auto viewer = dynamic_cast<ImageViewerWidget *>(widget);
    if (edit) {
        path = edit->filePath();
    } else if (hex) {
        path = hex->filePath();
    } else if (viewer) {
        path = viewer->filePath();
    }
    const int total = m_ModelOpenFiles->rowCount();
    for (int i = 0; i < total; ++i) {
        const QModelIndex &mindex = m_ModelOpenFiles->index(i, 0);
        if (QString::compare(mindex.data(Qt::UserRole + 1).toString(), path) == 0) {
            m_ModelOpenFiles->removeRow(mindex.row());
            break;
        }
    }
    m_ActionUndo->setEnabled(false);
    m_ActionRedo->setEnabled(false);
    m_ActionCut->setEnabled(false);
    m_ActionCopy->setEnabled(false);
    m_ActionPaste->setEnabled(false);
    m_TabEditors->removeTab(index);
    if (m_TabEditors->count() == 0) {
        m_CentralStack->setCurrentIndex(0);
    }
}

void MainWindow::handleTreeContextMenu(const QPoint &point)
{
    QMenu menu(this);
    auto item = m_DirectoryTree->itemAt(point);
    if (item) {
        const int type = item->data(0, Qt::UserRole + 1).toInt();
        const QString path = item->data(0, Qt::UserRole + 2).toString();
#ifdef QT_DEBUG
        qDebug() << "Context menu requested for" << item->text(0) << "at" << point;
#endif
        if (type == File) {
            auto open = menu.addAction(tr("Open"));
            connect(open, &QAction::triggered, [=] {
                openFile(path);
            });
            auto open1 = menu.addAction(tr("Open As Text"));
            connect(open1, &QAction::triggered, [=] {
                openFile(path, true, false);
            });
            auto open2 = menu.addAction(tr("Open As Hex"));
            connect(open2, &QAction::triggered, [=] {
                openFile(path, false, true);
            });
        }
#ifdef Q_OS_WIN
        auto openin = menu.addAction(tr("Open in Explorer"));
        connect(openin, &QAction::triggered, [=] {
            QStringList args;
            if (type == File) {
                args << QLatin1String("/select,");
            }
            args << QDir::toNativeSeparators(path);
            QProcess::startDetached("explorer.exe", args);
        });
#elif defined(Q_OS_MACOS)
        auto openin = menu.addAction(tr("Open in Finder"));
        connect(openin, &QAction::triggered, [=] {
            QStringList args;
            args << "-e" << QString("tell application \"Finder\" to reveal POSIX file \"%1\"").arg(path);
            QProcess::execute("/usr/bin/osascript", args);
            args.clear();
            args << "-e" << "tell application \"Finder\" to activate";
            QProcess::execute("/usr/bin/osascript", args);
        });
#else
        auto openin = menu.addAction(tr("Open in Files"));
        connect(openin, &QAction::triggered, [=] {
            QProcess::startDetached("xdg-open", QStringList() << path);
        });
#endif
        menu.addSeparator();
        auto build = menu.addAction(tr("Build"));
        connect(build, &QAction::triggered, this, &MainWindow::handleActionBuild);
        menu.addSeparator();
        auto install = menu.addAction(tr("Install"));
        connect(install, &QAction::triggered, this, &MainWindow::handleActionInstall);
        menu.addSeparator();
        if (type != File) {
            auto refresh = menu.addAction(tr("Refresh"));
            connect(refresh, &QAction::triggered, [=] {
                reloadChildren(item, 2);
            });
        }
    } else {
        auto folder = menu.addAction(tr("Open Folder"));
        connect(folder, &QAction::triggered, this, &MainWindow::handleActionFolder);
        menu.addSeparator();
    }
    auto collapse = menu.addAction(tr("Collapse All"));
    if (m_DirectoryTree->topLevelItemCount() == 0) {
        collapse->setEnabled(false);
    } else {
        connect(collapse, &QAction::triggered, m_DirectoryTree, &QTreeWidget::collapseAll);
    }
    auto closeAll = menu.addAction(tr("Close All"));
    if (m_DirectoryTree->topLevelItemCount() == 0) {
        closeAll->setEnabled(false);
    }
    else {
        connect(closeAll, &QAction::triggered, m_DirectoryTree, &QTreeWidget::clear);
    }
    menu.exec(m_DirectoryTree->mapToGlobal(point));
}

void MainWindow::handleListViewContextMenu(const QPoint& point)
{
    QMenu menu(this);
    auto newFile = menu.addAction(tr("New File"));
    connect(newFile, &QAction::triggered, this, &MainWindow::handleActionNew);
    auto openFile = menu.addAction(tr("Open File"));
    connect(openFile, &QAction::triggered, this, &MainWindow::handleActionFile);
    QModelIndex index = m_ListOpenFiles->indexAt(point);
    if (index.isValid()) {
        menu.addSeparator();
        QString path = index.data(Qt::UserRole + 1).toString();
        QString folder = QFileInfo(path).dir().absolutePath();
        auto openf = menu.addAction(tr("Open Folder"));
        connect(openf, &QAction::triggered, this, [this, folder]() { openFolder(folder); });
    }
    menu.addSeparator();
    auto closeAll = menu.addAction(tr("Close All"));
    connect(closeAll, &QAction::triggered, this, &MainWindow::handleActionCloseAll);
    menu.exec(m_ListOpenFiles->mapToGlobal(point));
}


void MainWindow::handleTreeDoubleClicked(const QModelIndex &index)
{
    const int type = index.data(Qt::UserRole + 1).toInt();
    const QString path = index.data(Qt::UserRole + 2).toString();
#ifdef QT_DEBUG
    qDebug() << "User double clicked" << path;
#endif
    switch (type) {
    case Root:
    case Folder:
        break;
    case File:
        openFile(path);
        break;
    }
}

void MainWindow::handleTreeSelectionChanged(const QItemSelection &selected, const QItemSelection &deselected)
{
    Q_UNUSED(deselected)
    if (!selected.isEmpty()) {
        auto index = selected.indexes().first();
        const int type = index.data(Qt::UserRole + 1).toInt();
        const QString path = index.data(Qt::UserRole + 2).toString();
    }
    bool checked = !selected.isEmpty();
    m_ActionBuild1->setEnabled(checked);
    m_ActionBuild2->setEnabled(checked);
    m_ActionInstall1->setEnabled(checked);
    m_ActionInstall2->setEnabled(checked);
}

void MainWindow::openFile(const QString& path, bool forceTxt, bool forceHex, const bool fromFolder)
{
    if (!fromFolder) {
        openFolder(QFileInfo(path).dir().absolutePath());
    }
#ifdef QT_DEBUG
    qDebug() << "Opening" << path;
#endif
    int type = 0;
    if (forceTxt) {
        type = 1;
    }
    else if (forceHex) {
        type = 2;
    }
    const int total = m_ModelOpenFiles->rowCount();
    for (int i = 0; i < total; ++i) {
        const QModelIndex &index = m_ModelOpenFiles->index(i, 0);
        if (QString::compare(index.data(Qt::UserRole + 1).toString(), path) == 0 && index.data(Qt::UserRole + 2).toInt() == type) {
            m_TabEditors->setCurrentIndex(findTabIndex(path));
            return;
        }
    }
    QFileInfo info(path);
    QWidget *widget;
    const QString extension = info.suffix();
    if (!forceTxt && !forceHex && !extension.isEmpty() && QString(IMAGE_EXTENSIONS).contains(extension, Qt::CaseInsensitive)) {
        auto viewer = new ImageViewerWidget(this);
        viewer->open(path);
        viewer->zoomReset();
        widget = viewer;
    } else if (forceTxt || (!forceHex && !extension.isEmpty() && QString(TEXT_EXTENSIONS).contains(extension, Qt::CaseInsensitive))) {
        auto editor = new SourceCodeEdit(this);
        editor->open(path);
        widget = editor;
    } else {
        auto hex = new HexEdit(this);
        hex->open(path);
        widget = hex;
    }
    const QIcon icon = m_FileIconProvider.icon(info);
    auto item = new QStandardItem(icon, info.fileName());
    item->setData(path, Qt::UserRole + 1);
    item->setData(type, Qt::UserRole + 2);
    m_ModelOpenFiles->appendRow(item);
    const int i = m_TabEditors->addTab(widget, icon, info.fileName());
    m_TabEditors->setCurrentIndex(i);
    m_TabEditors->setTabToolTip(i, path);
    if (m_CentralStack->currentIndex() != 1) {
        m_CentralStack->setCurrentIndex(1);
    }
    m_ActionClose->setEnabled(true);
    m_ActionCloseAll->setEnabled(true);
}

void MainWindow::openFindReplaceDialog(QPlainTextEdit *edit, const bool replace)
{
    if (!m_FindReplaceDialog) {
        m_FindReplaceDialog = new FindReplaceDialog(replace, this);
        connect(m_FindReplaceDialog, &QDialog::finished, [=] {
            m_FindReplaceDialog->deleteLater();
            m_FindReplaceDialog = nullptr;
        });
        m_FindReplaceDialog->show();
    }
    m_FindReplaceDialog->setTextEdit(edit);
}

void MainWindow::openFolder(const QString& folder, const bool fromFile)
{
    QSettings settings;
    settings.setValue("open_folder", folder);
    settings.sync();
    QTreeWidgetItem *item = nullptr;
    int ct = m_DirectoryTree->topLevelItemCount();
    for (int i = 0; i < ct; ++i) {
        QTreeWidgetItem *fitem = m_DirectoryTree->topLevelItem(i);
        if (QString::compare(fitem->data(0, Qt::UserRole + 2).toString(), folder) == 0) {
            item = fitem;
            break;
        }
    }
    if (!item) {
        QFileInfo info(folder);
        item = new QTreeWidgetItem(m_DirectoryTree);
        item->setData(0, Qt::UserRole + 1, Root);
        item->setData(0, Qt::UserRole + 2, folder);
        item->setIcon(0, m_FileIconProvider.icon(info));
        item->setText(0, info.fileName());
        reloadChildren(item, 2);
    }
    m_DirectoryTree->collapseAll();
    m_DirectoryTree->addTopLevelItem(item);
    m_DirectoryTree->expandItem(item);
    QDir dir(folder);
    if (!fromFile) {
        const QString hookScp = dir.filePath("hook.txt");
        if (QFile::exists(hookScp)) {
            openFile(hookScp, false, false, true);
        }
    }
}

void MainWindow::reloadChildren(QTreeWidgetItem *item, int maxLevel, int curLevel)
{
    while (item->childCount()) {
        qDeleteAll(item->takeChildren());
    }
    QDir dir(item->data(0, Qt::UserRole + 2).toString());
    if (dir.exists()) {
        QFileInfoList files = dir.entryInfoList(QDir::AllEntries | QDir::NoDotAndDotDot, QDir::DirsFirst);
        foreach (auto info, files) {
            QTreeWidgetItem *child = new QTreeWidgetItem(item);
            child->setData(0, Qt::UserRole + 1, info.isDir() ? Folder : File);
            child->setData(0, Qt::UserRole + 2, info.absoluteFilePath());
            child->setIcon(0, m_FileIconProvider.icon(info));
            child->setText(0, info.fileName());
            const QString tooltip = QString("%1 - %2")
                    .arg(QDir::toNativeSeparators(info.filePath()))
                    .arg(QLocale::system().formattedDataSize(info.size(), 2, QLocale::DataSizeTraditionalFormat));
            child->setToolTip(0, tooltip);
            item->addChild(child);
            if (info.isDir() && curLevel < maxLevel) {
                reloadChildren(child, maxLevel, curLevel + 1);
            }
        }
    }
}

void MainWindow::handleActionRefreshLayout()
{
    refreshDockWidgets(100);
}

QString MainWindow::selectionInDirectoryTree()const
{
    auto active = m_DirectoryTree->currentItem();
    if (!active) {
        active = m_DirectoryTree->topLevelItem(0);
    }
    QString folder;
    if (active) {
        folder = active->data(0, Qt::UserRole + 2).toString();
    }
    return folder;
}

QString MainWindow::selectionInFileList()const
{
    QString file;
    auto index = m_TabEditors->currentIndex();
    if (index >= 0) {
        auto widget = m_TabEditors->widget(index);
        auto edit = dynamic_cast<SourceCodeEdit*>(widget);
        auto hex = dynamic_cast<HexEdit*>(widget);
        auto viewer = dynamic_cast<ImageViewerWidget*>(widget);
        if (edit) {
            file = edit->filePath();
        }
        else if (hex) {
            file = hex->filePath();
        }
        else if (viewer) {
            file = viewer->filePath();
        }
    }
    return file;
}

void MainWindow::refreshDockWidgets(int delay)
{
    QTimer::singleShot(delay, [=] {
        QCoreApplication::processEvents();
        m_DockButtons->setVisible(true);
        m_DockButtons->setFloating(false);
        m_DockButtons->setDockLocation(Qt::BottomDockWidgetArea);
        m_DockConsole->setVisible(true);
        m_DockConsole->setFloating(false);
        m_DockConsole->setDockLocation(Qt::BottomDockWidgetArea);
        auto&& tabs = tabifiedDockWidgets(m_DockButtons);
        if (tabs.size() > 0) {
            for (auto&& tab : tabs) {
                splitDockWidget(m_DockButtons, tab, Qt::Vertical);
            }
        }
        splitDockWidget(m_DockButtons, m_DockConsole, Qt::Vertical);

        if (m_DockFolder->isVisible() || m_DockFiles->isVisible()) {
            m_DockFolder->setFloating(false);
            m_DockFiles->setFloating(false);

            auto&& tabs = tabifiedDockWidgets(m_DockFolder);
            if (tabs.size() > 0) {
                for (auto&& tab : tabs) {
                    splitDockWidget(m_DockFolder, tab, Qt::Vertical);
                }
            }
            splitDockWidget(m_DockFolder, m_DockFiles, Qt::Vertical);
        }
    });
}

bool MainWindow::saveTab(int i)
{
    auto widget = m_TabEditors->widget(i);
    auto edit = dynamic_cast<SourceCodeEdit *>(widget);
    if (edit) {
        return edit->save();
    } else {
        auto hex = dynamic_cast<HexEdit *>(widget);
        if (hex) {
            return hex->save();
        }
    }
    return true;
}

MainWindow::~MainWindow()
{
    s_pInstance = nullptr;
}

MainWindow* MainWindow::instance()
{
    return s_pInstance;
}