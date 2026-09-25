using System;

namespace MedicalAppointment.Shared.Entities
{
    public abstract class BaseEntity
    {
        public DateTime? UpdatedAt { get; set; }
    }
}
