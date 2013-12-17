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
IF OBJECT_ID('SPTvLastEpisode') IS NOT NULL
DROP PROC SPTvLastEpisode
GO
-- =============================================
-- Author:		ericmas001
-- Create date: 2013-03-17
-- Description:	Set the last season and the last episode
-- =============================================
CREATE PROCEDURE SPTvLastEpisode 
	-- Add the parameters for the stored procedure here
	@website NVARCHAR(50), 
	@name NVARCHAR(50), 
	@lastSeason INT,
	@lastEpisode INT,
	@ok BIT OUT,
	@info NVARCHAR(100) OUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE	@oldLastSeason INT
	DECLARE	@oldLastEpisode INT
	SET @ok = 0
	SELECT @ok = 1, @oldLastSeason = lastSeason, @oldLastEpisode = lastEpisode  from [ericmas001].[TTvShow] where website = @website AND showname = @name

	IF @ok = 1
	BEGIN
		IF @lastSeason = @oldLastSeason AND @lastEpisode = @oldLastEpisode
			SET @info = 'Unchanged.'
		ELSE
		BEGIN
			SET @info = 'Changed ' + CAST(@oldLastSeason AS VARCHAR) + 'x' + CAST(@oldLastEpisode AS VARCHAR) + ' => ' + CAST(@lastSeason AS VARCHAR) + 'x' + CAST(@lastEpisode AS VARCHAR)
			UPDATE [ericmas001].[TTvShow] SET lastSeason = @lastSeason, lastEpisode = @lastEpisode WHERE website = @website AND showname = @name
		END
	END
	ELSE
		SET @info = 'The show does not exists for this website'
END
GO
