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
IF OBJECT_ID('SPUserValidToken') IS NOT NULL
DROP PROC SPUserValidToken
GO
-- =============================================
-- Author:		ericmas001
-- Create date: 2013-03-16
-- Description:	Check if a token is still valid
-- =============================================
CREATE PROCEDURE SPUserValidToken 
	-- Add the parameters for the stored procedure here
	@username NVARCHAR(50), 
	@session NVARCHAR(32),
	@ok BIT = 0 OUT,
	@info NVARCHAR(100) OUT,
	@validUntil DATETIME OUT,
	@idUser INT OUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SET @ok = 0
	SELECT @ok = 1, @idUser = idUser from [ericmas001].[TUser] where username = @username AND sessionToken = @session AND GETDATE() < validUntil

	IF @ok = 1
	BEGIN
		EXEC [ericmas001].[SPUserRefresh] @idUser, @validUntil output
		SELECT @info = @session
	END
	ELSE
	BEGIN
		SET @info = 'The token is not valid anymore'
	END
END
GO