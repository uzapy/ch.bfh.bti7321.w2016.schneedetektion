select distinct [Polygons].CameraName, CombinationMethod from [Combined_Statistics], [Polygons]
where Polygons.ID = Combined_Statistics.Polygon_ID

select distinct [StartOfWeek] from [Combined_Statistics]
where [Polygon_ID] in (select ID from [Polygons] where [CameraName] = 'mvk120')
order by [StartOfWeek]

select distinct [Polygons].CameraName from [Entity_Statistics], [Polygons]
where Polygons.ID = [Entity_Statistics].Polygon_ID and Image_ID is not null

select count(*) from [Entity_Statistics]
where [Entity_Statistics].[Polygon_ID] in (104,105,106,107)