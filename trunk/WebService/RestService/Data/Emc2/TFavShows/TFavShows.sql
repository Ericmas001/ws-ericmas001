USE [emc2]
GO

IF OBJECT_ID('[ericmas001].[TFavShows]') IS NOT NULL
DROP TABLE [ericmas001].[TFavShows]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ericmas001].[TFavShows](
	[idUser] [int] NOT NULL,
	[idShow] [int] NOT NULL,
	[lastViewedSeason] [int] NULL,
	[lastViewedEpisode] [int] NULL,
 CONSTRAINT [PK_TFavShows] PRIMARY KEY CLUSTERED 
(
	[idUser] ASC,
	[idShow] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


