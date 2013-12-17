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
IF OBJECT_ID('SPTvAllShows') IS NOT NULL
DROP PROC SPTvAllShows
GO
-- =============================================
-- Author:		ericmas001
-- Create date: 2013-03-17
-- Description:	Get the useful information of all shows
-- =============================================
CREATE PROCEDURE SPTvAllShows 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT website, showname, showtitle, lastSeason, lastEpisode FROM [ericmas001].[TTvShow]
END
GO
