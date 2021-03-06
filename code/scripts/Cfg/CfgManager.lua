local allCfgs = nil

local CfgManager = {}

function CfgManager.Init()
    allCfgs = require "Cfg.Config"
end

function CfgManager.GetConfig(cfgName)
    if allCfgs then
        return allCfgs[cfgName]
    end
    return nil
end

return CfgManager