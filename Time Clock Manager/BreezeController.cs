public class BruinClockContextProvider : EFContextProvider<BruinClock_16Entities>
{
    public BruinClockContextProvider() : base() { }

    protected override bool BeforeSaveEntity(EntityInfo entityInfo)
    {
        // If times are submitted, intercept to ensure they remain in local time
        if (entityInfo.Entity.GetType() == typeof(Punch) &&
            (entityInfo.EntityState == Breeze.ContextProvider.EntityState.Modified ||
            entityInfo.EntityState == Breeze.ContextProvider.EntityState.Added))
        {
            UpdatePunchObject(entityInfo);
        }
        else if (entityInfo.Entity.GetType() == typeof(Hour))
        {
            ConvertHoursToLocal(entityInfo);
        }
        return true; // Return true to proceed with saving through Breeze
    }

    private UpdatePunchObject(EntityInfo entityInfo)
    {
        var punch = (Punch)entityInfo.Entity;
        var punchTime = (DateTime)punch.datPunch; //Cast as DateTime so that ToLocalTime() method becomes available
        punch.datPunch = punchTime.ToLocalTime();

        punch.ModifiedBy = WindowsIdentity.GetCurrent().Name.ToLower().Replace("athletics\\", "");
        punch.ModifiedDate = DateTime.Now;

        // Since ModifiedBy and ModifiedDate values are generated server-side, Breeze does not recognize these as client-modified values, so we need to explicitly map these values
        entityInfo.OriginalValuesMap.Add("ModifiedBy", punch.ModifiedBy);
        entityInfo.OriginalValuesMap.Add("ModifiedDate", punch.ModifiedDate);
    }

    private ConvertHoursToLocal(EntityInfo entityInfo)
    {
        var hour = (Hour)entityInfo.Entity;

        // Cast as DateTime so that ToLocalTime() method becomes available
        var workDate = (DateTime)hour.WorkDate;
        var entryDate = (DateTime)hour.EntryDate;

        hour.WorkDate = workDate.ToLocalTime();
        hour.EntryDate = entryDate.ToLocalTime();
    }
}

[BreezeController]
public class BreezeController : ApiController
{
    readonly BruinClockContextProvider _contextProvider = new BruinClockContextProvider();

    [HttpGet]
    public List<ADStaff> ADStaff()
    {
        DirectorySearcher searcher = new DirectorySearcher(new DirectoryEntry("**LDAP query**")); // Query Active Directory users

        searcher.SearchScope = SearchScope.OneLevel;
        var results = searcher.FindAll();

        List<ADStaff> list = new List<ADStaff>(); // Note: SupervisorVm model is registered as entity client-side in datacontext.js

        int i = 0;
        foreach (SearchResult result in results)
        {
            DirectoryEntry de = result.GetDirectoryEntry() as DirectoryEntry;

            if (!string.IsNullOrEmpty(de.Properties["cn"].Value as string))
            {
                var employee = new ADStaff();
                employee.Id = i++; // Generate ID as required by Breeze, but ID value is not important. We will not be modifying AD entries through app 
                employee.FirstName = de.Properties["givenName"].Value.ToString();
                employee.LastName = de.Properties["sn"].Value.ToString();
                employee.WindowsIdentity = de.Properties["sAMAccountName"].Value.ToString();

                list.Add(employee);
            }
        }

        return list;
    }

    [HttpGet]
    public IQueryable<Department> Departments()
    {
        var departments = _contextProvider.Context.Departments;
        return departments;
    }

    [HttpGet]
    public IQueryable<Employee> Employees()
    {
        var employees = _contextProvider.Context.Employees;
        return employees;
    }

    [HttpGet]
    public IQueryable<Hour> Hours([FromUri] DateTime start, DateTime end)
    {
        var hours = _contextProvider.Context.Hours
            .Where(a => a.WorkDate >= start)
            .Where(b => b.WorkDate < DbFunctions.AddDays(end, 1));

        foreach (var item in hours)
        {
            item.WorkDate = DateTime.SpecifyKind((DateTime)item.WorkDate, DateTimeKind.Local);
            item.EntryDate = DateTime.SpecifyKind((DateTime)item.EntryDate, DateTimeKind.Local);
        }

        return hours;
    }

    [HttpGet]
    public IQueryable<PayCode> PayCodes()
    {
        var payCodes = _contextProvider.Context.PayCodes;
        return payCodes;
    }

    [HttpGet]
    public IQueryable<PayPeriod> PayPeriods()
    {
        var payPeriods = _contextProvider.Context.PayPeriods
            .OrderByDescending(x => x.StartDate);

        foreach (var item in payPeriods)
        {
            item.StartDate = DateTime.SpecifyKind((DateTime)item.StartDate, DateTimeKind.Local);
            item.EndDate = DateTime.SpecifyKind((DateTime)item.EndDate, DateTimeKind.Local);
        }

        return payPeriods;
    }

    [HttpGet]
    public List<PunchHoursByDay> PunchHoursByDay([FromUri] DateTime start, DateTime end)
    {
        var query = _contextProvider.Context.getPunchHoursByDay(start, end)
            .Select(x => new PunchHoursByDay
            {
                Id = x.Id,
                Hours = x.Hours,
                Date = x.Date,
                fkEmployeeID = x.fkEmployeeID
            }).ToList();

        return query;
    }
 
    [HttpPost]
    public SaveResult SaveChanges(JObject saveBundle)
    {
        return _contextProvider.SaveChanges(saveBundle);
    }
}
