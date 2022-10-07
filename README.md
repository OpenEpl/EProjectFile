***Only Chinese document is available.***

# EProjectFile
一个用于编辑易语言项目文件的第三方库（C#）  
[![NuGet](https://img.shields.io/nuget/v/QIQI.EProjectFile.svg)](https://www.nuget.org/packages/QIQI.EProjectFile)  
命名空间：`QIQI.EProjectFile`  

## 安装方式
Package Manager `Install-Package QIQI.EProjectFile`  
.NET CLI `dotnet add package QIQI.EProjectFile`  
使用 VS 的 NuGet 包管理器（GUI）搜索 `QIQI.EProjectFile`  
支持：.NET Core 2.0 或更高（已经不再支持在 XP 环境下运行）

## 版权相关
- 本项目是贡献到 **公共领域（Public Domain）** 的自由、无限制的程序库
- 在法律许可的范围内，任何人可以出于任何用途（非商业用途或商业用途等）、以任何形式（源代码或编译好的二进制文件等）、任何使用方式（复制、修改、发布、使用、分发、售卖等）使用本项目而不受到限制。作者自愿永久放弃一切（现有的或未来的）由各国版权法与用于版权保护的国际公约提供的权利，将本项目奉献于公共领域，并清楚地认识到这可能会损害作者及其继承者本可获得的利益。**但请注意，对于因您的不当使用而可能产生的对其他主体的侵权行为（包括但不限于因使用本项目所依赖的第三方程序库而可能造成的风险、将本项目用于侵害他人合法权益的用途而产生的纠纷等），需由您自行承担相关责任**
- 本项目以原样提供，不作任何形式（明示或暗示）的保证，包括但不限于对适销性、特定用途适用性和非侵权性的保证。在任何情况下，如合同诉讼、侵权诉讼或其他诉讼中，作者或版权持有人**均不承担**因本项目或本项目的使用、交易或其他行为而产生的、引起的或因其它任何原因与之相关的任何责任（包括但不限于索赔、损害等）。

## 项目状态
本项目属于**实验性项目**，所有 API 随时可能发生破坏性的变更（breaking changes）  
由于可能存在的程序错误，本项目可能造成被操作文件的数据丢失、损坏，请在进行操作前提前备份。  
当前项目的完成度可用于从 *.e 文件中提取出所有代码信息和少量编辑信息，并进行代码编辑，  
可用于 文本代码互转、EplDiff、甚至自行实现编译器等场景。  
### 数据段支持情况

| 段标识 | 段名称 | 本项目使用的类名 | 备注 |
| ---- | ---- | ---- | ---- |
| `0x01007319` | 用户信息段 | `ProjectConfigSection` |
| `0x02007319` | 系统信息段 | `ESystemInfoSection` |
| `0x03007319` | 程序段 | `CodeSection` |
| `0x04007319` | 程序资源段 | `ResourceSection` |
| `0x05007319` | 可丢失程序段 | `LosableSection` | 只支持解析部分数据 |
| `0x06007319` | *\<Unknown\>* | *Not supported* | 新版易语言已经不会写出此段 |
| `0x07007319` | *(empty)* | `EndOfFileSection` |
| `0x08007319` | 初始模块段 | `InitECSection` |
| `0x09007319` | 编辑信息段2 | `EditorInfoSection` |
| `0x0A007319` | 辅助信息段1 | `EventIndicesSection` |
| `0x0B007319` | 辅助信息段2 | `ClassPublicitySection` |
| `0x0C007319` | 易模块记录段 | `ECDependenciesSection` |
| `0x0D007319` | 易包信息段1 | `EPackageInfoSection` |
| `0x0E007319` | 编辑过滤器信息段 | `FolderSection` |
| `0x0F007319` | 易模块记录段2 | *Not supported* | 当引用加密模块时，此段用于记忆密码 |
| `0x10007319` | 辅助信息段3 | `ProjectConfigExSection` |
| `0x11007319` | 编译条件信息段 | `ConditionalCompilationSection` |

### 代码解析与生成

| 特性名称 | 完成情况 |
| ---- | :----: |
| 代码数据中语义部分的解析与生成 | ✔️ |
| 代码数据中注释部分的解析与生成 | ✔️ |
| 处理 屏蔽 标志 | ✔️ |
| 处理 书签 标志 | ❌ |
| 处理 折叠 标志 | ❌ |
| 处理 展开 标志 | ❌ |

## 交流途径
一般的 bug 反馈 与 feature 请求，请用 GitHub 的 Issues 模块反馈  
如果您希望对本项目做出贡献，请使用标准 GitHub 工作流：Fork + Pull request  
进一步的快速讨论：请加入 QQ 群 605310933 *（注意不要在群中反馈 bug，这很可能导致反馈没有被记录。聊天消息较 Issues 模块比较混乱）*  

## 编译方式
使用 NuGet 安装缺失包，然后按照一般流程编译即可  
注意：**必须 VS2017 或更高版本，使用了最新语言特性**  

## 使用例程
可以参考 [OpenEpl/InjectedEComRepair](https://github.com/OpenEpl/InjectedEComRepair) 项目

## 特别感谢
本项目的发展得益于开源社区中许多贡献者的支持，这里仅列举部分人员用于表示感谢，*请注意该列表并非全部贡献者*

| 用户 | 成就 |
| ---- | ---- |
| 东灿 | [最早开源的项目文件结构分析代码](https://bbs.125.la/forum.php?mod=viewthread&tid=13751690) |
| 曙光 | 最早开源的代码数据解析软件 |
| 为你芯冻 | [e.net](https://github.com/wnxd/e.net)（包含一定的项目解析代码） |
| JimStone（谢栋） | 支持库兼容性信息分析 [`80348c3`](https://github.com/OpenEpl/EProjectFile/commit/80348c3e42d775c1b2f2c45af699356c46b3503d) |
| @clhhz | 发现 MemoryAddress 字段 \([#2](https://github.com/OpenEpl/EProjectFile/pull/2)\) |