using Navigate.Models.Classifiers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Navigate.Models
{
    public class WorkItem
    {

        public WorkItem()
        {
            this.isCompleted = false;
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the work item id
        /// </summary>
        [Key]
        public long Id { get; set; }
 
        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        [Required]
        [MaxLength(40)]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the location
        /// </summary>
        [MaxLength(255)]
        public string Location { get; set; }
        
        /// <summary>
        /// Gets or sets the start date time of the task
        /// Needed only if the work item is scheduled for a predetermined amount of time, which would then require explicit start and end times
        /// </summary>       
        [DataType(DataType.DateTime)]
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time of a scheduled task or the due date of a more generic task with a estimated amount of time for completion
        /// </summary>
        [Required] 
        [DataType(DataType.DateTime)]
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the estimated time of completion for a task
        /// </summary>
        public decimal? EstimatedTime { get; set; }

        /// <summary>
        /// Gets or sets the work item type id, which stores the assigned id value of a work item type from the work item type classifier
        /// </summary>
        public int WorkItemTypeId { get; set; }

        /// <summary>
        /// Gets or sets the work item type, which reference the work item type classifier
        /// </summary>
        [ForeignKey("WorkItemTypeId")]
        public virtual WorkItemType WorkItemType { get; set; }

        /// <summary>
        /// Gets or set the work item priority
        /// </summary>
        [DefaultValue(WorkItemPriority.NormalPriority)]
        public WorkItemPriority? Priority { get; set; }

        /// <summary>
        /// Gets or sets the additional info for a task
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the outlook entry id for identifying imported outlook calendar items
        /// </summary>
        public string OutlookEntryId { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the task is completed or not
        /// </summary>
        public bool isCompleted { get; set; }

        /// <summary>
        /// Gets or sets a value indiciating whether the task is recurring or not
        /// </summary>
        public bool isRecurring { get; set; }

        public RecurrenceType? RecurrenceType { get; set; }

        public virtual ICollection<RecurringItem> RecurringItems { get; set; }

        public long? WIRecurrencePatternId { get; set; }

        [ForeignKey("WIRecurrencePatternId")]
        public virtual WIRecurrencePattern RecurrencePattern { get; set; }

        /// <summary>
        /// Gets or sets the user id that created the task
        /// </summary>
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the user id that updated the task
        /// </summary>
        public int UpdatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the user id that the work item was assigned to
        /// </summary>
        public int? AssignedToUserId { get; set; }

        /// <summary>
        /// Gets or sets the created by user that references the users model
        /// </summary>
        [ForeignKey("CreatedByUserId")]
        public virtual UserProfile CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the work item was created at
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the work item was last changed
        /// </summary>
        public DateTime UpdatedAt { get; set; }

    }
}