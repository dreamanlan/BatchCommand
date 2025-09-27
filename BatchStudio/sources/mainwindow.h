#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QDockWidget>
#include <QFileIconProvider>
#include <QLabel>
#include <QListView>
#include <QMainWindow>
#include <QMap>
#include <QProgressDialog>
#include <QStackedWidget>
#include <QStandardItemModel>
#include <QTextEdit>
#include <QTreeWidget>
#include "findreplacedialog.h"
#include "processutils.h"

class QToolBar;
class QToolButton;
class QMenu;
class FlowLayout;
class HistoryLineEdit;
class MainWindow : public QMainWindow
{
    Q_OBJECT
public:
    enum TreeItemType {
        Root = 0,
        Folder,
        File
    };
    explicit MainWindow(const QMap<QString, QString> &versions, QWidget *parent = nullptr);
    ~MainWindow();
public:
    void addSchemeMenu(const QString& path, const QString& tooltip);
    void addButton(const QString& label, const QString& tooltip, const QString& cmd_type, const QString& cmd_args);
    void addInput(const QString& label, const QString& tooltip, const QString& def_val, const QString& cmd_type, const QString& cmd_args);
    void clearConsole();
    void showWindowsConsole();
    void hideWindowsConsole();
protected:
    void closeEvent(QCloseEvent *event);
private:
    QAction *m_ActionBuild1;
    QAction *m_ActionBuild2;
    QAction *m_ActionClose;
    QAction *m_ActionCloseAll;
    QAction *m_ActionCopy;
    QAction *m_ActionCut;
    QAction *m_ActionFind;
    QAction *m_ActionGoto;
    QAction *m_ActionInstall1;
    QAction *m_ActionInstall2;
    QAction *m_ActionPaste;
    QAction *m_ActionRedo;
    QAction *m_ActionReplace;
    QAction *m_ActionSave;
    QAction *m_ActionSaveAll;
    QAction *m_ActionUndo;
    QAction *m_ActionViewFolder;
    QAction *m_ActionViewFiles;
    QAction *m_ActionViewButtons;
    QAction *m_ActionViewConsole;
    QAction *m_ActionViewToolBar;
    QAction *m_ActionViewWindowsConsole;
    QStackedWidget *m_CentralStack;
    QToolBar *m_MainToolBar;
    QDockWidget *m_DockFolder;
    QDockWidget *m_DockFiles;
    QDockWidget *m_DockButtons;
    QDockWidget *m_DockConsole;
    QToolButton *m_SchemesButton;
    QMenu *m_SchemesMenu;
    FlowLayout *m_FlowLayout;
    QTextEdit *m_EditConsole;
    QList<QMetaObject::Connection> m_EditorConnections;
    QFileIconProvider m_FileIconProvider;
    FindReplaceDialog *m_FindReplaceDialog;
    QListView *m_ListOpenFiles;
    QStandardItemModel *m_ModelOpenFiles;
    QProgressDialog *m_ProgressDialog;
    QTreeWidget *m_DirectoryTree;
    QLabel *m_StatusCursor;
    QLabel *m_StatusMessage;
    QTabWidget *m_TabEditors;
    QToolBar *buildMainToolBar();
    QMenuBar *buildMenuBar();
    QDockWidget *buildFolderDock();
    QDockWidget *buildFilesDock();
    QWidget *buildCentralWidget();
    QDockWidget *buildButtonsDock();
    QDockWidget *buildConsoleDock();
    QStatusBar *buildStatusBar(const QMap<QString, QString> &versions);
    int findTabIndex(const QString& path);
    void executeDsl(const QString& dsl);
    void executeCommand(const QString& cmdType, const QString& cmdArgs);
private slots:
    void handleActionAbout();
    void handleActionRefresh();
    void handleActionBuild();
    void handleActionInstall();
    void handleActionClose();
    void handleActionCloseAll();
    void handleActionContribute();
    void handleActionCopy();
    void handleActionCut();
    void handleActionDocumentation();
    void handleActionNew();
    void handleActionFile();
    void handleActionFind();
    void handleActionFolder();
    void handleActionGoto();
    void handleActionPaste();
    void handleActionQuit();
    void handleActionRedo();
    void handleActionReplace();
    void handleActionReportIssues();
    void handleActionSave();
    void handleActionSaveAll();
    void handleActionSayThanks();
    void handleActionClearConsole();
    void handleActionWindowsConsole(bool checked);
    void handleActionSettings();
    void handleActionUndo();
    void handleActionRefreshLayout();
    void handleClipboardDataChanged();
    void handleOutputLog(const QString& text);
    void handleProgress(const int percent, const QString& message);
    void handleCommandFinished(const ProcessResult &result);
    void handleCommandStarting(const QString& exe, const QStringList &args);
    void handleCursorPositionChanged();
    void handleFilesSelectionChanged(const QItemSelection &selected, const QItemSelection &deselected);
    void handleExecuteFailed(const QString& cmdType, const QString& cmdArgs, const QString& selInTree, const QString& selInList);
    void handleExecuteFinished(const QString& cmdType, const QString& cmdArgs, const QString& selInTree, const QString& selInListnList);
    void handleBuildFailed(const QString& selInTree, const QString& selInList);
    void handleBuildFinished(const QString& selInTree, const QString& selInList);
    void handleInstallFailed(const QString& selInTree, const QString& selInList);
    void handleInstallFinished(const QString& selInTree, const QString& selInList);
    void handleTabChanged(const int index);
    void handleTabCloseRequested(const int index);
    void handleTreeContextMenu(const QPoint &point);
    void handleListViewContextMenu(const QPoint &point);
    void handleTreeDoubleClicked(const QModelIndex &index);
    void handleTreeSelectionChanged(const QItemSelection &selected, const QItemSelection &deselected);
private:
    void openFile(const QString& file) { openFile(file, false, false, false); }
    void openFile(const QString& file, bool forceTxt, bool forceHex) { openFile(file, forceTxt, forceHex, false); }
    void openFile(const QString& file, bool forceTxt, bool forceHex, const bool fromFolder);
    void openFindReplaceDialog(QPlainTextEdit* edit, const bool replace);
    void openFolder(const QString& folder) { openFolder(folder, false); }
    void openFolder(const QString& folder, const bool fromFile);
    void reloadChildren(QTreeWidgetItem* item, int maxLevel) { reloadChildren(item, maxLevel, 0); }
    void reloadChildren(QTreeWidgetItem* item, int maxLevel, int curLevel);
    QString selectionInDirectoryTree()const;
    QString selectionInFileList()const;
    void refreshDockWidgets(int delay);
    bool saveTab(int index);
public:
    static MainWindow* instance();
private:
    static MainWindow* s_pInstance;
};

Q_DECLARE_METATYPE(MainWindow::TreeItemType);

#endif // MAINWINDOW_H
