require "3rd.pblua.login_pb"
require "3rd.pbc.protobuf"

--管理器--
Game = {};
local this = Game;

local game;
local transform;
local gameObject;

function Game.InitViewPanels()
    for i = 1, #PanelNames do
        require("View/" .. tostring(PanelNames[i]))
    end
end

--初始化完成，发送链接服务器信息--
function Game.OnInitOK()
    AppConst.SocketPort = 2012;
    AppConst.SocketAddress = "127.0.0.1";
    NetworkMgr:SendConnect();

    --注册LuaView--
    this.InitViewPanels();

    coroutine.start(this.test_coroutine);

    CtrlManager.Init();
    local ctrl = CtrlManager.GetCtrl(CtrlNames.Prompt);
    if ctrl ~= nil and AppConst.ExampleMode == 1 then
        ctrl:Awake();
    end

    local va = string.sub("ax0bxQ", 1, -2)
    local a = { b = { c = { d = 2 } } }
    local b = {}
    LuaUtils.deep_copy_to(a, b)
    logError(b.b.c.d)
    logWarn('LuaFramework InitOK--->>>');
end

--测试协同--
function Game.test_coroutine()
    logWarn("1111");
    coroutine.wait(1);
    logWarn("2222");

    local www = WWW("http://bbs.ulua.org/readme.txt");
    coroutine.www(www);
    logWarn(www.text);
end