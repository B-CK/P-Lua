require "3rd.pblua.login_pb"
require "3rd.pbc.protobuf"

--管理器--
Game = {};
local this = Game;

local game;
local transform;
local gameObject;

--初始化完成，发送链接服务器信息--
function Game.OnInitOK()
    AppConst.SocketPort = 2012;
    AppConst.SocketAddress = "127.0.0.1";
    NetworkMgr:SendConnect();

    PrintYellow('--------------->> LuaFramework InitOK');

    Game.test_function()
end

function Game.test_function()
    local game = FindObj('GameManager')
    PrintYellow(game.name)
    local tbl = { [1] = 5,
                  [3] = 'test',
                  ['a'] = 77543,
                  [2] = { ['t'] = 9, ['mon'] = 1, [4] = { ['thou'] = 1000, ['w'] = 10000, [143] = 'gil' }, ['what'] = 'why' },
                  ['apr'] = 4 }
    tbl.go = game
    PrintTable(tbl, "表格数据信息")
end