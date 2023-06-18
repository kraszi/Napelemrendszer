GO
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'napelem')
BEGIN
    CREATE DATABASE napelem
END
GO
    USE napelem
    DROP TABLE IF EXISTS [Project]
    DROP TABLE IF EXISTS [Component]
    DROP TABLE IF EXISTS [Storage]
    DROP TABLE IF EXISTS [Employee]
	DROP TABLE IF EXISTS [Progress]
GO

----- táblák létrehozása -----
USE napelem
GO
CREATE TABLE Project(
	[ID]			INT				PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[EmployeeID]	INT				NOT NULL,
	[ProgressID]	INT				NOT NULL,
	[Title]			NVARCHAR(30)	NOT NULL,
	[Address]		NVARCHAR(60)	NOT NULL,
	[ComponentsID]	INT				,
	[FullPrice]		INT				,
	[RequiredTime]	TIME			,
);

CREATE TABLE [Component](
	[ID]				INT			PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[StorageID]			INT			NOT NULL,
	[RequiredQuantity]	INT			NOT NULL,
);

CREATE TABLE [Storage](
	[ID]			INT				PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[Row]			INT				NOT NULL,
	[Column]		INT				NOT NULL,
	[Shelf]			INT				NOT NULL,
	[Cell]			AS (([Row]-1)*24 + ([Column]-1)*6 + [Shelf]),
	[ProductName]	NVARCHAR(30)	NOT NULL,
	[Price]			INT				DEFAULT 0,
	[MaxQuantity]	INT				DEFAULT 1,
	[Quantity]		INT				DEFAULT 1,
	[Reserved]		INT				DEFAULT 0,
);

CREATE TABLE [Employee](
	[ID]			INT				PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[Name]			NVARCHAR (40)	NOT NULL,
	[Username]		NVARCHAR (30)	NOT NULL,
	[Password]		NVARCHAR (60)	NOT NULL,
	[Usertype]		NVARCHAR (20)	NOT NULL CHECK (usertype IN ('Specialist', 'Manager', 'Warehouseman')),
);

CREATE TABLE [Progress](
	[ID]			INT				PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[ProjectID]		INT				NOT NULL,
	[New]			DATETIME		NOT NULL,
	[Draft]			DATETIME		,
	[Wait]			DATETIME		,
	[Scheduled]		DATETIME		,
	[InProgress]	DATETIME		,
	[Completed]		DATETIME		,
	[Failed]		DATETIME		,
);
GO 

----- insert -----
GO
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (1, 1, 1,'Polycrystalline panel', 180000, 10, 6)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (1, 2, 1,'Monocrystalline panel', 150000, 10, 8)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (10, 4, 1,'Inverter 150W', 20000, 20, 4)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (10, 4, 2,'Inverter 300W', 35000, 16, 5)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (10, 4, 3,'Inverter 600W', 50000, 14, 8)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (10, 4, 4,'Inverter 1000W', 90000, 10, 3)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (10, 4, 5,'Inverter 1500W', 125000, 8, 2)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (10, 4, 6,'Inverter 2500W', 180000, 6, 2)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (9, 4, 1,'Cable 1mm 100m', 60000, 14, 7)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (9, 4, 2,'Cable 2mm 100m', 90000, 14, 6)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (9, 3, 1,'Cable Seal', 10000, 100, 68)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (9, 3, 2,'Fuse holder', 1000, 300, 222)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (9, 3, 3,'DC Connector', 4500, 200, 137)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (2, 1, 1,'Roof mount 3m', 30000, 40, 24)
INSERT INTO Storage (Row, [Column], Shelf, ProductName, Price, MaxQuantity, Quantity) VALUES (2, 2, 1,'Wall Mount 1m', 10000, 40, 17)

INSERT INTO Employee (Name, Username, Password, UserType) VALUES ('Manus', 'manager', 'manager', 'Manager')
