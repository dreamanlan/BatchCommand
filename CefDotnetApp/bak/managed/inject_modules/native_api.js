// ============================================================================
// NativeApi - Direct C++ bridge for platform-native features.
//
// These calls bypass the DSL/C# path used by AgentBridge.sendCommand and talk
// straight to C++ via window.cefQuery. C++ side handles them inside
// cef_query_handler.cc (search for "show_file_dialog").
//
// Returned Promises:
//   pickFile / pickDirectory / saveFile -> Promise<string | null>
//     - resolves to the selected absolute path
//     - resolves to null when the user canceled
//   pickFiles                            -> Promise<string[] | null>
//     - resolves to the list of selected absolute paths
//     - resolves to null when the user canceled
//
// All Promises reject on transport / native failure (never on user cancel).
// ============================================================================
const NativeApi = (() => {
  function _query(payload) {
    return new Promise((resolve, reject) => {
      if (typeof window.cefQuery !== 'function') {
        reject(new Error('NativeApi: window.cefQuery unavailable'));
        return;
      }
      try {
        window.cefQuery({
          request: JSON.stringify(payload),
          persistent: false,
          onSuccess: (response) => {
            try {
              const data = JSON.parse(response || '{}');
              resolve(data);
            } catch (e) {
              reject(new Error('NativeApi: bad response JSON: ' + e.message));
            }
          },
          onFailure: (code, msg) => {
            reject(new Error('NativeApi failure ' + code + ': ' + msg));
          },
        });
      } catch (e) {
        reject(e);
      }
    });
  }

  async function _showFileDialog(mode, opts) {
    const payload = { action: 'show_file_dialog', mode };
    if (opts) {
      if (opts.title) payload.title = String(opts.title);
      if (opts.defaultPath) payload.default_path = String(opts.defaultPath);
      if (Array.isArray(opts.acceptFilters) && opts.acceptFilters.length > 0) {
        payload.accept_filters = opts.acceptFilters.map(String);
      }
    }
    const data = await _query(payload);
    if (data && data.canceled) return null;
    return (data && Array.isArray(data.paths)) ? data.paths : null;
  }

  // Convenience wrappers: pickFile / pickDirectory / saveFile return a single
  // path (or null); pickFiles returns an array (or null).
  async function pickFile(opts)      { const p = await _showFileDialog('open',          opts); return p && p.length ? p[0] : null; }
  async function pickFiles(opts)     { return await _showFileDialog('open_multiple', opts); }
  async function pickDirectory(opts) { const p = await _showFileDialog('open_folder',   opts); return p && p.length ? p[0] : null; }
  async function saveFile(opts)      { const p = await _showFileDialog('save',          opts); return p && p.length ? p[0] : null; }

  return { pickFile, pickFiles, pickDirectory, saveFile };
})();
