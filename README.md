# BatchCommand

A simple batch processing and template code generation script based on DSL syntax. The main purpose is to write batch processing on different platforms such as Windows and Mac in one script, or to generate code.

deps/DslCalculator.cs is a relatively general script interpreter and implementation of common APIs.
Script/BatchScript.cs is an API for batch processing, such as running another program, directory and file operations, etc. In addition, the template code generation mechanism is also implemented here, which is to embed the script again in the output text to output the program way to digitize data.

The difference between this interpreter and general interpreters is that the MetaDSL grammar abstracts the common grammatical constructs of C-like languages ​​into three types of grammar: values, functions, and statements, and then the interpreter directly hands these grammatical constructs to the API for processing. , so functions and statements are implemented by API.

[Interpreter principle notes](https://zhuanlan.zhihu.com/p/82055862), this note is based on the previous version of MetaDSL. At that time, the basic syntax of MetaDSL had four categories: value, function call, function definition, and statement. Now there are three categories, but the principle is the same.

In addition, when BatchCommand is used for code generation, the template code generation function is often used. This is [note](https://zhuanlan.zhihu.com/p/618899030) on the relevant principles .

Because it is developed in C#, it is usually not necessary to check the source code to understand the API. It is more convenient to use ilspy to directly decompile BatchCommand.exe to check the API.

There are two main entry points for API registration. You can use ilspy to find the API from these two entries. One is DslCalculator.Init in the DslCalculator.cs file. You can see this by searching for this in ilspy:

- Basic API registration
![Basic API](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/api1.png)

- Batch processing and template replacement API registration
![Batch processing and template replacement API registration](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/api2.png)

There are two ways to implement the API. One is to inherit the AbstractExpression class and override the Load and DoCalc methods. The Load method has different overloaded versions corresponding to different syntax categories of MetaDSL. The main purpose here is to parse the API to prepare runtime information. DoCalc is To implement the function of the API, it is generally to calculate the parameter value, and then calculate the result based on the parameter value. The other is to inherit the SimpleExpression class and override the DoCalc method. Most APIs fall into this category. It is a function API that receives some value parameters and then calculates the results. The base class has already calculated the parameter values. In DoCalc, you only need to calculate the parameters based on the parameters. value to calculate the result.

- AbstractExpression类
![api abstract class, used in all class](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/apiintf1.png)

- SimpleExpression类
![Simple function class API, used for common value parameters and return value APIs, the vast majority of APIs are of this type.](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/apiintf2.png)

The following are simple examples of two types of APIs. Usually we click on the implemented api class in the registration section of ilspy and jump to see what parameters the API receives, what work it does, and what results it returns.

- Simple function API example
![Function API implementation example
](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/api_func_exam.png)

- More complex API examples (statement APIs usually use this type of implementation, or APIs whose parameters are not ordinary value types)
![Other API implementation examples](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/api_other_exam.png)

![Other API implementation examples](https://raw.githubusercontent.com/dreamanlan/BatchCommand/master/api_other2_exam.png)

