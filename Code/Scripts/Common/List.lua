local list = list
local ilist = ilist
local rilist = rilist
local LuaUtils = LuaUtils

---@class List:list
local List = Class:new(list)

function List:new()
    ---@type List
    local o = list:new()
    setmetatable(o, self)
    return o
end

function List:Add(item)
    self:push(item)
end

function List:Remove(item)
    local node = self:find(item)
    self:remove(node)
end

function List:Clear()
    self:clear()
end

function List:Contains(item)
    local node = self:find(item)
    return node ~= nil
end

function List:Count()
    return self.length
end

function List:Find(match)
    for i, v in ilist(self) do
        if match then
            return v
        end
    end
    return nil
end

function List:FindLast(match)
    for i, v in rilist(self) do
        if match then
            return v
        end
    end
    return nil
end

function List:Sort(comparer)
    LuaUtils.TableSort(self, comparer)
end

function List:Clone()
    return self:clone()
end

function List:MoveNext()
    return self:movenext()
end
function List:Current()
    return self:current()
end
function List:InitEnumerator()
    self:initenumerator()
end
function List:ToTable()
    return self:totable()
end

return List