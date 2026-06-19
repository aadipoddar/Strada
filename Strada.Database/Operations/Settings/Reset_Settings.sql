CREATE PROCEDURE [dbo].[Reset_Settings]
AS
BEGIN
	DELETE FROM [Settings]

	-- Primary Configuration
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'PrimaryCompanyLinkingId', N'1', N'Company Id for the Primary Company Account')

	-- Login Settings
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'EnableLoginWithCode', N'true', N'Enable or disable login with code feature')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'EnableUsersToResetPassword', N'true', N'Allow users to reset their passwords')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'MaxLoginAttempts', N'5', N'Maximum number of login attempts before lockout')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'CodeResendLimit', N'3', N'Maximum number of code resends allowed')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'CodeExpiryMinutes', N'10', N'Expiry time for codes in minutes')

	-- Master Code Prefixes
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'LedgerCodePrefix', N'LD', N'Prefix for Ledger Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'VehicleTypeCodePrefix', N'VHTY', N'Prefix for Vehicle Type Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'DocumentTypeCodePrefix', N'DCTY', N'Prefix for Document Type Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'OMCCodePrefix', N'OMC', N'Prefix for OMC Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'OMCCardCodePrefix', N'OMCC', N'Prefix for OMC Card Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'LocationCodePrefix', N'LC', N'Prefix for Location Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'RouteCodePrefix', N'RT', N'Prefix for Route Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'DriverCodePrefix', N'DR', N'Prefix for Driver Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'ExpenseTypeCodePrefix', N'ET', N'Prefix for Expense Type Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'TyreCompanyCodePrefix', N'TC', N'Prefix for Tyre Company Codes')

	-- Transaction Prefixes
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'FinancialAccountingTransactionPrefix', N'AC', N'Prefix for Financial Accounting Transaction Numbers')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'TripTransactionPrefix', N'TR', N'Prefix for Trip Transaction Numbers')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'BillTransactionPrefix', N'BL', N'Prefix for Bill Transaction Numbers')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'ExpenseTransactionPrefix', N'EX', N'Prefix for Expense Transaction Numbers')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'OMCCardMoneyTransferTransactionPrefix', N'OMCMT', N'Prefix for OMC Card Money Transfer Transaction Numbers')

	-- Ledger Linking
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'CashLedgerId', N'1', N'Cash ledger account for Cash Entries')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'GSTLedgerId', N'2', N'GST ledger account for GST Tax Entries')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'BillLedgerId', N'9', N'Ledger account for Bill entries')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'OMCCardMoneyTransferLedgerId', N'12', N'Ledger account for OMC Card Money Transfer entries')

	-- Bank Reconciliation
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'BankAccountTypeId', N'2', N'Account Type that identifies Bank ledgers for reconciliation')

	-- Default Values
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'DefaultSelectedVoucherId', N'1', N'Default selected voucher type in transactions')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'BillVoucherId', N'2', N'Voucher type for Bill transactions')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'OMCCardMoneyTransferVoucherId', N'3', N'Voucher type for OMC Card Money Transfer transactions')

	-- Report Settings
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'AutoRefreshReportTimer', N'5', N'Auto refresh interval for reports in minutes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'ReportWarningDays', N'30', N'Days threshold used to highlight due items in reports')

	-- Notification Settings
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'NotificationEmail', N'ajay@ashokroadlines.com', N'Recipient email for transaction notifications; leave blank to disable emails')

END
