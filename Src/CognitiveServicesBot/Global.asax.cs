using Autofac;
using CognitiveServicesBot.Modules;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace CognitiveServicesBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
			this.RegisterBotModules();
			GlobalConfiguration.Configure(WebApiConfig.Register);
        }

		private void RegisterBotModules()
		{
			var builder = new ContainerBuilder();

			builder.RegisterModule(new ReflectionSurrogateModule());

			builder.RegisterModule<GlobalMessageHandlersBotModule>();

			builder.Update(Conversation.Container);
		}
	}
}
