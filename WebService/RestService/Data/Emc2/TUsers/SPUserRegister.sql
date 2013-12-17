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
IF OBJECT_ID('SPUserRegister') IS NOT NULL
DROP PROC SPUserRegister
GO
-- =============================================
-- Author:		ericmas001
-- Create date: 2013-03-16
-- Description:	Register a user
-- =============================================
CREATE PROCEDURE SPUserRegister 
	-- Add the parameters for the stored procedure here
	@username NVARCHAR(50), 
	@password NVARCHAR(50), 
	@email NVARCHAR(100),
	@ok BIT OUT,
	@info NVARCHAR(100) OUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SET @ok = 1
	SELECT @ok = 0 from [ericmas001].[TUser] where username = @username

	IF @ok = 1
	BEGIN
		DECLARE @hashed NVARCHAR(32)
		SELECT @hashed = CONVERT(NVARCHAR(32),HashBytes('MD5', @password),2)

		INSERT INTO [ericmas001].[TUser] (username, password, email) Values(@username, @hashed, @email)
	END
	ELSE
	BEGIN
		SET @info = 'The username already exists'
	END
END
GO
