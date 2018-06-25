@cd ../Tool
ConfigGen.exe -optMode all ^
-configDir ../Csv ^
-exportCsv ../Code/Config ^
-exportLua ../Code/Scripts/Cfg ^
-exportCSharp ../UnityLua/Assets/Config ^
-exportCsLson ../UnityLua/Assets/Lson ^

@pause