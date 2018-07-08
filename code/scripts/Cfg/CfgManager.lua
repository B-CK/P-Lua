local allCfgs = nil

local function init()
    allCfgs = require "Cfg.Config"
end

local function GetConfig(cfgName)
    if allCfgs then
        return allCfgs[cfgName]
    end
    return nil
end

return {
    init,
    GetConfig,
}