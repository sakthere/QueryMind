IF DB_ID(N'QueryMindAnalytics') IS NULL CREATE DATABASE QueryMindAnalytics;
IF DB_ID(N'QueryMindApp') IS NULL CREATE DATABASE QueryMindApp;
GO
USE QueryMindAnalytics;
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'analytics') EXEC(N'CREATE SCHEMA analytics');
GO
CREATE TABLE analytics.regions (id int NOT NULL PRIMARY KEY, name nvarchar(80) NOT NULL UNIQUE);
CREATE TABLE analytics.categories (id int NOT NULL PRIMARY KEY, name nvarchar(80) NOT NULL UNIQUE);
CREATE TABLE analytics.customers (id int NOT NULL PRIMARY KEY, full_name nvarchar(160) NOT NULL, region_id int NOT NULL REFERENCES analytics.regions(id));
CREATE TABLE analytics.products (id int NOT NULL PRIMARY KEY, name nvarchar(160) NOT NULL, category_id int NOT NULL REFERENCES analytics.categories(id), unit_price decimal(12,2) NOT NULL);
CREATE TABLE analytics.orders (id int NOT NULL PRIMARY KEY, customer_id int NOT NULL REFERENCES analytics.customers(id), region_id int NOT NULL REFERENCES analytics.regions(id), ordered_at datetime2 NOT NULL, status varchar(16) NOT NULL CHECK (status IN ('completed','cancelled','returned')), total_amount decimal(12,2) NOT NULL);
CREATE TABLE analytics.order_items (id int NOT NULL PRIMARY KEY, order_id int NOT NULL REFERENCES analytics.orders(id), product_id int NOT NULL REFERENCES analytics.products(id), quantity int NOT NULL CHECK (quantity > 0), unit_price decimal(12,2) NOT NULL);
CREATE TABLE analytics.payments (id int NOT NULL PRIMARY KEY, order_id int NOT NULL REFERENCES analytics.orders(id), amount decimal(12,2) NOT NULL, paid_at datetime2 NULL);
CREATE TABLE analytics.returns (id int NOT NULL PRIMARY KEY, order_id int NOT NULL REFERENCES analytics.orders(id), reason nvarchar(160) NOT NULL, returned_at datetime2 NOT NULL);
CREATE TABLE analytics.sales_representatives (id int NOT NULL PRIMARY KEY, full_name nvarchar(160) NOT NULL, region_id int NOT NULL REFERENCES analytics.regions(id));
GO
INSERT INTO analytics.regions VALUES (1,'North'),(2,'South'),(3,'East'),(4,'West');
INSERT INTO analytics.categories VALUES (1,'Electronics'),(2,'Home & Living'),(3,'Outdoor'),(4,'Wellness'),(5,'Office');
;WITH n AS (SELECT TOP (2000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS id FROM sys.all_objects a CROSS JOIN sys.all_objects b) INSERT INTO analytics.customers SELECT id, CONCAT('Customer ', id), ((id - 1) % 4) + 1 FROM n;
;WITH n AS (SELECT TOP (100) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS id FROM sys.all_objects) INSERT INTO analytics.products SELECT id, CONCAT('Product ', id), ((id - 1) % 5) + 1, CAST(15 + ((id * 37) % 485) AS decimal(12,2)) FROM n;
;WITH n AS (SELECT TOP (10000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS id FROM sys.all_objects a CROSS JOIN sys.all_objects b) INSERT INTO analytics.orders SELECT id, ((id - 1) % 2000) + 1, ((id - 1) % 4) + 1, DATEADD(day, id % 365, CAST('2025-12-01' AS datetime2)), CASE WHEN id % 29 = 0 THEN 'cancelled' WHEN id % 31 = 0 THEN 'returned' ELSE 'completed' END, CAST(40 + ((id * 73) % 900) AS decimal(12,2)) FROM n;
;WITH n AS (SELECT TOP (15000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS id FROM sys.all_objects a CROSS JOIN sys.all_objects b) INSERT INTO analytics.order_items SELECT id, ((id - 1) % 10000) + 1, ((id - 1) % 100) + 1, 1 + (id % 4), CAST(15 + ((id * 41) % 485) AS decimal(12,2)) FROM n;
GO
USE master;
GO
CREATE LOGIN analytics_reader WITH PASSWORD = '$(AnalyticsReaderPassword)', CHECK_POLICY = ON;
GO
USE QueryMindAnalytics;
GO
CREATE USER analytics_reader FOR LOGIN analytics_reader;
GRANT SELECT ON SCHEMA::analytics TO analytics_reader;
GO
