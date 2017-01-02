select distinct [Polygons].CameraName, [Combined_Statistics].CombinationMethod, count(*) as [Count]
from Combined_Statistics, Polygons
where Combined_Statistics.Polygon_ID = Polygons.ID
group by [CombinationMethod], [Polygons].CameraName
order by [Polygons].CameraName

select distinct CAST(FLOOR(CAST([Images].[DateTime] as FLOAT)) as DateTime) as dates from [Images]
order by dates

select count(*) from Images

select max([Images].[DateTime]), [Place] from [Images]
group by [Images].[Place]
order by [Images].[Place]

select max([Images].[DateTime]), [Images].[Place] from [Images]
where [Images].[Snow] = 1 and [Images].[DateTime] > '2015-06-01'
group by [Images].[Place]
order by [Images].[Place]

select distinct [Combined_Statistics].[StartOfWeek] from [Combined_Statistics]
order by [Combined_Statistics].[StartOfWeek]