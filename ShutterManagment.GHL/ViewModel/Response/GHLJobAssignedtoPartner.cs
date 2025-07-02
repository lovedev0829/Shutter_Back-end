using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagment.GHL.ViewModel.Response
{
    public class Opportunitys
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public int? monetaryValue { get; set; }
        public string? pipelineId { get; set; }
        public string? pipelineStageId { get; set; }
        public string? pipelineStageUId { get; set; }
        public string? assignedTo { get; set; }
        public string? status { get; set; }
        public object? source { get; set; }
        public DateTime? lastStatusChangeAt { get; set; }
        public DateTime? lastStageChangeAt { get; set; }
        public DateTime? createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
        public int? indexVersion { get; set; }
        public string? contactId { get; set; }
        public string? locationId { get; set; }
        public List<object>? customFields { get; set; }
        public object? lostReasonId { get; set; }
        public List<object>? followers { get; set; }
        // public List<Relation>? relations { get; set; }
        public Contact? contact { get; set; }
        public List<object>? sort { get; set; }
        public List<object>? attributions { get; set; }
     }

    //public class Pipelines
    //{
    //    public int docCountErrorUpperBound { get; set; }
    //    public int sumOtherDocCount { get; set; }
    //    public List<Bucket> buckets { get; set; }
    //    public Hit? hits { get; set; }
    //}

    //public class Relation
    //{
    //    public string associationId { get; set; }
    //    public string relationId { get; set; }
    //    public bool primary { get; set; }
    //    public string objectKey { get; set; }
    //    public string recordId { get; set; }
    //    public string fullName { get; set; }
    //    public string contactName { get; set; }
    //    public object companyName { get; set; }
    //    public string email { get; set; }
    //    public string phone { get; set; }
    //    public List<object> tags { get; set; }
    //    public bool? attributed { get; set; }
    //}

    //public class Revenues
    //{
    //    public int value { get; set; }
    //}

    public class GHLJobAssignedtoPartner
    {
        public List<Opportunitys>? opportunities { get; set; }
        //  public Meta meta { get; set; }
        // public Aggregations? aggregations { get; set; }
        public string? traceId { get; set; }
    }

    //public class Source
    //{
    //    public string locationId { get; set; }
    //    public object dateAdded { get; set; }
    //    public object dateUpdated { get; set; }
    //    public string pipelineId { get; set; }
    //    public string pipelineStageId { get; set; }
    //    public int monetaryValue { get; set; }
    //    public string status { get; set; }
    //    public string name { get; set; }
    //    public object source { get; set; }
    //    public string assignedTo { get; set; }
    //    public int indexVersion { get; set; }
    //    public object lostReasonId { get; set; }
    //    public object lastStatusChangeDate { get; set; }
    //    public object lastStageChangeDate { get; set; }
    //    public List<object> followers { get; set; }
    //    public List<object> customFields { get; set; }
    //    public List<object> currentCampaigns { get; set; }
    //    public List<object> pastCampaigns { get; set; }
    //    public string contactId { get; set; }
    //    public string fullName { get; set; }
    //    public string contactName { get; set; }
    //    public object companyName { get; set; }
    //    public string email { get; set; }
    //    public string phone { get; set; }
    //    public List<object> tags { get; set; }
    //    public string type { get; set; }
    //    public bool attributed { get; set; }
    //    public List<object> scoring { get; set; }
    //}

    //public class Total
    //{
    //    public int value { get; set; }
    //    public string relation { get; set; }
    //}
}
