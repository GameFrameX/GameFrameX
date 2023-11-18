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
            return new {className}(CreateGObject(), userData);
        }

        public static UniTask<{className}> CreateInstanceAsync(Entity domain, object userData = null)
        {
            UniTaskCompletionSource<{className}> tcs = new UniTaskCompletionSource<{className}>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(new {className}(go, userData));
            });
            return tcs.Task;
        }
]]

---@type string UI代码模板
local FUITemplate = [[using FairyGUI;
using Cysharp.Threading.Tasks;
using FairyGUI.Utils;
using GameFrameX.Runtime;

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
        public static {className} Create(GObject go, FUI parent = null, object userData = null)
        {
            return new {className}(go, userData, parent);
        }
        /*
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
            fui.isFromFGUIPool = true;
            return fui;
        }
        */

        protected override void InitView()
        {
            if(GObject == null)
            {
                return;
            }

            self = ({superClassName})GObject;
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
__DISPOSE__
            self = null;            
        }

        private {className}(GObject gObject, object userData, FUI parent = null) : base(gObject, parent, userData)
        {
            // Awake(gObject);
        }
    }
}]]
---@type string UI包资源模板
local FUIPackageTemplate = [[using FairyGUI;
using FairyGUI.Utils;
using Cysharp.Threading.Tasks;

namespace {namespaceName}
{
    public static partial class FUIPackage {
__PKGNAMES__
__RES__
    }
}]]

---split
---@param input string
---@param delimiter string
local function split(input, delimiter)
    if type(delimiter) ~= "string" or #delimiter <= 0 then
        return
    end
    local start = 1
    local arr = {}
    while true do
        local pos = string.find(input, delimiter, start, true)
        if not pos then
            break
        end
        table.insert(arr, string.sub(input, start, pos - 1))
        start = pos + string.len(delimiter)
    end
    table.insert(arr, string.sub(input, start))
    return arr
end

---IsCustomComponent 是否是自定义组件
---@param classes table
---@param typeName string
local function IsCustomComponent(classes, typeName)
    local classCnt = classes.Count;
    for i = 0, classCnt - 1 do
        local classInfo = classes[i]
        if typeName == classInfo.className then
            return true;
        end
    end
    return false;
end

local function genCode(handler)
    local settings = handler.project:GetSettings("Publish").codeGeneration
    local codePkgName = handler:ToFilename(handler.pkg.name); -- convert chinese to pinyin, remove special chars etc.
    ---@type string
    local exportCodePath = handler.exportCodePath;
    -- 规范化路径
    exportCodePath = string.gsub(exportCodePath, '\\', '/')
    -- UI组件
    local exportCodeComponentPath = handler.exportCodePath .. '/' .. codePkgName .. '/Components';
    handler:SetupCodeFolder(exportCodeComponentPath, "cs")
    local namespaceName = codePkgName

    if settings.packageName ~= nil and settings.packageName ~= '' then
        namespaceName = settings.packageName .. '.' .. namespaceName;
    end

    namespaceName = "Hotfix.UI";
    if string.find(exportCodePath, "Unity/Assets/Scripts") then
        namespaceName = "Game.Model";
    end
    -- CollectClasses(stripeMemeber, stripeClass, fguiNamespace)
    local classes = handler:CollectClasses(settings.ignoreNoname, settings.ignoreNoname, nil)
    -- check if target folder exists, and delete old files

    local getMemberByName = settings.getMemberByName

    local templeteString = FUITemplate;

    local classCnt = classes.Count
    local writer = CodeWriter.new()
    for i = 0, classCnt - 1 do
        local classInfo = classes[i]
        local members = classInfo.members
        local memberInfoExported = classInfo.res.exported;
        writer:reset()
        fprint(' ' .. tostring(memberInfoExported) .. '  ')
        local genCreateString = CreateTemplate;

        local genCodeString = templeteString;
        --- 处理导出的差异
        if memberInfoExported then
            genCodeString = string.gsub(genCodeString, '__CREATETEMPLATE__', genCreateString)
        else
            genCodeString = string.gsub(genCodeString, '__CREATETEMPLATE__', '')
        end

        genCodeString = string.gsub(genCodeString, '{namespaceName}', namespaceName)
        genCodeString = string.gsub(genCodeString, '{className}', classInfo.className)
        genCodeString = string.gsub(genCodeString, '{superClassName}', classInfo.superClassName)
        genCodeString = string.gsub(genCodeString, '{codePkgName}', handler.pkg.name)
        genCodeString = string.gsub(genCodeString, '{resName}', classInfo.resName)
        genCodeString = string.gsub(genCodeString, '{URL}', 'ui://' .. handler.pkg.id .. classInfo.resId)

        local propertyStr = "";
        local propertyTable = {};
        local AwakeStr = "";
        local AwakeTable = {};
        local DisposeStr = "";
        local disposeTable = {};
        local memberCnt = members.Count
        for j = 0, memberCnt - 1 do
            local memberInfo = members[j]
            local typeName = memberInfo.type
            local varName = memberInfo.varName
            local memberInfoName = memberInfo.name;

            table.insert(propertyTable, '\t\tpublic ' .. typeName .. ' ' .. varName .. ' { get; private set; }');

            if string.find(typeName, "Scene") then
                table.insert(disposeTable, '\t\t\t' .. varName .. '.Dispose();');
            end
            table.insert(disposeTable, '\t\t\t' .. varName .. ' = null;');

            if memberInfo.group == 0 then

                -- 判断是不是自定义类型组件
                if IsCustomComponent(classes, typeName) then
                    table.insert(AwakeTable, '\t\t\t\t' .. varName .. ' = ' .. typeName .. '.Create(com.GetChild("' ..
                        memberInfoName .. '"), this);');
                else
                    table.insert(AwakeTable, '\t\t\t\t' .. varName .. ' = (' .. typeName .. ')com.GetChild("' ..
                        memberInfoName .. '");');
                end
            elseif memberInfo.group == 1 then
                table.insert(AwakeTable, '\t\t\t\t' .. varName .. ' = com.GetController("' .. memberInfoName .. '");');
            else
                table.insert(AwakeTable, '\t\t\t\t' .. varName .. ' = com.GetTransition("' .. memberInfoName .. '");');
            end
        end
        propertyStr = table.concat(propertyTable, '\n')
        DisposeStr = table.concat(disposeTable, '\n')
        AwakeStr = table.concat(AwakeTable, '\n')
        genCodeString = string.gsub(genCodeString, '__PROPERTY__', propertyStr)
        genCodeString = string.gsub(genCodeString, '__AWAKE__', AwakeStr)
        genCodeString = string.gsub(genCodeString, '__DISPOSE__', DisposeStr)
        if string.find(namespaceName, ".Model") then
            genCodeString = string.gsub(genCodeString, '{comB}', '/***')
            genCodeString = string.gsub(genCodeString, '{comE}', '***/')

            genCodeString = string.gsub(genCodeString, '{comAdd}', '')
            -- genCodeString = string.gsub(genCodeString, '{comRemove}', '')

        else
            genCodeString = string.gsub(genCodeString, '{comB}', '')
            genCodeString = string.gsub(genCodeString, '{comE}', '')

            genCodeString = string.gsub(genCodeString, '{comAdd}', '')
            -- genCodeString = string.gsub(genCodeString, '{comRemove}', '')
        end
        writer:writeln(genCodeString);
        writer:save(exportCodeComponentPath .. '/' .. classInfo.className .. '.cs');
    end

    writer:reset()

    local packageTempleteString = FUIPackageTemplate;
    local pkgNamesStr = "";
    packageTempleteString = string.gsub(packageTempleteString, '{namespaceName}', namespaceName)

    pkgNamesStr = pkgNamesStr .. '\t\tpublic const string ' .. codePkgName .. ' = "' .. codePkgName .. '"; \n'
    for i = 0, classCnt - 1 do
        local classInfo = classes[i];
        pkgNamesStr = pkgNamesStr .. '\t\tpublic const string ' .. codePkgName .. '_' .. classInfo.resName ..
                          ' = "ui://' .. codePkgName .. '/' .. classInfo.resName .. '"; \n'
    end
    packageTempleteString = string.gsub(packageTempleteString, '__PKGNAMES__', pkgNamesStr)

    local binderName = 'Package' .. codePkgName;

    local resStr = "";
    if codePkgName == 'UIRes' then
        resStr = "\n\n \t\t/**************** res uri *****************/ \n\n"
        -- local itemsCnt = handler.items.Count;
        -- for i = 0, itemsCnt - 1 do
        --    local item = handler.items[i];
        --    resStr = resStr .. '\t\tpublic const string Res_' .. item.type .. '_' .. item.name .. ' = "' .. item:GetURL() .. '"; \n'
        -- end
    end

    packageTempleteString = string.gsub(packageTempleteString, '__RES__', resStr)
    writer:writeln(packageTempleteString);
    writer:save(exportCodeComponentPath .. '/' .. binderName .. '.cs');
end

return genCode
