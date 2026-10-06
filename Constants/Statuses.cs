namespace Backend.Constants
{
    // Statuses are stored as strings in the database; keep every value in one place
    // so services never compare against hand-typed literals.

    public static class RoleNames
    {
        public const string Admin="Admin";
        public const string Patient="Patient";
        public const string Doctor="Doctor";
        public const string Pathologist="Pathologist";
        public const string LabTechnician="LabTechnician";
        public const string Phlebotomist="Phlebotomist";
        public const string Receptionist="Receptionist";

        public static readonly string[] All={Admin, Patient, Doctor, Pathologist, LabTechnician, Phlebotomist, Receptionist};
    }

    public static class AppointmentTypes
    {
        public const string LabVisit="LabVisit";
        public const string HomeCollection="HomeCollection";

        public static readonly string[] All={LabVisit, HomeCollection};
    }

    public static class AppointmentStatuses
    {
        public const string Booked="Booked";
        public const string Confirmed="Confirmed";
        public const string SampleCollected="SampleCollected";
        public const string InProgress="InProgress";
        public const string Completed="Completed";
        public const string Cancelled="Cancelled";

        public static readonly string[] All={Booked, Confirmed, SampleCollected, InProgress, Completed, Cancelled};
    }

    public static class AppointmentTestStatuses
    {
        public const string Ordered="Ordered";
        public const string SampleCollected="SampleCollected";
        public const string ResultEntered="ResultEntered";
        public const string Cancelled="Cancelled";
    }

    public static class SpecimenStatuses
    {
        public const string Collected="Collected";
        public const string Received="Received";
        public const string Rejected="Rejected";
    }

    public static class ResultFlags
    {
        public const string Normal="Normal";
        public const string Low="Low";
        public const string High="High";
        public const string NotApplicable="N/A";
    }

    public static class ReportStatuses
    {
        public const string PendingValidation="PendingValidation";
        public const string Validated="Validated";
        public const string Rejected="Rejected";
        public const string Released="Released";
    }

    public static class ProviderTypes
    {
        public const string Lab="Lab";
        public const string Phlebotomist="Phlebotomist";
    }

    public static class AuditActions
    {
        public const string Create="Create";
        public const string Update="Update";
        public const string Delete="Delete";
        public const string StatusChange="StatusChange";
        public const string Login="Login";
    }
}
