// SecretStore - Encrypted storage using IndexedDB + Web Crypto API (AES-GCM 256-bit)
// Simplified version for AiClaw project
class SecretStore {
  constructor(dbName) {
    this._dbName = dbName || 'aiclaw_secret_store';
    this._db = null;
    this._key = null;
    this._readyPromise = this._init();
  }

  async ready() {
    return this._readyPromise;
  }

  async _init() {
    this._db = await this._openDB();
    this._key = await this._getOrCreateKey();
  }

  _openDB() {
    return new Promise(function (resolve, reject) {
      const request = indexedDB.open(this._dbName, 1);
      request.onupgradeneeded = function (event) {
        const db = event.target.result;
        if (!db.objectStoreNames.contains('crypto_keys')) {
          db.createObjectStore('crypto_keys');
        }
        if (!db.objectStoreNames.contains('encrypted_data')) {
          db.createObjectStore('encrypted_data');
        }
      };
      request.onsuccess = function (event) {
        resolve(event.target.result);
      };
      request.onerror = function (event) {
        reject(new Error('Failed to open IndexedDB: ' + event.target.error));
      };
    }.bind(this));
  }

  async _getOrCreateKey() {
    const existing = await this._idbGet('crypto_keys', 'master_key');
    if (existing) {
      return existing;
    }
    const key = await crypto.subtle.generateKey(
      { name: 'AES-GCM', length: 256 },
      false,
      ['encrypt', 'decrypt']
    );
    await this._idbPut('crypto_keys', 'master_key', key);
    return key;
  }

  async setItem(key, value) {
    const iv = crypto.getRandomValues(new Uint8Array(12));
    const encoded = new TextEncoder().encode(value);
    const encrypted = await crypto.subtle.encrypt(
      { name: 'AES-GCM', iv: iv },
      this._key,
      encoded
    );
    await this._idbPut('encrypted_data', key, { iv: iv, data: encrypted });
  }

  async getItem(key) {
    const record = await this._idbGet('encrypted_data', key);
    if (!record) return null;
    try {
      const decrypted = await crypto.subtle.decrypt(
        { name: 'AES-GCM', iv: record.iv },
        this._key,
        record.data
      );
      return new TextDecoder().decode(decrypted);
    } catch (e) {
      console.error('SecretStore: decrypt failed for key=' + key, e);
      return null;
    }
  }

  async removeItem(key) {
    await this._idbDelete('encrypted_data', key);
  }

  _idbGet(storeName, key) {
    return new Promise(function (resolve, reject) {
      const tx = this._db.transaction(storeName, 'readonly');
      const store = tx.objectStore(storeName);
      const request = store.get(key);
      request.onsuccess = function () { resolve(request.result); };
      request.onerror = function () { reject(request.error); };
    }.bind(this));
  }

  _idbPut(storeName, key, value) {
    return new Promise(function (resolve, reject) {
      const tx = this._db.transaction(storeName, 'readwrite');
      const store = tx.objectStore(storeName);
      const request = store.put(value, key);
      request.onsuccess = function () { resolve(); };
      request.onerror = function () { reject(request.error); };
    }.bind(this));
  }

  _idbDelete(storeName, key) {
    return new Promise(function (resolve, reject) {
      const tx = this._db.transaction(storeName, 'readwrite');
      const store = tx.objectStore(storeName);
      const request = store.delete(key);
      request.onsuccess = function () { resolve(); };
      request.onerror = function () { reject(request.error); };
    }.bind(this));
  }
}
