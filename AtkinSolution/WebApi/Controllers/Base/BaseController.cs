using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers.Base
{
    public abstract class BaseController : ControllerBase
    {
        protected int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) 
                ?? throw new UnauthorizedAccessException("User ID not found in token");
            
            return int.Parse(claim.Value);
        }
    }
} 