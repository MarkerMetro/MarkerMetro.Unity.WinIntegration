using System;
using System.Collections.Generic;
using System.IO;
#if NETFX_CORE
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.Storage;
#elif WINDOWS_PHONE
using System.IO.IsolatedStorage;
#endif
using System.Linq;
using System.Threading;

namespace MarkerMetro.Unity.WinIntegration.LocalNotifications
{
    public static class ReminderManager
    {
#if NETFX_CORE
        private const string ScheduleFileName = "schedule.win8";
#elif WINDOWS_PHONE
        private const string ScheduleFileName = "schedule.wp8";
#endif
        private const string MutexName = "reminders";

        public static int MutexTimeout = -1;

        public static bool AreRemindersEnabled()
        {
#if NETFX_CORE
            var roamingSettings = ApplicationData.Current.RoamingSettings;
            var remindersEnabled = true;
            var remindersEnabledValue = roamingSettings.Values["RemindersEnabled"];
            if (remindersEnabledValue == null)
            {
                roamingSettings.Values["RemindersEnabled"] = remindersEnabled;
            }
            else
            {
                remindersEnabled = Convert.ToBoolean(remindersEnabledValue);
            }
            return remindersEnabled;
#elif WINDOWS_PHONE
            var appSettings = IsolatedStorageSettings.ApplicationSettings;
            var remindersEnabled = true;
            if (appSettings.Contains("RemindersEnabled"))
            {
                remindersEnabled = Convert.ToBoolean(appSettings["RemindersEnabled"]);
            }
            else
            {
                appSettings.Add("RemindersEnabled", remindersEnabled);
                appSettings.Save();
            }        
            return remindersEnabled;
#else
            throw new PlatformNotSupportedException("ReminderManager.AreRemindersEnabled");
#endif
        }

        public static void SetRemindersStatus(bool enabled)
        {
            if (!enabled)
                ClearReminders();
#if NETFX_CORE
            var roamingSettings = ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["RemindersEnabled"] = enabled;
#elif WINDOWS_PHONE
            var appSettings = IsolatedStorageSettings.ApplicationSettings;
            if (appSettings.Contains("RemindersEnabled"))
            {
                appSettings["RemindersEnabled"] = enabled;
            }
            else
            {
                appSettings.Add("RemindersEnabled", enabled);
            }
            appSettings.Save();
#else
            throw new PlatformNotSupportedException("ReminderManager.SetRemindersStatus");
#endif
        }

        /// <summary>
        /// Creates a reminder at specific time (system reminder on win phone, scheduled notification toast on win 8)
        /// </summary>
        public static void RegisterReminder(string id, string title, string content, DateTime triggerTime)
        {
            if (triggerTime <= DateTime.Now) return; // Trigger time has passed

            // ensure we respect the user's settings for reminders
            if (!AreRemindersEnabled()) return;

#if NETFX_CORE
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var reminders = notifier.GetScheduledToastNotifications();
            var existingReminder = reminders.FirstOrDefault(n => n.Id == id);
            if (existingReminder != null)
            {
                notifier.RemoveFromSchedule(existingReminder);
            }

            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(title));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(content));

            XmlNodeList toastImageElements = toastXml.GetElementsByTagName("image");

            ((XmlElement)toastImageElements[0]).SetAttribute("src", "ms-appx:///Assets/Logo.png");

            ScheduledToastNotification scheduledToast = new ScheduledToastNotification(toastXml, triggerTime);
            scheduledToast.Id = id;
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledToast);

#elif WINDOWS_PHONE        

            if (Microsoft.Phone.Scheduler.ScheduledActionService.GetActions<Microsoft.Phone.Scheduler.Reminder>().Where(r => r.Name == id).Count() > 0)
                Microsoft.Phone.Scheduler.ScheduledActionService.Remove(id);

            var reminder = new Microsoft.Phone.Scheduler.Reminder(id);
            reminder.BeginTime = reminder.ExpirationTime = triggerTime;
            reminder.Title = title;
            reminder.Content = content;
            Microsoft.Phone.Scheduler.ScheduledActionService.Add(reminder);
#endif
        }

        /// <summary>
        /// Clears all the reminders set to occur at a specific time 
        /// </summary>
        private static void ClearReminders()
        {
#if NETFX_CORE
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var reminders = notifier.GetScheduledToastNotifications();
            foreach(var reminder in reminders)
            {
                notifier.RemoveFromSchedule(reminder);
            }
#elif WINDOWS_PHONE
            var reminders = Microsoft.Phone.Scheduler.ScheduledActionService.GetActions<Microsoft.Phone.Scheduler.Reminder>();
            foreach (var r in reminders)
                Microsoft.Phone.Scheduler.ScheduledActionService.Remove(r.Name);
#endif
        }

        /// <summary>
        /// Remove a scheduled reminder
        /// </summary>
        public static void RemoveReminder(string id)
        {
            // if reminders not enabled, its not scheduled
            if (!AreRemindersEnabled()) return;

#if NETFX_CORE
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var reminders = notifier.GetScheduledToastNotifications();
            var existingReminder = reminders.FirstOrDefault(n => n.Id == id);
            if (existingReminder != null)
            {
                notifier.RemoveFromSchedule(existingReminder);
            }
#elif WINDOWS_PHONE
            if (Microsoft.Phone.Scheduler.ScheduledActionService.GetActions<Microsoft.Phone.Scheduler.Reminder>().Where(r => r.Name == id).Count() > 0)
                Microsoft.Phone.Scheduler.ScheduledActionService.Remove(id);
#endif
        }
    }
}
