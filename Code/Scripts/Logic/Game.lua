require "3rd.pblua.login_pb"
require "3rd.pbc.protobuf"
local UIMgr = require("Logic.UIManager")

--管理器--
Game = {};

--初始化完成，发送链接服务器信息--
function Game.OnInitOK()
    AppConst.SocketPort = 2012;
    AppConst.SocketAddress = "127.0.0.1";
    NetworkMgr:SendConnect();

    PrintYellow('--------------->> LuaFramework InitOK');

    UIMgr.Show("DlgMain")

    --Game.test_function()
    --Game.test_stringMatch()
    --UpdateBeat:Add(Game.Update, "")
    --Game.test_queue()
end



function Game.test_queue()
    local linkList = list:new()
    linkList:push(1)
    linkList:push(2)
    linkList:push(3)

    local list = List:new()
    list:Add(1)
    list:Add(2)
    list:Add(3)
    list:Add("-A")
    list:Add("-B")
    list:InitEnumerator()
    while list:MoveNext() do
        PrintYellow("Enumerator Current", list:Current())
    end
    PrintTable(list:ToTable())
end
local i = 0
function Game.Update()
    i = i + 2
    PrintYellow(i)
end

function Game.test_stringMatch()
    local viewName = "ui.friend.tabfriend"
    local v1, v2, v3 = string.match(viewName, "^(.*)%.(.*)%.(.*)$")
    PrintYellow(v1, v2, v3)
    Event.Brocast("gm", 'ABCDEFG')
end

function Game.test_function()
    Event.AddListener("gm", function(p)
        PrintYellow("-------------Event", p)
    end)
    local game = FindObj('GameManager')
    PrintYellow(game.name)
    local tbl = {
        [1]     = 5,
        [3]     = 'test',
        ['a']   = 77543,
        [2]     = { ['t'] = 9, ['mon'] = 1, [4] = { ['thou'] = 1000, ['w'] = 10000, [143] = 'gil' }, ['what'] = 'why' },
        ['apr'] = 4 }
    tbl.go = game
    PrintTable(tbl, "表格数据信息")
    --local img = FindObj("Canvas/GuiCamera/Image")
    -----@type UnityEngine.EventSystems.EventTriggerType
    --local evtType = UnityEngine.EventSystems.EventTriggerType
    --LuaHelper.AddEventTrigger(img, evtType.PointerClick, function (data)
    --    PrintYellow(data)
    --end)
end
