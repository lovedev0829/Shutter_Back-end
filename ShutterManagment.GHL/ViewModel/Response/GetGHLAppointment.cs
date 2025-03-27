using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagment.GHL.ViewModel.Response
{
    public class Calendar
    {
        public string id { get; set; }
        public string locationId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string widgetSlug { get; set; }
        public string calendarType { get; set; }
        public string eventTitle { get; set; }
        public string eventColor { get; set; }
        public int slotDuration { get; set; }
        public string slotDurationUnit { get; set; }
        public int slotInterval { get; set; }
        public string slotIntervalUnit { get; set; }
        public string slotBufferUnit { get; set; }
        public int appoinmentPerSlot { get; set; }
        public List<OpenHour> openHours { get; set; }
        public bool enableRecurring { get; set; }
        public bool autoConfirm { get; set; }
        public bool googleInvitationEmails { get; set; }
        public bool allowReschedule { get; set; }
        public bool allowCancellation { get; set; }
        public string formSubmitType { get; set; }
        public string formSubmitThanksMessage { get; set; }
        public List<object> availabilities { get; set; }
        public string consentLabel { get; set; }
        public string preBufferUnit { get; set; }
        public List<TeamMember> teamMembers { get; set; }
        public string eventType { get; set; }
        public string groupId { get; set; }
        public string widgetType { get; set; }
        public object appoinmentPerDay { get; set; }
        public Recurring recurring { get; set; }
        public string formId { get; set; }
        public bool? stickyContact { get; set; }
        public bool? isLivePaymentMode { get; set; }
        public bool? shouldAssignContactToTeamMember { get; set; }
        public bool? shouldSkipAssigningContactForExisting { get; set; }
        public string notes { get; set; }
        public string pixelId { get; set; }
        public string formSubmitRedirectUrl { get; set; }
        public string guestType { get; set; }
        public string calendarCoverImage { get; set; }
        public LookBusyConfig lookBusyConfig { get; set; }
        public string allowBookingAfterUnit { get; set; }
        public string allowBookingForUnit { get; set; }
    }

    public class Hour
    {
        public int openHour { get; set; }
        public int openMinute { get; set; }
        public int closeHour { get; set; }
        public int closeMinute { get; set; }
    }

    public class LocationConfiguration
    {
        public string kind { get; set; }
        public string location { get; set; }
        public int position { get; set; }
        public string zoomOauthId { get; set; }
        public string meetingId { get; set; }
    }

    public class LookBusyConfig
    {
        public bool enabled { get; set; }
        public int lookBusyPercentage { get; set; }
    }

    public class OpenHour
    {
        public List<int> daysOfTheWeek { get; set; }
        public List<Hour> hours { get; set; }
    }

    public class Recurring
    {
        public List<object> monthDays { get; set; }
        public List<object> weekDays { get; set; }
        public int count { get; set; }
        public string bookingOption { get; set; }
        public string bookingOverlapDefaultStatus { get; set; }
        public object interval { get; set; }
        public string freq { get; set; }
    }

    public class GetGHLAppointment
    {
        public List<Calendar> calendars { get; set; }
        public string traceId { get; set; }
    }

    public class TeamMember
    {
        public double priority { get; set; }
        public bool selected { get; set; }
        public string userId { get; set; }
        public string isZoomAdded { get; set; }
        public string zoomOauthId { get; set; }
        public List<LocationConfiguration> locationConfigurations { get; set; }
        public bool? isPrimary { get; set; }
        public bool? useMeetingForCollective { get; set; }
        public string meetingLocation { get; set; }
    }


   
}
