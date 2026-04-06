// ============================================================================
// SecretStore - Encrypted storage using IndexedDB + Web Crypto API
// Uses a non-extractable AES-GCM CryptoKey stored in IndexedDB.
// Provides async getItem/setItem/removeItem for sensitive data.
// ============================================================================
class SecretStore {
  static DB_NAME = 'agent_secret_store';
  static DB_VERSION = 1;
  static KEY_STORE = 'crypto_keys';
  static DATA_STORE = 'encrypted_data';
  static CRYPTO_KEY_ID = 'master_key';

  constructor() {
    this._db = null;
    this._cryptoKey = null;
    this._readyPromise = this._init();
  }

  async _init() {
    try {
      this._db = await this._openDB();
      this._cryptoKey = await this._getOrCreateKey();
      // Migrate legacy localStorage secrets on first run
      await this._migrateLegacy();
    } catch (e) {
      logger.error('[SecretStore] Initialization failed: ' + e);
      throw e;
    }
  }

  // Wait for initialization to complete
  async ready() {
    return this._readyPromise;
  }

  _openDB() {
    return new Promise((resolve, reject) => {
      const req = indexedDB.open(SecretStore.DB_NAME, SecretStore.DB_VERSION);
      req.onupgradeneeded = (e) => {
        const db = e.target.result;
        if (!db.objectStoreNames.contains(SecretStore.KEY_STORE)) {
          db.createObjectStore(SecretStore.KEY_STORE, { keyPath: 'id' });
        }
        if (!db.objectStoreNames.contains(SecretStore.DATA_STORE)) {
          db.createObjectStore(SecretStore.DATA_STORE, { keyPath: 'id' });
        }
      };
      req.onsuccess = (e) => resolve(e.target.result);
      req.onerror = (e) => reject(e.target.error);
    });
  }

  async _getOrCreateKey() {
    // Try to load existing key from IndexedDB
    const existing = await this._idbGet(SecretStore.KEY_STORE, SecretStore.CRYPTO_KEY_ID);
    if (existing && existing.key) {
      return existing.key;
    }
    // Generate a new non-extractable AES-GCM 256-bit key
    const key = await crypto.subtle.generateKey(
      { name: 'AES-GCM', length: 256 },
      false, // non-extractable
      ['encrypt', 'decrypt']
    );
    // Store the CryptoKey object in IndexedDB (structured clone preserves it)
    await this._idbPut(SecretStore.KEY_STORE, { id: SecretStore.CRYPTO_KEY_ID, key });
    return key;
  }

  async _encrypt(plaintext) {
    const enc = new TextEncoder();
    const iv = crypto.getRandomValues(new Uint8Array(12));
    const cipherBuf = await crypto.subtle.encrypt(
      { name: 'AES-GCM', iv },
      this._cryptoKey,
      enc.encode(plaintext)
    );
    // Store iv + ciphertext as arrays for IndexedDB structured clone
    return {
      iv: Array.from(iv),
      data: Array.from(new Uint8Array(cipherBuf))
    };
  }

  async _decrypt(record) {
    const iv = new Uint8Array(record.iv);
    const data = new Uint8Array(record.data);
    const plainBuf = await crypto.subtle.decrypt(
      { name: 'AES-GCM', iv },
      this._cryptoKey,
      data
    );
    return new TextDecoder().decode(plainBuf);
  }

  // ---- Public API ----

  async setItem(key, value) {
    const encrypted = await this._encrypt(value);
    await this._idbPut(SecretStore.DATA_STORE, { id: key, ...encrypted });
  }

  async getItem(key) {
    const record = await this._idbGet(SecretStore.DATA_STORE, key);
    if (!record || !record.iv) return null;
    try {
      return await this._decrypt(record);
    } catch (e) {
      logger.warn('[SecretStore] Decrypt failed for key: ' + key + ' ' + e);
      return null;
    }
  }

  async removeItem(key) {
    return this._idbDelete(SecretStore.DATA_STORE, key);
  }

  // ---- IndexedDB helpers ----

  _idbGet(storeName, key) {
    return new Promise((resolve, reject) => {
      const tx = this._db.transaction(storeName, 'readonly');
      const req = tx.objectStore(storeName).get(key);
      req.onsuccess = () => resolve(req.result || null);
      req.onerror = () => reject(req.error);
    });
  }

  _idbPut(storeName, value) {
    return new Promise((resolve, reject) => {
      const tx = this._db.transaction(storeName, 'readwrite');
      const req = tx.objectStore(storeName).put(value);
      req.onsuccess = () => resolve();
      req.onerror = () => reject(req.error);
    });
  }

  _idbDelete(storeName, key) {
    return new Promise((resolve, reject) => {
      const tx = this._db.transaction(storeName, 'readwrite');
      const req = tx.objectStore(storeName).delete(key);
      req.onsuccess = () => resolve();
      req.onerror = () => reject(req.error);
    });
  }

  _idbGetAllKeys(storeName) {
    return new Promise((resolve, reject) => {
      const tx = this._db.transaction(storeName, 'readonly');
      const req = tx.objectStore(storeName).getAllKeys();
      req.onsuccess = () => resolve(req.result || []);
      req.onerror = () => reject(req.error);
    });
  }

  // Get all user data keys (excluding internal keys like _migrated and master_key)
  async getAllKeys() {
    const allKeys = await this._idbGetAllKeys(SecretStore.DATA_STORE);
    return allKeys.filter(k => k !== '_migrated');
  }

  // ---- Migration from localStorage ----

  async _migrateLegacy() {
    // Only migrate once: check a flag in IndexedDB
    const flag = await this._idbGet(SecretStore.DATA_STORE, '_migrated');
    if (flag) return;

    let migrated = 0;

    // 1. Migrate agent_environments
    try {
      const envRaw = localStorage.getItem('agent_environments');
      if (envRaw) {
        await this.setItem('agent_environments', envRaw);
        localStorage.removeItem('agent_environments');
        migrated++;
      }
    } catch (e) {
      logger.warn('[SecretStore] Migration of agent_environments failed: ' + e);
    }

    // 2. Migrate openclaw_panel_config (contains apiKey)
    try {
      const clawRaw = localStorage.getItem('openclaw_panel_config');
      if (clawRaw) {
        await this.setItem('openclaw_panel_config', clawRaw);
        localStorage.removeItem('openclaw_panel_config');
        migrated++;
      }
    } catch (e) {
      logger.warn('[SecretStore] Migration of openclaw_panel_config failed: ' + e);
    }

    // 3. Migrate openclaw secrets from inject_config
    try {
      const cfgRaw = localStorage.getItem('inject_config');
      if (cfgRaw) {
        const cfg = JSON.parse(cfgRaw);
        if (cfg.openclaw) {
          const secrets = {};
          if (cfg.openclaw.apiKey) {
            secrets.apiKey = cfg.openclaw.apiKey;
            delete cfg.openclaw.apiKey;
          }
          if (cfg.openclaw.session) {
            secrets.session = cfg.openclaw.session;
            delete cfg.openclaw.session;
          }
          if (Object.keys(secrets).length > 0) {
            await this.setItem('openclaw_secrets', JSON.stringify(secrets));
            // Write back cleaned config
            localStorage.setItem('inject_config', JSON.stringify(cfg));
            migrated++;
          }
        }
      }
    } catch (e) {
      logger.warn('[SecretStore] Migration of inject_config secrets failed: ' + e);
    }

    // Mark migration complete
    await this._idbPut(SecretStore.DATA_STORE, { id: '_migrated', iv: [0], data: [1] });
    if (migrated > 0) {
      logger.info('[SecretStore] Migrated ' + migrated + ' item(s) from localStorage');
    }
  }
}

// Create global instance (initialization is async, use secretStore.ready() before first access)
const secretStore = new SecretStore();
