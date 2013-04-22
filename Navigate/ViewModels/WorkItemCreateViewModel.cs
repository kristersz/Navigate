using Navigate.Models;
using Navigate.Models.Classifiers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Navigate.ViewModels
{
    public class WorkItemCreateViewModel
    {

        public WorkItemCreateViewModel()
        {
            this.AllWorkItemTypes = new List<SelectListItem>();
            this.AllUsers = new List<SelectListItem>();
            this.StartDate = DateTime.Now;
            this.EndDate = DateTime.Now;
            this.isCompleted = false;
        }

        //public WorkItemCreateViewModel(WorkItem workItem)
        //{
        //    this.Subject = workItem.Subject;
        //    this.Location = workItem.Location;
        //    this.StartDate = workItem.StartDate;
        //    this.EndDate = workItem.EndDate;
        //    this.EstimatedTime = workItem.EstimatedTime;
        //    this.WorkItemType = workItem.WorkItemTypeId;
        //    this.Priority = workItem.Priority;
        //    this.AssignedToUserId = workItem.AssignedToUserId;
        //}

        public long Id { get; set; }

        [Required(ErrorMessage="Subject is required")]
        [MaxLength(40, ErrorMessage="Subject cannot exceed 40 characters")]
        public string Subject { get; set; }

        public string Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal? EstimatedTime { get; set; }

        public int WorkItemType { get; set; }
        public IEnumerable<SelectListItem> AllWorkItemTypes { get; set; }

        [DefaultValue(WorkItemPriority.NormalPriority)]
        public WorkItemPriority? Priority { get; set; }

        public bool isCompleted { get; set; }

        public bool isRecurring { get; set; }

        public int CreatedByUserId { get; set; }

        public int UpdatedByUserId { get; set; }

        public int AssignedToUserId { get; set; }

        public IEnumerable<SelectListItem> AllUsers { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public WorkItem TransformToWorkItem()
        {
            var workItem = new WorkItem();
            workItem.Subject = this.Subject;
            workItem.StartDate = this.StartDate;
            workItem.EndDate = this.EndDate;
            workItem.EstimatedTime = this.EstimatedTime;
            workItem.WorkItemTypeId = this.WorkItemType;
            workItem.Priority = this.Priority;
            workItem.isCompleted = this.isCompleted;
            workItem.isRecurring = this.isRecurring;
            workItem.CreatedAt = DateTime.Now;
            workItem.UpdatedAt = DateTime.Now;
            workItem.AssignedToUserId = this.AssignedToUserId;

            return workItem;
        }
    }
}