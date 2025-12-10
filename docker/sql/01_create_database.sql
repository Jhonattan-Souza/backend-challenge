-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CnabProcessor')
BEGIN
    CREATE DATABASE CnabProcessor;
END
GO

USE CnabProcessor;
GO

-- Create StoreOwners table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StoreOwners')
BEGIN
    CREATE TABLE StoreOwners (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Cpf NVARCHAR(11) NOT NULL,
        CreatedAt DATETIMEOFFSET NOT NULL,
        UpdatedAt DATETIMEOFFSET NOT NULL
    );
    
    CREATE UNIQUE INDEX IX_StoreOwners_Cpf ON StoreOwners(Cpf);
END
GO

-- Create Store table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Store')
BEGIN
    CREATE TABLE Store (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        OwnerId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIMEOFFSET NOT NULL,
        UpdatedAt DATETIMEOFFSET NOT NULL,
        CONSTRAINT FK_Store_StoreOwners FOREIGN KEY (OwnerId) REFERENCES StoreOwners(Id)
    );
    
    CREATE UNIQUE INDEX IX_StoreName ON Store(Name);
END
GO

-- Create Transactions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
BEGIN
    CREATE TABLE Transactions (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Type INT NOT NULL,
        Date DATETIMEOFFSET NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        Cpf NVARCHAR(11) NOT NULL,
        CardNumber NVARCHAR(12) NOT NULL,
        StoreId UNIQUEIDENTIFIER NOT NULL,
        LineHash NVARCHAR(64) NOT NULL,
        CreatedAt DATETIMEOFFSET NOT NULL,
        UpdatedAt DATETIMEOFFSET NOT NULL,
        CONSTRAINT FK_Transactions_Store FOREIGN KEY (StoreId) REFERENCES Store(Id)
    );
    
    CREATE UNIQUE INDEX IX_Transactions_LineHash ON Transactions(LineHash);
    CREATE INDEX IX_Transactions_Cpf ON Transactions(Cpf);
    CREATE INDEX IX_Transactions_StoreId ON Transactions(StoreId);
END
GO

PRINT 'Database schema created successfully!';
GO
