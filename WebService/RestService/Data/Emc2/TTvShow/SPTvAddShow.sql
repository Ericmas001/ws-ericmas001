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
IF OBJECT_ID('SPTvAddShow') IS NOT NULL
DROP PROC SPTvAddShow
GO
-- =============================================
-- Author:		ericmas001
-- Create date: 2013-03-17
-- Description:	Add a new show to the database
-- =============================================
CREATE PROCEDURE SPTvAddShow 
	-- Add the parameters for the stored procedure here
	@website NVARCHAR(50), 
	@name NVARCHAR(50), 
	@title NVARCHAR(100),
	@lastSeason INT,
	@lastEpisode INT,
	@ok BIT OUT,
	@info NVARCHAR(100) OUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SET @ok = 1
	SELECT @ok = 0 from [ericmas001].[TTvShow] where website = @website AND showname = @name

	IF @ok = 1
		INSERT INTO [ericmas001].[TTvShow] (website, showname, showtitle, lastSeason, lastEpisode) Values(@website, @name, @title, @lastSeason, @lastEpisode)
	ELSE
		SET @info = 'The show already exists for this website'
END
GO
