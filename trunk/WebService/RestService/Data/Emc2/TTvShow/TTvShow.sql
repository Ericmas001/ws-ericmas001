USE [emc2]
GO

IF OBJECT_ID('[ericmas001].[TTvShow]') IS NOT NULL
DROP TABLE [ericmas001].[TTvShow]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ericmas001].[TTvShow](
	[idShow] [int] IDENTITY(1,1) NOT NULL,
	[website] [nvarchar](50) NOT NULL,
	[showname] [nvarchar](50) NOT NULL,
	[showtitle] [nvarchar](100) NULL,
	[lastSeason] [int] NULL,
	[lastEpisode] [int] NULL,
 CONSTRAINT [PK_TTvShow] PRIMARY KEY CLUSTERED 
(
	[idShow] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


