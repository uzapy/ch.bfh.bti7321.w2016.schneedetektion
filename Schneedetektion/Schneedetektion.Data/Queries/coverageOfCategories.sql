select distinct [Polygons].CameraName, [Combined_Statistics].CombinationMethod, count(*) as [Count]
from Combined_Statistics, Polygons
where Combined_Statistics.Polygon_ID = Polygons.ID
group by [CombinationMethod], [Polygons].CameraName
order by [Polygons].CameraName

select top(100) len([Statistics].[GreenHistogram]) as [length] from [Statistics]

select top(100) len([Combined_Statistics].[Images]) as [length] from [Combined_Statistics]