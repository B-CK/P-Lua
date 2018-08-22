local type = type
local debug = debug
local print = print
local pairs = pairs
local ipairs = ipairs
local tostring = tostring
local rep = string.rep
local format = string.format
local concat = table.concat
local insert = table.insert
local Util = Util
local Local = Local
local GameObject = GameObject

--输出日志--
function Log(str)
    if not Local.LogManager then
        return
    end
    if Local.LogTraceback then
        Util.Log(str .. "\r\n" .. debug.traceback());
    else
        Util.Log(str);
    end
end

--错误日志--
function LogError(str)
    if not Local.LogManager then
        return
    end
    if Local.LogTraceback then
        Util.LogError(str .. "\r\n" .. debug.traceback());
    else
        Util.LogError(str)
    end
end

--警告日志--
function LogWarning(str)
    if not Local.LogManager then
        return
    end
    if Local.LogTraceback then
        Util.LogWarning(str .. "\r\n" .. debug.traceback());
    else
        Util.LogWarning(str)
    end
end

--输出黄色日志-无格式
function LogYellow(...)
    if not Local.LogManager then
        return
    end
    local args = { ... }
    for k, v in ipairs(args) do
        args[k] = tostring(v)
    end

    if Local.LogTraceback then
        Util.Log("<color=yellow>" .. concat(args, '\t') .. "</color>\t\n" .. debug.traceback());
    else
        Util.Log("<color=yellow>" .. concat(args, '\t') .. "</color>\t\n");
    end
end

--输出黄色日志
function PrintYellow(...)
    if not Local.LogManager then
        return
    end
    local args = { ... }
    for k, v in ipairs(args) do
        args[k] = tostring(v)
    end

    if Local.LogTraceback then
        print("<color=yellow>" .. concat(args, '\t') .. "</color>\t\n" .. debug.traceback());
    else
        print("<color=yellow>" .. concat(args, '\t') .. "</color>\t\n");
    end
end

local function DumpTable(t, level)
    local code = { "{\n" }
    local tab = rep("\t", level)
    for k, v in pairs(t) do
        if type(v) ~= "table" then
            insert(code, tab .. "\t<color=orange>" .. tostring(k) .. "</color> = " .. tostring(v) .. ",\n")
        else
            insert(code, tab .. "\t<color=orange>" .. tostring(k) .. "</color> = " .. DumpTable(v, level + 1) .. ",\n")
        end
    end
    insert(code, tab .. "}")
    return concat(code)
end

---@param t table类型
---@param des table描述信息
function PrintTable(t, des)
    if not Local.LogManager then
        return
    end
    if type(t) == "table" then
        print(format("<color=orange>%s</color>\n%s", des, DumpTable(t, 0)))
    else
        print(des .. '\n' .. t)
    end
end

--查找对象--
function FindObj(str)
    return GameObject.Find(str);
end

function Destroy(obj)
    GameObject.Destroy(obj);
end

function NewObject(prefab)
    return GameObject.Instantiate(prefab);
end

--创建面板--
function CreatePanel(name)
    --PanelManager:CreatePanel(name);
end

function FindPanel(str)
    local obj = FindObj(str);
    if obj == nil then
        error(str .. " is null");
        return nil;
    end
    return obj:GetComponent("BaseLua");
end

---@param a 判定条件
---@param b true返回b
---@param c false返回c
function TernaryOperation(a, b, c)
    if a then
        return b
    else
        return c
    end
end