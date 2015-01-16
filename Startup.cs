using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Reversi.Startup))]
namespace Reversi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}