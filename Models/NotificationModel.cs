namespace AGPSadmin.Models
{
    public class NotificationModel
    {
        public string ProjectName { get; set; }
        public int PartId { get; set; }
        public string PartName { get; set; }
        public int Delta { get; set; }
        public int TotalDone { get; set; }
        public string MadeBy { get; set; }
        public string TypeOfWork { get; set; }
        public string Comments { get; set; }
    }
}