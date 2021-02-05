using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sabio.Models;
using Sabio.Models.Domain.Provider;
using Sabio.Models.Requests.ProviderDetails;
using Sabio.Models.Requests.ProviderDetails.Details;
using Sabio.Services;
using Sabio.Services.Interfaces;
using Sabio.Web.Controllers;
using Sabio.Web.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sabio.Web.Api.Controllers.Providers
{
    [Route("api/providers/")]
    [ApiController]
    public class ProviderApiController : BaseApiController

    {
        private IProviderService _service = null;
        private IAuthenticationService<int> _authService = null;

        public ProviderApiController(IProviderService service,
            ILogger<ProviderApiController> logger,
            IAuthenticationService<int>AuthService): base(logger)
        {
            _service = service;
            _authService = AuthService;
        }

        [HttpGet("find")]
        public ActionResult<ItemResponse<Paged<ProviderSearchResult>>> SearchByZipcode(int pageIndex,int pageSize, string query)
        {



            int iCode = 200;
            BaseResponse response = null;

            try
            {
                Paged<ProviderSearchResult> provider = _service.SearchByZipcode(pageIndex, pageSize, query);

                if (provider == null)
                {
                    iCode = 400;
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<ProviderSearchResult>> { Item = provider };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(iCode, response);
        }

        [HttpGet()]
        public ActionResult<ItemResponse<Provider>> GetByUserId()
        {
            int userId = 0;
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                userId = _authService.GetCurrentUserId();

                Provider provider = _service.GetByUserId(userId);



                if (provider == null)
                {
                    iCode = 400;
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Provider> { Item = provider };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(iCode, response);
        }

        [HttpGet("{id:int}/details")]
        public ActionResult<ItemResponse<ProviderDetail>> Get(int id)
        {
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                List<ProviderDetail> provDetails = _service.Get(id);

                if (provDetails == null)
                {
                    iCode = 400;
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<List<ProviderDetail>> { Item = provDetails };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(iCode, response);
        }

        [HttpGet("paginate")]
        public ActionResult<ItemResponse<Paged<ProviderDetail>>> GetAll(int pageIndex, int pageSize)
        {
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                Paged<ProviderDetail> provider = _service.GetAll(pageIndex, pageSize);

                if (provider == null)
                {
                    iCode = 400;
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<ProviderDetail>> { Item = provider };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(iCode, response);
        }

        [HttpGet("search")]
        public ActionResult<ItemResponse<Paged<ProviderDetail>>> Search(int pageIndex, int pageSize, string Query)
        {
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                Paged<ProviderDetail> provider = _service.Search(pageIndex, pageSize, Query);

                if (provider == null)
                {
                    iCode = 400;
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<ProviderDetail>> { Item = provider };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(iCode, response);
        }

        [HttpPost]
        public ActionResult<ItemResponse<int>> Create(ProviderAddRequest model)
        {
            int code = 201;
            BaseResponse response = null;

            try
            {
                int userId = _authService.GetCurrentUserId();
                int id = _service.Add(model, userId);
                response = new ItemResponse<int> { Item = id };  // Provider Id
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse($"ArgumentException Error: {ex.ToString() }");
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }


       
    }


}







