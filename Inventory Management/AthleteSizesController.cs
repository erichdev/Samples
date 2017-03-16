[RoutePrefix("api/athletesizes")]
public class AthleteSizesController : ApiController
{

    [Route("{athleteId}/sport/{sportId}"), HttpGet]
    [ResponseType(typeof(IQueryable<AthleteSizeVm>))]
    public IHttpActionResult GetAthleteSizeById(string sportId, string athleteId)
    {
        try 
        {
            var sizes = AthleteSizesService.GetAthleteSizeById(sportId, athleteId);

            if (sizes == null)
            {
                return NotFound();
            }

            return Ok(sizes);
        }
        catch (Exception ex) 
        {
            throw return ex;
        }
    }

    [Route("sport/{sportId}/counts"), HttpGet]
    public IQueryable<AthleteSizeCountVm> GetTeamSizeCounts(string sportId)
    {
        try
        {
            var counts = AthleteSizesService.GetTeamSizeCounts(sportId);

            if (counts == null)
            {
                return NotFound();
            }

            return Ok(counts);
        } 
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [Route("sport/{sportId}"), HttpGet]
    public DataTable GetTeamSizes(string sportId)
    {
        try
        {
            var sportItems = SportsItemService.GetSportItemsBySport(sportId);
            var teamSizes = AthleteSizesService.GetTeamSizes(sportId);
            
            DataTable table = AthleteSizesService.MapTeamSizes(sportItems, teamSizes);
            
            return table;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [Route("{athleteId}/sport/{sportId}"), HttpPut]
    [ResponseType(typeof(void))]
    public IHttpActionResult UpsertAthleteSizes(List<AthleteSizeUpsert> athleteSizes, string sportId, string athleteId)
    {
        try
        {
            AthleteSizesService.UpsertAthleteSizes(athleteSizes, sportId, athleteId);

            return StatusCode(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    

    
}
