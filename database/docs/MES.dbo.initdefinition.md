-- WF_MES_DEV.dbo.System_Dict_Data definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Dict_Data;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Dict_Data (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	DictTypeId **bigint** **NOT** **NULL**,

​	DictType **nvarchar**(100) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	DictLabel **nvarchar**(100) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	DictValue **nvarchar**(100) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Sort **int** **DEFAULT** 0 **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	Remark **nvarchar**(500) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreateTime **datetime2** **DEFAULT** sysdatetime() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime2** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Dict__3214EC07F52C277E **PRIMARY** **KEY** (Id)

);

 **CREATE** **NONCLUSTERED** **INDEX** IX_System_Dict_Data_DictType **ON** dbo.System_Dict_Data (  DictType **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Dict_Type definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Dict_Type;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Dict_Type (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	DictName **nvarchar**(100) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	DictType **nvarchar**(100) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	Remark **nvarchar**(500) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreateTime **datetime2** **DEFAULT** sysdatetime() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime2** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Dict__3214EC07964D1C83 **PRIMARY** **KEY** (Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_Dict_Type_DictType **ON** dbo.System_Dict_Type (  DictType **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Exception_Log definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Exception_Log;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Exception_Log (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	Module **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Message **nvarchar**(2000) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	StackTrace **nvarchar**(**MAX**) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	RequestUrl **nvarchar**(512) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	RequestMethod **nvarchar**(16) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	RequestParam **nvarchar**(**MAX**) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	OperIp **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	OperUserId **bigint** **NULL**,

​	OperUserName **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	ExceptionTime **datetime2** **DEFAULT** sysdatetime() **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Exce__3214EC071BCA3FEF **PRIMARY** **KEY** (Id)

);

 **CREATE** **NONCLUSTERED** **INDEX** IX_System_Exception_Log_ExceptionTime **ON** dbo.System_Exception_Log (  ExceptionTime **DESC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Menu definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Menu;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Menu (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	ParentId **bigint** **DEFAULT** 0 **NOT** **NULL**,

​	MenuName **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	MenuType **int** **NOT** **NULL**,

​	**[Path]** **nvarchar**(256) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Component **nvarchar**(256) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Permission **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Icon **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Sort **int** **DEFAULT** 0 **NOT** **NULL**,

​	ClientType **int** **DEFAULT** 1 **NOT** **NULL**,

​	I18nKey **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Visible **bit** **DEFAULT** 1 **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	Remark **nvarchar**(512) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Menu__3214EC074BA93908 **PRIMARY** **KEY** (Id)

);

 **CREATE** **NONCLUSTERED** **INDEX** IX_System_Menu_ClientType **ON** dbo.System_Menu (  ClientType **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Notice definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Notice;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Notice (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	Title **nvarchar**(200) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Content **nvarchar**(**MAX**) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	NoticeType **int** **DEFAULT** 1 **NOT** **NULL**,

​	Status **int** **DEFAULT** 0 **NOT** **NULL**,

​	PublishTime **datetime2** **NULL**,

​	CreateTime **datetime2** **DEFAULT** sysdatetime() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime2** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Noti__3214EC07696D426C **PRIMARY** **KEY** (Id)

);





-- WF_MES_DEV.dbo.System_Operation_Log definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Operation_Log;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Operation_Log (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	Module **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Title **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	BusinessType **nvarchar**(32) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	**[Method]** **nvarchar**(256) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	RequestMethod **nvarchar**(16) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	OperUrl **nvarchar**(512) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	OperIp **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	OperParam **nvarchar**(**MAX**) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	ErrorMsg **nvarchar**(2000) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	OperUserId **bigint** **NULL**,

​	OperUserName **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	OperTime **datetime2** **DEFAULT** sysdatetime() **NOT** **NULL**,

​	CostTime **bigint** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Oper__3214EC076DF599DE **PRIMARY** **KEY** (Id)

);

 **CREATE** **NONCLUSTERED** **INDEX** IX_System_Operation_Log_OperTime **ON** dbo.System_Operation_Log (  OperTime **DESC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Position definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Position;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Position (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	PositionCode **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	PositionName **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	ProcessCode **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	DeptId **bigint** **NULL**,

​	Sort **int** **DEFAULT** 0 **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	Remark **nvarchar**(512) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Posi__3214EC07AA34EDD7 **PRIMARY** **KEY** (Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_Position_PositionCode **ON** dbo.System_Position (  PositionCode **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Refresh_Token definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Refresh_Token;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Refresh_Token (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	UserId **bigint** **NOT** **NULL**,

​	ClientType **int** **DEFAULT** 1 **NOT** **NULL**,

​	FactoryId **bigint** **NULL**,

​	SessionId **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Token **nvarchar**(512) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	ExpireTime **datetime** **NOT** **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	IsRevoked **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Refr__3214EC07B8FA30D2 **PRIMARY** **KEY** (Id)

);

 **CREATE** **NONCLUSTERED** **INDEX** IX_System_Refresh_Token_User_Client **ON** dbo.System_Refresh_Token (  UserId **ASC**  , ClientType **ASC**  )  

​	 **WHERE**  (**[IsRevoked]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Region definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Region;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Region (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	RegionCode **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	RegionName **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Sort **int** **DEFAULT** 0 **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	Remark **nvarchar**(512) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Regi__3214EC074201A4CD **PRIMARY** **KEY** (Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_Region_RegionCode **ON** dbo.System_Region (  RegionCode **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Role definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Role;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Role (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	RoleCode **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	RoleName **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Sort **int** **DEFAULT** 0 **NOT** **NULL**,

​	DataScope **int** **DEFAULT** 3 **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	Remark **nvarchar**(512) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Role__3214EC07B8815D4B **PRIMARY** **KEY** (Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_Role_RoleCode **ON** dbo.System_Role (  RoleCode **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Role_Dept definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Role_Dept;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Role_Dept (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	RoleId **bigint** **NOT** **NULL**,

​	DeptId **bigint** **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Role__3214EC07618A6EED **PRIMARY** **KEY** (Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_Role_Dept **ON** dbo.System_Role_Dept (  RoleId **ASC**  , DeptId **ASC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Role_Menu definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Role_Menu;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Role_Menu (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	RoleId **bigint** **NOT** **NULL**,

​	MenuId **bigint** **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Role__3214EC07837BB284 **PRIMARY** **KEY** (Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_Role_Menu **ON** dbo.System_Role_Menu (  RoleId **ASC**  , MenuId **ASC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_User definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_User;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_User (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	UserName **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	PasswordHash **nvarchar**(256) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	NickName **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Email **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	DeptId **bigint** **NOT** **NULL**,

​	DefaultFactoryId **bigint** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	Remark **nvarchar**(512) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	LastLoginTime **datetime** **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	MustChangePassword **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_User__3214EC074B670C7D **PRIMARY** **KEY** (Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_User_UserName **ON** dbo.System_User (  UserName **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_User_Position definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_User_Position;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_User_Position (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	UserId **bigint** **NOT** **NULL**,

​	PositionId **bigint** **NOT** **NULL**,

​	**CONSTRAINT** PK__System_User__3214EC072AE05AB2 **PRIMARY** **KEY** (Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_User_Position **ON** dbo.System_User_Position (  UserId **ASC**  , PositionId **ASC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_User_Role definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_User_Role;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_User_Role (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	UserId **bigint** **NOT** **NULL**,

​	RoleId **bigint** **NOT** **NULL**,

​	**CONSTRAINT** PK__System_User__3214EC072879C3D8 **PRIMARY** **KEY** (Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_User_Role **ON** dbo.System_User_Role (  UserId **ASC**  , RoleId **ASC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Factory definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Factory;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Factory (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	RegionId **bigint** **NOT** **NULL**,

​	FactoryCode **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	FactoryName **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	TimeZone **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Sort **int** **DEFAULT** 0 **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	Remark **nvarchar**(512) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Fact__3214EC07B12C26F8 **PRIMARY** **KEY** (Id),

​	**CONSTRAINT** FK_System_Factory_Region **FOREIGN** **KEY** (RegionId) **REFERENCES** WF_MES_DEV.dbo.System_Region(Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_Factory_FactoryCode **ON** dbo.System_Factory (  FactoryCode **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Factory_Config definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Factory_Config;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Factory_Config (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	FactoryId **bigint** **NOT** **NULL**,

​	ConfigKey **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	ConfigValue **nvarchar**(**MAX**) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	**CONSTRAINT** PK__System_Fact__3214EC07556E149E **PRIMARY** **KEY** (Id),

​	**CONSTRAINT** FK_System_Factory_Config_Factory **FOREIGN** **KEY** (FactoryId) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_Factory_Config **ON** dbo.System_Factory_Config (  FactoryId **ASC**  , ConfigKey **ASC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_User_Factory definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_User_Factory;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_User_Factory (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	UserId **bigint** **NOT** **NULL**,

​	FactoryId **bigint** **NOT** **NULL**,

​	IsDefault **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_User__3214EC075BA003CA **PRIMARY** **KEY** (Id),

​	**CONSTRAINT** FK_System_User_Factory_Factory **FOREIGN** **KEY** (FactoryId) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id),

​	**CONSTRAINT** FK_System_User_Factory_User **FOREIGN** **KEY** (UserId) **REFERENCES** WF_MES_DEV.dbo.System_User(Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_User_Factory **ON** dbo.System_User_Factory (  UserId **ASC**  , FactoryId **ASC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.Warehouse_InboundOrder definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Warehouse_InboundOrder;



**CREATE** **TABLE** WF_MES_DEV.dbo.Warehouse_InboundOrder (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	FactoryId **bigint** **NOT** **NULL**,

​	InboundNo **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Status **int** **DEFAULT** 0 **NOT** **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__Warehouse_Inbou__3214EC07682F6CAF **PRIMARY** **KEY** (Id),

​	**CONSTRAINT** FK_Warehouse_InboundOrder_Factory **FOREIGN** **KEY** (FactoryId) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_Warehouse_InboundOrder_Factory_No **ON** dbo.Warehouse_InboundOrder (  FactoryId **ASC**  , InboundNo **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.Barcode_Customer definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Barcode_Customer;



**CREATE** **TABLE** WF_MES_DEV.dbo.Barcode_Customer (

​	Customer_Id **int** **IDENTITY**(1,1) **NOT** **NULL**,

​	Factory_Id **bigint** **DEFAULT** 1 **NOT** **NULL**,

​	Customer_Name **nvarchar**(100) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Enable **int** **DEFAULT** 1 **NOT** **NULL**,

​	CreatedBy **nvarchar**(50) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreateDate **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	UpdatedBy **nvarchar**(50) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	UpdatedAt **datetime** **NULL**,

​	**CONSTRAINT** PK__Barcode_Cust__8CB28699B876A898 **PRIMARY** **KEY** (Customer_Id),

​	**CONSTRAINT** FK_Barcode_Customer_Factory **FOREIGN** **KEY** (Factory_Id) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_Barcode_Customer_Factory_Name **ON** dbo.Barcode_Customer (  Factory_Id **ASC**  , Customer_Name **ASC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.Barcode_MaterialRule definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Barcode_MaterialRule;



**CREATE** **TABLE** WF_MES_DEV.dbo.Barcode_MaterialRule (

​	Rule_Id **int** **IDENTITY**(1,1) **NOT** **NULL**,

​	Factory_Id **bigint** **DEFAULT** 1 **NOT** **NULL**,

​	Customer_Id **int** **NOT** **NULL**,

​	Material_No **nvarchar**(50) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Barcode_Length **int** **DEFAULT** 0 **NOT** **NULL**,

​	Qa_Status **int** **DEFAULT** 0 **NOT** **NULL**,

​	Attachment_Uploaded_By **nvarchar**(50) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Attachment_Uploaded_At **datetime** **NULL**,

​	Qa_Reviewed_By **nvarchar**(50) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Qa_Reviewed_At **datetime** **NULL**,

​	Qa_Review_Remark **nvarchar**(500) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Drawing_Image **varbinary**(**MAX**) **NULL**,

​	Print_Sample_Image **varbinary**(**MAX**) **NULL**,

​	CreatedBy **nvarchar**(50) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreateDate **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	UpdatedBy **nvarchar**(50) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	UpdatedAt **datetime** **NULL**,

​	**CONSTRAINT** PK__Barcode_Mate__70B7089ECB6BA408 **PRIMARY** **KEY** (Rule_Id),

​	**CONSTRAINT** UQ_Barcode_MaterialRule **UNIQUE** (Factory_Id,Customer_Id,Material_No),

​	**CONSTRAINT** FK_Barcode_MaterialRule_Customer **FOREIGN** **KEY** (Customer_Id) **REFERENCES** WF_MES_DEV.dbo.Barcode_Customer(Customer_Id),

​	**CONSTRAINT** FK_Barcode_MaterialRule_Factory **FOREIGN** **KEY** (Factory_Id) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id)

);





-- WF_MES_DEV.dbo.Barcode_PurgeLog definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Barcode_PurgeLog;



**CREATE** **TABLE** WF_MES_DEV.dbo.Barcode_PurgeLog (

​	PurgeLog_Id **int** **IDENTITY**(1,1) **NOT** **NULL**,

​	Factory_Id **bigint** **DEFAULT** 1 **NOT** **NULL**,

​	RunAt **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CutoffDate **datetime** **NOT** **NULL**,

​	DeletedRecordCount **bigint** **DEFAULT** 0 **NOT** **NULL**,

​	DeletedGenerateCount **int** **DEFAULT** 0 **NOT** **NULL**,

​	DurationMs **int** **NULL**,

​	Status **nvarchar**(20) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Message **nvarchar**(500) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	**CONSTRAINT** PK__Barcode_Purg__597261AC5ED115D6 **PRIMARY** **KEY** (PurgeLog_Id),

​	**CONSTRAINT** FK_Barcode_PurgeLog_Factory **FOREIGN** **KEY** (Factory_Id) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id)

);

 **CREATE** **NONCLUSTERED** **INDEX** IX_Barcode_PurgeLog_RunAt **ON** dbo.Barcode_PurgeLog (  RunAt **DESC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.Barcode_RuleSegment definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Barcode_RuleSegment;



**CREATE** **TABLE** WF_MES_DEV.dbo.Barcode_RuleSegment (

​	Segment_Id **int** **IDENTITY**(1,1) **NOT** **NULL**,

​	Rule_Id **int** **NOT** **NULL**,

​	Sort_Order **int** **NOT** **NULL**,

​	Segment_Type **nvarchar**(20) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Config_Json **nvarchar**(500) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Include_In_ResetKey **int** **DEFAULT** 1 **NOT** **NULL**,

​	**CONSTRAINT** PK__Barcode_Rule__D81DD01AEB64F959 **PRIMARY** **KEY** (Segment_Id),

​	**CONSTRAINT** FK_Barcode_RuleSegment_Rule **FOREIGN** **KEY** (Rule_Id) **REFERENCES** WF_MES_DEV.dbo.Barcode_MaterialRule(Rule_Id) **ON** **DELETE** **CASCADE**

);

 **CREATE** **NONCLUSTERED** **INDEX** IX_Barcode_RuleSegment_Rule **ON** dbo.Barcode_RuleSegment (  Rule_Id **ASC**  , Sort_Order **ASC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.Barcode_SerialCounter definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Barcode_SerialCounter;



**CREATE** **TABLE** WF_MES_DEV.dbo.Barcode_SerialCounter (

​	Id **int** **IDENTITY**(1,1) **NOT** **NULL**,

​	Rule_Id **int** **NOT** **NULL**,

​	Reset_Key **nvarchar**(500) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Current_Value **bigint** **DEFAULT** 0 **NOT** **NULL**,

​	UpdatedAt **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	**CONSTRAINT** PK__Barcode_Seri__3214EC075E333387 **PRIMARY** **KEY** (Id),

​	**CONSTRAINT** UQ_Barcode_SerialCounter **UNIQUE** (Rule_Id,Reset_Key),

​	**CONSTRAINT** FK_Barcode_SerialCounter_Rule **FOREIGN** **KEY** (Rule_Id) **REFERENCES** WF_MES_DEV.dbo.Barcode_MaterialRule(Rule_Id)

);





-- WF_MES_DEV.dbo.Master_Material definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Master_Material;



**CREATE** **TABLE** WF_MES_DEV.dbo.Master_Material (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	FactoryId **bigint** **NOT** **NULL**,

​	MaterialCode **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	MaterialName **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	Remark **nvarchar**(512) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__Master_Mate__3214EC07DC7929E8 **PRIMARY** **KEY** (Id),

​	**CONSTRAINT** FK_Master_Material_Factory **FOREIGN** **KEY** (FactoryId) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_Master_Material_Factory_Code **ON** dbo.Master_Material (  FactoryId **ASC**  , MaterialCode **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.Master_Route definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Master_Route;



**CREATE** **TABLE** WF_MES_DEV.dbo.Master_Route (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	FactoryId **bigint** **NOT** **NULL**,

​	RouteCode **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	RouteName **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__Master_Rout__3214EC079031AB07 **PRIMARY** **KEY** (Id),

​	**CONSTRAINT** FK_Master_Route_Factory **FOREIGN** **KEY** (FactoryId) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_Master_Route_Factory_Code **ON** dbo.Master_Route (  FactoryId **ASC**  , RouteCode **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.Master_Station definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Master_Station;



**CREATE** **TABLE** WF_MES_DEV.dbo.Master_Station (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	FactoryId **bigint** **NOT** **NULL**,

​	StationCode **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	StationName **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__Master_Stat__3214EC07AB49FB6C **PRIMARY** **KEY** (Id),

​	**CONSTRAINT** FK_Master_Station_Factory **FOREIGN** **KEY** (FactoryId) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_Master_Station_Factory_Code **ON** dbo.Master_Station (  FactoryId **ASC**  , StationCode **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.Master_WorkCenter definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Master_WorkCenter;



**CREATE** **TABLE** WF_MES_DEV.dbo.Master_WorkCenter (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	FactoryId **bigint** **NOT** **NULL**,

​	WorkCenterCode **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	WorkCenterName **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__Master_Work__3214EC07808CFCD6 **PRIMARY** **KEY** (Id),

​	**CONSTRAINT** FK_Master_WorkCenter_Factory **FOREIGN** **KEY** (FactoryId) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_Master_WorkCenter_Factory_Code **ON** dbo.Master_WorkCenter (  FactoryId **ASC**  , WorkCenterCode **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.Production_PassRecord definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Production_PassRecord;



**CREATE** **TABLE** WF_MES_DEV.dbo.Production_PassRecord (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	FactoryId **bigint** **NOT** **NULL**,

​	WorkOrderNo **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	StationCode **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Barcode **nvarchar**(200) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	PassTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	**CONSTRAINT** PK__Production_Pass__3214EC0733CF5621 **PRIMARY** **KEY** (Id),

​	**CONSTRAINT** FK_Production_PassRecord_Factory **FOREIGN** **KEY** (FactoryId) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id)

);

 **CREATE** **NONCLUSTERED** **INDEX** IX_Production_PassRecord_Factory_Time **ON** dbo.Production_PassRecord (  FactoryId **ASC**  , PassTime **DESC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.Production_WorkOrder definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Production_WorkOrder;



**CREATE** **TABLE** WF_MES_DEV.dbo.Production_WorkOrder (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	FactoryId **bigint** **NOT** **NULL**,

​	WorkOrderNo **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	MaterialCode **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	Status **int** **DEFAULT** 0 **NOT** **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__Production_Work__3214EC07FBDADC70 **PRIMARY** **KEY** (Id),

​	**CONSTRAINT** FK_Production_WorkOrder_Factory **FOREIGN** **KEY** (FactoryId) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id)

);

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_Production_WorkOrder_Factory_No **ON** dbo.Production_WorkOrder (  FactoryId **ASC**  , WorkOrderNo **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.System_Dept definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.System_Dept;



**CREATE** **TABLE** WF_MES_DEV.dbo.System_Dept (

​	Id **bigint** **IDENTITY**(1,1) **NOT** **NULL**,

​	FactoryId **bigint** **NOT** **NULL**,

​	ParentId **bigint** **DEFAULT** 0 **NOT** **NULL**,

​	DeptCode **nvarchar**(64) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	DeptName **nvarchar**(128) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	DeptType **int** **NOT** **NULL**,

​	Sort **int** **DEFAULT** 0 **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	Remark **nvarchar**(512) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreateTime **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	CreateBy **bigint** **NULL**,

​	UpdateTime **datetime** **NULL**,

​	UpdateBy **bigint** **NULL**,

​	IsDeleted **bit** **DEFAULT** 0 **NOT** **NULL**,

​	**CONSTRAINT** PK__System_Dept__3214EC07D79B113B **PRIMARY** **KEY** (Id),

​	**CONSTRAINT** FK_System_Dept_Factory **FOREIGN** **KEY** (FactoryId) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id)

);

 **CREATE** **NONCLUSTERED** **INDEX** IX_System_Dept_FactoryId **ON** dbo.System_Dept (  FactoryId **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;

 **CREATE**  **UNIQUE** **NONCLUSTERED** **INDEX** UX_System_Dept_Factory_DeptCode **ON** dbo.System_Dept (  FactoryId **ASC**  , DeptCode **ASC**  )  

​	 **WHERE**  (**[IsDeleted]**=(0))

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.Barcode_GenerateRecord definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Barcode_GenerateRecord;



**CREATE** **TABLE** WF_MES_DEV.dbo.Barcode_GenerateRecord (

​	Generate_Record_Id **int** **IDENTITY**(1,1) **NOT** **NULL**,

​	Factory_Id **bigint** **DEFAULT** 1 **NOT** **NULL**,

​	Generate_No **nvarchar**(50) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Rule_Id **int** **NOT** **NULL**,

​	Material_No **nvarchar**(50) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Reset_Key **nvarchar**(500) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Print_Date **date** **NOT** **NULL**,

​	Quantity **int** **NOT** **NULL**,

​	Serial_Start **bigint** **NOT** **NULL**,

​	Serial_End **bigint** **NOT** **NULL**,

​	Print_Status **int** **DEFAULT** 0 **NOT** **NULL**,

​	Last_Reprinted_At **datetime** **NULL**,

​	Last_Reprinted_By **nvarchar**(50) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreatedBy **nvarchar**(50) **COLLATE** Chinese_PRC_CI_AS **NULL**,

​	CreatedAt **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	**CONSTRAINT** PK__Barcode_Gene__53768611EC58ED0F **PRIMARY** **KEY** (Generate_Record_Id),

​	**CONSTRAINT** UQ_Barcode_GenerateRecord_No **UNIQUE** (Generate_No),

​	**CONSTRAINT** FK_Barcode_GenerateRecord_Factory **FOREIGN** **KEY** (Factory_Id) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id),

​	**CONSTRAINT** FK_Barcode_GenerateRecord_Rule **FOREIGN** **KEY** (Rule_Id) **REFERENCES** WF_MES_DEV.dbo.Barcode_MaterialRule(Rule_Id)

);

 **CREATE** **NONCLUSTERED** **INDEX** IX_Barcode_GenerateRecord_CreatedAt **ON** dbo.Barcode_GenerateRecord (  CreatedAt **ASC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;





-- WF_MES_DEV.dbo.Barcode_Record definition



-- Drop table



-- DROP TABLE WF_MES_DEV.dbo.Barcode_Record;



**CREATE** **TABLE** WF_MES_DEV.dbo.Barcode_Record (

​	Record_Id **int** **IDENTITY**(1,1) **NOT** **NULL**,

​	Factory_Id **bigint** **DEFAULT** 1 **NOT** **NULL**,

​	Generate_Record_Id **int** **NOT** **NULL**,

​	Rule_Id **int** **NOT** **NULL**,

​	Barcode **nvarchar**(200) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Reset_Key **nvarchar**(500) **COLLATE** Chinese_PRC_CI_AS **NOT** **NULL**,

​	Serial_Value **bigint** **NOT** **NULL**,

​	Status **int** **DEFAULT** 1 **NOT** **NULL**,

​	CreatedAt **datetime** **DEFAULT** **getdate**() **NOT** **NULL**,

​	**CONSTRAINT** PK__Barcode_Reco__603A0C40A848D981 **PRIMARY** **KEY** (Record_Id),

​	**CONSTRAINT** UQ_Barcode_Record_Barcode **UNIQUE** (Barcode),

​	**CONSTRAINT** FK_Barcode_Record_Factory **FOREIGN** **KEY** (Factory_Id) **REFERENCES** WF_MES_DEV.dbo.System_Factory(Id),

​	**CONSTRAINT** FK_Barcode_Record_GenerateRecord **FOREIGN** **KEY** (Generate_Record_Id) **REFERENCES** WF_MES_DEV.dbo.Barcode_GenerateRecord(Generate_Record_Id),

​	**CONSTRAINT** FK_Barcode_Record_Rule **FOREIGN** **KEY** (Rule_Id) **REFERENCES** WF_MES_DEV.dbo.Barcode_MaterialRule(Rule_Id)

);

 **CREATE** **NONCLUSTERED** **INDEX** IX_Barcode_Record_GenerateRecord **ON** dbo.Barcode_Record (  Generate_Record_Id **ASC**  )  

​	 **WITH** (  PAD_INDEX = **OFF** ,**FILLFACTOR** = 100  ,SORT_IN_TEMPDB = **OFF** , IGNORE_DUP_KEY = **OFF** , STATISTICS_NORECOMPUTE = **OFF** , ONLINE = **OFF** , ALLOW_ROW_LOCKS = **ON** , ALLOW_PAGE_LOCKS = **ON**  )

​	 **ON** **[PRIMARY ]** ;