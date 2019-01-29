
USE WhatIfDemoDb;
GO

CREATE TABLE [dbo].[Policies] (
    [id]            UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [userId]        NVARCHAR (255)   NOT NULL,
    [productId]     NVARCHAR (255)   NOT NULL,
    [paymentAmount] DECIMAL (18, 2)  NOT NULL,
    [dateCreated]   DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

CREATE TABLE [dbo].[Claims] (
    [id]          UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [userId]      NVARCHAR (255)   NOT NULL,
    [licenseId]   NVARCHAR (24)    NOT NULL,
    [dateCreated] DATETIME         DEFAULT (getdate()) NOT NULL,
    [amount]      DECIMAL (18, 2)  NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);
GO
