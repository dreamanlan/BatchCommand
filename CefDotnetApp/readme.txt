CefDotnetApp是一个c# dll工程，它由cef调用，用来执行浏览器页面发来的命令，它使用dsl脚本，同时用c#实现框架层的脚本api与脚本解释器。
AgentCore也是一个c# dll工程，它实际完成agent的功能，由CefDotnetApp加载执行，实现agent的主要功能、流程，并提供dsl脚本的api。
cefclient目录是实际运行的版本，其中managed子目录下的script.dsl与script_renderer.dsl就是dsl脚本，inject.js是在浏览器页面上执行的javascript脚本，浏览器打开时会把这个脚本注入浏览器运行。
cefclient的managed目录下还有一个monitor.dsl脚本，这个是配合cefclient目录下的BatchCmdDslHost.exe与managed目录下的BatchCmdDsl.dll工作的独立进程脚本，主要用来监控cefclient进程并保活。
也就是说，整体架构上
1、cefclient启动嵌入浏览器，注入inject.js执行(或inject_modules/*.js)
2、inject.js负责模拟页面操作，典型是模仿用户使用LLM模型会话，读取页面上LLM的回复，或模拟用户输入与点击
3、inject.js通过cefclient扩展的浏览器与c#通信api与c# dll通信，由c# dll完成agent的功能
4、c# dll调用script.dsl与script_renderer.dsl脚本来实现主体逻辑
5、agent的主要功能都在AgentCore工程里实现，这个c# dll设计为支持热更新与热加载，由CefDotnetApp负责加载与执行。dsl脚本使用的agent的api都由AgentCore工程实现
6、AgentCore里面的api实现考虑分为2层，一层是纯c#功能架构，另一层是脚本api包装，这一层把c#层的功能包装成api提供给dsl脚本
7、dsl脚本的语法采用MetaDSL语法
8、cefclient是多进程架构的浏览器，目前agent的c#功能主要在renderer进程完成，暂时未涉及browser进程，但需要了解这几个进程间通信与协作的机制
9、monitor.dsl提供了另外一层的保障，确保cefclient进程持续存在，这部分不需要进行修改，功能相对单一
10、注意，dsl脚本不支持异常处理，所以并没有try-catch这样的语法，目前api也只有普通函数样式的，不支持命名参数语法，此外，变量可以是对象，所以有类似对象成员访问或对象方法调用的语法

另外的cef目录是cefclient的实现（cefclient在cef源码的tests目录下），这里包含了简单的浏览器页面与c#通信（双向）的机制，你可以阅读这里的代码，但不要修改这部分代码

你看一下你平时写代码会需要agent给你提供哪些功能，把它们设计成c# api并在AgentCore工程里实现。
1、先规划好功能架构，列出要实现哪些api
2、规划好要添加的c#源代码文件与分别实现的api
3、按规划逐一实现
4、是否可以持续的基于一个编程agent所需的功能迭代开发，目标是实现一个基本可用的编程agent
5、从流程上实现这个webagent的自举，也就是后面我们在这个agent里继续修改这个agent的功能，直到完善
6、AgentCore目录下有一个hotreload_test.html，是测试页面，需要测试的功能入口放到这里
7、在CefDotnetApp与AgentCore目录还有cefclient目录下都各有一个docs子目录，你的开发备注md文档都放到这些目录下，不要放在各模块根目录下了
8、在cefclient目录下有2个log文件，debug.log似乎更多是renderer进程输出的，是累积的日志，console.log是页面javascript的日志，另外还有一个日志文件cefclient_cache/chrome_debug.log是每次运行的日志，这个日志更全，但只是最近一次运行的日志
9、在cefclient目录下有个子目录cefclientdbg是调试版本的cefclient，调试版本里8中提到的3个日志文件会在这个目录下
10、在CefDotnetApp目录下有个bak子目录，这是cefclient使用的脚本的备份，不是实际工作的版本，不要读写这些文件（因为cefclient没有版本管理，备份是为了有版本管理）
11、**所有 C# 直接调用 JS 的代码都从 window 对象上的方法调用**（强制规则，确保通信接口清晰统一）
12、dsl的api是面向LLM，用于LLM与agent沟通的语言，设计上应该符合LLM的习惯，输入通常应是文件或字符串或基础类型数值或布尔值，输出结果应该是字符串或基础类型数值或布尔值

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
