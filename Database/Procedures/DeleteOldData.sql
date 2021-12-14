if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DeleteOldData]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].DeleteOldData
GO

CREATE PROCEDURE DeleteOldData
	@age int = 90
AS
BEGIN
	DECLARE @dateOf datetime
	SELECT @dateOf = DATEADD(day, -@age, getdate())
	
	DELETE B
	from Game G
	join GamePlayer GP on GP.GameId = G.GameId
	join GamePlayerBid B ON B.GameId = GP.GameId AND B.PlayerId = GP.PlayerId
	where G.StartDate < @dateOf

	DELETE GP
	from Game G
	join GamePlayer GP on GP.GameId = G.GameId
	where G.StartDate < @dateOf

	DELETE T
	from Game G
	join Trick T ON T.GameId = G.GameId
	where G.StartDate < @dateOf

	DELETE G
	from Game G
	where G.StartDate < @dateOf
END

-- EXEC DeleteOldData 89