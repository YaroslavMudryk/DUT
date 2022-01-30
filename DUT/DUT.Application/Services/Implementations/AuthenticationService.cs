﻿using DUT.Application.Extensions;
using DUT.Application.Helpers;
using DUT.Application.Services.Interfaces;
using DUT.Application.ViewModels;
using DUT.Application.ViewModels.Identity;
using DUT.Application.ViewModels.User;
using DUT.Constants;
using DUT.Domain.Models;
using DUT.Infrastructure.Data.Context;
using Extensions.DeviceDetector;
using Extensions.Password;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DUT.Application.Services.Implementations
{
    public class AuthenticationService : BaseService<User>, IAuthenticationService
    {
        private readonly DUTDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identityService;
        private readonly ISessionManager _sessionManager;
        private readonly ILocationService _locationService;
        private readonly ITokenService _tokenService;
        private readonly IRoleService _roleService;
        private readonly IDetector _detector;

        public AuthenticationService(DUTDbContext db, IHttpContextAccessor httpContextAccessor, IIdentityService identityService, ISessionManager sessionManager, ILocationService locationService, ITokenService tokenService, IRoleService roleService, IDetector detector) : base(db)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _identityService = identityService;
            _sessionManager = sessionManager;
            _locationService = locationService;
            _tokenService = tokenService;
            _roleService = roleService;
            _detector = detector;
        }

        public async Task<Result<AuthenticationInfo>> ChangePasswordAsync(PasswordCreateModel model)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.UserId);
            if (user == null)
                return Result<AuthenticationInfo>.NotFound("User not found");

            if (model.OldPassword.VerifyPasswordHash(user.PasswordHash))
                return Result<AuthenticationInfo>.Error("Password not comparer");

            if (model.OldPassword == model.NewPassword)
                return Result<AuthenticationInfo>.Error("This passwords are match");

            user.PasswordHash = model.NewPassword.GeneratePasswordHash();

            user.PrepareToUpdate(_identityService);

            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            var notification = NotificationsHelper.GetChangePasswordNotification();
            notification.UserId = user.Id;
            await _db.Notifications.AddAsync(notification);
            await _db.SaveChangesAsync();
            return Result<AuthenticationInfo>.SuccessWithData(new AuthenticationInfo
            {
                User = user,
            });
        }

        public async Task<Result<JwtToken>> LoginByPasswordAsync(LoginCreateModel model)
        {
            var app = await _db.Apps
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AppId == model.AppId && x.AppSecret == model.AppSecret);

            if (app == null)
                return Result<JwtToken>.Error("App not found");

            if (!app.IsActive)
                return Result<JwtToken>.Error("App already unactive");

            if (!app.IsActiveByTime())
                return Result<JwtToken>.Error("App is expired");

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Login == model.Login);
            if (user == null)
                return Result<JwtToken>.Error("User by login not found");

            if (user.LockoutEnabled)
            {
                if (user.IsLocked())
                {
                    return Result<JwtToken>.Error($"Your account has been locked up to {user.LockoutEnd.Value.ToString("HH:mm (dd.MM.yyyy)")}");
                }

                if (user.AccessFailedCount == 5)
                {
                    user.AccessFailedCount = 0;
                    user.LockoutEnd = DateTime.Now.AddHours(1);
                    var notifyLogin = NotificationsHelper.GetLockedNotification();
                    notifyLogin.UserId = user.Id;
                    _db.Users.Update(user);
                    await _db.Notifications.AddAsync(notifyLogin);
                    await _db.SaveChangesAsync();

                    return Result<JwtToken>.Error($"Account locked up to {user.LockoutEnd.Value.ToString("HH:mm (dd.MM.yyyy)")}");
                }
            }

            if (!model.Password.VerifyPasswordHash(user.PasswordHash))
            {
                user.AccessFailedCount++;
                _db.Users.Update(user);
                model.IP = _identityService.GetIP();
                var loginAttemptNotify = NotificationsHelper.GetLoginAttemptNotification(model);
                loginAttemptNotify.UserId = user.Id;
                await _db.Notifications.AddAsync(loginAttemptNotify);
                await _db.SaveChangesAsync();
                return Result<JwtToken>.Error("Password is incorrect");
            }

            if (model.Client == null)
                model.Client = _detector.GetClientInfo();

            var location = await _locationService.GetIpInfoAsync(model.IP);

            var appDb = new AppModel
            {
                Id = app.Id,
                Name = app.Name,
                ShortName = app.ShortName,
                Image = app.Image,
                Description = app.Description
            };

            var sessionId = Guid.NewGuid();

            var session = new Session
            {
                Id = sessionId,
                IsActive = true,
                App = appDb,
                Client = model.Client,
                Location = location,
                UserId = user.Id,
                Token = "tempToken"
            };

            session.PrepareToCreate();

            var jwtToken = await _tokenService.GetUserTokenAsync(user.Id, sessionId, "pwd");

            session.Token = jwtToken.Token;

            var loginNotify = NotificationsHelper.GetLoginNotification(session);
            loginNotify.UserId = user.Id;

            await _db.Notifications.AddAsync(loginNotify);

            await _db.Sessions.AddAsync(session);

            await _db.SaveChangesAsync();

            _sessionManager.AddSession(jwtToken.Token);

            return Result<JwtToken>.SuccessWithData(jwtToken);
        }

        public async Task<Result<bool>> LogoutAllAsync(int userId)
        {
            var sessions = await _db.Sessions.Where(x => x.IsActive && x.UserId == userId).ToListAsync();

            var tokens = sessions.Select(x => x.Token);

            var currentSessionId = _identityService.GetCurrentSessionId();

            sessions.ForEach(session =>
            {
                session.IsActive = false;
                session.DeactivatedBySessionId = currentSessionId;
                session.DeactivatedAt = DateTime.Now;
                session.PrepareToUpdate(_identityService);
            });

            _db.Sessions.UpdateRange(sessions);
            await _db.Notifications.AddAsync(NotificationsHelper.GetAllLogoutNotification());
            await _db.SaveChangesAsync();

            _sessionManager.RemoveRangeSession(tokens);

            return Result<bool>.Success();
        }

        public async Task<Result<bool>> LogoutAsync()
        {
            var token = _identityService.GetBearerToken();
            if (!_sessionManager.IsActiveSession(token))
                return Result<bool>.Error("Session is already expired");

            var session = await _db.Sessions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == _identityService.GetCurrentSessionId());
            if (session == null)
                return Result<bool>.Error("Session not found");

            var now = DateTime.Now;
            session.IsActive = false;
            session.DeactivatedAt = now;
            session.DeactivatedBySessionId = _identityService.GetCurrentSessionId();
            session.PrepareToUpdate(_identityService);
            _db.Sessions.Update(session);
            await _db.SaveChangesAsync();

            var notify = NotificationsHelper.GetLogoutNotification(session);

            await _db.Notifications.AddAsync(notify);
            await _db.SaveChangesAsync();

            _sessionManager.RemoveSession(token);
            return Result<bool>.SuccessWithData(true);
        }

        public async Task<Result<bool>> LogoutBySessionIdAsync(Guid id)
        {
            var sessionToClose = await _db.Sessions.FirstOrDefaultAsync(x => x.Id == id);
            if (sessionToClose == null)
                return Result<bool>.Error("Session not found");
            var now = DateTime.Now;
            sessionToClose.IsActive = false;
            sessionToClose.DeactivatedAt = now;
            sessionToClose.DeactivatedBySessionId = _identityService.GetCurrentSessionId();
            sessionToClose.PrepareToUpdate(_identityService);
            _db.Sessions.Update(sessionToClose);
            await _db.Notifications.AddAsync(NotificationsHelper.GetLogoutNotification(sessionToClose));
            await _db.SaveChangesAsync();
            _sessionManager.RemoveSession(sessionToClose.Token);
            return Result<bool>.Success();
        }

        public async Task<Result<AuthenticationInfo>> RegisterAsync(RegisterViewModel model)
        {
            if (await IsExistAsync(s => s.Login == model.Login))
                return Result<AuthenticationInfo>.Error("Login is busy");

            var groupInvite = await _db.GroupInvites.AsNoTracking().FirstOrDefaultAsync(s => s.CodeJoin == model.Code);
            if (groupInvite == null)
                return Result<AuthenticationInfo>.Error("Code isn't exist");

            if (!groupInvite.IsActive)
                return Result<AuthenticationInfo>.Error("Code already unactive");

            if (!groupInvite.IsActiveByTime())
                return Result<AuthenticationInfo>.Error("Code is expired");

            var newUser = new User(model.FirstName, null, model.LastName, model.Login, Generator.GetUsername());

            newUser.PrepareToCreate(_identityService);

            await _db.Users.AddAsync(newUser);
            await _db.SaveChangesAsync();

            var role = await _roleService.GetRoleByNameAsync(Roles.Student);

            var userRole = new UserRole
            {
                UserId = newUser.Id,
                RoleId = role.Id
            };
            userRole.PrepareToCreate(_identityService);

            await _db.UserRoles.AddAsync(userRole);
            await _db.SaveChangesAsync();

            var groupStudent = new UserGroup
            {
                UserId = newUser.Id,
                GroupId = groupInvite.GroupId,
                IsAdmin = false,
                Status = UserGroupStatus.New,
                Title = "Студент"
            };

            groupStudent.PrepareToCreate(_identityService);

            await _db.UserGroups.AddAsync(groupStudent);
            await _db.SaveChangesAsync();

            return Result<AuthenticationInfo>.Success();
        }
    }
}
