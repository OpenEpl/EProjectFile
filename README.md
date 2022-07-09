***Only Chinese document is available.***

# EProjectFile
一个用于编辑易语言项目文件的第三方库（C#）  
[![NuGet](https://img.shields.io/nuget/v/QIQI.EProjectFile.svg)](https://www.nuget.org/packages/QIQI.EProjectFile)  
命名空间：`QIQI.EProjectFile`  

# 安装
Package Manager `Install-Package QIQI.EProjectFile`  
.NET CLI `dotnet add package QIQI.EProjectFile`  
使用 VS 的 NuGet 包管理器（GUI）搜索 `QIQI.EProjectFile`  
支持：.NET Core 2.0 或更高（已经不再支持在 XP 环境下运行）

# 声明
1. 仅供学习与交流使用
2. 本项目属于**实验性项目**，所有 API 随时变更
3. 本项目随时终止，不做任何可用承诺
4. 对于使用本项目造成的一切损失，概不负责
5. 本项目可能造成被操作文件的数据丢失，请提前备份
6. 禁止用于非法用途，包括但不限于：破解他人软件
7. **本项目目前处于起步阶段，基本只提供底层解析/生成支持，功能极不完善**

# 交流
一般 bug 请用 Github 的 Issues 模块反馈  
如果您希望对本项目做出贡献，请使用标准 GitHub 工作流：Fork + Pull request  
进一步的快速讨论：请加入 QQ 群 605310933 *（注意不要在群中反馈 bug，这很可能导致反馈没有被记录。聊天消息较 Issues 模块比较混乱）*  

# 编译
使用 NuGet 安装缺失包，然后一般编译即可  
注意：**必须 VS2017 或更高版本，使用了最新语言特性**  

# 例程
可以参考 [OpenEpl/InjectedEComRepair](https://github.com/OpenEpl/InjectedEComRepair) 项目

# 感谢
本项目的发展得益于开源社区中许多贡献者的支持，这里仅列举部分人员用于表示感谢，*请注意该列表并非全部贡献者*
| 用户 | 成就 |
| ---- | ---- |
| 东灿 | [最早开源的项目文件结构分析代码](https://bbs.125.la/forum.php?mod=viewthread&tid=13751690) |
| 曙光 | 最早开源的代码数据解析软件 |
| 为你芯冻 | [e.net](https://github.com/wnxd/e.net)（包含一定的项目解析代码） |
| JimStone（谢栋） | 支持库兼容性信息分析 [`80348c3`](https://github.com/OpenEpl/EProjectFile/commit/80348c3e42d775c1b2f2c45af699356c46b3503d) |
| @clhhz | 发现 MemoryAddress 字段 (#2) |