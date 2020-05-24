***Only Chinese document is available.***

# EProjectFile
一个用于编辑易语言项目文件的第三方库（C#）  
[![NuGet](https://img.shields.io/nuget/v/QIQI.EProjectFile.svg)](https://www.nuget.org/packages/QIQI.EProjectFile) [![CodeFactor](https://www.codefactor.io/repository/github/openepl/eprojectfile/badge)](https://www.codefactor.io/repository/github/openepl/eprojectfile)  
命名空间：`QIQI.EProjectFile`  

# 安装
Package Manager `Install-Package QIQI.EProjectFile`  
.NET CLI `dotnet add package QIQI.EProjectFile`  
使用VS的NuGet包管理器（GUI）搜索 `QIQI.EProjectFile`  
支持：.NET Framework 4.0或更高/.NET Core 2.0或更高

# 声明
1. 仅供学习与交流使用
2. 本项目属于**实验性项目**，所有API随时变更
3. 本项目随时终止，不做任何可用承诺
4. 对于使用本项目造成的一切损失，概不负责
5. 本项目可能造成被操作文件的数据丢失，请提前备份
6. 禁止用于非法用途，包括但不限于：破解他人软件
7. **本项目目前处于起步阶段，基本只提供底层解析/生成支持，功能极不完善**

# 交流
一般bug请用Github的Issues模块反馈  
如果您希望对本项目做出贡献，请使用标准GitHub工作流：Fork + Pull request  
进一步的快速讨论：请加入QQ群605310933 *（注意不要在群中反馈bug，这很可能导致反馈没有被记录。聊天消息较Issues模块比较混乱）*  

# 编译
使用NuGet安装缺失包，然后一般编译即可  
注意：**必须VS2017或更高版本，使用了最新语言特性** *（不过运行环境最低只需 .NET Framework 4.0 即可，支持XP）*  

# 例程
```cs
using QIQI.EProjectFile;
using QIQI.EProjectFile.Expressions;
using QIQI.EProjectFile.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


private static T[] AddArrayElement<T>(T[] array, T element)
{
	var newArray = new T[array.Length + 1];
	Array.Copy(array, newArray, array.Length);
	newArray[array.Length] = element;
	return newArray;
}
		
// 添加Tag子程序（如果不存在），然后在每个子程序的开头加上：Tag(<子程序名>)
var projectFile = new EProjectFile();
projectFile.Load(File.OpenRead(fileName));
int tagMethod = 0;
try
{
	tagMethod = Array.Find(projectFile.Code.Methods, x => x.IsStatic && string.Compare(x.Name, "Tag") == 0).Id;
}
catch (Exception)
{
	int tagStaticClass = projectFile.Code.AllocId(EplSystemId.Type_StaticClass);
	tagMethod = projectFile.Code.AllocId(EplSystemId.Type_Method);
	projectFile.Code.Classes = AddArrayElement(projectFile.Code.Classes, new ClassInfo(tagStaticClass)
	{
		Name = "TagMoudle",
		Method = new int[] { tagMethod }
	});
	projectFile.Code.Methods = AddArrayElement(projectFile.Code.Methods, new MethodInfo(tagMethod)
	{
		Name = "Tag",
		Class = tagStaticClass,
		CodeData = new StatementBlock() { new ExpressionStatement() }.ToCodeData(projectFile.Encoding),
		Parameters = new MethodParameterInfo[]
		{
			new MethodParameterInfo(projectFile.Code.AllocId(EplSystemId.Type_Local))
			{
				Name = "Name",
				DataType = EplSystemId.DataType_String
			}
		}
	});
	if (projectFile.EPackageInfo != null)
	{
		projectFile.EPackageInfo.FileNames = AddArrayElement(projectFile.EPackageInfo.FileNames, null);
	}
}
foreach (var method in projectFile.Code.Methods)
{
	if (method.Id != tagMethod)
	{
		StatementBlock block = CodeDataParser.ParseStatementBlock(method.CodeData.ExpressionData, method.CodeData.Encoding);
		{
			if (block[0] is ExpressionStatement exprStat && exprStat.Expression is CallExpression callExpr) 
				if (callExpr.LibraryId == -2 && callExpr.MethodId == tagMethod)
					block.RemoveAt(0);
		}
		block.Insert(0, new ExpressionStatement(new CallExpression(-2, tagMethod, new ParamListExpression() { new StringLiteral(method.Name) }), false, "Added from C# Project \"EProjectFile\""));
		method.CodeData = block.ToCodeData(projectFile.Encoding);
	}
}
projectFile.Save(File.Create(fileName));
```

# 感谢
本项目的发展得益于开源社区中许多贡献者的支持，这里仅列举部分人员用于表示感谢，*请注意该列表并非全部贡献者*
| 用户 | 成就 |
| ---- | ---- |
| 东灿 | [最早开源的项目文件结构分析代码](https://bbs.125.la/forum.php?mod=viewthread&tid=13751690) |
| 曙光 | 最早开源的代码数据解析软件 |
| 为你芯冻 | [e.net](https://github.com/wnxd/e.net)（包含一定的项目解析代码） |
| JimStone（谢栋） | 支持库兼容性信息分析 OpenEpl/EProjectFile@80348c3e42d775c1b2f2c45af699356c46b3503d |
