using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(airbnbmonitor.Startup))]
namespace airbnbmonitor
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
