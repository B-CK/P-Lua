local Stream = require("Cfg.DataStruct")
local cfgs = {}
for _, s in ipairs({
    { name = 'fullClassName', index = 'id', output = 'filePath' },
})
do
    local data = Stream.New(s.output)
    local method = 'Get' .. s.name
    local cfg = {}
    while data.hasNext do
        cfg[s.index] = data[method](data)
    end
    cfgs[s.name] = cfg
end

return cfgs