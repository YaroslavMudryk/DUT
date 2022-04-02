﻿using DUT.Application.Options;
using DUT.Application.ViewModels;
using DUT.Application.ViewModels.Group;
using DUT.Application.ViewModels.Group.GroupMember;
using DUT.Domain.Models;

namespace DUT.Application.Services.Interfaces
{
    public interface IGroupService : IBaseService<Group>
    {
        Task<Result<List<GroupViewModel>>> GetGroupsBySpecialtyIdAsync(int specialtyId);
        Task<Result<List<GroupViewModel>>> GetAllGroupsAsync(int count, int afterId);
        Task<Result<GroupViewModel>> GetGroupByIdAsync(int id);
        Task<Result<List<GroupViewModel>>> SearchGroupsAsync(SearchGroupOptions options);
        Task<Result<GroupViewModel>> CreateGroupAsync(GroupCreateModel model);
        Task<Result<GroupViewModel>> IncreaseCourseOfGroupAsync(int groupId);
        Task<Result<GroupMemberViewModel>> UpdateClassTeacherGroupAsync(GroupClassTeacherEditModel model);
        Task<Result<List<GroupShortViewModel>>> GetUserGroupsAsync(int userId);
    }
}