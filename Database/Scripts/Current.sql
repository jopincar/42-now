-- drop table player
IF NOT EXISTS (SELECT 1 FROM SYS.Objects WHERE Name = 'Player')
BEGIN
	CREATE TABLE Player (
		PlayerId int not null identity,
		PlayerName varchar(20) not null,
		Password varchar(50) not null,
		Email varchar(255) not null,
		PasswordQuestion varchar(500) null,
		PasswordAnswer varchar(500) null,
		IsApproved bit not null,
		IsLockedOut bit not null default(0),
		Comment varchar(250) not null default '',
		CreationDate datetime not null default GETDATE(),
		LastLoginDate  datetime null,
		LastActivityDate datetime null,
		LastPasswordChangeDate datetime null,
		LastLockoutDate datetime null,
	)
	ALTER TABLE Player ADD CONSTRAINT PK_Player PRIMARY KEY (PlayerId)
	ALTER TABLE Player ADD CONSTRAINT U_Player_PlayerName UNIQUE (PlayerName)
	ALTER TABLE Player ADD CONSTRAINT U_Player_Email UNIQUE (Email)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'Game')
BEGIN
	CREATE TABLE Game (
		GameId int IDENTITY not null,
		StartDate datetime not null,
		EndDate datetime not null
	)
	ALTER TABLE Game ADD CONSTRAINT PK_Game_GameId PRIMARY KEY (GameId)
	CREATE INDEX IX_Game_StartDate ON Game(StartDate)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'GamePlayer')
BEGIN
	CREATE TABLE GamePlayer (
		GamePlayerId int IDENTITY not null,
		GameId int not null,
		PlayerId int not null,
		Seat int not null,
		MarksWon int not null default (0),
		MarksLost int not null default (1)
	)
	ALTER TABLE GamePlayer ADD CONSTRAINT PK_GamePlayer_GamePlayerId PRIMARY KEY NONCLUSTERED (GamePlayerId)
	ALTER TABLE GamePlayer ADD CONSTRAINT UC_GamePlayer_PlayerId_GameId UNIQUE CLUSTERED (PlayerId, GameId)
	ALTER TABLE GamePlayer ADD CONSTRAINT FK_GamePlayer_GameId FOREIGN KEY (GameId) REFERENCES Game(GameId)
	ALTER TABLE GamePlayer ADD CONSTRAINT FK_GamePlayer_PlayerId FOREIGN KEY (PlayerId) REFERENCES Player(PlayerId)
	CREATE INDEX IX_GamePlayer_GameId_Seat ON GamePlayer(GameId, Seat)
END
GO

SET IDENTITY_INSERT Player ON
GO

IF NOT EXISTS ( SELECT * FROM Player WHERE PlayerId = -1 ) 
BEGIN
	INSERT Player (PlayerId, PlayerName, Password, Email, IsApproved, IsLockedOut, Comment, CreationDate)
	VALUES (-1, 'Monica', '', '', 1, 0, '', GETDATE())
END
GO

IF NOT EXISTS ( SELECT * FROM Player WHERE PlayerId = -2 ) 
BEGIN
	INSERT Player (PlayerId, PlayerName, Password, Email, IsApproved, IsLockedOut, Comment, CreationDate)
	VALUES (-2, 'Alex', '', 'alex@alex.com', 1, 0, '', GETDATE())
END
GO

IF NOT EXISTS ( SELECT * FROM Player WHERE PlayerId = -3 ) 
BEGIN
	INSERT Player (PlayerId, PlayerName, Password, Email, IsApproved, IsLockedOut, Comment, CreationDate)
	VALUES (-3, 'Lila', '', 'lila@lila.com', 1, 0, '', GETDATE())
END
GO

SET IDENTITY_INSERT Player OFF
GO

if not exists (select * from information_schema.columns where table_name = 'game' and column_name = 'EndDate' and is_nullable = 'YES')
	alter table game alter column EndDate datetime null
	
IF NOT EXISTS (SELECT * FROM information_schema.columns where table_name = 'gameplayer' and column_name = 'LowMarksWon')
	ALTER TABLE GamePlayer ADD LowMarksWon int not null default 0
GO
	
IF NOT EXISTS (SELECT * FROM information_schema.columns where table_name = 'gameplayer' and column_name = 'LowMarksLost')
	ALTER TABLE GamePlayer ADD LowMarksLost int not null default 0
GO

IF NOT EXISTS (SELECT * FROM information_schema.columns where table_name = 'gameplayer' and column_name = 'BidsSetSum')
	ALTER TABLE GamePlayer ADD BidsSetSum int null
GO

IF NOT EXISTS (SELECT * FROM information_schema.columns where table_name = 'gameplayer' and column_name = 'BidsMadeSum')
	ALTER TABLE GamePlayer ADD BidsMadeSum int null
GO

IF NOT EXISTS (SELECT * FROM information_schema.tables where table_name = 'GamePlayerBid')
BEGIN
	CREATE TABLE GamePlayerBid (
		GamePlayerBidId int not null IDENTITY,
		PlayerId int not null,
		GameId int not null,
		HandCount int not null,
		BidPosition int not null,
		Amount int not null,
		TrumpSuit int not null,
		IsLow bit not null,
		IsWinning bit not null,
		WasMade bit not null,
		TricksPlayed int not null,
		Hand nvarchar(50) not null,
		CreateDate datetime not null default GETDATE()
	)
	ALTER TABLE GamePlayerBid ADD CONSTRAINT PK_GamePlayerBid_GamePlayerBidId PRIMARY KEY NONCLUSTERED (GamePlayerBidId)
	ALTER TABLE GamePlayerBid ADD CONSTRAINT U_GamePlayerBid_PlayerId_GameId_HandCount UNIQUE CLUSTERED (PlayerId, GameId, HandCount)
	ALTER TABLE GamePlayerBid ADD CONSTRAINT FK_GamePlayerBid_PlayerId_GameId FOREIGN KEY (PlayerId, GameId) REFERENCES GamePlayer(PlayerId, GameId)
END
GO

IF NOT EXISTS (SELECT * FROM information_schema.columns where table_name = 'game' and column_name = 'HintCount')
	ALTER TABLE Game ADD HintCount int not null default 0
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Game' AND COLUMN_NAME = 'FirstHintDate')
BEGIN
	ALTER TABLE Game ADD FirstHintDate datetime null
END

IF NOT EXISTS (SELECT 1 FROM SYS.Objects WHERE Name = 'SavedHand')
BEGIN
	CREATE TABLE SavedHand (
		SavedHandId int not null IDENTITY,
		PlayerId int not null,
		Hand varchar(50) not null,
		CreateDate datetime not null default GETDATE()
	)
	ALTER TABLE SavedHand ADD CONSTRAINT PK_SavedHand_SavedHandId PRIMARY KEY (SavedHandId)
	ALTER TABLE SavedHand ADD CONSTRAINT U_SavedHand_PlayerId_Hand UNIQUE (PlayerId, Hand)
	ALTER TABLE SavedHand ADD CONSTRAINT FK_SavedHand_PlayerId FOREIGN KEY (PlayerId) REFERENCES Player(PlayerId)
END
