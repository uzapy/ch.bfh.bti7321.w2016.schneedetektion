select distinct [Polygons].CameraName, [Combined_Statistics].CombinationMethod, count(*) as [Count]
from Combined_Statistics, Polygons
where Combined_Statistics.Polygon_ID = Polygons.ID
group by [CombinationMethod], [Polygons].CameraName
order by [Polygons].CameraName