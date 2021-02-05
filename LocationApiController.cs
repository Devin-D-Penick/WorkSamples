using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sabio.Models;
using Sabio.Models.Domain;
using Sabio.Models.Requests.Locations;
using Sabio.Services;
using Sabio.Web.Controllers;
using Sabio.Web.Models.Responses;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Sabio.Web.Api.Controllers
{
    [Route("api/locations")]
    [ApiController]
    public class LocationApiController : BaseApiController
    {
        private ILocationService _service = null;
        private IAuthenticationService<int> _authService = null;

        public LocationApiController(ILocationService service,
            ILogger<LocationApiController> logger,
            IAuthenticationService<int> AuthService) : base(logger)
        {
            _service = service;
            _authService = AuthService;
        }
        [HttpPost]
        public ActionResult<ItemResponse<int>> Create(LocationAddRequest request)
        {

            ObjectResult result = null;
            try
            {
                int currentUserId = _authService.GetCurrentUserId();
                int id = _service.Add(request, currentUserId);

                ItemResponse<int> response = new ItemResponse<int>() { Item = id };

                result = Created201(response);
            }
            catch (Exception exResponse)
            {
                Logger.LogError(exResponse.ToString());
                ErrorResponse response = new ErrorResponse(exResponse.Message);
                result = StatusCode(500, response);

            }


            return result;

        }
        [HttpGet("{id:int}")]
        public ActionResult<ItemResponse<Location>> Get(int id)
        {
            BaseResponse response = null;
            int rCode = 200;

            try
            {
                Location locations = _service.Get(id);

                if (locations == null)
                {
                    rCode = 404;
                    response = new ErrorResponse("Application Resource not found");
                }
                else
                {
                    response = new ItemResponse<Location> { Item = locations };
                }
            }
          
            catch (Exception ex)
            {
                rCode = 500;
                response = new ErrorResponse($"Exception Error:{ex.Message}");

            }


            return StatusCode(rCode, response);
        }
        [HttpPut("{id:int}")]
        public ActionResult<SuccessResponse> Update(LocationUpdateRequest request)
        {
            BaseResponse response = null;
            int rCode = 200;

            try
            {
                int currentUserId = _authService.GetCurrentUserId();
                int id = _service.Add(request, currentUserId);
                rCode = 200;
                response = new SuccessResponse();
            }
            catch (Exception exResult)
            {
                rCode = 500;
                response = new ErrorResponse(exResult.Message);
                base.Logger.LogError(exResult.ToString());
            }

            return StatusCode(rCode, response);
        }
        [HttpDelete("{id:int}")]
        public ActionResult<SuccessResponse> Delete(int id)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                _service.Delete(id);

                response = new SuccessResponse();
            }
            catch (Exception exResult)
            {
                code = 500;
                response = new ErrorResponse(exResult.Message);
                base.Logger.LogError(exResult.ToString());
            }
            return StatusCode(code, response);
        }
        [HttpGet("paginate")]
        public ActionResult<ItemResponse<Paged<Location>>> Paged(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {

                Paged<Location> paged = _service.GetAllPaged(pageIndex, pageSize);

                if (paged == null)
                {
                    code = 404;
                    response = new ErrorResponse("App not Found");


                }
                else
                {
                    response = new ItemResponse<Paged<Location>> { Item = paged};
                }
            }
            catch (Exception exceptionResponse)
            {
                code = 500;
                response = new ErrorResponse(exceptionResponse.Message);
                base.Logger.LogError(exceptionResponse.ToString());
            }

            return StatusCode(code, response);


        }
        [HttpGet("current")]
        public ActionResult<ItemResponse<Paged<Location>>> GetCurrent(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                int currentUserId = _authService.GetCurrentUserId();
              


                Paged<Location> paged = _service.GetCurrent(pageIndex, pageSize,currentUserId);

                if (paged == null)
                {
                    code = 404;
                    response = new ErrorResponse("App not Found");


                }
                else
                {
                    response = new ItemResponse<Paged<Location>> { Item = paged };
                }
            }
            catch (Exception exceptionResponse)
            {
                code = 500;
                response = new ErrorResponse(exceptionResponse.Message);
                base.Logger.LogError(exceptionResponse.ToString());
            }

            return StatusCode(code, response);


        }
        [HttpGet("radius")]
        public ActionResult<ItemResponse<Paged<Location>>>GetLocation(int pageIndex, int pageSize, int radius)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {

                Paged<Location> geolocation = _service.GetLocation(pageIndex,pageSize,radius);

                if (geolocation == null)
                {
                    code = 404;
                    response = new ErrorResponse("App not Found");


                }
                else
                {
                    response = new ItemResponse<Paged<Location>> { Item = geolocation };
                }
            }
            catch (Exception exceptionResponse)
            {
                code = 500;
                response = new ErrorResponse(exceptionResponse.Message);
                base.Logger.LogError(exceptionResponse.ToString());
            }

            return StatusCode(code, response);


        }

    }
}

