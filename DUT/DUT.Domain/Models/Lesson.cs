﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DUT.Domain.Models
{
    public class Lesson : BaseModel<long>
    {
        [Required, StringLength(250, MinimumLength = 50)]
        public string Theme { get; set; }
        public Journal Journal { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public LessonType LessonType { get; set; }
        [StringLength(1000, MinimumLength = 1)]
        public string Homework { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public long? PreviewLessonId { get; set; }
        public Lesson PreviewLesson { get; set; }
        [InverseProperty("PreviewLesson")]
        public virtual ICollection<Lesson> PreviewLessonChildren { get; set; }

        public long? NextLessonId { get; set; }
        public Lesson NextLesson { get; set; }
        [InverseProperty("NextLesson")]
        public virtual ICollection<Lesson> NextLessonChildren { get; set; }
    }

    public class Journal
    {
        public JournalStatistics Statistics { get; set; }
        public List<Student> Students { get; set; }
    }

    public class JournalStatistics
    {
        public int CountOfStudents { get; set; }
        public int CountOfExist { get; set; }
        public int CountWithoutMarks { get; set; }
        public int CountWithMarks { get; set; }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Mark { get; set; }
    }
}
