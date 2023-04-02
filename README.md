# BatchCommand

一个简单的基于dsl语法的批处理脚本，主要目的是在一个脚本里写在windows与mac等不同平台上的批处理。

deps/DslCalculator.cs是一个相对通用的脚本解释器与常用API的实现。
Script/BatchScript.cs是用于批处理的一个API，比如运行另外一个程序、目录与文件操作之类，另外也在这里实现了模板代码生成的机制，就是在输出文本里面再次嵌入脚本来输出程序化数据的方式。

这个解释器与一般解释器的不同点在于，因为MetaDSL语法将类C语言的常用语法构造都抽象成了3类语法：值、函数、语句，然后解释器直接将这些语法构造交给API来处理，所以函数与语句都是API实现的。

因为是C#开发的，通常不用查看源码来了解API，使用ilspy直接反编译BatchCommand.exe来查看API会更方便一些。

主要有2个API注册的入口点，可以用ilspy从这2处入口来查找API。一个是DslCalculator.cs文件里的DslCalculator.Init，ilspy里搜索这个就可以看到：

- 基础API注册
![基础API,相对通用的](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/api1.png)

- 批处理与模板替换API注册
![批处理与模板替换API，专用](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/api2.png)

API的实现有2种方式，一种是继承AbstractExpression类，重写Load与DoCalc方法，Load方法有不同的重载版本对应MetaDSL的不同语法类别，这里主要是解析API来准备运行时信息，DoCalc是实现API的功能，一般就是计算参数值，然后根据参数值来计算结果。另一种是继承SimpleExpression类，重写DoCalc方法，绝大多数API属于此类，它是接收一些值参数然后计算结果的函数类API，基类已经计算了参数值，在DoCalc里只需要根据参数值来计算结果即可。

- AbstractExpression类
![api抽象类，适用于所有API](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/apiintf1.png)

- SimpleExpression类
![简单函数类API，用于常见的值参数与返回值类的api，绝大多数API都是此类](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/apiintf2.png)

下面是2种类型的API的简单示例，通常我们在ilspy的注册部分点击实现的api类跳过来查看就可以知道这个API接收哪些参数，做什么工作，返回什么结果了。

- 简单函数API示例
![函数API实现示例](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/api_func_exam.png)

- 复杂一些的API示例（语句类API通常采用此类实现，或者参数不是普通值类型的API）
![其他API实现示例](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/api_other_exam.png)

![其他API实现示例](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/api_other2_exam.png)

