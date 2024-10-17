local CreateTemplate = [[
        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static {className} CreateInstance(object userData = null)
        {
            return Create(CreateGObject(), userData);
        }

        public static UniTask<{className}> CreateInstanceAsync(Entity domain, object userData = null)
        {
            UniTaskCompletionSource<{className}> tcs = new UniTaskCompletionSource<{className}>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go, userData));
            });
            return tcs.Task;
        }
]]

---@type string UI代码模板
local FUITemplate = [[using FairyGUI;
using Cysharp.Threading.Tasks;
using FairyGUI.Utils;
using GameFrameX.Entity.Runtime;
using GameFrameX.UI.Runtime;
using GameFrameX.UI.FairyGUI.Runtime;
using GameFrameX.Runtime;
using UnityEngine;

namespace {namespaceName}
{
    public sealed partial class {className} : FUI
    {
        public const string UIPackageName = "{codePkgName}";
        public const string UIResName = "{resName}";
        public const string URL = "{URL}";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public {superClassName} self { get; private set; }

__PROPERTY__

__CREATETEMPLATE__
        public static {className} Create(GObject go, object userData = null)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<{className}>();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static {className} GetFormPool(GObject go)
        {
            var fui =  go.Get<{className}>();
            if(fui == null)
            {
                fui = Create(go);
            }
            fui.IsFromPool = true;
            return fui;
        }

        protected override void InitView()
        {
            if(GObject == null)
            {
                return;
            }

            self = ({superClassName})GObject;
            self.Add(this);
            {comAdd}
            var com = GObject.asCom;
            if(com != null)
            {
__AWAKE__
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose();
            self.Remove();
__DISPOSE__
            self = null;            
        }

        private {className}(GObject gObject, object userData) : base(gObject, userData)
        {
            // Awake(gObject);
        }
    }
}]]
---@type string UI包资源模板
local FUIPackageTemplate = [[using FairyGUI;
using FairyGUI.Utils;
using Cysharp.Threading.Tasks;
using GameFrameX.Entity.Runtime;

namespace {namespaceName}
{
    public static partial class FUIPackage {
__PKGNAMES__
__RES__
    }
}]]

--- 将输入字符串按指定分隔符分割成数组
---@param input: 输入的字符串
---@param delimiter: 分隔符
local function split(input, delimiter)
    -- 检查分隔符是否为字符串且长度大于0
    if type(delimiter) ~= "string" or #delimiter <= 0 then
        return -- 如果不是，返回空
    end
    local start = 1 -- 定义起始位置
    local arr = {} -- 创建空数组
    while true do -- 进入循环
        local pos = string.find(input, delimiter, start, true) -- 查找分隔符位置
        if not pos then -- 如果没有找到
            break -- 退出循环
        end
        table.insert(arr, string.sub(input, start, pos - 1)) -- 将分隔符之前的部分插入数组
        start = pos + string.len(delimiter) -- 更新起始位置
    end
    table.insert(arr, string.sub(input, start)) -- 插入剩余部分到数组
    return arr -- 返回数组
end

--- 判断是否为自定义组件
--- @param classes table 包含类信息的表
--- @param typeName string 待检查的类型名称
--- @return boolean 如果是自定义组件则返回true，否则返回false
local function IsCustomComponent(classes, typeName)
    local classCnt = classes.Count; -- 获取类的数量
    for i = 0, classCnt - 1 do -- 遍历类表
        local classInfo = classes[i] -- 获取当前类信息
        if typeName == classInfo.className then -- 检查类型名称是否匹配
            return true; -- 如果匹配则返回true
        end
    end
    return false; -- 类型名称未匹配则返回false
end

--- 生成代码
--- @param handler table 处理程序
local function genCode(handler)
    local settings = handler.project:GetSettings("Publish").codeGeneration -- 获取发布设置中的代码生成选项
    local codePkgName = handler:ToFilename(handler.pkg.name); -- 将包名转换为文件名格式（拼音），去除特殊字符等
    ---@type string
    local exportCodePath = handler.exportCodePath; -- 导出代码路径
    -- 规范化路径
    exportCodePath = string.gsub(exportCodePath, '\\', '/') -- 将路径中的反斜杠替换为斜杠
    -- UI组件
    local exportCodeComponentPath = handler.exportCodePath .. '/' .. codePkgName .. '/Components'; -- 导出代码的组件路径
    handler:SetupCodeFolder(exportCodeComponentPath, "cs") -- 设置代码文件夹
    local namespaceName = codePkgName -- 命名空间名称

    if settings.packageName ~= nil and settings.packageName ~= '' then
        namespaceName = settings.packageName .. '.' .. namespaceName; -- 如果设置中有包名，则使用设置中的包名
    end

    namespaceName = "Hotfix.UI"; -- 命名空间名称为"Hotfix.UI"
    if string.find(exportCodePath, "Unity/Assets/Scripts") then
        namespaceName = "Unity.Startup"; -- 如果导出路径包含"Unity/Assets/Scripts"，则命名空间名称为"Unity.Startup"
    end
    -- CollectClasses(stripeMemeber, stripeClass, fguiNamespace)
    local classes = handler:CollectClasses(settings.ignoreNoname, settings.ignoreNoname, nil) -- 收集类信息（忽略无名称的类）
    -- 检查目标文件夹是否存在，并删除旧文件

    local templeteString = FUITemplate; -- 模板字符串

    local classCnt = classes.Count -- 类的数量
    local writer = CodeWriter.new() -- 代码写入器
    for i = 0, classCnt - 1 do
        local classInfo = classes[i] -- 获取类信息
        local members = classInfo.members -- 成员变量
        local memberInfoExported = classInfo.res.exported; -- 成员信息是否导出
        writer:reset() -- 重置写入器

        local genCreateString = CreateTemplate; -- 生成创建字符串

        local genCodeString = templeteString; -- 生成代码字符串
        --- 处理导出的差异
        if memberInfoExported then
            genCodeString = string.gsub(genCodeString, '__CREATETEMPLATE__', genCreateString) -- 如果成员信息导出，则替换模板字符串中的__CREATETEMPLATE__
        else
            genCodeString = string.gsub(genCodeString, '__CREATETEMPLATE__', '') -- 否则移除__CREATETEMPLATE__
        end

        genCodeString = string.gsub(genCodeString, '{namespaceName}', namespaceName) -- 替换命名空间名称
        genCodeString = string.gsub(genCodeString, '{className}', classInfo.className) -- 替换类名
        genCodeString = string.gsub(genCodeString, '{superClassName}', classInfo.superClassName) -- 替换父类名
        genCodeString = string.gsub(genCodeString, '{codePkgName}', handler.pkg.name) -- 替换包名
        genCodeString = string.gsub(genCodeString, '{resName}', classInfo.resName) -- 替换资源名
        genCodeString = string.gsub(genCodeString, '{URL}', 'ui://' .. handler.pkg.id .. classInfo.resId) -- 替换资源URL

        local propertyStr = ""; -- 属性字符串
        local propertyTable = {}; -- 属性表
        local AwakeStr = ""; -- Awake字符串
        local AwakeTable = {}; -- Awake表
        local DisposeStr = ""; -- Dispose字符串
        local disposeTable = {}; -- Dispose表
        local memberCnt = members.Count -- 成员数量
        for j = 0, memberCnt - 1 do
            local memberInfo = members[j] -- 获取成员信息
            local typeName = memberInfo.type -- 类型名称
            local varName = memberInfo.varName -- 变量名
            local memberInfoName = memberInfo.name; -- 成员信息名称
            -- 这里查找组件是否是其他包里的资源
            local isWhetherToCrossPackages = false; -- 是否跨包引用
            if memberInfo.res ~= nil then
                if memberInfo.res.owner.name ~= handler.pkg.name then -- 判断是不是本包的资源
                    local memberResName = memberInfo.res.name; -- 获取跨包的组件名称
                    if IsCustomComponent(classes, typeName) then -- 判断是不是自定义类型组件
                    typeName = memberResName; -- 替换类型名称为跨包的组件名称
                    end
                    isWhetherToCrossPackages = true;
                end
            end

            table.insert(propertyTable, '\t\tpublic ' .. typeName .. ' ' .. varName .. ' { get; private set; }'); -- 插入属性表

            if string.find(typeName, "Scene") then
                table.insert(disposeTable, '\t\t\t' .. varName .. '.Dispose();'); -- 如果类型名称中包含"Scene"，则插入Dispose表
            end
            table.insert(disposeTable, '\t\t\t' .. varName .. ' = null;'); -- 插入Dispose表

            if memberInfo.group == 0 then
                -- 判断是不是自定义类型组件 和跨包引用
                if IsCustomComponent(classes, typeName) then
                    table.insert(AwakeTable, '\t\t\t\t' .. varName .. ' = ' .. typeName .. '.Create(com.GetChild("' ..
                        memberInfoName .. '"), this);'); -- 如果是自定义类型组件且是跨包引用，则插入Awake表
                else
                    table.insert(AwakeTable, '\t\t\t\t' .. varName .. ' = (' .. typeName .. ')com.GetChild("' ..
                        memberInfoName .. '");'); -- 否则插入Awake表
                end
            elseif memberInfo.group == 1 then
                table.insert(AwakeTable, '\t\t\t\t' .. varName .. ' = com.GetController("' .. memberInfoName .. '");'); -- 插入Awake表
            else
                table.insert(AwakeTable, '\t\t\t\t' .. varName .. ' = com.GetTransition("' .. memberInfoName .. '");'); -- 插入Awake表
            end
        end
        propertyStr = table.concat(propertyTable, '\n') -- 属性字符串连接
        DisposeStr = table.concat(disposeTable, '\n') -- Dispose字符串连接
        AwakeStr = table.concat(AwakeTable, '\n') -- Awake字符串连接
        genCodeString = string.gsub(genCodeString, '__PROPERTY__', propertyStr) -- 替换属性字符串
        genCodeString = string.gsub(genCodeString, '__AWAKE__', AwakeStr) -- 替换Awake字符串
        genCodeString = string.gsub(genCodeString, '__DISPOSE__', DisposeStr) -- 替换Dispose字符串
        if string.find(namespaceName, ".Model") then
            genCodeString = string.gsub(genCodeString, '{comB}', '/***') -- 替换字符串
            genCodeString = string.gsub(genCodeString, '{comE}', '***/') -- 替换字符串

            genCodeString = string.gsub(genCodeString, '{comAdd}', '') -- 替换字符串
            -- genCodeString = string.gsub(genCodeString, '{comRemove}', '') -- 替换字符串

        else
            genCodeString = string.gsub(genCodeString, '{comB}', '') -- 替换字符串
            genCodeString = string.gsub(genCodeString, '{comE}', '') -- 替换字符串

            genCodeString = string.gsub(genCodeString, '{comAdd}', '') -- 替换字符串
            -- genCodeString = string.gsub(genCodeString, '{comRemove}', '') -- 替换字符串
        end
        writer:writeln(genCodeString); -- 写入生成的代码字符串
        writer:save(exportCodeComponentPath .. '/' .. classInfo.className .. '.cs'); -- 保存生成的代码文件
    end

    writer:reset() -- 重置写入器

    local packageTempleteString = FUIPackageTemplate; -- 包模板字符串
    local pkgNamesStr = ""; -- 包名字符串
    local pkgNamesTable = {}; -- 包名表
    packageTempleteString = string.gsub(packageTempleteString, '{namespaceName}', namespaceName) -- 替换命名空间名称

    table.insert(pkgNamesTable, '\t\tpublic const string ' .. codePkgName .. ' = "' .. codePkgName .. '";'); -- 插入包名表
    for i = 0, classCnt - 1 do
        local classInfo = classes[i];
        table.insert(pkgNamesTable,
            '\t\tpublic const string ' .. codePkgName .. '_' .. classInfo.resName .. ' = "ui://' .. codePkgName .. '/' ..
                classInfo.resName .. '";'); -- 插入包名表
    end
    pkgNamesStr = table.concat(pkgNamesTable, '\n'); -- 包名字符串连接
    packageTempleteString = string.gsub(packageTempleteString, '__PKGNAMES__', pkgNamesStr) -- 替换包名字符串

    local binderName = 'Package' .. codePkgName; -- 绑定器名称

    local resStr = ""; -- 资源字符串
    if codePkgName == 'UIRes' then
        resStr = "\n\n \t\t/**************** res uri *****************/ \n\n" -- 如果包名为'UIRes'，则添加资源URI注释
        -- local itemsCnt = handler.items.Count;
        -- for i = 0, itemsCnt - 1 do
        --    local item = handler.items[i];
        --    resStr = resStr .. '\t\tpublic const string Res_' .. item.type .. '_' .. item.name .. ' = "' .. item:GetURL() .. '"; \n'
        -- end
    end

    packageTempleteString = string.gsub(packageTempleteString, '__RES__', resStr) -- 替换资源字符串
    writer:writeln(packageTempleteString); -- 写入包模板字符串
    writer:save(exportCodeComponentPath .. '/' .. binderName .. '.cs'); -- 保存包绑定器文件
end

return genCode
