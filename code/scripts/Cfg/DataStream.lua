local lower = string.lower
local setmetatable = setmetatable
local tonumber = tonumber
local format = string.format

local Stream = {}
Stream.__index = Stream

local Split = function(line)
    local sep, fields = "▃", {}
    local pattern = format("([^%s]+)", sep)
    line:gsub(pattern, function(c)
        fields[#fields + 1] = c
    end)
    return fields
end

function Stream.New(dataFile)
    local o = {}
    setmetatable(o, Stream)
    o.GetLine = io.lines(dataFile)
    o.cloums = Split(o.GetLine())
    o.hasNext = true
    return o
end

function Stream:Count()
    return #self.columns
end

function Stream:Close()
    while self.GetLine() do
    end
end

function Stream:NextRow()
    local line = self.GetLine()
    if line == nil then
        return nil
    end
    self.columns = Split(line)
end

function Stream:NextColum(Stream)
    if self.cIndex > #self.columns then
        local status = self:NextRow()
        if not status then
            self.hasNext = false
            return nil
        end
    end
    self.cIndex = self.cIndex + 1
    return self.columns[self.cIndex]
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
    return getter()
end

return Stream