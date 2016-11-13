select Image_ID, COUNT(*) as cou from Entity_Statistics
group by Image_ID
having COUNT(*) > 1
order by Image_ID