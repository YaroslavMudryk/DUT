﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using URLS.Application.Extensions;
using URLS.Application.Services.Interfaces;
using URLS.Application.ViewModels;
using URLS.Application.ViewModels.Specialty;
using URLS.Domain.Models;
using URLS.Infrastructure.Data.Context;

namespace URLS.Application.Services.Implementations
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly URLSDbContext _db;
        private readonly IMapper _mapper;
        private readonly IIdentityService _identityService;
        private readonly IFacultyService _faultyService;
        private readonly ICommonService _commonService;
        public SpecialtyService(URLSDbContext db, IMapper mapper, IIdentityService identityService, IFacultyService faultyService, ICommonService commonService)
        {
            _db = db;
            _mapper = mapper;
            _identityService = identityService;
            _faultyService = faultyService;
            _commonService = commonService;
        }

        public async Task<Result<SpecialtyViewModel>> CreateSpecialtyAsync(SpecialtyCreateModel model)
        {
            if (await _commonService.IsExistAsync<Specialty>(x => x.Name == model.Name && x.Code == model.Code))
                return Result<SpecialtyViewModel>.Error("Specialty already exist");
            var currentFaculty = await _faultyService.GetFacultyByIdAsync(model.FacultyId);
            if (currentFaculty.IsNotFound)
                return Result<SpecialtyViewModel>.NotFound("Faculty not found");
            var newSpecialty = new Specialty
            {
                Code = model.Code,
                Name = model.Name,
                FacultyId = model.FacultyId
            };
            newSpecialty.PrepareToCreate(_identityService);
            await _db.Specialties.AddAsync(newSpecialty);
            await _db.SaveChangesAsync();

            return Result<SpecialtyViewModel>.Created(_mapper.Map<SpecialtyViewModel>(newSpecialty));
        }

        public async Task<Result<List<SpecialtyViewModel>>> GetAllSpecialtiesAsync()
        {
            return Result<List<SpecialtyViewModel>>.SuccessList(await _db.Specialties.AsNoTracking().Select(x => new SpecialtyViewModel
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                Name = x.Name,
                Code = x.Code
            }).ToListAsync());
        }

        public async Task<Result<List<SpecialtyViewModel>>> GetSpecialtiesByFacultyIdAsync(int facultyId)
        {
            return Result<List<SpecialtyViewModel>>.SuccessList(await _db.Specialties.AsNoTracking().Where(x => x.FacultyId == facultyId).Select(x => new SpecialtyViewModel
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                Name = x.Name,
                Code = x.Code
            }).ToListAsync());
        }

        public async Task<Result<SpecialtyViewModel>> GetSpecialtyByIdAsync(int id)
        {
            var specialty = await _db.Specialties.AsNoTracking().Select(x => new SpecialtyViewModel
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                Name = x.Name,
                Code = x.Code
            }).FirstOrDefaultAsync(x => x.Id == id);
            if (specialty == null)
                return Result<SpecialtyViewModel>.NotFound();
            return Result<SpecialtyViewModel>.SuccessWithData(specialty);
        }

        public async Task<Result<SpecialtyViewModel>> UpdateSpecialtyAsync(SpecialtyEditModel model)
        {
            var currentSpecialty = await _db.Specialties.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.Id);
            if (currentSpecialty == null)
                return Result<SpecialtyViewModel>.NotFound();
            var faculty = await _db.Faculties.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.FacultyId);
            if (faculty == null)
                return Result<SpecialtyViewModel>.NotFound("Facuty not found");
            currentSpecialty.Name = model.Name;
            currentSpecialty.Code = model.Code;
            currentSpecialty.FacultyId = model.FacultyId;
            currentSpecialty.PrepareToUpdate(_identityService);
            _db.Specialties.Update(currentSpecialty);
            await _db.SaveChangesAsync();
            return Result<SpecialtyViewModel>.SuccessWithData(_mapper.Map<SpecialtyViewModel>(currentSpecialty));
        }
    }
}