using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DbReportGenerator.Startup))]
namespace DbReportGenerator
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
