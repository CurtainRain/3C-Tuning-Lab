# Unity 3C 系统架构说明

## 架构概述

这是一个**解耦的 3C（Camera, Character, Control）系统**，采用**输入层与逻辑层分离**的设计模式。

### 核心设计理念

1. **输入层（Input Layer）**：负责读取原始输入
2. **数据层（Data Layer）**：使用 `InputData` 结构体传递输入数据
3. **逻辑层（Logic Layer）**：角色控制器和摄像机控制器处理游戏逻辑

### 架构优势

✅ **解耦**：输入读取和游戏逻辑完全分离
✅ **可扩展**：轻松添加新的输入源（手柄、触摸屏等）
✅ **可测试**：可以模拟输入数据进行单元测试
✅ **可维护**：各组件职责清晰，易于维护

---

## 文件结构

```
Assets/Scripts/
├── Input/
│   ├── IInputProvider.cs          # 输入提供者接口
│   ├── InputData.cs                # 输入数据结构
│   ├── KeyboardMouseInputProvider.cs  # 键盘鼠标输入实现
│   └── InputHandler.cs             # 输入处理器（桥梁）
├── Character/
│   └── CharacterController3C.cs   # 角色控制器
└── Camera/
    └── CameraController3C.cs      # 摄像机控制器
```

---

## 使用步骤

### 1. 设置角色（Player）

1. 创建一个 GameObject，命名为 "Player"
2. 添加 `CharacterController` 组件（Unity 内置）
3. 添加 `KeyboardMouseInputProvider` 组件
4. 添加 `InputHandler` 组件
5. 添加 `CharacterController3C` 组件
6. 设置 Tag 为 "Player"（可选，用于摄像机自动查找）

**组件配置：**
- `InputHandler` → Input Provider: 拖入 `KeyboardMouseInputProvider` 组件
- `CharacterController3C` → 根据需要调整移动速度、跳跃高度等参数

### 2. 设置摄像机（Main Camera）

1. 选择场景中的 Main Camera
2. 添加 `CameraController3C` 组件
3. 设置 Target 为 Player 对象
4. 调整 Offset（建议：X=0, Y=1.6, Z=0）

### 3. 设置地面

1. 创建一个 Plane 或使用地形
2. 确保地面有 Collider
3. 在 `CharacterController3C` 中设置 Ground Mask，确保能检测到地面

---

## 组件说明

### IInputProvider（接口）

定义输入提供者的标准接口，可以创建不同的实现：
- `KeyboardMouseInputProvider`：键盘鼠标输入
- `GamepadInputProvider`：手柄输入（可扩展）
- `TouchInputProvider`：触摸屏输入（可扩展）

### InputData（数据结构）

包含所有输入信息：
- `moveInput`：移动方向（归一化向量）
- `lookInput`：视角旋转（鼠标/右摇杆）
- `jumpPressed`：跳跃按下
- `sprintHeld`：冲刺按住
- `interactPressed`：交互按下

### InputHandler（输入处理器）

- 收集所有输入提供者的数据
- 转换为 `InputData` 结构
- 通过事件通知其他组件（`OnInputUpdated`）

### CharacterController3C（角色控制器）

- 处理移动逻辑
- 处理跳跃逻辑
- 处理重力
- **不直接读取输入**，而是订阅 `InputHandler` 的事件

### CameraController3C（摄像机控制器）

- 处理摄像机跟随
- 处理视角旋转
- **不直接读取输入**，而是订阅 `InputHandler` 的事件

---

## 扩展指南

### 添加手柄支持

1. 创建新脚本 `GamepadInputProvider.cs`
2. 实现 `IInputProvider` 接口
3. 使用 Unity 的 Input System 读取手柄输入
4. 在 `InputHandler` 中选择使用 `GamepadInputProvider` 而不是 `KeyboardMouseInputProvider`

### 添加新的输入动作

1. 在 `IInputProvider` 接口中添加新方法
2. 在 `InputData` 结构中添加新字段
3. 在所有输入提供者实现中添加对应逻辑
4. 在 `InputHandler` 中收集新输入
5. 在角色/摄像机控制器中使用新输入

---

## 最佳实践

1. **不要**在角色控制器中直接使用 `Input.GetAxis()`
2. **不要**在摄像机控制器中直接使用 `Input.GetAxis()`
3. **使用** `InputHandler` 作为唯一的输入入口
4. **使用**事件系统进行组件间通信
5. **保持**输入层和逻辑层的分离

---

## 示例场景设置

```
Scene Hierarchy:
├── Player (GameObject)
│   ├── CharacterController
│   ├── KeyboardMouseInputProvider
│   ├── InputHandler
│   └── CharacterController3C
├── Main Camera
│   └── CameraController3C
└── Ground (Plane with Collider)
```

---

## 故障排除

### 角色不移动
- 检查 `InputHandler` 是否正确连接到 `KeyboardMouseInputProvider`
- 检查 `CharacterController3C` 是否订阅了 `InputHandler` 的事件
- 检查 `CharacterController` 组件是否存在

### 摄像机不跟随
- 检查 `CameraController3C` 的 Target 是否设置
- 检查 `CameraController3C` 是否订阅了 `InputHandler` 的事件

### 无法跳跃
- 检查地面检测设置（Ground Check、Ground Distance、Ground Mask）
- 确保角色有 `CharacterController` 组件
