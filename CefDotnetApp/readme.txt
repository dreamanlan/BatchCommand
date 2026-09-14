CefDotnetApp是一个c# dll工程，它由cef调用，用来执行浏览器页面发来的命令，它使用dsl脚本，同时用c#实现框架层的脚本api与脚本解释器。
webagent目录是实际运行的版本，其中managed子目录下的script.dsl与script_renderer.dsl就是dsl脚本，inject.js是在浏览器页面上执行的javascript脚本，浏览器打开时会把这个脚本注入浏览器运行。
webagent的managed目录下还有一个monitor.dsl脚本，这个是配合webagent目录下的BatchCmdDslHost.exe与managed目录下的BatchCmdDsl.dll工作的独立进程脚本，主要用来监控webagent进程并保活。
也就是说，整体架构上
1、webagent启动嵌入浏览器，注入inject.js执行(或inject_modules/*.js)
2、inject.js负责模拟页面操作，典型是模仿用户使用LLM模型会话，读取页面上LLM的回复，或模拟用户输入与点击
3、inject.js通过cefclient扩展的浏览器与c#通信api与c# dll通信
4、c# dll调用script.dsl与script_renderer.dsl脚本来实现主体逻辑
5、dsl脚本的语法采用MetaDSL语法
6、monitor.dsl提供了另外一层的保障，确保webagent进程持续存在，这部分不需要进行修改，功能相对单一
7、注意，dsl脚本不支持异常处理，所以并没有try-catch这样的语法，目前api也只有普通函数样式的，不支持命名参数语法，此外，变量可以是对象，所以有类似对象成员访问或对象方法调用的语法
8、实际agent功能由AgentCore实现，这个工程考虑运行在单独的进程以不破坏renderer进程的sandbox特性

另外的cef目录是webagent的实现（webagent在cef源码的myapp目录下），这里包含了简单的浏览器页面与c#通信（双向）的机制，你可以阅读这里的代码，但不要修改这部分代码

框架层依赖关系：
CefDotnetApp (框架层)
    └── 提供框架功能（DSL解释器、NativeApi等）

多进程对应关系：
Browser进程
  └── 多个 CefBrowser对象（多个窗口）
        └── 每个 CefBrowser 有一个 WebContents
              └── WebContents 包含多个 RenderFrameHost（主frame + subframes）
                    └── RenderFrameHost 指向 RenderProcessHost

Renderer进程
  └── RenderProcessHost（进程）
        └── 可以服务多个 WebContents 的 frame（同源情况下）
