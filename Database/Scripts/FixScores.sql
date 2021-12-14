IF NOT EXISTS (SELECT * FROM information_schema.columns where table_name = 'game' and column_name = 'HintCount')
	ALTER TABLE Game ADD HintCount int not null default 0
GO

select top 20 * from Game order by GameId desc

select * from GamePlayer where GameId = 152

select * from Player where PlayerId = 54

select top 10 * from gameplayerbid

select * from gameplayerbid where gameid = 124 and iswinning = 1
select * from gameplayer where gameid = 124

update G set winningteam = case when won0 >= won1 THEN 0 ELSE 1 END
select g.GameId, g.WinningTeam, won0, won1
from Game g
join (
select GameId, 
	won0 = SUM(CASE WHEN Seat % 2 = 0 THEN markswon ELSE 0 END + CASE WHEN Seat % 2 = 1 THEN marksLost ELSE 0 END),
	won1 = SUM(CASE WHEN Seat % 2 = 1 THEN markswon ELSE 0 END + CASE WHEN Seat % 2 = 0 THEN marksLost ELSE 0 END)
from GamePlayer GP
group by GameId
) xx on xx.GameId = g.GameId
