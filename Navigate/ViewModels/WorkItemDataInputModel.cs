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
            SetInitialValues();
        }

        public void SetInitialValues()
        {
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
            this.Categories = new List<Category>();
            this.SelectedCategoryIds = new List<int>();
            SetInitialValues();

            this.WorkItemId = workItem.Id;
            this.Subject = workItem.Subject;
            this.Location = workItem.Location;
            this.Body = workItem.Body;
            this.Priority = workItem.Priority;
            this.Reminder = workItem.Reminder;
            this.Origin = workItem.Origin;
            this.WorkItemType = workItem.WorkItemType;
            if (workItem.WorkItemType == WorkItemType.Task)
            {
                this.DueDate = workItem.EndDateTime;
                this.StartDate = DateTime.Now;
                this.EndDate = DateTime.Now;
            }
            else if (workItem.WorkItemType == WorkItemType.Appointment)
            {
                this.AllDayEvent = workItem.AllDayEvent;
                this.StartDate = workItem.StartDateTime;
                this.EndDate = workItem.EndDateTime;
                this.DueDate = DateTime.Now;
            }
            this.Duration = workItem.Duration.Value;         
            this.isRecurring = workItem.isRecurring;
            if (workItem.isRecurring)
            {
                var recurrencePattern = workItem.RecurrencePattern;
                this.RecurrenceType = (RecurrenceType)workItem.RecurrenceType;
                this.RecurringItemStart = workItem.RecurringItems.FirstOrDefault(o => o.Exception == false).Start;
                this.RecurringItemEnd = workItem.RecurringItems.FirstOrDefault(o => o.Exception == false).End;
                switch (workItem.RecurrenceType)
                {
                    case RecurrenceType.Daily:
                        this.DailyInterval = recurrencePattern.Interval;
                        break;
                    case RecurrenceType.Weekly:
                        this.WeeklyInterval = recurrencePattern.Interval;
                        var mask = (int)recurrencePattern.DayOfWeekMask;
                        this.WeekDays = (DaysOfWeek)Enum.ToObject(typeof(DaysOfWeek), mask);
                        break;
                    case RecurrenceType.Monthly:
                        this.MonthlyInterval =  recurrencePattern.Interval;
                        this.DayOfMonth = recurrencePattern.DayOfMonth;
                        break;
                    case RecurrenceType.MonthNth:
                        this.MonthNthInterval = recurrencePattern.Interval;
                        this.MonthInstance = recurrencePattern.Instance;
                        this.MonthDayOfWeekMask = recurrencePattern.DayOfWeekMask;
                        break;
                    case RecurrenceType.Yearly:
                        this.YearlyInterval =  recurrencePattern.Interval;
                        this.DayOfMonthForYear = recurrencePattern.DayOfMonth;
                        break;
                    case RecurrenceType.YearNth:
                        this.YearNthInterval = recurrencePattern.Interval;
                        this.YearInstance = recurrencePattern.Instance;
                        this.YearDayOfWeekMask = recurrencePattern.DayOfWeekMask;
                        break;
                }
            }
        }

        public long WorkItemId { get; set; }

        [Display(Name="Nosaukums")]
        [Required(ErrorMessage="{0} ir obligāts lauks")]
        [MaxLength(180, ErrorMessage="Nosaukums nevar pārsniegt 180 simbolu garumu")]
        public string Subject { get; set; }

        [Display(Name = "Atrašanās vietas adrese")]
        [MaxLength(255, ErrorMessage = "Atrašanās vietas adrese nevar pārsniegt 255 simbolu garumu")]
        public string Location { get; set; }

        [Display(Name = "Papildus informācija")]
        public string Body { get; set; }

        [Display(Name = "Sākuma datums un laiks")]
        [Required(ErrorMessage = "{0} ir obligāts lauks")]
        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Beigu datums un laiks")]
        [Required(ErrorMessage = "{0} ir obligāts lauks")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Termiņš")]
        [Required(ErrorMessage = "{0} ir obligāts lauks")]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Notiek visu dienu")]
        public bool AllDayEvent { get; set; }

        [Display(Name = "Izpildes ilgums stundās")]
        public double Duration { get; set; }

        [Display(Name = "Uzdevuma tips")]
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

        [Display(Name = "Atgādinājums")]
        public Reminder Reminder { get; set; }

        [Display(Name = "Sākumpunkts")]
        public string Origin { get; set; }

        public IEnumerable<SelectListItem> AllReminders
        {
            get
            {
                return Enums.GetValues<Reminder>().Select(enumValue => new SelectListItem { Value = enumValue.ToString(), Text = enumValue.GetDescription() });
            }
        }

        public WorkItem TransformToWorkItem()
        {
            var workItem = new WorkItem();
            workItem.Subject = this.Subject;
            workItem.Location = this.Location;
            workItem.Body = this.Body;
            workItem.Priority = this.Priority;
            workItem.Reminder = this.Reminder;
            workItem.Origin = this.Origin;
            workItem.WorkItemType = this.WorkItemType;           
            if (this.WorkItemType == WorkItemType.Task)
            {
                workItem.Duration = this.Duration;
                workItem.StartDateTime = null;
                workItem.EndDateTime = this.DueDate;
            }
            else
            {
                workItem.StartDateTime = this.StartDate;
                workItem.EndDateTime = this.EndDate;
                workItem.AllDayEvent = this.AllDayEvent;
                workItem.isRecurring = this.isRecurring;
                workItem.Duration = (this.EndDate - this.StartDate.Value).TotalHours;
            }         

            return workItem;
        }

        #endregion

        #region [Recurrence]

        [Display(Name = "Katru dienu")]
        [Range(1, 6)]
        public int DailyInterval { get; set; }

        [Display(Name = "Katru nedēļu")]
        [Range(1, 3)]
        public int WeeklyInterval { get; set; }

        [Display(Name = "Katra mēneša norādītajā datumā")]
        [Range(1, 11)]
        public int MonthlyInterval { get; set; }

        [Display(Name = "Katra mēneša norādītajā dienā")]
        [Range(1, 11)]
        public int MonthNthInterval { get; set; }

        [Display(Name = "Katra gada norādītajā datumā")]
        [Range(1, 3)]
        public int YearlyInterval { get; set; }

        [Display(Name = "Katra gada norādītajā dienā")]
        [Range(1, 3)]
        public int YearNthInterval { get; set; }

        public DaysOfWeek WeekDays { get; set; }

        [Display(Name = "Mēneša diena")]
        [Range(1, 31)]
        public int DayOfMonth { get; set; }

        [Display(Name = "Mēneša diena")]
        [Range(1, 31)]
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
                recurrencePattern.Interval = this.DailyInterval;
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

        [Display(Name = "Periodiskā uzdevuma sākuma laiks")]
        [DataType(DataType.Time)]
        public DateTime? RecurringItemStart { get; set; }

        [Display(Name = "Periodiskā uzdevuma beigu laiks")]
        [DataType(DataType.Time)]
        public DateTime? RecurringItemEnd { get; set; }

        #endregion

        internal void UpdateWorkItem(WorkItem workItem)
        {
            workItem.Subject = this.Subject;
            workItem.Location = this.Location;
            workItem.Body = this.Body;          
            workItem.Duration = this.Duration;
            workItem.Priority = this.Priority; 
            workItem.WorkItemType = this.WorkItemType;
            if (this.WorkItemType == WorkItemType.Task)
            {
                workItem.isRecurring = false;
                workItem.StartDateTime = DateTime.Now;
                workItem.EndDateTime = this.DueDate;
            }
            else
            {
                workItem.AllDayEvent = this.AllDayEvent;
                workItem.StartDateTime = this.StartDate;
                workItem.isRecurring = this.isRecurring;
                workItem.EndDateTime = this.EndDate;
            }                      
        }
    }
}