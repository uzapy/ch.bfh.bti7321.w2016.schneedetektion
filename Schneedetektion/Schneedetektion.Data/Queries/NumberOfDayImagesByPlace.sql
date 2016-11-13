select Place, COUNT(*) as cou from Images
where [Day] = 1
group by Place
order by Place