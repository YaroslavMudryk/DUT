﻿using Microsoft.AspNetCore.Mvc;
using URLS.Application.Services.Interfaces;
using URLS.Application.ViewModels.Setting;

namespace URLS.Web.Controllers.V1
{
    [ApiVersion("1.0")]
    public class SettingsController : ApiBaseController
    {
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        public SettingsController(IPermissionService permissionService, ISettingService settingService)
        {
            _permissionService = permissionService;
            _settingService = settingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSetting()
        {
            return JsonResult(await _settingService.GetRootSettingAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateSetting([FromBody] SettingCreateModel model)
        {
            return JsonResult(await _settingService.CreateSettingAsync(model));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSetting([FromBody] SettingEditModel model)
        {
            return JsonResult(await _settingService.UpdateSettingAsync(model));
        }
    }
}