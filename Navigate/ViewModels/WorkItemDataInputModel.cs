using Navigate.Models;
using Navigate.Models.Classifiers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UnconstrainedMelody;

namespace Navigate.ViewModels
{
    public class WorkItemDataInputModel
    {

        public WorkItemDataInputModel()
        {
            this.AllWorkItemTypes = new List<SelectListItem>();
            this.AllUsers = new List<SelectListItem>();
            this.Categories = new List<Category>();
            this.SelectedCategoryIds = new List<int>();
            this.Priority = WorkItemPriority.NormalPriority;
            this.StartDate = DateTime.Now;
            this.EndDate = DateTime.Now;
            this.DueDate = DateTime.Now;
        }

        #region [Work Item]

        public WorkItemDataInputModel(WorkItem workItem)
        {
            this.Subject = workItem.Subject;
            this.Location = workItem.Location;
            this.Body = workItem.Body;
            this.EndDate = workItem.EndDateTime;
            this.EstimatedTime = workItem.EstimatedTime;
            this.WorkItemType = workItem.WorkItemTypeId;
            this.Priority = workItem.Priority;
            this.isRecurring = workItem.isRecurring;
        }

        [Required(ErrorMessage="Subject is required")]
        [MaxLength(40, ErrorMessage="Subject cannot exceed 40 characters")]
        public string Subject { get; set; }

        public string Location { get; set; }

        public string Body { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        public decimal? EstimatedTime { get; set; }

        public int WorkItemType { get; set; }
        public IEnumerable<SelectListItem> AllWorkItemTypes { get; set; }

        public WorkItemPriority? Priority { get; set; }

        public IEnumerable<SelectListItem> AllPriorities {
            get
            {
                return Enums.GetValues<WorkItemPriority>().Select(enumValue => new SelectListItem { Value = enumValue.ToString(), Text = enumValue.GetDescription() });
            }
        }

        public List<Category> Categories { get; set; }

        public List<int> SelectedCategoryIds { get; set; }

        public bool isRecurring { get; set; }

        public RecurrenceType RecurrenceType { get; set; }

        public int AssignedToUserId { get; set; }

        public IEnumerable<SelectListItem> AllUsers { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public WorkItem TransformToWorkItem()
        {
            var workItem = new WorkItem();
            workItem.Subject = this.Subject;
            workItem.Location = this.Location;
            workItem.Body = this.Body;
            workItem.StartDateTime = this.StartDate;
            workItem.EndDateTime = this.EndDate;
            workItem.EstimatedTime = this.EstimatedTime;
            workItem.WorkItemTypeId = this.WorkItemType;
            workItem.Priority = this.Priority;
            workItem.isRecurring = this.isRecurring;
            workItem.AssignedToUserId = this.AssignedToUserId;
            return workItem;
        }

        #endregion

        #region [Recurrence]

        //[Range(1, 7)]
        public int DailyInterval { get; set; }

        //[Range(1, 4)]
        public int WeeklyInterval { get; set; }

        //[Range(1, 12)]
        public int MonthlyInterval { get; set; }

        //[Range(1, 12)]
        public int MonthNthInterval { get; set; }

        //[Range(1, 2)]
        public int YearlyInterval { get; set; }

        //[Range(1, 2)]
        public int YearNthInterval { get; set; }

        public bool EveryWeekday { get; set; }

        public DaysOfWeek WeekDays { get; set; }

        //[Range(0, 31)]
        public int DayOfMonth { get; set; }

        //[Range(0, 31)]
        public int DayOfMonthForYear { get; set; }

        public Instance MonthInstance { get; set; }

        public Instance YearInstance { get; set; }

        public IEnumerable<SelectListItem> AllInstances
        {
            get
            {
                return Enums.GetValues<Instance>().Select(enumValue => new SelectListItem { Value = enumValue.ToString(), Text = enumValue.GetDescription() });
            }

            set { }
        }

        public DayOfWeekMask MonthDayOfWeekMask { get; set; }

        public DayOfWeekMask YearDayOfWeekMask { get; set; }

        public IEnumerable<SelectListItem> AllDaysOfWeek
        {
            get
            {
                return Enums.GetValues<DayOfWeekMask>().Select(enumValue => new SelectListItem { Value = enumValue.ToString(), Text = enumValue.GetDescription() });
            }

            set { }
        }

        public MonthOfYear YearMonthOfYear { get; set; }

        public MonthOfYear YearNthMonthOfYear { get; set; }

        public IEnumerable<SelectListItem> AllMonthsOfYear
        {
            get
            {
                return Enums.GetValues<MonthOfYear>().Select(enumValue => new SelectListItem { Value = enumValue.ToString(), Text = enumValue.GetDescription() });
            }

            set { }
        }

        public WIRecurrencePattern TransformToRecurrencePattern()
        {
            var recurrencePattern = new WIRecurrencePattern();
            if (this.RecurrenceType == RecurrenceType.Daily)
            {
                if (this.EveryWeekday == true)
                {
                    recurrencePattern.DayOfWeekMask = DayOfWeekMask.Weekdays;
                    this.RecurrenceType = RecurrenceType.Weekly;
                }
                else recurrencePattern.Interval = this.DailyInterval;
            }
            else if (this.RecurrenceType == RecurrenceType.Weekly)
            {
                recurrencePattern.Interval = this.WeeklyInterval;
                var mask = (int)this.WeekDays;
                recurrencePattern.DayOfWeekMask = (DayOfWeekMask)Enum.ToObject(typeof(DayOfWeekMask), mask);
            }
            else if (this.RecurrenceType == RecurrenceType.Monthly)
            {
                recurrencePattern.Interval = this.MonthlyInterval;
                recurrencePattern.DayOfMonth = this.DayOfMonth;
            }
            else if (this.RecurrenceType == RecurrenceType.MonthNth)
            {
                recurrencePattern.Interval = this.MonthNthInterval;
                recurrencePattern.Instance = this.MonthInstance;
                recurrencePattern.DayOfWeekMask = this.MonthDayOfWeekMask;
            }
            else if (this.RecurrenceType == RecurrenceType.Yearly)
            {
                recurrencePattern.Interval = this.YearlyInterval;
                recurrencePattern.MonthOfYear = this.YearMonthOfYear;
                recurrencePattern.DayOfMonth = this.DayOfMonthForYear;
            }
            else
            {
                recurrencePattern.Interval = this.YearNthInterval;
                recurrencePattern.MonthOfYear = this.YearNthMonthOfYear;
                recurrencePattern.Instance = this.YearInstance;
                recurrencePattern.DayOfWeekMask = this.YearDayOfWeekMask;
            }

            return recurrencePattern;
        }

        [DataType(DataType.Time)]
        public DateTime? RecurringItemStart { get; set; }

        [DataType(DataType.Time)]
        public DateTime? RecurringItemEnd { get; set; }

        #endregion
    }
}