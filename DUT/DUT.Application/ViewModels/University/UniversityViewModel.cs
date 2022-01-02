﻿using DUT.Application.ViewModels.Faculty;
namespace DUT.Application.ViewModels.University
{
    public class UniversityViewModel
    {
        public string Name { get; set; }
        public string NameEng { get; set; }
        public string ShortName { get; set; }
        public string ShortNameEng { get; set; }
        public List<FacultyViewModel> Faculties { get; set; }
    }
}