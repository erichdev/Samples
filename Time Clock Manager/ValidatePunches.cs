private void ValidatePunch(string start, string end)
{
    var transaction = _contextProvider.Context.Database.BeginTransaction();

    try
    {
        _contextProvider.Context.Database.ExecuteSqlCommand("DELETE FROM PunchError");

        DateTime startDate = Convert.ToDateTime(start);
        DateTime endDate = Convert.ToDateTime(end);

        var punches = _contextProvider.Context.Punches
            .Where(a => a.datPunch > startDate && a.datPunch < DbFunctions.AddDays(endDate, 1))
            .OrderBy(c => c.fkEmployeeID)
            .ThenBy(d => d.datPunch)
            .ToList();

        int? currentEmployeeId;
        DateTime? currentdatPunch;
        bool? punchDirection;

        if (punches.Count > 0)
        {
            int n = 0;

            while (n < punches.Count)
            {
                // Grab employee ID and start loop for this employee
                currentEmployeeId = punches[n].fkEmployeeID;

                while (punches[n].fkEmployeeID == currentEmployeeId)
                {

                    // If first punch is not IN, log error
                    if (punches[n].bIn != true)
                    {
                        LogPunchError(currentEmployeeId, punches[n].datPunch, "First punch is OUT");

                        MoveToNextDay(punches, punches[n].datPunch, ref n);
                    }
                    else
                    {
                        // Grab date and direction of punch
                        currentdatPunch = punches[n].datPunch;
                        punchDirection = punches[n].bIn;

                        // Move to next punch
                        n++;

                        // Check if reached end of punches
                        if (n == punches.Count)
                        {
                            LogPunchError(currentEmployeeId, currentdatPunch, "Missing punch at End of File");
                            break;
                        }

                        // Check if next employee is same employee and date
                        while (currentEmployeeId == punches[n].fkEmployeeID && currentdatPunch.Value.Date == punches[n].datPunch.Value.Date)
                        {
                            // If two consecutive IN or OUT punches, log error
                            if (punches[n].bIn == punchDirection)
                            {
                                LogPunchError(currentEmployeeId, punches[n].datPunch, "Punch missing or out of sequence");

                                MoveToNextDay(punches, punches[n].datPunch, ref n);
                            }
                            else
                            {
                                // Otherwise, save this punch direction and move to next record
                                punchDirection = punches[n].bIn;
                                n++;
                            }

                            // Check if end of punches or if new employee
                            if (n == punches.Count)
                            {
                                if (punchDirection == true)
                                {
                                    LogPunchError(currentEmployeeId, currentdatPunch, "Last punch is IN");
                                }
                                break;
                            }
                            else if (currentEmployeeId != punches[n].fkEmployeeID && punchDirection == true)
                            {
                                LogPunchError(currentEmployeeId, currentdatPunch, "Last punch is IN");
                                break;
                            }
                        }

                        // Check if end of punches reached and last punch is IN
                        if (n == punches.Count)
                        {
                            if (punchDirection == true)
                            {
                                LogPunchError(currentEmployeeId, currentdatPunch, "Last punch is IN");
                            }
                            break;
                        }
                        // Check if reached new employee without finding matching OUT record
                        else if (currentEmployeeId != punches[n].fkEmployeeID && punchDirection == true)
                        {
                            LogPunchError(currentEmployeeId, currentdatPunch, "Last punch is IN");
                        }
                        else if (currentEmployeeId == punches[n].fkEmployeeID &&
                            punchDirection == punches[n].bIn &&
                            punches[n].bIn == true)
                        {
                            LogPunchError(currentEmployeeId, currentdatPunch, "Last punch is IN");
                        }
                    }


                    // Final check to see if end of punches to avoid errors
                    if (n == punches.Count)
                    {
                        // No need to check for errors, just prevent continuing if end of punches
                        break;
                    }
                }
            }

        }

        transaction.Commit();

        return;
    }
    catch (Exception ex)
    {
        transaction.Rollback();

        throw ex;
    }


}

private void LogPunchError(int? empId, DateTime? punchDate, string errMsg)
{
    if (empId == 0 || empId == null)
    {
        return;
    }
    var emp = _contextProvider.Context.tblEmployees
        .Where(x => x.pkID == empId)
        .FirstOrDefault();

    PunchError err = new PunchError()
    {
        EmpApptDistId = empId,
        datWorkDate = punchDate,
        Message = errMsg,
        datErrorDate = DateTime.Now.ToLocalTime(),
        tblEmployee = emp

    };

    _contextProvider.Context.PunchErrors.Add(err);
    _contextProvider.Context.SaveChanges();
}

private void MoveToNextDay(List<Punch> punches, DateTime? workDate, ref int n)
{
    while (punches[n].datPunch == workDate)
    {
        n++;
        if (n == punches.Count)
        {
            return;
        }
    }
}