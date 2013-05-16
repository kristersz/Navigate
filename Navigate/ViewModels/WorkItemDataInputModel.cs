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
            this.Categories = new List<Category>();
            this.SelectedCategoryIds = new List<int>();
            this.StartDate = DateTime.Now;
            this.EndDate = DateTime.Now;
            this.DueDate = DateTime.Now;
            this.DailyInterval = 1;
            this.WeeklyInterval = 1;
            this.MonthlyInterval = 1;
            this.MonthNthInterval = 1;
            this.YearlyInterval = 1;
            this.YearNthInterval = 1;
            this.DayOfMonth = 1;
            this.DayOfMonthForYear = 1;
        }

        #region [Work Item]

        public WorkItemDataInputModel(WorkItem workItem)
        {
            this.WorkItemId = workItem.Id;
            this.Subject = workItem.Subject;
            this.Location = workItem.Location;
            this.Body = workItem.Body;
            this.EndDate = workItem.EndDateTime;
            this.EstimatedTime = workItem.EstimatedTime;
            this.WorkItemType = workItem.WorkItemType;
            this.Priority = workItem.Priority;
            this.isRecurring = workItem.isRecurring;
        }

        public long WorkItemId { get; set; }

        [Display(Name="Nosaukums")]
        [Required(ErrorMessage="Nosaukums ir obligāts lauks")]
        [MaxLength(180, ErrorMessage="Nosaukums nevar pārsniegt 180 simbolu garumu")]
        public string Subject { get; set; }

        [Display(Name = "Atrašanās vieta")]
        [MaxLength(180, ErrorMessage = "Atrašānās vietas nosaukums nevar pārsniegt 255 simbolu garumu")]
        public string Location { get; set; }

        [Display(Name = "Informācija")]
        public string Body { get; set; }

        [Display(Name = "Sākuma laiks")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Beigu laiks")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Termiņš")]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Izpildes ilgums")]
        public decimal? EstimatedTime { get; set; }

        [Display(Name = "Uzdevuma tips")]
        [Required]
        public WorkItemType WorkItemType { get; set; }

        public IEnumerable<SelectListItem> AllWorkItemTypes
        {
            get
            {
                return Enums.GetValues<WorkItemType>().Select(enumValue => new SelectListItem { Value = enumValue.ToString(), Text = enumValue.GetDescription() });
            }
        }

        [Display(Name = "Prioritāte")]
        public WorkItemPriority? Priority { get; set; }

        public IEnumerable<SelectListItem> AllPriorities {
            get
            {
                return Enums.GetValues<WorkItemPriority>().Select(enumValue => new SelectListItem { Value = enumValue.ToString(), Text = enumValue.GetDescription() });
            }
        }

        public List<Category> Categories { get; set; }

        public List<int> SelectedCategoryIds { get; set; }

        [Display(Name = "Periodisks uzdevums")]
        public bool isRecurring { get; set; }

        [Display(Name = "Periodiskums")]
        public RecurrenceType RecurrenceType { get; set; }

        public WorkItem TransformToWorkItem()
        {
            var workItem = new WorkItem();
            workItem.Subject = this.Subject;
            workItem.Location = this.Location;
            workItem.Body = this.Body;
            workItem.StartDateTime = this.StartDate;
            workItem.EndDateTime = this.EndDate;
            workItem.EstimatedTime = this.EstimatedTime;
            workItem.WorkItemType = this.WorkItemType;
            workItem.Priority = this.Priority;
            workItem.isRecurring = this.isRecurring;
            return workItem;
        }

        #endregion

        #region [Recurrence]

        [Display(Name = "Katru dienu")]
        [Range(1, 7)]
        public int DailyInterval { get; set; }

        [Display(Name = "Katru nedēļu")]
        [Range(1, 4)]
        public int WeeklyInterval { get; set; }

        [Display(Name = "Katra mēneša norādītajā datumā")]
        [Range(1, 12)]
        public int MonthlyInterval { get; set; }

        [Display(Name = "Katra mēneša norādītajā dienā")]
        [Range(1, 12)]
        public int MonthNthInterval { get; set; }

        [Display(Name = "Katra gada norādītajā datumā")]
        [Range(1, 2)]
        public int YearlyInterval { get; set; }

        [Display(Name = "Katra gada norādītajā dienā")]
        [Range(1, 2)]
        public int YearNthInterval { get; set; }

        [Display(Name = "Katru darbadienu")]
        public bool EveryWeekday { get; set; }

        public DaysOfWeek WeekDays { get; set; }

        [Display(Name = "Mēneša diena")]
        [Range(0, 31)]
        public int DayOfMonth { get; set; }

        [Display(Name = "Mēneša diena")]
        [Range(0, 31)]
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

        [Display(Name = "Mēnesis")]
        public MonthOfYear YearMonthOfYear { get; set; }

        [Display(Name = "Mēnesis")]
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

        internal void UpdateWorkItem(WorkItem workItem)
        {
            workItem.Subject = this.Subject;
            workItem.Location = this.Location;
            workItem.Body = this.Body;
            workItem.StartDateTime = this.StartDate;
            workItem.EndDateTime = this.EndDate;
            workItem.EstimatedTime = this.EstimatedTime;
            workItem.WorkItemType = this.WorkItemType;
            workItem.Priority = this.Priority;
            workItem.isRecurring = this.isRecurring;
        }
    }
}