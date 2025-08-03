using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clarity.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task SendTaskReminderAsync(string userEmail, string taskTitle, DateTime dueDate);
    }
}
