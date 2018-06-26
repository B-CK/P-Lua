local lower = string.lower
local setmetatable = setmetatable
local tonumber = tonumber
local format = string.format
local root = Util.DataPath .. '../../../Code/Config/'

local Stream = {}
Stream.__index = Stream
Stream.name = "Stream"

local out = ''

local Split = function(line)
    local sep, fields = "▃", {}
    local pattern = format("([^%s]+)", sep)
    line:gsub(pattern, function(c)
        fields[#fields + 1] = c
    end)
    return fields
end

function Stream.new(dataFile)
    out = ''
    local o = {}
    setmetatable(o, Stream)
    o.dataFile = dataFile
    o.GetLine = io.lines(root .. dataFile)
    o.idx = 0
    o.line = 0
    o:NextRow()
    o.hasNext = true
    return o
end

function Stream:Count()
    return #self.columns
end

function Stream:Close()
    while self.GetLine() do
    end
    print(out)
end

function Stream:NextRow()
    local line = self.GetLine()
    if line == nil then
        return nil
    end
    print("----------------------------", out, line ~= nil)
    self.columns = Split(line)
    self.idx = 1
    self.line = self.line + 1
    out = out .. '\r\n'
end

function Stream:NextColum()
    if self.idx > #self.columns then
        local status = self:NextRow()
        if not status then
            self.hasNext = false
            return nil
        end
    end
    local result = self.columns[self.idx]
    out = out .. "▃" .. result
    print("-->>", self.idx, result, debug.traceback("Data"))
    self.idx = self.idx + 1
    return result
end

function Stream:GetInt()
    local next = self:NextColum()
    return tonumber(next)
end

function Stream:GetLong()
    local next = self:NextColum()
    return tonumber(next)
end

function Stream:GetFloat()
    local next = self:NextColum()
    return tonumber(next)
end

function Stream:GetBool()
    local next = lower(self:NextColum())
    if next == "0" then
        return false
    else
        return true
    end
end

function Stream:GetString()
    return self:NextColum()
end

function Stream:GetList(type)
    local result = {}
    local length = self:GetInt()
    local method = self['Get' .. type]
    print(self.dataFile, self.line, self.idx, length)
    for i = 1, length do
        result[i] = method(self)
    end
    return result
end

function Stream:GetDict(key, value)
    local result = {}
    local optKey = self['Get' .. key]
    local optValue = self['Get' .. value]
    local length = self:GetInt()
    for i = 1, length do
        result[optKey(self)] = optValue(self)
    end
    return result
end

function Stream:GetObject(name)
    local getter = self["Get" .. name]
    return getter(self)
end

return Stream