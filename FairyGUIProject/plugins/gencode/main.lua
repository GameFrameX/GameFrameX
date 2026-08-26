--FYI: https://github.com/Tencent/xLua/blob/master/Assets/XLua/Doc/XLua_Tutorial_EN.md

local genCode = require(PluginPath .. '/GenCode_CSharp')

function onPublish(publishHandler)

    fprint(' 导出前目录 ' .. publishHandler.exportPath)
    publishHandler.exportPath = publishHandler.exportPath .. '/' .. publishHandler.pkg.name
    local isExists = CS.System.IO.Directory.Exists(publishHandler.exportPath)
    if not isExists then
        CS.System.IO.Directory.CreateDirectory(publishHandler.exportPath)
    end

    fprint(' 导出后目录 ' .. publishHandler.exportPath)

    if not publishHandler.genCode then
        return
    end
    publishHandler.genCode = false --prevent default output

    genCode(publishHandler)
end

function onDestroy()
    -------do cleanup here-------
end