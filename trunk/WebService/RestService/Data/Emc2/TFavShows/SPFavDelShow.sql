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
IF OBJECT_ID('SPFavDelShow') IS NOT NULL
DROP PROC SPFavDelShow
GO
-- =============================================
-- Author:		ericmas001
-- Create date: 2013-05-19
-- Description:	Delete a favorite show to the database for a specific user
-- =============================================
CREATE PROCEDURE SPFavDelShow 
	-- Add the parameters for the stored procedure here
	@username NVARCHAR(50),
	@session NVARCHAR(32),
	@website NVARCHAR(50), 
	@name NVARCHAR(50), 
	@ok BIT OUT,
	@info NVARCHAR(100) OUT,
	@validUntil DATETIME OUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	

	-- Get the show information, and if it doesn't exist, create the show !
	DECLARE	@idShow INT
	SET @ok = 0
	SELECT @ok = 1, @idShow = idShow from [ericmas001].[TTvShow] where website = @website AND showname = @name
	
	-- If the show was already there or added correctly, we can continue
	IF @ok = 1
	BEGIN
		DECLARE	@idUser INT
		EXEC [ericmas001].[SPUserValidToken] @username, @session, @ok output, @info output, @validUntil output, @idUser output
		IF @ok = 1	
		BEGIN
			SET @ok = 0
			SELECT @ok = 1 from [ericmas001].[TFavShows] where idUser = @idUser AND idShow = @idShow
			IF @ok = 1
				DELETE FROM [ericmas001].[TFavShows] WHERE idUser = @idUser AND idShow = @idShow
			ELSE
				SET @info = 'The show was not a favorite for this user'
		END
	END
	ELSE
		SET @info = 'The show does not exist'
END
GO
