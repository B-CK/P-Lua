local pairs             = pairs
local format            = string.format
local print             = print
local require           = require

local Utils             = require "Common.Utils"
local ViewUtil          = require "Common.ViewUtil"


local CfgMgr            = CfgMgr
local ResMgr            = ResMgr
local PrintTable        = PrintTable
local Vector3           = Vector3
local GameObject        = GameObject
local LogError          = LogError

local views = {} -- viewname -> { status = ? , isshow = ?, depth = ?, hide_time = ? }
local guiCamera = nil
local destroy                   --页面资源销毁
local onshow                    --页面显示回调
local onhide                    --页面隐藏回调
local showloading               --显示加载中提示
local hideloading               --隐藏加载中提示
local showloadedview            --显示已经加载的页面 用于弹出堆栈 或者tab页切换
local hideloadedview            --隐藏已经加载的页面 用于弹出堆栈 或者tab页切换



return {

}