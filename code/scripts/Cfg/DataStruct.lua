local Stream = require("Cfg.DataStream")
local find = string.find
local sub = string.sub

local function GetOrCreate(namespace)
    local t = _G
    local idx = 1
    while true do
        local start, ends = find(namespace, ".", idx, true)
        local subname = sub(namespace, idx, start and start - 1)
        local subt = t[subname]
        if not subt then
            subt = {}
            t[subname] = subt
        end
        t = subt
        if start then
            idx = ends + 1
        else
            return t
        end
    end
end

local meta
--类
meta= {}
meta.__index = meta
meta.GREEN = 1 --Const
meta.RED = 2
GetOrCreate('namespace')['name'] = meta
function Stream:GetNamespaceName()
    local o = {}
    setmetatable(o, namespaceName)
    --...解析字段
    --...
    return o
end
--枚举
GetOrCreate('namespace')['name'] = {
    NULL = -1,
    GREEN = 1,
    RED = 2,
}
