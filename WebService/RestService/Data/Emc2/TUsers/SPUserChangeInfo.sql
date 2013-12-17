-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID('SPUserChangeInfo') IS NOT NULL
DROP PROC SPUserChangeInfo
GO
-- =============================================
-- Author:		ericmas001
-- Create date: 2013-03-16
-- Description: Change info of a user
-- =============================================
CREATE PROCEDURE SPUserChangeInfo 
	-- Add the parameters for the stored procedure here
	@username NVARCHAR(50), 
	@session NVARCHAR(32),
	@email NVARCHAR(100),
	@ok BIT = 0 OUT,
	@info NVARCHAR(100) OUT,
	@validUntil DATETIME OUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	EXEC [ericmas001].[SPUserValidToken] @username, @session, @ok output, @info output, @validUntil output

	IF @ok = 1
	BEGIN
		UPDATE [ericmas001].[TUser] SET email = @email where username = @username
	END
END
GO