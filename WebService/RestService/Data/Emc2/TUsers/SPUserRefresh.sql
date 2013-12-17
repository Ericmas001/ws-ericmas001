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
IF OBJECT_ID('SPUserRefresh') IS NOT NULL
DROP PROC SPUserRefresh
GO
-- =============================================
-- Author:		ericmas001
-- Create date: 2013-03-16
-- Description:	Refresh the session token of a user
-- =============================================
CREATE PROCEDURE SPUserRefresh 
	-- Add the parameters for the stored procedure here
	@idUser INT,
	@validUntil DATETIME OUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @now DATETIME
	SELECT @now = GETDATE()
	
	DECLARE @ok BIT = 0
	SELECT @ok = 1 FROM [ericmas001].[TUser] where idUser = @idUser AND @now < validUntil

	IF @ok = 1
	BEGIN
		DECLARE @expire DATETIME
		SELECT @expire = DATEADD(minute, 5, @now)

		UPDATE [ericmas001].[TUser] SET validUntil = @expire WHERE idUser = @idUser
		SELECT @validUntil=@expire
	END
END
GO
