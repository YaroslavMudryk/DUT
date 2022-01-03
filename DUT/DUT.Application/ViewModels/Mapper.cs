﻿using AutoMapper;
using DUT.Application.ViewModels.Group;
using DUT.Application.ViewModels.User;
using System.Text;

namespace DUT.Application.ViewModels
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<Domain.Models.Group, GroupViewModel>()
                .ForMember(x => x.Name, s => s.MapFrom(x => $"{x.Name} ({x.StartStudy.Year})"));


            CreateMap<Domain.Models.User, UserViewModel>()
                .ForMember(x => x.FullName, s => s.MapFrom(s => BuildFullName(s)));
        }

        private string BuildFullName(Domain.Models.User user)
        {
            var sb = new StringBuilder();
            sb.Append(user.LastName);
            sb.Append(" ");
            sb.Append(user.FirstName);
            if (!string.IsNullOrEmpty(user.MiddleName) && !string.IsNullOrWhiteSpace(user.MiddleName))
            {
                sb.Append(" ");
                sb.Append(user.MiddleName);
            }
            return sb.ToString();
        }
    }
}