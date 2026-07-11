/*
  WF.MES 数据库一键初始化脚本（全新库，不删表）
  按顺序执行: 01 -> 02 -> 03
  用法 (sqlcmd):
    sqlcmd -S localhost,1433 -U sa -P "YourPassword" -i 00_init_all.sql

  若需清空重建（不保留历史），请改用:
    sqlcmd -S localhost,1433 -U sa -P "YourPassword" -i 00_rebuild_all.sql
*/

:r .\01_create_database.sql
:r .\02_create_tables.sql
:r .\03_seed_data.sql

PRINT N'MES 数据库初始化完成';
GO
