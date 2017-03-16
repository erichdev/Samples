[RoutePrefix("api/budget")]
public class BudgetApiController : ApiController
{
    private IUserService _userService;
    private IBudgetService _budgetService;
    public BudgetApiController(IUserService service, IBudgetService service2)
    {
        this._userService = service;
        this._budgetService = service2;
    }
    
    [Route("users"), HttpGet]
    public async Task<HttpResponseMessage> GetOrCreateAtriumUser()
    {
        string userId = _userService.GetCurrentUserId();
        try
        {
            string atriumUserId = _budgetService.GetAtriumUserId(userId);
            if (String.IsNullOrEmpty(atriumUserId) || String.IsNullOrWhiteSpace(atriumUserId))
            {
                atriumUserId = await _budgetService.CreateAtriumUser(userId);
            }
            ItemResponse<string> response = new ItemResponse<string>();
            response.Item = atriumUserId;
            return Request.CreateResponse(response);
        }
        catch (Exception ex)
        {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
        }
    }

    [Route("users/{atriumUserId}/connect_widget_url"), HttpGet]
    public async Task<HttpResponseMessage> GetWidgetUrl(string atriumUserId)
    {
        ItemResponse<string> response = new ItemResponse<string>();
        try
        {
            response.Item = await _budgetService.GetWidgetUrl(atriumUserId);
            return Request.CreateResponse(response);
        }
        catch (Exception ex)
        {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
        }
    }
    
    [Route("users/{atriumUserId}/members"), HttpGet]
    public async Task<HttpResponseMessage> GetMembers(string atriumUserId)
    {
        ItemResponse<AtriumMembersVM> response = new ItemResponse<AtriumMembersVM>();
        try
        {
            response.Item = await _budgetService.GetMembers(atriumUserId);
            return Request.CreateResponse(response);
        }
        catch (Exception ex)
        {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
        }
    }

    [Route("users/{atriumUserId}/accounts"), HttpGet]
    public async Task<HttpResponseMessage> GetAccounts(string atriumUserId)
    {
        ItemResponse<AtriumAccountsVm> response = new ItemResponse<AtriumAccountsVm>();
        try
        {
            response.Item = await _budgetService.GetAccounts(atriumUserId);
            return Request.CreateResponse(response);
        }
        catch (Exception ex)
        {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
        }
    }

    [Route("users/{atriumUserId}/accounts/{accountId}/transactions"), HttpGet]
    public async Task<HttpResponseMessage> GetTransactionsByAccountId(string atriumUserId, string accountId, [FromUri] int page = 1)
    {
        ItemResponse<AtriumTransactionsVM> response = new ItemResponse<AtriumTransactionsVM>();
        string userId = _userService.GetCurrentUserId();
        try
        {
            response.Item = await _budgetService.GetTransactionsByAccountId(userId, atriumUserId, accountId, page);
            return Request.CreateResponse(response);
        }
        catch (Exception ex)
        {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
        }
    }

    [Route("users/{atriumUserId}/transactions"), HttpGet]
    public async Task<HttpResponseMessage> GetTransactionsByUserId(string atriumUserId, [FromUri] int page = 1)
    {
        ItemResponse<AtriumTransactionsVM> response = new ItemResponse<AtriumTransactionsVM>();
        string userId = _userService.GetCurrentUserId();
        try
        {
            response.Item = await _budgetService.GetTransactionsByUserId(userId, atriumUserId, page);
            return Request.CreateResponse(response);
        }
        catch (Exception ex)
        {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
        }
    }

    [Route("transactions/{transactionId}"), HttpPut]
    public HttpResponseMessage UpdateTransactionCategory(string transactionId, [FromUri] int categoryId)
    {
        SuccessResponse response = new SuccessResponse();
        try
        {
            _budgetService.UpdateTransactionCategory(transactionId, categoryId);
            return Request.CreateResponse(response);
        }
        catch (Exception ex)
        {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
        }
    }
}