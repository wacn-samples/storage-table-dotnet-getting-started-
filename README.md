---
services: storage
platforms: dotnet
author: jasonnewyork

---

# .NET中使用Azure表服务起步

演示如何使用Azure表存储执行普通的任务，示例包括创建表、增删改查操作，批量操作和不同的查询技术

注意：这个示例使用.NET 4.5异步编程模型来演示如何使用存储客户端库的异步API调用存储服务。 在实际的应用中这种方式可以提高程序的响应速度。存储服务需要在调用时前面添加关键字await。如果您还没有Azure订阅，请点击[此处](/pricing/1rmb-trial)申请试用的订阅账号。


## 运行这个示例

这个示例可以在Azure存储模拟器（存储模拟器是Azure SDK安装的一部分）上运行，或者通过修改App.Config文档中的AccountName（存储账号）和Key（存储密钥）的方式来使用。 
   
使用Azure存储模拟器运行该示例（默认方式）

1. 下载并安装Azure存储模拟器，[下载地址](/downloads) 
2. 点击开始按钮或者Windows键，然后输入"Azure Storage Emulator"找到存储模拟器，点击运行。
3. 设置断点，使用F10运行该示例。

使用Azure存储服务来运行这个示例

1. 打开app.config文件, 注释掉模拟器的那一部分连接字符串(UseDevelopmentStorage=True) ，然后删掉存储服务的连接字符串的注释 (AccountName=[]...)。
2. 在Azure门户网站上创建存储账号，然后修改App.Config的 [AccountName]（存储账号）和 [AccountKey]（存储密钥）。
3. 设置断点，使用F10运行该示例。


##参考文档: 

- [什么是存储账号](/documentation/articles/storage-create-storage-account/)
- [表服务起步](/documentation/articles/storage-dotnet-how-to-use-tables/)
- [表服务概念](https://msdn.microsoft.com/zh-cn/library/dd179463.aspx)
- [表服务 REST API](https://msdn.microsoft.com/zh-cn/library/dd179423.aspx)
- [表服务 C# API](https://msdn.microsoft.com/zh-cn/library/azure/mt347887.aspx)
- [存储模拟器](/documentation/articles/storage-use-emulator/)
- [使用 Async 和 Await异步编程](http://msdn.microsoft.com/zh-cn/library/hh191443.aspx)
