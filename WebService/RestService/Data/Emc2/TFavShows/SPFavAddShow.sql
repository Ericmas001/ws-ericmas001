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
IF OBJECT_ID('SPFavAddShow') IS NOT NULL
DROP PROC SPFavAddShow
GO
-- =============================================
-- Author:		ericmas001
-- Create date: 2013-05-01
-- Description:	Add a new favorite show to the database
-- =============================================
CREATE PROCEDURE SPFavAddShow 
	-- Add the parameters for the stored procedure here
	@username NVARCHAR(50),
	@session NVARCHAR(32),
	@website NVARCHAR(50), 
	@name NVARCHAR(50), 
	@title NVARCHAR(100),
	@lastSeason INT,
	@lastEpisode INT,
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
	
	IF @ok = 0
	BEGIN
		EXEC [ericmas001].[SPTvAddShow] @website, @name, @title, @lastSeason, @lastEpisode, @ok output, @info output
		IF @ok = 1
		BEGIN
			SET @ok = 0
			SELECT @ok = 1, @idShow = idShow from [ericmas001].[TTvShow] where website = @website AND showname = @name
			IF @ok = 0
				SET @info = 'The show does not exists for this website'
		END
	END
	
	-- If the show was already there or added correctly, we can continue
	IF @ok = 1
	BEGIN
		DECLARE	@idUser INT
		EXEC [ericmas001].[SPUserValidToken] @username, @session, @ok output, @info output, @validUntil output, @idUser output
		IF @ok = 1	
		BEGIN
			SELECT @ok = 0 from [ericmas001].[TFavShows] where idUser = @idUser AND idShow = @idShow
			IF @ok = 1
				INSERT INTO [ericmas001].[TFavShows] (idUser, idShow) Values(@idUser, @idShow)
			ELSE
				SET @info = 'The show is already a favorite for this user '
		END
	END
END
GO
