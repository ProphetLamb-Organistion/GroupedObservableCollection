using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GroupedObservableCollection.Demo.DataModels
{
    public enum PersonType
    {
        [Display(Name ="Not applicable")]
        Unknown,
        [Display(Name = "Child")]
        Child,
        [Display(Name = "Pupil")]
        Pupil,
        [Display(Name = "Student")]
        Student,
        [Display(Name = "Adult")]
        Adult,
    }
}
