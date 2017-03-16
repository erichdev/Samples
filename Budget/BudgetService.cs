public class BudgetService : BaseService, IBudgetService
{
    private static string _apiKey = WebConfigurationManager.AppSettings["MXKey"];
    private static string _apiKeyHeader = "MX-API-KEY";
    private static string _clientIdHeader = "MX-CLIENT-ID";
    private static string _clientId = WebConfigurationManager.AppSettings["MXClient"];
    private static string _baseAddress = WebConfigurationManager.AppSettings["MXBaseAddress"];

    public string GetAtriumUserId(string userId)
    {
        string atriumUserId = null;
        DataProvider.ExecuteCmd(GetConnection, "dbo.Atrium_GetAtriumUserByUserId"
            , inputParamMapper: delegate (SqlParameterCollection paramCollection)
            { paramCollection.AddWithValue("@UserId", userId); }
            , map: delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;
                atriumUserId = reader.GetSafeString(startingIndex++);
            }
            );
        return atriumUserId;
    }

    public async Task<string> CreateAtriumUser(string userId)
    {
        try
        {
            using (var atrium = AtriumHttpClient())
            {
                AtriumUserGetRequest user = new AtriumUserGetRequest();
                user.user = new AtriumUserGetRequest.UserObj();
                HttpResponseMessage response = await atrium.PostAsJsonAsync<AtriumUserGetRequest>("/users", user);
                if (response.IsSuccessStatusCode)
                {
                    var deserialized = await Deserialize<AtriumUserVM>(response);
                    SaveAtriumUserId(userId, deserialized.user.guid);
                    return deserialized.user.guid;
                }
                else
                {
                    Exception ex = new Exception(response.ReasonPhrase);
                    throw ex;
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    
    private void SaveAtriumUserId(string userId, string atriumUserId)
    {
        DataProvider.ExecuteNonQuery(GetConnection, "dbo.Atrium_InsertAtriumUserId"
            , inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@UserId", userId);
                paramCollection.AddWithValue("@AtriumUserId", atriumUserId);
            }
            );
    }

    public async Task<string> GetWidgetUrl(string atriumUserId)
    {
        try
        {
            using (var atrium = AtriumHttpClient())
            {
                AtriumUserGetRequest user = new AtriumUserGetRequest();
                user.user = new AtriumUserGetRequest.UserObj();
                HttpResponseMessage response = await atrium.PostAsync("/users/" + atriumUserId + "/connect_widget_url", null);
                if (response.IsSuccessStatusCode)
                {
                    var deserialized = await Deserialize<AtriumUserGetWidgetUrl>(response);
                    return deserialized.user.connect_widget_url;
                }
                else
                {
                    Exception ex = new Exception(response.ReasonPhrase);
                    throw ex;
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<AtriumMembersVM> GetMembers(string atriumUserId)
    {
        try
        {
            using (var atrium = AtriumHttpClient())
            {
                AtriumUserGetRequest user = new AtriumUserGetRequest();
                user.user = new AtriumUserGetRequest.UserObj();
                HttpResponseMessage response = await atrium.GetAsync("/users/" + atriumUserId + "/members");
                if (response.IsSuccessStatusCode)
                {
                    return await Deserialize<AtriumMembersVM>(response);
                }
                else
                {
                    Exception ex = new Exception(response.ReasonPhrase);
                    throw ex;
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<AtriumAccountsVm> GetAccounts(string atriumUserId)
    {
        try
        {
            using (var atrium = AtriumHttpClient())
            {
                AtriumUserGetRequest user = new AtriumUserGetRequest();
                user.user = new AtriumUserGetRequest.UserObj();
                HttpResponseMessage response = await atrium.GetAsync("/users/" + atriumUserId + "/accounts");
                if (response.IsSuccessStatusCode)
                {
                    return await Deserialize<AtriumAccountsVm>(response);
                }
                else
                {
                    Exception ex = new Exception(response.ReasonPhrase);
                    throw ex;
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<AtriumTransactionsVM> GetTransactionsByAccountId(string userId, string atriumUserId, string accountId, int page)
    {
        try
        {
            using (var atrium = AtriumHttpClient())
            {
                AtriumUserGetRequest user = new AtriumUserGetRequest();
                user.user = new AtriumUserGetRequest.UserObj();
                HttpResponseMessage response = await atrium.GetAsync("/users/" + atriumUserId + "/accounts/" + accountId + "/transactions?page=" + page.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var transactions = await Deserialize<AtriumTransactionsVM>(response);
                    if (transactions.Transactions != null)
                    {
                        SyncTransaction(transactions.Transactions, userId);
                    }
                    AtriumTransactionsVM transactionsFromDb = GetTransactionsFromDb(userId, page, accountId);
                    transactionsFromDb.Pagination = new AtriumPagination();
                    transactionsFromDb.Pagination = transactions.Pagination;
                    return transactionsFromDb;
                }
                else
                {
                    Exception ex = new Exception(response.ReasonPhrase);
                    throw ex;
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<AtriumTransactionsVM> GetTransactionsByUserId(string userId, string atriumUserId, int page)
    {
        try
        {
            using (var atrium = AtriumHttpClient())
            {
                AtriumUserGetRequest user = new AtriumUserGetRequest();
                user.user = new AtriumUserGetRequest.UserObj();
                HttpResponseMessage response = await atrium.GetAsync("/users/" + atriumUserId + "/transactions?page=" + page.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var transactions = await Deserialize<AtriumTransactionsVM>(response);
                    if (transactions.Transactions != null)
                    {
                        SyncTransaction(transactions.Transactions, userId);
                    }
                    AtriumTransactionsVM transactionsFromDb = GetTransactionsFromDb(userId, page, null);
                    transactionsFromDb.Pagination = new AtriumPagination();
                    transactionsFromDb.Pagination = transactions.Pagination;
                    return transactionsFromDb;
                }
                else
                {
                    Exception ex = new Exception(response.ReasonPhrase);
                    throw ex;
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public void UpdateTransactionCategory(string transactionId, int categoryId)
    {
        DataProvider.ExecuteNonQuery(GetConnection, "dbo.AtriumTransactions_UpdateMdmfCategory"
            , inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@AtriumTransactionId", transactionId);
                paramCollection.AddWithValue("@CategoryId", categoryId);
            }
            , returnParameters: delegate (SqlParameterCollection param)
            {
            }
            );
    }

    private AtriumTransactionsVM GetTransactionsFromDb(string userId, int page, string bankAccountId = null)
    {
        AtriumTransactionsVM list =  new AtriumTransactionsVM();
        list.Transactions = new List<TransactionObj>();
        DataProvider.ExecuteCmd(GetConnection, "dbo.AtriumTransactions_GetByUserId"
            , inputParamMapper: delegate (SqlParameterCollection paramCollection) {
                paramCollection.AddWithValue("@UserId", userId);
                paramCollection.AddWithValue("@Page", page);
                paramCollection.AddWithValue("@BankAccountId", bankAccountId);
            }
            , map: delegate (IDataReader reader, short set)
            {
                TransactionObj p = new TransactionObj();
                p = MapTransactions(reader);
                if (list.Transactions == null)
                {
                    list.Transactions = new List<TransactionObj>();
                }
                list.Transactions.Add(p);
            }
            );
        return list;
    }

    private TransactionObj MapTransactions(IDataReader reader)
    {
        TransactionObj p = new TransactionObj();
        int startingIndex = 0;
        p.description = reader.GetSafeString(startingIndex++);
        p.CategoryId = reader.GetSafeInt32(startingIndex++);
        p.amount = reader.GetSafeDecimal(startingIndex++);
        p.transacted_at = reader.GetSafeDateTime(startingIndex++);
        p.SyncDate = reader.GetSafeDateTime(startingIndex++);
        p.AtriumTransactionId = reader.GetSafeString(startingIndex++);
        p.AtriumCategory = reader.GetSafeString(startingIndex++);
        return p;
    }

    private void SyncTransaction(List<TransactionObj> transactions, string userId)
    {
        // Go through all transactions and sync to DB
        for (int i = 0; i < transactions.Count; i++)
        {
            var transaction = transactions[i];
            string id = userId;
            string atriumTransactionId = transaction.guid;
            string description = transaction.description;
            decimal? amount = transaction.amount;
            DateTime transactionDate = transaction.transacted_at;
            DateTime syncDate = new DateTime();
            syncDate = DateTime.Now;
            string category = transaction.category;
            string bankAccountId = transaction.account_guid;
            bool updateDatabase = SaveTransactionToDbB(id, atriumTransactionId, description,
                amount, transactionDate, syncDate, category, bankAccountId);
        }
    }

    private bool SaveTransactionToDbB(string userId, string atriumTransactionId,
        string description, decimal? amount, DateTime transactionDate, DateTime syncDate, string category, string bankAccountId)
    {
        int rowsOfDataInserted = DataProvider.ExecuteNonQuery(GetConnection, "dbo.Atrium_SyncTransaction"
            , inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@UserId", userId);
                paramCollection.AddWithValue("@AtriumTransactionId", atriumTransactionId);
                paramCollection.AddWithValue("@Description", description);
                paramCollection.AddWithValue("@Amount", amount);
                paramCollection.AddWithValue("@TransactionDate", transactionDate);
                paramCollection.AddWithValue("@SyncDate", syncDate);
                paramCollection.AddWithValue("@AtriumCategory", category);
                paramCollection.AddWithValue("@BankAccountId", bankAccountId);
            }
        , returnParameters: delegate (SqlParameterCollection param)
        {
        }
        );
        return rowsOfDataInserted > 0;
    }

    private async Task<T> Deserialize<T>(HttpResponseMessage response)
    {
        var obj = await response.Content.ReadAsStringAsync();
        var deserialized = JsonConvert.DeserializeObject<T>(obj);
        return deserialized;
    }

    private HttpClient AtriumHttpClient()
    {
        var atrium = new HttpClient();
        atrium.BaseAddress = new Uri(_baseAddress);
        atrium.DefaultRequestHeaders.Add(_clientIdHeader, _clientId);
        atrium.DefaultRequestHeaders.Add(_apiKeyHeader, _apiKey);
        atrium.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return atrium;
    }
}