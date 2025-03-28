namespace DoctorAppointmentBlazor.Models;

public class Appointment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DocName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNum { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public bool IsReminded { get; set; }
}