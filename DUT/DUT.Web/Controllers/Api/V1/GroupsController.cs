﻿using DUT.Application.Services.Interfaces;
using DUT.Application.ViewModels.Group;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DUT.Web.Controllers.Api.V1
{
    [ApiVersion("1.0")]
    public class GroupsController : ApiBaseController
    {
        private readonly IGroupService _groupService;
        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] GroupCreateModel model)
        {
            return JsonResult(await _groupService.CreateGroupAsync(model));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGroups(int afterId = int.MaxValue, int count = 20)
        {
            return JsonResult(await _groupService.GetAllGroupsAsync(afterId, count));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupById(int id)
        {
            return JsonResult(await _groupService.GetGroupByIdAsync(id));
        }

        [HttpGet("{groupId}/members")]
        public async Task<IActionResult> GetGroupMembers(int groupId, int afterId = int.MaxValue, int count = 20, int status = 0)
        {
            return JsonResult(await _groupService.GetGroupMembersAsync(groupId, afterId, count, status));
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchGroups(string name)
        {
            return JsonResult(await _groupService.SearchGroupsAsync(name));
        }
    }
}
