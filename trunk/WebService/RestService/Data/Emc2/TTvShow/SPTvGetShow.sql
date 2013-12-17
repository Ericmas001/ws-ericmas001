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
IF OBJECT_ID('SPTvGetShow') IS NOT NULL
DROP PROC SPTvGetShow
GO
-- =============================================
-- Author:		ericmas001
-- Create date: 2013-03-17
-- Description:	Get the useful information of a specific show
-- =============================================
CREATE PROCEDURE SPTvGetShow 
	-- Add the parameters for the stored procedure here
	@website NVARCHAR(50), 
	@name NVARCHAR(50), 
	@ok BIT OUT,
	@info NVARCHAR(100) OUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SET @ok = 0
	SELECT @ok = 1 from [ericmas001].[TTvShow] where website = @website AND showname = @name

	IF @ok = 1
		SELECT website, showname, showtitle, lastSeason, lastEpisode FROM [ericmas001].[TTvShow] WHERE website = @website AND showname = @name
	ELSE
		SET @info = 'The show does not exists for this website'
END
GO
