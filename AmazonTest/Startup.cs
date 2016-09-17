using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AmazonTest.Startup))]
namespace AmazonTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
