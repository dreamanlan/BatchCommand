// ============================================================================
// ProjectPanel - Project configuration panel
// ============================================================================
class ProjectPanel {
  static STORAGE_KEY = 'project_panel_configs';

  constructor(bridge) {
    this.bridge = bridge;
    this.visible = false;
    this.panel = null;
    // config fields
    this.cfgProjectDir = null;
    this.cfgProjectIdentity = null;
    this.projectList = null;
    this.cfgProjectUrl = null;
    // loaded data: { projects: [ {projectDir, projectIdentity, lastUsed} ], currentIndex: -1 }
    this.data = { projects: [], currentIndex: -1 };
    this.onVisibilityChange = null;
    this.createPanel();
  }

  _inputStyle() {
    return `
        flex: 1;
        background: #1e1e1e;
        color: #d4d4d4;
        border: 1px solid #444;
        border-radius: 4px;
        padding: 5px 8px;
        font-size: 12px;
        outline: none;
        min-width: 0;
      `;
  }

  _makeInput(type, placeholder, value) {
    const el = document.createElement('input');
    el.type = type;
    el.placeholder = placeholder;
    el.value = value || '';
    el.style.cssText = this._inputStyle();
    return el;
  }

  _makeLabel(text) {
    const el = document.createElement('span');
    el.textContent = text;
    el.style.cssText = 'color:#aaa;font-size:11px;white-space:nowrap;min-width:70px;';
    return el;
  }

  _makeRow() {
    const row = document.createElement('div');
    row.style.cssText = 'display:flex;gap:6px;align-items:center;';
    return row;
  }

  _makeBtn(text, bg, onClick) {
    const btn = document.createElement('button');
    btn.textContent = text;
    btn.style.cssText = `
        padding: 4px 12px;
        background: ${bg};
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
        white-space: nowrap;
      `;
    btn.onclick = onClick;
    return btn;
  }

  createPanel() {
    this.panel = document.createElement('div');
    this.panel.id = 'project-panel';
    this.panel.style.cssText = `
        position: fixed;
        left: 20px;
        bottom: 120px;
        width: 420px;
        background: #2d2d2d;
        border: 1px solid #555;
        border-radius: 8px;
        display: none;
        flex-direction: column;
        z-index: 10003;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Arial, sans-serif;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
        padding: 0;
        gap: 0;
        max-height: 500px;
      `;

    // Prevent events from bubbling out
    ['input', 'change', 'paste', 'cut', 'keydown', 'keyup', 'keypress', 'beforeinput'].forEach(t => {
      this.panel.addEventListener(t, (e) => { e.stopPropagation(); }, false);
    });

    // ---- Header (draggable) ----
    const header = document.createElement('div');
    header.style.cssText = `
        padding: 8px 12px;
        background: #1a1a1a;
        border-bottom: 1px solid #444;
        border-radius: 8px 8px 0 0;
        display: flex;
        justify-content: space-between;
        align-items: center;
        cursor: move;
      `;
    const title = document.createElement('span');
    title.textContent = 'Project Config';
    title.style.cssText = 'color:#fff;font-weight:600;font-size:13px;';
    const closeBtn = document.createElement('button');
    closeBtn.innerHTML = '&times;';
    closeBtn.style.cssText = 'background:none;border:none;color:#999;font-size:18px;cursor:pointer;padding:0 4px;';
    closeBtn.onclick = () => this.hide();
    header.appendChild(title);
    header.appendChild(closeBtn);
    this.panel.appendChild(header);

    // ---- Config area ----
    const configArea = document.createElement('div');
    configArea.style.cssText = 'display:flex;flex-direction:column;gap:5px;padding:8px 10px;border-bottom:1px solid #444;';

    // ProjectDir
    const r1 = this._makeRow();
    this.cfgProjectDir = this._makeInput('text', 'D:/MyProject', '');
    const browseBtn = this._makeBtn('Browse', '#555', () => this._browseDirectory());
    // Hidden file input for picking a file to extract its directory path
    this._dirInput = document.createElement('input');
    this._dirInput.type = 'file';
    this._dirInput.style.display = 'none';
    this._dirInput.addEventListener('change', (e) => {
      const filePath = e.target.value;
      if (filePath) {
        // Extract directory from file path (handle both \ and /)
        // e.target.value may be "C:\fakepath\file" in browsers or full path in CEF
        if (filePath.indexOf('fakepath') < 0) {
          const sep = filePath.lastIndexOf('\\') >= 0 ? '\\' : '/';
          const dirPath = filePath.substring(0, filePath.lastIndexOf(sep));
          if (dirPath) {
            this.cfgProjectDir.value = dirPath;
          }
        }
      }
      // Reset so the same file can be re-selected
      this._dirInput.value = '';
    });
    r1.appendChild(this._makeLabel('ProjectDir:'));
    r1.appendChild(this.cfgProjectDir);
    r1.appendChild(browseBtn);
    r1.appendChild(this._dirInput);
    configArea.appendChild(r1);

    // ProjectIdentity
    const r2 = this._makeRow();
    this.cfgProjectIdentity = this._makeInput('text', 'myproject', '');
    r2.appendChild(this._makeLabel('Identity:'));
    r2.appendChild(this.cfgProjectIdentity);
    configArea.appendChild(r2);

    // ProjectUrl
    const rUrl = this._makeRow();
    this.cfgProjectUrl = this._makeInput('text', 'http://localhost:8082', '');
    rUrl.appendChild(this._makeLabel('ProjectUrl:'));
    rUrl.appendChild(this.cfgProjectUrl);
    configArea.appendChild(rUrl);

    // Warning banner (shown when no current project is configured)
    this.warningBanner = document.createElement('div');
    this.warningBanner.style.cssText = `
        display: none;
        background: #5c1a1a;
        color: #ff6b6b;
        border: 1px solid #ff4444;
        border-radius: 4px;
        padding: 6px 10px;
        font-size: 11px;
        font-weight: 600;
        text-align: center;
      `;
    this.warningBanner.textContent = '⚠ No active project. Please configure and save a project to work properly.';
    configArea.appendChild(this.warningBanner);

    // Buttons row
    const r3 = this._makeRow();
    r3.appendChild(this._makeBtn('Save', '#4caf50', () => this._saveCurrentConfig()));
    r3.appendChild(this._makeBtn('Delete', '#f44336', () => this._deleteSelected()));
    configArea.appendChild(r3);

    this.panel.appendChild(configArea);

    // ---- Project list area ----
    const listHeader = document.createElement('div');
    listHeader.style.cssText = 'padding:6px 10px;color:#aaa;font-size:11px;border-bottom:1px solid #333;background:#252525;';
    listHeader.textContent = 'Saved Projects:';
    this.panel.appendChild(listHeader);

    this.projectList = document.createElement('div');
    this.projectList.style.cssText = `
        flex: 1;
        overflow-y: auto;
        padding: 4px 0;
        min-height: 60px;
        max-height: 300px;
        background: #1e1e1e;
        border-radius: 0 0 8px 8px;
      `;
    this.panel.appendChild(this.projectList);

    document.body.appendChild(this.panel);
    this._initDrag(header);
  }

  // ---- Config persistence (localStorage) ----

  async _restoreConfig() {
    // Load localStorage data
    try {
      const raw = localStorage.getItem(ProjectPanel.STORAGE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw);
        if (parsed && Array.isArray(parsed.projects)) {
          this.data = parsed;
        }
      }
    } catch (_) { }

    // Query C# for initial project identity (from command line)
    let csharpIdentity = '';
    try {
      const result = await this._queryInitialProjectIdentity();
      if (result) {
        csharpIdentity = result.projectIdentity || '';
      }
    } catch (_) { }

    // If command line specified a projectIdentity, try to locate it in history
    if (csharpIdentity) {
      const idx = this.data.projects.findIndex(p => p.projectIdentity === csharpIdentity);
      if (idx >= 0) {
        // Found matching project in history, set as current
        this.data.currentIndex = idx;
      } else {
        // Command line identity not in history, clear current selection
        this.data.currentIndex = -1;
        // Fill the identity input with the command line value for user convenience
        this.cfgProjectIdentity.value = csharpIdentity;
      }
      this._persistData();
    }

    // Apply current config to input fields
    if (this.data.currentIndex >= 0 && this.data.currentIndex < this.data.projects.length) {
      const cur = this.data.projects[this.data.currentIndex];
      this.cfgProjectDir.value = cur.projectDir || '';
      this.cfgProjectIdentity.value = cur.projectIdentity || '';
      this.cfgProjectUrl.value = cur.projectUrl || '';
    }
    this._renderList();
    this._updateWarning();

    // Notify C# on initial load
    this._notifyDsl();
  }

  // Query C# for initial project identity via bridge command
  _queryInitialProjectIdentity() {
    return new Promise((resolve) => {
      if (!this.bridge) {
        resolve(null);
        return;
      }
      this.bridge.sendCommand('get_initial_project_identity', {}, (success, data, error) => {
        if (success && data) {
          try {
            const parsed = typeof data === 'string' ? JSON.parse(data) : data;
            resolve(parsed);
          } catch (_) {
            resolve(null);
          }
        } else {
          resolve(null);
        }
      });
      // Timeout fallback
      setTimeout(() => resolve(null), 3000);
    });
  }

  _persistData() {
    try {
      localStorage.setItem(ProjectPanel.STORAGE_KEY, JSON.stringify(this.data));
    } catch (_) { }
  }

  _updateWarning() {
    if (!this.warningBanner) return;
    const hasActive = this.data.currentIndex >= 0 && this.data.currentIndex < this.data.projects.length;
    this.warningBanner.style.display = hasActive ? 'none' : 'block';
  }

  _saveCurrentConfig() {
    const projectDir = this.cfgProjectDir.value.trim();
    const projectIdentity = this.cfgProjectIdentity.value.trim();
    const projectUrl = this.cfgProjectUrl.value.trim();
    if (!projectDir || !projectIdentity) return;

    // Check if this project already exists (match by projectIdentity)
    let idx = this.data.projects.findIndex(p => p.projectIdentity === projectIdentity);
    if (idx >= 0) {
      // Update existing
      this.data.projects[idx].projectIdentity = projectIdentity;
      this.data.projects[idx].projectUrl = projectUrl;
      this.data.projects[idx].lastUsed = new Date().toISOString();
    } else {
      // Add new
      this.data.projects.push({
        projectDir: projectDir,
        projectIdentity: projectIdentity,
        lastUsed: new Date().toISOString(),
        projectUrl: projectUrl,
      });
      idx = this.data.projects.length - 1;
    }
    this.data.currentIndex = idx;
    this._persistData();
    this._renderList();
    this._updateWarning();
    this._notifyDsl();
  }

  _selectProject(idx) {
    if (idx < 0 || idx >= this.data.projects.length) return;
    this.data.currentIndex = idx;
    const proj = this.data.projects[idx];
    this.cfgProjectDir.value = proj.projectDir || '';
    this.cfgProjectIdentity.value = proj.projectIdentity || '';
    this.cfgProjectUrl.value = proj.projectUrl || '';
    proj.lastUsed = new Date().toISOString();
    this._persistData();
    this._renderList();
    this._updateWarning();
    this._notifyDsl();
  }

  _deleteSelected() {
    const idx = this.data.currentIndex;
    if (idx < 0 || idx >= this.data.projects.length) return;
    this.data.projects.splice(idx, 1);
    // Adjust currentIndex
    if (this.data.projects.length === 0) {
      this.data.currentIndex = -1;
      this.cfgProjectDir.value = '';
      this.cfgProjectIdentity.value = '';
      this.cfgProjectUrl.value = '';
    } else {
      this.data.currentIndex = Math.min(idx, this.data.projects.length - 1);
      const cur = this.data.projects[this.data.currentIndex];
      this.cfgProjectDir.value = cur.projectDir || '';
      this.cfgProjectIdentity.value = cur.projectIdentity || '';
      this.cfgProjectUrl.value = cur.projectUrl || '';
    }
    this._persistData();
    this._renderList();
    this._updateWarning();
  }

  _renderList() {
    this.projectList.innerHTML = '';
    if (this.data.projects.length === 0) {
      const empty = document.createElement('div');
      empty.style.cssText = 'padding:8px 10px;color:#666;font-size:11px;text-align:center;';
      empty.textContent = 'No saved projects';
      this.projectList.appendChild(empty);
      return;
    }
    this.data.projects.forEach((proj, i) => {
      const item = document.createElement('div');
      const isCurrent = (i === this.data.currentIndex);
      item.style.cssText = `
          padding: 6px 10px;
          cursor: pointer;
          display: flex;
          justify-content: space-between;
          align-items: center;
          background: ${isCurrent ? '#333' : 'transparent'};
          border-left: 3px solid ${isCurrent ? '#4caf50' : 'transparent'};
          font-size: 12px;
        `;
      item.addEventListener('mouseenter', () => {
        if (!isCurrent) item.style.background = '#2a2a2a';
      });
      item.addEventListener('mouseleave', () => {
        if (!isCurrent) item.style.background = 'transparent';
      });

      const info = document.createElement('div');
      info.style.cssText = 'display:flex;flex-direction:column;gap:2px;overflow:hidden;';
      const dirLine = document.createElement('span');
      dirLine.style.cssText = `color:${isCurrent ? '#4caf50' : '#d4d4d4'};font-size:12px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;`;
      dirLine.textContent = proj.projectDir;
      dirLine.title = proj.projectDir;
      const prefixLine = document.createElement('span');
      prefixLine.style.cssText = 'color:#888;font-size:10px;';
      prefixLine.textContent = 'identity: ' + proj.projectIdentity;
      info.appendChild(dirLine);
      info.appendChild(prefixLine);

      item.appendChild(info);

      if (isCurrent) {
        const badge = document.createElement('span');
        badge.textContent = 'current';
        badge.style.cssText = 'color:#4caf50;font-size:10px;white-space:nowrap;margin-left:8px;';
        item.appendChild(badge);
      }

      item.onclick = () => this._selectProject(i);
      this.projectList.appendChild(item);
    });
  }

  // ---- Notify C# via bridge command ----

  _notifyDsl() {
    if (this.data.currentIndex < 0 || this.data.currentIndex >= this.data.projects.length) return;
    const proj = this.data.projects[this.data.currentIndex];
    if (!proj.projectDir || !proj.projectIdentity) return;
    if (this.bridge) {
      this.bridge.sendCommand('update_project_config', {
        projectDir: proj.projectDir,
        projectIdentity: proj.projectIdentity
      });
    }
  }

  // ---- Drag ----

  _initDrag(header) {
    const panel = this.panel;
    let dragging = false;
    let startX, startY, startLeft, startTop;
    header.addEventListener('mousedown', (e) => {
      const tag = e.target.tagName.toLowerCase();
      if (tag === 'input' || tag === 'button') return;
      dragging = true;
      startX = e.clientX;
      startY = e.clientY;
      const rect = panel.getBoundingClientRect();
      startLeft = rect.left;
      startTop = rect.top;
      panel.style.right = '';
      panel.style.bottom = '';
      panel.style.left = startLeft + 'px';
      panel.style.top = startTop + 'px';
      e.preventDefault();
    });
    document.addEventListener('mousemove', (e) => {
      if (!dragging) return;
      panel.style.left = (startLeft + e.clientX - startX) + 'px';
      panel.style.top = (startTop + e.clientY - startY) + 'px';
    });
    document.addEventListener('mouseup', () => { dragging = false; });
  }

  // ---- Show / hide ----

  _browseDirectory() {
    if (this._dirInput) {
      this._dirInput.click();
    }
  }

  show() {
    this.visible = true;
    this.panel.style.display = 'flex';
    if (this.onVisibilityChange) this.onVisibilityChange(true);
  }

  hide() {
    this.visible = false;
    this.panel.style.display = 'none';
    if (this.onVisibilityChange) this.onVisibilityChange(false);
  }

  toggle() {
    if (this.visible) this.hide(); else this.show();
  }
}
