
--drop table GamePlayerBidNew
CREATE TABLE [dbo].[GamePlayerBidNew] (
	[GamePlayerBidId] [int] IDENTITY(1,1) NOT NULL,
	[PlayerId] [int] NOT NULL,
	[GameId] [int] NOT NULL,
	[HandCount] [tinyint] NOT NULL,
	[BidPosition] [tinyint] NOT NULL,
	[Amount] [tinyint] NOT NULL,
	[TrumpSuit] [smallint] NOT NULL,
	[IsLow] [bit] NOT NULL,
	[IsWinning] [bit] NOT NULL,
	[WasMade] [bit] NOT NULL,
	[TricksPlayed] [tinyint] NOT NULL,
	[Hand] [nvarchar](50) NOT NULL,
	[CreateDate] [smalldatetime] NOT NULL
)
GO

SET Identity_Insert GamePlayerBidNew ON
GO

INSERT GamePlayerBidNew (
	GamePlayerBidId,
	PlayerId,
	GameId,
	HandCount,
	BidPosition,
	Amount,
	TrumpSuit,
	IsLow,
	IsWinning,
	WasMade,
	TricksPlayed,
	Hand,
	CreateDate
)
SELECT 
	GamePlayerBidId,
	PlayerId,
	GameId,
	HandCount,
	BidPosition,
	Amount,
	TrumpSuit,
	IsLow,
	IsWinning,
	WasMade,
	TricksPlayed,
	Hand,
	CreateDate
FROM GamePlayerBid
GO
SET Identity_Insert GamePlayerBidNew OFF
GO

drop table GamePlayerBid
GO

sp_rename GamePlayerBidNew, GamePlayerBid
GO

ALTER TABLE GamePlayerBid ADD CONSTRAINT [PK_GamePlayerBid_GamePlayerBidId] PRIMARY KEY NONCLUSTERED ([GamePlayerBidId] ASC)
ALTER TABLE GamePlayerBid ADD CONSTRAINT [U_GamePlayerBid_PlayerId_GameId_HandCount] UNIQUE CLUSTERED (	[PlayerId] ASC,	[GameId] ASC,[HandCount] ASC )
ALTER TABLE [dbo].[GamePlayerBid]  WITH CHECK ADD  CONSTRAINT [FK_GamePlayerBid_PlayerId_GameId] FOREIGN KEY([PlayerId], [GameId]) REFERENCES [dbo].[GamePlayer] ([PlayerId], [GameId])
GO

ALTER TABLE [dbo].[GamePlayerBid] CHECK CONSTRAINT [FK_GamePlayerBid_PlayerId_GameId]
GO

ALTER TABLE [dbo].[GamePlayerBid] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO


select top 10 * from gameplayerbid order by gameplayerbidid desc