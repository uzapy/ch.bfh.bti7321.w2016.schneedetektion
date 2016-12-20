select
[Images].ID, [Statistics].ModeBlue, [Statistics].ModeGreen, [Statistics].ModeRed,
[Statistics].MeanBlue, [Statistics].MeanGreen, [Statistics].MeanRed,
[Statistics].MedianBlue, [Statistics].MedianGreen, [Statistics].MedianRed,
[Statistics].MinimumBlue, [Statistics].MinimumGreen, [Statistics].MinimumRed,
[Statistics].MaximumBlue, [Statistics].MaximumGreen, [Statistics].MaximumRed,
[Statistics].StandardDeviationBlue, [Statistics].StandardDeviationGreen, [Statistics].StandardDeviationRed,
[Statistics].ContrastBlue, [Statistics].ContrastGreen, [Statistics].ContrastRed,
[Images].UnixTime, [Images].Snow,[Images].NoSnow, [Images].Night, [Images].Dusk, [Images].[Day],
[Images].Foggy, [Images].Cloudy, [Images].Rainy, [Images].BadLighting, [Images].GoodLighting,
[Entity_Statistics].Polygon_ID
from Entity_Statistics, [Statistics], [Images]
where Entity_Statistics.Statistics_ID = [Statistics].ID
and Entity_Statistics.Image_ID = Images.ID
and Entity_Statistics.Polygon_ID IN (100,101,102,103)
order by [Images].ID


-- ModeBlue,ModeGreen,ModeRed,MeanBlue,MeanGreen,MeanRed,MedianBlue,MedianGreen,MedianRed,MinimumBlue,MinimumGreen,MinimumRed,MaximumBlue,MaximumGreen,MaximumRed,StandardDeviationBlue,StandardDeviationGreen,StandardDeviationRed,ContrastBlue,ContrastGreen,ContrastRed,UnixTime,Snow,NoSnow,Night,Dusk,Day,Foggy,Cloudy,Rainy,BadLighting,GoodLighting,Polygon_ID

select
[Images].ID, [Statistics].ModeBlue, [Statistics].ModeGreen, [Statistics].ModeRed,
[Statistics].MeanBlue, [Statistics].MeanGreen, [Statistics].MeanRed,
[Statistics].MedianBlue, [Statistics].MedianGreen, [Statistics].MedianRed,
[Statistics].MinimumBlue, [Statistics].MinimumGreen, [Statistics].MinimumRed,
[Statistics].MaximumBlue, [Statistics].MaximumGreen, [Statistics].MaximumRed,
[Statistics].StandardDeviationBlue, [Statistics].StandardDeviationGreen, [Statistics].StandardDeviationRed,
[Statistics].ContrastBlue, [Statistics].ContrastGreen, [Statistics].ContrastRed,
[Images].UnixTime, [Images].Snow,[Images].NoSnow, [Images].Night, [Images].Dusk, [Images].[Day],
[Images].Foggy, [Images].Cloudy, [Images].Rainy, [Images].BadLighting, [Images].GoodLighting,
[Entity_Statistics].Polygon_ID
from Entity_Statistics, [Statistics], [Images]
where Entity_Statistics.Statistics_ID = [Statistics].ID
and Entity_Statistics.Image_ID = Images.ID
and Entity_Statistics.Polygon_ID = 103
order by [Images].ID