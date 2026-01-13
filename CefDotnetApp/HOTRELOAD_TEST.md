# Hot Reload Test Guide

## Overview

This guide explains how to test the hot reload functionality for AgentCore.dll.

## Architecture

### Hot Reload Flow

1. **User triggers hot reload** (via test page or MetaDSL script)
2. **AgentPlugin.HotReloadAgentCore()** builds JSON request and calls JavaScript
3. **JavaScript executes cefQuery** with hot reload request
4. **C++ hot_reload_test.cc** receives the request:
   - Closes all browser windows (unloads DLLs)
   - Copies new AgentCore.dll from build output to runtime directory
   - Reopens browser window
5. **C++ calls OnBrowserHotReload** callback in C#
6. **C# calls on_browser_hot_reload** in Script.dsl
7. **Hot reload complete** - new DLL is loaded

## Components

### 1. AgentCore.dll Hot Reload
- **Mechanism**: Full browser restart (close → update DLL → reopen)
- **Source**: `AgentCore/bin/Debug/net8.0/AgentCore.dll`
- **Dest**: `cefclient/managed/AgentCore.dll`
- **Trigger**: `hot_reload('agentcore');` in MetaDSL

### 2. Script.dsl Hot Reload
- **Mechanism**: Auto-reload on every execution
- **No manual action needed** - just modify the file
- **Handled by**: `Program.cs::TryLoadDSL()`

### 3. inject.js Hot Reload
- **Mechanism**: Page refresh
- **Action**: Press F5 or Ctrl+R
- **Alternative**: `location.reload()` in JavaScript

## Testing Steps

### Test 1: Modify AgentCore.dll

1. **Modify AgentCore code** (e.g., add a log message in `AgentPlugin.cs`)
   ```csharp
   public void Initialize(string basePath)
   {
       // Add this line
       Core.AgentCore.Instance.Logger.Info("HOT RELOAD TEST - Modified version");
       // ... rest of code
   }
   ```

2. **Build AgentCore project**
   ```bash
   cd AgentCore
   dotnet build
   ```

3. **Open test page** in cefclient
   ```
   file:///d:/GitHub/BatchCommand/CefDotnetApp/hotreload_test.html
   ```

4. **Click "Reload AgentCore.dll" button**
   - Browser will close
   - DLL will be copied
   - Browser will reopen automatically

5. **Check logs** for your new log message

### Test 2: Modify Script.dsl

1. **Modify Script.dsl** (e.g., add a log in `on_browser_hot_reload`)
   ```dsl
   script(on_browser_hot_reload)
   {
       nativelog("[dsl] HOT RELOAD TEST - Script modified");
       return(0);
   };
   ```

2. **Execute any MetaDSL command** - Script.dsl will be auto-reloaded

3. **Check logs** for your new log message

### Test 3: Modify inject.js

1. **Modify inject.js** (e.g., add a console.log)
   ```javascript
   console.log('[inject.js] HOT RELOAD TEST - Modified version');
   ```

2. **Refresh the page** (F5 or Ctrl+R)

3. **Check browser console** for your new log message

## Expected Behavior

### AgentCore.dll Hot Reload

**Before:**
```
[csharp] AgentPlugin initialized at D:\GitHub\CEF_Src_Build\cefclient
```

**After hot reload:**
```
[csharp] Browser Hot Reload
[csharp] Call dsl on_browser_hot_reload
[dsl] on_browser_hot_reload called - AgentCore.dll has been reloaded
[csharp] HOT RELOAD TEST - Modified version
```

### Script.dsl Hot Reload

**Automatic on every execution:**
```
[csharp] DSL file changed, reloading...
[dsl] HOT RELOAD TEST - Script modified
```

### inject.js Hot Reload

**After page refresh:**
```
[inject.js] HOT RELOAD TEST - Modified version
[Agent] Initializing Agent Collaboration Framework...
```

## Troubleshooting

### Issue: Browser doesn't close

**Cause**: cefQuery not working or JavaScript error

**Solution**:
- Check browser console for errors
- Verify cefQuery is available: `typeof window.cefQuery`
- Check C++ logs for hot reload request

### Issue: DLL not updated

**Cause**: File copy failed or DLL locked

**Solution**:
- Check file paths in logs
- Verify source DLL exists
- Ensure no other process is locking the DLL

### Issue: Browser doesn't reopen

**Cause**: Window creation failed

**Solution**:
- Check C++ logs for errors
- Verify RootWindowManager is working
- Check if termination was re-enabled

## Implementation Files

- **C++**: `cef/tests/cefclient/browser/hot_reload_test.cc`
- **C# Framework**: `CefDotnetApp/Program.cs::OnBrowserHotReload`
- **C# Plugin**: `AgentCore/AgentPlugin.cs::HotReloadAgentCore`
- **DSL Script**: `cefclient/managed/Script.dsl::on_browser_hot_reload`
- **Test Page**: `CefDotnetApp/hotreload_test.html`

## Notes

1. **AgentCore.dll hot reload** is the only one that requires full browser restart
2. **Script.dsl** is automatically reloaded - no manual action needed
3. **inject.js** only needs page refresh - no browser restart
4. The hot reload mechanism is designed for development, not production
5. All hot reload operations are logged for debugging
