/*
  WF.MES 数据库一键重建脚本（清空后重建，不保留历史数据）
  执行顺序: 01 -> 20 -> 02 -> 03
  用法 (sqlcmd):
    sqlcmd -S localhost,1433 -U sa -P "YourPassword" -i 00_rebuild_all.sql
  警告: 20_drop_all_tables.sql 会删除全部业务表及数据
*/

:r .\01_create_database.sql
:r .\20_drop_all_tables.sql
:r .\02_create_tables.sql
:r .\03_seed_data.sql
:r .\04_seed_business_data.sql

PRINT N'WF_MES_DEV database rebuild completed';
GO
