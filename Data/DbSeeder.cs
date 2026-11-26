using MedicalAppointment.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext db)
    {
        var adminRole = db.Roles.FirstOrDefault(r => r.Key == "ADMIN");
        var patientRole = db.Roles.FirstOrDefault(r => r.Key == "PATIENT");
        if (adminRole == null || patientRole == null)
        {
            return;
        }

        if (!db.Users.Any(u => u.RoleId == adminRole.Id && u.IsActive))
        {
            db.Users.Add(new User
            {
                FullName = "Quản trị viên",
                Phone = "0987654321",
                Email = "admin@ytehiepphuoc.vn",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                RoleId = adminRole.Id,
                IsActive = true
            });
            db.SaveChanges();
        }

        if (!db.Users.Any(u => u.RoleId == patientRole.Id))
        {
            var samplePatient = new User
            {
                FullName = "Nguyễn Minh An",
                Phone = "0912345678",
                Email = "benhnhan@ytehiepphuoc.vn",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                RoleId = patientRole.Id,
                IsActive = true
            };
            db.Users.Add(samplePatient);
            db.SaveChanges();

            db.PatientProfiles.Add(new PatientProfile
            {
                UserId = samplePatient.Id,
                FullName = "Nguyễn Minh An",
                DateOfBirth = new DateTime(1998, 5, 15),
                Gender = "Nam",
                Ethnicity = "Kinh",
                Occupation = "Nhân viên văn phòng",
                Relationship = "Bản thân",
                IdentityCardNumber = "079098012345",
                HealthInsuranceNumber = "DN4791234567890",
                Address = "Xã Hiệp Phước, Huyện Nhà Bè, TP.HCM"
            });
            db.SaveChanges();
        }

        if (db.Departments.Any())
        {
            return;
        }

        var departments = new List<Department>
        {
            new()
            {
                Name = "Nội tổng quát",
                Description = "Khám, tư vấn và điều trị các bệnh lý nội khoa tổng quát, tầm soát sức khỏe định kỳ.",
                ConsultationFee = 150000m,
                ImageUrl = "/images/departments/dept-noi-tong-quat.jpg",
                IsActive = true
            },
            new()
            {
                Name = "Tim mạch",
                Description = "Chẩn đoán và điều trị tăng huyết áp, rối loạn nhịp tim, bệnh mạch vành và suy tim.",
                ConsultationFee = 220000m,
                ImageUrl = "/images/departments/dept-tim-mach.jpg",
                IsActive = true
            },
            new()
            {
                Name = "Nhi khoa",
                Description = "Chăm sóc sức khỏe toàn diện cho trẻ sơ sinh, trẻ nhỏ và tư vấn dinh dưỡng, tiêm chủng.",
                ConsultationFee = 160000m,
                ImageUrl = "/images/departments/dept-nhi-khoa.jpg",
                IsActive = true
            },
            new()
            {
                Name = "Tai Mũi Họng",
                Description = "Nội soi và điều trị viêm mũi xoang, viêm họng, viêm tai giữa và rối loạn tiền đình.",
                ConsultationFee = 180000m,
                ImageUrl = "/images/departments/dept-tai-mui-hong.jpg",
                IsActive = true
            },
            new()
            {
                Name = "Da liễu",
                Description = "Khám và điều trị các bệnh lý da, dị ứng, viêm da cơ địa, mụn và chăm sóc da chuyên sâu.",
                ConsultationFee = 180000m,
                ImageUrl = "/images/departments/dept-da-lieu.jpg",
                IsActive = true
            },
            new()
            {
                Name = "Sản Phụ khoa",
                Description = "Khám thai định kỳ, siêu âm sản khoa và chăm sóc sức khỏe sinh sản nữ giới.",
                ConsultationFee = 200000m,
                ImageUrl = "/images/departments/dept-san-phu-khoa.jpg",
                IsActive = true
            }
        };

        db.Departments.AddRange(departments);
        db.SaveChanges();

        var rooms = new List<Room>
        {
            new() { RoomNumber = "P.101", Floor = 1, DepartmentId = departments[0].Id, IsActive = true },
            new() { RoomNumber = "P.102", Floor = 1, DepartmentId = departments[0].Id, IsActive = true },
            new() { RoomNumber = "P.103", Floor = 1, DepartmentId = departments[1].Id, IsActive = true },
            new() { RoomNumber = "P.104", Floor = 1, DepartmentId = departments[1].Id, IsActive = true },
            new() { RoomNumber = "P.201", Floor = 2, DepartmentId = departments[2].Id, IsActive = true },
            new() { RoomNumber = "P.202", Floor = 2, DepartmentId = departments[2].Id, IsActive = true },
            new() { RoomNumber = "P.203", Floor = 2, DepartmentId = departments[3].Id, IsActive = true },
            new() { RoomNumber = "P.204", Floor = 2, DepartmentId = departments[3].Id, IsActive = true },
            new() { RoomNumber = "P.301", Floor = 3, DepartmentId = departments[4].Id, IsActive = true },
            new() { RoomNumber = "P.302", Floor = 3, DepartmentId = departments[4].Id, IsActive = true },
            new() { RoomNumber = "P.303", Floor = 3, DepartmentId = departments[5].Id, IsActive = true },
            new() { RoomNumber = "P.304", Floor = 3, DepartmentId = departments[5].Id, IsActive = true }
        };

        db.Rooms.AddRange(rooms);
        db.SaveChanges();

        var doctors = new List<Doctor>
        {
            // 7 bác sĩ Nội tổng quát (đủ test phân trang Trang chủ, Admin, và Bác sĩ liên quan >= 6)
            new()
            {
                FullName = "Nguyễn Minh Quân",
                Qualification = "TS.BS",
                Email = "nmquan@ytehiepphuoc.vn",
                Phone = "0901000001",
                DepartmentId = departments[0].Id,
                RoomId = rooms[0].Id,
                ExperienceYears = 18,
                AvatarUrl = "/images/doctors/doc-01.jpg",
                Description = "Trưởng khoa Nội tổng quát, chuyên sâu bệnh lý chuyển hóa, đái tháo đường và tiêu hóa.",
                IsActive = true
            },
            new()
            {
                FullName = "Trần Thị Thanh Hà",
                Qualification = "BS.CKII",
                Email = "tttha@ytehiepphuoc.vn",
                Phone = "0901000002",
                DepartmentId = departments[0].Id,
                RoomId = rooms[0].Id,
                ExperienceYears = 15,
                AvatarUrl = "/images/doctors/doc-07.jpg",
                Description = "Chuyên khám tầm soát sức khỏe tổng quát, bệnh lý hô hấp và nội tiết.",
                IsActive = true
            },
            new()
            {
                FullName = "Lê Hoàng Phúc",
                Qualification = "ThS.BS",
                Email = "lhphuc@ytehiepphuoc.vn",
                Phone = "0901000003",
                DepartmentId = departments[0].Id,
                RoomId = rooms[1].Id,
                ExperienceYears = 11,
                AvatarUrl = "/images/doctors/doc-02.jpg",
                Description = "Bác sĩ nội khoa giàu kinh nghiệm điều trị bệnh dạ dày, gan mật và tăng huyết áp.",
                IsActive = true
            },
            new()
            {
                FullName = "Phạm Ngọc Mai",
                Qualification = "BS.CKI",
                Email = "pnmai@ytehiepphuoc.vn",
                Phone = "0901000004",
                DepartmentId = departments[0].Id,
                RoomId = rooms[1].Id,
                ExperienceYears = 9,
                AvatarUrl = "/images/doctors/doc-08.jpg",
                Description = "Tận tâm tư vấn chế độ dinh dưỡng và điều trị bệnh mạn tính cho người cao tuổi.",
                IsActive = true
            },
            new()
            {
                FullName = "Võ Quốc Bảo",
                Qualification = "BS.CKII",
                Email = "vqbao@ytehiepphuoc.vn",
                Phone = "0901000005",
                DepartmentId = departments[0].Id,
                RoomId = rooms[0].Id,
                ExperienceYears = 20,
                AvatarUrl = "/images/doctors/doc-03.jpg",
                Description = "Hơn 20 năm công tác tại các bệnh viện tuyến đầu về Nội tổng quát và Cơ xương khớp.",
                IsActive = true
            },
            new()
            {
                FullName = "Đặng Thu Thảo",
                Qualification = "ThS.BS",
                Email = "dtthao@ytehiepphuoc.vn",
                Phone = "0901000006",
                DepartmentId = departments[0].Id,
                RoomId = rooms[1].Id,
                ExperienceYears = 7,
                AvatarUrl = "/images/doctors/doc-09.jpg",
                Description = "Chuyên khám sức khỏe định kỳ, tư vấn phòng bệnh và điều trị rối loạn giấc ngủ.",
                IsActive = true
            },
            new()
            {
                FullName = "Bùi Đức Anh",
                Qualification = "BS",
                Email = "bdanh@ytehiepphuoc.vn",
                Phone = "0901000007",
                DepartmentId = departments[0].Id,
                RoomId = rooms[1].Id,
                ExperienceYears = 6,
                AvatarUrl = "/images/doctors/doc-04.jpg",
                Description = "Bác sĩ trẻ nhiệt huyết, chuyên theo dõi sức khỏe ban đầu và khám bảo hiểm y tế.",
                IsActive = true
            },

            // 5 bác sĩ đại diện cho 5 chuyên khoa còn lại
            new()
            {
                FullName = "Huỳnh Gia Huy",
                Qualification = "PGS.TS.BS",
                Email = "hghuy@ytehiepphuoc.vn",
                Phone = "0901000008",
                DepartmentId = departments[1].Id,
                RoomId = rooms[2].Id,
                ExperienceYears = 22,
                AvatarUrl = "/images/doctors/doc-05.jpg",
                Description = "Chuyên gia đầu ngành Tim mạch, siêu âm tim và điều trị rối loạn nhịp tim.",
                IsActive = true
            },
            new()
            {
                FullName = "Nguyễn Thị Kim Ngân",
                Qualification = "BS.CKII",
                Email = "ntkngan@ytehiepphuoc.vn",
                Phone = "0901000009",
                DepartmentId = departments[2].Id,
                RoomId = rooms[4].Id,
                ExperienceYears = 14,
                AvatarUrl = "/images/doctors/doc-10.jpg",
                Description = "Bác sĩ Nhi khoa chuyên điều trị hô hấp nhi, dị ứng và tư vấn tăng trưởng cho trẻ.",
                IsActive = true
            },
            new()
            {
                FullName = "Trương Tấn Tài",
                Qualification = "ThS.BS",
                Email = "tttai@ytehiepphuoc.vn",
                Phone = "0901000010",
                DepartmentId = departments[3].Id,
                RoomId = rooms[6].Id,
                ExperienceYears = 10,
                AvatarUrl = "/images/doctors/doc-06.jpg",
                Description = "Chuyên nội soi Tai Mũi Họng không đau và điều trị viêm xoang mạn tính.",
                IsActive = true
            },
            new()
            {
                FullName = "Lý Mỹ Linh",
                Qualification = "BS.CKI",
                Email = "lmlinh@ytehiepphuoc.vn",
                Phone = "0901000011",
                DepartmentId = departments[4].Id,
                RoomId = rooms[8].Id,
                ExperienceYears = 8,
                AvatarUrl = "/images/doctors/doc-11.jpg",
                Description = "Chuyên điều trị mụn trứng cá, viêm da dị ứng và phục hồi da thẩm mỹ y khoa.",
                IsActive = true
            },
            new()
            {
                FullName = "Đỗ Thị Hồng Nhung",
                Qualification = "BS.CKII",
                Email = "dthnhung@ytehiepphuoc.vn",
                Phone = "0901000012",
                DepartmentId = departments[5].Id,
                RoomId = rooms[10].Id,
                ExperienceYears = 17,
                AvatarUrl = "/images/doctors/doc-12.jpg",
                Description = "Chuyên theo dõi thai kỳ nguy cơ cao, siêu âm hình thái và khám phụ khoa định kỳ.",
                IsActive = true
            }
        };

        db.Doctors.AddRange(doctors);
        db.SaveChanges();

        var schedules = new List<DoctorSchedule>();
        foreach (var doctor in doctors)
        {
            for (var day = 0; day <= 6; day++)
            {
                schedules.Add(new DoctorSchedule
                {
                    DoctorId = doctor.Id,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(7, 0, 0),
                    EndTime = new TimeSpan(11, 30, 0),
                    MaxPatients = 20
                });
                schedules.Add(new DoctorSchedule
                {
                    DoctorId = doctor.Id,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(13, 30, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    MaxPatients = 20
                });
            }
        }

        db.DoctorSchedules.AddRange(schedules);
        db.SaveChanges();
    }
}
