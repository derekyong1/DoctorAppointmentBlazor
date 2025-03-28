using DoctorAppointmentBlazor.Models;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace DoctorAppointmentBlazor.Services;

public class AppointmentService
{
    private readonly List<Appointment> _appointments = new();
    private readonly IConfiguration _configuration;
    private int _nextId = 1;

    public AppointmentService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<List<Appointment>> GetAppointmentsAsync()
    {
        return Task.FromResult(_appointments);
    }

    public async Task AddAppointmentAsync(Appointment appointment)
    {
        appointment.Id = _nextId++;
        _appointments.Add(appointment);
        await SendConfirmationEmailAsync(appointment);
    }

    public List<Appointment> GetUpcomingAppointments(DateTime withinNext)
    {
        var now = DateTime.Now;
        return _appointments
            .Where(a => a.DateTime > now && a.DateTime <= withinNext && !a.IsReminded)
            .ToList();
    }

    public void MarkAsReminded(Appointment appointment)
    {
        appointment.IsReminded = true;
    }

    private async Task SendConfirmationEmailAsync(Appointment appointment)
    {
        // Email Contents
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Doctor Office", "no-reply@doctoroffice.com"));
        message.To.Add(new MailboxAddress(appointment.Name, appointment.Email));
        message.Subject = "Appointment Confirmation";
        message.Body = new TextPart("plain")
        {
            Text = $"Hello {appointment.Name},\n\n
                    Your appointment is confirmed for {appointment.DateTime} with {appointment.DocName}.\n\n
                    Thank you!\n
                    Your Doctor's Office"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _configuration["EmailSettings:SmtpServer"],
            int.Parse(_configuration["EmailSettings:SmtpPort"]),
            MailKit.Security.SecureSocketOptions.StartTls
        );
        await client.AuthenticateAsync(
            _configuration["EmailSettings:Username"],
            _configuration["EmailSettings:Password"]
        );
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        Console.WriteLine("Confirmation Email Sent. Check Ethereal for the preview URL.");
    }

    public async Task SendReminderEmailAsync(Appointment appointment)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Doctor Office", "no-reply@doctoroffice.com"));
        message.To.Add(new MailboxAddress(appointment.Name, appointment.Email));
        message.Subject = "Appointment Reminder";
        message.Body = new TextPart("plain")
        {
            Text = $"Hello {appointment.Name},\n\nThis is a reminder for your appointment on {appointment.DateTime}.\n\nSee you soon!"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _configuration["EmailSettings:SmtpServer"],
            int.Parse(_configuration["EmailSettings:SmtpPort"]),
            MailKit.Security.SecureSocketOptions.StartTls
        );
        await client.AuthenticateAsync(
            _configuration["EmailSettings:Username"],
            _configuration["EmailSettings:Password"]
        );
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        Console.WriteLine("Reminder Email Sent. Check Ethereal for the preview URL.");
    }
}