@cd ../Tool
ConfigGen.exe -optMode all ^
-configDir ../Csv ^
-exportCsv ../Code/Config ^
-exportLua ../Code/Scripts/Cfg ^
-exportCSharp ../UnityLua/Assets/Editor/Config ^
-exportCsLson ../UnityLua/Assets/Editor/Lson ^

@pause