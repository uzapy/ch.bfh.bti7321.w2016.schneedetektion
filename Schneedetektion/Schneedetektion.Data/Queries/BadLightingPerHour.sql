select datepart(hour, [Images].[DateTime]), count(*), [Images].Place from Images
where [Images].BadLighting = 1
group by datepart(hour, [Images].[DateTime]), [Images].Place
order by datepart(hour, [Images].[DateTime]), [Images].Place