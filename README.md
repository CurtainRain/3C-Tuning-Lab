## 3C Tuning Lab - 第三人称相机调参工具链

### 项目背景

为解决3C参数调优效率低、版本回归测试困难的问题，开发了一套完整的调参工具链，
支持参数预设切换、输入录制回放、运动数据分析等功能。

### 核心技术实现

#### 1. 输入系统解耦架构

- **技术方案**：设计 IInputProvider 接口，实现输入层与逻辑层完全分离
- **架构设计**：KeyboardMouseInputProvider → PlayerInputHandler → CharacterController/CameraController
- **核心价值**：通过输入数据拦截，实现零侵入式录制回放，支持以下场景：
  - 录制玩家操作序列，用于bug复现
  - 参数调整后回放相同输入，验证手感差异
  - 版本回归测试，确保参数修改不破坏原有体验

#### 2. 渲染帧/逻辑帧分离

- **问题分析**：物理计算在FixedUpdate（固定50Hz），渲染在Update（不固定帧率），
  直接应用物理结果会导致低帧率下画面抖动
- **解决方案**：
  - FixedUpdate 计算逻辑位置（lastPosition → currentPosition）
  - Update 根据时间比例插值渲染位置（Lerp/Slerp）

#### 3. 智能相机避障系统

- **分层碰撞策略**：
  - 普通物体（Default/Ground）：SphereCast检测 + 强制拉近
  - 纤细物体（Thin层）：RaycastAll精确检测 + 选择性穿透
- **平滑处理**：
  - 发生碰撞且相机在遮挡内：直接赋值（避免穿模）
  - 发生碰撞但相机在外部：插值拉近（保持平滑）
- **性能考虑**：LayerMask查询、碰撞过滤、检测距离控制

#### 4. 数据驱动的调参系统

- **预设管理**：ScriptableObject存储参数
- **Editor扩展**：自定义Inspector面板
- **数据导出**：CSV格式导出运动数据（位置、速度、旋转、输入），支持Excel分析
- **工作流程**：调参 → F1录制 → F2回放 → 导出CSV → 数据分析 → 迭代优化

### 使用说明

#### 操作流程

1. **调整参数**：在初始场景（AppInitialScene）- GameRuntimeContext 中选择不同的参数预设，或实时调整参数
2. **录制输入**：按 `F1` 开始录制玩家输入，再次按 `F1` 停止录制
3. **回放验证**：按 `F2` 开始回放录制的输入，验证参数修改效果
4. **数据分析**：回放时自动导出CSV文件，用于量化分析

#### 输出文件路径

**CSV数据文件：**

- **路径**：`Application.persistentDataPath/MovementRecords/Record_{预设名称}.csv`
- **Windows示例**：`C:/Users/{用户名}/AppData/LocalLow/{公司名}/{项目名}/MovementRecords/Record_DefaultParams_CameraParams.csv`
- **Mac示例**：`~/Library/Application Support/{公司名}/{项目名}/MovementRecords/Record_DefaultParams_CameraParams.csv`

**输入录制文件：**

- **路径**：`Application.persistentDataPath/Records/Record.txt`
- **格式**：JSON格式，包含输入序列和初始状态

### 技术特点

- 架构解耦：输入层、数据层、逻辑层职责清晰，易于扩展和测试
- 可复现性：固定时间步 + 输入录制，确保回放一致性
- 工具化思维：完整的调参工作流，提升策划/TA的迭代效率
- 性能意识：帧分离、碰撞优化、数据缓存

### 技术栈

Unity3D、C#、CharacterController、Physics、ScriptableObject、Custom Editor、
FixedUpdate/Update、LayerMask、SphereCast、CSV数据处理

### 应用价值

- 策划调参效率提升：从"改代码-编译-测试"变为"调Slider-实时预览"
- 版本回归测试：参数修改后可用历史输入数据验证影响
- Bug复现：记录玩家操作序列，可有效复现问题场景
- 数据分析：导出CSV后可用于分析运动曲线等

### 已知限制 / 未来可扩展方向

- 回放基于固定逻辑帧 + 输入序列，长时间运行存在轻微浮点误差，但关键事件和运动模式可稳定复现。
- 目前以单人调参和技术验证为主要使用场景，尚未针对策划/TA的使用做深入的产品化打磨（如多录制管理、更完善的可视化界面等）
