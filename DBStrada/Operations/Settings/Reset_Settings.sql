CREATE PROCEDURE [dbo].[Reset_Settings]
AS
BEGIN
	DELETE FROM [Settings]

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'EnableLoginWithCode'				, N'true'	, N'Enable or disable login with code feature')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'MaxLoginAttempts'				, N'5'		, N'Maximum number of login attempts before lockout')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'EnableUsersToResetPassword'		, N'true'	, N'Allow users to reset their passwords')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'CodeResendLimit'					, N'3'		, N'Maximum number of code resends allowed')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'CodeExpiryMinutes'				, N'10'		, N'Expiry time for codes in minutes')

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'LedgerCodePrefix'				, N'LD'		, N'Prefix for Ledger Codes')
	
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'VehicleTypeCodePrefix'				, N'VHTY'	, N'Prefix for Vehicle Type Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'DocumentTypeCodePrefix'				, N'DCTY'	, N'Prefix for Document Type Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'OMCCodePrefix'						, N'OMC'	, N'Prefix for OMC Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'VehicleRouteLocationCodePrefix'		, N'VHRL'	, N'Prefix for Route Location Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'VehicleDriverCodePrefix'				, N'VHDR'	, N'Prefix for Vehicle Driver Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'VehicleRouteExpenseTypeCodePrefix'	, N'VREX'	, N'Prefix for Vehicle Route Expense Type Codes')
	
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'FinancialAccountingTransactionPrefix'	, N'ACCT'	, N'Prefix for Financial Accounting Transaction Numbers')

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'PrimaryCompanyLinkingId'			, N'1'		, N'Company Id for the Primary Company Account')

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'CashLedgerId'				, N'1', N'Cash ledger account for Cash Entries')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'GSTLedgerId'					, N'2', N'GST ledger account for GST Tax Entries')

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'DefaultSelectedVoucherId'	, N'1', N'Default selected voucher type in transactions')

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'AutoRefreshReportTimer'		, N'5', N'Auto refresh interval for reports in minutes')

END