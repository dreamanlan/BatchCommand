---
# Please note: Do not modify the header of this document. If modified, CodeBuddy (Internal Edition) will apply the default logic settings.
type: always
---

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

另外的cef目录是cefclient的实现（cefclient在cef源码的tests目录下），这里包含了简单的浏览器页面与c#通信（双向）的机制，你可以阅读这里的代码，但不要修改这部分代码

你看一下你平时写代码会需要agent给你提供哪些功能，把它们设计成c# api并在AgentCore工程里实现。
1、先规划好功能架构，列出要实现哪些api
2、规划好要添加的c#源代码文件与分别实现的api
3、按规划逐一实现
4、是否可以持续的基于一个编程agent所需的功能迭代开发，目标是实现一个基本可用的编程agent
5、从流程上实现这个webagent的自举，也就是后面我们在这个agent里继续修改这个agent的功能，直到完善

备注：
1、CefDotnetApp只保留与c++的互操作、与dsl解释相关的功能和调用dsl函数的功能，还有AgentCore的抽象接口，这部分架构已经确定，非必要不要修改这里的代码，如需要修改需要特别确认
2、你需要理解这个极简框架的思路，这部分是稳定不变的，后续修改应该主要发生在AgentCore、dsl脚本与inject.js这些代码里，而这些都是可以热更新热加载的
3、我的目标是实现热加载后，你就可以使用CefClient这个环境来工作，然后实现agent修改agent的自举
4、现在阶段，我们针对每个问题进行处理，从现在开始不再进行大规模的重构，只允许小范围修改，并且每次修改前先确认

框架层依赖关系：
CefDotnetApp (框架层)
    ├── 定义 IAgentPlugin 接口
    ├── 加载 AgentCore.dll
    ├── 通过 IAgentPlugin 接口调用 AgentCore
    └── 提供框架功能（DSL解释器、NativeApi等）

AgentCore (实现层)
    ├── 依赖 IAgentPlugin 接口（实现这个接口）
    ├── 依赖 CefDotnetApp 的框架功能
    └── 实现具体的 Agent 功能

依赖方向
CefDotnetApp → IAgentPlugin (定义接口)

AgentCore → IAgentPlugin (实现接口)

AgentCore → CefDotnetApp (使用框架功能)

CefDotnetApp ✗ AgentCore (不直接依赖实现)

多进程对应关系：
Browser进程
  └── 多个 CefBrowser对象（多个窗口）
        └── 每个 CefBrowser 有一个 WebContents
              └── WebContents 包含多个 RenderFrameHost（主frame + subframes）
                    └── RenderFrameHost 指向 RenderProcessHost

Renderer进程
  └── RenderProcessHost（进程）
        └── 可以服务多个 WebContents 的 frame（同源情况下）