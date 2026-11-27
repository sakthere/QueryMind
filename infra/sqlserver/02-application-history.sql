IF DB_ID(N'QueryMindApp') IS NULL CREATE DATABASE QueryMindApp;
GO
USE master;
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'querymind_app')
    EXEC(N'CREATE LOGIN querymind_app WITH PASSWORD = ''$(QueryMindAppPassword)'', CHECK_POLICY = ON;');
GO
USE QueryMindApp;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'querymind_app')
    CREATE USER querymind_app FOR LOGIN querymind_app;
GO
IF OBJECT_ID(N'dbo.query_history', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.query_history (
        id uniqueidentifier NOT NULL PRIMARY KEY,
        question nvarchar(500) NOT NULL,
        title nvarchar(200) NULL,
        provider nvarchar(100) NOT NULL,
        model nvarchar(200) NOT NULL,
        status nvarchar(40) NOT NULL,
        row_count int NULL,
        duration_ms bigint NULL,
        created_at datetime2 NOT NULL CONSTRAINT DF_query_history_created_at DEFAULT SYSUTCDATETIME()
    );
END
GO
GRANT SELECT, INSERT ON dbo.query_history TO querymind_app;
GO
