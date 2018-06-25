﻿local Stream = require("Cfg.DataStruct")
local cfgs = {}
for _, s in ipairs({
	{ name = 'AllClass', method = 'GetCfgAllTypeAllClass', index = 'ID', output = 'AllType/AllClass.xml' },
	{ name = 'SingleClass', method = 'GetCfgAllTypeSingleClass', index = 'Var1', output = 'AllType/SingleClass.xml' },
	{ name = 'Card', method = 'GetCfgCardCard', index = 'ID', output = 'Card/Card.xml' },
}) do
	local data = Stream.new(s.output)
	local cfg = {}
	while data.hasNext do
		cfg[s.index] = data[s.method](data)
	end
	cfgs[s.name] = cfg
	data:Close()
end

return cfgs
