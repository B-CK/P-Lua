local insert = table.insert
local concat = table.concat
local sort = table.sort
local tostring = tostring
local schar = string.char
local sbyte = string.byte
local find = string.find
local sub = string.sub
local type = type
local pairs = pairs
local ipairs = ipairs
local date = os.date
local print = print
local require = require
local math = math
local getmetatable = getmetatable
local setmetatable = setmetatable
local traceback = debug.traceback
local xpcall = xpcall

local Bit = Bit
local Util = Util

local Utils = {}
local this = Utils

function Utils.SwapValue(t, a, b)
    local temp = t[a]
    t[a] = t[b]
    t[b] = temp
end

function Utils.TableSort(t, cmp)
    for i = 1, #t do
        for j = #t, i + 1, -1 do
            if not cmp(t[j - 1], t[j]) then
                this.SwapValue(t, j, j - 1)
            end
        end
    end
end

function Utils.TableCount(t)
    local count = 0
    for i, k in pairs(t) do
        count = count + 1
    end
    return count
end

function Utils.TableSubCondition(t, condition)
    local t2 = {}
    for i, k in pairs(t) do
        if condition(i, k) then
            t2[i] = k
        end
    end
    return t2
end

function Utils.TableSubCount(t, count)
    local t2 = {}
    for i, k in ipairs(t) do
        if i <= count then
            t2[i] = k
        end
    end
    return t2
end

function Utils.CompareList(list1, list2, cmp)
    if list1 == list2 then
        return true
    else
        if nil == list1 or nil == list2 then
            return false
        elseif table.getn(list1) ~= table.getn(list2) then
            return false
        else
            if cmp then
                this.TableSort(list1, cmp)
                this.TableSort(list2, cmp)
            else
                sort(list1)
                sort(list2)
            end
            for i, value in ipairs(list1) do
                if value ~= list2[i] then
                    Util.Log(string.format("[Utils:CompareList] list1[%s]=[%s] while list2[%s]=[%s], return false!", i, value, i, list2[i]))
                    return false
                end
            end
            return true
        end
    end
end

function Utils.ErrHandler(e)
    Util.LogError(traceback())
end

function Utils.Myxpcall(func, data)
    return xpcall(function()
        func(data)
    end, this.ErrHandler)
end

function Utils.ArrayToSet(t)
    local r = {}
    for _, v in ipairs(t) do
        r[v] = true
    end
    return r
end

function Utils.tolong(s)
    local n = 0
    for i = #s, 1, -1 do
        n = n * 256 + sbyte(s, i)
    end
    return n
end

function Utils.BytesToString (bytes)
    local d = {}
    for i = 1, bytes.Length do
        local x = bytes[i]
        insert(d, schar(x >= 0 and x or x + 256))
    end
    return concat(d)
end

--function Utils.string_to_bytes(s)
--    local bytes = Byte[#s]
--    for i = 1, #s do
--        bytes[i - 1] = sbyte(s, i)
--    end
--    return bytes
--end

--local _test_string = "abcdefg\50\100\200"
--assert(_test_string == bytes_to_string(string_to_bytes(_test_string)))

function Utils.StringTimeFmt(fmt, t)
    return date(fmt, t)
end

function Utils.StringTime(t)
    return date("%Y-%m-%d %H:%M:%S", t)
end

-- 返回 hh:ss
function Utils.StringTimeInDay(t)
    return date("%H:%M:%S", t)
end

function Utils.GetOrCreate(namespace)
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

---@param src table
---@param dst table
function Utils.DeepCopy(src, dst)
    for k, v in pairs(src) do
        if type(v) ~= "table" then
            dst[k] = v
        else
            dst[k] = dst[k] or {}
            this.DeepCopy(v, dst[k])
        end
    end
end

function Utils.ShallowCopy(src, dst)
    for k, v in pairs(src) do
        dst[k] = v
    end
end

function Utils.ClearTable(t)
    if t == nil then
        return
    end
    for k, _ in pairs(t) do
        t[k] = nil
    end
end

function Utils.CopyTable(src)
    local inst = {};
    local k, v;
    for k, v in pairs(src) do
        if type(v) == "table" then
            inst[k] = this.CopyTable(v);
        else
            inst[k] = v;
        end
    end
    local mt = getmetatable(src);
    setmetatable(inst, mt);
    return inst;
end

function Utils.CreateObj(template, obj)
    local inst = obj or {};
    local k, v;
    for k, v in pairs(template) do
        if (not inst[k]) and type(v) ~= "function" then
            if type(v) == "table" and v ~= template then
                inst[k] = this.CopyTable(v);
            end
        end
    end
    setmetatable(inst, template);
    template.__index = template;
    return inst;
end

function Utils.InsidePolygon(polygon, p)
    --    PrintYellow("insidepolygon")
    --    printt(polygon)
    --    PrintYellow("polygon.count:",#polygon)
    local N = #polygon
    local counter = 0
    local i
    local xinters
    local p1
    local p2
    p1 = polygon[1]
    for i = 2, (N + 1) do
        if (i % N == 0) then
            p2 = polygon[i]
        else
            p2 = polygon[i % N]
        end
        if (p.z > math.min(p1.z, p2.z)) then
            if (p.z <= math.max(p1.z, p2.z)) then
                if (p.x <= math.max(p1.x, p2.x)) then
                    if (p1.z ~= p2.z) then
                        xinters = (p.z - p1.z) * (p2.x - p1.x) / (p2.z - p1.z) + p1.x
                        if ((p1.x == p2.x) or (p.x <= xinters)) then
                            counter = counter + 1
                        end
                    end
                end
            end
        end
        p1 = p2
    end
    if (counter % 2 == 0) then
        return false
    else
        return true
    end
end

--判断一个点是否在一个任意多边形内
function Utils.InsideAnyPolygon(polygon, p)
    local count = 0
    local n = #polygon
    local a
    local b
    a = polygon[1]
    for i = 2, n + 1 do
        if (i % n == 0) then
            b = polygon[i]
        else
            b = polygon[i % n]
        end
        if ((a.x <= p.x and p.x <= b.x) or (b.x <= p.x and p.x <= a.x)) then
            local r = (p.x - a.x) * (a.z - b.z) - (p.z - a.z) * (a.x - b.x)
            if (r == 0) then
                -- 在边上
                if (a.x ~= p.x or p.x ~= b.x or (a.z <= p.z and p.z <= b.z) or (b.z <= p.z and p.z <= a.z)) then
                    return true
                end
            elseif (r / (a.x - b.x) > 0) then
                count = count + 1
            end
        end
        a = b
    end
    return (count % 2 == 1)
end

--获取枚举名
function Utils.GetEnumName(enumtype, enumvalue)
    for name, value in pairs(enumtype) do
        if value == enumvalue then
            return name
        end
    end
    return ""
end

--设置粒子特效scale
function Utils.SetParticleSystemScale(gameObject, scale)
    if not gameObject or not scale or scale <= 0 or scale == 1 then
        return
    end

    local allParticleSystem = gameObject:GetComponentsInChildren(UnityEngine.ParticleSystem, true)
    for i = 1, allParticleSystem.Length do
        local particleSystem = allParticleSystem[i]
        if particleSystem then
            particleSystem.startSize = particleSystem.startSize * scale
            particleSystem.startSpeed = particleSystem.startSpeed * scale
        end
    end
end

function Utils.SetDefaultComponent(go, com)
    if go then
        local component = go:GetComponent(com)
        if not component then
            component = go:AddComponent(com)
        end
        return component
    end
    return nil
end

local exceptions_mask

local masks = { 0x00, 0xC0, 0xE0, 0xF0, 0xF8, 0xFC }

local value_masks = { 0x7F, 0x1F, 0x0F, 0x07, 0x03, 0x01 }

function Utils.BinSearch(tb, val)
    local from, to
    from = 1
    to = #tb
    while from <= to do
        local mid = math.floor(from / 2 + to / 2)
        -- PrintYellow(from,to,mid)
        if tb[mid] > val then
            to = mid - 1
        elseif tb[mid] < val then
            from = mid + 1
        else
            return tb[mid]
        end
    end
    return nil
end

function Utils.IsExsception(val)
    if not exceptions_mask then
        exceptions_mask = {}
        local path = LuaHelper.GetPath("config/encode.txt")
        local file = io.open(path, "r")
        if file then
            while true do
                local num = file:read("*number")
                if not num then
                    break
                end
                -- PrintYellow("num = ",num,#exceptions_mask+1)
                table.insert(exceptions_mask, num)
            end
            file:close()
        end
    end
    local ret = this.BinSearch(exceptions_mask, val)
    if ret then
        PrintYellow("IsExsception")
    end
    return ret --bin_search(exceptions_mask,val)
end

function Utils.IsChinese(val)
    --4e00-u9fa5
    local ret = val >= 0x4E00 and val <= 0x9FA5
    if ret then
        PrintYellow("IsChinese")
    end
    return ret
end

local ExpCharacters = { 91, 93, 62, 60, 63, 92, 47, 46, 44, 42, 35, 33, 38, 40, 41, 96, 58, 61, 59 }

function Utils.IsInExpCharacters(c)
    for _, char in ipairs(ExpCharacters) do
        if c == char then
            return true
        end
    end
    return false
end

function Utils.IsCharacter(bt)
    -- PrintYellow("IsCharacter")
    local ret = (bt >= 48 and bt <= 57) or (bt >= 65 and bt <= 90) or (bt >= 97 and bt <= 122) or IsInExpCharacters(bt)
    if ret then
        PrintYellow("IsCharacter")
    end
    return ret
end

function Utils.GetBytesValue(bytes, len)
    local ret = 0
    local msk = 0x3F
    ret = Bit.band(bytes[1], value_masks[len])
    for i = 2, len do
        ret = Bit.lshift(ret, 6)
        ret = Bit.bor(ret, Bit.band(bytes[i], msk))
    end
    return ret
end
-- param:name input
-- return b,info
-- b: type bool, true:legal name,false:illegal name
-- if b == true then info is legal name
-- else info is error info
function Utils.CheckName(name)
    name = string.gsub(name, "%[%a%]", "")
    name = string.gsub(name, "%[%a%a%]", "")
    name = string.gsub(name, "%[%x%x%x%x%x%x%]", "")
    -- read exceptions mask
    local errmgr = require "assistant.errormanager"

    local len = 0
    local k = 1
    while (k <= #name) do
        local bt = string.byte(name, k)
        if not bt then
            break
        end
        if len == 6 then
            exceptions_mask = nil
            return false, errmgr.GetErrorText(2803)
        end
        if this.IsCharacter(bt) then
            len = len + 1
        else
            local code_len = 0
            for i = 6, 1, -1 do
                if Bit.band(masks[i], bt) == masks[i] then
                    code_len = i
                    break
                end
            end
            local bytes = { bt }
            if code_len > 0 then
                -- PrintYellow("code_len",code_len)
                for i = 2, code_len do
                    local sbt = string.byte(name, k + i - 1)
                    table.insert(bytes, sbt)
                end
                local val = this.GetBytesValue(bytes, code_len)
                -- PrintYellow("val = ",val)
                if this.IsChinese(val) or this.IsExsception(val) then
                    k = k + code_len - 1
                    len = len + 1
                else
                    exceptions_mask = nil
                    return false, LocalString.ERR_ILLEAGE_NAME
                end
            else
                exceptions_mask = nil
                return false, LocalString.ERR_WRONG_NAME
            end
        end
        k = k + 1
    end
    exceptions_mask = nil
    return true, name
end

function Utils.Split(str, sep)
    local fields = {}
    local flag = "(.-)" .. sep
    local last_end = 1
    local s, e, cap = str:find(flag, 1)
    while s do
        if s ~= 1 or cap ~= "" then
            insert(fields, cap)
        end
        last_end = e + 1
        s, e, cap = str:find(flag, last_end)
    end
    if last_end <= #str then
        cap = str:sub(last_end)
        insert(fields, cap)
    end
    return fields
end

return Utils
