using Hangfire;
using Hangfire.Dashboard;
using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(ERPIndia.Startup1))]

namespace ERPIndia
{
    public class Startup1
    {
        public void Configuration(IAppBuilder app)
        {
            // app.UseHangfireDashboard();with out authtication

            //app.UseHangfireDashboard("/hangfire", new DashboardOptions
            //{
            //    Authorization = new[] { new MyAuthorizationFilter() }
            //});
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
