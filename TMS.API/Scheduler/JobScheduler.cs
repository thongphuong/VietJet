using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace TMS.API.Scheduler
{
    public class JobScheduler
    {
        public static void Start()
        {

            var scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            #region [-----------------Schedules-------------------]
            var jobSchedulesTypeRepeat = JobBuilder.Create<Schedules>().Build();
            var triggerSchedulesTypeRepeat = TriggerBuilder.Create()
                 .WithIdentity("Schedules", "IDG")
                 .WithDailyTimeIntervalSchedule(x => x.WithIntervalInHours(24).
                                                   StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(5, 0))).WithPriority(4).Build();
            scheduler.ScheduleJob(jobSchedulesTypeRepeat, triggerSchedulesTypeRepeat);
            #endregion
            #region [-----------------Send mail-------------------]
            var job = JobBuilder.Create<IDGJob>().Build();
            var trigger = TriggerBuilder.Create()
                 .WithIdentity("IDGJob", "IDG")
                                .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(90)
                        .RepeatForever()
                        )
                .StartAt(DateTime.UtcNow)
                .WithPriority(3)
                .Build();
            scheduler.ScheduleJob(job, trigger);
            #endregion
            #region [-----------------tạm không dùng Send mail reminder-------------------]
            var job2 = JobBuilder.Create<IDGJob2>().Build();
            var trigger2 = TriggerBuilder.Create()
                .WithIdentity("IDGJob2", "IDG")
                .WithDailyTimeIntervalSchedule(x =>
                                                    x.WithIntervalInHours(24)
                                                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0))
                                               )
                .WithPriority(6)
                .Build();
            //scheduler.ScheduleJob(job2, trigger2);
            #endregion

            #region [-----------------tạm không dùng Send mail reminder final course-------------------]
            var jobFinalCourse = JobBuilder.Create<Schedules_FinalCourse>().Build();
            var triggerFinalCourse = TriggerBuilder.Create()
                .WithIdentity("Schedules_FinalCourse", "IDG")
                .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(6)
                        .RepeatForever()
                        )
                .StartAt(DateTime.UtcNow)
                //.WithDailyTimeIntervalSchedule(x =>
                //                                    x.WithIntervalInHours(24)
                //                                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(8, 0))
                //                               )
                .WithPriority(5)
                .Build();
            //scheduler.ScheduleJob(jobFinalCourse, triggerFinalCourse);
            #endregion

            #region [-----------------Cronservices-------------------]
            var cronservice = JobBuilder.Create<Cronservices>().Build();
            var triggercronservice = TriggerBuilder.Create()
                 .WithIdentity("Cronservices", "IDG")
                 .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(6)
                        .RepeatForever()
                        )
                .StartAt(DateTime.UtcNow)
                .WithPriority(1)
                .Build();
            scheduler.ScheduleJob(cronservice, triggercronservice);
            #endregion

            #region [-----------------Cronservices-------------------]
            var cronservice_Assign = JobBuilder.Create<Cronservices_Assign>().Build();
            var triggercronservice_Assign = TriggerBuilder.Create()
                 .WithIdentity("Cronservices_Assign", "IDG")
                 .WithSimpleSchedule(x => x
                       .WithIntervalInMinutes(6)
                        .RepeatForever()
                        )
                .StartAt(DateTime.UtcNow)
                .WithPriority(2)
                .Build();
            scheduler.ScheduleJob(cronservice_Assign, triggercronservice_Assign);
            #endregion


        }
    }
}