public static class AthleteSizesService
{
    private InventoryManagementContext db = new InventoryManagementContext();

    public AthleteSizeVm GetAthleteSizeById(string sportId, string athleteId)
    {
        return db.SportsItems
                        .Where(x => x.SportId == sportId)
                        .GroupJoin(db.AthleteSizes
                                    .Where(x => x.AthleteId == athleteId),
                                            sp => sp.Id,
                                            ath => ath.SportsItemId,
                                            (sp, ath) => new { sp, ath })
                        .SelectMany(x => x.ath.DefaultIfEmpty(), 
                                    (x, at) => new AthleteSizeVm {
                                        Id = at.Id,
                                        AthleteId = at.AthleteId,
                                        Size = at.Size,
                                        SportsItemId = x.sp.Id,
                                        SportsItemName = x.sp.Name
                                    });
    }

    public IQueryable<AthleteSizeCountVm> GetTeamSizeCounts(string sportId)
    {
        return db.AthleteSizes
                .GroupBy(x => new { x.SportsItemId, x.Size })
                .Join(db.SportsItems, 
                        a => a.Key.SportsItemId, 
                        si => si.Id, 
                        (a, si) => new { a, si })
                .Where(x => x.si.SportId == sportId)
                .Select(x => new AthleteSizeCountVm() {
                    SportsItemId = x.a.Key.SportsItemId,
                    Size = x.a.Key.Size,
                    Count = x.a.Count(),
                    SportsItemName = x.si.Name
                })
                .OrderBy(x => x.SportsItemName)
                .ThenBy(x => x.Size);
    }

    public List<TeamSizeVm> GetTeamSizesBySport(string sportId)
    {
        return db.AthleteSizes
                    .Where(x => x.SportId == sportId)
                    .Select(x => new TeamSizeVm()
                    {
                        x.Athlete,
                        x.SportsItem,
                        x.Size
                    }).ToList();
    }

    public void UpsertAthleteSizes(List<AthleteSizeUpsert> athleteSizes, string sportId, string athleteId)
    {
        foreach (var item in athleteSizes)
        {
            AthleteSize athleteSize = new AthleteSize()
            {
                Id = item.Id.HasValue ? (int)item.Id : 0, // SQL Server will autogenerate identity if 0
                AthleteId = athleteId,
                Size = item.Size,
                SportsItemId = item.SportsItemId,
                SportId = sportId
            };

            if (item.Id == null)
            {
                db.AthleteSizes.Add(athleteSize);
            }
            else
            {
                db.AthleteSizes.Attach(athleteSize);
                db.Entry(athleteSize).State = EntityState.Modified;
            }
        }
        
        db.SaveChanges();
    }

    // Map details to DataTable since SQL/Entity Framework does not support dynamic pivoting
    public DataTable MapTeamSizes(List<string> sportItems, List<TeamSizeVm> teamSizes){
        DataTable table = new DataTable();
        table.Columns.Add(nameof(Athlete.Id));
        table.Columns.Add(nameof(Athlete.Name));

        foreach (var item in sportItems)
        {
            table.Columns.Add(item);
        }

        foreach (var athlete in teamSizes.Select(x => x.Athlete.Name).Distinct())
        {
            DataRow row = table.NewRow();
            row[nameof(Athlete.Name)] = athlete;
            row[nameof(Athlete.Id)] = teamSizes
                                        .Where(x => x.Athlete.Name == athlete)
                                        .Select(x => x.Athlete.Id)
                                        .FirstOrDefault();

            foreach(var item in sportItems)
            {
                row[item] = teamSizes
                                .Where(x => x.SportsItem.Name == item && x.Athlete.Name == athlete)
                                .Select(x => x.Size)
                                .FirstOrDefault();
            }

            table.Rows.Add(row);
        }


        return table;
    }
}