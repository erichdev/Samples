ALTER FUNCTION [dbo].[getPunchHoursByDay] (@Start datetime, @End datetime)

RETURNS TABLE 
AS
RETURN 
(
	SELECT
            Punch.fkEmployeeID,
            Format([datPunch], 'd') AS [Date],
            SUM((IIf([bIn] = 1, (-1) * Cast([datPunch] as float), Cast([datPunch] as float)) * 24)) AS Hours,
            Cast(ROW_NUMBER() over (order by fkEmployeeId) as int) as Id
        FROM Punch
        WHERE (((Punch.datPunch) >= @Start
        AND (Punch.datPunch) <= DateAdd(DAY, 1, @End)))
        GROUP BY Punch.fkEmployeeID,
                Format([datPunch], 'd')
   
)
