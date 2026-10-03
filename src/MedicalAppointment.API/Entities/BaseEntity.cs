using System;

namespace MedicalAppointment.API.Entities
{
    public abstract class BaseEntity
    {
        public DateTime? UpdatedAt { get; set; }
    }
}

