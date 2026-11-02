// ============================================================================
// ContactManager - Manage contacts, groups and session data for AiClaw
// ============================================================================

// Contact types
const CONTACT_TYPE = {
  LLM: 'llm',       // LLM client (from LlmManager)
  CLAW: 'claw',     // Relay claw client
  AICLAW: 'aiclaw'  // AiClaw built-in (User, PM, WxUser)
};

// ============================================================================
// ContactManager
// ============================================================================
class ContactManager {
  constructor() {
    // contacts: Map<id, {id, name, type, source, online}>
    this.contacts = new Map();
    // groups: Map<id, {id, name, memberIds:Set, announcement, saved}>
    this.groups = new Map();
    // sessions: Map<sessionId, {messages[], historyMessages[], currentMessages[], contextSummary}>
    this.sessions = new Map();
    // current active session id
    this.currentSessionId = null;
    // onChange callback
    this._onChange = null;

    this._loadGroups();
    this._loadSessions();
    this._ensureAllGroup();
  }

  // ---- Contact management ----

  // Add or update a contact
  addContact(id, name, type, source) {
    const existing = this.contacts.get(id);
    if (existing) {
      existing.name = name;
      existing.type = type;
      existing.source = source || existing.source;
      existing.online = true;
    } else {
      this.contacts.set(id, {
        id: id,
        name: name,
        type: type,
        source: source || '',
        online: true
      });
    }
    // Auto-add to "All" group
    const allGroup = this.groups.get('all');
    if (allGroup) {
      allGroup.memberIds.add(id);
    }
    // Ensure 1-on-1 session exists
    this._ensureSession('contact_' + id);
    this._emitChange();
  }

  // Remove a contact
  removeContact(id) {
    this.contacts.delete(id);
    // Remove from all groups
    this.groups.forEach(function (group) {
      group.memberIds.delete(id);
    });
    this._emitChange();
  }

  // Mark contact offline (don't remove, just flag)
  setContactOffline(id) {
    const contact = this.contacts.get(id);
    if (contact) {
      contact.online = false;
      this._emitChange();
    }
  }

  // Get contact by id
  getContact(id) {
    return this.contacts.get(id) || null;
  }

  // Get all contacts as array
  getAllContacts() {
    return Array.from(this.contacts.values());
  }

  // Get contacts by type
  getContactsByType(type) {
    return this.getAllContacts().filter(function (c) { return c.type === type; });
  }

  // ---- Sync from external sources ----

  // Sync contacts from LlmManager
  syncFromLlmManager(llmManager) {
    if (!llmManager) return;
    const currentLlmIds = new Set();
    llmManager.clients.forEach(function (client) {
      const id = 'llm_' + client.name;
      currentLlmIds.add(id);
      this.addContact(id, client.name, CONTACT_TYPE.LLM, 'llm');
      // Reflect enabled state as online
      const contact = this.contacts.get(id);
      if (contact) {
        contact.online = client.enabled;
      }
    }.bind(this));
    // Remove LLM contacts that no longer exist
    this.contacts.forEach(function (contact, id) {
      if (contact.type === CONTACT_TYPE.LLM && !currentLlmIds.has(id)) {
        this.contacts.delete(id);
      }
    }.bind(this));
    this._emitChange();
  }

  // Sync contacts from relay claw client list
  syncFromRelayClaw(clawClients) {
    if (!clawClients) return;
    const currentClawIds = new Set();
    clawClients.forEach(function (c) {
      const name = c.name || c.id;
      const id = 'claw_' + name;
      currentClawIds.add(id);
      this.addContact(id, name, CONTACT_TYPE.CLAW, 'relay');
    }.bind(this));
    // Mark missing claw contacts as offline
    this.contacts.forEach(function (contact, id) {
      if (contact.type === CONTACT_TYPE.CLAW && !currentClawIds.has(id)) {
        contact.online = false;
      }
    }.bind(this));
    this._emitChange();
  }

  // Ensure AiClaw built-in contacts exist
  ensureAiClawContacts(hasPm, wxUserName) {
    this.addContact('aiclaw_user', 'User', CONTACT_TYPE.AICLAW, 'local');
    if (hasPm) {
      this.addContact('aiclaw_pm', 'PM', CONTACT_TYPE.AICLAW, 'local');
    }
    if (wxUserName) {
      this.addContact('aiclaw_wx', wxUserName, CONTACT_TYPE.AICLAW, 'wechat');
    }
    this._emitChange();
  }

  // ---- Group management ----

  // Create a new group
  createGroup(name, memberIds) {
    const id = 'group_' + Date.now() + '_' + Math.random().toString(36).substr(2, 6);
    const group = {
      id: id,
      name: name || 'New Group',
      memberIds: new Set(memberIds || []),
      announcement: '',
      saved: true
    };
    this.groups.set(id, group);
    this._ensureSession(id);
    this._saveGroups();
    this._emitChange();
    return group;
  }

  // Save a group (mark as persistent)
  saveGroup(groupId) {
    const group = this.groups.get(groupId);
    if (group && group.id !== 'all') {
      group.saved = true;
      this._saveGroups();
      this._emitChange();
    }
  }

  // Dissolve (delete) a group
  dissolveGroup(groupId) {
    if (groupId === 'all') return; // cannot dissolve All group
    this.groups.delete(groupId);
    // Clean up session data
    this.sessions.delete(groupId);
    this._saveGroups();
    this._saveSessions();
    // Switch to All if current session was dissolved
    if (this.currentSessionId === groupId) {
      this.currentSessionId = 'all';
    }
    this._emitChange();
  }

  // Add member to group
  addMemberToGroup(groupId, contactId) {
    const group = this.groups.get(groupId);
    if (group) {
      group.memberIds.add(contactId);
      if (group.saved) { console.log('[DEBUG] _saveGroups triggered for:', groupId); this._saveGroups(); }
      this._emitChange();
    }
  }

  // Remove member from group
  removeMemberFromGroup(groupId, contactId) {
    const group = this.groups.get(groupId);
    if (group && group.id !== 'all') {
      group.memberIds.delete(contactId);
      if (group.saved) this._saveGroups();
      this._emitChange();
    }
  }

  // Update group announcement
  setAnnouncement(groupId, text) {
    const group = this.groups.get(groupId);
    if (group) {
      group.announcement = text || '';
      console.log('[DEBUG] setAnnouncement:', groupId, 'text:', JSON.stringify(text), 'saved:', group.saved);
      if (group.saved) this._saveGroups();
      this._emitChange();
    }
  }

  // Get group by id
  getGroup(id) {
    return this.groups.get(id) || null;
  }

  // Get all groups as array
  getAllGroups() {
    return Array.from(this.groups.values());
  }

  // Get group members as contact array
  getGroupMembers(groupId) {
    const group = this.groups.get(groupId);
    if (!group) return [];
    const self = this;
    return Array.from(group.memberIds).map(function (id) {
      return self.contacts.get(id);
    }).filter(Boolean);
  }

  // ---- Session data management ----

  // Get or create session data for a session id
  getSession(sessionId) {
    if (!this.sessions.has(sessionId)) {
      this._ensureSession(sessionId);
    }
    return this.sessions.get(sessionId);
  }

  // Get current active session data
  getCurrentSession() {
    if (!this.currentSessionId) {
      this.currentSessionId = 'all';
    }
    return this.getSession(this.currentSessionId);
  }

  // Switch to a different session
  switchSession(sessionId) {
    if (!this.sessions.has(sessionId)) {
      this._ensureSession(sessionId);
    }
    this.currentSessionId = sessionId;
    this._emitChange();
    return this.sessions.get(sessionId);
  }

  // Add a message to a session's UI messages
  addMessage(sessionId, msg) {
    const session = this.getSession(sessionId);
    session.messages.push(msg);
  }

  // Add to context (current round messages)
  addToContext(sessionId, role, content, name) {
    const session = this.getSession(sessionId);
    session.currentMessages.push({ role: role, content: content, name: name });
  }

  // Move current messages to history (round boundary)
  commitRound(sessionId) {
    const session = this.getSession(sessionId);
    if (session.currentMessages.length > 0) {
      session.historyMessages = session.historyMessages.concat(session.currentMessages);
      session.currentMessages = [];
    }
  }

  // Set context summary for a session
  setContextSummary(sessionId, summary) {
    const session = this.getSession(sessionId);
    session.contextSummary = summary;
    this._saveSessions();
  }

  // Append to context summary
  appendContextSummary(sessionId, text) {
    const session = this.getSession(sessionId);
    session.contextSummary = session.contextSummary
      ? session.contextSummary + '\n---\n' + text
      : text;
    this._saveSessions();
  }

  // Clear a session's data
  clearSession(sessionId) {
    const session = this.getSession(sessionId);
    session.messages = [];
    session.historyMessages = [];
    session.currentMessages = [];
    session.contextSummary = '';
    this._saveSessions();
  }

  // ---- Session ID helpers ----

  // Get session id for a 1-on-1 contact conversation
  getContactSessionId(contactId) {
    return 'contact_' + contactId;
  }

  // Get session id for a group
  getGroupSessionId(groupId) {
    return groupId; // group id is already the session id
  }

  // Check if a session id is a 1-on-1 contact session
  isContactSession(sessionId) {
    return sessionId && sessionId.startsWith('contact_');
  }

  // Check if a session id is a group session
  isGroupSession(sessionId) {
    return sessionId && !sessionId.startsWith('contact_');
  }

  // Get the contact id from a contact session id
  getContactIdFromSession(sessionId) {
    if (this.isContactSession(sessionId)) {
      return sessionId.substring(8); // remove 'contact_'
    }
    return null;
  }

  // Get members for current session (for forwarding)
  getCurrentSessionMembers() {
    const sessionId = this.currentSessionId || 'all';
    if (this.isGroupSession(sessionId)) {
      return this.getGroupMembers(sessionId);
    } else {
      // 1-on-1: just the contact
      const contactId = this.getContactIdFromSession(sessionId);
      const contact = this.contacts.get(contactId);
      return contact ? [contact] : [];
    }
  }

  // Check if a contact is a member of the current session
  isInCurrentSession(contactId) {
    const sessionId = this.currentSessionId || 'all';
    if (this.isGroupSession(sessionId)) {
      const group = this.groups.get(sessionId);
      return group ? group.memberIds.has(contactId) : false;
    } else {
      return this.getContactIdFromSession(sessionId) === contactId;
    }
  }

  // Find contact id by name (for matching incoming messages)
  findContactIdByName(name) {
    let found = null;
    this.contacts.forEach(function (contact, id) {
      if (contact.name === name) {
        found = id;
      }
    });
    return found;
  }

  // ---- onChange callback ----

  onChange(fn) {
    this._onChange = fn;
  }

  _emitChange() {
    if (this._onChange) {
      try { this._onChange(); } catch (e) {
        console.error('[contacts] onChange error:', e.message);
      }
    }
  }

  // ---- Internal helpers ----

  _ensureAllGroup() {
    if (!this.groups.has('all')) {
      this.groups.set('all', {
        id: 'all',
        name: 'All',
        memberIds: new Set(),
        announcement: '',
        saved: true
      });
    }
    this._ensureSession('all');
  }

  _ensureSession(sessionId) {
    if (!this.sessions.has(sessionId)) {
      this.sessions.set(sessionId, {
        messages: [],
        historyMessages: [],
        currentMessages: [],
        contextSummary: ''
      });
    }
  }

  // ---- Persistence ----

  _saveGroups() {
    try {
      const data = [];
      this.groups.forEach(function (group) {
        if (group.saved) {
          data.push({
            id: group.id,
            name: group.name,
            memberIds: Array.from(group.memberIds),
            announcement: group.announcement
          });
        }
      });
      localStorage.setItem('aiclaw_groups', JSON.stringify(data));
    } catch (e) {
      console.warn('[contacts] Failed to save groups:', e.message);
    }
  }

  _loadGroups() {
    try {
      const raw = localStorage.getItem('aiclaw_groups');
      if (raw) {
        const arr = JSON.parse(raw);
        arr.forEach(function (g) {
          this.groups.set(g.id, {
            id: g.id,
            name: g.name,
            memberIds: new Set(g.memberIds || []),
            announcement: g.announcement || '',
            saved: true
          });
        }.bind(this));
      }
    } catch (e) {
      console.warn('[contacts] Failed to load groups:', e.message);
    }
  }

  _saveSessions() {
    try {
      const data = {};
      this.sessions.forEach(function (session, id) {
        // Only save sessions that have contextSummary (lightweight persistence)
        // Full message history is not persisted to avoid localStorage bloat
        if (session.contextSummary) {
          data[id] = {
            contextSummary: session.contextSummary
          };
        }
      });
      localStorage.setItem('aiclaw_sessions', JSON.stringify(data));
    } catch (e) {
      console.warn('[contacts] Failed to save sessions:', e.message);
    }
  }

  _loadSessions() {
    try {
      const raw = localStorage.getItem('aiclaw_sessions');
      if (raw) {
        const obj = JSON.parse(raw);
        for (const id in obj) {
          if (obj.hasOwnProperty(id)) {
            this._ensureSession(id);
            const session = this.sessions.get(id);
            session.contextSummary = obj[id].contextSummary || '';
          }
        }
      }
    } catch (e) {
      console.warn('[contacts] Failed to load sessions:', e.message);
    }
  }
}
