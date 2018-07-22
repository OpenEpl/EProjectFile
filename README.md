***Only Chinese document is available.***

# EProjectFile
一个用于编辑易语言项目文件的第三方库（C#）  
NuGet地址：[https://www.nuget.org/packages/QIQI.EProjectFile](https://www.nuget.org/packages/QIQI.EProjectFile)  
命名空间：`QIQI.EProjectFile`  

# 安装
Package Manager `Install-Package QIQI.EProjectFile`  
.NET CLI `dotnet add package QIQI.EProjectFile`  
或使用VS的NuGet包管理器（GUI）搜索“QIQI.EProjectFile”  

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
注意：**必须VS2017或更高版本，使用了最新语言特性** *（不过运行环境只需.NET 4.0即可）*  

# 例程
```cs
using QIQI.EProjectFile;
using QIQI.EProjectFile.Expressions;
using QIQI.EProjectFile.Statements;


// 添加Tag子程序（如果不存在），然后在每个子程序的开头加上：Tag(<子程序名>)
ESystemInfo systemInfo = null;
CodeSectionInfo codeSectionInfo = null;
ResourceSectionInfo resourceSectionInfo = null;
InitEcSectionInfo initEcSectionInfo = null;
EPackageInfo ePackageInfo = null;
var sections = new List<SectionInfo>();
using (var projectFileReader = new ProjectFileReader(File.OpenRead(fileName)))
{
	while (!projectFileReader.IsFinish)
	{
		var section = projectFileReader.ReadSection();
		switch (section.Name)
		{
			case ESystemInfo.SectionName:
				systemInfo = ESystemInfo.Parse(section.Data);
				break;
			case CodeSectionInfo.SectionName:
				codeSectionInfo = CodeSectionInfo.Parse(section.Data, projectFileReader.CryptEc);
				break;
			case ResourceSectionInfo.SectionName:
				resourceSectionInfo = ResourceSectionInfo.Parse(section.Data);
				break;
			case InitEcSectionInfo.SectionName:
				initEcSectionInfo = InitEcSectionInfo.Parse(section.Data);
				break;
			case EPackageInfo.SectionName:
				ePackageInfo = EPackageInfo.Parse(section.Data);
				break;
			default:
				break;
		}
		sections.Add(section);
	}
}

int tagMethod = 0;
try
{
	tagMethod = Array.Find(codeSectionInfo.Methods, x => (EplSystemId.GetType(x.Class) == EplSystemId.Type_StaticClass || EplSystemId.GetType(x.Class) == EplSystemId.Type_FormClass) && x.Name.ToLower() == "Tag".ToLower()).Id;
}
catch (Exception)
{
	int tagStaticClass = codeSectionInfo.AllocId(EplSystemId.Type_StaticClass);
	tagMethod = codeSectionInfo.AllocId(EplSystemId.Type_Method);
	codeSectionInfo.Classes = new List<ClassInfo>(codeSectionInfo.Classes)
	{
		new ClassInfo(tagStaticClass)
		{
			Name = "TagMoudle",
			Method = new int[]{ tagMethod }
		}
	}.ToArray();
	codeSectionInfo.Methods = new List<MethodInfo>(codeSectionInfo.Methods)
	{
		new MethodInfo(tagMethod)
		{
			Name = "Tag",
			Class = tagStaticClass,
			CodeData = new StatementBlock(){ new ExpressionStatement() }.ToCodeData(),
			Parameters = new MethodParameterInfo[]
			{
				new MethodParameterInfo(codeSectionInfo.AllocId(EplSystemId.Type_Local))
				{
					Name = "Name",
					DataType = EplSystemId.DataType_String
				}
			}
		}
	}.ToArray();
	if (ePackageInfo != null)
	{
		ePackageInfo.FileNames = new List<string>(ePackageInfo.FileNames) { null }.ToArray();
	}
}

foreach (var method in codeSectionInfo.Methods) 
{
	if (method.Id != tagMethod)
	{
		StatementBlock block = CodeDataParser.ParseStatementBlock(method.CodeData.ExpressionData);
		{
			if (block[0] is ExpressionStatement exprStat && exprStat.Expression is CallExpression callExpr)
				if (callExpr.LibraryId == -2 && callExpr.MethodId == tagMethod)
					block.RemoveAt(0);
		}
		block.Insert(0, new ExpressionStatement(new CallExpression(-2, tagMethod, new ParamListExpression() { new StringLiteral(method.Name) }), false, "Added from C# Project \"EProjectFile\""));
		method.CodeData = block.ToCodeData();
	}
}

using (var projectFileWriter = new ProjectFileWriter(File.Create(fileName)))
{
	foreach (var section in sections)
	{
		switch (section.Name)
		{
			case ESystemInfo.SectionName:
				section.Data = systemInfo.ToBytes();
				break;
			case CodeSectionInfo.SectionName:
				section.Data = codeSectionInfo.ToBytes();
				break;
			case ResourceSectionInfo.SectionName:
				section.Data = resourceSectionInfo.ToBytes();
				break;
			case InitEcSectionInfo.SectionName:
				section.Data = initEcSectionInfo.ToBytes();
				break;
			case EPackageInfo.SectionName:
				section.Data = ePackageInfo.ToBytes();
				break;
			default:
				break;
		}
		projectFileWriter.WriteSection(section);
	}
}
```
