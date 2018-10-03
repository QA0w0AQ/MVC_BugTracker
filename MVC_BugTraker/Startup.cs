using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC_BugTraker.Startup))]
namespace MVC_BugTraker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
