CREATE TABLE [dbo].[Expense]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [TransactionNo] VARCHAR(100) NOT NULL UNIQUE,
    [CompanyId] INT NOT NULL,
    [TransactionDateTime] DATETIME NOT NULL,
    [FinancialYearId] INT NOT NULL,
    [VehicleId] INT NOT NULL,
	[TotalItems] INT NOT NULL,
    [TotalExpense] MONEY NOT NULL,
    [Remarks] VARCHAR(MAX) NULL,
	[CreatedBy] INT NOT NULL,
	[CreatedAt] DATETIME NOT NULL DEFAULT (((getdate() AT TIME ZONE 'UTC') AT TIME ZONE 'India Standard Time')),
	[CreatedFromPlatform] VARCHAR(MAX) NOT NULL,
	[Status] BIT NOT NULL DEFAULT 1,
	[LastModifiedBy] INT NULL,
	[LastModifiedAt] DATETIME NULL, 
	[LastModifiedFromPlatform] VARCHAR(MAX) NULL, 
	CONSTRAINT [FK_Expense_ToCompany] FOREIGN KEY ([CompanyId]) REFERENCES [Company]([Id]),
    CONSTRAINT [FK_Expense_ToFinancialYear] FOREIGN KEY ([FinancialYearId]) REFERENCES [FinancialYear](Id),
    CONSTRAINT [FK_Expense_ToVehicle] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicle]([Id]),
    CONSTRAINT [FK_Expense_CreatedBy_ToUser] FOREIGN KEY ([CreatedBy]) REFERENCES [User]([Id]),
	CONSTRAINT [FK_Expense_LastModifiedBy_ToUser] FOREIGN KEY ([LastModifiedBy]) REFERENCES [User]([Id])
)
