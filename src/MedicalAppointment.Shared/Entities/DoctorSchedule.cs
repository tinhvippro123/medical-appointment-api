namespace MedicalAppointment.Shared.Entities
{
    public class DoctorSchedule : BaseEntity
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int DayOfWeek { get; set; }        // 0=CN, 1=T2, ..., 6=T7
        public TimeSpan StartTime { get; set; }    // 07:00
        public TimeSpan EndTime { get; set; }      // 11:30
        public int MaxPatients { get; set; } = 20; // Số bệnh nhân tối đa mỗi khung giờ

        // Navigation
        public Doctor Doctor { get; set; } = null!;
    }
}
