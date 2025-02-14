﻿using AutoMapper;
using Client.Torun.RavenDataService.DataStore;
using Client.Torun.RavenDataService.Entities;
using Client.Torun.RavenDataService.Enums;
using Client.Torun.RavenDataService.Models;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Scheduler.Mailer.Interfaces;
using Client.Torun.RavenDataService.Config;
using Client.Torun.RavenDataService.Helpers;
using Common;

namespace Client.Torun.RavenDataService.Controllers
{
    [Authorize]
    [Route("admin")]
    public class AdminController : Controller
    {
        private const int MAX_PAGE_SIZE = 10;
        private readonly IDocumentStore _store;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;
        private readonly ISchedulerMailer _schedulerMailer;
        private readonly DataServiceConfiguration _dataServiceConfiguration;

        public AdminController(IDocumentStoreHolder documentStoreHolder, IMapper mapper, IUrlHelper urlHelper, 
            ISchedulerMailer schedulerMailer, DataServiceConfiguration dataServiceConfiguration)
        {
            _store = documentStoreHolder.Store;
            _mapper = mapper;
            _urlHelper = urlHelper;
            _schedulerMailer = schedulerMailer;
            _dataServiceConfiguration = dataServiceConfiguration;

        }

        [HttpPost("users")]
        public async Task<IActionResult> AddUser([FromBody]UserToCreateDto userToCreteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            using (var session = _store.OpenAsyncSession())
            {
                var user = await session.Query<User>()
                    .Where(u => u.Email.Equals(userToCreteDto.Email, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    return BadRequest(new { error = "The email is already in use." });
                }
                
                var dbUser = _mapper.Map<User>(userToCreteDto);
                dbUser.Color = await GetColor();
                var temporaryPassword =  RandomPasswordGenerator.GeneratePassword(15);
                var salt = Convert.ToBase64String(PasswordHasher.GenerateSalt());
                var hashedPassword = PasswordHasher.HashPassword(temporaryPassword, Convert.FromBase64String(salt));
                dbUser.Salt = salt;
                dbUser.TemporaryPassword = hashedPassword;
                
                await session.StoreAsync(dbUser);

                await session.SaveChangesAsync();

                string username = FormatUsername(dbUser.Email);

                string message = $"<b>Dear {dbUser.FirstName}</b></br><p>You're receiving this message because your Scheduler Account has been created.</p><p>Your <b>username</b>: {username}</p><p>Your <b>first-time login password</b>: {temporaryPassword}</p><p>Follow the link below to change your password and log in to Scheduler application:</p><p><a href={_dataServiceConfiguration.ClientUrl}>Login</a></p><p>Best</p><p>Scheduler Team</p>";

                _schedulerMailer.SendMail("Scheduler-Notifications", dbUser.Email, "Scheduler Account", message, _dataServiceConfiguration.MailBoxPassword); 

                var userToReturn = _mapper.Map<PostCreationUserToReturnDto>(dbUser);

                return CreatedAtRoute("GetUser", new { userId = dbUser.Id }, userToReturn);
                    
            }

        }

        [HttpGet("users/{userId}", Name = "GetUser")]
        public async Task<IActionResult> GetUser([FromRoute] string userId)
        {
            if (userId == null)
            {
                return BadRequest("The action requires a route parameter");
            }

            using (var session = _store.OpenAsyncSession())
            {
                var user = await session.LoadAsync<User>(userId);

                if (user == null)
                {
                    return NotFound();
                }

                var userToReturnDto = _mapper.Map<PostCreationUserToReturnDto>(user);

                return Ok(userToReturnDto);

            }
        }

        [HttpGet("users", Name = "GetUsers")]
        public async Task<IActionResult> GetUsers([FromQuery]int pageNumber = 1, [FromQuery]int pageSize = 5 )
        {
            if (pageSize > MAX_PAGE_SIZE || pageSize < 1)
            {
                pageSize = 5;
            }

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            var numberToSkip = (pageNumber - 1) * pageSize;

            using(var session = _store.OpenAsyncSession())
            {

                var users = await session.Query<User>().Statistics(out QueryStatistics stats)
                    .Skip(numberToSkip).Take(pageSize).ToListAsync();

                int collectionSize = stats.TotalResults;

                int totalPages = (int)Math.Ceiling(collectionSize / (double)pageSize);

                var previousPageLink = pageNumber > 1 ?
                    CreateGetUsersResourceUri(ResourceUriType.PreviousPage, pageNumber, pageSize, "GetUsers") : null;

                var nextPageLink = pageNumber < totalPages ?
                    CreateGetUsersResourceUri(ResourceUriType.NextPage, pageNumber, pageSize, "GetUsers") : null;

                var paginationMetadata = new
                {
                    totalCount = collectionSize,
                    totalPages,
                    pageSize,
                    currentPage = pageNumber,
                    previousPageLink,
                    nextPageLink
                };

                Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

                var usersToReturnDto = _mapper.Map<IEnumerable<UserToReturnDto>>(users);

                return Ok(usersToReturnDto);
            }

        }

        [HttpGet("allusers")]
        public async Task<IActionResult> GetAllUsersWithoutPaging()
        {
            using (var session = _store.OpenAsyncSession())
            {
                var allUsers = await session.Query<User>().ToListAsync();
                var allUsersToReturn = _mapper.Map<IEnumerable<UserToReturnDto>>(allUsers);

                return Ok(allUsersToReturn);
            }
        }

        private string CreateGetUsersResourceUri(ResourceUriType type, int pageNumber, int pageSize, string routeName)
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

        private string FormatUsername(string username)
        {
            username = username.Replace("@", "<span>@</span>");
            var index = username.LastIndexOf('.');
            username = username.Insert(index, "<span>.</span>");
            index = username.LastIndexOf('.');
            username = username.Remove(index, 1);

            return username;
        }

        private async Task<string> GetColor()
        {
            using (var session = _store.OpenAsyncSession())
            {
                var color = await session.Query<Color>().FirstAsync(c => c.IsInUse == false);
                color.IsInUse = true;
                await session.SaveChangesAsync();

                return color.Value;
            }
        }
    }
}
