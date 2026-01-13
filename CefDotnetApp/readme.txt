CefDotnetApp是一个c# dll工程，它由cef调用，用来执行浏览器页面发来的命令，它使用dsl脚本，同时用c#实现框架层的脚本api与脚本解释器。
AgentCore也是一个c# dll工程，它实际完成agent的功能，由CefDotnetApp加载执行，实现agent的主要功能、流程，并提供dsl脚本的api。
cefclient目录是实际运行的版本，其中managed子目录下的Script.dsl就是dsl脚本，inject.js是在浏览器页面上执行的javascript脚本，浏览器打开时会把这个脚本注入浏览器运行。
也就是说，整体架构上
1、cefclient启动嵌入浏览器，注入inject.js执行
2、inject.js负责模拟页面操作，典型是模仿用户使用LLM模型会话，读取页面上LLM的回复，或模拟用户输入与点击
3、inject.js通过cefclient扩展的浏览器与c#通信api与c# dll通信，由c# dll完成agent的功能
4、c# dll调用Script.dsl脚本来实现主体逻辑
5、agent的主要功能都在AgentCore工程里实现，这个c# dll设计为支持热更新与热加载，由CefDotnetApp负责加载与执行。dsl脚本使用的agent的api都由AgentCore工程实现
6、AgentCore里面的api实现考虑分为2层，一层是纯c#功能架构，另一层是脚本api包装，这一层把c#层的功能包装成api提供给dsl脚本
7、dsl脚本的语法采用MetaDSL语法
8、cefclient是多进程架构的浏览器，目前agent的c#功能主要在renderer进程完成，暂时未涉及browser进程，但需要了解这几个进程间通信与协作的机制

另外的cef目录是cefclient的实现（cefclient在cef源码的tests目录下），这里包含了简单的浏览器页面与c#通信（双向）的机制

备注：
1、CefDotnetApp只保留与c++的互操作、与dsl解释相关的功能和调用dsl函数的功能，还有AgentCore的抽象接口，这部分架构已经确定，非必要不要修改这里的代码，如需要修改需要特别确认

2、框架层依赖关系

CefDotnetApp (框架层)
    ├── 定义 IAgentPlugin 接口
    ├── 加载 AgentCore.dll
    ├── 通过 IAgentPlugin 接口调用 AgentCore
    └── 提供框架功能（DSL解释器、NativeApi等）

AgentCore (实现层)
    ├── 依赖 IAgentPlugin 接口（实现这个接口）
    ├── 依赖 CefDotnetApp 的框架功能
    └── 实现具体的 Agent 功能

3、依赖方向

CefDotnetApp → IAgentPlugin (定义接口)

AgentCore → IAgentPlugin (实现接口)

AgentCore → CefDotnetApp (使用框架功能)

CefDotnetApp ✗ AgentCore (不直接依赖实现)

4、多进程对应关系

Browser进程
  └── 多个 CefBrowser对象（多个窗口）
        └── 每个 CefBrowser 有一个 WebContents
              └── WebContents 包含多个 RenderFrameHost（主frame + subframes）
                    └── RenderFrameHost 指向 RenderProcessHost

Renderer进程
  └── RenderProcessHost（进程）
        └── 可以服务多个 WebContents 的 frame（同源情况下）