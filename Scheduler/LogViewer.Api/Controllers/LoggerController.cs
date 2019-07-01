using AutoMapper;
using LogViewer.Api.Entities;
using LogViewer.Api.Enums;
using LogViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LogViewer.Api.Controllers
{
    [Route("logs")]
    public class LoggerController : Controller
    {
        private const int MAX_PAGE_SIZE = 10;
        private readonly ElasticClient _client;
        private readonly IUrlHelper _urlHelper;

        public LoggerController(ElasticClient elasticClient, IUrlHelper urlHelper)
        {
            _client = elasticClient;
            _urlHelper = urlHelper;
        }

        [HttpGet("errors/chart-data/")]
        public IActionResult GetErrorDataForChart()
        {
            DateTime dateFrom = new DateTime(2019, 4, 1);


            //var result = await _client.SearchAsync<Logevent>(s => s.AllIndices().Query(q => q.Match(m => m.Field(f => f.Level).Query("Error"))
            //    && +q.DateRange(r => r.Field(f => f.Timestamp).GreaterThanOrEquals(dateFrom))));


            //var result = await _client.SearchAsync<Logevent>(s => s.AllIndices().Aggregations(a =>
            //    a.Terms("group_by_name", ts => ts.Field(o => o.Level))));

            /*.Query(q => q.Match(m => m.Field(f => f.Level).Query("Error"))
                  && +q.DateRange(r => r.Field(f => f.Timestamp).GreaterThanOrEquals(dateFrom))));*/
                  
            //var groupped = result.Documents.GroupBy(d => d.Timestamp.Date).Select(g => new { Date = g.Key.ToShortDateString(), Count = g.Count() });


            return Ok();

            
            
        }

        [HttpGet("info/chart-data")]
        public IActionResult GetInfoDataForChart()
        {
            return Ok();
        }

        [HttpGet("info/{index}", Name = "GetInfo")]
        public async Task<IActionResult> GetInfoLogs([FromRoute] string index, 
            [FromQuery]int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            if (string.IsNullOrEmpty(index))
            {
                return BadRequest();
            }

            if (pageSize > MAX_PAGE_SIZE || pageSize < 1)
            {
                pageSize = 5;
            }

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            var from = (pageNumber - 1) * pageSize;

            ISearchResponse<Logevent> searchResponse = await GetSearchResponse(index, from, pageSize);

            if (searchResponse.Hits.Count == 0)
            {
                return NotFound();
            }

            int totalPages = GetTotalPages(searchResponse, pageSize);

            var previousPageLink = pageNumber > 1 ?
                CreateGetLogsForIndexResourceUri(ResourceUriType.PreviousPage, pageNumber, pageSize, "GetInfo") : null;

            var nextPageLink = pageNumber < totalPages ?
                CreateGetLogsForIndexResourceUri(ResourceUriType.NextPage, pageNumber, pageSize, "GetInfo") : null;

            var paginationMetadata = new
            {
                totalCount = searchResponse.HitsMetadata.Total,
                totalPages,
                pageSize,
                currentPage = pageNumber,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var logeventsDtos = Mapper.Map<IEnumerable<LogeventDto>>(searchResponse.Hits);

            return Ok(logeventsDtos);
        }

        [HttpGet("errors/{index}", Name = "GetErrors")]
        public async Task<IActionResult> GetErrorLogs([FromRoute] string index, 
            [FromQuery]int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            if (string.IsNullOrEmpty(index))
            {
                return BadRequest();   
            }

            if(pageSize > MAX_PAGE_SIZE || pageSize < 1)
            {
                pageSize = 5;
            }

            if(pageNumber < 1)
            {
                pageNumber = 1;
            }

            var from = (pageNumber - 1) * pageSize;

            ISearchResponse<Logevent> searchResponse = await GetSearchResponse(index, from, pageSize);

            if (searchResponse.Hits.Count == 0)
            {
                return NotFound();
            }

            int totalPages = GetTotalPages(searchResponse, pageSize);

            var previousPageLink = pageNumber > 1 ? 
                CreateGetLogsForIndexResourceUri(ResourceUriType.PreviousPage, pageNumber, pageSize, "GetErrors") : null;

            var nextPageLink = pageNumber < totalPages ? 
                CreateGetLogsForIndexResourceUri(ResourceUriType.NextPage, pageNumber, pageSize, "GetErrors") : null;

            var totalCount = searchResponse.HitsMetadata.Total;
            
            var paginationMetadata = new
            {
                totalCount = searchResponse.HitsMetadata.Total,
                totalPages,
                pageSize,
                currentPage = pageNumber,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var logeventsDtos = Mapper.Map<IEnumerable<LogeventDto>>(searchResponse.Hits);

            return Ok(logeventsDtos);
        }

        [HttpDelete("info/{index}/{logeventId}")]
        public async Task<IActionResult> DeleteInfoLog([FromRoute] string index, [FromRoute] string logeventId)
        {
            if (string.IsNullOrEmpty(index))
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(logeventId))
            {
                return BadRequest();
            }

            var getResponse = await _client.GetAsync<Logevent>(new GetRequest(index, typeof(Logevent), logeventId));

            if (!getResponse.Found)
            {
                return NotFound();
            }

            var deleteResponse = await _client.DeleteAsync(new DeleteRequest(index, typeof(Logevent), logeventId));

            var refreshResponse = _client.Refresh(index);

            if (!deleteResponse.IsValid)
            {
                throw new Exception($"Deleting document id {logeventId} failed");
            }

            return NoContent();
        }

        [HttpDelete("errors/{index}/{logeventId}")]
        public async Task<IActionResult> DeleteErrorLog([FromRoute] string index, [FromRoute] string logeventId)
        {
            if (string.IsNullOrEmpty(index))
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(logeventId)) 
            {
                return BadRequest();
            }

            var getResponse = await _client.GetAsync<Logevent>(new GetRequest(index, typeof(Logevent), logeventId));

            if(!getResponse.Found)
            {
                return NotFound();
            }

            var deleteResponse = await _client.DeleteAsync(new DeleteRequest(index, typeof(Logevent), logeventId));

            var refreshResponse = _client.Refresh(index);

            if (!deleteResponse.IsValid)
            {
                throw new Exception($"Deleting document id {logeventId} failed");
            }

            return NoContent();

        }

        
        private int GetTotalPages(ISearchResponse<Logevent> searchResponse, int pageSize)
        {
            return (int)Math.Ceiling(searchResponse.HitsMetadata.Total / (double)pageSize);
        }

        private string CreateGetLogsForIndexResourceUri(ResourceUriType type, int pageNumber, int pageSize, string routeName)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link(routeName, new
                    {
                        pageNumber = pageNumber - 1,
                        pageSize
                    });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link(routeName, new
                    {
                        pageNumber = pageNumber + 1,
                        pageSize
                    });
                default:
                    return _urlHelper.Link(routeName, new
                    {
                        pageNumber,
                        pageSize
                    });
            }
        }

        private async Task<ISearchResponse<Logevent>> GetSearchResponse(string index, int from, int pageSize)
        {
            return await _client.SearchAsync<Logevent>(s =>
                     s.Index(index).From(from).Size(pageSize).Query(q => q.MatchAll()));
                   
        } 

        

    }
}
