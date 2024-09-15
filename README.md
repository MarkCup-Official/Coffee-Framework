# Coffee Framework

> 轻量化Unity框架

## 目录

- [简介](#简介)
- [安装与使用](#安装与使用)
- [贡献](#贡献)
- [许可证](#许可证)
- [联系我们](#联系我们)

## 简介

轻量化Unity框架, 即插即用, 可以按需要根据依赖选择性使用部分框架
包含以下模块:
1. GameManager - 一个单例游戏生命周期管理器, 提供GameUpdate和FixedUpdate的调用, `支持暂停功能(Comming soon)`, 需要作为脚本挂载
1. CoffeeBehavior - 继承自MonoBehavior, 自动化在GameManager注册对象
1. `FixedTransform - 支持在FixedUpdate中修改位置并进行插值补帧, 需要作为脚本挂载(Comming soon)`
1. `Setting - 提供设置的保存与读取, 主动向订阅者更新设置信息, 根据预制体生成设置UI, 支持文本输入/数字输入/下拉框/选择框/按键接受(Comming soon)`
1. InputManager - 标准化输入控制, 使用适配器在其中注册后即可提交输入信息, 并提供支持在FixedUpdate中调用的GetKeyDown和GetKeyUp
1. IOManager - 更方便地读取和写入字符串到文件系统, 自动添加persistentDataPath的默认地址
1. `FSM - 有限状态机, 以回调函数的方式执行状态和进行状态切换检查(Comming soon)`
1. `ObjectPool - 对象池, 只要增加一个简单的ICanRestart接口即可注册对象并使用对象池(Comming soon)`
1. `We are keep working`

## 安装与使用

复制项目中的`Assets/Framework`下的文件即可, 可以根据文件开头的依赖项选择性复制个别文件

所有的框架都在名字空间`CoffeeFramework`下, 使用`using CoffeeFramework`来调用模块

根据注释, 部分框架需要一个场景物体作为依赖, 创建空物体并作为脚本挂载即可

大部分的单例模式框架会在第一次调用时自动初始化, 也可以提前通过`Init`手动初始化, 接口中的初始化检查不建议删除

## 贡献

我们欢迎任何人的贡献！请遵循以下步骤：

1. Fork 仓库
2. 创建你的特性分支并作业
5. 提交 Pull Request

## 许可证

此项目使用 MIT 许可证 - 请参阅 [LICENSE](LICENSE) 文件了解更多信息。

## 联系我们

如果你有任何问题或建议，请联系我们：

- 电子邮件: 2742586749@qq.com
- 项目链接: [Coffee Framework](https://github.com/MarkCup-Official/Coffee-Framework)
