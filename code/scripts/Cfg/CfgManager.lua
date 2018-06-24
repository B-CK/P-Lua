CfgManager = {}

local allCfgs = nil

function CfgManager.init()
    allCfgs = require "Cfg.Config"
end

function CfgManager.GetConfig(cfgName)
    if allCfgs then
        return allCfgs[cfgName]
    end
    return nil
end

return CfgManager